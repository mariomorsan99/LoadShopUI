import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { select, Store } from '@ngrx/store';
import { Subject } from 'rxjs';
import { distinctUntilChanged, map, takeUntil } from 'rxjs/operators';
import { getUserProfileEntity } from '../../../../user/store';
import { AcceptAgreementDocumentAction, CoreState, getAgreementDocumentLoading } from '../../../store';
@Component({
  selector: 'kbxl-user-agreement',
  templateUrl: './user-agreement.component.html',
  styleUrls: ['./user-agreement.component.scss'],
})
export class UserAgreementComponent implements OnInit, OnDestroy {
  userAgreement = false;
  destroyed$ = new Subject<boolean>();
  loading = false;
  displayModal = false;
  constructor(private store: Store<CoreState>, private router: Router) {}
  ngOnDestroy(): void {
    this.destroyed$.next(true);
    this.destroyed$.complete();
  }

  ngOnInit() {
    this.store.pipe(takeUntil(this.destroyed$), map(getAgreementDocumentLoading)).subscribe(x => (this.loading = x));

    this.store.pipe(select(getUserProfileEntity), takeUntil(this.destroyed$), distinctUntilChanged()).subscribe(user => {
      if (!user) {
        return;
      }
      // show the privacy policy first, then the terms
      if (user.hasAgreedToTerms) {
        this.router.navigate(['/']);
      } else {
        this.displayModal = true;
      }
    });
  }
  agree(): void {
    this.loading = true;
    this.displayModal = false;
    this.store.dispatch(new AcceptAgreementDocumentAction());
  }
}
