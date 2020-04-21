import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { select, Store } from '@ngrx/store';
import { Observable, Subject } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';
import { CoreState, getEquipment } from 'src/app/core/store';
import { Equipment, LoadCarrierGroup } from 'src/app/shared/models';
import { getUserProfileEntity, UserState } from 'src/app/user/store';
import {
  AdminState,
  getLoadCarrierGroups,
  getLoadingLoadCarrierGroups,
  LoadCarrierGroupCopyCarriersAction,
  LoadCarrierGroupLoadAction,
} from '../../../store';

@Component({
  selector: 'kbxl-carrier-group-container',
  templateUrl: './load-carrier-group-container.component.html',
  styleUrls: ['./load-carrier-group-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadCarrierGroupContainerComponent implements OnInit, OnDestroy {
  groups$: Observable<LoadCarrierGroup[]>;
  loading$: Observable<boolean>;
  equipment$: Observable<Equipment[]>;
  displayGrid = true;

  destroyed$ = new Subject<boolean>();

  constructor(
    private adminStore: Store<AdminState>,
    private userStore: Store<UserState>,
    private coreStore: Store<CoreState>,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    this.userStore.pipe(select(getUserProfileEntity), takeUntil(this.destroyed$)).subscribe(user => {
      if (user && user.primaryCustomerId) {
        this.adminStore.dispatch(new LoadCarrierGroupLoadAction({ customerId: user.primaryCustomerId }));
      }
    });

    this.groups$ = this.adminStore.pipe(
      select(getLoadCarrierGroups),
      map(_ => _.map(g => new LoadCarrierGroup(g)))
    );

    this.equipment$ = this.coreStore.pipe(map(getEquipment));

    this.loading$ = this.coreStore.pipe(map(getLoadingLoadCarrierGroups));
  }

  ngOnDestroy() {
    this.destroyed$.next(true);
    this.destroyed$.complete();
  }

  displayDetail(group: LoadCarrierGroup) {
    const path: [string | number] = ['maint/carrier-groups/detail'];
    if (group && group.loadCarrierGroupId > 0) {
      path.push(group.loadCarrierGroupId);
    }
    this.router.navigate(path);
  }

  displayCarriers(group: LoadCarrierGroup) {
    this.router.navigate(['carriers', group.loadCarrierGroupId], { relativeTo: this.route });
  }

  copyCarriers(loadCarrierGroupId: number): void {
    this.adminStore.dispatch(new LoadCarrierGroupCopyCarriersAction({ loadCarrierGroupId: loadCarrierGroupId }));
    this.router.navigate(['maint/carrier-groups/detail']);
  }
}
