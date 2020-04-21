export interface CustomerLoadType {
  customerLoadTypeId: number;
  name: string;
}

export function isCustomerLoadType(x: any): x is CustomerLoadType {
  return typeof x.name === 'string'
      && typeof x.customerLoadTypeId === 'number' ;
}

export function isCustomerLoadTypeArray(x: any): x is CustomerLoadType[] {
  return x.every(isCustomerLoadType);
}

export enum CustomerLoadTypes {
  HighPriority = 1,
  NewShipper = 2
}
