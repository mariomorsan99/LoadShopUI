import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { NavigationGoAction } from '@tms-ng/core';
import { Observable, of } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { UserProfileLoadAction } from '../../../user/store';
import { AgreementDocumentService } from '../../services/agreement-document.service';
import {
  AcceptAgreementDocumentAction,
  AcceptAgreementDocumentFailureAction,
  AcceptAgreementDocumentSuccessAction,
  AgreementDocumentActionTypes,
} from '../actions';

@Injectable()
export class AgreementDocumentEffects {
  @Effect()
  $acceptAgreement: Observable<Action> = this.actions$.pipe(
    ofType<AcceptAgreementDocumentAction>(AgreementDocumentActionTypes.AcceptAgreement),
    switchMap(() => {
      return this.agreementDocumentService.acceptAgreement().pipe(
        switchMap(() => [
          new AcceptAgreementDocumentSuccessAction(),
          new UserProfileLoadAction(), // reload user profile
          new NavigationGoAction({ path: ['/'] }),
        ]),
        catchError(err => {
          return of(new AcceptAgreementDocumentFailureAction(err));
        })
      );
    })
  );

  constructor(private actions$: Actions, private agreementDocumentService: AgreementDocumentService) {}
}
