import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { TreeNode } from 'primeng/api';
import { Equipment, State, CustomerLoadType } from 'src/app/shared/models';
import { ShippingLoadFilter } from 'src/app/shared/models/shipping-load-filter';
import { groupBy } from 'src/app/shared/utilities';

@Component({
  selector: 'kbxl-shipping-load-filter',
  templateUrl: './shipping-load-filter.component.html',
  styleUrls: ['./shipping-load-filter.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShippingLoadFilterComponent implements OnChanges {
  @Input() filterCriteria: ShippingLoadFilter;
  @Input() visible: boolean;
  @Input() equipment: TreeNode[];
  @Input() customerLoadTypes: CustomerLoadType[];
  @Input() states: State[];
  @Output() filterCriteriaChange: EventEmitter<ShippingLoadFilter> = new EventEmitter<ShippingLoadFilter>();
  @Output() visibleChange: EventEmitter<boolean> = new EventEmitter<boolean>();

  selectedEquipment: TreeNode[];

  localFilterCriteria: ShippingLoadFilter;

  constructor() {}

  ngOnChanges() {
    const stringCopy = JSON.stringify(this.filterCriteria);
    this.localFilterCriteria = JSON.parse(stringCopy);

    this.updateSelectedEquipment();
  }

  clearFilter(prop: any) {
    this.localFilterCriteria[prop] = null;
  }

  clear() {
    this.selectedEquipment = [];
    this.localFilterCriteria = new ShippingLoadFilter();
  }

  search() {
    this.localFilterCriteria.equipment = this.buildEquipment();

    this.filterCriteriaChange.emit(this.localFilterCriteria);
    this.visibleChange.emit(false);
  }

  private buildEquipment(): Equipment[] {
    return this.selectedEquipment.filter(x => x.leaf).map(x => x.data as Equipment);
  }

  private updateSelectedEquipment() {
    if (this.localFilterCriteria && this.localFilterCriteria.equipment && this.equipment) {
      const flattenTreeNodes = this.equipment.map(_ => _.children).reduce((acc, value) => acc.concat(value));

      const equipmentTreeNodes = this.localFilterCriteria.equipment.map(currentEquipment => {
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
}
