using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exporter
{
    public class TextureUV
    {
        public TextureUV(double u, double v)
        {
            this.U = u;
            this.V = v;
        }

        public double U { get; set; }
        public double V { get; set; }
    }
}
