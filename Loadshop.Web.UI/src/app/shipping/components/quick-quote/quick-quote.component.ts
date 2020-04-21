import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import {
  Place,
  State,
  PlaceTypeEnum,
  Equipment,
  SmartSpotQuoteRequest,
  ValidationProblemDetails,
  SmartSpotQuoteCreateRequest
} from 'src/app/shared/models';
import { GoogleMapService } from 'src/app/core/services';
import { TreeNode } from 'primeng/api';
import { SmartSpotBrandMarkup } from 'src/app/core/utilities/constants';

@Component({
  selector: 'kbxl-quick-quote',
  templateUrl: './quick-quote.component.html',
  styleUrls: ['./quick-quote.component.scss']
})
export class QuickQuoteComponent {
  @Input() states: State[];
  @Input() groupedEquipment: TreeNode[];
  @Input() loading: boolean;
  @Input() smartSpotQuote: number;
  @Input() set smartSpotErrors(value: ValidationProblemDetails) {
    this.setErrors(value ? value.errors || {} : {});
  }
  @Input() allowManualLoadCreation: boolean;

  @Output() createOrder = new EventEmitter<SmartSpotQuoteCreateRequest>();
  @Output() getQuote = new EventEmitter<SmartSpotQuoteRequest>();
  @Output() clear = new EventEmitter<object>();

  public formGroup: FormGroup;
  public suggestions: Place[];
  public selectedEquipment: Equipment;
  public errorSummary: string;
  public errorCount: number;
  private equipment: Equipment;

  private errorMap = [
    { urn: '', formControlName: '' },
    { urn: 'EquipmentId', formControlName: 'equipmentId' },
    { urn: 'Weight', formControlName: 'weight' },
    { urn: 'PickupDate', formControlName: 'pickupDate' },
    { urn: 'OriginPostalCode', formControlName: 'origin' },
    { urn: 'OriginState', formControlName: 'origin' },
    { urn: 'DestinationPostalCode', formControlName: 'destination' },
    { urn: 'DestinationState', formControlName: 'destination' },
  ];

  smartSpotLabel = SmartSpotBrandMarkup;

  constructor(private fb: FormBuilder, private googleService: GoogleMapService) {
    this.formGroup = this.fb.group({
      origin: null,
      destination: null,
      equipmentId: null,
      weight: null,
      pickupDate: null
    });

    this.formGroup.valueChanges.subscribe(_ => this.clear.emit(null));
  }

  public postalCodeSearch(event) {
    this.googleService.autoComplete(event.query).then(x => {
      this.suggestions = x.filter(_ => _.placeType === PlaceTypeEnum.PostalCode).map(y => {
        if (y) {
          const state = this.getState(y.state, y.stateAbbrev);
          if (state != null) {
            y.stateAbbrev = state.abbreviation || y.stateAbbrev;
            y.state = state.name || y.state;
          }
        }

        return y;
      });
    });
  }

  private getState(stateName: string, stateAbbrev: string) {
    // lookup the state attempting both name and abbreviation comparisons against
    // each other as some zip code searches like 34329 return the full state
    // name in the stateAbbrev property
    const state = this.states.find(x =>
      x.name.toLowerCase() === (stateName || stateAbbrev).toLowerCase()
      || x.abbreviation.toLowerCase() === (stateAbbrev || stateName).toLowerCase()
    );

    return state;
  }

  equipmentSelectionMade(node: TreeNode) {
    this.equipment = node && node.data ? node.data as Equipment : null;
    this.formGroup.patchValue({
      equipmentId: this.equipment ? this.equipment.equipmentId : null,
    });
  }

  createOrderClick() {
    const values = this.formGroup.value;
    this.createOrder.emit({
      origin: values ? values.origin : null,
      destination: values ? values.destination : null,
      equipment: this.equipment,
      weight: values ? values.weight : null,
      pickupDate: values ? values.pickupDate : null,
    });
  }

  clearClick() {
    this.formGroup.reset();
    this.selectedEquipment = null;
    this.clear.emit(null);
  }

  private getRequest(): SmartSpotQuoteRequest {
    const values = this.formGroup.value;
    const origin = values ? values.origin as Place : null;
    const destination = values ? values.destination as Place : null;

    return {
      originCity: origin ? origin.city : null,
      originState: origin ? origin.stateAbbrev : null,
      originPostalCode: origin ? origin.postalCode : null,
      originCountry: origin ? origin.country : null,
      destinationCity: destination ? destination.city : null,
      destinationState: destination ? destination.stateAbbrev : null,
      destinationPostalCode: destination ? destination.postalCode : null,
      destinationCountry: destination ? destination.country : null,
      equipmentId: values ? values.equipmentId : null,
      weight: values ? values.weight : null,
      pickupDate: values ? values.pickupDate : null
    };
  }

  getQuoteClicked() {
    this.getQuote.emit(this.getRequest());
  }

  private setErrors(errors: object) {
    const urnRoot = 'urn:root';

    const messages = this.setFormGroupErrors(this.formGroup, urnRoot, this.errorMap, errors);
    this.errorSummary = messages ? messages.join('\n') : '';
    this.errorCount = messages ? messages.length : 0;
  }

  private setFormGroupErrors(
    formObject: FormGroup,
    urnPrefix: string, errorMap: { urn: string; formControlName: string }[], errors
  ): string[] {
    const errorList: string[] = [];
    formObject.setErrors(null);
    Object.keys(formObject.controls).forEach(key => {
      formObject.controls[key].setErrors(null);
    });

    for (const entry of errorMap) {
      const currentUrn = urnPrefix + (entry.urn && entry.urn.length ? ':' + entry.urn : '');
      const name = entry.formControlName;
      const controlErrors = errors ? errors[currentUrn] : null;

      // Track the full list of errors
      if (controlErrors) {
        for (const error of controlErrors) {
          if (error) {
            errorList.push(error.trim());
          }
        }

        if (name != null) {
          if (name.length === 0) {
            formObject.setErrors(controlErrors);
          } else {
            formObject.get(name).setErrors(controlErrors);
          }
        }
      }
    }
    return errorList;
  }
}
