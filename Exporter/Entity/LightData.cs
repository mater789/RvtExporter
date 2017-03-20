using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Exporter
{
    [ProtoContract]
    public class LightData
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public double ColorTemperature { get; set; }

        [ProtoMember(3)]
        public double Lumen { get; set; }

        [ProtoMember(4)]
        public ColorData Color { get; set; }

        [ProtoMember(5)]
        public TransformData TransData { get; set; }

    }
}
