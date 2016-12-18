using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exporter
{
    public class MaterialData
    {
        private string m_name = string.Empty;
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        private int m_transparency = 0;
        public int Transparency
        {
            get { return m_transparency; }
            set { m_transparency = value; }
        }

        private ColorData m_color = new ColorData();
        public ColorData Color
        {
            get { return m_color; }
            set { m_color = value; }
        }

        public string DiffuseMap { get; set; } = string.Empty;
        public double TextureScaleX { get; set; } = 1.0;
        public double TextureScaleY { get; set; } = 1.0;


        public string BumpMap { get; set; } = string.Empty;
    }
}
