import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { select, Store } from '@ngrx/store';
import { BehaviorSubject, Observable } from 'rxjs';
import { distinctUntilChanged } from 'rxjs/operators';
import { PageableQueryData, PageableQueryHelper, PageableUiHelper } from 'src/app/shared/utilities';
import { CoreState, getEquipment, getMenuVisibilityBadge, getServiceTypes, getStates } from '../../../core/store';
import { Equipment, LoadDetail, LoadView, Search, ServiceType, State, UserModel, VisibilityBadge } from '../../../shared/models';
import { getUserProfileModel } from '../../../user/store';
import {
  getLoadBoardBookedErrorLoadId,
  getLoadBoardBookedLoading,
  getLoadBoardBookedLoads,
  getLoadBoardBookedPhoneError,
  getLoadBoardBookedQueryHelper,
  getLoadBoardBookedSavingLoadId,
  getLoadBoardBookedTruckError,
  getLoadBoardSelectedLoad,
  LoadBoardBookedLoadAction,
  LoadBoardBookedSaveLoadAction,
  LoadBoardBookedUpdateQueryAction,
  LoadBoardState,
  getLoadBoardBookedTotalRecords,
} from '../../store';
import { LoadGridComponent } from '../load-grid';

@Component({
  templateUrl: './booked-container.component.html',
  styleUrls: ['./booked-container.component.css'],
})
export class BookedContainerComponent implements OnInit, OnDestroy {
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
  pagingUiHelper: PageableUiHelper<LoadBoardBookedUpdateQueryAction, LoadBoardBookedLoadAction, LoadBoardState>;
  totalRecords$: Observable<number>;

  constructor(
    private store: Store<LoadBoardState>,
    private coreStore: Store<CoreState>,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    this.loads$ = this.store.pipe(
      select(getLoadBoardBookedLoads),
      distinctUntilChanged((x, y) => x.length === y.length)
    );
    this.states$ = this.coreStore.pipe(select(getStates));
    this.equipment$ = this.coreStore.pipe(select(getEquipment));
    this.loadDetail$ = this.store.pipe(select(getLoadBoardSelectedLoad));
    this.totalRecords$ = this.store.pipe(select(getLoadBoardBookedTotalRecords));
    this.dashboardLoading$ = this.store.pipe(select(getLoadBoardBookedLoading));
    this.savingLoadId$ = this.store.pipe(select(getLoadBoardBookedSavingLoadId));
    this.errorLoadId$ = this.store.pipe(select(getLoadBoardBookedErrorLoadId));
    this.phoneError$ = this.store.pipe(select(getLoadBoardBookedPhoneError));
    this.truckError$ = this.store.pipe(select(getLoadBoardBookedTruckError));
    this.user$ = this.store.pipe(select(getUserProfileModel));
    this.visibilityBadge$ = this.coreStore.pipe(select(getMenuVisibilityBadge));
    this.serviceTypes$ = this.coreStore.pipe(select(getServiceTypes));

    this.pagingUiHelper = new PageableUiHelper({
      filterBehavior: new BehaviorSubject<Search>(null),
      pageableQueryHelper$: this.store.pipe(select(getLoadBoardBookedQueryHelper)),
      pagingBehavior: new BehaviorSubject<PageableQueryData>(null),
      store: this.store,
      pageableComponent: this.loadGrid,
      getQueryUpdateAction: (currentQuery: PageableQueryHelper) => new LoadBoardBookedUpdateQueryAction(currentQuery),
      getLoadAction: (currentQuery: PageableQueryHelper) => new LoadBoardBookedLoadAction({ queryHelper: currentQuery }),
    });
  }

  selected(loadId: string) {
    if (!this.isDisabled) {
      this.router.navigate(['detail', loadId], { relativeTo: this.route });
    }
  }

  updateVisibility(load: LoadView) {
    this.store.dispatch(new LoadBoardBookedSaveLoadAction(load));
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
