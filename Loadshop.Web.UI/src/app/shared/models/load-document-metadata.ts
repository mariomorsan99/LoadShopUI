import { LoadDocumentType } from '.';

export interface LoadDocumentMetadata {
  loadDocumentId: string;
  loadId: string;
  loadDocumentType: LoadDocumentType;
  fileName: string;
  comment: string;
  createdBy: string;
  created: Date;
}
