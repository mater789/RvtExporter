using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exporter
{
    public class CurveData
    {
        public CurveData()
        {
            Points = new List<PointData>();
            IsArc = false;
            Center = new PointData();
            Normal = new PointData();
            Radius = 0;
            StartParameter = 0;
            EndParameter = 0;
        }

        public List<PointData> Points { get; set; }

        public bool IsArc { get; set; }

        public PointData Center { get; set; }

        public PointData Normal { get; set; }

        public double Radius { get; set; }

        public double StartParameter { get; set; }

        public double EndParameter { get; set; }

        public void ConvertToMM()
        {
            Points.ForEach(pt => pt.ConvertToMM());
            Center.ConvertToMM();
            Radius *= Tools.Ft2MmScale;
        }
    }
}
