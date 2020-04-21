import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { select, Store } from '@ngrx/store';
import { ConfirmationService, MessageService } from 'primeng/api';
import { combineLatest, Observable, Subject } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';
import { Equipment, SpecialInstruction, State, ValidationProblemDetails } from 'src/app/shared/models';
import { getUserProfileEntity, UserState } from 'src/app/user/store';
import { CoreState, getEquipment, getLoadingEquipment, getLoadingStates, getStates } from '../../../../core/store';
import {
  AdminState,
  getLoadingSelectedSpecialInstructions,
  getLoadingSpecialInstructions,
  getSaveSpecialInstructionsProblemDetails,
  getSaveSpecialInstructionsSucceeded,
  getSavingSpecialInstructions,
  getSelectedSpecialInstructions,
  SpecialInstructionsAddAction,
  SpecialInstructionsClearSaveSucceededAction,
  SpecialInstructionsDeleteAction,
  SpecialInstructionsLoadInstructionAction,
  SpecialInstructionsLoadInstructionSuccessAction,
  SpecialInstructionsUpdateAction,
} from '../../../store';

@Component({
  selector: 'kbxl-special-instructions-detail-container',
  templateUrl: './special-instructions-detail-container.component.html',
  styleUrls: ['./special-instructions-detail-container.component.scss'],
})
export class SpecialInstructionsDetailContainerComponent implements OnInit, OnDestroy {
  specialInstruction$: Observable<SpecialInstruction>;
  states$: Observable<State[]>;
  processing$: Observable<boolean>;
  error$: Observable<ValidationProblemDetails>;
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
    this.route.params
      .pipe(
        map(p => (p.id ? parseInt(p.id, 10) : null)),
        takeUntil(this.destroyed$)
      )
      .subscribe(id => {
        if (isNaN(id)) {
          this.messageService.add({ detail: 'Invalid special instruction id provided.', severity: 'error' });
        } else if (id > 0) {
          this.adminStore.dispatch(new SpecialInstructionsLoadInstructionAction({ specialInstructionId: id }));
        } else {
          this.userStore.pipe(select(getUserProfileEntity), takeUntil(this.destroyed$)).subscribe(user => {
            if (user && user.primaryCustomerId) {
              this.adminStore.dispatch(
                new SpecialInstructionsLoadInstructionSuccessAction(
                  new SpecialInstruction({ customerId: user.primaryCustomerId, specialInstructionEquipment: [] })
                )
              );
            }
          });
        }
      });

    this.equipment$ = this.coreStore.pipe(map(getEquipment));

    this.states$ = this.coreStore.pipe(map(getStates));
    this.specialInstruction$ = this.adminStore.pipe(
      select(getSelectedSpecialInstructions),
      map(g => new SpecialInstruction(g))
    );
    this.processing$ = combineLatest(
      this.adminStore.pipe(select(getLoadingSpecialInstructions)),
      this.adminStore.pipe(select(getLoadingSelectedSpecialInstructions)),
      this.adminStore.pipe(select(getSavingSpecialInstructions)),
      this.coreStore.pipe(select(getLoadingEquipment)),
      this.coreStore.pipe(select(getLoadingStates))
    ).pipe(map(args => args[0] || args[1] || args[2] || args[3] || args[4]));

    this.error$ = this.adminStore.pipe(select(getSaveSpecialInstructionsProblemDetails));

    this.adminStore.pipe(select(getSaveSpecialInstructionsSucceeded), takeUntil(this.destroyed$)).subscribe(saveSucceeded => {
      if (saveSucceeded) {
        this.adminStore.dispatch(new SpecialInstructionsClearSaveSucceededAction());
        this.router.navigate(['maint/special-instructions']);
      }
    });
  }

  ngOnDestroy() {
    this.adminStore.dispatch(new SpecialInstructionsLoadInstructionSuccessAction(null));
    this.destroyed$.next(true);
    this.destroyed$.complete();
  }

  updated(instruction: SpecialInstruction) {
    if (instruction) {
      if (instruction.specialInstructionId > 0) {
        this.adminStore.dispatch(new SpecialInstructionsUpdateAction(instruction));
      } else {
        this.adminStore.dispatch(new SpecialInstructionsAddAction(instruction));
      }
    }
  }

  delete(instruction: SpecialInstruction) {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete this special instruction?`,
      accept: () => {
        this.adminStore.dispatch(new SpecialInstructionsDeleteAction(instruction));
      },
    });
  }
}
