import { PageableUiData } from './pagable-ui-data';
import { Action } from '@ngrx/store';
import { Subscription, Observable } from 'rxjs';
import { delay, withLatestFrom } from 'rxjs/operators';
import { PageableQueryHelper, PageableQueryData } from './pageable-query-helper';
import { OnDestroy } from '@angular/core';
import { Search } from '../../models';

export class PageableUiHelper<TQueryUpdateAction extends Action, TLoadAction extends Action, TStoreType> implements OnDestroy {
  filterSub: Subscription;
  pagingSub: Subscription;
  filter$: Observable<Search>;
  pagingData$: Observable<PageableQueryData>;
  init = false;

  constructor(public pageableUiData: PageableUiData<TQueryUpdateAction, TLoadAction, TStoreType>) {
    this.filter$ = pageableUiData.filterBehavior.asObservable();
    this.pagingData$ = pageableUiData.pagingBehavior.asObservable();

    this.pagingSub = this.pagingData$.pipe(withLatestFrom(pageableUiData.pageableQueryHelper$), delay(0)).subscribe(args => {
      const pagingData = args[0];
      const currentQuery = args[1];
      if (pagingData) {
        currentQuery.pageNumber = pagingData.pageNumber;
        currentQuery.pageSize = pagingData.pageSize;
        currentQuery.sortField = pagingData.sortField;
        currentQuery.descending = pagingData.descending;

        this.dispatchActions(currentQuery);
      }
      this.init = true;
    });

    this.filterSub = this.filter$.pipe(withLatestFrom(pageableUiData.pageableQueryHelper$)).subscribe(args => {
      if (this.init) {
        const newFilter = args[0];
        const currentQuery = args[1];
        currentQuery.filter = newFilter;
        // When search changes rest page to 1 or you will still skip records
        currentQuery.pageNumber = 1;
        pageableUiData.pageableComponent.setFirst(0);

        this.dispatchActions(currentQuery);
      }
    });
  }

  private dispatchActions(currentQuery: PageableQueryHelper) {
    this.pageableUiData.store.dispatch(this.pageableUiData.getQueryUpdateAction(currentQuery));
    this.pageableUiData.store.dispatch(this.pageableUiData.getLoadAction(currentQuery));
  }

  ngOnDestroy(): void {
    this.pageableUiData.store.dispatch(this.pageableUiData.getQueryUpdateAction(PageableQueryHelper.default()));
    this.filterSub.unsubscribe();
    this.pagingSub.unsubscribe();
  }
}
