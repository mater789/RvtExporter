using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Exporter
{
    [ProtoContract]
    public class TriangleData
    {
        public TriangleData()
        {
            this.Pt1 = new PointData();
            this.Pt2 = new PointData();
            this.Pt3 = new PointData();
        }

        public TriangleData(PointData pt1, PointData pt2, PointData pt3)
        {
            this.Pt1 = pt1;
            this.Pt2 = pt2;
            this.Pt3 = pt3;
        }

        [ProtoMember(1)]
        public PointData Pt1
        {
            get;
            set;
        }

        [ProtoMember(2)]
        public PointData Pt2
        {
            get;
            set;
        }

        [ProtoMember(3)]
        public PointData Pt3
        {
            get;
            set;            
        }
    }
}

