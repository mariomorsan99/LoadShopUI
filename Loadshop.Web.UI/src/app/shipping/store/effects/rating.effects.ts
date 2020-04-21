import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { Observable, of } from 'rxjs';
import { catchError, map, mergeMap } from 'rxjs/operators';
import { RatingService } from '../../services/rating.service';
import { RatingActionTypes, RatingGetQuestionsAction, RatingGetQuestionsFailureAction, RatingGetQuestionsSuccessAction } from '../actions';

@Injectable()
export class RatingEffects {
  @Effect()
  $getRatingQuestion: Observable<Action> = this.actions$.pipe(
    ofType<RatingGetQuestionsAction>(RatingActionTypes.Get_Rating_Questions),
    mergeMap(() => {
      return this.ratingService.getRatingQuestions().pipe(
        map(data => new RatingGetQuestionsSuccessAction(data)),
        catchError(err => of(new RatingGetQuestionsFailureAction(err)))
      );
    })
  );

  constructor(private actions$: Actions, private ratingService: RatingService) {}
}
