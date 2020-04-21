import { OrderEntryEffects } from './order-entry.effects';
import { RatingEffects } from './rating.effects';
import { ShippingBookedEffects } from './shipping-booked.effects';
import { ShippingDeliveredEffects } from './shipping-delivered.effects';
import { ShippingLoadAuditLogEffects } from './shipping-load-audit-logs.effects';
import { ShippingLoadCarrierGroupEffects } from './shipping-load-carrier-group.effects';
import { ShippingLoadCarrierScacEffects } from './shipping-load-carrier-scac.effects';
import { ShippingLoadCarrierScacRestrictionEffects } from './shipping-load-carrier-scac-restriction.effects';
import { ShippingLoadDetailEffects } from './shipping-load-detail.effects';
import { ShippingPostedEffects } from './shipping-posted.effects';

export const effects: any[] = [
  ShippingLoadDetailEffects,
  ShippingPostedEffects,
  ShippingBookedEffects,
  ShippingDeliveredEffects,
  ShippingLoadAuditLogEffects,
  ShippingLoadCarrierGroupEffects,
  ShippingLoadCarrierScacEffects,
  ShippingLoadCarrierScacRestrictionEffects,
  OrderEntryEffects,
  RatingEffects,
];

export * from './shipping-booked.effects';
export * from './shipping-delivered.effects';
export * from './shipping-load-audit-logs.effects';
export * from './shipping-load-carrier-group.effects';
export * from './shipping-load-carrier-scac.effects';
export * from './shipping-load-carrier-scac-restriction.effects';
export * from './shipping-load-detail.effects';
export * from './shipping-posted.effects';
