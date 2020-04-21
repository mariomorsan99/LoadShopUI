import { QuestionData, QuestionResponseData } from '../../models';
import { FeedbackActions, FeedbackActionTypes } from '../actions';

export interface FeedbackState {
    loadingQuestion: boolean;
    loadingResponse: boolean;
    savingResponse: boolean;
    question: QuestionData;
    response: QuestionResponseData;
}

const initialState: FeedbackState = {
    loadingQuestion: false,
    loadingResponse: false,
    savingResponse: false,
    question: null,
    response: null,
};

export function feedbackReducer(
    state: FeedbackState = initialState,
     action: FeedbackActions
): FeedbackState {
    switch (action.type) {
        case FeedbackActionTypes.LoadQuestion: {
            return { ...state, loadingQuestion: true, question: null };
        }
        case FeedbackActionTypes.LoadQuestionSuccess: {
            return { ...state, loadingQuestion: false, question: action.payload };
        }
        case FeedbackActionTypes.LoadQuestionFailure: {
            return { ...state, loadingQuestion: false };
        }

        case FeedbackActionTypes.LoadResponse: {
            return { ...state, loadingResponse: true, response: null };
        }
        case FeedbackActionTypes.LoadResponseSuccess: {
            return { ...state, loadingResponse: false, response: action.payload };
        }
        case FeedbackActionTypes.LoadResponseFailure: {
            return { ...state, loadingResponse: false };
        }

        case FeedbackActionTypes.SaveResponse: {
            return { ...state, savingResponse: true };
        }
        case FeedbackActionTypes.SaveResponseSuccess: {
            return { ...state, savingResponse: false, response: action.payload };
        }
        case FeedbackActionTypes.SaveResponseFailure: {
            return { ...state, savingResponse: false };
        }

        default:
            return state;
    }
}
