import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Observable, combineLatest } from 'rxjs';
import { Carrier, UserAdminData, CarrierProfile } from 'src/app/shared/models';
import { Store, select } from '@ngrx/store';
import {
  AdminState,
  UserAdminLoadAdminUsersAction,
  getAdminUsers,
  getCarrierProfile,
  CarrierProfileLoadAction,
  CarrierProfileUpdateAction,
  getCarrierProfileLoading,
  getAdminUsersLoading,
  getCarrierProfileUpdating,
  CarrierProfileCancelUpdateAction,
  getAllCarriersLoading,
  getAllCarriers,
  CarrierProfileLoadAllAction
} from 'src/app/admin/store';
import { map } from 'rxjs/operators';

@Component({
  selector: 'kbxl-carrier-profile-container',
  templateUrl: './carrier-profile-container.component.html',
  styleUrls: ['./carrier-profile-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CarrierProfileContainerComponent {

  carriers$: Observable<Carrier[]>;
  adminUsers$: Observable<UserAdminData[]>;
  processing$: Observable<boolean>;
  selectedCarrierProfile$: Observable<CarrierProfile>;

  constructor(private adminStore: Store<AdminState>) {

    this.adminStore.dispatch(new CarrierProfileLoadAllAction());
    this.adminStore.dispatch(new UserAdminLoadAdminUsersAction());

    this.adminUsers$ = this.adminStore.pipe(select(getAdminUsers));
    this.carriers$ = this.adminStore.pipe(select(getAllCarriers));

    this.selectedCarrierProfile$ = this.adminStore.pipe(select(getCarrierProfile));

    this.processing$ = combineLatest(
      this.adminStore.pipe(map(getCarrierProfileLoading)),
      this.adminStore.pipe(map(getAllCarriersLoading)),
      this.adminStore.pipe(map(getAdminUsersLoading)),
      this.adminStore.pipe(map(getCarrierProfileUpdating))
    ).pipe(map(args => args[0] || args[1] || args[2] || args[3]));
  }

  carrierSelected(carrier: Carrier) {
    this.adminStore.dispatch(new CarrierProfileLoadAction({ carrierId: carrier.carrierId }));
  }

  update(carrier: CarrierProfile) {
    this.adminStore.dispatch(new CarrierProfileUpdateAction(carrier));
  }

  cancelUpdateClick() {
    this.adminStore.dispatch(new CarrierProfileCancelUpdateAction());
  }

}
