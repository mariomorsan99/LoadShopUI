import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { select, Store } from '@ngrx/store';
import { combineLatest, Observable, Subject } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';
import {
  CoreState,
  getCarriers,
  getCommodities,
  getLoadingCarriers,
  getLoadingCommodities,
  getLoadingServiceTypes,
  getServiceTypes,
  getSmartSpotLoading,
  getSmartSpotPrice,
  SmartSpotPriceLoadAction,
} from 'src/app/core/store';
import {
  Carrier,
  Commodity,
  defaultSmartSpotPriceRequest,
  ILoadAuditLogData,
  IShippingLoadDetail,
  LoadCarrierGroupData,
  LoadCarrierScacData,
  LoadCarrierScacRestrictionData,
  ServiceType,
  ShippingLoadDetail,
  ShippingLoadDetailError,
  ValidationProblemDetails,
} from 'src/app/shared/models';
import {
  getShippingLoadAuditLogForLoadId,
  getShippingLoadCarrierGroupsForLoadId,
  getShippingLoadCarrierScacRestrictionsForLoadId,
  getShippingLoadCarrierScacsForLoadId,
  getShippingLoadLoadingAuditLogForLoadId,
  getShippingLoadLoadingCarrierGroupForLoadId,
  getShippingLoadLoadingCarrierScacForLoadId,
  getShippingLoadLoadingCarrierScacRestrictionForLoadId,
  ShippingLoadAuditLogsLoadAction,
  ShippingLoadCarrierGroupsLoadAction,
  ShippingLoadCarrierScacRestrictionsLoadAction,
  ShippingLoadCarrierScacsLoadAction,
  ShippingLoadDetailUpdateLoadAction,
  ShippingState,
} from '../../../store';

@Component({
  selector: 'kbxl-shipping-load-detail-container',
  templateUrl: './shipping-load-detail-container.component.html',
  styleUrls: ['./shipping-load-detail-container.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShippingLoadDetailContainerComponent implements OnInit {
  private _detail: ShippingLoadDetail;
  destroyed$ = new Subject<boolean>();

  @Input() set detail(value: ShippingLoadDetail) {
    this._detail = value;

    this.loadCarrierGroups$ = this.shippingStore.pipe(select(getShippingLoadCarrierGroupsForLoadId, { loadId: this._detail.loadId }));
    this.loadCarrierScacs$ = this.shippingStore.pipe(select(getShippingLoadCarrierScacsForLoadId, { loadId: this._detail.loadId }));
    this.loadCarrierScacRestrictions$ = this.shippingStore.pipe(
      select(getShippingLoadCarrierScacRestrictionsForLoadId, {
        loadId: this._detail.loadId,
      })
    );
    this.loadAuditLogs$ = this.shippingStore.pipe(select(getShippingLoadAuditLogForLoadId, { loadId: this._detail.loadId }));
    this.loadingAuditLogs$ = this.shippingStore.pipe(select(getShippingLoadLoadingAuditLogForLoadId, { loadId: this._detail.loadId }));
    this.loadingCarriers$ = combineLatest(
      this.shippingStore.pipe(select(getShippingLoadLoadingCarrierGroupForLoadId, { loadId: this._detail.loadId })),
      this.coreStore.pipe(select(getLoadingCarriers))
    ).pipe(map((args) => args[0] || args[1]));
    this.loadingScacs$ = this.shippingStore.pipe(select(getShippingLoadLoadingCarrierScacForLoadId, { loadId: this._detail.loadId }));
    this.loadingScacRestrictions$ = this.shippingStore.pipe(
      select(getShippingLoadLoadingCarrierScacRestrictionForLoadId, {
        loadId: this._detail.loadId,
      })
    );
    this.loadingSmartSpot$ = this.coreStore.pipe(select(getSmartSpotLoading, { loadId: this._detail.loadId }));

    this.shippingStore.dispatch(new ShippingLoadCarrierGroupsLoadAction({ loadId: this._detail.loadId }));
    this.shippingStore.dispatch(new ShippingLoadCarrierScacsLoadAction({ loadId: this._detail.loadId }));
    this.shippingStore.dispatch(new ShippingLoadCarrierScacRestrictionsLoadAction({ loadId: this._detail.loadId }));
    this.shippingStore.dispatch(new ShippingLoadAuditLogsLoadAction({ loadId: this._detail.loadId }));

    this.coreStore
      .pipe(select(getSmartSpotPrice, { loadId: this._detail.loadId }), takeUntil(this.destroyed$))
      .subscribe((smartSpotPrice) => {
        if (smartSpotPrice && Number(smartSpotPrice.price)) {
          this._detail.smartSpotRate = smartSpotPrice.price;

          if (!this._detail.lineHaulRate || this._detail.lineHaulRate === 0) {
            if (this._detail.shippersFSC && this._detail.shippersFSC > 0) {
              this._detail.lineHaulRate = this._detail.smartSpotRate - this._detail.shippersFSC;
            } else {
              this._detail.lineHaulRate = this.detail.smartSpotRate;
            }
            this.updateSummaryCard(this._detail);
          }
        }
      });
  }
  get detail() {
    return this._detail;
  }

  private _problemDetails: ValidationProblemDetails;
  @Input() set errors(value: ValidationProblemDetails) {
    this._problemDetails = value;

    if (!value || !value.errors) {
      return;
    }
    if (!this.detail && !this.detail.loadId) {
      return;
    }

    // Map from ValidationProblemDetails format to shipping load detail form format
    const decodedErrors: ShippingLoadDetailError = new ShippingLoadDetailError();
    decodedErrors.root = value.errors['urn:root'];
    decodedErrors.load = value.errors[`urn:load:${this.detail.loadId}`];
    decodedErrors.commodity = value.errors[`urn:load:${this.detail.loadId}:Commodity`];
    decodedErrors.lineHaulRate = value.errors[`urn:load:${this.detail.loadId}:LineHaulRate`];
    decodedErrors.shippersFSC = value.errors[`urn:load:${this.detail.loadId}:ShippersFSC`];
    this.detailErrors = decodedErrors;
  }
  get errors() {
    return this._problemDetails;
  }

  // Generic validation problem details mapped to a typed set of errors for the Shipping Load Detail component
  detailErrors: ShippingLoadDetailError = null;

  @Output() deselectLoad = new EventEmitter<ShippingLoadDetail>();

  loadCarrierGroups$: Observable<LoadCarrierGroupData[]>;
  loadCarrierScacs$: Observable<LoadCarrierScacData[]>;
  loadCarrierScacRestrictions$: Observable<LoadCarrierScacRestrictionData[]>;
  carriers$: Observable<Carrier[]>;
  loadingCarriers$: Observable<boolean>;
  loadingScacs$: Observable<boolean>;
  loadingScacRestrictions$: Observable<boolean>;
  commodities$: Observable<Commodity[]>;
  loadingCommodities$: Observable<boolean>;
  loadAuditLogs$: Observable<ILoadAuditLogData[]>;
  loadingAuditLogs$: Observable<boolean>;
  loadingSmartSpot$: Observable<boolean>;
  serviceTypes$: Observable<ServiceType[]>;
  loadingServiceTypes$: Observable<boolean>;
  displayCarrierClicksDialog = false;

  constructor(private shippingStore: Store<ShippingState>, private coreStore: Store<CoreState>) {}

  ngOnInit() {
    this.carriers$ = this.coreStore.pipe(select(getCarriers));

    this.commodities$ = this.coreStore.pipe(select(getCommodities));
    this.loadingCommodities$ = this.coreStore.pipe(select(getLoadingCommodities));
    this.serviceTypes$ = this.coreStore.pipe(select(getServiceTypes));
    this.loadingServiceTypes$ = this.coreStore.pipe(select(getLoadingServiceTypes));
  }

  viewCarrierClicks() {
    this.displayCarrierClicksDialog = true;
  }

  close(load: ShippingLoadDetail) {
    this.deselectLoad.emit(load);
  }

  updateSmartSpotPrice(obj: any) {
    const request = {
      ...defaultSmartSpotPriceRequest,
      loadId: obj.details.loadId,
      weight: obj.details.weight,
      commodity: obj.details.commodity,
      equipmentId: obj.details.equipmentId,
      carrierIds: obj.carrierIds,
    };
    this.coreStore.dispatch(new SmartSpotPriceLoadAction([request]));
  }

  updateSummaryCard(detail: IShippingLoadDetail) {
    this.shippingStore.dispatch(new ShippingLoadDetailUpdateLoadAction(detail));
  }
}
