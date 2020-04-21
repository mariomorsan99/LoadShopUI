import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { mapResponse } from '../operators/map-response';
import {
  QuestionData,
  ServiceResponse,
  FeedbackQuestionEnum,
  QuestionResponseData
} from '../models';

@Injectable()
export class FeedbackService {
  constructor(private http: HttpClient) {}

  getQuestion(feedbackQuestionCode: FeedbackQuestionEnum) {
    return this.http.get<ServiceResponse<QuestionData>>(
      `${environment.apiUrl}/api/feedback/${feedbackQuestionCode}`
      ).pipe(mapResponse());
  }

  getResponse(feedbackQuestionCode: FeedbackQuestionEnum, loadId: string) {
    return this.http.get<ServiceResponse<QuestionResponseData>>(
      `${environment.apiUrl}/api/feedback/` +
      `${feedbackQuestionCode}/${loadId}`
      ).pipe(mapResponse());
  }

  saveResponse(response: QuestionResponseData) {
    return this.http.post<ServiceResponse<QuestionResponseData>>(
      `${environment.apiUrl}/api/feedback`, response
      ).pipe(mapResponse());
  }
}
