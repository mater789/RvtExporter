using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using VectorDraw.Generics;
using VectorDraw.Geometry;
using VectorDraw.Professional.Control;
using VectorDraw.Professional.vdCollections;
using VectorDraw.Professional.vdFigures;
using VectorDraw.Professional.vdObjects;
using VectorDraw.Professional.vdPrimaries;

namespace Exporter
{
    public class ReplaceParkingNumber
    {
        private VectorDrawBaseControl m_vDraw = null;

        public ReplaceParkingNumber(VectorDrawBaseControl vDraw)
        {
            m_vDraw = vDraw;
        }

        public ParkingExportSetting Setting
        {
            get;
            set;
        }

        public void ReplaceText()
        {
            ReplaceText(m_vDraw.ActiveDocument.Blocks, m_vDraw.ActiveDocument, this.Setting.Width, this.Setting.Height, this.Setting.PropertyName);
        }

        /// <summary>
        /// 替换字体
        /// </summary>
        /// <param name="vdbs"></param>
        private void ReplaceText(vdBlocks vdbs, vdDocument doc, double width, double height, string PropName)
        {
            //doc.Purge();
            Dictionary<string, string> BlksMap = new Dictionary<string, string>();
            Dictionary<string, string> BlksMapTmp = new Dictionary<string, string>();

            //删除车位编号
            foreach (vdFigure vdf in doc.ActiveLayOut.Entities)
            {
                if (vdf is vdInsert)
                {
                    vdInsert vdi = vdf as vdInsert;
                    vdXProperty vdx = vdi.XProperties.FindName(PropName);
                    if (vdx != null)
                    {
                        string strName = vdx.PropValue.ToString();
                        vdBlock vdb = vdi.Block;
                        Box box = vdb.BoundingBox();

                        Box textbox = new Box(new gPoint(box.MidPoint.x - width / 2.0, box.Min.y), new gPoint(box.MidPoint.x + width / 2.0, box.Min.y + height));
                        vdArray<vdFigure> vda = new vdArray<vdFigure>();
                        foreach (vdFigure vdff in vdb.Entities)
                        {
                            if (vdff is vdPolyface)
                            {
                                if (vdff.BoundingBox.BoxInBox(textbox))
                                {
                                    vda.AddItem(vdff);
                                }
                            }
                        }

                        Box b = new Box();
                        foreach (vdFigure vdf1 in vda)
                        {
                            b.AddBox(vdf1.BoundingBox);
                            vdb.Entities.RemoveItem(vdf1);
                        }

                        //文字样式
                        vdTextstyle vdts = doc.TextStyles.FindName("车位编号");
                        if (vdts == null)
                        {
                            vdts = new vdTextstyle(doc);
                            vdts.Name = "车位编号";
                            vdts.Height = b.Height;
                            //MessageBox.Show(b.Height.ToString());
                            doc.TextStyles.Add(vdts);
                        }

                        //文字
                        vdText vdt = new vdText(doc);
                        vdt.Style = vdts;
                        vdt.TextString = vdx.PropValue.ToString();
                        vdt.HorJustify = VectorDraw.Professional.Constants.VdConstHorJust.VdTextHorCenter;
                        vdt.VerJustify = VectorDraw.Professional.Constants.VdConstVerJust.VdTextVerCen;
                        vdt.InsertionPoint = b.MidPoint;
                        vdb.Entities.AddItem(vdt);

                        vdf.Invalidate();
                        vdf.Update();
                        string ssss = GetBlkMd5(vdi.Block);

                        if (!BlksMap.ContainsKey(vdi.Block.Name))
                            BlksMap.Add(vdi.Block.Name, ssss);
                        else
                        {
                            MessageBox.Show("可能存在重复编号的车位，请仔细查看！");
                        }
                    }
                }
            }

            //查找md5的计数

            Dictionary<string, int> md5count = new Dictionary<string, int>();
            foreach (KeyValuePair<string, string> kv in BlksMap)
            {
                if (!md5count.ContainsKey(kv.Value))
                {
                    md5count.Add(kv.Value, 1);
                }
                else
                {
                    md5count[kv.Value]++;
                }
            }

            foreach (KeyValuePair<string, string> kv in BlksMap)
            {
                if (md5count[kv.Value] > 1)
                {
                    vdBlock vdb2 = doc.Blocks.FindName(kv.Value);
                    vdBlock vdb = doc.Blocks.FindName(kv.Key);
                    if (vdb2 == null)  //md5码的块不存在的话
                    {
                        if (vdb != null)
                        {
                            vdBlock vdb1 = vdb.Clone(doc) as vdBlock;
                            for (int i = vdb1.Entities.Count-1; i >= 0; i--)
                            {
                                if (vdb1.Entities[i] is vdText)
                                {
                                    vdb1.Entities.RemoveItem(vdb1.Entities[i]);
                                }
                            }
                                vdb1.Name = kv.Value;
                            doc.Blocks.Add(vdb1);
                        }
                    }
                    //原来的内容作为一个块，加进去    

                    for (int i = vdb.Entities.Count-1; i >= 0; i--)
                    {
                        if (!(vdb.Entities[i] is vdText))
                        {
                            vdb.Entities.RemoveItem(vdb.Entities[i]);
                        }
                    }

                    //vdb.Entities.RemoveAll();
                    vdInsert vdi = new vdInsert(doc);
                    vdi.Block = doc.Blocks.FindName(kv.Value);  //
                    vdb.Entities.Add(vdi);

                    //将字体加上去

                    vdb.Update();

                }
            }

            //ProCarNum(doc, PropName, width, height);  //处理车位编号
        }

        

        string GetBlkMd5(vdBlock vdb)
        {
            SortedList<string, string> ss = new SortedList<string, string>();

            //Dictionary<string, string> dics = new Dictionary<string, string>();
            foreach (vdFigure vdf in vdb.Entities)
            {
                if (vdf is vdPolyface)
                {
                    vdPolyface vdp = vdf as vdPolyface;
                    string str1 = Polyface2string(vdp);
                    if (!ss.ContainsKey(str1))
                    {
                        ss.Add(str1, str1);
                    }
                }

            }
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, string> s in ss)
            {
                sb.AppendLine(s.Key);
            }

            string str2 = GetMD5HashFromString(sb.ToString());
            return str2;
        }

        /// <summary>
        /// 处理车位编号
        /// </summary>
        /// <returns></returns>
        void ProCarNum(vdDocument doc, string PropName, double width, double height)
        {
            foreach (vdFigure vdf in doc.ActiveLayOut.Entities)
            {
                if (vdf is vdInsert)
                {
                    vdInsert vdi = vdf as vdInsert;

                    vdXProperty vdx = vdi.XProperties.FindName(PropName);
                    if (vdx != null)
                    {
                        //删除原来的文字
                        vdBlock vdb = vdi.Block;
                        for (int i = vdb.Entities.Count - 1; i >= 0; i--)
                        {
                            if (vdb.Entities[i] is vdText)
                            {
                                vdb.Entities.RemoveAt(i);
                            }
                        }
                        //文字样式
                        vdTextstyle vdts = doc.TextStyles.FindName("车位编号");
                        if (vdts == null)
                        {
                            vdts = new vdTextstyle(doc);
                            vdts.Height = height / 5.0;
                        }
                        //字体
                        vdText vdt = new vdText(doc);
                        vdt.Style = vdts;
                        vdt.TextString = vdx.PropValue.ToString();
                        vdt.HorJustify = VectorDraw.Professional.Constants.VdConstHorJust.VdTextHorCenter;
                        vdt.VerJustify = VectorDraw.Professional.Constants.VdConstVerJust.VdTextVerCen;


                        Box bb = vdb.BoundingBox();
                        vdt.InsertionPoint = new gPoint(bb.MidPoint.x, bb.Min.y + height / 2.0, bb.Min.z);
                        vdb.Entities.Add(vdt);
                    }
                }
            }
        }

        /// <summary>
        /// 生成指定字符串的MD5码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string GetMD5HashFromString(string str)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytValue, bytHash;
            bytValue = System.Text.Encoding.UTF8.GetBytes(str);
            bytHash = md5.ComputeHash(bytValue);
            md5.Clear();

            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            }

            return sTemp.ToUpper();
        }

        string Polyface2string(vdPolyface vdp)
        {
            if (vdp == null) return "";
            StringBuilder sb = new StringBuilder();
            foreach (int i in vdp.FaceList)
            {
                sb.Append(i + ",");
            }
            sb.Append("\n");
            foreach (gPoint gp in vdp.VertexList)
            {
                sb.Append(gp.x + "," + gp.y + "," + gp.z + ",");
            }
            sb.Append("\n");
            sb.AppendLine(vdp.Layer.Name);
            sb.AppendLine(vdp.PenColor.ColorIndex.ToString());
            sb.AppendLine(vdp.PenWidth.ToString());
            sb.AppendLine(vdp.SmoothAngle.ToString());
            sb.AppendLine(vdp.visibility.ToString());
            return sb.ToString();

        }
    }
}
