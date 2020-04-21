import { createSelector } from '@ngrx/store';
import { CoreState } from '../reducers';
import { getEntities, getLoading } from '../reducers/unit-of-measure.reducer';

export const getUnitOfMeasureState = (state: CoreState) => state.unitOfMeasure;
export const getUnitsOfMeasure = createSelector(getUnitOfMeasureState, getEntities);
export const getLoadingUnitsOfMeasure = createSelector(getUnitOfMeasureState, getLoading);
