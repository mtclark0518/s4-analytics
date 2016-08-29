﻿import { PbcatInfo, PbcatPedestrianInfo, PbcatBicyclistInfo } from './pbcat-info';
import { PbcatStep } from './pbcat-step';
import { PbcatItem } from './pbcat-item';
import { PbcatCrashType } from './pbcat-crash-type';
import { PbcatConfig, PbcatScreenConfig, PbcatItemConfig } from './pbcat-config.d.ts';

export enum FlowType {
    Bicyclist,
    Pedestrian
}

export class PbcatState {
    autoAdvance: boolean = true;
    crashType: PbcatCrashType;
    private config: PbcatConfig;
    private currentStepIndex: number = -1;

    // property backing fields
    private _stepHistory: PbcatStep[] = [];
    private _isFlowComplete: boolean = false;
    private _hasValidState: boolean = true;
    private _flowType: FlowType;
    private _hsmvReportNumber: number;

    resetFlow(
        config: PbcatConfig,
        flowType: FlowType,
        hsmvReportNumber: number,
        autoAdvance: boolean) {
        this.config = config;
        this._flowType = flowType;
        this._hsmvReportNumber = hsmvReportNumber;
        this.autoAdvance = autoAdvance;
        this.currentStepIndex = -1;
    }

    get stepHistory() { return this._stepHistory; }

    get isFlowComplete() { return this._isFlowComplete; }

    get hasValidState() { return this._hasValidState; }

    get flowType() { return this._flowType; }

    get hsmvReportNumber() { return this._hsmvReportNumber; }

    get isFinalStep(): boolean {
        return this._isFlowComplete && this.currentStepIndex === this._stepHistory.length - 1;
    }

    get showSummary(): boolean {
        return this.currentStepIndex === this._stepHistory.length;
    }

    get currentStep(): PbcatStep {
        return this._stepHistory[this.currentStepIndex];
    }

    get previousStep(): PbcatStep {
        return this._stepHistory[this.currentStepIndex - 1];
    }

    get nextStep(): PbcatStep {
        return this._stepHistory[this.currentStepIndex + 1];
    }

    get currentStepNumber(): number {
        return this.currentStepIndex + 1;
    }

    get previousStepNumber(): number {
        return this.previousStep ? this.currentStepNumber - 1 : undefined;
    }

    get nextStepNumber(): number {
        return this.nextStep ? this.currentStepNumber + 1 : undefined;
    }

    get canProceed(): boolean {
        return !this.showSummary && this.currentStep && this.currentStep.selectedItem !== undefined;
    }

    get canGoBack(): boolean { return this.previousStep !== undefined; }

    get canReturnToSummary(): boolean {
        return this._isFlowComplete && !this.showSummary && !this.isFinalStep;
    }

    get pbcatInfo(): PbcatInfo {
        // mock logic to create pbcatInfo ...
        let info = this._flowType === FlowType.Pedestrian
            ? new PbcatPedestrianInfo()
            : new PbcatBicyclistInfo();
        for (let step of this._stepHistory) {
            if (step.selectedItem !== undefined) {
                (info as any)[step.infoAttrName] = step.selectedItem.infoAttrValue;
            }
        }
        return info;
    }

    goToStep(stepNumber: number) {
        let stepIndex = stepNumber - 1; // stepNumber is 1-based
        let isNewFlow = stepIndex === 0 && this._stepHistory.length === 0;
        let stepExists = stepIndex >= 0 && stepIndex < this._stepHistory.length;
        this.currentStepIndex = stepIndex;

        if (isNewFlow) {
            let currentStep = this.getFirstStep();
            this._stepHistory.push(currentStep);
        }
        else if (!stepExists) {
            this._hasValidState = false;
        }
    }

    goToSummary() {
        if (this._isFlowComplete) {
            this.currentStepIndex = this._stepHistory.length; // summary isn't actually in the stepHistory
        }
        else {
            this._hasValidState = false;
        }
    }

    selectItemForCurrentStep(item: PbcatItem) {
        let isCurrentSelectedItem = this.currentStep.selectedItem === item;

        if (!isCurrentSelectedItem) {
            // select the item
            this.currentStep.selectedItem = item;
            // clear the step history after this step
            this._stepHistory = this._stepHistory.slice(0, this.currentStepIndex + 1);
            // queue up the next step
            let nextStep = this.calculateNextStep();
            if (nextStep) {
                this._isFlowComplete = false;
                this._stepHistory.push(nextStep);
            }
            else {
                this._isFlowComplete = true;
            }
        }
    }

    private getFirstStep(): PbcatStep {
        let screenName = '1';
        let screenConfig = this.config[screenName];
        let step = new PbcatStep(screenConfig.title, screenConfig.description, screenConfig.infoAttrName);
        step.items = screenConfig.items.map((item, index) =>
            new PbcatItem(
                index, item.infoAttrValue, item.title,
                item.nextScreenName, item.description,
                item.imageUrl, false)
        );
        return step;
    }

    private calculateNextStep(): PbcatStep {
        let screenName: string;
        let step: PbcatStep;
        let screenConfig: PbcatScreenConfig;

        if (this.currentStep) {
            screenName = this.currentStep.selectedItem.nextScreenName;
        }
        if (screenName !== 'END') {
            screenConfig = this.config[screenName];
        }
        if (screenConfig) {
            step = new PbcatStep(screenConfig.title, screenConfig.description, screenConfig.infoAttrName);
            step.items = screenConfig.items.map((item, index) =>
                new PbcatItem(
                    index, item.infoAttrValue, item.title,
                    item.nextScreenName, item.description,
                    item.imageUrl, false)
            );
        }

        return step;
    }
}