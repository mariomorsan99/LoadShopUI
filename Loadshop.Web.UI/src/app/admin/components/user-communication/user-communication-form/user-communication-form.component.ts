import { ChangeDetectionStrategy, Component, Input, OnChanges, SimpleChanges, Output, EventEmitter } from '@angular/core';
import { UserCommunicationDetail, Customer, Carrier, UserAdminData, ISecurityAccessRoleData } from 'src/app/shared/models';
import { FormGroup, FormBuilder, FormControl, ValidatorFn, ValidationErrors, Validators } from '@angular/forms';
import { ConfirmationService } from 'primeng/api';

@Component({
  selector: 'kbxl-user-communication-form',
  templateUrl: './user-communication-form.component.html',
  styleUrls: ['./user-communication-form.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserCommunicationFormComponent implements OnChanges {
  @Input() processing: boolean;
  @Input() userCommunication: UserCommunicationDetail;
  @Input() allCarriers: Carrier[];
  @Input() allShippers: Customer[];
  @Input() allUsers: UserAdminData[];
  @Input() allSecurityRoles: ISecurityAccessRoleData[];
  @Input() createMode: boolean;
  @Output() userCommunicationUpdate = new EventEmitter<UserCommunicationDetail>();
  @Output() userCommunicationCreate = new EventEmitter<UserCommunicationDetail>();
  @Output() userCommunicationCancel = new EventEmitter();

  userCommunicationForm: FormGroup;
  showTargetUsersSection = true;

  constructor(private formBuilder: FormBuilder, private confirmationService: ConfirmationService) {
    // Just for for init design
    this.allCarriers = [];
    this.allShippers = [];
    this.allUsers = [];
    this.allSecurityRoles = [];

    this.userCommunicationForm = this.formBuilder.group(
      {
        title: new FormControl(null, Validators.required),
        message: new FormControl(null, Validators.required),
        effectiveDate: new FormControl(null, Validators.required),
        expirationDate: new FormControl(null),
        allUsers: new FormControl(false),
        // acknowledgementRequired: new FormControl(null),
        userCommunicationShippers: new FormControl([]),
        userCommunicationCarriers: new FormControl([]),
        userCommunicationUsers: new FormControl([]),
        userCommunicationSecurityAccessRoles: new FormControl([]),
      },
      { validators: this.targetAtLeastOneUserValdiator() }
    );
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.userCommunication && this.userCommunication) {
      this.updateUserCommuncationForm();
    }
  }

  updateUserCommuncationForm() {
    this.userCommunicationForm.reset();

    this.userCommunicationForm.patchValue({
      userCommunicationId: this.userCommunication.userCommunicationId,
      title: this.userCommunication.title,
      message: this.userCommunication.message,
      effectiveDate: this.userCommunication.effectiveDate,
      expirationDate: this.userCommunication.expirationDate,
      allUsers: this.userCommunication.allUsers,
      // acknowledgementRequired: this.userCommunication.acknowledgementRequired,
      userCommunicationShippers: this.userCommunication.userCommunicationShippers.map(c => c.customerId),
      userCommunicationCarriers: this.userCommunication.userCommunicationCarriers.map(c => c.carrierId),
      userCommunicationUsers: this.userCommunication.userCommunicationUsers.map(c => c.userId),
      userCommunicationSecurityAccessRoles: this.userCommunication.userCommunicationSecurityAccessRoles.map(c => c.accessRoleId),
    });

    this.setTargetUsersVisibility();
  }

  onSubmit() {
    if (this.userCommunicationForm.valid) {
      const formModel = this.userCommunicationForm.value;

      this.userCommunication.title = formModel.title;
      this.userCommunication.message = formModel.message;
      this.userCommunication.effectiveDate = formModel.effectiveDate;
      this.userCommunication.expirationDate = formModel.expirationDate;
      this.userCommunication.allUsers = formModel.allUsers;
      this.userCommunication.acknowledgementRequired = formModel.acknowledgementRequired;

      if (!this.userCommunication.allUsers) {
        this.userCommunication.userCommunicationShippers = this.mapArrayFromKeys(
          this.userCommunication.userCommunicationShippers,
          formModel.userCommunicationShippers,
          shipper => shipper.customerId,
          shipperId => ({ userCommunicationId: this.userCommunication.userCommunicationId, customerId: shipperId })
        );
        this.userCommunication.userCommunicationCarriers = this.mapArrayFromKeys(
          this.userCommunication.userCommunicationCarriers,
          formModel.userCommunicationCarriers,
          carrier => carrier.carrierId,
          carrierId => ({ userCommunicationId: this.userCommunication.userCommunicationId, carrierId: carrierId })
        );
        this.userCommunication.userCommunicationUsers = this.mapArrayFromKeys(
          this.userCommunication.userCommunicationUsers,
          formModel.userCommunicationUsers,
          carrier => carrier.userId,
          userId => ({ userCommunicationId: this.userCommunication.userCommunicationId, userId: userId })
        );
        this.userCommunication.userCommunicationSecurityAccessRoles = this.mapArrayFromKeys(
          this.userCommunication.userCommunicationSecurityAccessRoles,
          formModel.userCommunicationSecurityAccessRoles,
          securityAccessRole => securityAccessRole.accessRoleId,
          accessRoleId => ({ userCommunicationId: this.userCommunication.userCommunicationId, accessRoleId: accessRoleId })
        );
      } else {
        this.userCommunication.userCommunicationShippers = [];
        this.userCommunication.userCommunicationCarriers = [];
        this.userCommunication.userCommunicationSecurityAccessRoles = [];
        this.userCommunication.userCommunicationSecurityAccessRoles = [];
      }

      if (this.createMode) {
        this.userCommunicationCreate.emit(this.userCommunication);
      } else {
        this.userCommunicationUpdate.emit(this.userCommunication);
      }
    }
  }

  mapArrayFromKeys<T, TKey>(currentsItems: T[], selectedKeys: TKey[], keySelector: (item: T) => TKey, buildNewT: (key: TKey) => T): T[] {
    const newSelectedItems = new Array<T>();

    selectedKeys.forEach(key => {
      const currentItem = currentsItems.find(item => keySelector(item) === key);

      if (currentItem) {
        newSelectedItems.push(currentItem);
      } else {
        newSelectedItems.push(buildNewT(key));
      }
    });

    return newSelectedItems;
  }

  setTargetUsersVisibility() {
    const allUsersValue = this.userCommunicationForm.value.allUsers;

    this.showTargetUsersSection = !allUsersValue;
  }

  cancel() {
    this.confirmationService.confirm({
      message: `Are you sure you want to cancel? You will lose your changes to User Communication -
                        ${this.userCommunicationForm.value.title}.`,
      accept: () => {
        this.updateUserCommuncationForm();
        this.userCommunicationCancel.emit();
      },
    });
  }

  public targetAtLeastOneUserValdiator(): ValidatorFn {
    return (userCommunicationFormGroup: FormGroup): ValidationErrors => {
      if (userCommunicationFormGroup.dirty || userCommunicationFormGroup.touched) {
        const formModel = userCommunicationFormGroup.value;

        const userCommunicationCarriersControl = userCommunicationFormGroup.controls['userCommunicationCarriers'];
        const userCommunicationShippersControl = userCommunicationFormGroup.controls['userCommunicationShippers'];
        const userCommunicationUsersControl = userCommunicationFormGroup.controls['userCommunicationUsers'];
        const userCommunicationSecurityAccessRolesControl = userCommunicationFormGroup.controls['userCommunicationSecurityAccessRoles'];
        const userCommunicationAllUsersControl = userCommunicationFormGroup.controls['allUsers'];

        if (
          userCommunicationCarriersControl.dirty ||
          userCommunicationCarriersControl.touched ||
          userCommunicationShippersControl.dirty ||
          userCommunicationShippersControl.touched ||
          userCommunicationUsersControl.dirty ||
          userCommunicationUsersControl.touched ||
          userCommunicationSecurityAccessRolesControl.dirty ||
          userCommunicationSecurityAccessRolesControl.touched ||
          userCommunicationAllUsersControl.dirty ||
          userCommunicationAllUsersControl.touched
        ) {
          if (
            !(
              formModel.userCommunicationCarriers.length > 0 ||
              formModel.userCommunicationShippers.length > 0 ||
              formModel.userCommunicationUsers.length > 0 ||
              formModel.userCommunicationSecurityAccessRoles.length > 0 ||
              formModel.allUsers
            )
          ) {
            return { noTargetusers: true };
          }
        }
      }
      return;
    };
  }

  hasErrors(field: string): boolean {
    const formControl = this.userCommunicationForm.get(field);
    if (formControl && (formControl.touched || formControl.dirty)) {
      return formControl.errors !== null;
    }

    return false;
  }

  clearExpirationDateClick() {
    this.userCommunicationForm.patchValue({ expirationDate: null });
  }

  messageChange() {
    this.userCommunicationForm.updateValueAndValidity();
  }
}
