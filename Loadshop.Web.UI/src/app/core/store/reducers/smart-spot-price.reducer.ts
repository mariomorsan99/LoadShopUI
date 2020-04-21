import { SmartSpotPriceActions, SmartSpotPriceActionTypes } from '../actions';
import { SmartSpotPrice, ValidationProblemDetails, SmartSpotQuoteCreateRequest, defaultSmartSpotPrice } from '../../../shared/models';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { MenuItem } from 'primeng/api';

export interface SmartSpotPriceState extends EntityState<SmartSpotPrice> {
  quickQuoteEvent: { originalEvent: Event, item: MenuItem };
  quickQuoteValue: number;
  loadingQuote: boolean;
  quickQuoteProblemDetails: ValidationProblemDetails;
  createOrderQuoteDetails: SmartSpotQuoteCreateRequest;
}

export const adapter: EntityAdapter<SmartSpotPrice> = createEntityAdapter<SmartSpotPrice>({
  selectId: x => x.loadId,
});

const initialState: SmartSpotPriceState = adapter.getInitialState({
  quickQuoteEvent: null,
  quickQuoteValue: null,
  loadingQuote: false,
  quickQuoteProblemDetails: null,
  createOrderQuoteDetails: null,
});

export function smartSpotPriceReducer(state: SmartSpotPriceState = initialState, action: SmartSpotPriceActions): SmartSpotPriceState {
  switch (action.type) {
    case SmartSpotPriceActionTypes.Load: {
      const loadingSmartSpots = action.payload
        .map<SmartSpotPrice>(x => {
          return { ...defaultSmartSpotPrice, loadId: x.loadId, loading: true };
        });

      return adapter.upsertMany(loadingSmartSpots, {...state});
    }
    case SmartSpotPriceActionTypes.LoadSuccess: {
      const defaultNewState = { ...state, loading: false };
      if (action && action.payload && Array.isArray(action.payload)) {
        // Only update positive prices, otherwise, consider them invalid and do not save them
        const nonZeroPrices = action.payload.filter(x => x && x.price && x.price > 0.0);
        if (nonZeroPrices && nonZeroPrices.length) {
          const newState = adapter.upsertMany(nonZeroPrices.map(x => ({ ...x, loading: false })), { ...defaultNewState });
          return newState;
        } else {
          return defaultNewState;
        }
      } else {
        return defaultNewState;
      }
    }
    case SmartSpotPriceActionTypes.LoadFailure: {
      // We have no way to know which one failed, so do nothing and let the success handler clear the
      // loading indicators of the ones that succeeded
      return { ...state };
    }
    case SmartSpotPriceActionTypes.ShowQuickQuote: {
      return { ...state, quickQuoteEvent: action.payload };
    }
    case SmartSpotPriceActionTypes.HideQuickQuote: {
      return { ...state, quickQuoteEvent: null };
    }
    case SmartSpotPriceActionTypes.LoadQuote: {
      return { ...state, loadingQuote: true, quickQuoteValue: null, quickQuoteProblemDetails: null };
    }
    case SmartSpotPriceActionTypes.LoadQuoteSuccess: {
      return { ...state, loadingQuote: false, quickQuoteValue: action.payload, quickQuoteProblemDetails: null };
    }
    case SmartSpotPriceActionTypes.LoadQuoteFailure: {
      return { ...state, loadingQuote: false, quickQuoteProblemDetails: action.payload.error };
    }
    case SmartSpotPriceActionTypes.CreateOrderFromQuote: {
      return { ...state, createOrderQuoteDetails: action.payload };
    }
    case SmartSpotPriceActionTypes.ClearCreateOrderFromQuote: {
      return { ...state, createOrderQuoteDetails: null };
    }
    default:
      return state;
  }
}

const selectors = adapter.getSelectors();
export const getEntities = (state: SmartSpotPriceState) => selectors.selectEntities(state);
export const getAllSmartSpots = (state: SmartSpotPriceState) => selectors.selectAll(state);
