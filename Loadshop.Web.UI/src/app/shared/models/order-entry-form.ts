import { LoadContact, defaultLoadContact } from './load-contact';
import { LoadLineItem, defaultLoadLineItem } from './load-line-item';
import { OrderEntryLoadStop, defaultOrderEntryPickupStop, defaultOrderEntryDeliveryStop } from './order-entry-load-stop';
import { ServiceType } from '.';

export interface OrderEntryForm {
    loadId: string;
    referenceLoadDisplay: string;
    referenceLoadId: string;
    commodity: string;
    equipment: string;
    equipmentDesc: string;
    categoryEquipmentDesc: string;
    shipperPickupNumber: string;
    transportationMode: string;
    specialInstructions: string;
    onLoadshop: boolean;
    miles: number;
    lineHaulRate: number;
    fuelRate: number;
    weight: number;
    cube: number;

    services: ServiceType[];
    loadStops: OrderEntryLoadStop[];
    contacts: LoadContact[];
    lineItems: LoadLineItem[];
}

export const defaultOrderEntry: OrderEntryForm = {
    loadId: null,
    referenceLoadDisplay: null,
    referenceLoadId: null,
    commodity: null,
    equipment: null,
    equipmentDesc: null,
    categoryEquipmentDesc: null,
    shipperPickupNumber: null,
    transportationMode: null,
    specialInstructions: null,
    onLoadshop: false,
    miles: 0,
    lineHaulRate: 0,
    fuelRate: 0,
    weight: 0,
    cube: 0,

    services: [],
    loadStops: [{ ...defaultOrderEntryPickupStop }, { ...defaultOrderEntryDeliveryStop }],
    contacts: [defaultLoadContact],
    lineItems: [{...defaultLoadLineItem}]
};
