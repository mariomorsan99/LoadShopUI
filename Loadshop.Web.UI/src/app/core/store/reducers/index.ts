import { ActionReducer, ActionReducerMap, MetaReducer } from '@ngrx/store';
import { environment } from '../../../../environments/environment';
import { adminMenuReducer, AdminMenuState } from './admin-menu.reducer';
import {
  appointmentSchedulerConfirmationTypeReducer,
  AppointmentSchedulerConfirmationTypeState,
} from './appointment-scheduler-confirmation-type.reducer';
import { browserReducer, BrowserState } from './browser.reducer';
import { carrierReducer, CarrierState } from './carrier.reducer';
import { commodityReducer, CommodityState } from './commodity.reducer';
import { CustomerLoadTypeState, customerLoadTypeReducer } from './customer-load-type.reducer';
import { customerReducer, CustomerState } from './customer.reducer';
import { equipmentReducer, EquipmentState } from './equipment.reducer';
import { menuReducer, MenuState } from './menu.reducer';
import { serviceTypeReducer, ServiceTypeState } from './service-type.reducer';
import { stateReducer, StateState } from './state.reducer';
import { transportationModeReducer, TransportationModeState } from './transportation-mode.reducer';
import { unitOfMeasureReducer, UnitOfMeasureState } from './unit-of-measure.reducer';
import { smartSpotPriceReducer, SmartSpotPriceState } from './smart-spot-price.reducer';
import { loadStatusReducer, LoadStatusState } from './load-status.reducer';
import { AgreementDocumentState, agreementDocumentReducer } from './agreement-document.reducer';

export interface CoreState {
  menu: MenuState;
  states: StateState;
  equipment: EquipmentState;
  browser: BrowserState;
  carriers: CarrierState;
  commodities: CommodityState;
  customer: CustomerState;
  customerLoadTypes: CustomerLoadTypeState;
  adminMenu: AdminMenuState;
  unitOfMeasure: UnitOfMeasureState;
  transportationMode: TransportationModeState;
  serviceType: ServiceTypeState;
  appointmentSchedulerConfirmationType: AppointmentSchedulerConfirmationTypeState;
  smartSpotPrice: SmartSpotPriceState;
  loadStatus: LoadStatusState;
  agreementDocumentState: AgreementDocumentState;
}

export const reducers: ActionReducerMap<CoreState> = {
  menu: menuReducer,
  states: stateReducer,
  equipment: equipmentReducer,
  browser: browserReducer,
  carriers: carrierReducer,
  commodities: commodityReducer,
  customer: customerReducer,
  customerLoadTypes: customerLoadTypeReducer,
  adminMenu: adminMenuReducer,
  unitOfMeasure: unitOfMeasureReducer,
  transportationMode: transportationModeReducer,
  serviceType: serviceTypeReducer,
  appointmentSchedulerConfirmationType: appointmentSchedulerConfirmationTypeReducer,
  smartSpotPrice: smartSpotPriceReducer,
  loadStatus: loadStatusReducer,
  agreementDocumentState: agreementDocumentReducer,
};

export function logger(reducer: ActionReducer<CoreState>): ActionReducer<CoreState> {
  return function(state: CoreState, action: any): CoreState {
    // console.log('state', state);
    // console.log('action', action);
    return reducer(state, action);
  };
}

export const metaReducers: MetaReducer<CoreState>[] = !environment.production ? [logger] : [];
