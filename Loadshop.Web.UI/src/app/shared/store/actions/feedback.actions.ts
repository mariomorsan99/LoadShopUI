import { Action } from '@ngrx/store';
import {
  FeedbackQuestionEnum,
  QuestionResponseData,
  QuestionData
} from '../../models';

export enum FeedbackActionTypes {
  LoadQuestion = '[Feedback] Load Question',
  LoadQuestionSuccess = '[Feedback] Load Question Success',
  LoadQuestionFailure = '[Feedback] Load Question Failure',
  LoadResponse = '[Feedback] Load Response',
  LoadResponseSuccess = '[Feedback] Load Response Success',
  LoadResponseFailure = '[Feedback] Load Response Failure',
  SaveResponse = '[Feedback] Save Response',
  SaveResponseSuccess = '[Feedback] Save Response Success',
  SaveResponseFailure = '[Feedback] Save Response Failure',
}

export class LoadFeedbackQuestionAction implements Action {
  readonly type = FeedbackActionTypes.LoadQuestion;

  constructor(public payload: FeedbackQuestionEnum) { }
}

export class LoadFeedbackQuestionSuccessAction implements Action {
  readonly type = FeedbackActionTypes.LoadQuestionSuccess;

  constructor(public payload: QuestionData) { }
}

export class LoadFeedbackQuestionFailureAction implements Action {
  readonly type = FeedbackActionTypes.LoadQuestionFailure;

  constructor(public payload: Error) { }
}

export class LoadFeedbackResponseAction implements Action {
  readonly type = FeedbackActionTypes.LoadResponse;

  constructor(public payload: {
    feedbackQuestionCode: FeedbackQuestionEnum,
    loadId: string
  }) { }
}

export class LoadFeedbackResponseSuccessAction implements Action {
  readonly type = FeedbackActionTypes.LoadResponseSuccess;

  constructor(public payload: QuestionResponseData) { }
}

export class LoadFeedbackResponseFailureAction implements Action {
  readonly type = FeedbackActionTypes.LoadResponseFailure;

  constructor(public payload: Error) { }
}

export class SaveFeedbackResponseAction implements Action {
  readonly type = FeedbackActionTypes.SaveResponse;

  constructor(public payload: QuestionResponseData) { }
}

export class SaveFeedbackResponseSuccessAction implements Action {
  readonly type = FeedbackActionTypes.SaveResponseSuccess;

  constructor(public payload: QuestionResponseData) { }
}

export class SaveFeedbackResponseFailureAction implements Action {
  readonly type = FeedbackActionTypes.SaveResponseFailure;

  constructor(public payload: Error) { }
}

export type FeedbackActions =
  LoadFeedbackQuestionAction |
  LoadFeedbackQuestionSuccessAction |
  LoadFeedbackQuestionFailureAction |
  LoadFeedbackResponseAction |
  LoadFeedbackResponseSuccessAction |
  LoadFeedbackResponseFailureAction |
  SaveFeedbackResponseAction |
  SaveFeedbackResponseSuccessAction |
  SaveFeedbackResponseFailureAction
;
