import { UserNotification } from './user-notification';
import { ISecurityAccessRoleData } from '.';

export interface UserAdminData {
  userId: string;
  identUserId: string;
  isNotificationsEnabled: boolean;
  username: string;
  firstName: string;
  lastName: string;
  primaryScac: string;
  primaryCustomerId: string;
  email: string;
  companyName: string;
  securityRoleIds: string[];
  securityRoles: ISecurityAccessRoleData[];
  shipperIds: string[];
  carrierScacs: string[];
  userNotifications: UserNotification[];
}

export const defaultUserAdminData: UserAdminData = {
  userId: null,
  identUserId: null,
  isNotificationsEnabled: true,
  username: null,
  firstName: null,
  lastName: null,
  primaryScac: null,
  primaryCustomerId: null,
  email: null,
  companyName: null,
  securityRoleIds: [],
  securityRoles: [],
  shipperIds: [],
  carrierScacs: [],
  userNotifications: [],
};

export function isUserAdminData(x: any): x is UserAdminData {
  return typeof x.userId === 'string' && typeof x.firstName === 'string' && typeof x.lastName === 'string';
}

export function isUserAdminDataArray(x: any): x is UserAdminData[] {
  return x.every(isUserAdminData);
}
