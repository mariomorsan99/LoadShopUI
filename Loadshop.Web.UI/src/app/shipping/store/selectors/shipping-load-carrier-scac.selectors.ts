import { createSelector } from '@ngrx/store';
import { LoadCarrierScacData } from 'src/app/shared/models';
import { getShippingFeatureState, ShippingState } from '../reducers';
import { getLoadCarrierScacs, getLoadsLoading } from '../reducers/shipping-load-carrier-scac.reducer';

const getShippingLoadCarrierScacState = createSelector(getShippingFeatureState, (state: ShippingState) => state.loadCarrierScac);

const getShippingLoadCarrierScacs = createSelector(getShippingLoadCarrierScacState, getLoadCarrierScacs);
export const getShippingLoadCarrierScacsForLoadId = createSelector(
  getShippingLoadCarrierScacs,
  (state: { [s: string]: LoadCarrierScacData[] }, props) => state[props.loadId]
);

const getShippingLoadsLoading = createSelector(getShippingLoadCarrierScacState, getLoadsLoading);
export const getShippingLoadLoadingCarrierScacForLoadId = createSelector(
  getShippingLoadsLoading,
  (state: { [s: string]: boolean }, props) => state[props.loadId] || false
);
