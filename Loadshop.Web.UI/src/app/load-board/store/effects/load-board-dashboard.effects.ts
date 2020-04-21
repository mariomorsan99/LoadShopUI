import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { Observable, of, timer } from 'rxjs';
import { catchError, map, mapTo, switchMap, takeUntil } from 'rxjs/operators';
import { UpdateFocusEntityAction, UserFocusEntitySelectorTypes } from 'src/app/user/store';
import { LoadBoardService } from '../../services';
import {
  LoadBoardDashboardActionTypes,
  LoadBoardDashboardLoadAction,
  LoadBoardDashboardLoadFailureAction,
  LoadBoardDashboardLoadSuccessAction,

} from '../actions';
import {
  LoadBoardLoadBookActionTypes,
  LoadBoardLoadBookSuccessAction
} from '../../../shared/store';

@Injectable()
export class LoadBoardDashboardEffects {
  previousUrl: string = null;
  dashboardUrl = '/loads/search';
  timerSeconds = 300;

  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType<LoadBoardDashboardLoadAction>(LoadBoardDashboardActionTypes.Load),
    mapToPayload<string>(),
    switchMap(() => {
      return this.loadBoardService.getDashboardLoadsByUser().pipe(
        map(data => new LoadBoardDashboardLoadSuccessAction(data)),
        catchError(err => of(new LoadBoardDashboardLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadBooked: Observable<Action> = this.actions$.pipe(
    ofType<LoadBoardLoadBookSuccessAction>(LoadBoardLoadBookActionTypes.Book_Success),
    map(() => new LoadBoardDashboardLoadAction())
  );

  @Effect()
  $startPolling: Observable<Action> = this.actions$.pipe(
    ofType(LoadBoardDashboardActionTypes.Start_Polling),
    switchMap(() =>
      timer(0, this.timerSeconds * 1000).pipe(
        takeUntil(this.actions$.pipe(ofType(LoadBoardDashboardActionTypes.Cancel_Polling))),
        mapTo(new LoadBoardDashboardLoadAction())
      )
    )
  );

  @Effect()
  $clearAll: Observable<Action> = this.actions$.pipe(
    ofType<UpdateFocusEntityAction>(UserFocusEntitySelectorTypes.UpdateFocusEntity),
    switchMap(() => of(new LoadBoardDashboardLoadSuccessAction([])))
  );

  constructor(private actions$: Actions, private loadBoardService: LoadBoardService) { }
}
