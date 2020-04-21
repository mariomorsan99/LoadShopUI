import { Action } from '@ngrx/store';
import { HttpErrorResponse } from '@angular/common/http';
import { OrderEntryForm } from 'src/app/shared/models/order-entry-form';

export enum OrderEntryActionTypes {
    Get_Load = '[OrderEntry] GET_LOAD',
    Get_Load_Success = '[OrderEntry] GET_LOAD_SUCCESS',
    Get_Load_Failure = '[OrderEntry] GET_LOAD_FAILURE',
    Create_Load = '[OrderEntry] CREATE_LOAD',
    Create_Load_Success = '[OrderEntry] CREATE_LOAD_SUCCESS',
    Create_Load_Failure = '[OrderEntry] CREATE_LOAD_FAILURE',
    Update_Load = '[OrderEntry] UPDATE_LOAD',
    Update_Load_Success = '[OrderEntry] UPDATE_LOAD_SUCCESS',
    Update_Load_Failure = '[OrderEntry] UPDATE_LOAD_FAILURE',
    Clear_Errors = '[OrderEntry] CLEAR_ERRORS',
    Reset_Saved = '[OrderEntry] RESET_SAVED',
}

export class OrderEntryGetLoadAction implements Action {
    readonly type = OrderEntryActionTypes.Get_Load;
    constructor(public payload: string) { }
}

export class OrderEntryGetLoadSuccessAction implements Action {
    readonly type = OrderEntryActionTypes.Get_Load_Success;
    constructor(public payload: OrderEntryForm) { }
}

export class OrderEntryGetLoadFailureAction implements Action {
    readonly type = OrderEntryActionTypes.Get_Load_Failure;
    constructor(public payload: HttpErrorResponse) { }
}

export class OrderEntryCreateLoadAction implements Action {
    readonly type = OrderEntryActionTypes.Create_Load;
    constructor(public payload: any) { }
}

export class OrderEntryCreateLoadSuccessAction implements Action {
    readonly type = OrderEntryActionTypes.Create_Load_Success;

    constructor(public payload: OrderEntryForm) { }
}

export class OrderEntryCreateLoadFailureAction implements Action {
    readonly type = OrderEntryActionTypes.Create_Load_Failure;
    constructor(public payload: HttpErrorResponse) { }
}

export class OrderEntryUpdateLoadAction implements Action {
  readonly type = OrderEntryActionTypes.Update_Load;
  constructor(public payload: any) { }
}

export class OrderEntryUpdateLoadSuccessAction implements Action {
  readonly type = OrderEntryActionTypes.Update_Load_Success;

  constructor(public payload: OrderEntryForm) { }
}

export class OrderEntryUpdateLoadFailureAction implements Action {
  readonly type = OrderEntryActionTypes.Update_Load_Failure;
  constructor(public payload: HttpErrorResponse) { }
}

export class OrderEntryClearErrorsAction implements Action {
    readonly type = OrderEntryActionTypes.Clear_Errors;
    constructor( ) { }
}

export class OrderEntryResetSavedAction implements Action {
    readonly type = OrderEntryActionTypes.Reset_Saved;
    constructor( ) { }
}

export type OrderEntryActions =
    OrderEntryGetLoadAction |
    OrderEntryGetLoadSuccessAction |
    OrderEntryGetLoadFailureAction |
    OrderEntryCreateLoadAction |
    OrderEntryCreateLoadSuccessAction |
    OrderEntryCreateLoadFailureAction |
    OrderEntryUpdateLoadAction |
    OrderEntryUpdateLoadSuccessAction |
    OrderEntryUpdateLoadFailureAction |
    OrderEntryClearErrorsAction |
    OrderEntryResetSavedAction;

