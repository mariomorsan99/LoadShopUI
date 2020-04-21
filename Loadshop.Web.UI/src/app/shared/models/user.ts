import { SelectItem } from 'primeng/api';
import { Customer, ISecurityAccessRoleData } from '.';
import { Commodity } from './commodity';
import { SecurityAppActionType } from './security-app-action-type';
import { UserFocusEntity } from './user-focus-entity';
import { UserNotification } from './user-notification';

export interface User {
  userId: string;
  name: string;
  carrierName: string;
  carrierScac: string;
  isNotificationsEnabled: boolean;
  userNotifications: UserNotification[];
  availableCarrierScacs: string[];
  availableCarrierScacsSelectItems: SelectItem[];
  carrierVisibilityTypes: string[];
  isCarrier: boolean;
  isShipper: boolean;
  isAdmin: boolean;
  securityAccessRoles: ISecurityAccessRoleData[];
  authorizedShippers: Customer[];
  authorizedShippersForMyPrimaryScac: Customer[];
  myCustomerContractedScacs: string[];
  focusEntity: UserFocusEntity;
  primaryCustomerId: string;
  canSetDefaultCommodity: boolean;
  selectedCommodity: Commodity;
  defaultCommodity: string;
  hasAgreedToTerms: boolean;
  allowManualLoadCreation: boolean;
}

export class UserModel implements User {
  constructor(init?: Partial<UserModel>) {
    Object.assign(this, init);
  }

  userId: string;
  name: string;
  carrierName: string;
  carrierScac: string;
  isNotificationsEnabled: boolean;
  userNotifications: UserNotification[];
  availableCarrierScacs: string[];
  availableCarrierScacsSelectItems: SelectItem[];
  carrierVisibilityTypes: string[];
  isCarrier: boolean;
  isShipper: boolean;
  isAdmin: boolean;
  securityAccessRoles: ISecurityAccessRoleData[];
  authorizedShippers: Customer[];
  authorizedShippersForMyPrimaryScac: Customer[];
  myCustomerContractedScacs: string[];
  focusEntity: UserFocusEntity;
  primaryCustomerId: string;
  canSetDefaultCommodity: boolean;
  selectedCommodity: Commodity;
  defaultCommodity: string;
  hasAgreedToTerms: boolean;
  allowManualLoadCreation: boolean;

  hasSecurityAction(actionType: SecurityAppActionType): boolean {
    return this.securityAccessRoles
      ? this.securityAccessRoles.some(r => r.appActions && r.appActions.some(a => a.appActionId === actionType))
      : false;
  }
}
