export interface SmartSpotPrice {
    price: number;
    loadId: string;
    datGuardRate: number;
    machineLearningRate: number;
    loading: boolean;
}

export const defaultSmartSpotPrice: SmartSpotPrice = {
    price: 0.00,
    loadId: null,
    datGuardRate: null,
    machineLearningRate: null,
    loading: true
};
