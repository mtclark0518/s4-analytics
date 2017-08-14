﻿import { Injectable } from '@angular/core';
import { NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { NewUserRequest } from './new-user-request';
import { RequestActionResults } from './request-action-results';
import { QueueFilter } from './new-user-request-enum';


@Injectable()
export class NewUserRequestStateService {
    newUserRequests: NewUserRequest[];
    selectedRequest: NewUserRequest;
    currentRequestActionResults: RequestActionResults;
    contractViewerWindow?: Window;
    sortAsc: boolean = true;
    sortColumnName: string = 'requestNbr';
    queueFilter: QueueFilter = QueueFilter.Pending;
    currentActionForm: NgbModalRef;
    requestorWarningMessages: string[];
    consultantWarningMessages: string[];

    warningMessages: string[];
}