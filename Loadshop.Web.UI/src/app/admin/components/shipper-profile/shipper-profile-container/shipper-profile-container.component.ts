import { Component, ChangeDetectionStrategy, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Store, select } from '@ngrx/store';
import {
  AdminState,
  ShipperProfileLoadShippersAction,
  getShippers,
  ShipperProfileLoadAction,
  getSelectedShipper,
  getLoadingSelectedShipper,
  getLoadingShippers,
  UserAdminLoadAdminUsersAction,
  getAdminUsers,
  getAdminUsersLoading,
  ShipperProfileAddAction,
  ShipperProfileUpdateAction,
  getSavingCustomer,
  ShipperProfileLoadNewAction,
  ShipperProfileEnableShipperApiAction,
} from 'src/app/admin/store';
import { map, distinctUntilChanged, filter } from 'rxjs/operators';
import { Observable, combineLatest, Subscription } from 'rxjs';
import {
  Customer,
  CustomerProfile,
  Commodity,
  CarrierCarrierScacGroup,
  UserAdminData,
  defaultCustomerProfile,
  CustomerLoadType
} from 'src/app/shared/models';
import {
  CoreState,
  getLoadingCommodities,
  getCommodities,
  CarrierCarrierScacLoadAction,
  getAllCarrierGroups,
    getLoadingAllCarrierGroups,
    getCustomerLoadTypes,
    getLoadingCustomerLoadTypes,
    CustomerLoadTypeLoadAction,
} from 'src/app/core/store';
import { defaultCustomerContact } from 'src/app/shared/models/customer-contact';
import { SelectItem, TreeNode, ConfirmationService } from 'primeng/api';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'kbxl-shipper-profile-container',
  templateUrl: './shipper-profile-container.component.html',
  styleUrls: ['./shipper-profile-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShipperProfileContainerComponent implements OnInit, OnDestroy {
  @ViewChild('form') form: NgForm;
  public shippers$: Observable<Customer[]>;
  public shipper$: Observable<CustomerProfile>;
  public processing$: Observable<boolean>;
  public selectedShipper: Customer;
  public displayShipperMappingsDialog = false;
  carriers$: Observable<SelectItem[]>;
    commodities$: Observable<Commodity[]>;
    customerLoadTypes$: Observable<CustomerLoadType[]>;
  adminUsers$: Observable<UserAdminData[]>;

  carriers: SelectItem[] = [];
  carrierShipperSub: Subscription;

  multiSelectVisible = false;
  multiSelectOptions: TreeNode[] = [];

  constructor(
    private adminStore: Store<AdminState>,
    private coreStore: Store<CoreState>,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit(): void {
    // Dispatch
    this.adminStore.dispatch(new ShipperProfileLoadShippersAction());
    this.adminStore.dispatch(new UserAdminLoadAdminUsersAction());
    this.coreStore.dispatch(new CarrierCarrierScacLoadAction());
    this.coreStore.dispatch(new CustomerLoadTypeLoadAction());

    // Observables
    this.shippers$ = this.adminStore.pipe(map(getShippers));
    this.shipper$ = this.adminStore.pipe(map(getSelectedShipper), distinctUntilChanged());
    this.carriers$ = this.coreStore.pipe(
      select(getAllCarrierGroups),
      filter(x => x && x.length > 0),
      map(x => this.toSelectItem(x))
    );
    this.commodities$ = this.coreStore.pipe(select(getCommodities));
    this.customerLoadTypes$ = this.coreStore.pipe(select(getCustomerLoadTypes));
    this.adminUsers$ = this.adminStore.pipe(select(getAdminUsers));
    this.processing$ = combineLatest(
      this.adminStore.pipe(select(getLoadingSelectedShipper)),
      this.adminStore.pipe(select(getLoadingShippers)),
      this.adminStore.pipe(select(getSavingCustomer)),
      this.adminStore.pipe(select(getAdminUsersLoading)),
      this.coreStore.pipe(select(getLoadingAllCarrierGroups)),
      this.coreStore.pipe(select(getLoadingCommodities)),
      this.coreStore.pipe(select(getLoadingCustomerLoadTypes)),
    ).pipe(map(args => args[0] || args[1] || args[2] || args[3] || args[4] || args[5] || args[6]));

    // Subscriptions
    this.carrierShipperSub = combineLatest(this.carriers$, this.shipper$)
      .pipe(map(args => args[0].sort((a, b) => this.carrierSort(a, b, args[1]))))
      .subscribe(x => {
        this.carriers = x;
      });
  }

  ngOnDestroy() {
    if (this.carrierShipperSub) {
      this.carrierShipperSub.unsubscribe();
    }
  }

  loadShipper() {
    this.adminStore.dispatch(
      new ShipperProfileLoadAction({
        customerId: this.selectedShipper.customerId,
      })
    );
  }

  newShipper() {
    this.adminStore.dispatch(new ShipperProfileLoadNewAction({ ...defaultCustomerProfile }));
  }

  addContact(s: CustomerProfile) {
    s.customerContacts.push({ ...defaultCustomerContact });
  }

  deleteContact(s: CustomerProfile, i: number) {
    s.customerContacts = s.customerContacts.filter((x, n) => n !== i);
  }

  saveChanges(s: CustomerProfile) {
    s.inNetworkFlatFee = s.inNetworkFlatFee || 0;
    s.inNetworkPercentFee = s.inNetworkPercentFee || 0;
    s.inNetworkFeeAdd = s.inNetworkFeeAdd || false;
    s.outNetworkFlatFee = s.outNetworkFlatFee || 0;
    s.outNetworkPercentFee = s.outNetworkPercentFee || 0;
    s.outNetworkFeeAdd = s.outNetworkFeeAdd || false;
    if (s.customerId) {
      this.updated(s);
    } else {
      this.added(s);
    }
  }

  added(s: CustomerProfile) {
    this.adminStore.dispatch(new ShipperProfileAddAction(s));
  }

  updated(s: CustomerProfile) {
    this.adminStore.dispatch(new ShipperProfileUpdateAction(s));
  }

  cancel(s: CustomerProfile) {
    this.confirmationService.confirm({
      message: 'Are you sure you want to cancel? New Shipper profile will not be saved.',
      accept: () => {
        this.adminStore.dispatch(new ShipperProfileLoadNewAction(null));
        this.selectedShipper = null;
      },
    });
  }

  toSelectItem(groups: CarrierCarrierScacGroup[]): SelectItem[] {
    const selectItems: SelectItem[] = [];
    groups.forEach(x => {
      x.carrierScacs.forEach(y => {
        selectItems.push({
          label: x.carrier.carrierName + ' - ' + y.scac,
          value: y.scac,
        });
      });
    });

    return selectItems.sort();
  }

  carrierChange(s: CustomerProfile) {
    this.carriers.sort((a, b) => this.carrierSort(a, b, s));
  }

  carrierSort(a: SelectItem, b: SelectItem, s: CustomerProfile): number {
    if (s && s.customerCarrierScacs.length > 0) {
      const selectedA = s.customerCarrierScacs.indexOf(a.value) >= 0;
      const selectedB = s.customerCarrierScacs.indexOf(b.value) >= 0;

      if (selectedA !== selectedB) {
        return selectedA ? -1 : 1;
      }
    }

    return a.label.localeCompare(b.label);
  }

  viewShipperMappings() {
    this.displayShipperMappingsDialog = true;
  }

  isShipperMappingEligable(s: CustomerProfile): boolean {
    if (s.topsOwnerId && s.customerId && s.identUserSetup) {
      return true;
    }
    return false;
  }

  enableShipperApi(s: CustomerProfile) {
    this.adminStore.dispatch(new ShipperProfileEnableShipperApiAction(s));
  }

  isShipperApiEligable(s: CustomerProfile): boolean {
    if (s.topsOwnerId && s.customerId && !s.identUserSetup) {
      return true;
    }
    return false;
  }
}
