import { Action } from '@ngrx/store';

export enum LoadshopApplicationActionTypes {
  LoadshopStart = '[Loadshop] LOADSHOP_START',
}

export class LoadshopApplicationStartAction implements Action {
  readonly type = LoadshopApplicationActionTypes.LoadshopStart;
}
export type LoadshopApplicationActions = LoadshopApplicationStartAction;
