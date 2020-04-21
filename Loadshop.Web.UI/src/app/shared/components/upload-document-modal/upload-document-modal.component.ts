import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Store } from '@ngrx/store';
import { LoadDocumentType, LoadDocumentUpload } from '../../models';
import { LoadDocumentAddDocumentAction, SharedState } from '../../store';

@Component({
  selector: 'kbxl-upload-document-modal',
  templateUrl: './upload-document-modal.component.html',
  styleUrls: ['./upload-document-modal.component.scss'],
})
export class UploadDocumentModalComponent implements OnInit {
  @Input() documentTypes: LoadDocumentType[];
  @Input() loadId: string;

  @Output() close: EventEmitter<boolean> = new EventEmitter<boolean>();
  selectedDocumentType: LoadDocumentType;
  comment = '';
  file: File | null = null;
  uploadDisabled = true;
  showComment = false;
  constructor(private store: Store<SharedState>) {}

  ngOnInit() {
    // todo
  }

  documentTypeChange(): void {
    this.showComment = false;
    this.uploadDisabled = true;

    if (this.selectedDocumentType && this.selectedDocumentType.description.toLowerCase() === 'other') {
      this.showComment = true;
    } else {
      this.uploadDisabled = false;
    }
  }
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  commentChanged(text: string): void {
    this.uploadDisabled = this.comment.length === 0;
  }
  uploadDocument(event: any): void {
    if (event.target.files.length < 1) {
      return;
    }
    // capture file
    this.file = event.target.files[0];

    const payload: LoadDocumentUpload = {
      loadId: this.loadId,
      file: this.file,
      loadDocumentType: this.selectedDocumentType,
      comment: this.comment,
    };
    this.store.dispatch(new LoadDocumentAddDocumentAction(payload));
    this.close.emit(true);

    // remove file from html input
    event.target.value = null;

    this.reset();
  }

  reset(): void {
    this.selectedDocumentType = null;
    this.uploadDisabled = true;
    this.comment = '';
    this.showComment = false;
    this.file = null;
  }
}
