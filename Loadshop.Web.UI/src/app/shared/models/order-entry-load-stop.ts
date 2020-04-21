import { LoadStop } from '.';
import { defaultPickupStop, defaultDeliveryStop } from './load-stop';

export interface OrderEntryLoadStop extends LoadStop {
    earlyDate: any;
    earlyTime: any;
    lateDate: any;
    lateTime: any;
    stateName: any;
}

export const defaultOrderEntryPickupStop: OrderEntryLoadStop = {
    ...defaultPickupStop,
    earlyDate: '',
    earlyTime: '',
    lateDate: '',
    lateTime: '',
    stateName: ''
};

export const defaultOrderEntryDeliveryStop: OrderEntryLoadStop = {
    ...defaultDeliveryStop,
    earlyDate: '',
    earlyTime: '',
    lateDate: '',
    lateTime: '',
    stateName: ''
};
