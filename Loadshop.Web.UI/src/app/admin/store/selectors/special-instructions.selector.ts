import { createSelector } from '@ngrx/store';
import { AdminState, getAdminFeatureState } from '../reducers';
import { selectors, SpecialInstructionsState } from '../reducers/special-instructions.reducer';

const getSpecialInstructionsState = createSelector(getAdminFeatureState, (state: AdminState) => state.specialInstructions);

export const getSpecialInstructions = createSelector(getSpecialInstructionsState, (state: SpecialInstructionsState) =>
  selectors.selectAll(state)
);
export const getLoadingSpecialInstructions = createSelector(
  getSpecialInstructionsState,
  (state: SpecialInstructionsState) => state.loading
);

export const getSelectedSpecialInstructions = createSelector(
  getSpecialInstructionsState,
  (state: SpecialInstructionsState) => state.selectedSpecialInstructions
);
export const getLoadingSelectedSpecialInstructions = createSelector(
  getSpecialInstructionsState,
  (state: SpecialInstructionsState) => state.loadingSelectedSpecialInstructions
);

export const getSavingSpecialInstructions = createSelector(
  getSpecialInstructionsState,
  (state: SpecialInstructionsState) => state.savingSpecialInstructions
);
export const getSaveSpecialInstructionsSucceeded = createSelector(
  getSpecialInstructionsState,
  (state: SpecialInstructionsState) => state.savingSpecialInstructionsSucceeded
);

export const getSaveSpecialInstructionsProblemDetails = createSelector(
  getSpecialInstructionsState,
  (state: SpecialInstructionsState) => state.savingSpecialInstructionsProblemDetails
);
