import { ActionReducerMap, createFeatureSelector } from '@ngrx/store';
import { LoadBoardDashboardState, loadboardDashboardReducer } from './load-board-dashboard.reducer';
import { LoadBoardBookedState, LoadBoardBookedReducer } from './load-board-booked.reducer';
import { LoadBoardDeliveredState, LoadBoardDeliveredReducer } from './load-board-delivered.reducer';

export interface LoadBoardState {
  dashboard: LoadBoardDashboardState;
  booked: LoadBoardBookedState;
  delivered: LoadBoardDeliveredState;
}

export const reducers: ActionReducerMap<LoadBoardState> = {
  dashboard: loadboardDashboardReducer,
  booked: LoadBoardBookedReducer,
  delivered: LoadBoardDeliveredReducer,
};

export const getLoadBoardFeatureState = createFeatureSelector<LoadBoardState>('loadboard');
