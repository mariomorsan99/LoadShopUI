import { createSelector } from '@ngrx/store';
import { CoreState } from '../reducers';
import { getEntities, SmartSpotPriceState, getAllSmartSpots } from '../reducers/smart-spot-price.reducer';
import { SmartSpotPrice } from 'src/app/shared/models';

export const getSmartSpotPriceState = (state: CoreState) => state.smartSpotPrice;
export const getSmartSpotPrices = createSelector(getSmartSpotPriceState, getEntities);
export const getSmartSpotPrice = createSelector(getSmartSpotPriceState,
    (state: SmartSpotPriceState, props: { loadId: string }) => state.entities[props.loadId]);
export const getSmartSpotLoading = createSelector(getSmartSpotPriceState,
    (state: SmartSpotPriceState, props: { loadId: string }) => state.entities[props.loadId] ? state.entities[props.loadId].loading : true );

const __getAnySmartSpotsLoading = createSelector(getAllSmartSpots,
    (smartSpots: SmartSpotPrice[]) => {
        const loading = smartSpots.filter(x => x.loading);
        return (loading && loading.length > 0);
    });
export const getAnySmartSpotsLoading = createSelector(getSmartSpotPriceState, __getAnySmartSpotsLoading);

export const getSmartSpotQuickQuoteEvent = createSelector(getSmartSpotPriceState, (state) => state.quickQuoteEvent);
export const getSmartSpotQuickQuoteValue = createSelector(getSmartSpotPriceState, (state) => state.quickQuoteValue);
export const getSmartSpotLoadingQuote = createSelector(getSmartSpotPriceState, (state) => state.loadingQuote);
export const getSmartSpotQuickQuoteErrors = createSelector(getSmartSpotPriceState, (state) => state.quickQuoteProblemDetails);
export const getQuickQuoteCreateOrderDetails = createSelector(getSmartSpotPriceState, (state) => state.createOrderQuoteDetails);
