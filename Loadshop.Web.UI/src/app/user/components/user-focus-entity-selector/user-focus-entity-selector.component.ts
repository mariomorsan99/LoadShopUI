import { Component, EventEmitter, Input, Output } from '@angular/core';
import { SelectItemGroup } from 'primeng/api';
import { UserFocusEntity } from '../../../shared/models';

@Component({
  selector: 'kbxl-user-focus-entity-selector',
  templateUrl: './user-focus-entity-selector.component.html',
  styleUrls: ['./user-focus-entity-selector.component.scss'],
})
export class UserFocusEntitySelectorComponent {
  @Output() updated = new EventEmitter<UserFocusEntity>();
  @Output() searchChanged = new EventEmitter<string>();

  @Input() availableEntitiesFlat: UserFocusEntity[];
  @Input() searchResults: UserFocusEntity[];
  @Input() availableEntities: SelectItemGroup[];
  @Input() selectedEntity: UserFocusEntity;

  update() {
    this.updated.emit(this.selectedEntity);
  }

  search(event) {
    this.searchChanged.emit(event.query);
  }
}
