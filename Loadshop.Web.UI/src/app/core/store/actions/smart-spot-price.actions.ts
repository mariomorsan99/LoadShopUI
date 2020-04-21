import { Action } from '@ngrx/store';
import { SmartSpotPrice, SmartSpotPriceRequest, SmartSpotQuoteRequest, SmartSpotQuoteCreateRequest } from '../../../shared/models';
import { MenuItem } from 'primeng/api';
import { HttpErrorResponse } from '@angular/common/http';

export enum SmartSpotPriceActionTypes {
    Load = '[SmartSpotPrice] LOAD',
    LoadSuccess = '[SmartSpotPrice] LOAD_SUCCESS',
    LoadFailure = '[SmartSpotPrice] LOAD_FAILURE',
    ShowQuickQuote = '[SmartSpotPrice] SHOW_QUICK_QUOTE',
    HideQuickQuote = '[SmartSpotPrice] HIDE_QUICK_QUOTE',
    LoadQuote = '[SmartSpotPrice] LOAD_QUOTE',
    LoadQuoteSuccess = '[SmartSpotPrice] LOAD_QUOTE_SUCCESS',
    LoadQuoteFailure = '[SmartSpotPrice] LOAD_QUOTE_FAILURE',
    CreateOrderFromQuote = '[SmartSpotPrice] CREATE_ORDER_FROM_QUOTE',
    ClearCreateOrderFromQuote = '[SmartSpotPrice] CLEAR_CREATE_ORDER_FROM_QUOTE',
}

export class SmartSpotPriceLoadAction implements Action {
    readonly type = SmartSpotPriceActionTypes.Load;

    constructor(public payload: SmartSpotPriceRequest[]) { }
}

export class SmartSpotPriceLoadSuccessAction implements Action {
    readonly type = SmartSpotPriceActionTypes.LoadSuccess;

    constructor(public payload: SmartSpotPrice[]) { }
}

export class SmartSpotPriceLoadFailureAction implements Action {
    readonly type = SmartSpotPriceActionTypes.LoadFailure;

    constructor(public payload: Error) { }
}

export class SmartSpotPriceShowQuickQuoteAction implements Action {
    readonly type = SmartSpotPriceActionTypes.ShowQuickQuote;

    constructor(public payload: { originalEvent: Event, item: MenuItem }) { }
}

export class SmartSpotPriceHideQuickQuoteAction implements Action {
    readonly type = SmartSpotPriceActionTypes.HideQuickQuote;
}

export class SmartSpotPriceLoadQuoteAction implements Action {
    readonly type = SmartSpotPriceActionTypes.LoadQuote;

    constructor(public payload: SmartSpotQuoteRequest) { }
}

export class SmartSpotPriceLoadQuoteSuccessAction implements Action {
    readonly type = SmartSpotPriceActionTypes.LoadQuoteSuccess;

    constructor(public payload: number) { }
}

export class SmartSpotPriceLoadQuoteFailureAction implements Action {
    readonly type = SmartSpotPriceActionTypes.LoadQuoteFailure;

    constructor(public payload: HttpErrorResponse) { }
}

export class SmartSpotCreateOrderFromQuoteAction implements Action {
    readonly type = SmartSpotPriceActionTypes.CreateOrderFromQuote;

    constructor(public payload: SmartSpotQuoteCreateRequest) { }
}

export class SmartSpotClearCreateOrderFromQuote implements Action {
    readonly type = SmartSpotPriceActionTypes.ClearCreateOrderFromQuote;
}


export type SmartSpotPriceActions =
    SmartSpotPriceLoadAction |
    SmartSpotPriceLoadSuccessAction |
    SmartSpotPriceLoadFailureAction |
    SmartSpotPriceShowQuickQuoteAction |
    SmartSpotPriceHideQuickQuoteAction |
    SmartSpotPriceLoadQuoteAction |
    SmartSpotPriceLoadQuoteSuccessAction |
    SmartSpotPriceLoadQuoteFailureAction |
    SmartSpotCreateOrderFromQuoteAction |
    SmartSpotClearCreateOrderFromQuote;
