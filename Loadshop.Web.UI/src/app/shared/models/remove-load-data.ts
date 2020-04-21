import { Load } from '.';
import { RatingQuestionAnswer } from './rating-question-answer';

export interface RemoveLoadData {
  load: Load;
  ratingQuestionAnswer?: RatingQuestionAnswer;
}
