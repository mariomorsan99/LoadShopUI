import { Directive, ElementRef, EventEmitter, HostListener, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { Store } from '@ngrx/store';
import { Subscription } from 'rxjs';
import { map } from 'rxjs/operators';
import { getUserProfileEntity, UserState } from 'src/app/user/store';
import { User } from '../../models';

export enum EnforcementType {
  disable = 'disable',
  hide = 'hide',
}

@Directive({
  selector: '[kbxlActionCheck]',
  exportAs: 'kbxlActionCheck',
})
export class ActionCheckDirective implements OnDestroy, OnInit {
  @Input() enforcementType = 'hide';
  @Input() action: string;
  // tslint:disable-next-line:no-input-rename
  @Input('kbxlActionCheck') kbxlActionCheck: string;
  @Output() isDisabledChange = new EventEmitter<boolean>();

  userProfileSub: Subscription;
  userProfile: User;
  hasInit = false;
  isDisabled = false;

  constructor(private el: ElementRef, userState: Store<UserState>) {
    this.userProfileSub = userState.pipe(map(getUserProfileEntity)).subscribe(userProfile => {
      this.userProfile = userProfile;
      if (this.hasInit) {
        this.enforceSecurity();
      }
    });
  }

  ngOnInit(): void {
    this.hasInit = true;
    this.enforceSecurity();
  }

  private enforceSecurity() {
    const action = this.action || this.kbxlActionCheck;
    if (this.userProfile && action) {
      const actions = this.userProfile.securityAccessRoles
        .map(role => role.appActions)
        .reduce((currArray, nextArray) => currArray.concat(nextArray));

      const hasAtLeastOneAction = actions.filter(a => a.appActionId === action).length > 0;

      if (!hasAtLeastOneAction) {
        this.secureElement();
      } else {
        this.showElement();
      }
    } else {
      this.secureElement();
    }
  }

  private secureElement() {
    const et = EnforcementType[this.enforcementType];
    this.showElement();

    switch (et) {
      case EnforcementType.hide:
        this.el.nativeElement.style.display = 'none';
        break;
      case EnforcementType.disable:
        this.el.nativeElement.setAttribute('disabled', true);
        break;
    }

    this.isDisabled = true;
    this.isDisabledChange.emit(true);
  }

  private showElement() {
    this.el.nativeElement.style.display = null;
    this.el.nativeElement.removeAttribute('disabled');
    this.isDisabled = false;
    this.isDisabledChange.emit(false);
  }

  ngOnDestroy(): void {
    this.userProfileSub.unsubscribe();
  }

  @HostListener('click', ['$event'])
  clickEvent(event) {
    if (this.isDisabled) {
      event.preventDefault();
      event.stopPropagation();
    }
  }
}
