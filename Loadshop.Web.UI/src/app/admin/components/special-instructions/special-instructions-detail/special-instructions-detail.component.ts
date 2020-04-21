import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { TreeNode } from 'primeng/api';
import { groupBy } from 'src/app/shared/utilities';
import { createPlace } from 'src/app/shared/utilities/create-place';
import {
  Equipment,
  Place,
  SpecialInstruction,
  SpecialInstructionData,
  State,
  ValidationProblemDetails,
} from '../../../../shared/models';
import { SpecialInstructionEquipmentData } from '../../../../shared/models/special-instruction-equipment-data';

@Component({
  selector: 'kbxl-special-instructions-detail',
  templateUrl: './special-instructions-detail.component.html',
  styleUrls: ['./special-instructions-detail.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpecialInstructionsDetailComponent {
  @Output() updated: EventEmitter<SpecialInstructionData> = new EventEmitter<SpecialInstructionData>();
  @Output() delete: EventEmitter<SpecialInstructionData> = new EventEmitter<SpecialInstructionData>();
  @Input() processing: boolean;
  @Input() error: ValidationProblemDetails;

  @Input() set specialInstruction(value: SpecialInstructionData) {
    this._specialInstructions = value;
    this.updateDetails();
    this.updateSelectedEquipment();
  }
  @Input() set groupedEquipment(value: TreeNode[]) {
    this._groupedEquipment = value;
    this.updateSelectedEquipment();
  }
  @Input() set states(value: State[]) {
    this._states = value;
    this.updateDetails();
  }

  get specialInstruction(): SpecialInstructionData {
    return this._specialInstructions;
  }
  get groupedEquipment(): TreeNode[] {
    return this._groupedEquipment;
  }
  get states(): State[] {
    return this._states;
  }

  private _specialInstructions: SpecialInstructionData;
  private _groupedEquipment: TreeNode[];
  private _states: State[];

  selectedCarriers: string[];
  selectedEquipment: TreeNode[];
  origin: Place;
  destination: Place;
  allOrigins = true;
  allDestinations = true;

  get adding(): boolean {
    return this.specialInstruction && !(this.specialInstruction.specialInstructionId > 0);
  }

  constructor() {}

  updateDetails() {
    this.updateSelectedEquipment();
    this.updateOrigin();
    this.updateDestination();
  }

  private updateSelectedEquipment() {
    if (this._specialInstructions && this._specialInstructions.specialInstructionEquipment && this._groupedEquipment) {
      const flattenTreeNodes = this._groupedEquipment.map(_ => _.children).reduce((acc, value) => acc.concat(value));

      const equipmentTreeNodes = this._specialInstructions.specialInstructionEquipment.map(currentEquipment => {
        const equipment = flattenTreeNodes.find(_ => _.data.equipmentId === currentEquipment.equipmentId);

        if (equipment && equipment.parent) {
          equipment.parent.partialSelected = true;
        }
        return equipment;
      });

      const groupedSelections = groupBy(x => x.data.categoryId, equipmentTreeNodes);

      groupedSelections.forEach(group => {
        // get the first item's parent as they should all be the same;
        const treeViewGroup = group.items[0].parent;

        if (treeViewGroup && group.items.length === treeViewGroup.children.length) {
          treeViewGroup.partialSelected = false;
          equipmentTreeNodes.push(treeViewGroup);
        }
      });

      this.selectedEquipment = equipmentTreeNodes;
    } else {
      this.selectedEquipment = [];
    }
  }

  private updateOrigin() {
    this.origin =
      this.specialInstruction && this.states
        ? this.createPlace(
            this.specialInstruction.originAddress1,
            this.specialInstruction.originCity,
            this.specialInstruction.originState,
            this.specialInstruction.originPostalCode,
            this.specialInstruction.originCountry
          )
        : null;

    this.originChanged(this.specialInstruction === null
      || (this.specialInstruction.originAddress1 === null
      && this.specialInstruction.originCity === null
      && this.specialInstruction.originState === null
      && this.specialInstruction.originPostalCode === null
      && this.specialInstruction.originCountry === null));
  }

  private updateDestination() {
    this.destination =
      this.specialInstruction && this.states
        ? this.createPlace(
            this.specialInstruction.destinationAddress1,
            this.specialInstruction.destinationCity,
            this.specialInstruction.destinationState,
            this.specialInstruction.destinationPostalCode,
            this.specialInstruction.destinationCountry
          )
        : null;

    this.destinationChanged( this.specialInstruction === null
      || (this.specialInstruction.destinationAddress1 === null
      && this.specialInstruction.destinationCity === null
      && this.specialInstruction.destinationState === null
      && this.specialInstruction.destinationPostalCode === null
      && this.specialInstruction.destinationCountry === null));
  }

  private createPlace(address: string, city: string, stateAbbreviation: string, postalCode: string, country: string): Place {
    if (!city && !stateAbbreviation && !country) {
      return null;
    }

    const state = stateAbbreviation && this.states ? this.states.find(_ => _.abbreviation === stateAbbreviation) : null;
    return createPlace(address, city, state ? state.name : null, stateAbbreviation, postalCode, country);
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

  updateClick() {
    const group = Object.assign(
      {},
      this.specialInstruction,
      new SpecialInstruction({
        originAddress1: this.origin ? this.origin.address : null,
        originCity: this.origin ? this.origin.city : null,
        originState: this.origin ? this.origin.stateAbbrev : null,
        originPostalCode: this.origin ? this.origin.postalCode : null,
        originCountry: this.origin ? this.origin.country : null,
        destinationAddress1: this.destination ? this.destination.address : null,
        destinationCity: this.destination ? this.destination.city : null,
        destinationState: this.destination ? this.destination.stateAbbrev : null,
        destinationPostalCode: this.destination ? this.destination.postalCode : null,
        destinationCountry: this.destination ? this.destination.country : null,
        specialInstructionEquipment: this.buildSelectedEquipment(),
      })
    );
    this.updated.emit(group);
  }

  private buildSelectedEquipment() {
    const currentEquipment = this.specialInstruction.specialInstructionEquipment;
    const newEquipment: SpecialInstructionEquipmentData[] = new Array<SpecialInstructionEquipmentData>();
    const selectedEquipment = this.selectedEquipment.filter(_ => _.leaf).map(_ => _.data as Equipment);

    selectedEquipment.forEach(equipment => {
      const result = currentEquipment.find(ce => ce.equipmentId === equipment.equipmentId);
      if (result) {
        newEquipment.push(result);
      } else {
        newEquipment.push({
          equipmentId: equipment.equipmentId,
          specialInstructionId: this._specialInstructions.specialInstructionId,
        });
      }
    });
    return newEquipment;
  }

  deleteClick() {
    const group = Object.assign({}, this.specialInstruction);
    this.delete.emit(group);
  }

  decodeProblemDetails() {
    if (!this.error || !this.error.errors) {
      return;
    }

    const groupErrors = this.error.errors['urn:SpecialInstruction'];
    if (groupErrors && Array.isArray(groupErrors)) {
      return groupErrors.join('\n');
    } else if (groupErrors && typeof groupErrors === 'string') {
      return groupErrors;
    }
    return;
  }

  public originChanged(allSelected: boolean) {
    this.allOrigins = allSelected;

    if (allSelected === true) {
      this.origin = null;
    }
  }

  public destinationChanged(allSelected: boolean) {
    this.allDestinations = allSelected;

    if (allSelected === true) {
      this.destination = null;
    }
  }
}
