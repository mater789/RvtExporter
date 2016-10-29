using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exporter
{
    public class SketchData
    {
        public SketchData()
        {
            Curves = new List<CurveData>();
        }

        public List<CurveData> Curves { get; set; }

        public PointData Origin { get; set; }

        public PointData Normal { get; set; }

        public PointData XVector { get; set; }

        public PointData YVector { get; set; }

        public void ConvertToMM()
        {
            Curves.ForEach(curve => curve.ConvertToMM());
            Origin.ConvertToMM();
        }
    }
}
