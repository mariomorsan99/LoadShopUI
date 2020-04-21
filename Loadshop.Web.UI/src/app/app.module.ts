// angular libs
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';

// app  modules
import { CoreModule } from './core/core.module';
import { SharedModule } from './shared/shared.module';
import { TmsCoreModule, ForbiddenMessageService, UnauthorizedMessageService } from '@tms-ng/core';
import { UserModule } from './user/user.module';
import { LoadBoardModule } from './load-board/load-board.module';
import { AdminModule } from './admin/admin.module';
import { ShippingModule } from './shipping/shipping.module';

// main component
import { AppComponent } from './app.component';

// routing
import { AppRoutingModule } from './app.routing.module';
import { LoadshopForbiddenMessageService, LoadshopUnauthorizedMessageService } from './core/services';

// services and guards

@NgModule({
    declarations: [
        AppComponent
    ],
    imports: [
        // Core modules
        BrowserModule,
        BrowserAnimationsModule,

        // Reactive Forms
        ReactiveFormsModule,

        // App Modules
        CoreModule,
        SharedModule,
        UserModule,
        LoadBoardModule,
        AdminModule,
        ShippingModule,

        // routing modules
        AppRoutingModule,
        TmsCoreModule
    ],
    providers: [
        { provide: ForbiddenMessageService, useClass: LoadshopForbiddenMessageService },
        { provide: UnauthorizedMessageService, useClass: LoadshopUnauthorizedMessageService },
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
