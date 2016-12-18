using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;
using Newtonsoft.Json;

namespace Exporter
{
    public class InsertData
    {
        public TransformData TransMatrix
        {
            get;set;
        }

        public string BlockName { get; set; }

        private Dictionary<string, List<PropertyData>> m_dictProperties = new Dictionary<string, List<PropertyData>>();
        public Dictionary<string, List<PropertyData>> DictProperties
        {
            get { return m_dictProperties; }
            set { m_dictProperties = value; }
        }

        private List<PointData> m_Location = null;

        public List<PointData> LocationInfo
        {
            get { return m_Location; }
            set { m_Location = value; }
        }
    }
}
