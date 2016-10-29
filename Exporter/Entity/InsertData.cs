using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;

namespace Exporter
{
    public class InsertData
    {
        private Transform m_trans = Transform.Identity;
        public Transform TransMatrix
        {
            get { return m_trans; }
            set { m_trans = value; }
        }

        private BlockData m_block = null;
        public BlockData BlockRef
        {
            get { return m_block; }
            set { m_block = value; }
        }

        private BlockData m_blockBelone = null;
        public BlockData BlockBeloneTo
        {
            get { return m_blockBelone; }
            set { m_blockBelone = value; }
        }

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
