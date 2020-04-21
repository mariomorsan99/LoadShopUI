import { Injectable } from '@angular/core';

import { Observable, of } from 'rxjs';
import { Effect, Actions, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { map, switchMap, catchError } from 'rxjs/operators';
import {
    Tops2GoLoadStatusLoadSuccessAction,
    Tops2GoLoadStatusLoadFailureAction,
    LoadshopLoadStatusLoadSuccessAction,
    LoadshopLoadStatusLoadFailureAction,
    LoadStatusActionTypes,
    LoadshopLoadStatusSaveSuccessAction,
    LoadshopLoadStatusSaveFailureAction,
    LoadStatusLoadAllSuccessAction,
    LoadStatusLoadAllFailureAction,
} from '../actions';
import { LoadStatusService } from '../../services';
import { mapToPayload } from '@tms-ng/shared';
import { LoadStatusStopData, LoadStatusInTransitData } from 'src/app/shared/models';
import { MessageService } from 'primeng/api';

@Injectable()
export class LoadStatusEffects {
    @Effect()
    $loadT2GStatus: Observable<Action> = this.actions$.pipe(
        ofType(LoadStatusActionTypes.Load),
        mapToPayload<{ loadId: string, referenceLoadId: string }>(),
        switchMap((payload: { loadId: string, referenceLoadId: string}) => {
            return this.loadStatusService.getLatestTopsToGoLoadStatus(payload.referenceLoadId).pipe(
                map(data => new Tops2GoLoadStatusLoadSuccessAction(data)),
                catchError(err => of(new Tops2GoLoadStatusLoadFailureAction(err))),
            );
        }),
    );

    @Effect()
    $loadAllT2GStatuses: Observable<Action> = this.actions$.pipe(
        ofType(LoadStatusActionTypes.LoadAll),
        mapToPayload<{ referenceLoadId: string }>(),
        switchMap((payload: { referenceLoadId: string }) => {
            return this.loadStatusService.getAllTopsToGoLoadStatuses(payload.referenceLoadId).pipe(
                map(data => new LoadStatusLoadAllSuccessAction(data)),
                catchError(err => of(new LoadStatusLoadAllFailureAction(err))),
            );
        }),
    );

    @Effect()
    $loadLoadshopStatus: Observable<Action> = this.actions$.pipe(
        ofType(LoadStatusActionTypes.Load),
        mapToPayload<{ loadId: string, referenceLoadId: string }>(),
        switchMap((payload: { loadId: string, referenceLoadId: string }) => {
            return this.loadStatusService.getLatestLoadshopLoadStatus(payload.loadId).pipe(
                map(data => new LoadshopLoadStatusLoadSuccessAction(data)),
                catchError(err => of(new LoadshopLoadStatusLoadFailureAction(err))),
            );
        }),
    );

    @Effect()
    $saveInTransit: Observable<Action> = this.actions$.pipe(
        ofType(LoadStatusActionTypes.LoadshopInTransitStatusSave),
        mapToPayload<LoadStatusInTransitData>(),
        switchMap((payload: LoadStatusInTransitData) => {
            return this.loadStatusService.saveInTransit(payload).pipe(
                map(data => new LoadshopLoadStatusSaveSuccessAction(data)),
                catchError(err => {
                    if (err.error && err.error.errors) {
                        this.messageService.add({ summary: err.error.title, detail: err.error.detail, severity: 'error' });
                    }
                    return of(new LoadshopLoadStatusSaveFailureAction(err));
                }),
            );
        }),
    );

    @Effect()
    $saveStopStatus: Observable<Action> = this.actions$.pipe(
        ofType(LoadStatusActionTypes.LoadshopStopStatusSave),
        mapToPayload<LoadStatusStopData>(),
        switchMap((payload: LoadStatusStopData) => {
            return this.loadStatusService.saveStopData(payload).pipe(
                map(data => new LoadshopLoadStatusSaveSuccessAction(data)),
                catchError(err => {
                    if (err.error && err.error.errors) {
                        this.messageService.add({ summary: err.error.title, detail: err.error.detail, severity: 'error' });
                    }
                    return of(new LoadshopLoadStatusSaveFailureAction(err));
                }),
            );
        }),
    );

    constructor(
        private loadStatusService: LoadStatusService,
        private actions$: Actions,
        private messageService: MessageService
    ) {}
}
