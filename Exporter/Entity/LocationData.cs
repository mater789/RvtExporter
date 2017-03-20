using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Exporter
{
    [ProtoContract]
    public class LocationData
    {
        public LocationData()
        {
            LocationCurve = new CurveData();
            FaceVec = new PointData();
            HandVec = new PointData();
        }

        [ProtoMember(1)]
        public CurveData LocationCurve { get; set; }

        [ProtoMember(2)]
        public PointData FaceVec { get; set; }

        [ProtoMember(3)]
        public PointData HandVec { get; set; }
    }
}
