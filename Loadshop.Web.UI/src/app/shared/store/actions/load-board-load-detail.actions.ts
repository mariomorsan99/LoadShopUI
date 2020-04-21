/* eslint-disable @typescript-eslint/camelcase */
import { Action } from '@ngrx/store';
import { LoadDetail } from '../../models';
import { LoadAudit } from 'src/app/shared/models/load-audit';

export enum LoadBoardLoadDetailActionTypes {
    Load = '[LoadBoardLoadDetail] LOAD',
    Load_Success = '[LoadBoardLoadDetail] LOAD_SUCCESS',
    Load_Failure = '[LoadBoardLoadDetail] LOAD_FAILURE',
    Load_Audit = '[LoadBoardLoadDetail] LOAD_AUDIT',
    Load_Audit_Success = '[LoadBoardLoadDetail] LOAD_AUDIT_SUCCESS',
    Load_Audit_Failure = '[LoadBoardLoadDetail] LOAD_AUDIT_FAILURE'
}

export class LoadBoardLoadDetailLoadAction implements Action {
    readonly type = LoadBoardLoadDetailActionTypes.Load;

    constructor(public payload: string) { }
}

export class LoadBoardLoadDetailLoadSuccessAction implements Action {
    readonly type = LoadBoardLoadDetailActionTypes.Load_Success;

    constructor(public payload: LoadDetail) { }
}

export class LoadBoardLoadDetailLoadFailureAction implements Action {
    readonly type = LoadBoardLoadDetailActionTypes.Load_Failure;

    constructor(public payload: Error) { }
}


export class LoadBoardLoadAuditAction implements Action {
    readonly type = LoadBoardLoadDetailActionTypes.Load_Audit;

    constructor(public payload: LoadAudit) { }
}

export class LoadBoardLoadAuditSuccessAction implements Action {
    readonly type = LoadBoardLoadDetailActionTypes.Load_Audit_Success;

    constructor(public payload: number) { }
}

export class LoadBoardLoadAuditFailureAction implements Action {
    readonly type = LoadBoardLoadDetailActionTypes.Load_Audit_Failure;

    constructor(public payload: Error) { }
}


export type LoadBoardLoadDetailActions =
    LoadBoardLoadDetailLoadAction |
    LoadBoardLoadDetailLoadSuccessAction |
    LoadBoardLoadDetailLoadFailureAction |
    LoadBoardLoadAuditAction |
    LoadBoardLoadAuditSuccessAction |
    LoadBoardLoadAuditFailureAction;
