import { LoadCarrierGroupCarrierEffects } from './load-carrier-group-carrier.effects';
import { LoadCarrierGroupEffects } from './load-carrier-group.effects';
import { ShipperProfileEffects } from './shipper-profile.effects';
import { SpecialInstructionsEffects } from './special-instructions.effects';
import { UserAdminEffects } from './user-admin.effects';
import { CarrierProfileEffects } from './carrier-profile.effects';
import { UserCommunicationEffects } from './user-communication.effects';

export const effects: any[] = [
  LoadCarrierGroupEffects,
  LoadCarrierGroupCarrierEffects,
  ShipperProfileEffects,
  UserAdminEffects,
  SpecialInstructionsEffects,
  CarrierProfileEffects,
  UserCommunicationEffects
];

export * from './load-carrier-group-carrier.effects';
export * from './load-carrier-group.effects';
export * from './shipper-profile.effects';
export * from './user-admin.effects';
export * from './carrier-profile.effects';
export * from './user-communication.effects';
