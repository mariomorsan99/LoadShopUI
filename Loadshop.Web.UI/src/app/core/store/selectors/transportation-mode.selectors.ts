import { createSelector } from '@ngrx/store';
import { CoreState } from '../reducers';
import { getEntities, getLoading } from '../reducers/transportation-mode.reducer';

export const getTransportationModeState = (state: CoreState) => state.transportationMode;
export const getTransporationModes = createSelector(getTransportationModeState, getEntities);
export const getLoadingTransporationModes = createSelector(getTransportationModeState, getLoading);
