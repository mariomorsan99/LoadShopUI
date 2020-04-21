import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { select, Store } from '@ngrx/store';
import { ConfirmationService, MessageService } from 'primeng/api';
import { combineLatest, Observable, Subject } from 'rxjs';
import { distinctUntilChanged, map, takeUntil } from 'rxjs/operators';
import {
  Carrier,
  Equipment,
  LoadCarrierGroup,
  LoadCarrierGroupCarrierData,
  LoadCarrierGroupType,
  State,
  ValidationProblemDetails,
} from 'src/app/shared/models';
import { getUserProfileEntity, UserState } from 'src/app/user/store';
import {
  CarrierLoadAction,
  CoreState,
  getCarriers,
  getEquipment,
  getLoadingEquipment,
  getLoadingStates,
  getStates,
} from '../../../../core/store';
import {
  AdminState,
  getLoadCarrierGroupTypes,
  getLoadCarrierGroupTypesLoading,
  getLoadingLoadCarrierGroups,
  getLoadingSelectedLoadCarrierGroup,
  getSaveLoadCarrierGroupProblemDetails,
  getSaveLoadCarrierGroupSucceeded,
  getSavingLoadCarrierGroup,
  getSelectedLoadCarrierGroup,
  LoadCarrierGroupAddAction,
  LoadCarrierGroupClearSaveSucceededAction,
  LoadCarrierGroupDeleteAction,
  LoadCarrierGroupLoadCarrierGroupTypesAction,
  LoadCarrierGroupLoadGroupAction,
  LoadCarrierGroupLoadGroupSuccessAction,
  LoadCarrierGroupUpdateAction,
} from '../../../store';

@Component({
  selector: 'kbxl-carrier-group-detail-container',
  templateUrl: './load-carrier-group-detail-container.component.html',
  styleUrls: ['./load-carrier-group-detail-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadCarrierGroupDetailContainerComponent implements OnInit, OnDestroy {
  group$: Observable<LoadCarrierGroup>;
  carriers$: Observable<LoadCarrierGroupCarrierData[]>;
  states$: Observable<State[]>;
  processing$: Observable<boolean>;
  error$: Observable<ValidationProblemDetails>;
  allCarriers$: Observable<Carrier[]>;
  loadCarrierGroupTypes$: Observable<LoadCarrierGroupType[]>;

  destroyed$ = new Subject<boolean>();
  equipment$: Observable<Equipment[]>;

  constructor(
    private adminStore: Store<AdminState>,
    private coreStore: Store<CoreState>,
    private userStore: Store<UserState>,
    private route: ActivatedRoute,
    private router: Router,
    private confirmationService: ConfirmationService,
    private messageService: MessageService
  ) {}

  ngOnInit() {
    this.route.params.pipe(map(p => (p.id ? parseInt(p.id, 0) : null))).subscribe(id => {
      if (isNaN(id)) {
        this.messageService.add({ detail: 'Invalid load carrier group id provided.', severity: 'error' });
      } else if (id > 0) {
        this.adminStore.dispatch(new LoadCarrierGroupLoadGroupAction({ loadCarrierGroupId: id }));
      } else {
        this.userStore.pipe(select(getUserProfileEntity), takeUntil(this.destroyed$)).subscribe(user => {
          if (user && user.primaryCustomerId) {
            this.adminStore.dispatch(
              new LoadCarrierGroupLoadGroupSuccessAction(
                new LoadCarrierGroup({ customerId: user.primaryCustomerId, loadCarrierGroupEquipment: [], carriers: [] })
              )
            );
          }
        });
      }
    });

    this.loadCarrierGroupTypes$ = this.adminStore.pipe(map(getLoadCarrierGroupTypes));
    this.adminStore.dispatch(new LoadCarrierGroupLoadCarrierGroupTypesAction());

    this.equipment$ = this.coreStore.pipe(map(getEquipment));

    this.coreStore.dispatch(new CarrierLoadAction());

    this.allCarriers$ = this.coreStore.pipe(map(getCarriers));

    this.states$ = this.coreStore.pipe(map(getStates));
    this.group$ = this.adminStore.pipe(
      map(getSelectedLoadCarrierGroup),
      distinctUntilChanged(),
      map(g => new LoadCarrierGroup(g))
    );
    this.processing$ = combineLatest(
      this.adminStore.pipe(map(getLoadingLoadCarrierGroups)),
      this.adminStore.pipe(map(getLoadingSelectedLoadCarrierGroup)),
      this.adminStore.pipe(map(getSavingLoadCarrierGroup)),
      this.coreStore.pipe(map(getLoadingEquipment)),
      this.coreStore.pipe(map(getLoadingStates)),
      this.adminStore.pipe(map(getLoadCarrierGroupTypesLoading))
    ).pipe(map(args => args[0] || args[1] || args[2] || args[3] || args[4] || args[5]));

    this.error$ = this.adminStore.pipe(map(getSaveLoadCarrierGroupProblemDetails));

    this.adminStore.pipe(map(getSaveLoadCarrierGroupSucceeded), takeUntil(this.destroyed$)).subscribe(saveSucceeded => {
      if (saveSucceeded) {
        this.adminStore.dispatch(new LoadCarrierGroupClearSaveSucceededAction());
        this.router.navigate(['maint/carrier-groups']);
      }
    });
  }

  ngOnDestroy() {
    this.adminStore.dispatch(new LoadCarrierGroupLoadGroupSuccessAction(null));
    this.destroyed$.next(true);
    this.destroyed$.complete();
  }

  updated(group: LoadCarrierGroup) {
    if (group) {
      if (group.loadCarrierGroupId > 0) {
        this.adminStore.dispatch(new LoadCarrierGroupUpdateAction(group));
      } else {
        this.adminStore.dispatch(new LoadCarrierGroupAddAction(group));
      }
    }
  }

  delete(group: LoadCarrierGroup) {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete this load carrier group?`,
      accept: () => {
        this.adminStore.dispatch(new LoadCarrierGroupDeleteAction(group));
      },
    });
  }
}
