import { Action } from '@ngrx/store';
import { T2GLoadStatus } from 'src/app/shared/models/t2g-load-status';
import { LoadStatusTransaction, LoadStatusStopData, LoadStatusInTransitData } from 'src/app/shared/models';
import { HttpErrorResponse } from '@angular/common/http';

export enum LoadStatusActionTypes {
    Load = '[Load Status] LOAD',
    Tops2GoStatusLoadSuccess = '[T2G Load Status] LOAD_SUCCESS',
    Tops2GoStatusLoadFailure = '[T2G Load Status] LOAD_FAILURE',
    LoadshopStatusLoadSuccess = '[Loadshop Load Status] LOAD_SUCCESS',
    LoadshopStatusLoadFailure = '[Loadshop Load Status] LOAD_FAILURE',
    LoadshopInTransitStatusSave = '[Loadshop Load Status] SAVE IN TRANSIT',
    LoadshopStopStatusSave = '[Loadshop Load Status] SAVE STOP',
    LoadshopLoadStatusSaveSuccess = '[Loadshop Load Status] SAVE SUCCESS',
    LoadshopLoadStatusSaveFailure = '[Loadshop Load Status] SAVE FAILURE',
    LoadAll = '[Load Status] LOAD ALL',
    LoadAllSuccess = '[Load Status] LOAD ALL SUCCESS',
    LoadAllFailure = '[Load Status] LOAD ALL FAILURE',
}

export class LoadStatusLoadAction implements Action {
    readonly type = LoadStatusActionTypes.Load;

    constructor(public payload: { loadId: string; referenceLoadId: string }) { }
}

export class Tops2GoLoadStatusLoadSuccessAction implements Action {
    readonly type = LoadStatusActionTypes.Tops2GoStatusLoadSuccess;

    constructor(public payload: T2GLoadStatus) { }
}

export class Tops2GoLoadStatusLoadFailureAction implements Action {
    readonly type = LoadStatusActionTypes.Tops2GoStatusLoadFailure;

    constructor(public payload: Error) { }
}

export class LoadshopLoadStatusLoadSuccessAction implements Action {
    readonly type = LoadStatusActionTypes.LoadshopStatusLoadSuccess;

    constructor(public payload: LoadStatusTransaction) { }
}

export class LoadshopLoadStatusLoadFailureAction implements Action {
    readonly type = LoadStatusActionTypes.LoadshopStatusLoadFailure;

    constructor(public payload: Error) { }
}

export class LoadshopInTransitStatusSaveAction implements Action {
    readonly type = LoadStatusActionTypes.LoadshopInTransitStatusSave;

    constructor(public payload: LoadStatusInTransitData) { }
}

export class LoadshopStopStatusSaveAction implements Action {
    readonly type = LoadStatusActionTypes.LoadshopStopStatusSave;

    constructor(public payload: LoadStatusStopData) { }
}

export class LoadshopLoadStatusSaveSuccessAction implements Action {
    readonly type = LoadStatusActionTypes.LoadshopLoadStatusSaveSuccess;

    constructor(public payload: LoadStatusTransaction) { }
}

export class LoadshopLoadStatusSaveFailureAction implements Action {
    readonly type = LoadStatusActionTypes.LoadshopLoadStatusSaveFailure;

    constructor(public payload: HttpErrorResponse) { }
}

export class LoadStatusLoadAllAction implements Action {
    readonly type = LoadStatusActionTypes.LoadAll;

    constructor(public payload: { referenceLoadId: string }) { }
}

export class LoadStatusLoadAllSuccessAction implements Action {
    readonly type = LoadStatusActionTypes.LoadAllSuccess;

    constructor(public payload: T2GLoadStatus[]) { }
}

export class LoadStatusLoadAllFailureAction implements Action {
    readonly type = LoadStatusActionTypes.LoadAllFailure;

    constructor(public payload: HttpErrorResponse) { }
}

export type LoadStatusActions =
    LoadStatusLoadAction |
    Tops2GoLoadStatusLoadSuccessAction |
    Tops2GoLoadStatusLoadFailureAction |
    LoadshopLoadStatusLoadSuccessAction |
    LoadshopLoadStatusLoadFailureAction |
    LoadshopInTransitStatusSaveAction |
    LoadshopStopStatusSaveAction |
    LoadshopLoadStatusSaveSuccessAction |
    LoadshopLoadStatusSaveFailureAction |
    LoadStatusLoadAllAction |
    LoadStatusLoadAllSuccessAction |
    LoadStatusLoadAllFailureAction;
