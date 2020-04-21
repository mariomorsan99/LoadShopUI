import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { AllMessageTypes, LoadStatusNotificationsData, User, ValidationProblemDetails } from '../../../shared/models';

@Component({
  selector: 'kbxl-user-load-notifications',
  templateUrl: './user-load-notifications.component.html',
  styleUrls: ['./user-load-notifications.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserLoadNotificationsComponent implements OnChanges {
  @Input() user: User;
  @Input() loadStatusNotifications: LoadStatusNotificationsData;
  @Input() error: ValidationProblemDetails;
  @Output() saveLoadStatusNotifications: EventEmitter<LoadStatusNotificationsData> = new EventEmitter<LoadStatusNotificationsData>();
  loadNotificationForm: FormGroup;
  constructor() {
    this.createForm();
  }
  ngOnChanges(changes: SimpleChanges): void {
    if (changes && ((changes.loadStatusNotifications && changes.loadStatusNotifications.currentValue) || changes.user)) {
      // check if a number / email was provided, else use their profile info
      this.setDefaults();

      if (this.loadStatusNotifications) {
        this.updateForm();
      }
    }
  }

  createForm(): void {
    this.loadNotificationForm = new FormGroup({
      textMessageEnabled: new FormControl(false),
      textMessageNumber: new FormControl('', [Validators.required, Validators.maxLength(10)]),
      emailEnabled: new FormControl(false),
      email: new FormControl('', [Validators.required, Validators.maxLength(256), Validators.email]),
      departedEnabled: new FormControl(false),
      arrivedEnabled: new FormControl(false),
      deliveredEnabled: new FormControl(false),
    });
  }

  updateForm(): void {
    this.loadNotificationForm.patchValue({
      textMessageEnabled: this.loadStatusNotifications.textMessageEnabled,
      textMessageNumber: this.loadStatusNotifications.textMessageNumber,
      emailEnabled: this.loadStatusNotifications.emailEnabled,
      email: this.loadStatusNotifications.email,
      departedEnabled: this.loadStatusNotifications.departedEnabled,
      arrivedEnabled: this.loadStatusNotifications.arrivedEnabled,
      deliveredEnabled: this.loadStatusNotifications.deliveredEnabled,
    });
  }

  setDefaults(): void {
    if (
      this.user &&
      this.loadStatusNotifications &&
      (!this.loadStatusNotifications.email || this.loadStatusNotifications.email.length === 0)
    ) {
      this.loadStatusNotifications.email = this.user.userNotifications.find(
        x => x.messageTypeId === AllMessageTypes.Email
      ).notificationValue;
    }

    if (
      this.user &&
      this.loadStatusNotifications &&
      (!this.loadStatusNotifications.textMessageNumber || this.loadStatusNotifications.textMessageNumber.length === 0)
    ) {
      this.loadStatusNotifications.textMessageNumber = this.user.userNotifications.find(
        x => x.messageTypeId === AllMessageTypes.CellPhone
      ).notificationValue;
    }
  }
  onSubmit(): void {
    const formModel = this.loadNotificationForm.value;

    const payload: LoadStatusNotificationsData = {
      textMessageEnabled: formModel.textMessageEnabled as boolean,
      textMessageNumber: formModel.textMessageNumber as string,
      emailEnabled: formModel.emailEnabled as boolean,
      email: formModel.email as string,
      departedEnabled: formModel.departedEnabled as boolean,
      arrivedEnabled: formModel.arrivedEnabled as boolean,
      deliveredEnabled: formModel.deliveredEnabled as boolean,
    };
    this.saveLoadStatusNotifications.emit(payload);
  }

  decodeProblemDetails() {
    if (!this.error || !this.error.errors) {
      return;
    }

    const groupErrors = this.error.errors['urn:Visibility'];
    if (groupErrors && Array.isArray(groupErrors)) {
      return groupErrors.join('\n');
    } else if (groupErrors && typeof groupErrors === 'string') {
      return groupErrors;
    }
    return;
  }
}
