using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;

namespace Exporter
{
    public class BlockData
    {
        private string m_name = string.Empty;
        public string Name
        {
            set { m_name = value; }
            get { return m_name; }
        }

        private List<InsertData> m_Inserts = new List<InsertData>();
        public List<InsertData> Inserts
        {
            set { m_Inserts = value; }
            get { return m_Inserts; }
        }

        private List<MeshData> m_Meshs = new List<MeshData>();
        public List<MeshData> Meshs
        {
            get { return m_Meshs; }
            set { m_Meshs = value; }
        }

        public bool m_bIsPipe = false;
        public bool IsPipe
        {
            get { return m_bIsPipe; }
            set { m_bIsPipe = value; }
        }

        private PipeInfo m_Pipe = null;
        public PipeInfo PipeInfo
        {
            get { return m_Pipe; }
            set { m_Pipe = value; }
        }

        private bool m_bIsByBlock = false;
        public bool IsByBlock
        {
            get { return m_bIsByBlock; }
            set { m_bIsByBlock = value; }
        }
    }
}
