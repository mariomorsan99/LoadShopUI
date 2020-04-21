import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { select, Store } from '@ngrx/store';
import { Observable, Subject } from 'rxjs';
import { filter, map, take, takeUntil, tap } from 'rxjs/operators';
import { CoreState, LoadStatusLoadAction, LoadStatusLoadAllAction } from 'src/app/core/store';
import {
  getAllLoadStatusDetails,
  getLoadStatusDetail,
  getLoadStatusLoading,
  getLoadStatusLoadingAll,
} from 'src/app/core/store/selectors/load-status.selector';
import { getRatingQuestions, RatingGetQuestionsAction, ShippingLoadDetailDeleteLoadAction, ShippingState } from '../../../shipping/store';
import { getUserProfileEntity, UserState } from '../../../user/store';
import {
  AuditType,
  Load,
  LoadAudit,
  LoadDetail,
  LoadDocumentMetadata,
  LoadDocumentType,
  LoadStatusDetail,
  RatingQuestion,
  RemoveCarrierData,
  RemoveLoadData,
  TransactionType,
} from '../../models';
import { LoadDocumentDownload } from '../../models/load-document-download';
import {
  getLoadBoardSelectedLoad,
  getLoadDocumentDownload,
  getLoadDocumentLoading,
  getLoadDocumentTypes,
  LoadBoardLoadAuditAction,
  LoadBoardLoadBookAction,
  LoadBoardLoadDetailLoadAction,
  LoadDetailCarrierRemovedAction,
  LoadDetailDeleteLoadAction,
  LoadDocumentDeleteDocumentAction,
  LoadDocumentDownloadDocumentAction,
  LoadDocumentDownloadDocumentClearAction,
  LoadDocumentLoadTypesAction,
  SharedState,
} from '../../store';

@Component({
  templateUrl: './load-detail-container.component.html',
  styleUrls: ['./load-detail-container.component.scss'],
})
export class LoadDetailContainerComponent implements OnInit, OnDestroy {
  loadingDocuments$: Observable<boolean>;
  loadDetail$: Observable<LoadDetail>;
  ratingQuestions$: Observable<RatingQuestion[]>;
  loadStatus$: Observable<LoadStatusDetail>;
  loadingStatus$: Observable<boolean>;
  allStatuses$: Observable<LoadStatusDetail[]>;
  loadingAllStatuses$: Observable<boolean>;
  displayInSidebar: boolean;
  isShippingDetail: boolean;
  destroyed$ = new Subject<boolean>();
  loadDocumentTypes$: Observable<LoadDocumentType[]>;
  captureReasonOnDelete = false;

  private _detail: LoadDetail;

  constructor(
    private store: Store<SharedState>,
    private shippingStore: Store<ShippingState>,
    private route: ActivatedRoute,
    private router: Router,
    private userStore: Store<UserState>,
    private coreStore: Store<CoreState>
  ) {}

  ngOnInit() {
    this.displayInSidebar = this.route.snapshot.data && this.route.snapshot.data.sidebar === true;
    this.setIsShippingDetail();

    this.userStore.pipe(select(getUserProfileEntity), takeUntil(this.destroyed$)).subscribe(user => {
      if (user && user.isShipper) {
        this.shippingStore.dispatch(new RatingGetQuestionsAction());
      }
    });
    this.route.params
      .pipe(
        filter(p => p.id),
        map(params => params.id),
        takeUntil(this.destroyed$)
      )
      .subscribe(id => {
        this.store.dispatch(new LoadBoardLoadDetailLoadAction(id));
        this.route.queryParams
          .pipe(
            filter(p => p.at),
            map(queryParams => queryParams.at),
            takeUntil(this.destroyed$)
          )
          .subscribe(at => {
            const auditTypeId = at as keyof typeof AuditType;
            switch (auditTypeId) {
              /**
               * Because the detail container is now routeable and loaded via URL, we have to pass in query params
               * for all views that we want to track.
               */
              case AuditType.MarketplaceView:
                this.store.dispatch(new LoadBoardLoadAuditAction(new LoadAudit(id, AuditType.MarketplaceView)));
                break;
              case AuditType.FavoritesMatchEmailView:
                this.store.dispatch(new LoadBoardLoadAuditAction(new LoadAudit(id, AuditType.FavoritesMatchEmailView)));
                break;
              case AuditType.ReadyToBookEmailView:
                this.store.dispatch(new LoadBoardLoadAuditAction(new LoadAudit(id, AuditType.ReadyToBookEmailView)));
                break;
              default:
                break; // Do not audit anything, just assume the param being passed is bad
            }
          });
      });

    this.route.data
      .pipe(
        filter(p => p.captureReasonOnDelete),
        map(params => params.captureReasonOnDelete),
        takeUntil(this.destroyed$)
      )
      .subscribe(captureReasonOnDelete => {
        // this route param will be set in the routing file and will enable a reason why modal to show when the user removes the load
        this.captureReasonOnDelete = captureReasonOnDelete;
      });
    this.loadDetail$ = this.store.pipe(
      select(getLoadBoardSelectedLoad),
      tap(detail => {
        this._detail = detail;
        this.triggerLoadStatusLoadAction();
      })
    );
    this.ratingQuestions$ = this.shippingStore.pipe(select(getRatingQuestions));
    this.loadStatus$ = this.coreStore.pipe(select(getLoadStatusDetail));
    this.loadingStatus$ = this.coreStore.pipe(select(getLoadStatusLoading));
    this.allStatuses$ = this.coreStore.pipe(select(getAllLoadStatusDetails));
    this.loadingAllStatuses$ = this.coreStore.pipe(select(getLoadStatusLoadingAll));
    this.loadDocumentTypes$ = this.store.pipe(select(getLoadDocumentTypes));
    this.loadingDocuments$ = this.store.pipe(select(getLoadDocumentLoading));
    this.store.dispatch(new LoadDocumentLoadTypesAction());

    this.store.pipe(select(getLoadDocumentDownload), takeUntil(this.destroyed$)).subscribe(x => this.handleFileDownload(x));
  }
  ngOnDestroy(): void {
    this.destroyed$.next(true);
    this.destroyed$.complete();
  }
  setIsShippingDetail() {
    if (this.route.pathFromRoot && this.route.pathFromRoot.length > 1 && this.route.pathFromRoot[1].url) {
      this.route.pathFromRoot[1].url.pipe(take(1)).subscribe(segments => {
        if (segments && segments.length > 0) {
          this.isShippingDetail = segments[0].path && segments[0].path.toLowerCase() === 'shipping';
        }
      });
    }
  }

  book(load: Load) {
    this.store.dispatch(new LoadBoardLoadBookAction(load));
    this.onHide();
  }

  removeCarrier(payload: RemoveCarrierData) {
    this.store.dispatch(new LoadDetailCarrierRemovedAction(payload));
    this.onHide();
  }

  deleteLoad(payload: string) {
    this.store.dispatch(new ShippingLoadDetailDeleteLoadAction(payload));
    this.onHide();
  }

  deleteDetailLoad(payload: RemoveLoadData) {
    this.store.dispatch(new LoadDetailDeleteLoadAction(payload));
    this.onHide();
  }

  onHide() {
    // navigate up two segments from our detail/:id path
    this.router.navigate(['../..'], { relativeTo: this.route });
  }

  private triggerLoadStatusLoadAction() {
    const detail = this._detail;
    if (
      detail &&
      detail.isPlatformPlus &&
      this.isShippingDetail &&
      detail.loadTransaction &&
      detail.loadTransaction.transactionType === TransactionType.Accepted
    ) {
      this.store.dispatch(
        new LoadStatusLoadAction({
          loadId: detail.loadId,
          referenceLoadId: detail.platformPlusLoadId || detail.referenceLoadId,
        })
      );
    }
  }

  loadAllStatuses() {
    const detail = this._detail;
    if (
      detail &&
      detail.isPlatformPlus &&
      this.isShippingDetail &&
      detail.loadTransaction &&
      detail.loadTransaction.transactionType === TransactionType.Accepted
    ) {
      this.store.dispatch(
        new LoadStatusLoadAllAction({
          referenceLoadId: detail.platformPlusLoadId || detail.referenceLoadId,
        })
      );
    }
  }

  loadCurrentStatus() {
    this.triggerLoadStatusLoadAction();
  }

  deleteDocument(doc: LoadDocumentMetadata): void {
    this.store.dispatch(new LoadDocumentDeleteDocumentAction(doc));
  }

  downloadDocument(doc: LoadDocumentMetadata): void {
    this.store.dispatch(new LoadDocumentDownloadDocumentAction(doc));
  }

  private handleFileDownload(fileDownload: LoadDocumentDownload): void {
    if (!fileDownload) {
      return;
    }
    // IE doesn't allow using a blob object directly as link href
    // instead it is necessary to use msSaveOrOpenBlob
    if (window.navigator && window.navigator.msSaveOrOpenBlob) {
      window.navigator.msSaveOrOpenBlob(fileDownload.file);

      // clear the document from the store
      this.store.dispatch(new LoadDocumentDownloadDocumentClearAction());
      return;
    }

    // For other browsers:
    // Create a link pointing to the ObjectURL containing the blob.
    const data = window.URL.createObjectURL(fileDownload.file);

    const link = document.createElement('a');
    link.href = data;
    link.download = fileDownload.metadata.fileName;
    // this is necessary as link.click() does not work on the latest firefox
    link.dispatchEvent(
      new MouseEvent('click', {
        bubbles: true,
        cancelable: true,
        view: window,
      })
    );
    this.store.dispatch(new LoadDocumentDownloadDocumentClearAction());

    setTimeout(function() {
      // For Firefox it is necessary to delay revoking the ObjectURL
      window.URL.revokeObjectURL(data);
      link.remove();
    }, 100);
  }
}
