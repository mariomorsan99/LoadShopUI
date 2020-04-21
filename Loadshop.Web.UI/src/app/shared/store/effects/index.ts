import { LoadBoardLoadBookEffects } from './load-board-load-book.effects';
import { LoadBoardLoadDetailEffects } from './load-board-load-detail.effects';
import { LoadDocumentEffects } from './load-document.effects';
import { UserCommunicationDisplayEffects } from './user-communication-display.effects';
import { FeedbackEffects } from './feedback.effects';

export const effects: any[] = [
    LoadBoardLoadBookEffects,
    LoadBoardLoadDetailEffects,
    UserCommunicationDisplayEffects,
    LoadDocumentEffects,
    FeedbackEffects,
];

export * from './load-board-load-book.effects';
export * from './load-board-load-detail.effects';
export * from './load-document.effects';
export * from './user-communication-display.effects';
export * from './feedback.effects';
