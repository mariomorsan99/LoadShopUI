import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormArray, FormBuilder, FormGroup } from '@angular/forms';
import { ConfirmationService } from 'primeng/api';
import { GoogleMapService } from 'src/app/core/services';
import { ShippingService } from 'src/app/shipping/services';
import {
  AppointmentSchedulerConfirmationType,
  CustomerLocation,
  defaultPickupStop,
  Place,
  PlaceTypeEnum,
  State,
  UnitOfMeasure,
} from '../../models';
import { StopTypes } from '../../models/stop-types';

@Component({
  selector: 'kbxl-load-stop-form',
  templateUrl: './load-stop-form.component.html',
  styleUrls: ['./load-stop-form.component.scss'],
  changeDetection: ChangeDetectionStrategy.Default,
})
export class LoadStopFormComponent implements OnChanges {
  @Input() form: FormGroup;
  @Input() index: number;
  @Input() includeDelete: boolean;
  @Input() schedulerConfirmationTypes: AppointmentSchedulerConfirmationType[];
  @Input() loadingSchedulerConfirmationTypes: boolean;
  @Input() states: State[];
  @Input() loadingStates: boolean;
  @Input() unitsOfMeasure: UnitOfMeasure[];
  @Input() loadingUnitsOfMeasure: boolean;
  @Input() pickupStopNumbers: { stopNbr: number }[];

  @Output() addLoadStop = new EventEmitter<number>();
  @Output() changeLoadStopType = new EventEmitter<number>();
  @Output() deleteLoadStop = new EventEmitter<number>();

  suggestions: Place[];
  locationSuggestions: CustomerLocation[];
  displayAddressLine2 = false;
  displayAddressLine3 = false;
  StopTypes = StopTypes; // Enable use of global enum in template by duplicating it as a property of this component

  constructor(
    private fb: FormBuilder,
    private confirmationService: ConfirmationService,
    private googleService: GoogleMapService,
    private shippingService: ShippingService
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes) {
      if (changes.form) {
        const address2 = this.form.controls['address2'].value;
        if (address2 && address2.length > 0) {
          this.displayAddressLine2 = true;
        }
        const address3 = this.form.controls['address3'].value;
        if (address3 && address3.length > 0) {
          this.displayAddressLine3 = true;
        }
      }
    }
  }

  get contacts() {
    return this.form.controls['contacts'] as FormArray;
  }

  get stopNbr() {
    return this.form.controls['stopNbr'].value;
  }

  get stopType() {
    return this.form.controls['stopType'].value;
  }

  get city() {
    return this.form.controls['city'].value;
  }

  get state() {
    return this.form.controls['state'].value;
  }

  get stopTypeTitle() {
    let title = `${this.stopNbr}. ${this.stopType} Stop`;
    if (this.city && this.city.length > 0 && this.state && this.state.abbreviation) {
      title += ` (${this.city}, ${this.state.abbreviation})`;
    }

    return title;
  }

  get countries() {
    return [
      { id: 'USA', name: 'USA' },
      { id: 'Canada', name: 'Canada' },
    ];
  }

  get lineItems() {
    return this.form.controls['lineItems'] as FormArray;
  }

  addContact() {
    this.contacts.push(
      this.fb.group({
        firstName: '',
        lastName: '',
        position: '',
        phoneNumber: '',
        email: '',
      })
    );
  }

  deleteContact(index: number) {
    this.contacts.removeAt(index);
  }

  addLineItem() {
    this.lineItems.push(
      this.fb.group({
        loadLineItemNumber: null,
        quantity: null,
        unitOfMeasure: null,
        weight: null,
        customerPurchaseOrder: null,
        pickupStopNumber: defaultPickupStop.stopNbr,
        deliveryStopNumber: this.stopNbr,
      })
    );
  }

  addStop() {
    this.addLoadStop.emit(this.index);
  }

  changeStopType() {
    this.changeLoadStopType.emit(this.index);
  }

  deleteLineItem(index: number) {
    this.lineItems.removeAt(index);
  }

  deleteStop() {
    this.confirmationService.confirm({
      message: `Are you sure you want to remove this stop?<br/> ${this.stopTypeTitle}`,
      accept: () => {
        this.deleteLoadStop.emit(this.stopNbr);
      },
    });
  }

  addAddressLine() {
    if (!this.displayAddressLine2) {
      this.displayAddressLine2 = true;
    } else if (!this.displayAddressLine3) {
      this.displayAddressLine3 = true;
    }
  }

  citySearch(event: any) {
    this.googleService.addressAutoComplete(event.query).then(x => {
      this.suggestions = x.map(y => {
        if (y) {
          if (!y.stateAbbrev) {
            y.stateAbbrev = this.getStateAbbrev(y.state);
          }
          if (!y.state) {
            y.state = this.getState(y.stateAbbrev);
          }
        }

        return y;
      });
    });
  }

  populateAddress(place: Place) {
    this.populatePlace(place).then(x => {
      this.form.patchValue({ address1: x.address });
      this.form.patchValue({ city: x.city });
      this.form.patchValue({ state: { abbreviation: x.stateAbbrev, name: x.state } });
      this.form.patchValue({ postalCode: x.postalCode });
      this.form.patchValue({ country: { name: x.country } });
      this.form.patchValue({ latitude: x.latitude });
      this.form.patchValue({ longitude: x.longitude });
    });
  }

  private populatePlace(place: Place): Promise<Place> {
    return this.googleService.geocodePlace(place.description).then(x => {
      if (place.placeType === PlaceTypeEnum.Establistment) {
        place.address = x.address;
        place.postalCode = x.postalCode;
        place.placeType = PlaceTypeEnum.Address;
      }

      if (place.placeType === PlaceTypeEnum.Route) {
        if (x.address) {
          place.address = x.address;
          place.placeType = PlaceTypeEnum.Address;
        } else {
          place.address = null;
          place.placeType = PlaceTypeEnum.City;
        }
      } else if (x.postalCode) {
        place.postalCode = x.postalCode;
      }

      if (x.latitude) {
        place.latitude = x.latitude;
      }
      if (x.longitude) {
        place.longitude = x.longitude;
      }

      return place;
    });
  }

  getStateAbbrev(stateName: string): string {
    if (!stateName) {
      return '';
    }
    if (!this.states) {
      return stateName;
    }
    const state = this.states.find(x => x.name.toLowerCase() === stateName.toLowerCase());
    return state ? state.abbreviation : '';
  }

  getState(stateAbbrev: string): string {
    if (!stateAbbrev) {
      return '';
    }
    if (!this.states) {
      return stateAbbrev;
    }
    const state = this.states.find(x => x.abbreviation.toLowerCase() === stateAbbrev.toLowerCase());
    return state ? state.name : '';
  }

  locationSearch(event: any) {
    this.shippingService
      .searchLocations(event.query)
      .toPromise()
      .then(x => {
        this.locationSuggestions = x;
      });
  }

  populateAddressFromLocation(location: CustomerLocation) {
    if (location) {
      this.form.patchValue({ locationId: location.locationId });
      this.form.patchValue({ locationName: location.locationName });
      this.form.patchValue({ address1: location.address1 });
      this.form.patchValue({ address2: location.address2 });
      this.form.patchValue({ address3: location.address3 });
      this.form.patchValue({ city: location.city });
      this.form.patchValue({ state: { abbreviation: location.state, name: this.getState(location.state) } });
      this.form.patchValue({ postalCode: location.postalCode });
      this.form.patchValue({ country: { name: location.country } });
      this.form.patchValue({ latitude: location.latitude });
      this.form.patchValue({ longitude: location.longitude });
      this.form.patchValue({ schedulerConfirmationType: location.appointmentSchedulerConfirmationTypeId });

      this.form.patchValue({ addressAutoComplete: { address: location.address1 } });
    }
  }

  populateToField() {
    const earlyDate = this.form.controls['earlyDate'].value;
    const earlyTime = this.form.controls['earlyTime'].value;

    const lateDate = this.form.controls['lateDate'].value;
    const lateTime = this.form.controls['lateTime'].value;

    if (earlyDate && !lateDate) {
      this.form.controls['lateDate'].setValue(earlyDate);
    }

    if (earlyTime && !lateTime) {
      this.form.controls['lateTime'].setValue(earlyTime);
    }
  }

  updateLateDtTm(): void {
    if (
      this.form.value.lateDate &&
      this.form.value.lateDate.toString().length > 0 &&
      this.form.value.lateTime &&
      this.form.value.lateTime.length > 0
    ) {
      this.form.patchValue({
        lateDtTm: '', // this is because we only set this value on the server; however we need both values to validate the control
      });
    }
  }
}
