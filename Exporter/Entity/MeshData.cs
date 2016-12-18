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
        public List<TriangleIndexData> TriangleIndexes { get; set; }
        public List<PointData> Vertexes { get; set; }
        public List<PointData> Normals { get; set; }
        public List<TextureUV> TextureUVs { get; set; }

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
