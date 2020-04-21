import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '@tms-ng/core';
import { AppActionGuard, AppActionOperatorType } from '../shared/guards';
import { SecurityAppActionType } from '../shared/models/security-app-action-type';
import {
  AdminContainerComponent,
  CarrierProfileContainerComponent,
  ShipperProfileContainerComponent,
  SpecialInstructionsContainerComponent,
  SpecialInstructionsDetailContainerComponent,
  UsersContainerComponent,
  UserCommunicationContainerComponent,
  UserCommunicationFormContainerComponent,
} from './components';
import { LoadCarrierGroupContainerComponent, LoadCarrierGroupDetailContainerComponent } from './components/load-carrier-group';
import { AdminGuard } from './guards/admin.guard';

const routes: Routes = [
  {
    // if root path changes be sure to update AdminGuard and CommonService.getMenuItems
    path: 'maint',
    component: AdminContainerComponent,
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        component: AdminContainerComponent,
        canActivate: [AdminGuard],
      },
      {
        path: 'users',
        component: UsersContainerComponent,
        canActivate: [AppActionGuard],
        data: {
          roles: [SecurityAppActionType.CarrierUserAddEdit, SecurityAppActionType.ShipperUserAddEdit],
          operator: AppActionOperatorType.Any,
        },
      },
      {
        path: 'shipper-profile',
        component: ShipperProfileContainerComponent,
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.ShipperAddEdit] },
      },
      {
        path: 'special-instructions',
        component: SpecialInstructionsContainerComponent,
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.SpecialInstructionsAddEdit] },
      },
      {
        path: 'special-instructions/detail/:id',
        component: SpecialInstructionsDetailContainerComponent,
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.SpecialInstructionsAddEdit] },
      },
      {
        path: 'special-instructions/detail',
        component: SpecialInstructionsDetailContainerComponent,
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.SpecialInstructionsAddEdit] },
      },
      {
        path: 'carrier-groups',
        component: LoadCarrierGroupContainerComponent,
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.ShipperCarrierGroupsAddEdit] },
      },
      {
        path: 'carrier-groups/detail/:id',
        component: LoadCarrierGroupDetailContainerComponent,
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.ShipperCarrierGroupsAddEdit] },
      },
      {
        path: 'carrier-groups/detail',
        component: LoadCarrierGroupDetailContainerComponent,
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.ShipperCarrierGroupsAddEdit] },
      },
      {
        path: 'carrier-profile',
        component: CarrierProfileContainerComponent,
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.CarrierAddEdit] },
      },
      {
        path: 'user-communications',
        component: UserCommunicationContainerComponent,
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.UserCommunicationAddEdit] },
      },
      {
        path: 'user-communications/detail/:id',
        component: UserCommunicationFormContainerComponent,
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.UserCommunicationAddEdit] },
      },
      {
        path: 'user-communications/detail',
        component: UserCommunicationFormContainerComponent,
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.UserCommunicationAddEdit] },
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  providers: [AdminGuard],
  exports: [RouterModule],
})
export class AdminRoutingModule {}
