import { ActionReducerMap, createFeatureSelector } from '@ngrx/store';
import { OrderEntryReducer, OrderEntryState } from './order-entry.reducer';
import { RatingReducer, RatingState } from './rating.reducer';
import { ShippingBookedReducer, ShippingBookedState } from './shipping-booked.reducer';
import { ShippingDeliveredReducer, ShippingDeliveredState } from './shipping-delivered.reducer';
import { ShippingLoadAuditLogReducer, ShippingLoadAuditLogState } from './shipping-load-audit-log.reducer';
import { ShippingLoadCarrierGroupReducer, ShippingLoadCarrierGroupState } from './shipping-load-carrier-group.reducer';
import { ShippingLoadCarrierScacReducer, ShippingLoadCarrierScacState } from './shipping-load-carrier-scac.reducer';
import {
  ShippingLoadCarrierScacRestrictionReducer,
  ShippingLoadCarrierScacRestrictionState,
} from './shipping-load-carrier-scac-restriction.reducer';
import { ShippingLoadDetailReducer, ShippingLoadDetailState } from './shipping-load-detail.reducer';
import { ShippingPostedReducer, ShippingPostedState } from './shipping-posted.reducer';

export interface ShippingState {
  loadDetail: ShippingLoadDetailState;
  shippingPosted: ShippingPostedState;
  shippingBooked: ShippingBookedState;
  shippingDelivered: ShippingDeliveredState;
  loadAuditLog: ShippingLoadAuditLogState;
  loadCarrierGroup: ShippingLoadCarrierGroupState;
  loadCarrierScac: ShippingLoadCarrierScacState;
  loadCarrierScacRestriction: ShippingLoadCarrierScacRestrictionState;
  orderEntry: OrderEntryState;
  rating: RatingState;
}

export const reducers: ActionReducerMap<ShippingState> = {
  loadDetail: ShippingLoadDetailReducer,
  shippingPosted: ShippingPostedReducer,
  shippingBooked: ShippingBookedReducer,
  shippingDelivered: ShippingDeliveredReducer,
  loadAuditLog: ShippingLoadAuditLogReducer,
  loadCarrierGroup: ShippingLoadCarrierGroupReducer,
  loadCarrierScac: ShippingLoadCarrierScacReducer,
  loadCarrierScacRestriction: ShippingLoadCarrierScacRestrictionReducer,
  orderEntry: OrderEntryReducer,
  rating: RatingReducer,
};

export const getShippingFeatureState = createFeatureSelector<ShippingState>('shipping');
