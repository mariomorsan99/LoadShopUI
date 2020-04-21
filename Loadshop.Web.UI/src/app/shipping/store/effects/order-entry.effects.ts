import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { MessageService } from 'primeng/api';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { ShippingService } from '../../services';
import {
  OrderEntryActionTypes,
  OrderEntryCreateLoadAction,
  OrderEntryCreateLoadFailureAction,
  OrderEntryCreateLoadSuccessAction,
  OrderEntryGetLoadAction,
  OrderEntryGetLoadFailureAction,
  OrderEntryGetLoadSuccessAction,
  OrderEntryUpdateLoadAction,
  OrderEntryUpdateLoadFailureAction,
  OrderEntryUpdateLoadSuccessAction,
} from '../actions';

@Injectable()
export class OrderEntryEffects {
  @Effect()
  $getLoad: Observable<Action> = this.actions$.pipe(
    ofType<OrderEntryGetLoadAction>(OrderEntryActionTypes.Get_Load),
    map(a => a.payload),
    switchMap(id => {
      return this.shippingService.getManuallyCreatedLoadById(id).pipe(
        map(data => new OrderEntryGetLoadSuccessAction(data)),
        catchError(err => of(new OrderEntryGetLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $createLoad = this.actions$.pipe(
    ofType<OrderEntryCreateLoadAction>(OrderEntryActionTypes.Create_Load),
    map(a => a.payload),
    switchMap((load: any) => {
      return this.shippingService.createLoad(load).pipe(
        map(data => new OrderEntryCreateLoadSuccessAction(data)),
        catchError(err => {
          if (err.error && err.error.errors) {
            this.messageService.add({ summary: err.error.title, detail: err.error.detail, severity: 'error' });
          }
          return of(new OrderEntryCreateLoadFailureAction(err));
        })
      );
    }),
    tap((action: OrderEntryCreateLoadSuccessAction) => {
      if (action && action.payload && action.payload.referenceLoadDisplay) {
        this.messageService.add({
          id: 0,
          detail: `Order ${action.payload.referenceLoadDisplay} was added to Post screen`,
          severity: 'success',
        });
      }
    })
  );

  @Effect()
  $updateLoad = this.actions$.pipe(
    ofType<OrderEntryUpdateLoadAction>(OrderEntryActionTypes.Update_Load),
    map(a => a.payload),
    switchMap((load: any) => {
      return this.shippingService.updateLoad(load).pipe(
        map(data => new OrderEntryUpdateLoadSuccessAction(data)),
        catchError(err => {
          if (err.error && err.error.errors) {
            this.messageService.add({ summary: err.error.title, detail: err.error.detail, severity: 'error' });
          }
          return of(new OrderEntryUpdateLoadFailureAction(err));
        })
      );
    }),
    tap((action: OrderEntryUpdateLoadSuccessAction) => {
      if (action && action.payload && action.payload.referenceLoadDisplay) {
        if (action.payload.onLoadshop) {
          this.messageService.add({
            id: 0,
            detail: `Order ${action.payload.referenceLoadDisplay} was removed from Marketplace and changes have been saved`,
            severity: 'success',
          });
        } else {
          this.messageService.add({
            id: 0,
            detail: `Order ${action.payload.referenceLoadDisplay} changes have been saved`,
            severity: 'success',
          });
        }
      }
    })
  );

  constructor(private actions$: Actions, private shippingService: ShippingService, private messageService: MessageService) {}
}
