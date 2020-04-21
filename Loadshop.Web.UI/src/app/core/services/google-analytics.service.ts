import { Injectable } from '@angular/core';
import { User } from '@tms-ng/core';

declare var window;

@Injectable({ providedIn: 'root'})
export class AnalyticsService {
  private loaded = false;

  constructor() { }

    initialize() {
      this.loaded = true;
    }

    addEvent(name: string, data: any) {
      if (this.loaded) {
        window.gtag('event', name, data);
      } else {
        setTimeout(() => {
          this.addEvent(name, data);
        }, 100);
      }
    }

    addLogin(user: User) {
      if (this.loaded) {
        window.gtag('set', {'user_id': user.sub});
      }
    }
}

