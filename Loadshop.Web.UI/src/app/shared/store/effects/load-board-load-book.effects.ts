import { Injectable } from '@angular/core';

import { Observable, of } from 'rxjs';
import { Effect, Actions, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';

import { map, switchMap, catchError, tap } from 'rxjs/operators';

import {
  LoadBoardLoadBookAction,
  LoadBoardLoadBookActionTypes,
  LoadBoardLoadBookSuccessAction,
  LoadBoardLoadBookFailureAction
} from '../actions';

import { LoadBoardService } from '../../../load-board/services';
import { mapToPayload } from '@tms-ng/shared';
import { Load } from '../../models';
import { MessageService } from 'primeng/api';

@Injectable()
export class LoadBoardLoadBookEffects {
  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType<LoadBoardLoadBookAction>(LoadBoardLoadBookActionTypes.Book),
    mapToPayload<Load>(),
    switchMap((load) => {
      return this.loadBoardService.update(load).pipe(
        map((data) => new LoadBoardLoadBookSuccessAction(data)),
        catchError((err) => of(new LoadBoardLoadBookFailureAction(err)))
      );
    })
  );

  @Effect({ dispatch: false })
  $booked: Observable<Action> = this.actions$.pipe(
    ofType<LoadBoardLoadBookSuccessAction>(LoadBoardLoadBookActionTypes.Book_Success),
    tap(action => this.messageService.add({ id: 0, detail: 'Load Booked', severity: 'success' }))
  );

  constructor(private actions$: Actions, private loadBoardService: LoadBoardService, private messageService: MessageService) { }
}
