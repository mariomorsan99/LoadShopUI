import { TransactionType } from './transaction-type';

export interface Transaction {
  fuelRate: number;
  lineHaulRate: number;
  scac: string;
  transactionType: TransactionType;
  lastUpdateTime: Date;
}
