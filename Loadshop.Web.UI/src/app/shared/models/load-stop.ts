import { LoadStopContact } from './load-stop-contact';
import { StopTypes } from './stop-types';

export interface LoadStop {
  loadStopId: string;
  loadId: string;
  stopNbr: number;
  city: string;
  state: string;
  country: string;
  latitude: number;
  longitude: number;
  earlyDtTm: any;
  lateDtTm: any;
  apptType: string;
  instructions: string;
  appointmentConfirmationCode: string;
  appointmentSchedulingCode: string;
  locationName: string;
  address1: string;
  address2: string;
  address3: string;
  postalCode: string;
  isLive: boolean;
  stopType: string;
  contacts?: LoadStopContact[];
}

export const defaultPickupStop: LoadStop = {
  loadStopId: null,
  loadId: null,
  stopNbr: 1,
  city: '',
  state: '',
  country: 'USA',
  latitude: 0.0,
  longitude: 0.0,
  earlyDtTm: '',
  lateDtTm: '',
  apptType: '',
  instructions: '',
  appointmentConfirmationCode: null,
  appointmentSchedulingCode: null,
  locationName: '',
  address1: '',
  address2: '',
  address3: '',
  postalCode: '',
  isLive: true,
  stopType: StopTypes[StopTypes.Pickup],
  contacts: [],
};

export const defaultDeliveryStop: LoadStop = {
  loadStopId: null,
  loadId: null,
  stopNbr: 2,
  city: '',
  state: '',
  country: 'USA',
  latitude: 0.0,
  longitude: 0.0,
  earlyDtTm: '',
  lateDtTm: '',
  apptType: '',
  instructions: '',
  appointmentConfirmationCode: null,
  appointmentSchedulingCode: null,
  locationName: '',
  address1: '',
  address2: '',
  address3: '',
  postalCode: '',
  isLive: true,
  stopType: StopTypes[StopTypes.Delivery],
  contacts: [],
};
