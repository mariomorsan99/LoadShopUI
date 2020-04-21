import { Action } from '@ngrx/store';

export enum BrowserActionTypes {
    SetIsMobile = '[Browser] SET IS MOBILE'
}

export class BrowserSetIsMobileAction implements Action {
    readonly type = BrowserActionTypes.SetIsMobile;

    constructor(public payload: { isMobile: boolean }) { }
}

export type BrowserActions =
    BrowserSetIsMobileAction;
