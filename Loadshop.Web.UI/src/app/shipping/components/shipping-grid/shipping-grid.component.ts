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
import { ShippingLoadView } from 'src/app/shared/models/shipping-load-view';
import { PageableComponent, PageableQueryData } from 'src/app/shared/utilities/';
import { ServiceType, User } from '../../../shared/models';

@Component({
  selector: 'kbxl-shipping-grid',
  templateUrl: './shipping-grid.component.html',
  styleUrls: ['./shipping-grid.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShippingGridComponent implements OnInit, OnChanges, PageableComponent {
  @Input() loads: ShippingLoadView[];
  @Input() totalRecords: number;
  @Input() loading: boolean;
  @Input() user: User;
  @Input() showExport = false;
  @Input() isBookedLoads = false;
  @Input() isDeliveredLoads = false;
  @Input() lazy = false;
  @Input() serviceTypes: ServiceType[];
  @Output() selected: EventEmitter<string> = new EventEmitter<string>();
  @Output() lazyLoad: EventEmitter<PageableQueryData> = new EventEmitter<PageableQueryData>();

  sortName: string;
  sortOrder: number;
  first = 0;
  numberOfColumns = 12;
  columns: any[];

  @ViewChild(Table) table;

  hoveredPricingRow: ShippingLoadView;

  ngOnInit() {
    this.columns = [
      { field: 'referenceLoadDisplay', header: 'Shipper’s Order Number' },
      { field: 'billingLoadDisplay', header: 'Billing Load ID' },
      { field: 'originDisplay', header: 'Origin' },
      { field: 'destinationDisplay', header: 'Destination' },
      { field: 'distanceFrom', header: 'Origin Radius' },
      { field: 'originLateDtTm', header: 'Pickup' },
      { field: 'destLateDtTm', header: 'Delivery' },
      { field: 'equipmentType', header: 'Equipment' },
      { field: 'stops', header: 'Stops' },
      { field: 'miles', header: 'Distance (mi)' },
      { field: 'totalRateDisplay', header: 'All-in Rate' },
      { field: 'scac', header: 'SCAC' },
    ];
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes) {
      if (changes.loads && !this.lazy) {
        this.first = 0;
      }
    }
  }

  setFirst(first: number) {
    this.first = first;
  }

  onRowSelect(load: ShippingLoadView) {
    this.selected.emit(load.loadId);
  }

  onPage($event) {
    if ($event && $event.first && !this.lazy) {
      this.first = $event.first;
    }
  }

  onLazyLoad($event: LazyLoadEvent) {
    const pageSize = $event.rows;
    const pageNumber = $event.first / $event.rows + 1;
    const sortField = $event.sortField;
    const descending = $event.sortOrder === -1;
    this.first = $event.first;

    this.lazyLoad.emit({ pageSize, pageNumber, filter: null, sortField: sortField, descending: descending });
  }

  serviceTypeMouseHover(load: ShippingLoadView, serviceTypesOverlay: any, event: Event): void {
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

  getServiceTypes(load: ShippingLoadView): string {
    if (!load.serviceTypeIds || load.serviceTypeIds.length === 0 || !this.serviceTypes) {
      return '';
    }

    if (load.serviceTypeIds.length > 1) {
      return load.serviceTypeIds.length.toString();
    }
    // return the first service type
    const first = this.serviceTypes.find((x) => x.serviceTypeId === load.serviceTypeIds[0]);
    return first.name;
  }
  getServiceTypesHover(load: ShippingLoadView): string {
    if (!load.serviceTypeIds || !this.serviceTypes) {
      return '';
    }

    return this.getServiceTypeNames(load);
  }

  getServiceTypeNames(load: ShippingLoadView): string {
    const names = [];
    load.serviceTypeIds.forEach((element) => {
      const serviceType = this.serviceTypes.find((x) => x.serviceTypeId === element);
      names.push(serviceType.name);
    });

    return names.join(', ');
  }
}
