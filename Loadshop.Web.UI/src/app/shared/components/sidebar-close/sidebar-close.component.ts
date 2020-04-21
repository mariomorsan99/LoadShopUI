import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'kbxl-sidebar-close',
  templateUrl: './sidebar-close.component.html',
  styleUrls: ['./sidebar-close.component.scss']
})
export class SidebarCloseComponent {
  @Output() closed: EventEmitter<boolean> = new EventEmitter<boolean>();

  close() {
    this.closed.emit(true);
  }
}
