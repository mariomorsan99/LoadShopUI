import { Observable, BehaviorSubject } from 'rxjs';
import { Search } from '../../models';
import { PageableQueryData, PageableQueryHelper } from './pageable-query-helper';
import { Store, Action } from '@ngrx/store';
import { PageableComponent } from './pageable-component';

export interface PageableUiData<TQueryUpdateAction extends Action, TLoadAction extends Action, TStoreType> {
  filterBehavior: BehaviorSubject<Search>;
  pageableQueryHelper$: Observable<PageableQueryHelper>;
  pagingBehavior: BehaviorSubject<PageableQueryData>;
  store: Store<TStoreType>;
  pageableComponent: PageableComponent;
  getQueryUpdateAction: (pageableQueryHelper: PageableQueryHelper) => TQueryUpdateAction;
  getLoadAction: (pageableQueryHelper: PageableQueryHelper) => TLoadAction;
}
