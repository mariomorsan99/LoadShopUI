import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, UrlTree } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { map, distinctUntilChanged, filter } from 'rxjs/operators';
import { UserState, getUserProfileModel } from 'src/app/user/store';
import { SecurityAppActionType } from '../models/security-app-action-type';

export enum AppActionOperatorType {
  Any = 'ANY',
  All = 'ALL',
}

@Injectable({
  providedIn: 'root',
})
export class AppActionGuard implements CanActivate {
  constructor(private userStore: Store<UserState>,  private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): Observable<boolean | UrlTree> {
    const roles = route.data.roles as Array<SecurityAppActionType>;
    let operator = route.data.operator as AppActionOperatorType;
    if (!operator) {
      operator = AppActionOperatorType.All;
    }

    return this.userStore.pipe(
      map(getUserProfileModel),
      distinctUntilChanged(),
      filter(x => x != null),
      map(user => {
        for (let i = 0; i < roles.length; i++) {
          if (operator === AppActionOperatorType.Any && user.hasSecurityAction(roles[i])) {
            return true;
          }
          if (operator === AppActionOperatorType.All && !user.hasSecurityAction(roles[i])) {
            return this.router.createUrlTree(['unauthorized']);
          }
        }
        if (operator === AppActionOperatorType.Any) {
            return this.router.createUrlTree(['unauthorized']);
        }
        return true;
      })
    );
  }
}
