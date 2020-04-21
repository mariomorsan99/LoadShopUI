export interface LoadshopShipperMapping {
  loadshopShipperMappingId: number;
  ownerId: string;
  sourceSystem: string;
  loadshopShipperId: string;
  isActive: boolean;
  useCustomerOrderNumber: boolean;
  usesAllInRates: boolean;
  usesSmartSpotPricing: boolean;
  useTopsLoadContacts: boolean;
  useCustomerFuel: boolean;
  createDtTm: Date;
  createBy: string;
  lastChgDtTm: Date;
  lastChgBy: string;
}

export const defaultLoadshopShipperMapping: LoadshopShipperMapping = {
  loadshopShipperMappingId: 0,
  ownerId: null,
  sourceSystem: null,
  loadshopShipperId: null,
  isActive: true,
  useCustomerOrderNumber: false,
  usesAllInRates: false,
  usesSmartSpotPricing: false,
  useTopsLoadContacts: false,
  useCustomerFuel: false,
  createDtTm: new Date(),
  createBy: null,
  lastChgDtTm: new Date(),
  lastChgBy: null,
};
