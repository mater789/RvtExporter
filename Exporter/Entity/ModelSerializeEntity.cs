using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Exporter
{
    [ProtoContract]
    public class ModelSerializeEntity
    {
        [ProtoMember(1)]
        public Dictionary<string, BlockData> Blocks { get; set; }
        [ProtoMember(2)]
        public List<MaterialData> Materials { get; set; }
        [ProtoMember(3)]
        public List<LevelData> Levels { get; set; }
    }
}
