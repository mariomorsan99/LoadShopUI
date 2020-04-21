import { createSelector } from '@ngrx/store';

import { CoreState } from '../reducers';
import { getIsMobile } from '../reducers/browser.reducer';

export const getBrowserState = (state: CoreState) => state.browser;
export const getBrowserIsMobile = createSelector(getBrowserState, getIsMobile);
