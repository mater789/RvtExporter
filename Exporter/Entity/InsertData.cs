using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;
using Newtonsoft.Json;
using ProtoBuf;

namespace Exporter
{
    [ProtoContract]
    public class InsertData
    {
        [ProtoMember(1)]
        public TransformData TransMatrix
        {
            get;set;
        }

        [ProtoMember(2)]
        public string BlockName { get; set; }

        private Dictionary<string, List<PropertyData>> m_dictProperties = new Dictionary<string, List<PropertyData>>();
        [ProtoMember(3)]
        public Dictionary<string, List<PropertyData>> DictProperties
        {
            get { return m_dictProperties; }
            set { m_dictProperties = value; }
        }

        private List<PointData> m_Location = null;
        [ProtoMember(4)]
        public List<PointData> LocationInfo
        {
            get { return m_Location; }
            set { m_Location = value; }
        }

        [ProtoMember(5)]
        public LightData Light { get; set; }
    }
}
