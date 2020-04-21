import { LoadBoardDashboardEffects } from './load-board-dashboard.effects';
import { LoadBoardBookedEffects } from './load-board-booked.effects';
import { LoadBoardDeliveredEffects } from './load-board-delivered.effects';

export const effects: any[] = [LoadBoardDashboardEffects, LoadBoardBookedEffects, LoadBoardDeliveredEffects];

export * from './load-board-dashboard.effects';
export * from '../../../shared/store/effects/load-board-load-detail.effects';
export * from '../../../shared/store/effects/load-board-load-book.effects';
export * from './load-board-booked.effects';
export * from './load-board-delivered.effects';
