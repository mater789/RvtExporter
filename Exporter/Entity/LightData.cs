using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exporter
{
    public class LightData
    {
        public string Name { get; set; }

        public double ColorTemperature { get; set; }

        public double Lumen { get; set; }

        public ColorData Color { get; set; }

        public TransformData TransData { get; set; }

    }
}
