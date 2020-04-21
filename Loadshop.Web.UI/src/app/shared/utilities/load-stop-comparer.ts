import { LoadStop } from '../models';

export function LoadStopComparer(a: LoadStop, b: LoadStop): number {
  if (a.stopNbr < b.stopNbr) {
    return -1;
  }
  if (a.stopNbr > b.stopNbr) {
    return 1;
  }
  return 0;
}
