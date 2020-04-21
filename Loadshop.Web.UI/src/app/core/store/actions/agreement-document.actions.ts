/* eslint-disable @typescript-eslint/camelcase */
import { Action } from '@ngrx/store';

export enum AgreementDocumentActionTypes {
  AcceptAgreement = '[AgreementDocument] ACCEPT_AGREEMENT_PRIVACY',
  AcceptAgreement_Success = '[AgreementDocument] ACCEPT_AGREEMENT_SUCCESS',
  AcceptAgreement_Failure = '[AgreementDocument] ACCEPT_AGREEMENT_FAILURE',
}

export class AcceptAgreementDocumentAction implements Action {
  readonly type = AgreementDocumentActionTypes.AcceptAgreement;
}

export class AcceptAgreementDocumentSuccessAction implements Action {
  readonly type = AgreementDocumentActionTypes.AcceptAgreement_Success;
}

export class AcceptAgreementDocumentFailureAction implements Action {
  readonly type = AgreementDocumentActionTypes.AcceptAgreement_Failure;
  constructor(public payload: Error) {}
}

export type AgreementDocumentActions =
  | AcceptAgreementDocumentAction
  | AcceptAgreementDocumentSuccessAction
  | AcceptAgreementDocumentFailureAction;
