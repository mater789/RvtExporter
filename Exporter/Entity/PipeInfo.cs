using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exporter
{
    public class PipeInfo
    {
        public PointData PtStart
        {
            get;
            set;
        }

        public PointData PtEnd
        {
            get;
            set;
        }

        private double m_dDiameter = -1.0;
        public double Diameter
        {
            get { return m_dDiameter; }
            set { m_dDiameter = value; }
        }

        private string matName = string.Empty;
        public string MaterialName
        {
            get { return matName; }
            set { matName = value; }
        }

    }
}
