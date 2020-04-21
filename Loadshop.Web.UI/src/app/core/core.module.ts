import { CommonModule, TitleCasePipe } from '@angular/common';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ErrorHandler, NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { EffectsModule } from '@ngrx/effects';
import { StoreRouterConnectingModule } from '@ngrx/router-store';
import { StoreModule } from '@ngrx/store';
//
import { ENVIRONMENTSETTINGS, TmsCoreModule } from '@tms-ng/core';
import { RecaptchaV3Module, RECAPTCHA_V3_SITE_KEY } from 'ng-recaptcha';
import { environment, extModules } from '../../environments/environment';
import { SharedModule } from '../shared/shared.module';
import { MobileBrowserDetectionComponent } from './components/mobile-browser-detection';
import { NoCarrierContainerComponent } from './components/no-carrier';
import { AgreementsComponent } from './components/user-agreement';
import { UserAgreementComponent } from './components/user-agreement/user-agreement';
import { CommonService, ErrorService, LoadStatusService } from './services';
import { AgreementDocumentService } from './services/agreement-document.service';
import { ResponseInterceptor } from './services/response-intercepter';
import { effects, metaReducers, reducers } from './store';
import { UsersnapComponent } from './components/usersnap';

@NgModule({
  imports: [
    CommonModule,
    RouterModule,
    HttpClientModule,
    SharedModule,
    StoreModule.forRoot(reducers, { metaReducers: metaReducers }),
    extModules, // environment specific modules
    EffectsModule.forRoot(effects),
    StoreRouterConnectingModule.forRoot({ stateKey: 'router' }),
    TmsCoreModule.forRoot(environment),
    RecaptchaV3Module,
  ],
  declarations: [
    NoCarrierContainerComponent,
    MobileBrowserDetectionComponent,
    AgreementsComponent,
    UserAgreementComponent,
    UsersnapComponent,
  ],
  exports: [NoCarrierContainerComponent, MobileBrowserDetectionComponent, UsersnapComponent],
  providers: [
    {
      provide: ErrorHandler,
      useClass: ErrorService,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ResponseInterceptor,
      multi: true,
    },
    {
      provide: ENVIRONMENTSETTINGS,
      useValue: environment,
    },
    CommonService,
    LoadStatusService,
    TitleCasePipe,
    { provide: RECAPTCHA_V3_SITE_KEY, useValue: environment.recaptchaSiteKey },
    AgreementDocumentService,
  ],
})
export class CoreModule {}
