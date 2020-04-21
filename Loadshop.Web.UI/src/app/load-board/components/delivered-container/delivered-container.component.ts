import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { select, Store } from '@ngrx/store';
import { BehaviorSubject, Observable } from 'rxjs';
import { distinctUntilChanged } from 'rxjs/operators';
import { PageableQueryData, PageableQueryHelper, PageableUiHelper } from 'src/app/shared/utilities';
import { CoreState, getBrowserIsMobile, getEquipment, getMenuVisibilityBadge, getServiceTypes, getStates } from '../../../core/store';
import { Equipment, LoadDetail, LoadView, Search, ServiceType, State, UserModel, VisibilityBadge } from '../../../shared/models';
import { getUserProfileModel } from '../../../user/store';
import {
  getLoadBoardDeliveredErrorLoadId,
  getLoadBoardDeliveredLoading,
  getLoadBoardDeliveredLoads,
  getLoadBoardDeliveredPhoneError,
  getLoadBoardDeliveredQueryHelper,
  getLoadBoardDeliveredSavingLoadId,
  getLoadBoardDeliveredTotalRecords,
  getLoadBoardDeliveredTruckError,
  getLoadBoardSelectedLoad,
  LoadBoardDeliveredLoadAction,
  LoadBoardDeliveredSaveLoadAction,
  LoadBoardDeliveredUpdateQueryAction,
  LoadBoardState,
} from '../../store';
import { LoadGridComponent } from '../load-grid';

@Component({
  templateUrl: './delivered-container.component.html',
  styleUrls: ['./delivered-container.component.scss'],
})
export class DeliveredContainerComponent implements OnInit, OnDestroy {
  loads$: Observable<LoadView[]>;
  equipment$: Observable<Equipment[]>;
  states$: Observable<State[]>;
  dashboardLoading$: Observable<boolean>;
  savingLoadId$: Observable<string>;
  errorLoadId$: Observable<string>;
  phoneError$: Observable<boolean>;
  truckError$: Observable<boolean>;
  loadDetail$: Observable<LoadDetail>;
  user$: Observable<UserModel>;
  visibilityBadge$: Observable<VisibilityBadge>;
  serviceTypes$: Observable<ServiceType[]>;
  isDisabled = false;

  // Paging
  @ViewChild(LoadGridComponent) loadGrid: LoadGridComponent;
  isMobile$: Observable<boolean>;
  pagingUiHelper: PageableUiHelper<LoadBoardDeliveredUpdateQueryAction, LoadBoardDeliveredLoadAction, LoadBoardState>;
  totalRecords$: Observable<number>;

  constructor(
    private store: Store<LoadBoardState>,
    private coreStore: Store<CoreState>,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    this.loads$ = this.store.pipe(
      select(getLoadBoardDeliveredLoads),
      distinctUntilChanged((x, y) => x.length === y.length)
    );
    this.states$ = this.coreStore.pipe(select(getStates));
    this.equipment$ = this.coreStore.pipe(select(getEquipment));
    this.loadDetail$ = this.store.pipe(select(getLoadBoardSelectedLoad));
    this.dashboardLoading$ = this.store.pipe(select(getLoadBoardDeliveredLoading));
    this.savingLoadId$ = this.store.pipe(select(getLoadBoardDeliveredSavingLoadId));
    this.errorLoadId$ = this.store.pipe(select(getLoadBoardDeliveredErrorLoadId));
    this.phoneError$ = this.store.pipe(select(getLoadBoardDeliveredPhoneError));
    this.truckError$ = this.store.pipe(select(getLoadBoardDeliveredTruckError));
    this.user$ = this.store.pipe(select(getUserProfileModel));
    this.visibilityBadge$ = this.coreStore.pipe(select(getMenuVisibilityBadge));
    this.isMobile$ = this.coreStore.pipe(select(getBrowserIsMobile));
    this.totalRecords$ = this.store.pipe(select(getLoadBoardDeliveredTotalRecords));
    this.serviceTypes$ = this.coreStore.pipe(select(getServiceTypes));

    this.pagingUiHelper = new PageableUiHelper({
      filterBehavior: new BehaviorSubject<Search>(null),
      pageableQueryHelper$: this.store.pipe(select(getLoadBoardDeliveredQueryHelper)),
      pagingBehavior: new BehaviorSubject<PageableQueryData>(null),
      store: this.store,
      pageableComponent: this.loadGrid,
      getQueryUpdateAction: (currentQuery: PageableQueryHelper) => new LoadBoardDeliveredUpdateQueryAction(currentQuery),
      getLoadAction: (currentQuery: PageableQueryHelper) => new LoadBoardDeliveredLoadAction({ queryHelper: currentQuery }),
    });
  }

  selected(loadId: string) {
    if (!this.isDisabled) {
      this.router.navigate(['detail', loadId], { relativeTo: this.route });
    }
  }

  updateVisibility(load: LoadView) {
    this.store.dispatch(new LoadBoardDeliveredSaveLoadAction(load));
  }

  setDisabled(isDisabled: boolean) {
    this.isDisabled = isDisabled;
  }

  displayUpdateStatus(loadId: string) {
    if (!this.isDisabled) {
      this.router.navigate(['/loads/status', loadId]);
    }
  }
  // Paging
  search(search: Search) {
    this.pagingUiHelper.pageableUiData.filterBehavior.next(search);
  }

  clear() {
    this.pagingUiHelper.pageableUiData.filterBehavior.next(null);
  }

  onLazyLoadEvent(pagingData: PageableQueryData) {
    this.pagingUiHelper.pageableUiData.pagingBehavior.next(pagingData);
  }

  ngOnDestroy(): void {
    this.pagingUiHelper.ngOnDestroy();
  }
}
