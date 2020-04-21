import { StopEventTypes } from './stop-event-types';

export interface LoadStatusStopEventData {
    stopNumber: number;
    eventType: StopEventTypes;
    eventTime: string;
}
