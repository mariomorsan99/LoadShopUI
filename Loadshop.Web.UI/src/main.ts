import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

if (environment.production) {
  enableProdMode();
}

declare var window;

const script = document.createElement('script');
script.src = `https://maps.googleapis.com/maps/api/js?key=${environment.googleKey}&libraries=places`;
script.async = true;
script.defer = true;
document.body.appendChild(script);

const gaScript = document.createElement('script');
gaScript.src = `https://www.googletagmanager.com/gtag/js?id=${environment.ga_tracking_id}`;
gaScript.async = true;
gaScript.onload = function() {
  window.dataLayer = window.dataLayer || [];
  window.gtag = function(event: any, data: any) {
    window.dataLayer.push(arguments);
  };
  window.gtag('js', new Date());
  window.gtag('config', environment.ga_tracking_id);
};
document.head.appendChild(gaScript);

platformBrowserDynamic().bootstrapModule(AppModule)
  .catch(err => console.log(err));
