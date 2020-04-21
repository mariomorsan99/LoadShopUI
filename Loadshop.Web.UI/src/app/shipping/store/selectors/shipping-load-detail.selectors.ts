import { createSelector } from '@ngrx/store';
import { getLoadingEquipment } from 'src/app/core/store';
import { IShippingLoadDetail, ShippingLoadDetail } from 'src/app/shared/models';
import { getShippingFeatureState, ShippingState } from '../reducers';
import {
  getLoading,
  getLoads,
  getPostValidationProblemDetails,
  getSuccessfullyPostedLoads,
  ShippingLoadDetailState,
} from '../reducers/shipping-load-detail.reducer';

const getShippingLoadDetailState = createSelector(getShippingFeatureState, (state: ShippingState) => state.loadDetail);

const getLoadingDetials = createSelector(getShippingLoadDetailState, (state: ShippingLoadDetailState) => getLoading(state));
export const getLoadingShippingLoadDetails = createSelector(getLoadingDetials, getLoadingEquipment, (b1: boolean, b2: boolean) => b1 || b2);

const getShippingLoads = createSelector(getShippingLoadDetailState, (state: ShippingLoadDetailState) => getLoads(state));
export const getShippingHomeLoads = createSelector(
  getShippingLoads,
  (loads: IShippingLoadDetail[]) => {
    if (!loads) {
      return null;
    }

    return loads.map(l => new ShippingLoadDetail({ ...l }));
  }
);

export const getShippingPostValidationProblemDetails = createSelector(getShippingLoadDetailState, (state: ShippingLoadDetailState) =>
  getPostValidationProblemDetails(state)
);
export const getShippingSuccessfullyPostedLoads = createSelector(getShippingLoadDetailState, (state: ShippingLoadDetailState) =>
  getSuccessfullyPostedLoads(state)
);
