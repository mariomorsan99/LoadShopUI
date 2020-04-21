// eslint-disable-next-line @typescript-eslint/interface-name-prefix
export interface ILoadAuditLogData {
  loadAuditLogId: number;
  auditTypeId: string;
  loadId: string;
  referenceLoadId: string;
  customerId: string;
  stops: number;
  pickupDtTm: Date;
  deliveryDtTm: Date;
  miles: number;
  lineHaulRate: number;
  fuelRate: number;
  weight: number;
  cube: number;
  commodity: string;
  equipmentId: string;
  isHazMat: boolean;
  comments: string;
  userId: string;
  scac: string;
  contractRate: number;
  carrierName: string;
  userName: string;
  firstName: string;
  email: string;
  phone: string;
}

export class LoadAuditLogData implements ILoadAuditLogData {
  constructor(init?: Partial<LoadAuditLogData>) {
    Object.assign(this, init);
  }

  loadAuditLogId: number;
  auditTypeId: string;
  loadId: string;
  referenceLoadId: string;
  customerId: string;
  stops: number;
  pickupDtTm: Date;
  deliveryDtTm: Date;
  miles: number;
  lineHaulRate: number;
  fuelRate: number;
  weight: number;
  cube: number;
  commodity: string;
  equipmentId: string;
  isHazMat: boolean;
  comments: string;
  userId: string;
  scac: string;
  contractRate: number;
  carrierName: string;
  userName: string;
  firstName: string;
  email: string;
  phone: string;
  createDtTm: Date;
}
