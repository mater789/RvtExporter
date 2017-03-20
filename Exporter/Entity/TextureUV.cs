using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Exporter
{
    [ProtoContract]
    public class TextureUV
    {
        public TextureUV()
        {
            this.U = 0.0;
            this.V = 0.0;
        }

        public TextureUV(double u, double v)
        {
            this.U = u;
            this.V = v;
        }

        [ProtoMember(1)]
        public double U { get; set; }
        [ProtoMember(2)]
        public double V { get; set; }
    }
}
