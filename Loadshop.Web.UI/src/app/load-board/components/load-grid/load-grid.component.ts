import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import { LazyLoadEvent } from 'primeng/api';
import { OverlayPanel } from 'primeng/overlaypanel';
import { Table } from 'primeng/table';
import { SecurityAppActionType } from 'src/app/shared/models/security-app-action-type';
import { PageableQueryData } from 'src/app/shared/utilities';
import { LoadView, ServiceType, UserLane, UserModel, VisibilityBadge } from '../../../shared/models';

@Component({
  selector: 'kbxl-load-grid',
  templateUrl: './load-grid.component.html',
  styleUrls: ['./load-grid.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadGridComponent implements OnInit, OnChanges {
  @Input() loads: LoadView[];
  @Input() lanes: UserLane[];
  @Input() loading: boolean;
  @Input() isMarketplace: boolean;
  @Input() serviceTypes: ServiceType[];
  @Output() selected: EventEmitter<string> = new EventEmitter<string>();
  @Output() updateStatus: EventEmitter<string> = new EventEmitter<string>();
  @Output() updateVisibility: EventEmitter<LoadView> = new EventEmitter<LoadView>();
  @Output() lazyLoad: EventEmitter<PageableQueryData> = new EventEmitter<PageableQueryData>();

  sortName: string;
  sortOrder: number;
  first = 0;
  numberOfColumns = 10;
  @Input() isBookedLoads = false;
  @Input() showExport = false;
  @Input() user: UserModel = null;
  @Input() savingLoadId: string = null;
  @Input() errorLoadId: string = null;
  @Input() phoneError = false;
  @Input() truckError = false;
  @Input() lazy = false;
  @Input() visibilityBadge: VisibilityBadge = null;
  @Input() totalRecords: number;

  columns: any[];

  @ViewChild(Table) table;

  editingTruckNumberLoadId: string;
  editingPhoneNumberLoadId: string;
  originalLoadViews: { [s: string]: LoadView } = {};

  hoveredPricingRow: LoadView;
  isDisabled = false;

  get displayStatusColumn() {
    return (
      this.isBookedLoads &&
      this.user &&
      this.user.hasSecurityAction(SecurityAppActionType.CarrierViewStatus) &&
      this.loads &&
      this.loads.find((_) => _.isPlatformPlus)
    );
  }

  ngOnInit() {
    this.columns = [
      { field: 'referenceLoadDisplay', header: 'Shipper’s Order Number' },
      { field: 'billingLoadDisplay', header: 'Billing Load ID' },
      { field: 'originDisplay', header: 'Origin' },
      { field: 'destinationDisplay', header: 'Destination' },
      { field: 'distanceFrom', header: 'Origin Radius' },
    ];

    this.numberOfColumns = 9;
    if (this.isBookedLoads) {
      this.numberOfColumns = 10;
      if (this.showPhoneNumberColumn()) {
        this.numberOfColumns++;
        this.columns.push({ field: 'visibilityPhoneNumber', header: 'Phone #' });
      }
      if (this.showTruckNumberColumn()) {
        this.numberOfColumns++;
        this.columns.push({ field: 'visibilityTruckNumber', header: 'Truck #' });
      }
    }

    this.columns.push(
      { field: 'originLateDtTm', header: 'Pickup' },
      { field: 'destLateDtTm', header: 'Delivery' },
      { field: 'equipmentType', header: 'Equipment' },
      { field: 'stops', header: 'Stops' },
      { field: 'miles', header: 'Distance (mi)' },
      { field: 'lineHaulRate', header: 'Line Haul Rate' },
      { field: 'fuelRate', header: 'FSC Rate' },
      { field: 'totalRateDisplay', header: 'All-in Rate' },
      { field: 'scac', header: 'SCAC' }
    );
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes) {
      if (changes.loads && !this.lazy) {
        this.first = 0;
      }
    }
  }

  onRowSelect(load: LoadView) {
    if (!this.isDisabled) {
      this.selected.emit(load.loadId);
    }
  }

  onStatusClicked(load: LoadView, event: Event) {
    event.preventDefault();
    event.stopPropagation();
    if (!this.isDisabled) {
      this.updateStatus.emit(load.loadId);
    }
  }

  onPage($event) {
    if ($event && $event.first && !this.lazy) {
      this.first = $event.first;
    }
  }

  showVisibilityWarning(load: LoadView): boolean {
    const preReqs = this.isBookedLoads && load && this.visibilityBadge && this.user && this.visibilityBadge.applicableDate;
    if (!preReqs) {
      return false;
    }

    /**
     * TODO: (jwv) I don't like having this logic effectively duplicated here
     * and in the API, but ngrx confusion has caused a lot of delays in completing
     * this feature, so maybe we can refactor that later, after this is confirmed
     * to even be what the testers want.
     */
    const pickup = new Date(load.originLateDtTm);
    const badgeDate = new Date(this.visibilityBadge.applicableDate);
    const showBasedOnDate = pickup > new Date() && pickup <= badgeDate;

    if (showBasedOnDate) {
      const delivery = load.destLateDtTm.toString();
      const isMissingTruckNumber =
        !this.showEditTruckNumber(delivery) || (this.showEditTruckNumber(delivery) && !load.visibilityTruckNumber);
      const isMissingPhoneNumber =
        !this.showEditPhoneNumber(delivery) ||
        (this.showEditPhoneNumber(delivery) && !load.visibilityPhoneNumber && !load.mobileExternallyEntered);

      return isMissingTruckNumber && isMissingPhoneNumber;
    }

    return false;
  }

  hasDeliveryDatePassed(deliveryDate: string): boolean {
    return deliveryDate && new Date(deliveryDate) < new Date();
  }

  showTruckNumberColumn(): boolean {
    return this.isBookedLoads && this.user && this.user.carrierVisibilityTypes && this.user.carrierVisibilityTypes.includes('Project 44');
  }

  showROTruckNumber(deliveryDate: string): boolean {
    return this.showTruckNumberColumn() && this.hasDeliveryDatePassed(deliveryDate);
  }

  showEditTruckNumber(deliveryDate: string): boolean {
    return this.showTruckNumberColumn() && !this.hasDeliveryDatePassed(deliveryDate);
  }

  showPhoneNumberColumn(): boolean {
    return this.isBookedLoads && this.user && this.user.carrierVisibilityTypes && this.user.carrierVisibilityTypes.includes('TOPS TO GO');
  }

  showROPhoneNumber(deliveryDate: string): boolean {
    return this.showPhoneNumberColumn() && this.hasDeliveryDatePassed(deliveryDate);
  }

  showEditPhoneNumber(deliveryDate: string): boolean {
    return this.showPhoneNumberColumn() && !this.hasDeliveryDatePassed(deliveryDate);
  }

  onEditTruckNumberInit(event: Event, load: LoadView) {
    this.editingTruckNumberLoadId = load.loadId;
    this.onEditInit(event, load);
  }

  onEditPhoneNumberInit(event: Event, load: LoadView) {
    this.editingPhoneNumberLoadId = load.loadId;
    this.onEditInit(event, load);
  }

  onEditInit(event: Event, load: LoadView) {
    event.stopPropagation();
    this.originalLoadViews[load.loadId] = { ...load };
  }

  onEditComplete(field: string, load: LoadView, index: number, table: Table, event: Event) {
    event.stopPropagation();
    const orig = this.originalLoadViews[load.loadId];
    const origValue = orig[field];
    let newValue = load[field] ? load[field].trim() : null;
    if (newValue === '(___) ___-____') {
      // blank value submitted via input mask on phone number
      newValue = null;
    }

    if (newValue && origValue !== newValue) {
      // Only update visibility fields if the value has changed
      this.updateVisibility.emit(load);
    }

    if (!newValue) {
      // If the user has cleared out the field, set it back to the original value before deleting the original
      load[field] = origValue;
    }

    // Always delete the clone when editing is complete, whether we've saved a new
    // value or cancelled by not changing the value
    delete this.originalLoadViews[load.loadId];
    this.editingTruckNumberLoadId = null;
    this.editingPhoneNumberLoadId = null;

    if (table) {
      // https://github.com/primefaces/primeng/issues/5368
      table.onEditComplete.emit({ field: null, data: null });
      table.editingCell = null;
    }

    if (!newValue && origValue) {
      // Throw an error if attempting to clear out an existing value
      const fld = this.columns.find((x) => x.field === field);

      if (fld) {
        const fieldLabel = this.columns.find((x) => x.field === field).header;
        throw new Error(`${fieldLabel} is required`);
      }
    }
  }

  setFirst(first: number) {
    this.first = first;
  }

  onLazyLoad($event: LazyLoadEvent) {
    const pageSize = $event.rows;
    const pageNumber = $event.first / $event.rows + 1;
    const sortField = $event.sortField;
    const descending = $event.sortOrder === -1;
    this.first = $event.first;

    this.lazyLoad.emit({ pageSize, pageNumber, filter: null, sortField: sortField, descending: descending });
  }

  serviceTypeMouseHover(load: LoadView, serviceTypesOverlay: any, event: Event): void {
    if (!load.serviceTypeIds || load.serviceTypeIds.length < 2) {
      return;
    }
    this.hoveredPricingRow = load;
    serviceTypesOverlay.show(event);
  }
  serviceTypeMouseHoverOut(serviceTypesOverlay: OverlayPanel): void {
    this.hoveredPricingRow = null;
    serviceTypesOverlay.hide();
  }

  getServiceTypes(load: LoadView): string {
    if (!load.serviceTypeIds || load.serviceTypeIds.length === 0 || !this.serviceTypes) {
      return '';
    }

    if (load.serviceTypeIds.length > 1) {
      return load.serviceTypeIds.length.toString();
    }
    // return the first service type
    const first = this.serviceTypes.find((x) => x.serviceTypeId === load.serviceTypeIds[0]);
    if (first) {
      return first.name;
    }
    return 'ERROR';
  }
  getServiceTypesHover(load: LoadView): string {
    if (!load.serviceTypeIds || !this.serviceTypes) {
      return '';
    }

    return this.getServiceTypeNames(load);
  }

  getServiceTypeNames(load: LoadView): string {
    const names = [];
    load.serviceTypeIds.forEach((element) => {
      const serviceType = this.serviceTypes.find((x) => x.serviceTypeId === element);
      names.push(serviceType.name);
    });

    return names.join(', ');
  }
}
