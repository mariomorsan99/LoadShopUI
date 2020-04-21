export interface LoadStatusNotificationsData {
  textMessageEnabled: boolean;
  textMessageNumber: string;
  emailEnabled: boolean;
  email: string;
  departedEnabled: boolean;
  arrivedEnabled: boolean;
  deliveredEnabled: boolean;
}

export const defaultLoadNotification: LoadStatusNotificationsData = {
  textMessageEnabled: false,
  textMessageNumber: '',
  emailEnabled: false,
  email: '',
  departedEnabled: false,
  arrivedEnabled: false,
  deliveredEnabled: false,
};
