import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { MessageService } from 'primeng/api';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { LoadCarrierGroup } from '../../../shared/models';
import { LoadCarrierGroupService } from '../../services';
import {
  LoadCarrierGroupActionTypes,
  LoadCarrierGroupAddAction,
  LoadCarrierGroupAddFailureAction,
  LoadCarrierGroupAddSuccessAction,
  LoadCarrierGroupCarrierLoadAction,
  LoadCarrierGroupCopyCarriersFailureAction,
  LoadCarrierGroupCopyCarriersSuccessAction,
  LoadCarrierGroupDeleteAction,
  LoadCarrierGroupDeleteFailureAction,
  LoadCarrierGroupDeleteSuccessAction,
  LoadCarrierGroupLoadAction,
  LoadCarrierGroupLoadCarrierGroupTypesAction,
  LoadCarrierGroupLoadCarrierGroupTypesFailure,
  LoadCarrierGroupLoadCarrierGroupTypesSuccessAction,
  LoadCarrierGroupLoadFailureAction,
  LoadCarrierGroupLoadGroupAction,
  LoadCarrierGroupLoadGroupFailureAction,
  LoadCarrierGroupLoadGroupSuccessAction,
  LoadCarrierGroupLoadSuccessAction,
  LoadCarrierGroupUpdateAction,
  LoadCarrierGroupUpdateFailureAction,
  LoadCarrierGroupUpdateSuccessAction,
} from '../actions';

@Injectable()
export class LoadCarrierGroupEffects {
  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType<LoadCarrierGroupLoadAction>(LoadCarrierGroupActionTypes.Load),
    mapToPayload<{ customerId: string }>(),
    switchMap((payload: { customerId: string }) => {
      return this.loadCarrierGroupService.getGroups(payload.customerId).pipe(
        map(data => new LoadCarrierGroupLoadSuccessAction(data)),
        catchError(err => of(new LoadCarrierGroupLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadGroup: Observable<Action> = this.actions$.pipe(
    ofType<LoadCarrierGroupLoadGroupAction>(LoadCarrierGroupActionTypes.Load_Group),
    mapToPayload<{ loadCarrierGroupId: number }>(),
    switchMap((payload: { loadCarrierGroupId: number }) => {
      return this.loadCarrierGroupService.getGroup(payload.loadCarrierGroupId).pipe(
        map(data => new LoadCarrierGroupLoadGroupSuccessAction(data)),
        catchError(err => of(new LoadCarrierGroupLoadGroupFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadCarrierGroupTypes: Observable<Action> = this.actions$.pipe(
    ofType<LoadCarrierGroupLoadCarrierGroupTypesAction>(LoadCarrierGroupActionTypes.Load_Carrier_Group_Types),
    switchMap(() => {
      return this.loadCarrierGroupService.getLoadCarrierGroupTypes().pipe(
        map(data => new LoadCarrierGroupLoadCarrierGroupTypesSuccessAction(data)),
        catchError(err => of(new LoadCarrierGroupLoadCarrierGroupTypesFailure(err)))
      );
    })
  );

  @Effect({ dispatch: false })
  $groupAdd: Observable<Action> = this.actions$.pipe(
    ofType<LoadCarrierGroupAddSuccessAction>(LoadCarrierGroupActionTypes.Add_Success),
    tap(_ => this.messageService.add({ id: 0, detail: 'Carrier Group Added', severity: 'success' }))
  );

  @Effect({ dispatch: false })
  $groupUpdate: Observable<Action> = this.actions$.pipe(
    ofType<LoadCarrierGroupUpdateSuccessAction>(LoadCarrierGroupActionTypes.Update_Success),
    tap(_ => this.messageService.add({ id: 0, detail: 'Carrier Group Updated', severity: 'success' }))
  );

  @Effect({ dispatch: false })
  $groupDelete: Observable<Action> = this.actions$.pipe(
    ofType<LoadCarrierGroupDeleteSuccessAction>(LoadCarrierGroupActionTypes.Delete_Success),
    tap(_ => this.messageService.add({ id: 0, detail: 'Carrier Group Deleted', severity: 'success' }))
  );

  @Effect()
  $update: Observable<Action> = this.actions$.pipe(
    ofType<LoadCarrierGroupUpdateAction>(LoadCarrierGroupActionTypes.Update),
    mapToPayload<LoadCarrierGroup>(),
    switchMap((group: LoadCarrierGroup) => {
      return this.loadCarrierGroupService.updateGroup(group).pipe(
        map(data => new LoadCarrierGroupUpdateSuccessAction(data)),
        catchError(err => {
          if (err && err.error && err.error.title && err.error.detail) {
            this.messageService.add({ summary: err.error.title, detail: err.error.detail, severity: 'error' });
          }
          return of(new LoadCarrierGroupUpdateFailureAction(err));
        })
      );
    })
  );

  @Effect()
  $add: Observable<Action> = this.actions$.pipe(
    ofType<LoadCarrierGroupAddAction>(LoadCarrierGroupActionTypes.Add),
    mapToPayload<LoadCarrierGroup>(),
    switchMap((group: LoadCarrierGroup) => {
      return this.loadCarrierGroupService.addGroup(group).pipe(
        map(data => new LoadCarrierGroupAddSuccessAction(data)),
        catchError(err => {
          if (err && err.error && err.error.title && err.error.detail) {
            this.messageService.add({ summary: err.error.title, detail: err.error.detail, severity: 'error' });
          }
          return of(new LoadCarrierGroupAddFailureAction(err));
        })
      );
    })
  );

  @Effect()
  $delete: Observable<Action> = this.actions$.pipe(
    ofType<LoadCarrierGroupDeleteAction>(LoadCarrierGroupActionTypes.Delete),
    mapToPayload<LoadCarrierGroup>(),
    switchMap((group: LoadCarrierGroup) => {
      return this.loadCarrierGroupService.deleteGroup(group).pipe(
        map(data => new LoadCarrierGroupDeleteSuccessAction(group)),
        catchError(err => of(new LoadCarrierGroupDeleteFailureAction(err)))
      );
    })
  );

  @Effect()
  $copyCarriers: Observable<Action> = this.actions$.pipe(
    ofType<LoadCarrierGroupCarrierLoadAction>(LoadCarrierGroupActionTypes.Copy_Carriers_Load),
    mapToPayload<{ loadCarrierGroupId: number }>(),
    switchMap((payload: { loadCarrierGroupId: number }) => {
      return this.loadCarrierGroupService.getGroup(payload.loadCarrierGroupId).pipe(
        map((group: LoadCarrierGroup) => {
          const carriers = group.carriers.map(x => ({
            loadCarrierGroupCarrierId: 0,
            loadCarrierGroupId: 0,
            carrierId: x.carrierId,
          }));

          return new LoadCarrierGroupCopyCarriersSuccessAction(carriers);
        }),
        catchError(err => of(new LoadCarrierGroupCopyCarriersFailureAction(err)))
      );
    })
  );

  constructor(
    private actions$: Actions,
    private loadCarrierGroupService: LoadCarrierGroupService,
    private messageService: MessageService
  ) {}
}
