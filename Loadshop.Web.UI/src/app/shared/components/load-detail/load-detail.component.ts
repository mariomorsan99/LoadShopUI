import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { ConfirmationService } from 'primeng/api';
import {
  defaultLoadDetail,
  FeedbackQuestionEnum,
  Load,
  LoadDetail,
  LoadDocumentMetadata,
  LoadDocumentType,
  LoadStatusDetail,
  LoadStop,
  RatingQuestion,
  RatingQuestionAnswer,
  RemoveCarrierData,
  RemoveLoadData,
  TransactionType,
  User,
} from '../../models';
import { Directions } from '../../models/directions';
import { LoadStopComparer } from '../../utilities/load-stop-comparer';

@Component({
  selector: 'kbxl-load-detail',
  templateUrl: './load-detail.component.html',
  styleUrls: ['./load-detail.component.scss'],
})
export class LoadDetailComponent implements OnChanges {
  @Input() user: User;
  @Input() load: LoadDetail;
  @Input() loading = false;
  @Input() loadingDocuments = false;
  @Input() isShippingDetail: boolean; // Do not show the view only warning if we're displaying load detail from a shipping tab
  @Input() loadStatus: LoadStatusDetail;
  @Input() loadingStatus: boolean;
  @Input() allStatuses: LoadStatusDetail[];
  @Input() loadingAllStatuses: boolean;
  @Input() captureReasonOnDelete = false;
  @Input() ratingQuestions: RatingQuestion[];
  @Input() loadDocumentTypes: LoadDocumentType[];

  @Output() book: EventEmitter<Load> = new EventEmitter<Load>();
  @Output() deleteLoad: EventEmitter<string> = new EventEmitter<string>();
  @Output() deleteDetailLoad: EventEmitter<RemoveLoadData> = new EventEmitter<RemoveLoadData>();
  @Output() removeCarrier: EventEmitter<RemoveCarrierData> = new EventEmitter<RemoveCarrierData>();
  @Output() deleteDocument: EventEmitter<LoadDocumentMetadata> = new EventEmitter<LoadDocumentMetadata>();
  @Output() loadCurrentStatus = new EventEmitter<object>();
  @Output() loadAllStatuses = new EventEmitter<object>();
  @Output() downloadLoadDocument = new EventEmitter<LoadDocumentMetadata>();

  feedbackQuestionEnum = FeedbackQuestionEnum;

  get accepted() {
    return this.load && this.load.loadTransaction && this.load.loadTransaction.transactionType === TransactionType.Accepted;
  }

  loadStops: LoadStop[];
  origin: LoadStop;
  dest: LoadStop;
  directions: Directions;
  carb = false;
  carbAdj = false;
  carbAdjStates = ['AZ', 'NV', 'OR', 'WA'];
  contacts: string[] = [];
  hasComments = false;
  showRemoveLoadPopup = false;
  showUploadDocumentModal = false;
  removeComment = '';
  removeRatingQuestionAnswer = '';
  removeText = '';
  removeMode = -1; // 1 for remove carrier, 2 for remove load
  viewingCurrentStatus = true;

  constructor(private confirmationService: ConfirmationService) {}

  ngOnChanges() {
    this.hasComments = false;

    this.loading = !this.load || this.load === defaultLoadDetail;

    if (this.load) {
      this.origin = this.load.loadStops[0];
      this.dest = this.load.loadStops[this.load.loadStops.length - 1];
      this.loadStops = this.load.loadStops.sort(LoadStopComparer);
      this.carb = this.origin.state === 'CA' || this.dest.state === 'CA';
      this.carbAdj = !this.carb && (this.carbAdjStates.includes(this.origin.state) || this.carbAdjStates.includes(this.dest.state));
      if (this.load.contacts && this.load.contacts.length) {
        this.contacts = this.load.contacts[0].email.split(';');
      }
      if (
        this.load.comments &&
        this.load.comments.length &&
        this.load.comments.trim() !== '<br>' &&
        this.load.comments.trim() !== '<div><br></div>'
      ) {
        this.hasComments = true;
      }
    } else {
      this.contacts = [];
      this.origin = null;
      this.dest = null;
      this.loadStops = [];
    }
  }

  click() {
    if (this.load && !this.load.viewOnly) {
      let message = `By clicking 'Yes' you are agreeing to pick up and deliver this load based on the terms and price
        displayed with a truck that runs under your companies’ authority (<strong>NO BROKERING OR UTILIZING
         PARTNER CARRIERS ON FREIGHT</strong>). Any unauthorized actions on our freight could lead to suspension
         or removal from platform.`;
      if (this.carb) {
        message += ` You are also confirming the tractor used on this load is CARB compliant with California’s Truck
          and Bus regulation (Title 13, California Code of Regulations, Section 2025).`;
      }
      if (this.carbAdj) {
        message += ` You are also confirming that either this load will not be driven through California or, if so, that the
          tractor used is CARB compliant with California’s Truck and Bus regulation (Title 13, California Code
          of Regulations, Section 2025).`;
      }

      message += `<br/><br/>
        The price displayed includes a fuel surcharge that will govern fuel compensation for this given
        load. By clicking 'Yes' you are agreeing to haul this load for the linehaul and fuel surcharge displayed.
        This electronic acceptance will act as the Rate Confirmation Agreement indicating both parties have signed
        and agreed to the transaction.`;
      this.confirmationService.confirm({
        message: message,
        accept: () => this.book.emit(this.load),
      });
    }
  }

  deleteLoadClick() {
    if (!this.captureReasonOnDelete) {
      // delete is clicked from marketplace, email, or delivered tab, do not show reason modal
      this.confirmationService.confirm({
        message: 'Are you sure you want to completely delete this load from Loadshop?',
        accept: () => this.deleteLoad.emit(this.load.loadId),
      });
    } else {
      // delete is clicked from booked
      this.removeMode = 2;
      this.showRemoveLoadPopup = true;
      this.removeText = 'Are you sure you want to delete this load from Loadshop?';
    }
  }

  removeCarrierClick() {
    this.removeMode = 1;
    this.showRemoveLoadPopup = true;
    this.removeText = 'Are you sure you want to remove the load from the marketplace?';
  }

  setDirections(directions: Directions) {
    this.directions = directions;
  }

  cancelRemovePopup(): void {
    this.showRemoveLoadPopup = false;
    this.removeComment = '';
    this.removeRatingQuestionAnswer = '';
    this.removeText = '';
    this.removeMode = -1;
  }
  removeConfirmed(): void {
    if (this.removeMode < 0) {
      return;
    }
    const ratingQuestionAnswer: RatingQuestionAnswer = {
      ratingQuestionId: this.removeRatingQuestionAnswer,
      answerYN: true,
      additionalComment: this.removeComment,
    };

    if (this.removeMode === 1) {
      const payload: RemoveCarrierData = {
        load: this.load,
        ratingQuestionAnswer: ratingQuestionAnswer,
      };
      this.removeCarrier.emit(payload);
    } else if (this.removeMode === 2) {
      const payload: RemoveLoadData = {
        load: this.load,
        ratingQuestionAnswer: ratingQuestionAnswer,
      };
      this.deleteDetailLoad.emit(payload);
    }
  }

  toggleLoadStatusView(event: Event) {
    event.stopPropagation();
    event.preventDefault();

    if (this.viewingCurrentStatus) {
      this.loadAllStatuses.emit(null);
    } else {
      this.loadCurrentStatus.emit(null);
    }
    this.viewingCurrentStatus = !this.viewingCurrentStatus;
  }

  addDocument(): void {
    this.showUploadDocumentModal = true;
  }

  getDocumentDescription(doc: LoadDocumentMetadata): string {
    if (doc.loadDocumentType.description.toLowerCase() === 'other') {
      return `${doc.loadDocumentType.description} - ${doc.comment}`;
    }
    return doc.loadDocumentType.description;
  }

  removeDocument(doc: LoadDocumentMetadata): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to remove ${doc.fileName}?`,
      accept: () => this.deleteDocument.emit(doc),
    });
  }

  downloadDocument(doc: LoadDocumentMetadata): void {
    this.downloadLoadDocument.emit(doc);
  }

  shouldDisplayDocuments(): boolean {
    if (
      this.load &&
      (this.load.loadTransaction.transactionType === 'PreTender' ||
        this.load.loadTransaction.transactionType === 'Pending' ||
        this.load.loadTransaction.transactionType === 'SentToShipperTender' ||
        this.load.loadTransaction.transactionType === 'Accepted' ||
        this.load.loadTransaction.transactionType === 'Delivered')
    ) {
      return true;
    }
    return false;
  }
  getServiceTypes(): string {
    if (this.load.serviceTypes) {
      return this.load.serviceTypes.map((x) => x.name).join(',');
    }
    return '';
  }
}
