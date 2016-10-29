using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exporter
{
    public class TriangleIndexData
    {
        public TriangleIndexData(int n1, int n2, int n3)
        {
            this.V1 = n1;
            this.V2 = n2;
            this.V3 = n3;
        }

        public int V1
        {
            get;
            set;
        }

        public int V2
        {
            get;
            set;
        }

        public int V3
        {
            get;
            set;
        }
    }
}

