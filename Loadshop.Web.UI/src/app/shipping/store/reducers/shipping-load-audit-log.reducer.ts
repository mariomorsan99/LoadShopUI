import { ILoadAuditLogData } from 'src/app/shared/models/load-audit-log-data';
import {
  ShippingLoadAuditLogActions,
  ShippingLoadAuditLogActionTypes,
  ShippingLoadDetailActions,
  ShippingLoadDetailActionTypes,
} from '../actions';

export interface ShippingLoadAuditLogState {
  loadsLoading: { [s: string]: boolean };
  loadAuditLogs: { [s: string]: ILoadAuditLogData[] };
}

const initialState: ShippingLoadAuditLogState = {
  loadsLoading: {},
  loadAuditLogs: {},
};

// tslint:disable-next-line:no-use-before-declare
const deleteLoadId = ({ [loadId]: _, ...state }, loadId: string) => state;
export function ShippingLoadAuditLogReducer(
  state: ShippingLoadAuditLogState = initialState,
  action: ShippingLoadAuditLogActions | ShippingLoadDetailActions
): ShippingLoadAuditLogState {
  switch (action.type) {
    case ShippingLoadAuditLogActionTypes.Load_Audit_Logs_Load: {
      return {
        ...state,
        loadAuditLogs: deleteLoadId(state.loadAuditLogs, action.payload.loadId),
        loadsLoading: { ...state.loadsLoading, ...{ [action.payload.loadId]: true } },
      };
    }
    case ShippingLoadAuditLogActionTypes.Load_Audit_Logs_Load_Success: {
      return {
        ...state,
        loadAuditLogs: { ...state.loadAuditLogs, ...{ [action.payload.loadId]: action.payload.logs } },
        loadsLoading: { ...state.loadsLoading, ...{ [action.payload.loadId]: false } },
      };
    }
    case ShippingLoadAuditLogActionTypes.Load_Audit_Logs_Load_Failure: {
      return { ...state, loadsLoading: { ...state.loadsLoading, ...{ [action.payload.loadId]: false } } };
    }
    case ShippingLoadDetailActionTypes.Delete_Load_Success: {
      return {
        ...state,
        loadAuditLogs: deleteLoadId(state.loadAuditLogs, action.payload.loadId),
        loadsLoading: { ...state.loadsLoading, ...{ [action.payload.loadId]: false } },
      };
    }
    default:
      return state;
  }
}

export const getLoadAuditLogs = (state: ShippingLoadAuditLogState) => state.loadAuditLogs;
export const getLoadsLoading = (state: ShippingLoadAuditLogState) => state.loadsLoading;
