using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exporter
{
    public class PointData
    {
        public PointData(double x = 0.0, double y = 0.0, double z = 0.0)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public double X
        {
            get;
            set;
        }

        public double Y
        {
            get;
            set;
        }

        public double Z
        {
            get;
            set;
        }

        public void ConvertToMM()
        {
            this.X *= Tools.Ft2MmScale;
            this.Y *= Tools.Ft2MmScale;
            this.Z *= Tools.Ft2MmScale;
        }
    }
}
