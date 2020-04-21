import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { Store } from '@ngrx/store';
import { MenuItem } from 'primeng/api';
import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { CoreState, getAdminMenuEntities } from 'src/app/core/store';

@Injectable({
  providedIn: 'root',
})
export class AdminGuard implements CanActivate {
  constructor(private store: Store<CoreState>, private router: Router) {}
  canActivate(): Observable<UrlTree> {
    return this.store.pipe(
      map(getAdminMenuEntities),
      // if user has admin menu items go to first tab otherwise go to home page
      map((data: MenuItem[]) =>
        data.length ? this.router.createUrlTree(['maint', ...data[0].routerLink]) : this.router.createUrlTree([''])
      ),
      take(1)
    );
  }
}
