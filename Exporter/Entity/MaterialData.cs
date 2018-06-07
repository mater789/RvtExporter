using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Exporter
{
    [ProtoContract]
    public class MaterialData
    {
        private string m_name = string.Empty;

        [ProtoMember(1)]
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        private int m_transparency = 0;
        [ProtoMember(2)]
        public int Transparency
        {
            get { return m_transparency; }
            set { m_transparency = value; }
        }

        private ColorData m_color = new ColorData();
        [ProtoMember(3)]
        public ColorData Color
        {
            get { return m_color; }
            set { m_color = value; }
        }

        private string _diffuseMap = string.Empty;

        [ProtoMember(4)]
        public string DiffuseMap
        {
            get { return _diffuseMap; }
            set { _diffuseMap = value; }
        }

        private double _textureScaleX = 1.0;

        [ProtoMember(5)]
        public double TextureScaleX
        {
            get { return _textureScaleX; }
            set { _textureScaleX = value; }
        }

        private double _textureScaleY = 1.0;

        [ProtoMember(6)]
        public double TextureScaleY
        {
            get { return _textureScaleY; }
            set { _textureScaleY = value; }
        }

        private string _bumpMap = string.Empty;

        [ProtoMember(7)]
        public string BumpMap
        {
            get { return _bumpMap; }
            set { _bumpMap = value; }
        }

        private double _metallic = 0.0;

        [ProtoMember(8)]
        public double Metallic
        {
            get { return _metallic; }
            set { _metallic = value; }
        }

        private double _smoothness = 0.5f;

        [ProtoMember(9)]
        public double Smoothness
        {
            get { return _smoothness; }
            set { _smoothness = value; }
        }

        public string GetValidMapFile()
        {
            if (!string.IsNullOrEmpty(DiffuseMap))
                return DiffuseMap;
            else
                return "";
        }
    }
}
