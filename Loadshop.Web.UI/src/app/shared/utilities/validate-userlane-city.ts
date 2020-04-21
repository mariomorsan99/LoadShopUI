import { UserLane } from '../models';
import { Message } from 'primeng/api';

export function validateUserlaneCity(lane: UserLane): Message[] {
  const errors = [];
  if (!lane.origLat && !lane.origLng && !lane.destLat && !lane.destLng) {
    errors.push({ severity: 'error', detail: 'Origin or destination needs to be set' });
  }
  return errors;
}
