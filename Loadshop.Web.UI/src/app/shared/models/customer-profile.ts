import { CustomerContact } from './customer-contact';

export interface CustomerProfile {
    customerId: string;
    name: string;
    defaultCommodity: string;
    useFuelRerating: boolean;
    fuelReratingNumberOfDays: number;
    allowZeroFuel: boolean;
    allowEditingFuel: boolean;
    topsOwnerId: string;
    successManagerUserId: string;
    successSpecialistUserId: string;
    comments: string;
    dataSource: string;
    customerLoadTypeId: string;
    customerLoadTypeExpirationDate: any;
    customerContacts: CustomerContact[];
    customerCarrierScacs: string[];
    autoPostLoad: boolean;
    validateUniqueReferenceLoadIds: boolean;
    identUserSetup: boolean;
    allowManualLoadCreation: boolean;
    inNetworkFlatFee: number;
    inNetworkPercentFee: number;
    outNetworkFlatFee: number;
    outNetworkPercentFee: number;
    inNetworkFeeAdd: boolean;
    outNetworkFeeAdd: boolean;
    requireMarginalAnalysis: boolean;
}

export const defaultCustomerProfile: CustomerProfile = {
    customerId: null,
    name: null,
    defaultCommodity: null,
    useFuelRerating: false,
    fuelReratingNumberOfDays: null,
    allowZeroFuel: false,
    allowEditingFuel: false,
    topsOwnerId: null,
    successManagerUserId: null,
    successSpecialistUserId: null,
    comments: null,
    dataSource: 'LOADSHOP',
    customerLoadTypeId: null,
    customerLoadTypeExpirationDate: null,
    customerContacts: [],
    customerCarrierScacs: [],
    autoPostLoad: false,
    validateUniqueReferenceLoadIds: false,
    identUserSetup: false,
    allowManualLoadCreation: false,
    inNetworkFlatFee: null,
    inNetworkPercentFee: null,
    outNetworkFlatFee: null,
    outNetworkPercentFee: null,
    inNetworkFeeAdd: null,
    outNetworkFeeAdd: null,
    requireMarginalAnalysis: false,
};
