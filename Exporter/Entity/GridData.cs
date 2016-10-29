using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exporter
{
    /// <summary>
    /// 轴网
    /// </summary>
    public class GridData
    {
        public string Name { get; set; }

        public List<CurveData> Curves { get; set; }

        public Dictionary<string, List<PropertyData>> Properties { get; set; }
    }
}
