import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { Store } from '@ngrx/store';
import { SelectItemGroup } from 'primeng/api';
import { BehaviorSubject, combineLatest, Observable } from 'rxjs';
import { debounceTime, map } from 'rxjs/operators';
import { User, UserFocusEntity } from '../../../shared/models';
import { getAuthorizedFocusEntitiesSelector, getUserProfileEntity, UpdateFocusEntityAction, UserState } from '../../store';

@Component({
  selector: 'kbxl-user-focus-entity-selector-container',
  templateUrl: './user-focus-entity-selector-container.component.html',
  styleUrls: ['./user-focus-entity-selector-container.component.scss'],
})
export class UserFocusEntitySelectorContainerComponent implements OnInit {
  @Output() updated = new EventEmitter<User>();

  availableEntitiesFlat$: Observable<UserFocusEntity[]>;
  searchResults: UserFocusEntity[];
  availableEntities$: Observable<SelectItemGroup[]>;
  selectedEntity$: Observable<UserFocusEntity>;
  searchBehaivor = new BehaviorSubject<string>(null);
  search$ = this.searchBehaivor.asObservable();

  constructor(private store: Store<UserState>) {}

  ngOnInit() {
    this.selectedEntity$ = this.store.pipe(
      map(getUserProfileEntity),
      map(userProfile => userProfile && userProfile.focusEntity)
    );

    this.availableEntitiesFlat$ = this.store.pipe(
      map(getAuthorizedFocusEntitiesSelector),
      map(userFocusEntityResult => userFocusEntityResult && userFocusEntityResult.focusEntites)
    );

    this.availableEntities$ = this.store.pipe(
      map(getAuthorizedFocusEntitiesSelector),
      map(userFocusEntityResult => userFocusEntityResult && userFocusEntityResult.groupedFocusEntities)
    );

    combineLatest(this.availableEntitiesFlat$, this.search$.pipe(debounceTime(350))).subscribe(args => {
      const entities = args[0];
      const query = args[1];
      let results = new Array<UserFocusEntity>();

      if (query != null) {
        results = results.concat(
          entities.filter(
            entity =>
              entity.id.toLocaleLowerCase().includes(query.toLocaleLowerCase()) ||
              entity.name.toLocaleLowerCase().includes(query.toLocaleLowerCase()) ||
              entity.group.toLocaleLowerCase().includes(query.toLocaleLowerCase())
          )
        );
      }

      this.searchResults = results;
    });
  }

  update(selectedEntity: UserFocusEntity) {
    this.store.dispatch(new UpdateFocusEntityAction(selectedEntity));
  }

  search(query: string) {
    this.searchBehaivor.next(query);
  }
}
