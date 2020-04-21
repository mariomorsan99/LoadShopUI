import { SelectItemGroup } from 'primeng/api';
import { UserFocusEntity } from './user-focus-entity';

export interface UserFocusEntityResult {
  focusEntites: UserFocusEntity[];
  groupedFocusEntities: SelectItemGroup[];
}

export const defaultUserFocusEntityResult: UserFocusEntityResult = {
  focusEntites: [],
  groupedFocusEntities: [],
};
