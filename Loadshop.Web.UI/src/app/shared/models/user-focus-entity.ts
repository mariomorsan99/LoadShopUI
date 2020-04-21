export interface UserFocusEntity {
  id: string;
  name: string;
  type: UserFocusEntityType;
  group: string;
}

export enum UserFocusEntityType {
  CarrierScac,
  Shipper,
}
