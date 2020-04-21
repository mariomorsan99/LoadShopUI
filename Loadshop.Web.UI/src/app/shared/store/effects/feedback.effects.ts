import { Injectable } from '@angular/core';

import { Observable, of } from 'rxjs';
import { Effect, Actions, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';

import { map, switchMap, catchError } from 'rxjs/operators';

import {
  FeedbackActionTypes,
  LoadFeedbackQuestionAction,
  LoadFeedbackQuestionSuccessAction,
  LoadFeedbackQuestionFailureAction,
  SaveFeedbackResponseAction,
  SaveFeedbackResponseSuccessAction,
  SaveFeedbackResponseFailureAction,
  LoadFeedbackResponseAction,
  LoadFeedbackResponseSuccessAction,
  LoadFeedbackResponseFailureAction,
} from '../actions';

import { mapToPayload } from '@tms-ng/shared';
import { FeedbackQuestionEnum, QuestionResponseData } from '../../models';
import { FeedbackService } from '../../services/feedback.service';

@Injectable()
export class FeedbackEffects {
  @Effect()
  $loadQuestion: Observable<Action> = this.actions$.pipe(
    ofType<LoadFeedbackQuestionAction>(FeedbackActionTypes.LoadQuestion),
    mapToPayload<FeedbackQuestionEnum>(),
    switchMap((feedbackQuestion) => {
      return this.feedbackService.getQuestion(feedbackQuestion).pipe(
        map((data) => new LoadFeedbackQuestionSuccessAction(data)),
        catchError((err) => of(new LoadFeedbackQuestionFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadResponse: Observable<Action> = this.actions$.pipe(
    ofType<LoadFeedbackResponseAction>(FeedbackActionTypes.LoadResponse),
    mapToPayload<{ feedbackQuestionCode: FeedbackQuestionEnum, loadId: string }>(),
    switchMap((request) => {
      return this.feedbackService.getResponse(
        request.feedbackQuestionCode,
        request.loadId
      ).pipe(
        map((data) => new LoadFeedbackResponseSuccessAction(data)),
        catchError((err) => of(new LoadFeedbackResponseFailureAction(err)))
      );
    })
  );

  @Effect()
  $saveResponse: Observable<Action> = this.actions$.pipe(
    ofType<SaveFeedbackResponseAction>(FeedbackActionTypes.SaveResponse),
    mapToPayload<QuestionResponseData>(),
    switchMap((response) => {
      return this.feedbackService.saveResponse(response).pipe(
        map((data) => new SaveFeedbackResponseSuccessAction(data)),
        catchError((err) => of(new SaveFeedbackResponseFailureAction(err)))
      );
    })
  );

  constructor(private actions$: Actions, private feedbackService: FeedbackService) { }
}
