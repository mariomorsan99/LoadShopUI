import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { select, Store } from '@ngrx/store';
import { Observable, Subject } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';
import { CoreState, getEquipment } from '../../../../core/store';
import { Equipment } from '../../../../shared/models';
import { SpecialInstruction } from '../../../../shared/models/special-instruction';
import { SpecialInstructionData } from '../../../../shared/models/special-instruction-data';
import { getUserProfileEntity, UserState } from '../../../../user/store';
import { AdminState, getLoadingSpecialInstructions, getSpecialInstructions, SpecialInstructionsLoadAction } from '../../../store';

@Component({
  selector: 'kbxl-special-instructions-container',
  templateUrl: './special-instructions-container.component.html',
  styleUrls: ['./special-instructions-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpecialInstructionsContainerComponent implements OnInit, OnDestroy {
  instructions$: Observable<SpecialInstruction[]>;
  loading$: Observable<boolean>;
  equipment$: Observable<Equipment[]>;
  destroyed$ = new Subject<boolean>();

  constructor(
    private adminStore: Store<AdminState>,
    private userStore: Store<UserState>,
    private coreStore: Store<CoreState>,
    private router: Router
  ) {}

  ngOnInit() {
    this.userStore.pipe(select(getUserProfileEntity), takeUntil(this.destroyed$)).subscribe(user => {
      if (user && user.primaryCustomerId) {
        this.adminStore.dispatch(new SpecialInstructionsLoadAction({ customerId: user.primaryCustomerId }));
      }
    });

    this.loading$ = this.adminStore.pipe(map(getLoadingSpecialInstructions));

    this.instructions$ = this.adminStore.pipe(
      select(getSpecialInstructions),
      map(_ => _.map(g => new SpecialInstruction(g)))
    );

    this.equipment$ = this.coreStore.pipe(map(getEquipment));
  }

  ngOnDestroy() {
    this.destroyed$.next(true);
    this.destroyed$.complete();
  }

  displayDetail(group: SpecialInstructionData) {
    const path: [any] = ['maint/special-instructions/detail'];
    if (group && group.specialInstructionId > 0) {
      path.push(group.specialInstructionId);
    }
    this.router.navigate(path);
  }
}
