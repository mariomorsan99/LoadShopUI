import { createSelector } from '@ngrx/store';
import { ILoadAuditLogData } from 'src/app/shared/models';
import { getShippingFeatureState, ShippingState } from '../reducers';
import { getLoadAuditLogs, getLoadsLoading } from '../reducers/shipping-load-audit-log.reducer';

const getShippingLoadAuditLogState = createSelector(getShippingFeatureState, (state: ShippingState) => state.loadAuditLog);

const getShippingLoadAuditLogs = createSelector(getShippingLoadAuditLogState, getLoadAuditLogs);
export const getShippingLoadAuditLogForLoadId = createSelector(
  getShippingLoadAuditLogs,
  (state: { [s: string]: ILoadAuditLogData[] }, props) => state[props.loadId]
);

const getShippingLoadsLoading = createSelector(getShippingLoadAuditLogState, getLoadsLoading);
export const getShippingLoadLoadingAuditLogForLoadId = createSelector(
  getShippingLoadsLoading,
  (state: { [s: string]: boolean }, props) => state[props.loadId] || false
);
