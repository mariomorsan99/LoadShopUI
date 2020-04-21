// eslint-disable-next-line @typescript-eslint/interface-name-prefix
export interface IPostingLoad {
  loadId: string;
  commodity: string;
  shippersFSC: number;
  lineHaulRate: number;
  comments: string;
  carrierIds: string[];
  smartSpotRate: number;
  datGuardRate: number;
  machineLearningRate: number;
  carrierGroupIds: number[];
  allCarriersPosted: boolean;
  serviceTypeIds: number[];
}
