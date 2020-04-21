import { Carrier } from '.';
import { defaultLoadCarrierGroup, LoadCarrierGroupData } from './load-carrier-group-data';

export interface ShippingLoadCarrierGroupData extends LoadCarrierGroupData {
  shippingLoadCarrierGroupDisplay: string;
  carriers: Carrier[];
}

export const defaultShippingLoadCarrierGroup: ShippingLoadCarrierGroupData = {
  ...defaultLoadCarrierGroup,
  shippingLoadCarrierGroupDisplay: null,
  carriers: [],
};
