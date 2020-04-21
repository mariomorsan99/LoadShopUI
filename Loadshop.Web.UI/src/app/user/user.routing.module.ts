import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '@tms-ng/core';
import { CanDeactivateGuard } from '../core/guards/can-deactivate.guard';
import { UserComponent, UserProfileContainerComponent } from './components';
import { UserLaneDetailContainerComponent } from './components/user-lane-detail-container';

const routes: Routes = [
  {
    path: 'user',
    component: UserComponent,
    canActivate: [AuthGuard],
    children: [
      {
        path: 'profile',
        component: UserProfileContainerComponent,
        children: [
          { path: 'lane/details/:id', component: UserLaneDetailContainerComponent },
          { path: 'lane/details', component: UserLaneDetailContainerComponent },
        ],
        canDeactivate: [CanDeactivateGuard],
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class CustomerRoutingModule {}
