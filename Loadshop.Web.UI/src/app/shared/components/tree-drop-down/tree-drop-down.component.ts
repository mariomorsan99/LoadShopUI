import { Component, ElementRef, EventEmitter, Input, Output } from '@angular/core';
import { TreeNode } from 'primeng/api';

@Component({
  selector: 'kbxl-tree-drop-down',
  templateUrl: './tree-drop-down.component.html',
  styleUrls: ['./tree-drop-down.component.scss'],
  // tslint:disable-next-line:use-host-property-decorator
  host: {
    '(document:click)': 'onClick($event)',
  },
})
export class TreeDropDownComponent {
  @Input() treeNodes: TreeNode[];
  @Output() selectedNodesChange = new EventEmitter<TreeNode[]>();
  @Output() selectedNodeChange = new EventEmitter<TreeNode>();
  @Input() selectedNodes: TreeNode[];
  @Input() selectedNode: TreeNode;
  @Input() placeHolder: string;
  @Input() singleSelection = false;

  show = false;

  constructor(private _el: ElementRef) {}

  hidePlaceholder(): boolean {
    if (this.singleSelection) {
      return this.selectedNode != null;
    } else {
      return this.selectedNodes ? this.selectedNodes.length > 0 : false;
    }
  }

  toggle() {
    this.show = !this.show;
  }

  getSelectedNodesDisplay(): string {
    if (this.singleSelection) {
      if (this.selectedNode) {
        return this.selectedNode.label;
      }
    } else {
      if (this.selectedNodes && this.selectedNodes.length < 3) {
        return this.selectedNodes.map(node => node.label).join(', ');
      } else if (this.selectedNodes) {
        return this.selectedNodes.filter(x => x.leaf).length + ' items selected';
      }
    }
  }

  onClick(event) {
    if (!this._el.nativeElement.contains(event.target)) {
      // similar checks
      this.show = false;
    }
  }

  selectionChanged() {
    if (this.singleSelection) {
      this.selectedNodeChange.emit(this.selectedNode);
      this.show = false;
    } else {
      this.selectedNodesChange.emit(this.selectedNodes);
    }
  }
}
