export interface LoadCarrierScacRestrictionData {
  scac: string;
  loadCarrierScacRestrictionTypeId: CarrierScacRestrictionTypes;
}

export const defaultLoadCarrierScacRestrictionData: LoadCarrierScacRestrictionData = {
  scac: null,
  loadCarrierScacRestrictionTypeId: null
};

export enum CarrierScacRestrictionTypes {
  UseOnly = 1,
  DoNotUse = 2
}
