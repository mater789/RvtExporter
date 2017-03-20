using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Exporter
{
    [ProtoContract]
    public class TransformData
    {
        [ProtoMember(1)]
        public PointData BasisX { get; set; }
        [ProtoMember(2)]
        public PointData BasisY { get; set; }
        [ProtoMember(3)]
        public PointData BasisZ { get; set; }
        [ProtoMember(4)]
        public PointData Origin { get; set; }
    }
}
