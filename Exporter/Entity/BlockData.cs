using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;
using ProtoBuf;

namespace Exporter
{
    [ProtoContract]
    public class BlockData
    {
        private string m_name = string.Empty;

        [ProtoMember(1)]
        public string Name
        {
            set { m_name = value; }
            get { return m_name; }
        }

        private List<InsertData> m_Inserts = new List<InsertData>();
        [ProtoMember(2)]
        public List<InsertData> Inserts
        {
            set { m_Inserts = value; }
            get { return m_Inserts; }
        }

        private List<MeshData> m_Meshs = new List<MeshData>();
        [ProtoMember(3)]
        public List<MeshData> Meshs
        {
            get { return m_Meshs; }
            set { m_Meshs = value; }
        }

        public bool m_bIsPipe = false;
        [ProtoMember(4)]
        public bool IsPipe
        {
            get { return m_bIsPipe; }
            set { m_bIsPipe = value; }
        }

        private PipeInfo m_Pipe = null;
        [ProtoMember(5)]
        public PipeInfo PipeInfo
        {
            get { return m_Pipe; }
            set { m_Pipe = value; }
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
