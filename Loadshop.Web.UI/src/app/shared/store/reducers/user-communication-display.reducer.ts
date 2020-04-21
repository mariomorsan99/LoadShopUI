import { UserCommunication } from '../../models';
import { UserCommunicationDisplayActions, UserCommunicationDisplayActionTypes } from '../actions';


export interface UserCommuncationDisplayState {
    userCommunicationsLoading: boolean;
    userCommunications: UserCommunication[];
    acknowledgePosting: boolean;
}
const initialState: UserCommuncationDisplayState = {
    userCommunicationsLoading: false,
    userCommunications: [],
    acknowledgePosting: false
};

export function UserCommunicationDisplayReducer(state: UserCommuncationDisplayState = initialState, action: UserCommunicationDisplayActions)
    : UserCommuncationDisplayState {
    switch (action.type) {
        case UserCommunicationDisplayActionTypes.Load: {
            return { ...state, userCommunicationsLoading: true };
        }
        case UserCommunicationDisplayActionTypes.Load_Success: {
            return { ...state, userCommunicationsLoading: false, userCommunications: action.payload };
        }
        case UserCommunicationDisplayActionTypes.Load_Failure: {
            return { ...state, userCommunicationsLoading: false, userCommunications: [] };
        }

        case UserCommunicationDisplayActionTypes.Acknowledge: {
            return { ...state, acknowledgePosting: true };
        }
        case UserCommunicationDisplayActionTypes.Acknowledge_Success: {
            return { ...state, acknowledgePosting: false, userCommunications: action.payload };
        }
        case UserCommunicationDisplayActionTypes.Acknowledge_Failure: {
            return { ...state, acknowledgePosting: false };
        }
        default:
            return state;
    }
}
