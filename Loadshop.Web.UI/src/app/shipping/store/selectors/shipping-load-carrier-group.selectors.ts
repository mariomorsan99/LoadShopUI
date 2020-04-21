import { createSelector } from '@ngrx/store';
import { ShippingLoadCarrierGroupData } from 'src/app/shared/models';
import { getShippingFeatureState, ShippingState } from '../reducers';
import { getLoadCarrierGroups, getLoadsLoading } from '../reducers/shipping-load-carrier-group.reducer';

const getShippingLoadCarrierGroupState = createSelector(getShippingFeatureState, (state: ShippingState) => state.loadCarrierGroup);

const getShippingLoadCarrierGroups = createSelector(getShippingLoadCarrierGroupState, getLoadCarrierGroups);
export const getShippingLoadCarrierGroupsForLoadId = createSelector(
  getShippingLoadCarrierGroups,
  (state: { [s: string]: ShippingLoadCarrierGroupData[] }, props) => state[props.loadId]
);

const getShippingLoadsLoading = createSelector(getShippingLoadCarrierGroupState, getLoadsLoading);
export const getShippingLoadLoadingCarrierGroupForLoadId = createSelector(
  getShippingLoadsLoading,
  (state: { [s: string]: boolean }, props) => state[props.loadId] || false
);
