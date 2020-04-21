import { LoadDocumentMetadata, ServiceType, UserContactData } from '.';
import { Contact } from './contact';
import { LoadStop } from './load-stop';
import { Transaction } from './transaction';
import { TransactionType } from './transaction-type';

export interface Load {
  comments: string;
  commodity: string;
  contacts: Contact[];
  cube: number;
  distanceFromOrig: number;
  distanceFromDest: number;
  equipmentId: string;
  equipmentType: string;
  equipmentCategoryId: string;
  equipmentCategoryEquipmentDesc: string;
  fuelRate: number;
  isAccepted: boolean;
  isHazMat: boolean;
  lineHaulRate: number;
  loadId: string;
  loadStops: LoadStop[];
  loadDocuments: LoadDocumentMetadata[];
  loadTransaction: Transaction;
  miles: number;
  referenceLoadId: string;
  referenceLoadDisplay: string;
  stops: number;
  userLaneId: string;
  weight: number;
  scac: string;
  bookedUser: UserContactData;
  equipmentTypeDisplay: string;
  platformPlusLoadId: string;
  isPlatformPlus: boolean;
  deliveredDate: null;
  serviceTypes: ServiceType[];
}

export const defaultLoad: Load = {
  loadId: '',
  userLaneId: null,
  referenceLoadId: '',
  referenceLoadDisplay: '',
  stops: 2,
  miles: 0,
  fuelRate: 0,
  lineHaulRate: 0,
  weight: 0,
  cube: 0,
  commodity: '',
  equipmentId: '',
  equipmentType: '',
  equipmentCategoryId: '',
  equipmentCategoryEquipmentDesc: '',
  equipmentTypeDisplay: '',
  isHazMat: false,
  isAccepted: false,
  comments: '',
  contacts: null,
  loadStops: [
    {
      loadStopId: null,
      loadId: null,
      stopNbr: 1,
      city: '',
      state: '',
      country: '',
      latitude: 0,
      longitude: 0,
      earlyDtTm: null,
      lateDtTm: null,
      apptType: '',
      instructions: '',
      appointmentConfirmationCode: null,
      appointmentSchedulingCode: null,
      locationName: null,
      address1: null,
      address2: null,
      address3: null,
      postalCode: null,
      isLive: true,
      stopType: null,
    },
    {
      loadStopId: null,
      loadId: null,
      stopNbr: 2,
      city: '',
      state: '',
      country: '',
      latitude: 0,
      longitude: 0,
      earlyDtTm: null,
      lateDtTm: null,
      apptType: '',
      instructions: '',
      appointmentConfirmationCode: null,
      appointmentSchedulingCode: null,
      locationName: null,
      address1: null,
      address2: null,
      address3: null,
      postalCode: null,
      isLive: true,
      stopType: null,
    },
  ],
  loadDocuments: [],
  loadTransaction: {
    fuelRate: 0,
    lineHaulRate: 0,
    scac: null,
    transactionType: TransactionType.New,
    lastUpdateTime: null,
  },
  distanceFromOrig: 0,
  distanceFromDest: 0,
  scac: '',
  bookedUser: null,
  platformPlusLoadId: null,
  isPlatformPlus: false,
  deliveredDate: null,
  serviceTypes: null,
};
