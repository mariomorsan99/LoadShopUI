import { Carrier, LoadStop, ServiceType } from '.';

// eslint-disable-next-line @typescript-eslint/interface-name-prefix
export interface IShippingLoadDetail {
  loadId: string;
  referenceLoadId: string;
  referenceLoadDisplay: string;
  mileage: number;
  commodity: string;
  weight: number;
  equipmentId: string;
  equipmentCategoryId: string;
  equipmentCategoryDesc: string;
  equipmentTypeDisplay: string;
  comments: string;
  lineHaulRate: number;
  smartSpotRate: number;
  shippersFSC: number;
  loadStops: LoadStop[];
  onLoadshop: boolean;
  scacsSentWithLoad: boolean;
  isEstimatedFSC: boolean;
  allowEditingFuel: boolean;
  manuallyCreated: boolean;
  customerLoadTypeId: number;
  hCapRate: number;
  xCapRate: number;
  datGuardRate: number;
  machineLearningRate: number;
  hasScacRestrictions: boolean;
  carrierGroupIds: number[];
  allCarriersPosted: boolean;
  serviceTypes: ServiceType[];
}

export class ShippingLoadDetail implements IShippingLoadDetail {
  constructor(init?: Partial<ShippingLoadDetail>) {
    this.hasChanges = false;

    Object.assign(this, init);
  }

  loadId: string;
  referenceLoadId: string;
  referenceLoadDisplay: string;
  mileage: number;
  commodity: string;
  weight: number;
  equipmentId: string;
  equipmentCategoryId: string;
  equipmentCategoryDesc: string;
  equipmentTypeDisplay: string;
  comments: string;
  lineHaulRate: number;
  smartSpotRate: number;
  shippersFSC: number;
  loadStops: LoadStop[];
  onLoadshop: boolean;
  scacsSentWithLoad: boolean;
  isEstimatedFSC: boolean;
  allowEditingFuel: boolean;
  manuallyCreated: boolean;
  customerLoadTypeId: number;
  hCapRate: number;
  xCapRate: number;
  datGuardRate: number;
  machineLearningRate: number;
  hasScacRestrictions: boolean;

  selectedGroupCarriers: Carrier[];
  selectedCarriers: Carrier[];
  hasChanges: boolean;
  carrierGroupIds: number[];
  allCarriersPosted: boolean;
  serviceTypes: ServiceType[];
}
