import { UserCommunication } from './user-communication';
import { UserCommunciationUser } from './user-communication-user';
import { UserCommunicationCarrier } from './user-communication-carrier';
import { UserCommunicationShipper } from './user-communication-shipper';
import { UserCommunicationSecurityAccessRole } from './user-communication-security-access-role';


export interface UserCommunicationDetail extends UserCommunication {
    userCommunicationUsers: UserCommunciationUser[];
    userCommunicationCarriers: UserCommunicationCarrier[];
    userCommunicationShippers: UserCommunicationShipper[];
    userCommunicationSecurityAccessRoles: UserCommunicationSecurityAccessRole[];
}

export const defualtUserCommunicationDetail: UserCommunicationDetail = {
    userCommunicationId: null,
    title: null,
    message: null,
    ownerId: null,
    effectiveDate: null,
    expirationDate: null,
    allUsers: false,
    acknowledgementRequired: false,

    acknowledgementCount: 0,

    userCommunicationUsers: [],
    userCommunicationCarriers: [],
    userCommunicationShippers: [],
    userCommunicationSecurityAccessRoles: [],
};
