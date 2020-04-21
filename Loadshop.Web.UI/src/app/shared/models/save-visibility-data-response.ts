import { LoadClaim } from './load-claim';
import { VisibilityBadge } from './visibility-badge';

export interface SaveVisibilityDataResponse {
  loadClaim: LoadClaim;
  visibilityBadge: VisibilityBadge;
}
