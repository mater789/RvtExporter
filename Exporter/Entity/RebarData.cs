using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Exporter
{
    [ProtoContract]
    public class RebarData
    {
        [ProtoMember(1)]
        public PointData DistributionVector { get; set; }

        [ProtoMember(2)]
        public int RepeatCount { get; set; } = 1;

        [ProtoMember(3)]
        public double RepeatDistance { get; set; } = -1;

        [ProtoMember(4)]
        public List<PointData> CurvePoints { get; set; }

        [ProtoMember(5)]
        public Dictionary<string, List<PropertyData>> Properties { get; set; }

        public void ConvertToMM()
        {
            RepeatDistance *= Tools.Ft2MmScale;
            CurvePoints.ForEach(c => c.ConvertToMM());
        }
    }
}
