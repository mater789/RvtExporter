using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exporter
{
    public class LocationData
    {
        public LocationData()
        {
            LocationCurve = new CurveData();
            FaceVec = new PointData();
            HandVec = new PointData();
        }

        public CurveData LocationCurve { get; set; }

        public PointData FaceVec { get; set; }

        public PointData HandVec { get; set; }
    }
}
