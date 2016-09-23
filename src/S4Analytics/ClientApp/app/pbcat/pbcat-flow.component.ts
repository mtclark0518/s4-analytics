﻿import { Component } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs/Subscription';
import { AppState } from '../app.state';
import { AlertType } from '../alert-type';
import { KeepSilverlightAliveService } from '../keep-silverlight-alive.service.ts';
import { OptionsService, Options } from '../options.service';
import {
    PbcatService, PbcatItem, PbcatCrashType,
    PbcatFlow, FlowType, PbcatState
} from './shared';

@Component({
    selector: 'pbcat-flow',
    template: require('./pbcat-flow.component.html')
})
export class PbcatFlowComponent {
    private state: PbcatState;
    private routeSub: Subscription;

    private AlertType = AlertType; // capture the enum for the template to use
    private alertHeading: string;
    private alertMessage: string;
    private alertType: AlertType;
    private alertVisible: boolean = false;
    private options: Options;

    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private appState: AppState,
        private pbcatService: PbcatService,
        private keepSilverlightAlive: KeepSilverlightAliveService,
        private optionsService: OptionsService) {
        this.state = appState.pbcatState;
    }

    ngOnInit() {
        let routeData: { [name: string]: any };
        this.routeSub = this.route.data
            .do(data => routeData = data)
            .switchMap(() => this.optionsService.getOptions())
            .do(options => this.options = options)
            .subscribe(
                options => this.handleRouteData(routeData),
                err => this.displayAlert('Error', err, AlertType.Danger)
            );
    }

    ngOnDestroy() {
        this.routeSub.unsubscribe();
    }

    private displayAlert(heading: string, message: string, type: AlertType) {
        this.alertHeading = heading;
        this.alertMessage = message;
        this.alertType = type;
        this.alertVisible = true;
    }

    private dismissAlert() {
        this.alertVisible = false;
    }

    private handleRouteData(data: { [name: string]: any }) {
        this.state.flow = data['flow'] as PbcatFlow;
        this.maintainReportViewer();
        if (this.state.flow.showSummary) {
            this.pbcatService
                .calculateCrashType(this.state.flow.flowType, this.state.flow.pbcatInfo)
                .first()
                .subscribe(
                    crashType => this.state.flow.crashType = crashType,
                    err => this.displayAlert('Error', err, AlertType.Danger)
                );
        }
        this.keepSilverlightAlive.keepAlive();
    }

    private get flow(): PbcatFlow { return this.state.flow; }

    private get pageTitle(): string {
        return this.flow.flowType === FlowType.Pedestrian
            ? 'Pedestrian Crash Typing'
            : 'Bicyclist Crash Typing';
    }

    private maintainReportViewer() {
        let userClosedWindow = this.state.reportViewerWindow && this.state.reportViewerWindow.closed;
        if (userClosedWindow) {
            // assume they don't want the crash report window anymore
            this.state.showReportViewer = false;
            delete this.state.reportViewerWindow;
        }
        if (this.state.showReportViewer) {
            let href = `${this.options.baseUrl}report-viewer/${this.hsmvReportNumber}`;
            let isWindowOpen = this.state.reportViewerWindow !== undefined;
            if (isWindowOpen) {
                let isUrlCurrent = this.state.reportViewerWindow.location.href.endsWith(href);
                if (!isUrlCurrent) {
                    this.state.reportViewerWindow.location.href = href; // update the window url
                }
            }
            else {
                this.state.reportViewerWindow = window.open(href, 'crashReportWindow'); // open a new window
            }
        }
    }

    private closeReportViewer() {
        if (this.state.reportViewerWindow && this.state.reportViewerWindow.close) {
            this.state.reportViewerWindow.close();
        }
        this.state.reportViewerWindow = undefined;
    }

    private get isSaved(): boolean { return this.flow.isSaved; }

    private get hsmvReportNumber() { return this.flow.hsmvReportNumber; }

    private get showSummary() { return this.flow.showSummary; }

    private get currentStepNumber() { return this.flow.currentStepNumber; }

    private get currentStep() { return this.flow.currentStep; }

    private get stepHistory() { return this.flow.stepHistory; }

    private get crashType() { return this.flow.crashType; }

    private get canGoBack(): boolean { return this.flow.canGoBack; }

    private get canProceed(): boolean { return this.flow.canProceed; }

    private get canReturnToSummary(): boolean { return this.flow.canReturnToSummary; }

    private get ready(): boolean {
        return this.flow && this.flow.hasValidState;
    }

    private get nextCrashExists(): boolean { return this.flow.isSaved && this.state.nextHsmvNumber !== undefined; }

    private get autoAdvance() { return this.state.autoAdvance; }

    private set autoAdvance(value: boolean) { this.state.autoAdvance = value; }

    private get showReportViewer() { return this.state.showReportViewer; }

    private set showReportViewer(value: boolean) {
        if (this.state.showReportViewer !== value) {
            this.state.showReportViewer = value;
            if (value) {
                this.maintainReportViewer();
            }
            else {
                this.closeReportViewer();
            }
        }
    }

    private selectItem(pbcatItem: PbcatItem): void {
        this.flow.selectItemForCurrentStep(pbcatItem);
        if (this.autoAdvance) {
            // a 300ms delay to give visual confirmation of selected item
            setTimeout(() => this.proceed(), 300);
        }
    }

    private get backLinkText(): string {
        return `${this.flow.previousStepNumber}. ${this.flow.previousStep.title}`;
    }

    private get proceedLinkText(): string {
        return this.flow.isFinalStep
            ? 'Summary'
            : `${this.flow.nextStepNumber}. ${this.flow.nextStep.title}`;
    }

    private get backLinkRoute(): any[] {
        return ['/pbcat', this.flow.hsmvReportNumber, 'step', this.flow.previousStepNumber];
    }

    private get proceedLinkRoute(): any[] {
        return this.flow.isFinalStep
            ? ['/pbcat', this.hsmvReportNumber, 'summary']
            : ['/pbcat', this.flow.hsmvReportNumber, 'step', this.flow.nextStepNumber];
    }

    private get summaryRoute(): any[] {
        return ['/pbcat', this.hsmvReportNumber, 'summary'];
    }

    private proceed(): void {
        if (this.proceedLinkRoute) {
            this.router.navigate(this.proceedLinkRoute);
        }
    }

    private jumpBackToStep(stepNumber: number) {
        let route = ['/pbcat', this.flow.hsmvReportNumber, 'step', stepNumber];
        this.router.navigate(route);
    }

    private handleSaved() {
        this.flow.isSaved = true;
        if (this.state.queue) {
            let i = this.state.queue.indexOf(this.hsmvReportNumber);
            if (i > -1) {
                this.state.queue.splice(i, 1);
            }
        }
        this.state.nextHsmvNumber = this.state.queue && this.state.queue.length > 0
            ? this.state.queue[0]
            : undefined;
    }

    private get advanceToNextRoute(): any[] {
        return ['/pbcat', this.state.nextHsmvNumber, 'step', 1];
    }

    private acceptAndSave(): void {
        if (this.flow.typingExists) {
            this.pbcatService.updatePbcatInfo(this.flow)
                .first()
                .subscribe(
                    nextCrash => this.handleSaved(),
                    err => this.displayAlert('Error', err, AlertType.Danger)
                );
        }
        else {
            this.pbcatService.createPbcatInfo(this.flow)
                .first()
                .subscribe(
                    nextCrash => this.handleSaved(),
                    err => this.displayAlert('Error', err, AlertType.Danger)
                );
        }
    }
}
