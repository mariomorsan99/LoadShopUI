import { NgModule } from '@angular/core';
import { PreloadAllModules, RouterModule, Routes } from '@angular/router';
import { NotFoundComponent } from '@tms-ng/core';
import { NoCarrierContainerComponent } from './core/components/no-carrier';
import { AgreementsComponent, UserAgreementComponent } from './core/components/user-agreement';
import { RootGuard } from './shared/guards/root.guard';
import { UserFocusEntityChangedComponent } from './user/components';

const routes: Routes = [
  {
    path: 'loads/dashboard',
    redirectTo: '/loads/search',
  },
  {
    path: '',
    pathMatch: 'full',
    canActivate: [RootGuard],
    children: [],
  },
  {
    path: 'signout-callback-oidc',
    redirectTo: '/loads/search',
    pathMatch: 'full',
  },
  {
    path: 'change-entity',
    pathMatch: 'full',
    component: UserFocusEntityChangedComponent,
  },
  {
    path: 'quick-quote',
    component: NoCarrierContainerComponent,
    outlet: 'modal',
  },
  {
    path: 'invalid',
    component: NoCarrierContainerComponent,
  },
  {
    path: 'agreements',
    children: [
      { path: '', component: AgreementsComponent },
      { path: ':documentType', component: AgreementsComponent },
    ],
  },
  {
    path: 'user-agreement',
    component: UserAgreementComponent,
  },
  {
    path: '**',
    component: NotFoundComponent,
  },
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes, {
      preloadingStrategy: PreloadAllModules,
      onSameUrlNavigation: 'reload',
    }),
  ],
  exports: [RouterModule],
})
export class AppRoutingModule {}
