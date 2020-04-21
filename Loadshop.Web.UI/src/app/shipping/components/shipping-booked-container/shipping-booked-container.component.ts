import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { select, Store } from '@ngrx/store';
import { BehaviorSubject, Observable } from 'rxjs';
import { distinctUntilChanged, filter, map, take } from 'rxjs/operators';
import { CoreState, getBrowserIsMobile, getEquipment, getServiceTypes, getStates } from 'src/app/core/store';
import { SecurityAppActionType } from 'src/app/shared/models/security-app-action-type';
import { ShippingLoadSearchTypes } from 'src/app/shared/models/shipping-load-search-types';
import { ShippingLoadView } from 'src/app/shared/models/shipping-load-view';
import { PageableQueryData, PageableQueryHelper, PageableUiHelper } from 'src/app/shared/utilities';
import { getUserProfileEntity, getUserProfileModel, UserState } from 'src/app/user/store';
import { LoadBoardState } from '../../../load-board/store';
import { Equipment, Search, State, User, ServiceType } from '../../../shared/models';
import {
  getShippingBookedLoading,
  getShippingBookedLoads,
  getShippingBookedQueryHelper,
  ShippingBookedLoadAction,
  ShippingBookedUpdateQueryAction,
  ShippingState,
} from '../../store';
import { ShippingGridComponent } from '../shipping-grid';

@Component({
  templateUrl: './shipping-booked-container.component.html',
  styleUrls: ['./shipping-booked-container.component.css'],
})
export class ShippingBookedContainerComponent implements OnInit, OnDestroy {
  loads$: Observable<ShippingLoadView[]>;
  loading$: Observable<boolean>;
  user$: Observable<User>;
  totalRecords$: Observable<number>;
  equipment$: Observable<Equipment[]>;
  serviceTypes$: Observable<ServiceType[]>;

  // Paging
  @ViewChild(ShippingGridComponent) shippingGrid: ShippingGridComponent;
  pagingUiHelper: PageableUiHelper<ShippingBookedUpdateQueryAction, ShippingBookedLoadAction, LoadBoardState>;
  states$: Observable<State[]>;
  isMobile$: Observable<boolean>;

  constructor(
    private store: Store<ShippingState>,
    private loadBoardStore: Store<LoadBoardState>,
    private coreState: Store<CoreState>,
    private userStore: Store<UserState>,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    this.loads$ = this.store.pipe(
      map(getShippingBookedLoads),
      map((x) => x.data),
      distinctUntilChanged((x, y) => x.length === y.length)
    );

    this.totalRecords$ = this.store.pipe(
      map(getShippingBookedLoads),
      map((x) => x.totalRecords)
    );

    // Filters
    this.equipment$ = this.coreState.pipe(map(getEquipment));
    this.states$ = this.coreState.pipe(map(getStates));
    this.isMobile$ = this.coreState.pipe(map(getBrowserIsMobile));

    this.loading$ = this.store.pipe(map(getShippingBookedLoading));
    this.user$ = this.loadBoardStore.pipe(map(getUserProfileEntity));
    this.serviceTypes$ = this.coreState.pipe(select(getServiceTypes));

    this.pagingUiHelper = new PageableUiHelper({
      filterBehavior: new BehaviorSubject<Search>(null),
      pageableQueryHelper$: this.loadBoardStore.pipe(select(getShippingBookedQueryHelper)),
      pagingBehavior: new BehaviorSubject<PageableQueryData>(null),
      store: this.loadBoardStore,
      pageableComponent: this.shippingGrid,
      getQueryUpdateAction: (currentQuery: PageableQueryHelper) => new ShippingBookedUpdateQueryAction(currentQuery),
      getLoadAction: (currentQuery: PageableQueryHelper) =>
        new ShippingBookedLoadAction({ searchType: ShippingLoadSearchTypes.Booked, queryHelper: currentQuery }),
    });
  }

  selected(loadId: string) {
    this.userStore
      .pipe(
        take(1),
        map(getUserProfileModel),
        filter((x) => x != null)
      )
      .subscribe((user) => {
        if (user.hasSecurityAction(SecurityAppActionType.ShipperViewBookedDetail)) {
          this.router.navigate(['detail', loadId], { relativeTo: this.route });
        }
      });
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
