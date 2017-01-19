using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exporter
{
    public class LevelData
    {
        public LevelData()
        {
            this.Name = "";
            this.Height = 0.0;
        }

        public string Name { get; set; }

        public double Height { get; set; }
    }
}
