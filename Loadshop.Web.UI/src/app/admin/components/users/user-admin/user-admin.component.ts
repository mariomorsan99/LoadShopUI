/* eslint-disable @typescript-eslint/camelcase */
import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { MessageService, SelectItem } from 'primeng/api';
import { environment } from '../../../../../environments/environment';
import { GuidEmpty } from '../../../../core/utilities/constants';
import {
  AllMessageTypes,
  ContactNumberMessageTypes,
  UserAdminData,
  UserNotification,
  UserNotificationModel,
} from '../../../../shared/models';
import { contactPhoneRequiredValidator } from '../../../../user/validators/user-profile-validators';

@Component({
  selector: 'kbxl-user-admin',
  templateUrl: './user-admin.component.html',
  styleUrls: ['./user-admin.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserAdminComponent implements OnChanges {
  @Output() userSearchFilterUpdated = new EventEmitter<string>();
  @Output() userSelected = new EventEmitter<UserAdminData>();
  @Output() carrierChanged = new EventEmitter();
  @Output() userSaved = new EventEmitter<{ user: UserAdminData; createMode: boolean }>();
  @Output() addUser = new EventEmitter<string>();
  @Output() carrierSelected = new EventEmitter<string>();

  @Input() public selectedUser: UserAdminData;
  @Input() public userResults: UserAdminData[];
  @Input() public updatingUser: UserAdminData;
  @Input() public allAuthorizedShippers: SelectItem[];
  @Input() public allAuthorizedSecurityRoles: SelectItem[];
  @Input() public allAuthorizedCarrierScacs: SelectItem[];
  @Input() public processing: boolean;
  @Input() public createMode: boolean;
  @Input() public loadingCarriers: boolean;
  @Input() public carriers: SelectItem[];
  @Input() public loadingCarrierUsers: boolean;
  @Input() public carrierUsers: UserAdminData[];

  registrationUrl: string;
  messageTypes = [];
  userProfileForm: FormGroup;
  submitted = false;
  public newUser: string;
  selectedCarrier: string;

  get contactNumbers() {
    const c = this.userProfileForm.get('contactNumbers') as FormArray;
    return c.controls;
  }
  constructor(private fb: FormBuilder, private messageService: MessageService) {
    this.registrationUrl = `${environment.identityServerUrl}/account/Register`;
    this.createForm();
  }
  ngOnChanges(changes: SimpleChanges) {
    if (changes && changes.updatingUser && changes.updatingUser.currentValue) {
      this.submitted = false;
      this.updateForm();
    }
  }

  searchUsers(event) {
    this.userSearchFilterUpdated.emit(event.query);
  }

  select() {
    this.userSelected.emit(this.selectedUser);
    this.selectedUser = null;
    this.selectedCarrier = null;
  }

  onCarrierSelected() {
    this.carrierSelected.emit(this.selectedCarrier);
  }

  carrierScacsChanged() {
    this.carrierChanged.emit();
  }

  saveUser() {
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

    const dedicatedPlanner = this.allAuthorizedSecurityRoles.find(x => x.label === 'Dedicated Planner');
    const selectedRoles = this.userProfileForm.value.securityRoleIds as string[];
    if (dedicatedPlanner && selectedRoles.find(x => dedicatedPlanner.value === x) && selectedRoles.length > 1) {
      this.messageService.add({
        summary: 'Error updating User Profile',
        detail: 'Yaint be using multiple roles wit "Dedicated Planner" be selected. Brian, Please provide new msg... this one is bad.',
        severity: 'error',
      });
      return;
    }
    const formModel = this.userProfileForm.value;

    this.updatingUser.carrierScacs = formModel.carrierScacs as string[];
    this.updatingUser.securityRoleIds = formModel.securityRoleIds as string[];
    this.updatingUser.shipperIds = formModel.shipperIds as string[];
    this.updatingUser.isNotificationsEnabled = formModel.isNotificationsEnabled as boolean;

    const contactNumbers = formModel.contactNumbers as UserNotificationModel[];
    const emailNotification = this.updatingUser.userNotifications.find(
      x => x.messageTypeId === AllMessageTypes.Email
    );
    // format the extension to append to the notification value
    contactNumbers.forEach(x => {
      if (x.messageTypeId === AllMessageTypes.Phone && x.additionalValue && x.additionalValue.length > 0) {
        x.notificationValue += `x${x.additionalValue}`;
        x.additionalValue = null;
      }
    });

    // replace contact numbers and email
    this.updatingUser.userNotifications = [];
    this.updatingUser.userNotifications.push(...contactNumbers);
    if (emailNotification) {
      this.updatingUser.userNotifications.push(emailNotification);
    }
     this.userSaved.emit({ user: this.updatingUser, createMode: this.createMode });
  }

  add(newUserpanel) {
    newUserpanel.hide();
    this.addUser.emit(this.newUser);
    this.newUser = '';
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
        securityRoleIds: new FormControl([]),
        shipperIds: new FormControl([]),
        carrierScacs: new FormControl([]),
        isNotificationsEnabled: new FormControl(false),
        contactNumbers: this.fb.array([]),
      },
      { validators: contactPhoneRequiredValidator }
    );
  }

  updateForm(): void {
    // reset form to wipe values if selected user changed
    this.userProfileForm.reset();
    const items = this.userProfileForm.get('contactNumbers') as FormArray;
    items.reset();
    // clear the form array
    for (let index = items.length; index > -1; index--) {
      items.removeAt(index);
    }

    this.userProfileForm.patchValue({
      securityRoleIds: this.updatingUser.securityRoleIds,
      shipperIds: this.updatingUser.shipperIds,
      carrierScacs: this.updatingUser.carrierScacs,
      isNotificationsEnabled: this.updatingUser.isNotificationsEnabled,
    });

    // we need to clear the array without losing the validations
    const phoneNotifications = this.updatingUser.userNotifications.filter(
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
        notificationEnabled: false,
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

  public displayCarrierScacs(): boolean {
    // Dont display carrier scacs for dedicated planners
    const dedicatedPlanner = this.allAuthorizedSecurityRoles.find(x => x.label === 'Dedicated Planner');
    const outboundDedicated = this.allAuthorizedSecurityRoles.find(x => x.label === 'KBX Outbound/Dedicated');
    const selectedRoles = this.userProfileForm.value.securityRoleIds as string[];
    if ((dedicatedPlanner && selectedRoles.find(x => dedicatedPlanner.value === x)) ||
        (outboundDedicated && selectedRoles.find(x => outboundDedicated.value === x))) {
      return false;
    }
    return this.allAuthorizedCarrierScacs.length > 0;
  }

  public displayShippers(): boolean {
        // Dont display shippers for dedicated planners
        const dedicatedPlanner = this.allAuthorizedSecurityRoles.find(x => x.label === 'Dedicated Planner');
        const selectedRoles = this.userProfileForm.value.securityRoleIds as string[];
        if (dedicatedPlanner && selectedRoles.find(x => dedicatedPlanner.value === x)) {
          return false;
        }
        return this.allAuthorizedShippers.length > 0;
  }
}
