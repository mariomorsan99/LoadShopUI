import { AllMessageTypes, NotificationMessageTypes } from './message-type';
import { SearchTypeData } from './search';
import { UserLaneMessageType } from './user-lane-message-type';

export interface UserLane {
  userLaneId: string;
  userId: string;
  searchType: SearchTypeData;
  origCity: string;
  origState: string;
  origCountry: string;
  origLat: number;
  origLng: number;
  origDH: number;
  destCity: string;
  destState: string;
  destCountry: string;
  destLat: number;
  destLng: number;
  destDH: number;
  monday: boolean;
  tuesday: boolean;
  wednesday: boolean;
  thursday: boolean;
  friday: boolean;
  saturday: boolean;
  sunday: boolean;
  userLaneMessageTypes: UserLaneMessageType[];
  laneNotifications: NotificationMessageTypes;
  equipmentIds: string[];
  equipmentCount: number;
  equipmentDisplay: string;
  display: boolean;
}

export const defaultLane: UserLane = {
  userLaneId: '',
  userId: '',
  searchType: SearchTypeData.OriginDest,
  destCity: null,
  destCountry: null,
  destDH: null,
  destLat: null,
  destLng: null,
  destState: null,
  friday: true,
  monday: true,
  origCity: null,
  origCountry: null,
  origDH: null,
  origLat: null,
  origLng: null,
  origState: null,
  saturday: true,
  sunday: true,
  thursday: true,
  tuesday: true,
  wednesday: true,
  laneNotifications: {
    Email: true,
    Cell_Phone: false,
  },
  userLaneMessageTypes: [
    {
      description: 'Email',
      messageTypeId: AllMessageTypes.Email,
      selected: true,
    },
    {
      description: 'Text Message',
      messageTypeId: AllMessageTypes.CellPhone,
      selected: false,
    },
  ],
  equipmentIds: [],
  equipmentCount: 0,
  equipmentDisplay: null,
  display: true,
};
