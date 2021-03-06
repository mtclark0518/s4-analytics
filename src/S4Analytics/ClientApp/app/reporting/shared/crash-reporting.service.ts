﻿import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { ReportOverTime } from './report-over-time';
import { CrashesOverTimeQuery } from './crashes-over-time-query';

@Injectable()
export class CrashReportingService {
    constructor(private http: Http) { }

    getMaxEventYear(): Observable<number> {
        return this.http
            .get('api/reporting/crash/max-event-year')
            .map(response => response.json() as number);
    }

    getMaxLoadYear(): Observable<number> {
        return this.http
            .get('api/reporting/crash/max-load-year')
            .map(response => response.json() as number);
    }

    getCrashesOverTimeByYear(query: CrashesOverTimeQuery): Observable<ReportOverTime> {
        return this.http
            .post('api/reporting/crash/year', query)
            .map(response => response.json() as ReportOverTime)
            .map(report => new ReportOverTime(report));
    }

    getCrashesOverTimeByMonth(year: number, query: CrashesOverTimeQuery): Observable<ReportOverTime> {
        return this.http
            .post(`api/reporting/crash/${year}/month`, query)
            .map(response => response.json() as ReportOverTime)
            .map(report => new ReportOverTime(report));
    }

    getCrashesOverTimeByDay(year: number, alignByWeek: boolean, query: CrashesOverTimeQuery): Observable<ReportOverTime> {
        return this.http
            .post(`api/reporting/crash/${year}/day?alignByWeek=${alignByWeek}`, query)
            .map(response => response.json() as ReportOverTime)
            .map(report => new ReportOverTime(report));
    }

    getDataTimeliness(year: number, query: CrashesOverTimeQuery): Observable<ReportOverTime> {
        return this.http
            .post(`api/reporting/crash/${year}/timeliness`, query)
            .map(response => response.json() as ReportOverTime)
            .map(report => new ReportOverTime(report));
    }

    getCrashesOverTimeByAttribute(year: number, attrName: string, query: CrashesOverTimeQuery): Observable<ReportOverTime> {
        return this.http
            .post(`api/reporting/crash/${year}/${attrName}`, query)
            .map(response => response.json() as ReportOverTime)
            .map(report => new ReportOverTime(report));
    }
}
