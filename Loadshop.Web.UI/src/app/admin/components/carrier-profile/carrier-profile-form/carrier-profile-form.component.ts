import { TitleCasePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
} from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ConfirmationService } from 'primeng/api';
import { Carrier, CarrierProfile, CarrierScac, User } from 'src/app/shared/models';

@Component({
  selector: 'kbxl-carrier-profile-form',
  templateUrl: './carrier-profile-form.component.html',
  styleUrls: ['./carrier-profile-form.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CarrierProfileFormComponent implements OnChanges {
  @Input() carriers: Carrier[];
  @Input() adminUsers: User[];
  @Input() selectedCarrierProfile: CarrierProfile;
  @Input() processing: boolean;
  @Output() selectedCarrierChange: EventEmitter<Carrier> = new EventEmitter<Carrier>();
  @Output() selectedCarrierProfileUpdate: EventEmitter<CarrierProfile> = new EventEmitter<CarrierProfile>();
  @Output() cancelUpdateClick = new EventEmitter();

  selectedCarrier: Carrier;
  carrierProfileForm: FormGroup;

  get scacs() {
    const c = this.carrierProfileForm.get('scacs') as FormArray;
    return c.controls;
  }

  constructor(
    private formBuilder: FormBuilder,
    private confirmationService: ConfirmationService,
    private changeDetectorRef: ChangeDetectorRef,
    private titleCasePipe: TitleCasePipe
  ) {
    this.carrierProfileForm = formBuilder.group({
      isLoadshopActive: new FormControl(false),
      kbxlContracted: new FormControl(false),

      carrierSuccessTeamLeadId: new FormControl(null, Validators.required),
      carrierSuccessSpecialistId: new FormControl(null, Validators.required),
      comments: new FormControl(null),

      scacs: this.formBuilder.array([]),
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes && changes.selectedCarrierProfile && changes.selectedCarrierProfile.currentValue) {
      this.updateForm();
    }
  }

  onSubmit() {
    if (this.carrierProfileForm.valid) {
      const formModel = this.carrierProfileForm.value;
      this.selectedCarrierProfile.isLoadshopActive = formModel.isLoadshopActive;
      this.selectedCarrierProfile.kbxlContracted = formModel.kbxlContracted;
      this.selectedCarrierProfile.carrierSuccessSpecialistId = formModel.carrierSuccessSpecialistId;
      this.selectedCarrierProfile.carrierSuccessTeamLeadId = formModel.carrierSuccessTeamLeadId;
      this.selectedCarrierProfile.comments = formModel.comments;

      const scacs = formModel.scacs as CarrierScac[];

      this.selectedCarrierProfile.scacs = scacs;

      this.selectedCarrierProfileUpdate.emit(this.selectedCarrierProfile);
    }
  }

  updateForm() {
    this.carrierProfileForm.reset();

    this.carrierProfileForm.patchValue({
      isLoadshopActive: this.selectedCarrierProfile.isLoadshopActive,
      kbxlContracted: this.selectedCarrierProfile.kbxlContracted,
      carrierSuccessSpecialistId: this.selectedCarrierProfile.carrierSuccessSpecialistId,
      carrierSuccessTeamLeadId: this.selectedCarrierProfile.carrierSuccessTeamLeadId,
      comments: this.selectedCarrierProfile.comments,
    });

    const scacs = this.carrierProfileForm.get('scacs') as FormArray;

    scacs.reset();

    for (let index = scacs.length; index > -1; index--) {
      scacs.removeAt(index);
    }

    this.toFormGroups(this.selectedCarrierProfile.scacs, this.formBuilder).forEach(item => scacs.push(item));
  }

  toFormGroups(items: any[], fb: FormBuilder): FormGroup[] {
    return items.map(item => this.toFormGroup(item, fb));
  }

  toFormGroup(item: any, fb: FormBuilder): FormGroup {
    const simplePropertyObj = this.toFormControl(item);
    const objects = this.getObjectsProperties(item);

    const subGroups = new Array<AbstractControl>();
    for (const subObj in objects) {
      if (Array.isArray(subObj)) {
        subGroups.push(fb.array(this.toFormGroups(subObj, fb)));
      } else {
        subGroups.push(this.toFormGroup(subObj, fb));
      }
    }

    return fb.group({ ...simplePropertyObj, ...subGroups });
  }

  toFormControl(item: any) {
    const keys = Object.getOwnPropertyNames(item);

    const onlyPrimitiveObject = keys.reduce((value, key) => {
      const itemVal = item[key];
      if (this.isPrimitive(itemVal) || itemVal instanceof Date) {
        value[key] = new FormControl(itemVal);
      }

      return value;
    }, {});

    return onlyPrimitiveObject;
  }

  getObjectsProperties(item: any): Array<any> {
    const keys = Object.getOwnPropertyNames(item);
    const objectsArray = new Array<any>();
    for (let i = 0; i < keys.length; i++) {
      const itemVal = item[keys[i]];
      if (!this.isPrimitive(itemVal) && !(itemVal instanceof Date)) {
        objectsArray.push(itemVal);
      }
    }
    return objectsArray;
  }

  isPrimitive(test) {
    return test !== Object(test);
  }

  carrierChange() {
    if (this.carrierProfileForm.dirty) {
      this.confirmationService.confirm({
        message: `Are you sure you want to select a different carrier?
                You will lose your changes to ${this.titleCasePipe.transform(this.selectedCarrierProfile.carrierName)}.`,
        accept: () => this.selectedCarrierChange.emit(this.selectedCarrier),
        reject: () => {
          this.dirtyFormRejected();
          this.changeDetectorRef.detectChanges();
        },
      });
    } else {
      this.selectedCarrierChange.emit(this.selectedCarrier);
    }
  }

  cancel() {
    this.confirmationService.confirm({
      message: `Are you sure you want to cancel? You will lose your changes to ${this.titleCasePipe.transform(
        this.selectedCarrierProfile.carrierName
      )}.`,
      accept: () => {
        this.cancelUpdateClick.emit();
        this.selectedCarrier = null;
        this.carrierProfileForm.reset();
      },
    });
  }

  dirtyFormRejected() {
    this.selectedCarrier = this.carriers.find(c => c.carrierId === this.selectedCarrierProfile.carrierId);
  }
}
