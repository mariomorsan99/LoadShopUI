import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { groupBy } from 'src/app/shared/utilities';
import { Equipment, LoadCarrierGroup, State } from '../../../../shared/models';

@Component({
  selector: 'kbxl-load-carrier-group-grid',
  templateUrl: './load-carrier-group-grid.component.html',
  styleUrls: ['./load-carrier-group-grid.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadCarrierGroupGridComponent {
  first = 0;

  @Input() loading: boolean;
  @Input() groups: LoadCarrierGroup[];
  @Input() states: State[];
  @Input() equipment: Equipment[];
  @Output() displayDetail = new EventEmitter<LoadCarrierGroup>();
  @Output() copyCarriers = new EventEmitter<number>();

  addClick() {
    this.displayDetail.emit(null);
  }

  onRowSelect(group: LoadCarrierGroup) {
    this.displayDetail.emit(group);
  }

  copyCarriersToNewGroup(group: LoadCarrierGroup, event: Event) {
    event.stopPropagation();
    this.copyCarriers.emit(group.loadCarrierGroupId);
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

  getEquipmentCategoryDisplay(loadCarrierGroup: LoadCarrierGroup) {
    if (this.equipment) {
      const equipment = this.equipment.filter(
        x => loadCarrierGroup.loadCarrierGroupEquipment.map(lcg => lcg.equipmentId).indexOf(x.equipmentId) !== -1
      );
      const equipmentGroups = groupBy(x => x.categoryId, equipment);

      if (equipmentGroups.length < 4) {
        return equipmentGroups.map(group => group.key).join(', ');
      }

      return equipmentGroups.length;
    }
    return null;
  }

  onPage($event) {
    if ($event && $event.first) {
      this.first = $event.first;
    }
  }
}
