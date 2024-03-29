﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorProxy.Service.Radius
{
    public enum RadiusCodeType : byte
    {
        Unknown = 0,
        AccessRequest = 1,
        AccessAccept = 2,
        AccessReject = 3,
        AccountingRequest = 4,
        AccountingResponse = 5,
        AccessChallenge = 11
    }

    public enum RadiusAttributeType : byte
    {
        UserName = 1,
        UserPassword = 2,
        CHAPPassword = 3,
        NASIPAddress = 4,
        NASPort = 5,
        ServiceType = 6,
        FramedProtocol = 7,
        FramedIPAddress = 8,
        FramedIPNetmask = 9,
        FramedRouting = 10,
        FilterId = 11,
        FramedMTU = 12,
        FramedCompression = 13,
        LoginIPHost = 14,
        LoginService = 15,
        LoginTCPPort = 16,
        ReplyMessage = 18,
        CallbackNumber = 19,
        CallbackId = 20,
        FramedRoute = 22,
        FramedIPXNetwork = 23,
        State = 24,
        Class = 25,
        VendorSpecific = 26,
        SessionTimeout = 27,
        IdleTimeout = 28,
        TerminationAction = 29,
        CalledStationId = 30,
        CallingStationId = 31,
        NASIdentifier = 32,
        ProxyState = 33,
        LoginLATService = 34,
        LoginLATNode = 35,
        LoginLATGroup = 36,
        FramedAppleTalkLink = 37,
        FramedAppleTalkNetwork = 38,
        FramedAppleTalkZone = 39,

        AcctStatusType = 40,
        AcctDelayTime = 41,
        AcctInputOctets = 42,
        AcctOutputOctets = 43,
        AcctSessionId = 44,
        AcctAuthentic = 45,
        AcctSessionTime = 46,
        AcctInputPackets = 47,
        AcctOutputPackets = 48,
        AcctTerminateCause = 49,
        AcctMultiSessionId = 50,
        AcctLinkCount = 51,

        AcctInputGigawords = 52,
        AcctOutputGigawords = 53,
        EventTimestamp = 55,

        CHAPChallenge = 60,
        NASPortType = 61,
        PortLimit = 62,
        LoginLATPort = 63,

        ConnectInfo = 77,
        NASPortId = 87
    }
}
