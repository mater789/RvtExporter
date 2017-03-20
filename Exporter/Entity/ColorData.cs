using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Exporter
{
    [ProtoContract]
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

        [ProtoMember(1)]
        public byte Red
        {
            get;
            set;
        }

        [ProtoMember(2)]
        public byte Green
        {
            get;
            set;
        }

        [ProtoMember(3)]
        public byte Blue
        {
            get;
            set;
        }
    }
}
