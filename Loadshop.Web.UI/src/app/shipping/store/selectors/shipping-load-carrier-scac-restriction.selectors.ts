import { createSelector } from '@ngrx/store';
import { LoadCarrierScacRestrictionData } from 'src/app/shared/models';
import { getShippingFeatureState, ShippingState } from '../reducers';
import { getLoadCarrierScacRestrictions, getLoadsLoading } from '../reducers/shipping-load-carrier-scac-restriction.reducer';

const getShippingLoadCarrierScacRestrictionState = createSelector(
  getShippingFeatureState,
  (state: ShippingState) => state.loadCarrierScacRestriction
);

const getShippingLoadCarrierScacRestrictions = createSelector(getShippingLoadCarrierScacRestrictionState, getLoadCarrierScacRestrictions);
export const getShippingLoadCarrierScacRestrictionsForLoadId = createSelector(
  getShippingLoadCarrierScacRestrictions,
  (state: { [s: string]: LoadCarrierScacRestrictionData[] }, props) => state[props.loadId]
);

const getShippingLoadsLoading = createSelector(getShippingLoadCarrierScacRestrictionState, getLoadsLoading);
export const getShippingLoadLoadingCarrierScacRestrictionForLoadId = createSelector(
  getShippingLoadsLoading,
  (state: { [s: string]: boolean }, props) => state[props.loadId] || false
);
