export enum AllMessageTypes {
  Email = 'Email',
  CellPhone = 'Cell_Phone',
  Phone = 'Phone',
}
// These match the API Constants casing
export type MessageType = 'Email' | 'Cell_Phone';
export type NotificationMessageTypes = {
  [P in MessageType]: boolean;
};

export const ContactNumberMessageTypes = [
  {
    description: 'Cell Phone',
    value: AllMessageTypes.CellPhone,
  },
  {
    description: 'Phone',
    value: AllMessageTypes.Phone,
  },
];
