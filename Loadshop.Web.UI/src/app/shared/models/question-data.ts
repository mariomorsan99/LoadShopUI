import { QuestionReasonData } from './question-reason-data';
import { FeedbackQuestionEnum } from './feedback-question-enum';

export interface QuestionData {
    feedbackQuestionCode: FeedbackQuestionEnum;
    questionId: number;
    applicationCode: string;
    category: string;
    questionText: string;
    description: string;
    isActive: boolean;
    questionReasons: QuestionReasonData[];
    updateUser: string;
}
