import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action, Store } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { MessageService } from 'primeng/api';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { CoreState } from 'src/app/core/store';
import { MenuVisibilityBadgeLoadAction, MenuVisibilityBadgeLoadSuccessAction } from 'src/app/core/store/actions';
import { LoadView } from '../../../shared/models';
import { LoadBoardService } from '../../services';
import {
  LoadBoardBookedActionTypes,
  LoadBoardBookedLoadAction,
  LoadBoardBookedLoadFailureAction,
  LoadBoardBookedLoadSuccessAction,
  LoadBoardBookedSaveLoadAction,
  LoadBoardBookedSaveLoadFailureAction,
  LoadBoardBookedSaveLoadSuccessAction,
} from '../actions';
import { PageableQueryHelper } from 'src/app/shared/utilities';

@Injectable()
export class LoadBoardBookedEffects {
  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType<LoadBoardBookedLoadAction>(LoadBoardBookedActionTypes.Load),
    mapToPayload<{ queryHelper: PageableQueryHelper }>(),
    switchMap(payload => {
      return this.loadBoardService.getBookedLoadsByUser(payload.queryHelper).pipe(
        map(data => {
          this.menuStore.dispatch(new MenuVisibilityBadgeLoadAction());
          return new LoadBoardBookedLoadSuccessAction(data);
        }),
        catchError(err => of(new LoadBoardBookedLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $save = this.actions$.pipe(
    ofType<LoadBoardBookedSaveLoadAction>(LoadBoardBookedActionTypes.Save_Load),
    mapToPayload<LoadView>(),
    switchMap(load => {
      return this.loadBoardService.saveVisibilityData(load).pipe(
        switchMap(data => [
          new MenuVisibilityBadgeLoadSuccessAction(data.visibilityBadge),
          new LoadBoardBookedSaveLoadSuccessAction(data.loadClaim),
        ]),
        catchError(err => of(new LoadBoardBookedSaveLoadFailureAction(err)))
      );
    })
  );

  @Effect({ dispatch: false })
  $visibilitySaved: Observable<Action> = this.actions$.pipe(
    ofType<LoadBoardBookedLoadAction>(LoadBoardBookedActionTypes.Save_Load_Success),
    tap(action => this.messageService.add({ id: 0, detail: 'Visibility Data Saved', severity: 'success' }))
  );

  constructor(
    private actions$: Actions,
    private loadBoardService: LoadBoardService,
    private menuStore: Store<CoreState>,
    private messageService: MessageService
  ) {}
}
