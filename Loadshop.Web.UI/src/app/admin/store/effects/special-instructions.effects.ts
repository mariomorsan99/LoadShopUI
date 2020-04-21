import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { MessageService } from 'primeng/api';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { SpecialInstruction } from '../../../shared/models';
import { SpecialInstructionsService } from '../../services/special-instructions.service';
import {
  SpecialInstructionsActionTypes,
  SpecialInstructionsAddAction,
  SpecialInstructionsAddFailureAction,
  SpecialInstructionsAddSuccessAction,
  SpecialInstructionsDeleteAction,
  SpecialInstructionsDeleteFailureAction,
  SpecialInstructionsDeleteSuccessAction,
  SpecialInstructionsLoadAction,
  SpecialInstructionsLoadFailureAction,
  SpecialInstructionsLoadInstructionAction,
  SpecialInstructionsLoadInstructionFailureAction,
  SpecialInstructionsLoadInstructionSuccessAction,
  SpecialInstructionsLoadSuccessAction,
  SpecialInstructionsUpdateAction,
  SpecialInstructionsUpdateFailureAction,
  SpecialInstructionsUpdateSuccessAction,
} from '../actions';

@Injectable()
export class SpecialInstructionsEffects {
  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType<SpecialInstructionsLoadAction>(SpecialInstructionsActionTypes.Load),
    mapToPayload<{ customerId: string }>(),
    switchMap((payload: { customerId: string }) => {
      return this.specialInstructionsService.getAll(payload.customerId).pipe(
        map(data => new SpecialInstructionsLoadSuccessAction(data)),
        catchError(err => of(new SpecialInstructionsLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadSpecialInstruction: Observable<Action> = this.actions$.pipe(
    ofType<SpecialInstructionsLoadInstructionAction>(SpecialInstructionsActionTypes.Load_Instruction),
    mapToPayload<{ specialInstructionId: number }>(),
    switchMap((payload: { specialInstructionId: number }) => {
      return this.specialInstructionsService.get(payload.specialInstructionId).pipe(
        map(data => new SpecialInstructionsLoadInstructionSuccessAction(data)),
        catchError(err => of(new SpecialInstructionsLoadInstructionFailureAction(err)))
      );
    })
  );

  @Effect({ dispatch: false })
  $groupAdd: Observable<Action> = this.actions$.pipe(
    ofType<SpecialInstructionsAddSuccessAction>(SpecialInstructionsActionTypes.Add_Success),
    tap(_ => this.messageService.add({ id: 0, detail: 'Special Instruction Added', severity: 'success' }))
  );

  @Effect({ dispatch: false })
  $groupUpdate: Observable<Action> = this.actions$.pipe(
    ofType<SpecialInstructionsUpdateSuccessAction>(SpecialInstructionsActionTypes.Update_Success),
    tap(_ => this.messageService.add({ id: 0, detail: 'Special Instruction Updated', severity: 'success' }))
  );

  @Effect({ dispatch: false })
  $groupDelete: Observable<Action> = this.actions$.pipe(
    ofType<SpecialInstructionsDeleteSuccessAction>(SpecialInstructionsActionTypes.Delete_Success),
    tap(_ => this.messageService.add({ id: 0, detail: 'Special Instruction Deleted', severity: 'success' }))
  );

  @Effect()
  $update: Observable<Action> = this.actions$.pipe(
    ofType<SpecialInstructionsUpdateAction>(SpecialInstructionsActionTypes.Update),
    mapToPayload<SpecialInstruction>(),
    switchMap((instruction: SpecialInstruction) => {
      return this.specialInstructionsService.update(instruction).pipe(
        map(data => new SpecialInstructionsUpdateSuccessAction(data)),
        catchError(err => {
          if (err && err.error && err.error.title && err.error.detail) {
            this.messageService.add({ summary: err.error.title, detail: err.error.detail, severity: 'error' });
          }
          return of(new SpecialInstructionsUpdateFailureAction(err));
        })
      );
    })
  );

  @Effect()
  $add: Observable<Action> = this.actions$.pipe(
    ofType<SpecialInstructionsAddAction>(SpecialInstructionsActionTypes.Add),
    mapToPayload<SpecialInstruction>(),
    switchMap((instruction: SpecialInstruction) => {
      return this.specialInstructionsService.add(instruction).pipe(
        map(data => new SpecialInstructionsAddSuccessAction(data)),
        catchError(err => {
          if (err && err.error && err.error.title && err.error.detail) {
            this.messageService.add({ summary: err.error.title, detail: err.error.detail, severity: 'error' });
          }
          return of(new SpecialInstructionsAddFailureAction(err));
        })
      );
    })
  );

  @Effect()
  $delete: Observable<Action> = this.actions$.pipe(
    ofType<SpecialInstructionsDeleteAction>(SpecialInstructionsActionTypes.Delete),
    mapToPayload<SpecialInstruction>(),
    switchMap((instruction: SpecialInstruction) => {
      return this.specialInstructionsService.delete(instruction).pipe(
        map(data => new SpecialInstructionsDeleteSuccessAction(instruction)),
        catchError(err => of(new SpecialInstructionsDeleteFailureAction(err)))
      );
    })
  );

  constructor(
    private actions$: Actions,
    private specialInstructionsService: SpecialInstructionsService,
    private messageService: MessageService
  ) {}
}
