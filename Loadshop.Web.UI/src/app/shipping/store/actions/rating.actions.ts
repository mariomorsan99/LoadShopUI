/* eslint-disable @typescript-eslint/camelcase */
import { HttpErrorResponse } from '@angular/common/http';
import { Action } from '@ngrx/store';
import { RatingQuestion } from '../../../shared/models';

export enum RatingActionTypes {
  Get_Rating_Questions = '[Rating] GET_RATING_QUESTIONS',
  Get_Ratings_Questions_Success = '[Rating] GET_RATING_QUESTIONS_SUCCESS',
  Get_Ratings_Questions_Failure = '[Rating] GET_RATING_QUESTIONS_FAILURE',
}

export class RatingGetQuestionsAction implements Action {
  readonly type = RatingActionTypes.Get_Rating_Questions;
}

export class RatingGetQuestionsSuccessAction implements Action {
  readonly type = RatingActionTypes.Get_Ratings_Questions_Success;
  constructor(public payload: RatingQuestion[]) {}
}

export class RatingGetQuestionsFailureAction implements Action {
  readonly type = RatingActionTypes.Get_Ratings_Questions_Failure;
  constructor(public payload: HttpErrorResponse) {}
}

export type RatingActions = RatingGetQuestionsAction | RatingGetQuestionsSuccessAction | RatingGetQuestionsFailureAction;
