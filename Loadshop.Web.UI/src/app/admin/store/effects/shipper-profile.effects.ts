import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { MessageService } from 'primeng/api';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { CustomerProfile } from 'src/app/shared/models';
import { ShipperProfileService } from '../../services';
import {
  ShipperProfileActionTypes,
  ShipperProfileAddAction,
  ShipperProfileAddFailureAction,
  ShipperProfileAddSuccessAction,
  ShipperProfileLoadAction,
  ShipperProfileLoadFailureAction,
  ShipperProfileLoadShippersAction,
  ShipperProfileLoadShippersFailureAction,
  ShipperProfileLoadShippersSuccessAction,
  ShipperProfileLoadSuccessAction,
  ShipperProfileUpdateAction,
  ShipperProfileUpdateFailureAction,
  ShipperProfileUpdateSuccessAction,
  ShipperProfileLoadShipperMappingsAction,
  ShipperProfileLoadSourceSystemOwnerAction,
  ShipperProfileLoadSourceSystemOwnerSuccessAction,
  ShipperProfileLoadShipperMappingsSuccessAction,
  ShipperProfileLoadSourceSystemOwnerFailureAction,
  ShipperProfileLoadShipperMappingsFailureAction,
  ShipperProfileCreateShipperMappingSuccessAction,
  ShipperProfileUpdateShipperMappingSuccessAction,
  ShipperProfileUpdateShipperMappingFailureAction,
  ShipperProfileCreateShipperMappingFailureAction,
  ShipperProfileUpdateShipperMappingAction,
  ShipperProfileCreateShipperMappingAction,
  ShipperProfileEnableShipperApiAction,
  ShipperProfileEnableShipperApiSuccessAction,
  ShipperProfileEnableShipperApiFailureAction,
} from '../actions';
import { LoadshopShipperMapping } from 'src/app/shared/models/loadshop-shipper-mapping';

@Injectable()
export class ShipperProfileEffects {
  @Effect()
  $loadShippers: Observable<Action> = this.actions$.pipe(
    ofType<ShipperProfileLoadShippersAction>(ShipperProfileActionTypes.Load_Shippers),
    switchMap(() => {
      return this.shipperProfileService.getAllShippers().pipe(
        map(data => new ShipperProfileLoadShippersSuccessAction(data)),
        catchError(err => of(new ShipperProfileLoadShippersFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadShipper: Observable<Action> = this.actions$.pipe(
    ofType<ShipperProfileLoadAction>(ShipperProfileActionTypes.Load),
    mapToPayload<{ customerId: string }>(),
    switchMap((payload: { customerId: string }) => {
      return this.shipperProfileService.getShipper(payload.customerId).pipe(
        map(data => new ShipperProfileLoadSuccessAction(data)),
        catchError(err => of(new ShipperProfileLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $createShipper: Observable<Action> = this.actions$.pipe(
    ofType<ShipperProfileAddAction>(ShipperProfileActionTypes.Add),
    mapToPayload<CustomerProfile>(),
    switchMap((customer: CustomerProfile) => {
      return this.shipperProfileService.createShipper(customer).pipe(
        map(data => new ShipperProfileAddSuccessAction(data)),
        catchError(err => of(new ShipperProfileAddFailureAction(err)))
      );
    }),
    tap((action: ShipperProfileAddSuccessAction) => {
        if (action && action.type === ShipperProfileActionTypes.Add_Success) {
            this.messageService.add({ id: 0, detail: `${action.payload.name} Created`, severity: 'success' });
        }
    })
  );

  @Effect()
  $updateShipper: Observable<Action> = this.actions$.pipe(
    ofType<ShipperProfileUpdateAction>(ShipperProfileActionTypes.Update),
    mapToPayload<CustomerProfile>(),
    switchMap((customer: CustomerProfile) => {
      return this.shipperProfileService.updateShipper(customer).pipe(
        map(data => new ShipperProfileUpdateSuccessAction(data)),
        catchError(err => of(new ShipperProfileUpdateFailureAction(err)))
      );
    }),
    tap((action: ShipperProfileUpdateSuccessAction) => {
        if (action && action.type === ShipperProfileActionTypes.Update_Success) {
            this.messageService.add({ id: 0, detail: `${action.payload.name} Updated`, severity: 'success' });
        }
    })
  );

  @Effect()
  $enableShipperApi: Observable<Action> = this.actions$.pipe(
    ofType<ShipperProfileEnableShipperApiAction>(ShipperProfileActionTypes.EnableShipperApi),
    mapToPayload<CustomerProfile>(),
    switchMap((customer: CustomerProfile) => {
      return this.shipperProfileService.setupCustomerApi(customer).pipe(
        map(data => new ShipperProfileEnableShipperApiSuccessAction(data)),
        catchError(err => of(new ShipperProfileEnableShipperApiFailureAction(err)))
      );
    }),
    tap((action: ShipperProfileEnableShipperApiSuccessAction) => {
        if (action && action.type === ShipperProfileActionTypes.EnableShipperApi_Success) {
            this.messageService.add({ id: 0, detail: `${action.payload.name} Updated`, severity: 'success' });
        }
    })
  );

  @Effect()
  $loadShipperMappings: Observable<Action> = this.actions$.pipe(
    ofType<ShipperProfileLoadShipperMappingsAction>(ShipperProfileActionTypes.Load_Shipper_Mappings
      , ShipperProfileActionTypes.Update_Shipper_Mapping_Success
      , ShipperProfileActionTypes.Create_Shipper_Mapping_Success),
    mapToPayload<{ ownerId: string }>(),
    switchMap((payload: { ownerId: string }) => {
      return this.shipperProfileService.getShipperMappings(payload.ownerId).pipe(
        map(data => new ShipperProfileLoadShipperMappingsSuccessAction(data)),
        catchError(err => of(new ShipperProfileLoadShipperMappingsFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadSourceSystemOwners: Observable<Action> = this.actions$.pipe(
    ofType<ShipperProfileLoadSourceSystemOwnerAction>(ShipperProfileActionTypes.Load_SourceSystem_Owners),
    mapToPayload<{ ownerId: string }>(),
    switchMap((payload: { ownerId: string }) => {
      return this.shipperProfileService.getSourceSystemOwners(payload.ownerId).pipe(
        map(data => new ShipperProfileLoadSourceSystemOwnerSuccessAction(data)),
        catchError(err => of(new ShipperProfileLoadSourceSystemOwnerFailureAction(err)))
      );
    })
  );

  @Effect()
  $updateShipperMapping: Observable<Action> = this.actions$.pipe(
    ofType<ShipperProfileUpdateShipperMappingAction>(ShipperProfileActionTypes.Update_Shipper_Mapping),
    mapToPayload<{ mapping: LoadshopShipperMapping }>(),
    switchMap((payload: { mapping: LoadshopShipperMapping }) => {
      return this.shipperProfileService.updateShipperMapping(payload.mapping).pipe(
        map(data => new ShipperProfileUpdateShipperMappingSuccessAction(data)),
        catchError(err => of(new ShipperProfileUpdateShipperMappingFailureAction(err)))
      );
    }),
    tap((action: ShipperProfileUpdateShipperMappingSuccessAction) => {
      if (action && action.type === ShipperProfileActionTypes.Update_Shipper_Mapping_Success) {
        const msg = `${action.payload.ownerId} - ${action.payload.sourceSystem} Updated`;
        this.messageService.add({ id: 0, detail: msg, severity: 'success' });
      }
    })
  );

  @Effect()
  $createShipperMapping: Observable<Action> = this.actions$.pipe(
    ofType<ShipperProfileCreateShipperMappingAction>(ShipperProfileActionTypes.Create_Shipper_Mapping),
    mapToPayload<{ mapping: LoadshopShipperMapping }>(),
    switchMap((payload: { mapping: LoadshopShipperMapping }) => {
      return this.shipperProfileService.createShipperMapping(payload.mapping).pipe(
        map(data => new ShipperProfileCreateShipperMappingSuccessAction(data)),
        catchError(err => of(new ShipperProfileCreateShipperMappingFailureAction(err)))
      );
    }),
    tap((action: ShipperProfileCreateShipperMappingSuccessAction) => {
      if (action && action.type === ShipperProfileActionTypes.Create_Shipper_Mapping_Success) {
        const msg = `${action.payload.ownerId} - ${action.payload.sourceSystem} Created`;
        this.messageService.add({ id: 0, detail: msg, severity: 'success' });
      }
    })
  );

  constructor(private actions$: Actions, private shipperProfileService: ShipperProfileService, private messageService: MessageService) {}
}
