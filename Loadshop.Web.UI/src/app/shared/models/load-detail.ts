import { Load, defaultLoad } from './load';

export interface LoadDetail extends Load {
  viewOnly: boolean;
  isEstimatedFSC: boolean;
  billingLoadId: string;
  billingLoadDisplay: string;
}

export const defaultLoadDetail: LoadDetail = {
  ...defaultLoad,
  viewOnly: false,
  isEstimatedFSC: null,
  billingLoadId: null,
  billingLoadDisplay: null
};
