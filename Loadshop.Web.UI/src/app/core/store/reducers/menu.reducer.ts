import { MenuItem } from 'primeng/api';
import { VisibilityBadge } from 'src/app/shared/models';
import { MenuActions, MenuActionTypes } from '../actions';

export interface MenuState {
  loading: boolean;
  loaded: boolean;
  entities: MenuItem[];
  visibilityBadge: VisibilityBadge;
}

const initialState: MenuState = {
  loading: false,
  loaded: false,
  entities: [],
  visibilityBadge: null,
};

export function menuReducer(state: MenuState = initialState, action: MenuActions): MenuState {
  switch (action.type) {
    case MenuActionTypes.Load:
    case MenuActionTypes.Update: {
      return Object.assign({}, state, {
        loading: true,
      });
    }
    case MenuActionTypes.Load_Success:
    case MenuActionTypes.Update_Success: {
      const data = action.payload;
      return Object.assign({}, state, {
        loading: false,
        loaded: true,
        entities: data,
      });
    }
    case MenuActionTypes.Load_Failure:
    case MenuActionTypes.Update_Failure: {
      return Object.assign({}, state, {
        loading: false,
        loaded: false,
      });
    }
    case MenuActionTypes.Visibility_Badge_Load_Success: {
      const visibilityBadge = action.payload;
      if (!visibilityBadge) {
        return state;
      }

      const menuItems = [...state.entities];
      const index = menuItems.findIndex(x => x.label === 'Booked');
      if (index >= 0 && index < menuItems.length) {
        // Update the Booked menu item's badge property if the menu item is found
        if (visibilityBadge.numRequiringInfo > 0) {
          menuItems[index].badge = visibilityBadge.numRequiringInfo.toString();
        } else {
          menuItems[index].badge = null;
        }
      }

      return {
        ...state,
        entities: menuItems,
        visibilityBadge: visibilityBadge,
      };
    }
    default:
      return state;
  }
}

export const getLoading = (state: MenuState) => state.loading;
export const getLoaded = (state: MenuState) => state.loaded;
export const getEntities = (state: MenuState) => state.entities;
export const getVisibilityBadge = (state: MenuState) => state.visibilityBadge;
