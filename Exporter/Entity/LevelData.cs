using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Exporter
{
    [ProtoContract]
    public class LevelData
    {
        public LevelData()
        {
            this.Name = "";
            this.Height = 0.0;
        }

        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public double Height { get; set; }
    }
}
