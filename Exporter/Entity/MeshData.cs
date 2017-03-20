using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Exporter
{
    /// <summary>
    /// 每个拥有单独材质设置的一堆面片。
    /// </summary>
    [ProtoContract]
    public class MeshData
    {
        [ProtoMember(1)]
        public List<TriangleIndexData> TriangleIndexes { get; set; }
        [ProtoMember(2)]
        public List<PointData> Vertexes { get; set; }
        [ProtoMember(3)]
        public List<PointData> Normals { get; set; }
        [ProtoMember(4)]
        public List<TextureUV> TextureUVs { get; set; }

        private string m_MaterialName = string.Empty;
        [ProtoMember(5)]
        public string MaterialName
        {
            get { return m_MaterialName; }
            set { m_MaterialName = value; }
        }

        private bool m_bIsByBlock = false;
        [ProtoMember(6)]
        public bool IsByBlock
        {
            get { return m_bIsByBlock; }
            set { m_bIsByBlock = value; }
        }
    }
}
