using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Exporter
{
    [ProtoContract]
    public class PipeInfo
    {
        [ProtoMember(1)]
        public PointData PtStart
        {
            get;
            set;
        }

        [ProtoMember(2)]
        public PointData PtEnd
        {
            get;
            set;
        }

        private double m_dDiameter = -1.0;
        [ProtoMember(3)]
        public double Diameter
        {
            get { return m_dDiameter; }
            set { m_dDiameter = value; }
        }

        private string matName = string.Empty;
        [ProtoMember(4)]
        public string MaterialName
        {
            get { return matName; }
            set { matName = value; }
        }

    }
}
