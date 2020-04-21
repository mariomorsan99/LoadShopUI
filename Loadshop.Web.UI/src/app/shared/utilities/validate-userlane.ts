import { UserLane } from '../models';
import { Message } from 'primeng/api';

export function validateUserlane(lane: UserLane): Message[] {
  const errors = [];
  const validOrigin = validPoint(lane.origCity, lane.origState, lane.origCountry, lane.origLat, lane.origLng);
  const validDest = validPoint(lane.destCity, lane.destState, lane.destCountry, lane.destLat, lane.destLng);
  if (!validOrigin && !validDest) {
    errors.push({ severity: 'error', detail: 'Origin or destination needs to be set' });
  }

  if (!lane.equipmentIds || lane.equipmentIds.length === 0) {
    errors.push({ severity: 'error', detail: 'At least one equipment type must be selected' });
  }
  return errors;
}

function validPoint(city: string, state: string, country: string, lat: number, lng: number): boolean {

  // State only check
  if (!lat && !lng && state) {
    return true;
  }

  // Lat/Lng check
  if (!city || !state || !country) {
    return false;
  }

  if (!lat || !lng) {
    return false;
  }

  return true;
}
