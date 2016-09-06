﻿import { Injectable } from '@angular/core';
import { PbcatState } from './pbcat/shared';

@Injectable()
export class AppState {
    public options: { [key: string]: any };

    constructor() {
        // todo: retrieve options from api
        this.options = {
            'silverlightBaseUrl': 'http://localhost:51063/'
        };
    }

    // read-only property for each module's state
    private _pbcatState: PbcatState;
    get pbcatState(): PbcatState {
        if (this._pbcatState === undefined) {
            this._pbcatState = new PbcatState();
        }
        return this._pbcatState;
    }
}
