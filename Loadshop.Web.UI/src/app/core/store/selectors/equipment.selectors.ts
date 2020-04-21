import { createSelector } from '@ngrx/store';
import { CoreState } from '../reducers';
import { EquipmentState, getEntities } from '../reducers/equipment.reducer';

export const getEquipmentState = (state: CoreState) => state.equipment;
export const getEquipment = createSelector(getEquipmentState, getEntities);
export const getLoadingEquipment = createSelector(getEquipmentState, (x: EquipmentState) => x.loading);
