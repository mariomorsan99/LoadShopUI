import { NgModule } from '@angular/core';
import { EffectsModule } from '@ngrx/effects';
import { StoreModule } from '@ngrx/store';
import { SharedModule } from '../shared/shared.module';
import { AdminRoutingModule } from './admin.routing.module';
import {
  AdminContainerComponent,
  CarrierProfileContainerComponent,
  CarrierProfileFormComponent,
  LoadCarrierGroupContainerComponent,
  LoadCarrierGroupDetailComponent,
  LoadCarrierGroupDetailContainerComponent,
  LoadCarrierGroupGridComponent,
  ShipperMappingModalComponent,
  ShipperProfileContainerComponent,
  SpecialInstructionsContainerComponent,
  SpecialInstructionsDetailComponent,
  SpecialInstructionsDetailContainerComponent,
  SpecialInstructionsGridComponent,
  UserAdminComponent,
  UsersContainerComponent,
  UserCommunicationContainerComponent,
  UserCommunicationGridComponent,
  UserCommunicationFormContainerComponent,
  UserCommunicationFormComponent
} from './components';
import {
  LoadCarrierGroupService,
  ShipperProfileService,
  UserAdminService,
  CarrierProfileService,
  UserCommunicationService
} from './services';
import { SpecialInstructionsService } from './services/special-instructions.service';
import { effects, reducers } from './store';

@NgModule({
  imports: [AdminRoutingModule, SharedModule, EffectsModule.forFeature(effects), StoreModule.forFeature('admin', reducers)],
  declarations: [
    AdminContainerComponent,
    UsersContainerComponent,
    UserAdminComponent,
    ShipperMappingModalComponent,
    ShipperProfileContainerComponent,
    SpecialInstructionsContainerComponent,
    CarrierProfileContainerComponent,
    CarrierProfileFormComponent,
    LoadCarrierGroupContainerComponent,
    LoadCarrierGroupGridComponent,
    LoadCarrierGroupDetailContainerComponent,
    LoadCarrierGroupDetailComponent,
    SpecialInstructionsGridComponent,
    SpecialInstructionsDetailContainerComponent,
    SpecialInstructionsDetailComponent,
    UserCommunicationContainerComponent,
    UserCommunicationGridComponent,
    UserCommunicationFormContainerComponent,
    UserCommunicationFormComponent
  ],
  providers: [
    LoadCarrierGroupService,
    ShipperProfileService,
    UserAdminService,
    SpecialInstructionsService,
    CarrierProfileService,
    UserCommunicationService],
})
export class AdminModule { }
