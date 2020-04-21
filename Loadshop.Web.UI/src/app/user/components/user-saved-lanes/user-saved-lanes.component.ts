import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { defaultLane, Equipment, State, UserLane } from '../../../shared/models';
import { deepCopyLane, groupBy } from '../../../shared/utilities';

@Component({
  selector: 'kbxl-user-saved-lanes',
  templateUrl: './user-saved-lanes.component.html',
  styleUrls: ['./user-saved-lanes.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserSavedLanesComponent implements OnChanges {
  first = 0;
  @Input() loading: boolean;
  @Input() lanes: UserLane[];
  @Input() states: State[];
  @Input() equipment: Equipment[];
  @Output() displayDetail = new EventEmitter<UserLane>();
  ngOnChanges(changes: SimpleChanges) {
    if (changes) {
      if (changes.lanes) {
        this.first = 0;
        this.populateLaneEquipmentDisplays();
      } else if (changes.equipment) {
        this.populateLaneEquipmentDisplays();
      }
    }
  }

  populateLaneEquipmentDisplays() {
    if (this.equipment && this.lanes) {
      this.lanes.map(x => {
        if (x.equipmentIds && x.equipmentIds.length === 1) {
          const equipment = this.equipment.find(e => e.equipmentId === x.equipmentIds[0]);
          if (equipment && equipment.categoryId) {
            x.equipmentDisplay = equipment.categoryId + '/' + equipment.categoryEquipmentDesc || equipment.equipmentDesc;
          } else {
            x.equipmentDisplay = equipment.equipmentDesc;
          }
        }
      });
    }
  }

  getEquipmentCategoryDisplay(lane: UserLane) {
    const equipment = this.equipment.filter(x => lane.equipmentIds.indexOf(x.equipmentId) !== -1);
    const equipmentGroups = groupBy(x => x.categoryId, equipment);

    if (equipmentGroups.length < 4) {
      return equipmentGroups.map(group => group.key).join(', ');
    }

    return equipmentGroups.length;
  }

  addClick() {
    const editLane = deepCopyLane(defaultLane);
    editLane.origDH = 50;
    editLane.destDH = 50;

    this.displayDetail.emit(editLane);
  }

  onRowSelect(lane: UserLane) {
    const editLane = deepCopyLane(lane);
    this.displayDetail.emit(editLane);
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

  onPage($event) {
    if ($event && $event.first) {
      this.first = $event.first;
    }
  }
}
