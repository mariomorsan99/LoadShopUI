import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '@tms-ng/core';
import { CanDeactivateGuard } from '../core/guards/can-deactivate.guard';
import { LoadDetailContainerComponent } from '../shared/components';
import { AppActionGuard } from '../shared/guards/appaction.guard';
import { SecurityAppActionType } from '../shared/models/security-app-action-type';
import { ShippingLoadCreateContainerComponent, ShippingLoadHomeContainerComponent, ShippingPostedContainerComponent } from './components';
import { ShippingBookedContainerComponent } from './components/shipping-booked-container';
import { ShippingDeliveredContainerComponent } from './components/shipping-delivered-container';

const routes: Routes = [
  {
    path: 'shipping',
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        redirectTo: 'home',
        pathMatch: 'full',
      },
      {
        path: 'home',
        pathMatch: 'full',
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.ShipperViewActiveLoads] },
        component: ShippingLoadHomeContainerComponent,
      },
      {
        // Path includes home so the Post tab remains highlighted
        path: 'home/create',
        pathMatch: 'full',
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.CreateManualLoad] },
        component: ShippingLoadCreateContainerComponent,
        canDeactivate: [CanDeactivateGuard],
      },
      {
        // Path includes home so the Post tab remains highlighted
        path: 'home/edit/:id',
        pathMatch: 'full',
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.EditManualLoad] },
        component: ShippingLoadCreateContainerComponent,
        canDeactivate: [CanDeactivateGuard],
      },
      {
        path: 'marketplace',
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.ShipperViewPostedLoads] },
        component: ShippingPostedContainerComponent,
        children: [{ path: 'detail/:id', component: LoadDetailContainerComponent, data: { sidebar: true } }],
      },
      {
        path: 'booked',
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.ShipperViewBookedLoads] },
        component: ShippingBookedContainerComponent,
        children: [{ path: 'detail/:id', component: LoadDetailContainerComponent, data: { sidebar: true, captureReasonOnDelete: true } }],
      },
      {
        path: 'delivered',
        canActivate: [AppActionGuard],
        data: { roles: [SecurityAppActionType.ShipperViewDeliveredLoads] },
        component: ShippingDeliveredContainerComponent,
        children: [{ path: 'detail/:id', component: LoadDetailContainerComponent, data: { sidebar: true } }],
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ShippingRoutingModule {}
