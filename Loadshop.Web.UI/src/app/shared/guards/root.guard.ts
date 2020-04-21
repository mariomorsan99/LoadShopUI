import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { Store, select } from '@ngrx/store';
import { Observable } from 'rxjs';
import { map, take, filter } from 'rxjs/operators';
import { CoreState, getMenuEntities } from 'src/app/core/store';
import { MenuItem } from 'primeng/api';
@Injectable({
    providedIn: 'root',
})
export class RootGuard implements CanActivate {
    constructor(private store: Store<CoreState>, private router: Router) { }
    canActivate(): Observable<UrlTree> {
        return this.store.pipe(
            select(getMenuEntities),
            filter(data => data.length > 0),
            map((data: MenuItem[]) => this.router.createUrlTree(data[0].routerLink)),
            take(1),
        );
    }
}


