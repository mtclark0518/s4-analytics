﻿import { Component } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import {
    NewUserRequestStateService, NewUserRequestService,
    RequestActionResults, NewUserRequestStatus, QueueFilter
} from './shared';


@Component({
    selector: 'request-action-component',
    templateUrl: './request-action.component.html'
})


export class RequestActionComponent  {

    newUserRequestStatus = NewUserRequestStatus;

    constructor(public state: NewUserRequestStateService,
        private newUserRequestService: NewUserRequestService,
        public modalService: NgbModal) {
        this.state.currentRequestActionResults = new RequestActionResults(this.state.selectedRequest.requestNbr);
    }

    newUserRequestMatch(nur: number) {
        return this.state.selectedRequest.requestStatus === nur;
    }

    disableTextArea() {
        return this.state.currentRequestActionResults.approved === undefined;
    }

    submit() {
        if (this.state.currentRequestActionResults.approved) {
            this.processOKResult();
        }
        else {
            this.processRejectedResult();
        }
        this.state.currentActionForm.close();
        this.closeContractViewer();
    }

    cancel() {
        this.state.currentActionForm.close();
        this.closeContractViewer();
    }

    private closeContractViewer() {
        if (this.state.contractViewerWindow != undefined) {
            this.state.contractViewerWindow.close();
            this.state.contractViewerWindow = undefined;
        }
    }

    private processOKResult(): void {
        this.newUserRequestService.approve(this.state.selectedRequest.requestStatus, this.state.currentRequestActionResults, this.state.selectedRequest)
            .subscribe(
            result => {
                this.state.selectedRequest = result;
                let index = this.state.newUserRequests.findIndex(newUserReq => newUserReq.requestNbr === this.state.selectedRequest.requestNbr);
                this.state.newUserRequests[index] = this.state.selectedRequest;
                this.state.queueFilter = QueueFilter.Pending;
            });
    }

    private processRejectedResult(): void {
        console.log(this.state.currentRequestActionResults.rejectionReason);

        this.newUserRequestService.reject(this.state.currentRequestActionResults, this.state.selectedRequest)
            .subscribe(
            result => {
                this.state.selectedRequest = result;
                let index = this.state.newUserRequests.findIndex(newUserReq => newUserReq.requestNbr === this.state.selectedRequest.requestNbr);
                this.state.newUserRequests[index] = this.state.selectedRequest;
                this.state.queueFilter = QueueFilter.Pending;
            });
    }



}
