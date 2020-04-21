import { UserLane } from '../models';

export function deepCopyLane(lane: UserLane): UserLane {
  return Object.assign({}, lane, {
    laneNotifications: Object.assign({}, lane.laneNotifications)
  });
}
