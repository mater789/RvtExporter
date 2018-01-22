using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows.Forms;

namespace AddinProcesser
{
    class Program
    {
#if _2016
        private static string _destFolderPath = @"c:\ProgramData\Autodesk\Revit\Addins\2016";
#elif _2017
        private static string _destFolderPath = @"c:\ProgramData\Autodesk\Revit\Addins\2017";
#endif

        private static string _addinFileName = @"BimExporter.addin";

        static void Main(string[] args)
        {
            ProcessXml();
        }

        public static bool ProcessXml()
        {
            var sourceFilePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\" + _addinFileName;
            if (!File.Exists(sourceFilePath))
            {
                MessageBox.Show("没有找到.addin文件，请联系管理员或者供应商");
                return false;
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(sourceFilePath);
                var nodes = doc.GetElementsByTagName("Assembly");
                if (nodes.Count < 1)
                {
                    MessageBox.Show("读取配置文件失败！请联系管理员或者供应商");
                    return false;
                }

                foreach (XmlNode node in nodes)
                {
#if _2016
                    node.InnerText = Path.GetDirectoryName(Application.ExecutablePath) + "\\Exporter.dll";
#elif _2017
                    node.InnerText = Path.GetDirectoryName(Application.ExecutablePath) + "\\Exporter2017.dll";
#endif
                }

                doc.Save(_destFolderPath + "\\" + _addinFileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

            return true;
        }
    }
}
