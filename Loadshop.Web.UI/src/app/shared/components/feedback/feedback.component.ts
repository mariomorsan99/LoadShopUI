import { Component, OnInit, Input, OnChanges, SimpleChanges, ChangeDetectionStrategy } from '@angular/core';
import { Store, select } from '@ngrx/store';
import {
  QuestionData,
  QuestionResponseData,
  FeedbackQuestionEnum
} from '../../models';
import {
  SharedState,
  LoadFeedbackQuestionAction,
  SaveFeedbackResponseAction,
  LoadFeedbackResponseAction
} from '../../store';
import { Observable, combineLatest } from 'rxjs';
import {
  getFeedbackQuestion,
  getFeedbackResponse,
  getLoadingQuestion,
  getLoadingResponse,
  getSavingResponse,
} from '../../store/selectors/feedback.selectors';
import { map } from 'rxjs/operators';
import * as moment from 'moment';

@Component({
  selector: 'kbxl-feedback',
  templateUrl: './feedback.component.html',
  styleUrls: ['./feedback.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FeedbackComponent implements OnInit, OnChanges {
  @Input() feedbackQuestionCode: FeedbackQuestionEnum;
  @Input() loadId: string;
  @Input() deliveredDate: Date;
  @Input() header: string;

  question$: Observable<QuestionData>;
  response$: Observable<QuestionResponseData>;
  processing$: Observable<boolean>;
  loadingResponse$: Observable<boolean>;

  get responseAllowed() {
    const today = moment().startOf('day');
    const daysDiff = today.diff(moment(this.deliveredDate), 'days');
    return daysDiff <= 3;
  }

  constructor(private store: Store<SharedState>) {}

  ngOnInit() {
    this.question$ = this.store.pipe(select(getFeedbackQuestion));
    this.response$ = this.store.pipe(
      select(getFeedbackResponse),
      map(_ => _ !== null ? _ : {
        feedbackQuestionCode: this.feedbackQuestionCode,
        answer: null,
        loadId: this.loadId
      } as QuestionResponseData)
    );
    this.processing$ = combineLatest(
      this.store.pipe(select(getLoadingQuestion)),
      this.store.pipe(select(getLoadingResponse)),
      this.store.pipe(select(getSavingResponse))
    ).pipe(
      map(([loadingQuestion, loadingResponse, savingResponse]) =>
        loadingQuestion || loadingResponse || savingResponse)
    );
    this.loadingResponse$ = this.store.pipe(select(getLoadingResponse));
  }

  ngOnChanges(changes: SimpleChanges) {
    if ((changes.feedbackQuestionCode || changes.loadId)
      && this.feedbackQuestionCode && this.loadId) {
      this.store.dispatch(new LoadFeedbackQuestionAction(this.feedbackQuestionCode));
      this.store.dispatch(new LoadFeedbackResponseAction({
        feedbackQuestionCode: this.feedbackQuestionCode,
        loadId: this.loadId
      }));
    }
  }

  saveResponse(response: QuestionResponseData) {
    this.store.dispatch(new SaveFeedbackResponseAction(response));
  }
}
