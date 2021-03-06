﻿export class CrashResult {
    id: number;
    crashDate: Date;
    crashTime: Date;
    hsmvReportNumber: string;
    agencyReportNumber: string;
    mapPointX?: number;
    mapPointY?: number;
    symbolAngle?: number;
    crashSegId?: number;
    nearestIntrsectId?: number;
    nearestIntrsectOffsetFt?: number;
    refIntrsectId?: number;
    refIntrsectOffsetFt?: number;
    nearIntrsectOffsetDir?: number;
    refIntrsectOffsetDir?: number;
    imgFileNm: string;
    formType: string;
    keyCrashSev?: number;
    keyCrashSevDtl?: number;
    keyCrashType?: number;
    crashSeverity: string;
    crashSeverityDetailed: string;
    crashType: string;
    crashTypeDetail: string;
    lightCond: string;
    weatherCond: string;
    county: string;
    city: string;
    streetName: string;
    intersectingStreet: string;
    isAlcoholRelated: string;
    isDistracted: string;
    isDrugRelated: string;
    lat?: number;
    lng?: number;
    offsetDir: string;
    offsetFt?: number;
    vehicleCount: number;
    nonmotoristCount: number;
    fatalityCount: number;
    injuryCount: number;
    totDmgAmt: number;
    agncyNm: string;
    agncyId: number;
    cntyCd: number;
    cityCd: number;
    crashTypeDir: string;
    crashRoadSurfCond: string;
    firstHarmfulEvent: string;
    bikeOrPed: string;
    bikePedCrashTypeName: string;
    bikeCount: number;
    pedCount: number;
    injuryNoneCount: number;
    injuryPossibleCount: number;
    injuryNonIncapacitatingCount: number;
    injuryIncapacitatingCount: number;
    injuryFatal30Count: number;
    injuryFatalNonTrafficCount: number;

    constructor(result: CrashResult) {
        // merge data from the api
        Object.assign(this, result);
        // rest api represents dates as strings at runtime; convert
        this.crashDate = new Date(this.crashDate);
        this.crashTime = new Date(this.crashTime);
    }
}
