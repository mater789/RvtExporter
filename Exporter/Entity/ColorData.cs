using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exporter
{
    public class ColorData
    {
        public ColorData(byte r, byte g, byte b)
        {
            this.Red = r;
            this.Green = g;
            this.Blue = b;
        }

        public ColorData()
        {
            this.Red = 255;
            this.Green = 255;
            this.Blue = 255;
        }


        public byte Red
        {
            get;
            set;
        }

        public byte Green
        {
            get;
            set;
        }

        public byte Blue
        {
            get;
            set;
        }
    }
}
