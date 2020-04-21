import { createSelector } from '@ngrx/store';
import { CoreState } from '../reducers';
import { LoadStatusState, getLoading, getSaving, getLoadingAll } from '../reducers/load-status.reducer';
import { LoadStatusDetail } from 'src/app/shared/models/load-status-detail';
import { LoadStatusTypes, T2GLoadStatus, LoadStatusTransaction } from 'src/app/shared/models';

export const getLoadStatusState = (state: CoreState) => state.loadStatus;
export const getLoadStatusLoading = createSelector(getLoadStatusState, (state: LoadStatusState) => getLoading(state));
export const getLoadStatusLoadingAll = createSelector(getLoadStatusState, (state: LoadStatusState) => getLoadingAll(state));
export const getLoadStatusSaving = createSelector(getLoadStatusState, (state: LoadStatusState) => getSaving(state));
export const getLoadStatusErrors = createSelector(getLoadStatusState, (state: LoadStatusState) => state ? state.problemDetails : null);


function getStatus(status: T2GLoadStatus) {
    const codeId = (status ? status.codeId : null) || '';
    switch (codeId.toUpperCase()) {
        case 'X3':
        case 'X1':
            return LoadStatusTypes.Arrived;
        case 'AF':
        case 'CD':
            return LoadStatusTypes.Departed;
        case 'D1':
            return LoadStatusTypes.Delivered;
        case 'X6':
        case 'ZZ':
            return LoadStatusTypes.InTransit;
        default:
            return null;
    }
}

function mergeStatuses(loading: boolean, tops2GoStatus: T2GLoadStatus, loadshopStatus: LoadStatusTransaction) {
    const processing = loadshopStatus && (!tops2GoStatus || tops2GoStatus.lastChgDtTm < loadshopStatus.messageTime);
    const noStatus = !tops2GoStatus;
    return {
        processingUpdates: processing,
        stopNumber: tops2GoStatus ? tops2GoStatus.stopNbr : '',
        description: loading ? 'Loading Status' :
            processing ? 'Processing Status Updates' :
                noStatus ? 'No Status Available' : tops2GoStatus.descriptionLong,
        status: getStatus(tops2GoStatus),
        dateLabel: (tops2GoStatus && !processing && !loading) ? tops2GoStatus.dateLabel : null,
        locationLabel: (tops2GoStatus && !processing && !loading) ? tops2GoStatus.locationLabel : null
    } as LoadStatusDetail;
}

const getTops2GoStatus = createSelector(getLoadStatusState, state => state.tops2GoStatus);
const getLoashopStatus = createSelector(getLoadStatusState, state => state.loadshopStatus);
export const getLoadStatusDetail = createSelector(getLoadStatusLoading, getTops2GoStatus, getLoashopStatus,
    (loading, tops2GoStatus, loadshopStatus) => mergeStatuses(loading, tops2GoStatus, loadshopStatus)
);

const getAllStatuses = createSelector(getLoadStatusState, state => state.allTops2GoStatuses);
export const getAllLoadStatusDetails = createSelector(getLoadStatusLoadingAll, getAllStatuses, (loading, allStatuses) => {
    return allStatuses == null || allStatuses.length === 0
        ? [ mergeStatuses(loading, null, null) ]
        : allStatuses.map(_ => mergeStatuses(loading, _, null));
});
