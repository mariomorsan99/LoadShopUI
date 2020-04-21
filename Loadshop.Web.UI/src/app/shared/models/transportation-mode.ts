export interface TransportationMode {
  transportationModeId: string;
  name: string;
}

export const defaultTransportationMode: TransportationMode = {
  transportationModeId: '1',
  name: 'TRUCK',
};
