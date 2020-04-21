import { Pipe, PipeTransform } from '@angular/core';
import { TreeNode } from 'primeng/api';
import { Equipment, isEquipmentArray } from '../models';
import { groupBy, toTreeNode, toTreeNodes } from '../utilities';

@Pipe({ name: 'toTreeNodes' })
export class ToTreeNodesPipe implements PipeTransform {
  public transform(list: Equipment[], onlySelectLeafNodes = false): TreeNode[] {
    if (!list || !list.length) {
      return undefined;
    }

    if (isEquipmentArray(list)) {
      const equipmentGroups = groupBy(x => x.categoryId, list);

      return equipmentGroups.map(category => {
        const equipmentTreeNodes = toTreeNodes(
          category.items,
          e => e.categoryEquipmentDesc || e.equipmentDesc,
          e => e.equipmentId,
          true
        );
        const categoryTreeNode = toTreeNode(
          category,
          c => c.key || 'Other',
          c => c.key,
          onlySelectLeafNodes ? false : true,
          equipmentTreeNodes
        );

        return categoryTreeNode;
      });
    }

    return [];
  }
}
