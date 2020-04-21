/* eslint-disable @typescript-eslint/camelcase */
import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { AuthLogoutAction } from '@tms-ng/core';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { CoreState } from 'src/app/core/store';
import { GuidEmpty } from '../../../core/utilities/constants';
import {
  AllMessageTypes,
  Commodity,
  ContactNumberMessageTypes,
  LoadStatusNotificationsData,
  User,
  UserNotification,
  UserNotificationModel,
  ValidationProblemDetails,
} from '../../../shared/models';
import { contactPhoneRequiredValidator } from '../../validators/user-profile-validators';

@Component({
  selector: 'kbxl-user-profile',
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.scss'],
  // changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserProfileComponent implements OnInit, OnDestroy, OnChanges {
  @Input() user: User;
  @Input() commodities: Commodity[];
  @Input() loadingCommodities: boolean;
  @Input() loading: boolean;
  @Input() error: ValidationProblemDetails;
  @Input() triggerValidation: Subject<boolean>;
  @Input() loadStatusNotifications: LoadStatusNotificationsData;
  @Input() loadStatusNotificationsErrors: ValidationProblemDetails;
  @Input() savedLoadStatusNotifications: boolean;
  @Output() updated: EventEmitter<User> = new EventEmitter<User>();
  @Output() saveLoadStatusNotifications: EventEmitter<LoadStatusNotificationsData> = new EventEmitter<LoadStatusNotificationsData>();
  messageTypes = [];

  userProfileForm: FormGroup;
  allowSelectingScac = false;
  submitted = false;
  destroyed$ = new Subject<boolean>();
  showManageLoadNotifications: false;

  get contactNumbers() {
    const c = this.userProfileForm.get('contactNumbers') as FormArray;
    return c.controls;
  }
  constructor(private store: Store<CoreState>, private fb: FormBuilder, private messageService: MessageService) {
    this.createForm();
  }

  ngOnDestroy(): void {
    this.destroyed$.next(true);
    this.destroyed$.complete();
  }
  ngOnChanges(changes: SimpleChanges) {
    if (changes && changes.user && changes.user.currentValue) {
      const currentValue = changes.user.currentValue;
      this.allowSelectingScac =
        !currentValue.carrierScac && currentValue.availableCarrierScacsSelectItems && currentValue.availableCarrierScacsSelectItems.length;

      if (currentValue.defaultCommodity && this.commodities) {
        const commodity = this.commodities.find(x => x.commodityName === currentValue.defaultCommodity);
        if (commodity) {
          this.user.selectedCommodity = commodity;
        }
      }

      this.submitted = false;
      this.updateForm();
    }

    if (changes && changes.savedLoadStatusNotifications) {
      if (this.savedLoadStatusNotifications) {
        this.showManageLoadNotifications = false;
      }
    }
  }
  ngOnInit(): void {
    this.triggerValidation.pipe(takeUntil(this.destroyed$)).subscribe(x => {
      if (x) {
        this.submitted = true;
        this.userProfileForm.markAsTouched();
        this.userProfileForm.markAsDirty();
        this.validateAllFormFields(this.userProfileForm);

        if (!this.userProfileForm.valid) {
          this.messageService.add({
            summary: 'Error updating User Profile',
            detail: 'One or more errors occurred when updating the User Profile.  See form for error details',
            severity: 'error',
          });
          return;
        }
      }
    });
  }

  createForm(): void {
    // build message type dropdown
    ContactNumberMessageTypes.forEach(x => {
      this.messageTypes.push({
        label: x.description,
        value: x.value,
      });
    });

    // create form
    this.userProfileForm = new FormGroup(
      {
        notificationEnabled: new FormControl(true),
        email: new FormControl('', [Validators.required, Validators.maxLength(256), Validators.email]),
        carrierScac: new FormControl(''),
        contactNumbers: this.fb.array([]),
        selectedCommodity: new FormControl(''),
      },
      { validators: contactPhoneRequiredValidator }
    );
  }

  updateForm(): void {
    const items = this.userProfileForm.get('contactNumbers') as FormArray;

    this.userProfileForm.patchValue({
      email: this.getEmailNotification(),
      carrierScac: this.user.carrierScac,
      notificationEnabled: this.user.isNotificationsEnabled,
      selectedCommodity: this.user.selectedCommodity,
    });

    // we need to clear the array without losing the validations
    const phoneNotifications = this.user.userNotifications.filter(
      x => x.messageTypeId !== AllMessageTypes.Email && x.userNotificationId !== GuidEmpty
    );

    for (let index = 0; index < phoneNotifications.length; index++) {
      const element = phoneNotifications[index];

      const exists = items.value.find(x => x.userNotificationId === element.userNotificationId);
      if (!exists) {
        items.push(this.createContactItem(element));
      }
    }

    if (phoneNotifications.length === 0 && items.value.length === 0) {
      this.addContact();
    }
  }

  removeContact(index: number): void {
    const items = this.userProfileForm.get('contactNumbers') as FormArray;
    items.removeAt(index);
    if (items.value.length === 0) {
      this.addContact();
    }
  }

  addContact(): void {
    this.submitted = false;
    const items = this.userProfileForm.get('contactNumbers') as FormArray;
    items.push(this.createContactItem());
  }

  createContactItem(notification?: UserNotification): FormGroup {
    if (!notification) {
      notification = new UserNotificationModel({
        userNotificationId: GuidEmpty,
        messageTypeId: AllMessageTypes.Phone,
        notificationValue: '',
        additionalValue: '',
        isDefault: false,
        notificationEnabled: this.userProfileForm.value.notificationEnabled,
      });
    } else {
      // check if we need to split the extension out
      if (notification.notificationValue.indexOf('x') > -1) {
        const split = notification.notificationValue.split('x');
        notification.notificationValue = split[0];
        notification.additionalValue = split[1];
      }
    }

    return this.fb.group({
      userNotificationId: notification.userNotificationId,
      messageTypeId: notification.messageTypeId,
      notificationValue: notification.notificationValue,
      additionalValue: notification.additionalValue,
      description: notification.description,
      notificationEnabled: notification.notificationEnabled,
    });
  }

  update() {
    this.updated.emit(this.user);
  }

  logout() {
    this.store.dispatch(new AuthLogoutAction());
  }

  onSubmit(): void {
    this.submitted = true;
    this.validateAllFormFields(this.userProfileForm);

    if (!this.userProfileForm.valid) {
      this.messageService.add({
        summary: 'Error updating User Profile',
        detail: 'One or more errors occurred when updating the User Profile.  See form for error details',
        severity: 'error',
      });
      return;
    }
    // pull the contact numbers from the form and save to the user object before emitting output
    const formModel = this.userProfileForm.value;

    const emailNotification = this.user.userNotifications.find(x => x.messageTypeId === AllMessageTypes.Email);

    if (emailNotification) {
      emailNotification.notificationValue = formModel.email as string;
    }

    const contactNumbers = formModel.contactNumbers as UserNotificationModel[];

    // format the extension to append to the notification value
    contactNumbers.forEach(x => {
      if (x.messageTypeId === AllMessageTypes.Phone && x.additionalValue && x.additionalValue.length > 0) {
        x.notificationValue += `x${x.additionalValue}`;
        x.additionalValue = null;
      }
    });

    // replace contact numbers and email
    this.user.userNotifications = [];
    this.user.userNotifications.push(...contactNumbers);
    this.user.userNotifications.push(emailNotification);

    this.user.defaultCommodity = null;
    if (formModel.selectedCommodity) {
      this.user.defaultCommodity = formModel.selectedCommodity.commodityName;
    }
    this.user.carrierScac = formModel.carrierScac;

    this.user.isNotificationsEnabled = formModel.notificationEnabled;

    this.update();
  }

  getEmailNotification(): string {
    const notification = this.user.userNotifications.find(x => x.messageTypeId === AllMessageTypes.Email);

    if (notification) {
      return notification.notificationValue;
    }
    return '';
  }

  decodeProblemDetails() {
    if (!this.error || !this.error.errors) {
      return;
    }

    const groupErrors = this.error.errors['urn:UserProfile'];
    if (groupErrors && Array.isArray(groupErrors)) {
      return groupErrors.join('\n');
    } else if (groupErrors && typeof groupErrors === 'string') {
      return groupErrors;
    }
    return;
  }
  resetCommodity(): void {
    this.userProfileForm.patchValue({
      selectedCommodity: '',
    });
  }

  onSaveLoadStatusNotifications(payload: LoadStatusNotificationsData): void {
    this.saveLoadStatusNotifications.emit(payload);
  }

  /*
   ** TODO when upgrade to angular 8, replace with this.userProfileForm.markAllAsTouched();
   */
  private validateAllFormFields(formGroup: FormGroup) {
    Object.keys(formGroup.controls).forEach(field => {
      formGroup.markAsTouched();
      const control = formGroup.get(field);
      if (control instanceof FormControl) {
        control.markAsTouched({ onlySelf: true });
        control.updateValueAndValidity();
      } else if (control instanceof FormGroup) {
        this.validateAllFormFields(control);
      } else if (control instanceof FormArray) {
        control.controls.forEach(x => {
          if (x instanceof FormGroup) {
            this.validateAllFormFields(x);
          }
        });
      }
    });
  }
}
