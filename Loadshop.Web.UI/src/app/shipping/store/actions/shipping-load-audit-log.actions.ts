/* eslint-disable @typescript-eslint/camelcase */
import { Action } from '@ngrx/store';
import { ILoadAuditLogData } from 'src/app/shared/models/load-audit-log-data';

export enum ShippingLoadAuditLogActionTypes {
  Load_Audit_Logs_Load = '[ShippingLoadDetail] LOAD_AUDIT_LOGS_LOAD',
  Load_Audit_Logs_Load_Success = '[ShippingLoadDetail] LOAD_AUDIT_LOGS_LOAD_SUCCESS',
  Load_Audit_Logs_Load_Failure = '[ShippingLoadDetail] LOAD_AUDIT_LOGS_LOAD_FAILURE',
}

export class ShippingLoadAuditLogsLoadAction implements Action {
  readonly type = ShippingLoadAuditLogActionTypes.Load_Audit_Logs_Load;

  constructor(public payload: { loadId: string }) {}
}

export class ShippingLoadAuditLogsLoadSuccessAction implements Action {
  readonly type = ShippingLoadAuditLogActionTypes.Load_Audit_Logs_Load_Success;

  constructor(public payload: { loadId: string; logs: ILoadAuditLogData[] }) {}
}

export class ShippingLoadAuditLogsLoadFailureAction implements Action {
  readonly type = ShippingLoadAuditLogActionTypes.Load_Audit_Logs_Load_Failure;

  constructor(public payload: { loadId: string; error: Error }) {}
}

export type ShippingLoadAuditLogActions =
  | ShippingLoadAuditLogsLoadAction
  | ShippingLoadAuditLogsLoadSuccessAction
  | ShippingLoadAuditLogsLoadFailureAction;
