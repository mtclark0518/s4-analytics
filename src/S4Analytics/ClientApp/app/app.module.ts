﻿import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { routes } from './app.routing';
import { AdminModule } from './admin';
import { EventAnalysisModule } from './event-analysis';
import { NetworkAnalysisModule } from './network-analysis';
import { ReportingModule } from './reporting';
import { TrendAnalysisModule } from './trend-analysis';
import { AppComponent } from './app.component';
import { LoginComponent } from './login.component';
import { Html5ConduitComponent } from './html5-conduit.component';
import {
    IdentityService, KeepSilverlightAliveService, OptionsService,
    AuthGuard, AnyAdminGuard, GlobalAdminGuard, Html5ConduitResolve
} from './shared';

@NgModule({
    imports: [
        RouterModule.forRoot(routes),
        HttpModule,
        BrowserModule,
        FormsModule,
        NgbModule.forRoot(),
        AdminModule,
        EventAnalysisModule,
        NetworkAnalysisModule,
        ReportingModule,
        TrendAnalysisModule
    ],
    declarations: [
        AppComponent,
        LoginComponent,
        Html5ConduitComponent
    ],
    providers: [
        OptionsService,
        IdentityService,
        AuthGuard,
        AnyAdminGuard,
        GlobalAdminGuard,
        KeepSilverlightAliveService,
        Html5ConduitResolve
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
