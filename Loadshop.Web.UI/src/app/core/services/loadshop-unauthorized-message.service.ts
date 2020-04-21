import { Injectable } from '@angular/core';
import { UnauthorizedMessageService } from '@tms-ng/core';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class LoadshopUnauthorizedMessageService extends UnauthorizedMessageService {
  constructor() {
    super();
  }

  /**
   * Set the plain text you want to show between the Unauthorized header and the
   * "Please click here" footer.  If you supply custom markup in getUnauthorizedMarkup
   * the markup message will override the plain text one, but it's recommended to
   * supply null for one and a value for the other.
   */
  getUnauthorizedMessage(): Observable<string> {
    return of(null);
    // return of(
    //   'You either do not have access to Loadshop or your session has expired.',
    // );
  }

  /**
   * Set the custom markup you want to show between the Unauthorized header and the
   * "Please click here" footer.  If you supply a plain text message in getUnauthorizedMessage
   * this markup message will override the plain text one, but it's recommended to
   * supply null for one and a value for the other.
   */
  getUnauthorizedMarkup(): Observable<string> {
    // return of(null);
    return of(`You either do not have access to <strong>Loadshop</strong> or your session has expired.`);
  }
}
