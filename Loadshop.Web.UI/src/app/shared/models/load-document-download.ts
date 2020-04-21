import { LoadDocumentMetadata } from '.';

export interface LoadDocumentDownload {
  metadata: LoadDocumentMetadata;
  file: Blob;
}
