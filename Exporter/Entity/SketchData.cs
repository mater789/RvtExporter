using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Exporter
{
    [ProtoContract]
    public class SketchData
    {
        public SketchData()
        {
            Curves = new List<CurveData>();
        }

        [ProtoMember(1)]
        public List<CurveData> Curves { get; set; }

        [ProtoMember(2)]
        public PointData Origin { get; set; }

        [ProtoMember(3)]
        public PointData Normal { get; set; }

        [ProtoMember(4)]
        public PointData XVector { get; set; }

        [ProtoMember(5)]
        public PointData YVector { get; set; }

        public void ConvertToMM()
        {
            Curves.ForEach(curve => curve.ConvertToMM());
            Origin.ConvertToMM();
        }
    }
}
