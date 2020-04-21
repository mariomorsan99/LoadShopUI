import { LoadDocumentType } from '.';

export interface LoadDocumentUpload {
  loadId: string;
  file: File;
  loadDocumentType: LoadDocumentType;
  comment: string;
}
