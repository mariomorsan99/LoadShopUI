import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { Observable, of } from 'rxjs';
import { catchError, map, mergeMap } from 'rxjs/operators';
import { ShippingService } from '../../services';
import {
  ShippingLoadAuditLogActionTypes,
  ShippingLoadAuditLogsLoadAction,
  ShippingLoadAuditLogsLoadFailureAction,
  ShippingLoadAuditLogsLoadSuccessAction,
} from '../actions';

@Injectable()
export class ShippingLoadAuditLogEffects {
  @Effect()
  $loadGroups: Observable<Action> = this.actions$.pipe(
    ofType<ShippingLoadAuditLogsLoadAction>(ShippingLoadAuditLogActionTypes.Load_Audit_Logs_Load),
    mapToPayload<{ loadId: string }>(),
    // use mergeMap to allow the logs for multiple loads to be loaded at once
    mergeMap((payload: { loadId: string }) => {
      return this.shippingService.getLoadAuditLogs(payload.loadId).pipe(
        map(data => new ShippingLoadAuditLogsLoadSuccessAction({ loadId: payload.loadId, logs: data })),
        catchError(err => of(new ShippingLoadAuditLogsLoadFailureAction(err)))
      );
    })
  );

  constructor(private actions$: Actions, private shippingService: ShippingService) {}
}
