import { AllMessageTypes } from '.';

export interface UserNotification {
  userNotificationId: string;
  messageTypeId: AllMessageTypes;
  notificationValue: string;
  isDefault: boolean;
  description: string;
  notificationEnabled: boolean;
  // client side value used for extensions
  additionalValue: string;
}

export class UserNotificationModel implements UserNotification {
  userNotificationId: string;
  messageTypeId: AllMessageTypes;
  notificationValue: string;
  isDefault: boolean;
  description: string;
  notificationEnabled: boolean;

  // client side value used for extensions
  additionalValue: string;
  constructor(init?: Partial<UserNotification>) {
    Object.assign(this, init);
  }
}
