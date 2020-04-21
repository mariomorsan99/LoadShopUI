import { RatingQuestion } from '../../../shared/models/rating-question';
import { RatingActions, RatingActionTypes } from '../actions';

export interface RatingState {
  loaded: boolean;
  loading: boolean;
  questions: RatingQuestion[];
}

const initialState: RatingState = {
  loaded: false,
  loading: false,
  questions: null,
};
export function RatingReducer(state: RatingState = initialState, action: RatingActions): RatingState {
  switch (action.type) {
    case RatingActionTypes.Get_Rating_Questions:
      return { ...state, loading: true, loaded: false };
    case RatingActionTypes.Get_Ratings_Questions_Success:
      return { ...state, questions: action.payload, loading: false, loaded: true };
    case RatingActionTypes.Get_Ratings_Questions_Failure:
      return { ...state, loading: false, loaded: false };
    default:
      return state;
  }
}

export const getLoaded = (state: RatingState) => state.loaded;
export const getLoading = (state: RatingState) => state.loading;
