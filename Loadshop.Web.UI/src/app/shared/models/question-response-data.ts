import { FeedbackQuestionEnum } from './feedback-question-enum';

export interface QuestionResponseData {
    feedbackQuestionCode: FeedbackQuestionEnum;
    answer: boolean;
    loadId: string;
}
