﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lib.PBCAT;

namespace S4Analytics.Models
{
    public interface IPbcatRepository
    {
        string GetSessionJson(Guid token);
        PBCATPedestrianInfo FindPedestrian(int hsmvRptNbr);
        PBCATBicyclistInfo FindBicyclist(int hsmvRptNbr);
        void Add(int hsmvRptNbr, PBCATPedestrianInfo pedInfo, CrashTypePedestrian crashType);
        void Add(int hsmvRptNbr, PBCATBicyclistInfo bikeInfo, CrashTypeBicyclist crashType);
        void Update(int hsmvRptNbr, PBCATPedestrianInfo pedInfo, CrashTypePedestrian crashType);
        void Update(int hsmvRptNbr, PBCATBicyclistInfo bikeInfo, CrashTypeBicyclist crashType);
        void RemovePedestrian(int hsmvRptNbr);
        void RemoveBicyclist(int hsmvRptNbr);
        CrashTypePedestrian GetCrashType(PBCATPedestrianInfo pedInfo);
        CrashTypeBicyclist GetCrashType(PBCATBicyclistInfo bikeInfo);
        PbcatParticipantInfo GetParticipantInfo(int hsmvRptNbr);
        bool CrashReportExists(int hsmvRptNbr);
    }
}