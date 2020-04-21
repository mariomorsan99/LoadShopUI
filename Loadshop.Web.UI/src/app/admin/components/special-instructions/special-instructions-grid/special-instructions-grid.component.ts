import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { Equipment, SpecialInstruction } from '../../../../shared/models';
import { SpecialInstructionData } from '../../../../shared/models/special-instruction-data';
import { groupBy } from '../../../../shared/utilities';

@Component({
  selector: 'kbxl-special-instructions-grid',
  templateUrl: './special-instructions-grid.component.html',
  styleUrls: ['./special-instructions-grid.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpecialInstructionsGridComponent {
  first = 0;

  @Input() loading: boolean;
  @Input() instructions: SpecialInstructionData[];
  @Input() equipment: Equipment[];
  @Output() displayDetail = new EventEmitter<SpecialInstructionData>();
  @Output() copyCarriers = new EventEmitter<number>();

  addClick() {
    this.displayDetail.emit(null);
  }

  onRowSelect(group: SpecialInstructionData) {
    this.displayDetail.emit(group);
  }

  getEquipmentCategoryDisplay(instruction: SpecialInstruction) {
    if (this.equipment) {
      const equipment = this.equipment.filter(
        x => instruction.specialInstructionEquipment.map(lcg => lcg.equipmentId).indexOf(x.equipmentId) !== -1
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
