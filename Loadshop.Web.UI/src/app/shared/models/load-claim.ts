export interface LoadClaim {
  loadClaimId: string;
  loadTransactionId: string;
  lineHaulRate: number;
  fuelRate: number;
  scac: string;
  userId: string;
  isCounterOffer: boolean;
  billingLoadId: string;
  billingLoadDisplay: string;
  visibilityPhoneNumber: string;
  visibilityTruckNumber: string;
  mobileExternallyEntered: boolean;
  visibilityChgDtTm: Date;
}
