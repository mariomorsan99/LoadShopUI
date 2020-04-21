export interface T2GLoadStatus {
    loadNbr: string;
    statusTime: Date;
    dateUTCMilliseconds: Date;
    dateOffset: string;
    dateTimezone: string;
    dateLabel: string;
    locationLabel: string;
    lastChgDtTm: Date;
    stopNbr: number;
    descriptionShor: string;
    descriptionLong: string;
    codeId: string;
    latitude: number;
    longitude: number;
    city: string;
    state: string;
    country: string;
}

export const defaultT2GLoadStatus: T2GLoadStatus = {
    loadNbr: null,
    statusTime: null,
    dateUTCMilliseconds: null,
    dateOffset: null,
    dateTimezone: null,
    dateLabel: null,
    locationLabel: null,
    lastChgDtTm: null,
    stopNbr: null,
    descriptionShor: null,
    descriptionLong: null,
    codeId: null,
    latitude: null,
    longitude: null,
    city: null,
    state: null,
    country: null
};
