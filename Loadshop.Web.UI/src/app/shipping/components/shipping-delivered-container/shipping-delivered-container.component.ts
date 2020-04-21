import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { select, Store } from '@ngrx/store';
import { BehaviorSubject, Observable } from 'rxjs';
import { distinctUntilChanged, map } from 'rxjs/operators';
import { CoreState, getBrowserIsMobile, getEquipment, getServiceTypes, getStates } from 'src/app/core/store';
import { ShippingLoadSearchTypes } from 'src/app/shared/models/shipping-load-search-types';
import { ShippingLoadView } from 'src/app/shared/models/shipping-load-view';
import { PageableQueryData, PageableQueryHelper, PageableUiHelper } from 'src/app/shared/utilities';
import { getUserProfileEntity } from 'src/app/user/store';
import { LoadBoardState } from '../../../load-board/store';
import { Equipment, Search, ServiceType, State, User } from '../../../shared/models';
import {
  getShippingDeliveredLoading,
  getShippingDeliveredLoads,
  getShippingDeliveredQueryHelper,
  ShippingDeliveredLoadAction,
  ShippingDeliveredQueryUpdateAction,
  ShippingState,
} from '../../store';
import { ShippingGridComponent } from '../shipping-grid';
@Component({
  templateUrl: './shipping-delivered-container.component.html',
  styleUrls: ['./shipping-delivered-container.component.css'],
})
export class ShippingDeliveredContainerComponent implements OnInit, OnDestroy {
  loads$: Observable<ShippingLoadView[]>;
  totalRecords$: Observable<number>;
  loading$: Observable<boolean>;
  user$: Observable<User>;
  isDisabled = false;
  equipment$: Observable<Equipment[]>;
  states$: Observable<State[]>;
  isMobile$: Observable<boolean>;
  serviceTypes$: Observable<ServiceType[]>;
  // Paging
  pagingUiHelper: PageableUiHelper<ShippingDeliveredQueryUpdateAction, ShippingDeliveredLoadAction, LoadBoardState>;
  @ViewChild(ShippingGridComponent) shippingGrid: ShippingGridComponent;

  constructor(
    private store: Store<ShippingState>,
    private router: Router,
    private route: ActivatedRoute,
    private loadBoardStore: Store<LoadBoardState>,
    private coreState: Store<CoreState>
  ) {}

  ngOnInit() {
    this.loads$ = this.store.pipe(
      map(getShippingDeliveredLoads),
      map((x) => x.data),
      distinctUntilChanged((x, y) => x.length === y.length)
    );

    this.totalRecords$ = this.store.pipe(
      map(getShippingDeliveredLoads),
      map((x) => x.totalRecords)
    );
    this.loading$ = this.store.pipe(map(getShippingDeliveredLoading));
    // Don't need to do this because data will be loaded when the the init paging event is fired
    // this.store.dispatch(
    //   new ShippingDeliveredLoadAction({ searchType: ShippingLoadSearchTypes.Delivered, queryHelper: PageableQueryHelper.default() })
    // );
    this.user$ = this.loadBoardStore.pipe(map(getUserProfileEntity));

    // Filters
    this.equipment$ = this.coreState.pipe(map(getEquipment));
    this.states$ = this.coreState.pipe(map(getStates));
    this.isMobile$ = this.coreState.pipe(map(getBrowserIsMobile));
    this.serviceTypes$ = this.coreState.pipe(select(getServiceTypes));

    this.pagingUiHelper = new PageableUiHelper({
      filterBehavior: new BehaviorSubject<Search>(null),
      pageableQueryHelper$: this.loadBoardStore.pipe(select(getShippingDeliveredQueryHelper)),
      pagingBehavior: new BehaviorSubject<PageableQueryData>(null),
      store: this.loadBoardStore,
      pageableComponent: this.shippingGrid,
      getQueryUpdateAction: (currentQuery: PageableQueryHelper) => new ShippingDeliveredQueryUpdateAction(currentQuery),
      getLoadAction: (currentQuery: PageableQueryHelper) =>
        new ShippingDeliveredLoadAction({ searchType: ShippingLoadSearchTypes.Delivered, queryHelper: currentQuery }),
    });
  }

  selected(loadId: string) {
    if (!this.isDisabled) {
      this.router.navigate(['detail', loadId], { relativeTo: this.route });
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
