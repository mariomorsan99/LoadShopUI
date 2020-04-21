import { Component, ChangeDetectionStrategy, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { MenuItem } from 'primeng/api';
import { getAdminMenuEntities, CoreState } from 'src/app/core/store';
import { Store } from '@ngrx/store';
import { map } from 'rxjs/operators';

@Component({
  selector: 'kbxl-admin-container',
  templateUrl: './admin-container.component.html',
  styleUrls: ['./admin-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminContainerComponent implements OnInit {
  menu$: Observable<MenuItem[]>;
  activeRoute$: Observable<string>;

  constructor(private store: Store<CoreState>) { }

  ngOnInit(): void {
    this.menu$ = this.store.pipe(map(getAdminMenuEntities));
  }
}
