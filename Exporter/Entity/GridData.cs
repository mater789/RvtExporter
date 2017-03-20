using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Exporter
{
    /// <summary>
    /// 轴网
    /// </summary>
    [ProtoContract]
    public class GridData
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public List<CurveData> Curves { get; set; }

        [ProtoMember(3)]
        public Dictionary<string, List<PropertyData>> Properties { get; set; }
    }
}
