using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exporter
{
    /// <summary>
    /// 每个拥有单独材质设置的一堆面片。
    /// </summary>
    public class MeshData
    {
        private List<TriangleData> m_listTriangles = null;
        public List<TriangleData> Triangles
        {
            get
            {
                return m_listTriangles;
            }
            set
            {
                m_listTriangles = value;
            }
        }

        private List<TriangleIndexData> m_listTriangleIndexes = null;
        public List<TriangleIndexData> TriangleIndexes
        {
            get { return m_listTriangleIndexes; }
            set { m_listTriangleIndexes = value; }
        }

        private List<PointData> m_listVertex = null;
        public List<PointData> Vertexes
        {
            get { return m_listVertex; }
            set { m_listVertex = value; }
        }

        private string m_MaterialName = string.Empty;
        public string MaterialName
        {
            get { return m_MaterialName; }
            set { m_MaterialName = value; }
        }

        private bool m_bIsByBlock = false;
        public bool IsByBlock
        {
            get { return m_bIsByBlock; }
            set { m_bIsByBlock = value; }
        }
    }
}
