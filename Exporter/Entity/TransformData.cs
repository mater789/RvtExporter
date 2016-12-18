using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exporter
{
    public class TransformData
    {
        public PointData BasisX { get; set; }
        public PointData BasisY { get; set; }
        public PointData BasisZ { get; set; }
        public PointData Origin { get; set; }
    }
}
