using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exporter
{
    public class TriangleData
    {
        public TriangleData(PointData pt1, PointData pt2, PointData pt3)
        {
            this.Pt1 = pt1;
            this.Pt2 = pt2;
            this.Pt3 = pt3;
        }

        public PointData Pt1
        {
            get;
            set;
        }

        public PointData Pt2
        {
            get;
            set;
        }

        public PointData Pt3
        {
            get;
            set;            
        }
    }
}

