import { NgModule } from '@angular/core';
import { EffectsModule } from '@ngrx/effects';
import { StoreModule } from '@ngrx/store';

import { LoadBoardRoutingModule } from './load-board.routing.module';
import { SharedModule } from '../shared/shared.module';

import { effects, reducers } from './store';
import { LoadBoardService } from './services';

import {
  LoadBoardComponent,
  LoadGridComponent,
  ButtonToggleComponent,
  SearchContainerComponent,
  BookedContainerComponent,
  TopSearchCriteriaComponent,
  LoadStatusContainerComponent,
  LoadStatusComponent,
  DeliveredContainerComponent,
} from './components';

@NgModule({
  imports: [LoadBoardRoutingModule, SharedModule, EffectsModule.forFeature(effects), StoreModule.forFeature('loadboard', reducers)],
  declarations: [
    LoadBoardComponent,
    LoadGridComponent,
    TopSearchCriteriaComponent,
    ButtonToggleComponent,
    SearchContainerComponent,
    BookedContainerComponent,
    LoadStatusContainerComponent,
    LoadStatusComponent,
    DeliveredContainerComponent,
  ],
  providers: [LoadBoardService],
})
export class LoadBoardModule {}
