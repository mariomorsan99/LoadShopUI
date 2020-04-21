import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { SelectItem, TreeNode } from 'primeng/api';
import { groupBy } from 'src/app/shared/utilities';
import { createPlace } from 'src/app/shared/utilities/create-place';
import {
  Carrier,
  Equipment,
  LoadCarrierGroup,
  LoadCarrierGroupCarrierData,
  LoadCarrierGroupEquipmentData,
  LoadCarrierGroupType,
  Place,
  State,
  ValidationProblemDetails,
} from '../../../../shared/models';

@Component({
  selector: 'kbxl-load-carrier-group-detail',
  templateUrl: './load-carrier-group-detail.component.html',
  styleUrls: ['./load-carrier-group-detail.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadCarrierGroupDetailComponent {
  @Output() updated: EventEmitter<LoadCarrierGroup> = new EventEmitter<LoadCarrierGroup>();
  @Output() delete: EventEmitter<LoadCarrierGroup> = new EventEmitter<LoadCarrierGroup>();
  @Input() processing: boolean;
  @Input() error: ValidationProblemDetails;
  @Input() loadCarrierGroupTypes: LoadCarrierGroupType[];

  @Input() set group(value: LoadCarrierGroup) {
    this._group = value;
    this.updateDetails();
    this.updateSelectedEquipment();
    this.updateSelectedCarriers();
  }
  @Input() set groupedEquipment(value: TreeNode[]) {
    this._groupedEquipment = value;
    this.updateSelectedEquipment();
  }
  @Input() set states(value: State[]) {
    this._states = value;
    this.updateDetails();
  }

  @Input() set allCarriers(value: Carrier[]) {
    this._allCarriers = value;

    this.carriers = this.toSelectItem(value);
    if (value) {
      this.carrierChange();
    }
  }
  get group(): LoadCarrierGroup {
    return this._group;
  }
  get groupedEquipment(): TreeNode[] {
    return this._groupedEquipment;
  }
  get states(): State[] {
    return this._states;
  }
  get allCarriers(): Carrier[] {
    return this._allCarriers;
  }

  carriers: SelectItem[] = [];
  private _allCarriers: Carrier[] = null;
  private _group: LoadCarrierGroup;
  private _groupedEquipment: TreeNode[];
  private _states: State[];

  selectedCarriers: string[];
  selectedEquipment: TreeNode[];
  origin: Place;
  destination: Place;
  showCarrierGroupTypeWarning = false;

  groupTypes = [
    { label: 'Include Carriers', value: 'Include Carriers' },
    { label: 'Exclude Carriers', value: 'Exclude Carriers' },
  ];
  groupType = this.groupTypes[0];

  get adding(): boolean {
    return this.group && !(this.group.loadCarrierGroupId > 0);
  }

  constructor() {}

  updateDetails() {
    // this.updateSelectedEquipment();
    this.updateOrigin();
    this.updateDestination();
  }

  private updateSelectedEquipment() {
    if (this._group && this._group.loadCarrierGroupEquipment && this._groupedEquipment) {
      const flattenTreeNodes = this._groupedEquipment.map(_ => _.children).reduce((acc, value) => acc.concat(value));

      const equipmentTreeNodes = this._group.loadCarrierGroupEquipment.map(currentEquipment => {
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
      this.group && this.states
        ? this.createPlace(
            this.group.originAddress1,
            this.group.originCity,
            this.group.originState,
            this.group.originPostalCode,
            this.group.originCountry
          )
        : null;
  }

  private updateDestination() {
    this.destination =
      this.group && this.states
        ? this.createPlace(
            this.group.destinationAddress1,
            this.group.destinationCity,
            this.group.destinationState,
            this.group.destinationPostalCode,
            this.group.destinationCountry
          )
        : null;
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
      this.group,
      new LoadCarrierGroup({
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
        loadCarrierGroupEquipment: this.buildSelectedEquipment(),
        carriers: this.buildSelectedCarriers(),
      })
    );
    this.updated.emit(group);
  }

  private buildSelectedEquipment() {
    const currentEquipment = this.group.loadCarrierGroupEquipment;
    const newEquipment: LoadCarrierGroupEquipmentData[] = new Array<LoadCarrierGroupEquipmentData>();
    const selectedEquipment = this.selectedEquipment.filter(_ => _.leaf).map(_ => _.data as Equipment);

    selectedEquipment.forEach(equipment => {
      const result = currentEquipment.find(ce => ce.equipmentId === equipment.equipmentId);
      if (result) {
        newEquipment.push(result);
      } else {
        newEquipment.push({ equipmentId: equipment.equipmentId, loadCarrierGroupId: this.group.loadCarrierGroupId });
      }
    });
    return newEquipment;
  }

  private buildSelectedCarriers(): LoadCarrierGroupCarrierData[] {
    const carriers: LoadCarrierGroupCarrierData[] = [];
    this.selectedCarriers.forEach(selection => {
      const exists = this.group.carriers.find(x => x.carrierId === selection);
      if (exists) {
        // if the carriers where copied, ensure they don't have null values
        carriers.push(exists);
      } else {
        carriers.push({
          loadCarrierGroupCarrierId: 0,
          loadCarrierGroupId: this.group.loadCarrierGroupId,
          carrierId: selection,
        });
      }
    });

    return carriers;
  }

  deleteClick() {
    const group = Object.assign({}, this.group);
    this.delete.emit(group);
  }

  decodeProblemDetails() {
    if (!this.error || !this.error.errors) {
      return;
    }

    const groupErrors = this.error.errors['urn:LoadCarrierGroup'];
    if (groupErrors && Array.isArray(groupErrors)) {
      return groupErrors.join('\n');
    } else if (groupErrors && typeof groupErrors === 'string') {
      return groupErrors;
    }
    return;
  }

  carrierChange() {
    this.carriers.sort((a, b) => this.carrierSort(a, b));
  }

  carrierSort(a: SelectItem, b: SelectItem): number {
    if (this.selectedCarriers && this.selectedCarriers.length > 0) {
      const selectedA = this.selectedCarriers.indexOf(a.value) >= 0;
      const selectedB = this.selectedCarriers.indexOf(b.value) >= 0;

      if (selectedA !== selectedB) {
        return selectedA ? -1 : 1;
      }
    }

    return a.label.localeCompare(b.label);
  }

  private updateSelectedCarriers(): void {
    if (!this.group || !this.group.carriers) {
      this.selectedCarriers = [];
    } else {
      this.selectedCarriers = this.group.carriers.map(x => x.carrierId);
      this.carrierChange();
    }
  }
  private toSelectItem(groups: Carrier[]): SelectItem[] {
    if (!groups) {
      return [];
    }
    const selectItems: SelectItem[] = [];
    groups.forEach(x => {
      selectItems.push({
        label: x.carrierName,
        value: x.carrierId,
      });
    });

    return selectItems.sort();
  }

  warnGroupTypeChange() {
    if (this._group.loadCarrierGroupId) {
      this.showCarrierGroupTypeWarning = true;
    }
  }
}
