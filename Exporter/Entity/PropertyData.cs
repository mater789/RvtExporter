using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;

namespace Exporter
{
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

        public String Name
        {
            get;
            set;
        }

        public String Value
        {
            get;
            set;
        }

        private string m_groupName = string.Empty;
        public string GroupName
        {
            get { return m_groupName; }
            set { m_groupName = value; }
        }
    }
}
