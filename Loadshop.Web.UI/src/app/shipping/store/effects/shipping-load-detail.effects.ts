import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { MessageService } from 'primeng/api';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { IShippingLoadDetail, ShippingLoadDetail } from 'src/app/shared/models';
import { IPostingLoad } from 'src/app/shared/models/posting-load';
import { UpdateFocusEntityAction, UserFocusEntitySelectorTypes } from 'src/app/user/store';
import { ShippingService } from '../../services';
import {
  ShippingLoadDetailActionTypes,
  ShippingLoadDetailDeleteLoadAction,
  ShippingLoadDetailDeleteLoadFailureAction,
  ShippingLoadDetailDeleteLoadSuccessAction,
  ShippingLoadDetailLoadAllAction,
  ShippingLoadDetailLoadAllFailureAction,
  ShippingLoadDetailLoadAllSuccessAction,
  ShippingLoadDetailPostLoadsAction,
  ShippingLoadDetailPostLoadsFailureAction,
  ShippingLoadDetailPostLoadsSuccessAction,
  ShippingLoadDetailRemoveLoadAction,
  ShippingLoadDetailRemoveLoadFailureAction,
  ShippingLoadDetailRemoveLoadSuccessAction,
} from '../actions';

@Injectable()
export class ShippingLoadDetailEffects {
  @Effect()
  $loadAll: Observable<Action> = this.actions$.pipe(
    ofType<ShippingLoadDetailLoadAllAction>(ShippingLoadDetailActionTypes.Load_All),
    switchMap(() => {
      return this.shippingService.getLoadsForHomeTab().pipe(
        map((data) => new ShippingLoadDetailLoadAllSuccessAction(data)),
        catchError((err) => of(new ShippingLoadDetailLoadAllFailureAction(err)))
      );
    })
  );

  @Effect()
  $clearAll: Observable<Action> = this.actions$.pipe(
    ofType<UpdateFocusEntityAction>(UserFocusEntitySelectorTypes.UpdateFocusEntity),
    switchMap(() => of(new ShippingLoadDetailLoadAllSuccessAction([])))
  );

  @Effect()
  $postLoads = this.actions$.pipe(
    ofType<ShippingLoadDetailPostLoadsAction>(ShippingLoadDetailActionTypes.Post_Loads),
    map((_) => _.payload.map(this.createPostingLoad)),
    switchMap((loads: IPostingLoad[]) => {
      return this.shippingService.postLoads(loads).pipe(
        map((data) => {
          if (data && data.validationProblemDetails && data.validationProblemDetails.errors) {
            // Load-specific errors will show on each load, but popup a notification error with the title and details as a summary message
            this.messageService.add({
              summary: data.validationProblemDetails.title,
              detail: data.validationProblemDetails.detail,
              severity: 'error',
            });
          }
          return new ShippingLoadDetailPostLoadsSuccessAction(data);
        }),
        catchError((err) => of(new ShippingLoadDetailPostLoadsFailureAction(err)))
      );
    })
  );

  @Effect()
  $removeLoad: Observable<Action> = this.actions$.pipe(
    ofType<ShippingLoadDetailRemoveLoadAction>(ShippingLoadDetailActionTypes.Remove_Load),
    mapToPayload<string>(),
    switchMap((loadId: string) => {
      return this.shippingService.removeLoad(loadId).pipe(
        map((data: IShippingLoadDetail) => new ShippingLoadDetailRemoveLoadSuccessAction(data)),
        catchError((err) => of(new ShippingLoadDetailRemoveLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $deleteLoad: Observable<Action> = this.actions$.pipe(
    ofType<ShippingLoadDetailDeleteLoadAction>(ShippingLoadDetailActionTypes.Delete_Load),
    mapToPayload<string>(),
    switchMap((loadId: string) => {
      return this.shippingService.deleteLoad(loadId).pipe(
        map((data: IShippingLoadDetail) => new ShippingLoadDetailDeleteLoadSuccessAction(data)),
        catchError((err) => of(new ShippingLoadDetailDeleteLoadFailureAction(err)))
      );
    })
  );

  constructor(private actions$: Actions, private shippingService: ShippingService, private messageService: MessageService) {}

  createPostingLoad(load: ShippingLoadDetail): IPostingLoad {
    return {
      loadId: load.loadId,
      shippersFSC: load.shippersFSC,
      lineHaulRate: load.lineHaulRate,
      comments: load.comments,
      commodity: load.commodity,
      carrierIds: (load.selectedGroupCarriers || []).map((_) => _.carrierId).concat((load.selectedCarriers || []).map((_) => _.carrierId)),
      smartSpotRate: load.smartSpotRate,
      datGuardRate: load.datGuardRate,
      machineLearningRate: load.machineLearningRate,
      carrierGroupIds: load.carrierGroupIds,
      allCarriersPosted: load.allCarriersPosted,
      serviceTypeIds: (load.serviceTypes || []).map((_) => _.serviceTypeId),
    };
  }
}
