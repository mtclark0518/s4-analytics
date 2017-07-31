﻿import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgbModule, NgbDropdown } from '@ng-bootstrap/ng-bootstrap';
import { CrashService } from './shared';
import { EventAnalysisComponent } from './event-analysis.component';
import { EventMapComponent } from './event-map.component';

@NgModule({
    imports: [
        RouterModule,
        CommonModule,
        NgbModule
    ],
    declarations: [
        EventAnalysisComponent,
        EventMapComponent
    ],
    providers: [
        CrashService,
        NgbDropdown
    ]
})
export class EventAnalysisModule { }
