import { Injectable } from '@angular/core';
import { Effect } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { defer, Observable, of, timer } from 'rxjs';
import { delayWhen, map, retryWhen } from 'rxjs/operators';
import { GoogleMapService } from '../../services/google-map.service';

declare let google: any;

@Injectable()
export class GoogleMapEffects {
  @Effect({ dispatch: false })
  $startup: Observable<Action> = defer(() => of(null)).pipe(
    map(() => {
      if (google.maps) {
        this.googleMapService.initialize();
      }
      throw new Error('google.maps not loaded yet');
    }),
    retryWhen(errors => errors.pipe(delayWhen(() => timer(10))))
  );

  constructor(private googleMapService: GoogleMapService) {}
}
