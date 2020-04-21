import { NgModule } from '@angular/core';
import { EffectsModule } from '@ngrx/effects';
import { StoreModule } from '@ngrx/store';
import { SharedModule } from '../shared/shared.module';
import {
  UserFocusEntityChangedComponent,
  UserFocusEntitySelectorComponent,
  UserFocusEntitySelectorContainerComponent,
  UserLaneDetailComponent,
  UserLoadNotificationsComponent,
  UserProfileComponent,
  UserProfileContainerComponent,
  UserSavedLanesComponent,
} from './components';
import { UserLaneDetailContainerComponent } from './components/user-lane-detail-container';
import { UserComponent } from './components/user.component';
import { UserLanesService, UserProfileService } from './services';
import { StateService } from './services/state.service';
import { effects, reducers } from './store';
import { CustomerRoutingModule } from './user.routing.module';

@NgModule({
  imports: [CustomerRoutingModule, SharedModule, EffectsModule.forFeature(effects), StoreModule.forFeature('user', reducers)],
  declarations: [
    UserComponent,
    UserProfileComponent,
    UserProfileContainerComponent,
    UserSavedLanesComponent,
    UserLaneDetailComponent,
    UserLaneDetailContainerComponent,
    UserFocusEntitySelectorComponent,
    UserFocusEntitySelectorContainerComponent,
    UserFocusEntityChangedComponent,
    UserLoadNotificationsComponent,
  ],
  exports: [UserFocusEntitySelectorContainerComponent, UserFocusEntityChangedComponent],
  providers: [UserProfileService, UserLanesService, StateService],
})
export class UserModule {}
