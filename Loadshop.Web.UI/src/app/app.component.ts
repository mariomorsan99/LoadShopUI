import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { select, Store } from '@ngrx/store';
import { MenuItem } from 'primeng/api';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { TabMenu } from 'primeng/tabmenu';
import { Observable, Subscription } from 'rxjs';
import { filter, withLatestFrom } from 'rxjs/operators';
import { CoreState, getMenuEntities, getSmartSpotQuickQuoteEvent } from './core/store';
import { User } from './shared/models';
import { loadshopApplicationReady, SharedState } from './shared/store';
import { getUserProfileEntity } from './user/store';

@Component({
  selector: 'kbxl-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit, OnDestroy {
  @ViewChild(TabMenu) menu: TabMenu;
  applicationReady$: Observable<boolean>;

  user$: Observable<User>;
  menu$: Observable<MenuItem[]>;
  activeItem: MenuItem = {};

  private quickQuoteItem: MenuItem = null;
  private lastActiveItem: MenuItem = null;
  private qqEventSub: Subscription;
  private routingSub: Subscription;

  constructor(private store: Store<CoreState>, private sharedStore: Store<SharedState>, private router: Router) {}

  ngOnInit() {
    this.user$ = this.store.pipe(select(getUserProfileEntity));
    this.menu$ = this.store.pipe(select(getMenuEntities));
    this.applicationReady$ = this.sharedStore.pipe(select(loadshopApplicationReady));
    this.qqEventSub = this.store.pipe(select(getSmartSpotQuickQuoteEvent)).subscribe(_ => {
      const visible = _ && _.originalEvent;
      if (visible && this.menu) {
        if (this.menu.activeItem !== _.item) {
          this.lastActiveItem = this.menu.activeItem;
          this.quickQuoteItem = _.item;
          this.activeItem = _.item;
        }
      } else if (this.menu) {
        if (this.menu.activeItem === this.quickQuoteItem) {
          this.activeItem = this.lastActiveItem;
        }
        this.lastActiveItem = null;
        this.quickQuoteItem = null;
      }
    });

    this.routingSub = this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        withLatestFrom(this.menu$)
      )
      .subscribe(([event, menuItems]) => {
        // for non-menu triggered navigation such as Create Order from Quick Quote
        const urlAfterRedirects = (event as NavigationEnd).urlAfterRedirects;
        if (menuItems) {
          const routesItem = menuItems.filter(_ => _.routerLink).find(_ => urlAfterRedirects.startsWith(_.routerLink[0]));
          if (routesItem) {
            this.activeItem = routesItem;
          }
        }
      });
  }

  ngOnDestroy() {
    if (this.qqEventSub) {
      this.qqEventSub.unsubscribe();
      this.qqEventSub = null;
    }
    if (this.routingSub) {
      this.routingSub.unsubscribe();
      this.routingSub = null;
    }
  }

  accept(confirmationDialog: ConfirmDialog) {
    confirmationDialog.accept();
    this.resetDefaults(confirmationDialog);
  }

  reject(confirmationDialog: ConfirmDialog) {
    confirmationDialog.reject();
    this.resetDefaults(confirmationDialog);
  }

  private resetDefaults(confirmationDialog: ConfirmDialog) {
    confirmationDialog.acceptLabel = 'YES';
    confirmationDialog.acceptVisible = true;
    confirmationDialog.rejectLabel = 'NO';
    confirmationDialog.rejectVisible = true;
  }
}
