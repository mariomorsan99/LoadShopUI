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
import { Equipment, Search, ServiceType, State, User } from '../../../shared/models';
import {
  getShippingPostedLoading,
  getShippingPostedLoads,
  getShippingPostedQueryHelper,
  ShippingPostedLoadAction,
  ShippingPostedUpdateQueryAction,
  ShippingState,
} from '../../store';
import { ShippingGridComponent } from '../shipping-grid';

@Component({
  templateUrl: './shipping-posted-container.component.html',
  styleUrls: ['./shipping-posted-container.component.css'],
})
export class ShippingPostedContainerComponent implements OnInit, OnDestroy {
  loads$: Observable<ShippingLoadView[]>;
  loading$: Observable<boolean>;
  user$: Observable<User>;

  // Paging
  @ViewChild(ShippingGridComponent) shippingGrid: ShippingGridComponent;
  pagingUiHelper: PageableUiHelper<ShippingPostedUpdateQueryAction, ShippingPostedLoadAction, LoadBoardState>;
  totalRecords$: Observable<number>;
  states$: Observable<State[]>;
  isMobile$: Observable<boolean>;
  equipment$: Observable<Equipment[]>;
  serviceTypes$: Observable<ServiceType[]>;
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
      map(getShippingPostedLoads),
      map((x) => x.data),
      distinctUntilChanged((x, y) => x.length === y.length)
    );

    this.totalRecords$ = this.store.pipe(
      map(getShippingPostedLoads),
      map((x) => x.totalRecords)
    );

    // Filters
    this.equipment$ = this.coreState.pipe(map(getEquipment));
    this.states$ = this.coreState.pipe(map(getStates));
    this.isMobile$ = this.coreState.pipe(map(getBrowserIsMobile));

    this.loading$ = this.store.pipe(map(getShippingPostedLoading));
    this.user$ = this.loadBoardStore.pipe(map(getUserProfileEntity));
    this.serviceTypes$ = this.coreState.pipe(select(getServiceTypes));

    this.pagingUiHelper = new PageableUiHelper({
      filterBehavior: new BehaviorSubject<Search>(null),
      pageableQueryHelper$: this.loadBoardStore.pipe(select(getShippingPostedQueryHelper)),
      pagingBehavior: new BehaviorSubject<PageableQueryData>(null),
      store: this.loadBoardStore,
      pageableComponent: this.shippingGrid,
      getQueryUpdateAction: (currentQuery: PageableQueryHelper) => new ShippingPostedUpdateQueryAction(currentQuery),
      getLoadAction: (currentQuery: PageableQueryHelper) =>
        new ShippingPostedLoadAction({ searchType: ShippingLoadSearchTypes.Posted, queryHelper: currentQuery }),
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
        if (user.hasSecurityAction(SecurityAppActionType.ShipperViewPostedDetail)) {
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
