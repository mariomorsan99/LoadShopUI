import { LoadStatusTypes } from './load-status-types';

export interface LoadStatusDetail {
    processingUpdates: boolean;
    stopNumber: number;
    status: LoadStatusTypes;
    description: string;
    dateLabel: string;
    locationLabel: string;
}
