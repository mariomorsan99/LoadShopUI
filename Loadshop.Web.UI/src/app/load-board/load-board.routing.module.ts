import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '@tms-ng/core';
import { LoadDetailContainerComponent } from '../shared/components';
import { AppActionGuard } from '../shared/guards';
import { SecurityAppActionType } from '../shared/models/security-app-action-type';
import {
  LoadBoardComponent,
  SearchContainerComponent,
  BookedContainerComponent,
  LoadStatusContainerComponent,
  DeliveredContainerComponent,
} from './components';

const routes: Routes = [
  {
    path: 'loads',
    component: LoadBoardComponent,
    canActivate: [AuthGuard],
    children: [
      {
        path: 'search',
        component: SearchContainerComponent,
        children: [{ path: 'detail/:id', component: LoadDetailContainerComponent, data: { sidebar: true } }],
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.CarrierMarketPlaceView] },
      },
      {
        path: 'booked',
        component: BookedContainerComponent,
        children: [{ path: 'detail/:id', component: LoadDetailContainerComponent, data: { sidebar: true, captureReasonOnDelete: true } }],
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.CarrierMyLoadsView] },
      },
      {
        path: 'delivered',
        component: DeliveredContainerComponent,
        children: [{ path: 'detail/:id', component: LoadDetailContainerComponent, data: { sidebar: true, captureReasonOnDelete: true } }],
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.CarrierViewDelivered] },
      },
      {
        path: 'detail/:id',
        component: LoadDetailContainerComponent,
        canActivate: [AppActionGuard],
        data: { sidebar: false, roles: [SecurityAppActionType.ViewDetail] },
      },
      {
        path: 'status/:id',
        canActivate: [AppActionGuard],
        component: LoadStatusContainerComponent,
        data: { roles: [SecurityAppActionType.CarrierViewStatus] },
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class LoadBoardRoutingModule {}
