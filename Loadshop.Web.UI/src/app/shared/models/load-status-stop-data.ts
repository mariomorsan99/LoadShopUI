import { LoadStatusStopEventData } from './load-status-stop-event-data';

export interface LoadStatusStopData {
    loadId: string;
    events: LoadStatusStopEventData[];
}
