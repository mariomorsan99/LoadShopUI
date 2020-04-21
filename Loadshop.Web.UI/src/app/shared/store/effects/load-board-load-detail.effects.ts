import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { LoadAudit } from 'src/app/shared/models/load-audit';
import { LoadBoardService } from '../../../load-board/services';
import { ShippingService } from '../../../shipping/services';
import { RemoveCarrierData, RemoveLoadData } from '../../models';
import {
  LoadBoardLoadAuditAction,
  LoadBoardLoadAuditFailureAction,
  LoadBoardLoadAuditSuccessAction,
  LoadBoardLoadDetailActionTypes,
  LoadBoardLoadDetailLoadAction,
  LoadBoardLoadDetailLoadFailureAction,
  LoadBoardLoadDetailLoadSuccessAction,
  LoadDetailCarrierRemovedAction,
  LoadDetailCarrierRemovedActionTypes,
  LoadDetailCarrierRemovedFailureAction,
  LoadDetailCarrierRemovedSuccessAction,
  LoadDetailDeleteLoadAction,
  LoadDetailDeleteLoadActionTypes,
  LoadDetailDeleteLoadFailureAction,
  LoadDetailDeleteLoadSuccessAction,
} from '../actions';

@Injectable()
export class LoadBoardLoadDetailEffects {
  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType<LoadBoardLoadDetailLoadAction>(LoadBoardLoadDetailActionTypes.Load),
    mapToPayload<string>(),
    switchMap(id => {
      return this.loadBoardService.getLoadById(id).pipe(
        map(data => new LoadBoardLoadDetailLoadSuccessAction(data)),
        catchError(err => of(new LoadBoardLoadDetailLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $view: Observable<Action> = this.actions$.pipe(
    ofType<LoadBoardLoadAuditAction>(LoadBoardLoadDetailActionTypes.Load_Audit),
    mapToPayload<LoadAudit>(),
    switchMap(loadAudit => {
      return this.loadBoardService.auditLoad(loadAudit).pipe(
        map(data => new LoadBoardLoadAuditSuccessAction(data)),
        catchError(err => of(new LoadBoardLoadAuditFailureAction(err)))
      );
    })
  );

  @Effect()
  $carrierRemvoed: Observable<Action> = this.actions$.pipe(
    ofType<LoadDetailCarrierRemovedAction>(LoadDetailCarrierRemovedActionTypes.CarrierRemoved),
    mapToPayload<RemoveCarrierData>(),
    switchMap(payload => {
      return this.shippingService.removeCarrier(payload).pipe(
        map(data => new LoadDetailCarrierRemovedSuccessAction(data)),
        catchError(err => of(new LoadDetailCarrierRemovedFailureAction(err)))
      );
    })
  );

  @Effect()
  $deleteLoad: Observable<Action> = this.actions$.pipe(
    ofType<LoadDetailDeleteLoadAction>(LoadDetailDeleteLoadActionTypes.DeleteLoad),
    mapToPayload<RemoveLoadData>(),
    switchMap(load => {
      return this.shippingService.deleteDetailLoad(load).pipe(
        map(data => new LoadDetailDeleteLoadSuccessAction(data)),
        catchError(err => of(new LoadDetailDeleteLoadFailureAction(err)))
      );
    })
  );

  constructor(private actions$: Actions, private loadBoardService: LoadBoardService, private shippingService: ShippingService) {}
}
