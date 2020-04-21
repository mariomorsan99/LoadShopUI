import { NgModule } from '@angular/core';
import { EffectsModule } from '@ngrx/effects';
import { StoreModule } from '@ngrx/store';
import { CanDeactivateGuard } from '../core/guards/can-deactivate.guard';
import { SharedModule } from '../shared/shared.module';
import {
  ShippingBookedContainerComponent,
  ShippingDeliveredContainerComponent,
  ShippingGridComponent,
  ShippingLoadAuditLogGridComponent,
  ShippingLoadCardsComponent,
  ShippingLoadCreateContainerComponent,
  ShippingLoadDetailComponent,
  ShippingLoadDetailContainerComponent,
  ShippingLoadFilterComponent,
  ShippingLoadHomeContainerComponent,
  ShippingLoadHomeHeaderComponent,
  ShippingPostedContainerComponent,
  QuickQuoteContainerComponent,
  QuickQuoteComponent,
  ShippingSearchCriteriaComponent,
} from './components';
import { RatingService, ShippingService } from './services';
import { ShippingRoutingModule } from './shipping-routing.module';
import { effects, reducers } from './store';

@NgModule({
  imports: [ShippingRoutingModule, SharedModule, EffectsModule.forFeature(effects), StoreModule.forFeature('shipping', reducers)],
  declarations: [
    ShippingLoadDetailContainerComponent,
    ShippingLoadDetailComponent,
    ShippingPostedContainerComponent,
    ShippingBookedContainerComponent,
    ShippingDeliveredContainerComponent,
    ShippingGridComponent,
    ShippingLoadHomeContainerComponent,
    ShippingLoadCardsComponent,
    ShippingLoadHomeHeaderComponent,
    ShippingLoadAuditLogGridComponent,
    ShippingLoadFilterComponent,
    ShippingLoadCreateContainerComponent,
    QuickQuoteContainerComponent,
    QuickQuoteComponent,
    ShippingSearchCriteriaComponent,
  ],
  providers: [ShippingService, RatingService, CanDeactivateGuard],
  exports: [QuickQuoteComponent, QuickQuoteContainerComponent],
})
export class ShippingModule {}
