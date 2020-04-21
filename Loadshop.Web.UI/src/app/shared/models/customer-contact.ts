export interface CustomerContact {
    customerContactId: string;
    customerId: string;
    firstName: string;
    lastName: string;
    position: string;
    phoneNumber: string;
    email: string;
}

export const defaultCustomerContact: CustomerContact = {
    customerContactId: null,
    customerId: null,
    firstName: null,
    lastName: null,
    position: null,
    phoneNumber: null,
    email: null
};
