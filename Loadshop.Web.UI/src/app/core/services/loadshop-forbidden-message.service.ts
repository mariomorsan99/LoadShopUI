import { Injectable } from '@angular/core';
import { ForbiddenMessageService } from '@tms-ng/core';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class LoadshopForbiddenMessageService extends ForbiddenMessageService {
  constructor() {
    super();
  }

  /**
   * Set the plain text you want to show as the forbidden message.
   * If you supply a custom markup message in getForbiddenMarkup
   * the markup message will override the plain text one, but it's recommended to
   * supply null for one and a value for the other.
   */
  getForbiddenMessage(): Observable<string> {
    // return of('You do not have access to this application.');
    return of(null);
  }

  /**
   * Set the custom markup you want to show as the forbidden message.
   * If you supply a plain text message in getForbiddenMessage
   * this markup message will override the plain text one, but it's recommended to
   * supply null for one and a value for the other.
   */
  getForbiddenMarkup(): Observable<string> {
    // return of(null);

    // return of(`You do not have access to this application. Please contact KBX Logistics
    //   at <a href="mailto:carrier@loadshop.com">carrier@loadshop.com</a> and let
    //   them know you need access. Thanks!`);

    // After building the feature to enable overriding the markup of the Forbidden
    // message, I received word they didn't want to change it in the first place.
    return super.getForbiddenMarkup();
  }
}
