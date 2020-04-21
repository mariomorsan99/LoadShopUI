export interface LoadContact {
    loadContactId: string;
    loadId: string;
    firstName: string;
    lastName: string;
    phoneNumber: string;
    email: string;
}

export const defaultLoadContact: LoadContact = {
    loadContactId: null,
    loadId: null,
    firstName: null,
    lastName: null,
    phoneNumber: null,
    email: null
};
