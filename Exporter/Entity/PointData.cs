using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Exporter
{
    [ProtoContract]
    public class PointData
    {
        public PointData()
        {
            
        }

        public PointData(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        [ProtoMember(1)]
        public double X
        {
            get;
            set;
        }
        [ProtoMember(2)]
        public double Y
        {
            get;
            set;
        }
        [ProtoMember(3)]
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
