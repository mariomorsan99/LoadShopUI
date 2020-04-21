import { Component, Input, OnDestroy } from '@angular/core';
import { CustomerLoadTypes, User } from '../../models';
import { Store } from '@ngrx/store';
import { UserState, getUserProfileEntity } from 'src/app/user/store';
import { Subscription } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
  selector: 'kbxl-customer-type-icon',
  templateUrl: './customer-type-icon.component.html',
  styleUrls: ['./customer-type-icon.component.scss']
})
export class CustomerTypeIconComponent implements OnDestroy {
  @Input() referenceLoadDisplay: string;
  @Input() customerLoadTypeId: number;

  showIcon = false;
  userProfileSub: Subscription;
  userProfile: User;
  CustomerLoadTypes = CustomerLoadTypes; // Enable use of global enum in template by duplicating it as a property of this component

  constructor(userState: Store<UserState>) {
    this.userProfileSub = userState.pipe(map(getUserProfileEntity)).subscribe(userProfile => {
      this.userProfile = userProfile;
      if (this.userProfile && this.userProfile.securityAccessRoles) {
        const securityRoles = ['System Admin', 'LS Admin'];
        this.showIcon = this.userProfile.securityAccessRoles.filter(x => securityRoles.includes(x.accessRoleName)).length > 0;
      }
    });
  }

  ngOnDestroy(): void {
    this.userProfileSub.unsubscribe();
  }
}
