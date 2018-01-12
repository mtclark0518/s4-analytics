﻿export class CrashQuery {
    // mvp
    dateRange: {
        startDate: Date,
        endDate: Date
    };
    dayOfWeek?: number[];
    timeRange?: {
        startTime: Date,
        endTime: Date
    };
    county?: number[];
    city?: number[];
    reportingAgency?: number[];
    formType?: string[];
    codeableOnly?: boolean;
    hsmvReportNumbers?: number[];
    cmvInvolved?: boolean;
    bikePedInvolved?: {
        pedestrian?: boolean,
        bicyclist?: boolean
    };
    crashSeverity?: number[];
    crashTypeSimple?: string[];
    crashTypeDetailed?: number[];
    roadSystemIdentifier?: number[];
    intersectionRelated?: boolean;
    behavioralFactors?: {
        alcohol?: boolean,
        drugs?: boolean,
        distraction?: boolean,
        aggressiveDriving?: boolean;
    };
    laneDepartures?: {
        offRoadAll?: boolean,
        offRoadRollover?: boolean,
        offRoadCollisionWithFixedObject?: boolean,
        crossedIntoOncomingTraffic?: boolean,
        sideswipe?: boolean;
    };
    weatherCondition?: number[];

    // post-mvp
    dotDistrict?: number[];
    mpoTpo?: number[];
    customArea?: {
        x: number,
        y: number
    }[];
    customExtent?: {
        minX: number,
        minY: number,
        maxX: number,
        maxY: number
    };
    intersection?: {
        intersectionId: number,
        offsetInFeet: number,
        offsetDirection?: string[];
    };
    street?: {
        linkIds: number[],
        includeCrossStreets: boolean;
    };
    customNetwork?: number[];
    publicRoadOnly?: boolean;
    driverGender?: number[];
    driverAgeRange?: string[];
    pedestrianAgeRange?: string[];
    cyclistAgeRange?: string[];
    nonAutoModesOfTravel?: {
        pedestrian?: boolean,
        bicyclist?: boolean,
        moped?: boolean,
        motorcycle?: boolean;
    };
    sourcesOfTransport?: {
        ems?: boolean,
        lawEnforcement?: boolean,
        other?: boolean;
    };
    commonViolations?: {
        speed?: boolean,
        redLight?: boolean,
        rightOfWay?: boolean,
        trafficControlDevice?: boolean,
        carelessDriving?: boolean,
        dui?: boolean;
    };
    vehicleType?: number[];
    bikePedCrashType?: {
        bikePedCrashTypeIds: number[],
        includeUntyped?: boolean;
    };
    cmvConfiguration?: number[];
    environmentalCircumstance?: number[];
    roadCircumstance?: number[];
    firstHarmfulEvent?: number[];
    lightCondition?: number[];
    otherCircumstances?: {
        hitAndRun?: boolean,
        schoolBusRelated?: boolean,
        withinCityLimits?: boolean,
        withinInterchange?: boolean,
        workZoneRelated?: boolean,
        workersInWorkZone?: boolean,
        lawEnforcementInWorkZone?: boolean;
    };
}
