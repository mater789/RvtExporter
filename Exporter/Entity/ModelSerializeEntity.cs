using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exporter
{
    public class ModelSerializeEntity
    {
        public Dictionary<string, BlockData> Blocks { get; set; }
        public List<MaterialData> Materials { get; set; }
        public List<LevelData> Levels { get; set; }
    }
}
