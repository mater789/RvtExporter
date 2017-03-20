using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;
using ProtoBuf;
namespace Exporter
{
    [ProtoContract]
    public class PropertyData
    {
        public PropertyData()
        {
            this.Name = string.Empty;
            this.Value = string.Empty;
        }

        public PropertyData (string name, string value, string groupName = "")
        {
            this.Name = name;
            this.Value = value;
            this.GroupName = groupName;
        }

        [ProtoMember(1)]
        public String Name
        {
            get;
            set;
        }

        [ProtoMember(2)]
        public String Value
        {
            get;
            set;
        }

        private string m_groupName = string.Empty;
        [ProtoMember(3)]
        public string GroupName
        {
            get { return m_groupName; }
            set { m_groupName = value; }
        }
    }
}
