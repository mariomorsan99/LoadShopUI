import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { select, Store } from '@ngrx/store';
import { ConfirmationService } from 'primeng/api';
import { BehaviorSubject, combineLatest, Observable, Subject } from 'rxjs';
import { debounceTime, map, takeUntil, tap, withLatestFrom } from 'rxjs/operators';
import {
  CarrierLoadAction,
  CommodityLoadAction,
  CoreState,
  getEquipment,
  getStates,
  CustomerLoadTypeLoadAction,
  getCustomerLoadTypes,
  SmartSpotClearCreateOrderFromQuote,
  getSmartSpotPrices,
  getAnySmartSpotsLoading
} from 'src/app/core/store';
import { getUserProfileEntity } from 'src/app/user/store';
import {
  Equipment,
  IShippingLoadDetail,
  ShippingLoadDetail,
  State,
  CustomerLoadType,
  SmartSpotPrice,
  User,
} from 'src/app/shared/models';
import { ShippingLoadFilter } from 'src/app/shared/models/shipping-load-filter';
import { ValidationProblemDetails } from 'src/app/shared/models/validation-problem-details';
import {
  getLoadingShippingLoadDetails,
  getShippingHomeLoads,
  getShippingPostValidationProblemDetails,
  getShippingSuccessfullyPostedLoads,
  ShippingLoadDetailDeleteLoadAction,
  ShippingLoadDetailDiscardChanges,
  ShippingLoadDetailLoadAllAction,
  ShippingLoadDetailPostLoadsAction,
  ShippingLoadDetailRemoveLoadAction,
  ShippingState
} from '../../../store';
import { Dictionary } from '@ngrx/entity';

@Component({
  selector: 'kbxl-shipping-load-home-container',
  templateUrl: './shipping-load-home-container.component.html',
  styleUrls: ['./shipping-load-home-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShippingLoadHomeContainerComponent implements OnInit, OnDestroy {
  loads$: Observable<ShippingLoadDetail[]>;
  availableLoads: ShippingLoadDetail[];
  selectedLoadsSubject = new BehaviorSubject<ShippingLoadDetail[]>([]);
  selectedLoads$ = this.selectedLoadsSubject.asObservable();
  loading$: Observable<boolean>;
  equipment$: Observable<Equipment[]>;
  customerLoadTypes$: Observable<CustomerLoadType[]>;
  states$: Observable<State[]>;
  postValidationProblemDetails$: Observable<ValidationProblemDetails>;
  loadingAnySmartSpotPrices$: Observable<boolean>;
  smartSpotPrices$: Observable<Dictionary<SmartSpotPrice>>;
  userProfile$: Observable<User>;

  filteredLoads: ShippingLoadDetail[];
  filterCriteria: ShippingLoadFilter = new ShippingLoadFilter();
  displayFilterCriteriaDialog = false;

  canCreateLoad = false;

  private searchSubject = new BehaviorSubject<ShippingLoadFilter>(null);
  private initialReferenceLoadIds: string[];
  private initialSelectionsMade = false;

  constructor(
    private shippingStore: Store<ShippingState>,
    private coreStore: Store<CoreState>,
    private confirmationService: ConfirmationService,
    private activatedRoute: ActivatedRoute,
    private router: Router
  ) { }

  /**
   * https://medium.com/@stodge/ngrx-common-gotchas-8f59f541e47c
   * Required to prevent memory leaks when subscribing to ngrx store for purposes
   * other than dumping to an HTML template with an async pipe
   */
  private ngUnsubscribe: Subject<void> = new Subject<void>();
  ngOnDestroy() {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }

  ngOnInit() {
    this.activatedRoute.queryParams.subscribe(params => {
      this.initialReferenceLoadIds =
        params.sep && params.loadids ? (params.loadids as string).split(params.sep).map(_ => _.toLowerCase()) : null;
    });

    this.loads$ = combineLatest(
      this.shippingStore.pipe(
        select(getShippingHomeLoads),
        map(_ => this.sortQueryLoadsToTop(_))
      ),
      this.searchSubject.pipe(debounceTime(350))
    ).pipe(
      map(args => {
        const filter = args[1];
        if (!filter) {
          return args[0];
        }
        const quickFilter = filter.quickFilter || '';
        return args[0].filter(
          x =>
            (!filter.equipment ||
              filter.equipment.length === 0 ||
              filter.equipment.map(equip => equip.equipmentId).some(equip => equip === x.equipmentId)) &&
            (!filter.customerLoadTypes ||
              filter.customerLoadTypes.length === 0 ||
              filter.customerLoadTypes.map(type => type.customerLoadTypeId).some(type => type === x.customerLoadTypeId)) &&
            (!filter.origin ||
              x.loadStops.some(
                y =>
                  y.stopNbr === 1 &&
                  (!filter.origin.city || this.includesIgnoreCase(y.city, filter.origin.city)) &&
                  this.includesIgnoreCase(y.state, filter.origin.stateAbbrev)
              )) &&
            (!filter.dest ||
              x.loadStops.some(
                y =>
                  y.stopNbr === 2 &&
                  (!filter.dest.city || this.includesIgnoreCase(y.city, filter.dest.city)) &&
                  this.includesIgnoreCase(y.state, filter.dest.stateAbbrev)
              )) &&
            // quick filter below
            (quickFilter === '' ||
              this.includesIgnoreCase(x.referenceLoadDisplay, quickFilter) ||
              this.includesIgnoreCase(x.equipmentTypeDisplay, quickFilter) ||
              x.loadStops.some(y => this.includesIgnoreCase(y.city, quickFilter) || this.includesIgnoreCase(y.state, quickFilter)))
        );
      }),
      tap(_ => {
        this.availableLoads = _;
        this.selectInitialLoads(_);
      })
    );

    this.loading$ = this.shippingStore.pipe(select(getLoadingShippingLoadDetails));
    this.postValidationProblemDetails$ = this.shippingStore.pipe(select(getShippingPostValidationProblemDetails));
    this.shippingStore.dispatch(new ShippingLoadDetailLoadAllAction());
    this.coreStore.dispatch(new CarrierLoadAction());
    this.coreStore.dispatch(new CommodityLoadAction());
    this.coreStore.dispatch(new CustomerLoadTypeLoadAction());
    this.equipment$ = this.coreStore.pipe(select(getEquipment));
    this.customerLoadTypes$ = this.coreStore.pipe(select(getCustomerLoadTypes));
    this.states$ = this.coreStore.pipe(select(getStates));
    this.loadingAnySmartSpotPrices$ = this.coreStore.pipe(select(getAnySmartSpotsLoading));
    this.smartSpotPrices$ = this.coreStore.pipe(select(getSmartSpotPrices));
    this.userProfile$ = this.coreStore.pipe(select(getUserProfileEntity));

    this.shippingStore
      .select(getShippingSuccessfullyPostedLoads)
      .pipe(
        tap(_ => this.clearQueryParams(_)),
        withLatestFrom(this.loads$),
        takeUntil(this.ngUnsubscribe)
      )
      .subscribe(([postedLoads, loads]: [ShippingLoadDetail[], ShippingLoadDetail[]]) => {
        this.handlePostedLoads(loads, postedLoads);
      });

    // Combat the race condition when trying to select a newly created load from order entry
    // before that load is been loaded by this component's store
    combineLatest(
      this.shippingStore.pipe(select(getShippingHomeLoads), takeUntil(this.ngUnsubscribe), map(_ => this.sortQueryLoadsToTop(_))),
      this.shippingStore.pipe(select(getLoadingShippingLoadDetails), takeUntil(this.ngUnsubscribe))
    ).subscribe(([loads, isLoading]: [ShippingLoadDetail[], boolean]) => {
      const selectedLoads = this.selectedLoadsSubject.getValue();
      const loadsShouldBeSelected = !isLoading && this.initialReferenceLoadIds
        && this.initialReferenceLoadIds.length > 0
        && (!selectedLoads || selectedLoads.length <= 0);
      if (loadsShouldBeSelected) {
        this.initialSelectionsMade = false;
        this.selectInitialLoads(loads);
      }
    });
  }

  private includesIgnoreCase(string1: string, string2: string): boolean {
    if (string1 === string2) {
      return true;
    }
    if (!string1 || !string2) {
      return false;
    }
    return string1.toLocaleLowerCase().includes(string2.toLocaleLowerCase());
  }

  clearQueryParams(postedLoads: IShippingLoadDetail[]) {
    if (postedLoads && postedLoads.length === this.selectedLoadsSubject.getValue().length) {
      // everything posted, clear the query params
      this.router.navigate([], { relativeTo: this.activatedRoute, queryParams: {}, replaceUrl: true });
      this.initialReferenceLoadIds = null;
    }
  }

  handlePostedLoads(loads: ShippingLoadDetail[], postedLoads: ShippingLoadDetail[]) {
    if (!loads || !postedLoads) {
      return;
    }

    const existingSelectedLoads = this.selectedLoadsSubject.getValue();
    const selectedLoadIds = existingSelectedLoads.map(_ => _.loadId);

    const postedLoadIds = postedLoads.map(_ => _.loadId);
    const newSelectedLoads = (this.availableLoads || [])
      // Only return loads that were already selected, but did not post successfully
      .filter(_ => selectedLoadIds.indexOf(_.loadId) >= 0 && postedLoadIds.indexOf(_.loadId) < 0)
      .map(_ => {
        // map to either the exsting selection or a new copy
        return existingSelectedLoads.find(s => s.loadId === _.loadId) || new ShippingLoadDetail({ ..._ });
      });

    this.selectedLoadsSubject.next(newSelectedLoads);
  }

  loadSelected(load: ShippingLoadDetail) {
    const existingSelectedLoads = this.selectedLoadsSubject.getValue();
    const selectedLoadIds = existingSelectedLoads.map(_ => _.loadId);
    selectedLoadIds.push(load.loadId);
    const newSelectedLoads = (this.availableLoads || [])
      .filter(_ => selectedLoadIds.indexOf(_.loadId) >= 0)
      .map(_ => {
        // map to either the exsting selection or a new copy
        return existingSelectedLoads.find(s => s.loadId === _.loadId) || new ShippingLoadDetail({ ..._ });
      });
    this.selectedLoadsSubject.next(newSelectedLoads);
  }

  loadUnselected(load: ShippingLoadDetail) {
    const existingSelectedLoads = this.selectedLoadsSubject.getValue();
    const unselectingLoad = existingSelectedLoads.find(_ => _.loadId === load.loadId);
    if (unselectingLoad.hasChanges) {
      this.confirmationService.confirm({
        message:
          // tslint:disable-next-line:max-line-length
          'There are unsaved changes on the load. Are you sure you want to close the load and discard the changes?<br/><br/>To save changes to the load select No and then click POST.',
        accept: () => {
          this.selectedLoadsSubject.next((existingSelectedLoads || []).filter(_ => _.loadId !== load.loadId));
          this.shippingStore.dispatch(new ShippingLoadDetailDiscardChanges(load));
        },
      });
    } else {
      this.selectedLoadsSubject.next((existingSelectedLoads || []).filter(_ => _.loadId !== load.loadId));
    }
  }

  postLoads(selectedLoads: ShippingLoadDetail[]) {
    this.shippingStore.dispatch(new ShippingLoadDetailPostLoadsAction(selectedLoads));
  }

  createLoad() {
    this.coreStore.dispatch(new SmartSpotClearCreateOrderFromQuote());
    this.router.navigate(['/shipping/home/create']);
  }

  removeLoad(load: ShippingLoadDetail) {
    this.confirmationService.confirm({
      message: 'Are you sure you want to remove this load from the Marketplace?  It will remain in Post.',
      accept: () => {
        this.shippingStore.dispatch(new ShippingLoadDetailRemoveLoadAction(load.loadId));
      },
    });
  }

  deleteLoad(load: ShippingLoadDetail) {
    this.confirmationService.confirm({
      message: 'Are you sure you want to completely delete this load from Loadshop?',
      accept: () => {
        this.shippingStore.dispatch(new ShippingLoadDetailDeleteLoadAction(load.loadId));
      },
    });
  }

  sortQueryLoadsToTop(loads: ShippingLoadDetail[]): ShippingLoadDetail[] {
    if (loads && this.initialReferenceLoadIds && this.initialReferenceLoadIds.length > 0) {
      const queryLoads: ShippingLoadDetail[] = [];
      loads = loads.filter(_ => {
        const queryLoad = this.initialReferenceLoadIds.includes(_.referenceLoadId.toLowerCase());
        if (queryLoad) {
          queryLoads.push(_);
        }
        return !queryLoad;
      });
      loads = queryLoads.concat(loads);
    }
    return loads;
  }

  selectInitialLoads(loads: ShippingLoadDetail[]) {
    if (!this.initialSelectionsMade && loads && loads.length > 0) {
      let selectedLoads;
      if (this.initialReferenceLoadIds && this.initialReferenceLoadIds.length > 0) {
        selectedLoads = loads.filter(_ => this.initialReferenceLoadIds.includes(_.referenceLoadId.toLowerCase()));
      } else {
        selectedLoads = [loads[0]];
      }

      this.selectedLoadsSubject.next(selectedLoads.map(_ => new ShippingLoadDetail({ ..._ })));
      this.initialSelectionsMade = true;
    }
  }

  onFilterChange(filterCriteria: ShippingLoadFilter) {
    this.searchSubject.next(filterCriteria);
  }

  viewFilterCriteria() {
    this.displayFilterCriteriaDialog = true;
  }

  clearFilter(prop: any) {
    this.filterCriteria = new ShippingLoadFilter();
    this.onFilterChange(this.filterCriteria);
  }
}
