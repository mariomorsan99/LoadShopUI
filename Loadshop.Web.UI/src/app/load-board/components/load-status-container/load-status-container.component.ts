import { Component, OnInit, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Store, select } from '@ngrx/store';
import { filter, map, tap, delay } from 'rxjs/operators';
import { getLoadBoardSelectedLoad } from '../../store';
import {
  LoadStatusLoadAction,
   CoreState,
  getStates,
  LoadshopStopStatusSaveAction,
  LoadshopInTransitStatusSaveAction
} from 'src/app/core/store';
import { Observable, of, Subscription } from 'rxjs';
import {
  LoadDetail,
  State,
  LoadStatusStopData,
  LoadStatusDetail,
  LoadStatusInTransitData,
  ValidationProblemDetails,
  UserModel,
  TransactionType
} from 'src/app/shared/models';
import {
  getLoadStatusDetail,
  getLoadStatusLoading,
  getLoadStatusSaving,
  getLoadStatusErrors
} from 'src/app/core/store/selectors/load-status.selector';
import { LoadBoardLoadDetailLoadAction } from 'src/app/shared/store';
import { getUserProfileModel } from 'src/app/user/store';

@Component({
  selector: 'kbxl-load-status-container',
  templateUrl: './load-status-container.component.html',
  styleUrls: ['./load-status-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadStatusContainerComponent implements OnInit, OnDestroy {
  loadDetail$: Observable<LoadDetail>;
  loadStatus$: Observable<LoadStatusDetail>;
  loadingStatus$: Observable<boolean>;
  savingStatus$: Observable<boolean>;
  states$: Observable<State[]>;
  loadStatusErrors$: Observable<ValidationProblemDetails>;
  user$: Observable<UserModel>;

  private loadDetail: LoadDetail;
  private reloadSub: Subscription;

  constructor(private store: Store<CoreState>, private route: ActivatedRoute) { }

  ngOnInit() {
    this.route.params
      .pipe(filter(p => p.id), map(p => p.id))
      .subscribe(id => {
        this.store.dispatch(new LoadBoardLoadDetailLoadAction(id));
      });
    this.loadDetail$ = this.store.pipe(
      select(getLoadBoardSelectedLoad),
      tap(load => {
        this.loadDetail = load;
        this.triggerLoadStatusLoadAction();
      }));
    this.loadStatus$ = this.store.pipe(
      select(getLoadStatusDetail),
      tap(status => {
        this.cancelReload();
        if (status && status.processingUpdates) {
          this.reloadSub = of(true).pipe(delay(10000)).subscribe(() => this.triggerLoadStatusLoadAction());
        }
      })
    );
    this.loadingStatus$ = this.store.pipe(select(getLoadStatusLoading));
    this.savingStatus$ = this.store.pipe(select(getLoadStatusSaving));
    this.states$ = this.store.pipe(select(getStates));
    this.loadStatusErrors$ = this.store.pipe(select(getLoadStatusErrors));
    this.user$ = this.store.pipe(select(getUserProfileModel));
  }

  ngOnDestroy() {
    this.cancelReload();
  }

  private cancelReload() {
    if (this.reloadSub) {
      this.reloadSub.unsubscribe();
      this.reloadSub = null;
    }
  }

  private triggerLoadStatusLoadAction() {
    if (this.loadDetail && this.loadDetail.loadTransaction
        && this.loadDetail.loadTransaction.transactionType === TransactionType.Accepted) {
      this.store.dispatch(new LoadStatusLoadAction({
        loadId: this.loadDetail.loadId,
        referenceLoadId: this.loadDetail.platformPlusLoadId || this.loadDetail.referenceLoadId
      }));
      this.cancelReload();
    }
  }

  public saveStopStatuses(stopStatuses: LoadStatusStopData) {
    this.store.dispatch(new LoadshopStopStatusSaveAction(stopStatuses));
  }

  public saveInTransitStatus(inTransitStatus: LoadStatusInTransitData) {
    this.store.dispatch(new LoadshopInTransitStatusSaveAction(inTransitStatus));
  }
}
