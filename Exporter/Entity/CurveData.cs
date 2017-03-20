using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Exporter
{
    public class CurveData
    {
        public CurveData()
        {
            Points = new List<PointData>();
            IsArc = false;
            Center = new PointData();
            Normal = new PointData();
            Radius = 0;
            StartParameter = 0;
            EndParameter = 0;
        }

        [ProtoMember(1)]
        public List<PointData> Points { get; set; }

        [ProtoMember(2)]
        public bool IsArc { get; set; }

        [ProtoMember(3)]
        public PointData Center { get; set; }

        [ProtoMember(4)]
        public PointData Normal { get; set; }

        [ProtoMember(5)]
        public double Radius { get; set; }

        [ProtoMember(6)]
        public double StartParameter { get; set; }

        [ProtoMember(7)]
        public double EndParameter { get; set; }

        public void ConvertToMM()
        {
            Points.ForEach(pt => pt.ConvertToMM());
            Center.ConvertToMM();
            Radius *= Tools.Ft2MmScale;
        }
    }
}
