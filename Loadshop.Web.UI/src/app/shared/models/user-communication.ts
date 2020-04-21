
export interface UserCommunication {
    userCommunicationId: string;
    title: string;
    message: string;
    ownerId: string;
    effectiveDate: any;
    expirationDate: any;
    allUsers: boolean;
    acknowledgementRequired: boolean;

    acknowledgementCount: number;
}
