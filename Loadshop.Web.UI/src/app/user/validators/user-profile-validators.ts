import { FormArray, ValidatorFn } from '@angular/forms';
import { AllMessageTypes, UserNotificationModel } from '../../shared/models';

export const contactPhoneRequiredValidator: ValidatorFn = (control: FormArray): { [key: string]: any } | null => {
  if (!control.touched || !control.dirty) {
    return;
  }

  const contactNumbers = control.value.contactNumbers as UserNotificationModel[];

  if (!contactNumbers || contactNumbers.length === 0) {
    control.value.contactNumbers.valid = false;
    return {
      missingContactNumbers: true,
    };
  }
  const phoneNotifications = contactNumbers.filter(
    x => x.messageTypeId && x.messageTypeId !== AllMessageTypes.Email
  );

  if (!phoneNotifications || phoneNotifications.some(x => !x.notificationValue || x.notificationValue.length === 0)) {
    control.value.contactNumbers.valid = false;
    return {
      noPhoneContacts: true,
    };
  }
  return null;
};
