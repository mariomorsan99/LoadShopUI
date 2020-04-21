import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { select, Store } from '@ngrx/store';
import * as moment from 'moment';
import { combineLatest, Observable, Subject } from 'rxjs';
import { distinctUntilChanged, tap } from 'rxjs/operators';
import { GoogleMapService } from 'src/app/core/services';
import { CoreState, getBrowserIsMobile, getEquipment, getServiceTypes, getStates } from '../../../core/store';
import { AuditType, Equipment, LoadDetail, LoadView, Search, ServiceType, State, UserLane } from '../../../shared/models';
import { convertObjectToParams } from '../../../shared/utilities';
import { getUserLaneEntities, UserLaneAddAction, UserLaneUpdateAction, UserState } from '../../../user/store';
import {
  getLoadBoardDashboardCurrentSearch,
  getLoadBoardDashboardLoading,
  getLoadBoardDashboardLoads,
  getLoadBoardSelectedLoad,
  LoadBoardDashboardCancelLoadPollingAction,
  LoadBoardDashboardSearchAddAction,
  LoadBoardDashboardSearchClearAction,
  LoadBoardDashboardStartLoadPollingAction,
  LoadBoardState,
} from '../../store';

@Component({
  templateUrl: './search-container.component.html',
  styleUrls: ['./search-container.component.css'],
})
export class SearchContainerComponent implements OnInit, OnDestroy {
  loads$: Observable<LoadView[]>;
  filteredLoads$: Observable<LoadView[]>;
  equipment$: Observable<Equipment[]>;
  states$: Observable<State[]>;
  loading$: Observable<boolean>;
  loadDetail$: Observable<LoadDetail>;
  recent$: Observable<Search[]>;
  criteria$: Observable<Search>;
  lanes$: Observable<UserLane[]>;
  isMobile$: Observable<boolean>;
  destroyed$ = new Subject<boolean>();
  serviceTypes$: Observable<ServiceType[]>;
  isDisabled = false;

  radius = 3956;
  pidiv180 = 0.017453293;

  // tslint:disable-next-line:no-unused-variable
  private _currentSearch: Search;
  constructor(
    private loadboardStore: Store<LoadBoardState>,
    private store: Store<CoreState>,
    private userStore: Store<UserState>,
    private router: Router,
    private route: ActivatedRoute,
    private googleService: GoogleMapService
  ) {}

  ngOnInit() {
    this.loads$ = this.loadboardStore.pipe(
      select(getLoadBoardDashboardLoads),
      distinctUntilChanged((x, y) => x.length === y.length)
    );

    // criteria comes from store, anything pass via query will route to store
    this.criteria$ = this.loadboardStore.pipe(
      select(getLoadBoardDashboardCurrentSearch),
      distinctUntilChanged((x, y) => JSON.stringify(x) === JSON.stringify(y)),
      tap((x) => (this._currentSearch = x))
    );

    // TODO figure out if we need ability for deeplinking within the search page, if so we need to uncomment below,
    // and fix how the search obj gets populated to ensure all fields are populated the same via search / favorites
    // this.route.queryParams.pipe(
    //   map(x => convertParamsToObject<Search>(x, Search)),
    //   takeUntil(this.destroyed$)
    // ).subscribe(x => {
    //   if ('at' in x) {
    //     return;
    //   }
    //   // since we write out the search in the URL, check to make sure its different and not default from clearing search
    //   if (x && JSON.stringify(x) !== JSON.stringify(new Search())) {
    //     if (!this._currentSearch || JSON.stringify(x) !== JSON.stringify(this._currentSearch)) {
    //        this.loadboardStore.dispatch(new LoadBoardDashboardSearchAddAction(x));
    //     }
    //   }
    // });
    this.filteredLoads$ = combineLatest(this.loads$, this.criteria$).pipe(
      select((args) => {
        const loads: LoadView[] = args[0];
        const criteria: Search = args[1];
        return loads.filter((x) => this.filterLoad(x, criteria, this.googleService));
      })
    );
    this.equipment$ = this.store.pipe(select(getEquipment));
    this.states$ = this.store.pipe(select(getStates));
    this.loadDetail$ = this.store.pipe(select(getLoadBoardSelectedLoad));
    this.loading$ = this.store.pipe(select(getLoadBoardDashboardLoading));
    this.lanes$ = this.userStore.pipe(
      select(getUserLaneEntities),
      distinctUntilChanged((x, y) => x.length === y.length)
    );
    this.isMobile$ = this.store.pipe(select(getBrowserIsMobile));
    this.serviceTypes$ = this.store.pipe(select(getServiceTypes));

    this.loadboardStore.dispatch(new LoadBoardDashboardStartLoadPollingAction());
  }

  ngOnDestroy() {
    this.destroyed$.next(true);
    this.destroyed$.complete();
    this.loadboardStore.dispatch(new LoadBoardDashboardCancelLoadPollingAction());
  }

  search(search: Search) {
    // save the search in the store so regardless of what page the user ends up on their search is saved
    this.loadboardStore.dispatch(new LoadBoardDashboardSearchAddAction(search));

    this.router.navigate(['/loads/search/'], {
      queryParams: convertObjectToParams(search),
    });
  }

  add(lane: UserLane) {
    this.userStore.dispatch(new UserLaneAddAction(lane));
  }

  update(lane: UserLane) {
    this.userStore.dispatch(new UserLaneUpdateAction(lane));
  }

  selected(loadId: string) {
    /**
     * Because the detail container is now routeable and loaded via URL, we have to pass in query params
     * for all views that we want to track.
     */
    if (!this.isDisabled) {
      this.router.navigate(['detail', loadId], { relativeTo: this.route, queryParams: { at: AuditType.MarketplaceView } });
    }
  }

  clear() {
    // save the search in the store so regardless of what page the user ends up on their search is saved
    this.loadboardStore.dispatch(new LoadBoardDashboardSearchClearAction());
    this.router.navigate(['/loads/search/']);
  }

  setDisabled(isDisabled) {
    this.isDisabled = isDisabled;
  }

  private filterLoad(load: LoadView, search: Search, googleService: GoogleMapService) {
    load.distanceFrom = 0;

    if (
      search.quickSearch &&
      !(
        (load.referenceLoadDisplay && load.referenceLoadDisplay.toLowerCase().includes(search.quickSearch.toLowerCase())) ||
        (load.referenceLoadId && load.referenceLoadId.toLowerCase().includes(search.quickSearch.toLowerCase())) ||
        load.loadId.toLowerCase() === search.quickSearch.toLowerCase() ||
        (load.equipmentCategoryDesc && load.equipmentCategoryDesc.toLowerCase().includes(search.quickSearch.toLowerCase())) ||
        (load.equipmentCategoryId && load.equipmentCategoryId.toLowerCase().includes(search.quickSearch.toLowerCase())) ||
        (load.equipmentId && load.equipmentId.toLowerCase().includes(search.quickSearch.toLowerCase())) ||
        (load.equipmentType && load.equipmentType.toLowerCase().includes(search.quickSearch.toLowerCase())) ||
        (load.originCity && load.originCity.toLowerCase().includes(search.quickSearch.toLowerCase())) ||
        (load.destCity && load.destCity.toLowerCase().includes(search.quickSearch.toLowerCase())) ||
        (load.originState && load.originState.toLowerCase().includes(search.quickSearch.toLowerCase())) ||
        (load.destState && load.destState.includes(search.quickSearch.toLowerCase())) ||
        (load.destCity + ', ' + load.destState).toLowerCase().includes(search.quickSearch.toLowerCase()) ||
        (load.originCity + ', ' + load.originState).toLowerCase().includes(search.quickSearch.toLowerCase()) ||
        (load.scac && load.scac.toLowerCase().includes(search.quickSearch.toLowerCase()))
      )
    ) {
      return false;
    }

    if (search.origDateStart) {
      const searchStartDt = moment(search.origDateStart);
      const searchEndDt = search.origDateEnd ? moment(search.origDateEnd).add(1, 'days') : moment(search.origDateStart).add(1, 'days');
      const endDt = moment(load.originLateDtTm);
      const startDt = load.originEarlyDtTm ? moment(load.originEarlyDtTm) : endDt;
      if (!startDt.isBetween(searchStartDt, searchEndDt) || !endDt.isBetween(searchStartDt, searchEndDt)) {
        return false;
      }
    }

    if (search.destDateStart) {
      const searchStartDt = moment(search.destDateStart);
      const searchEndDt = search.destDateEnd ? moment(search.destDateEnd).add(1, 'days') : moment(search.destDateStart).add(1, 'days');
      const endDt = moment(load.destLateDtTm);
      const startDt = load.destEarlyDtTm ? moment(load.destEarlyDtTm) : endDt;
      if (!startDt.isBetween(searchStartDt, searchEndDt) || !endDt.isBetween(searchStartDt, searchEndDt)) {
        return false;
      }
    }

    if (search.equipmentType) {
      const equipment = JSON.parse(search.equipmentType) as string[];
      if (!equipment.find((x) => x === load.equipmentId)) {
        return false;
      }
    }

    if (search.destState && !search.destLat && !search.destLng) {
      if (search.destState !== load.destState) {
        return false;
      }
    }

    if (search.origState && !search.origLat && !search.origLng) {
      if (search.origState !== load.originState) {
        return false;
      }
    }

    if (search.destLat && search.destLng) {
      const dist = googleService.calculateDistance(load.destLat, load.destLng, search.destLat, search.destLng);
      load.distanceFrom += dist;
      if (dist > search.destDH) {
        return false;
      }
    }

    if (search.origLat && search.origLng) {
      const dist = googleService.calculateDistance(load.originLat, load.originLng, search.origLat, search.origLng);
      load.distanceFrom += dist;
      if (dist > search.origDH) {
        return false;
      }
    }

    if (search.serviceTypes && search.serviceTypes.length > 0) {
      const intersect = load.serviceTypeIds.filter((x) => -1 !== search.serviceTypes.indexOf(x));
      if (!intersect || intersect.length === 0) {
        return false;
      }
    }
    return true;
  }
}
