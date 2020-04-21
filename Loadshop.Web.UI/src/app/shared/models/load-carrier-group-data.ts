import { LoadCarrierGroupCarrierData } from '.';
import { LoadCarrierGroupEquipmentData } from './load-carrier-group-equipment-data';

export interface LoadCarrierGroupData {
  loadCarrierGroupId: number;
  groupName: string;
  groupDescription: string;
  customerId: string;
  originAddress1: string;
  originCity: string;
  originState: string;
  originPostalCode: string;
  originCountry: string;
  destinationAddress1: string;
  destinationCity: string;
  destinationState: string;
  destinationPostalCode: string;
  destinationCountry: string;
  loadCarrierGroupTypeId: number;
  loadCarrierGroupTypeName: string;

  loadCarrierGroupEquipment: LoadCarrierGroupEquipmentData[];
}

export interface LoadCarrierGroupDetailData extends LoadCarrierGroupData {
  // TODO: customer: Customer;
  carrierCount: number;
  carriers: LoadCarrierGroupCarrierData[];
}

export const defaultLoadCarrierGroup: LoadCarrierGroupDetailData = {
  loadCarrierGroupId: null,
  groupName: null,
  groupDescription: null,
  customerId: null,
  originAddress1: null,
  originCity: null,
  originState: null,
  originPostalCode: null,
  originCountry: null,
  destinationAddress1: null,
  destinationCity: null,
  destinationState: null,
  destinationPostalCode: null,
  destinationCountry: null,
  loadCarrierGroupEquipment: [],
  loadCarrierGroupTypeId: null,
  loadCarrierGroupTypeName: null,

  // TODO: customer: Customer;
  carrierCount: null,
  carriers: [],
};
