using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exporter
{
    public class DecalData
    {
        public DecalData()
        {
            Points = new List<PointData>();
            MapFileName = string.Empty;
        }

        public List<PointData> Points { get; set; }
        public string MapFileName { get; set; }
    }
}
