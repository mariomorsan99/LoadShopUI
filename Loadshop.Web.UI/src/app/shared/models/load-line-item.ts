import { defaultDeliveryStop, defaultPickupStop } from './load-stop';

export interface LoadLineItem {
  loadLineItemNumber: number;
  quantity: number;
  unitOfMeasure: string;
  unitOfMeasureId: string;
  weight: number;
  customerPurchaseOrder: string;
  pickupStopNumber: number;
  deliveryStopNumber: number;
}

export const defaultLoadLineItem: LoadLineItem = {
  loadLineItemNumber: null,
  quantity: null,
  unitOfMeasure: null,
  unitOfMeasureId: null,
  weight: null,
  customerPurchaseOrder: null,
  pickupStopNumber: defaultPickupStop.stopNbr,
  deliveryStopNumber: defaultDeliveryStop.stopNbr,
};
