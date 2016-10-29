using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VectorDraw;
using VectorDraw.Professional;
using VectorDraw.Geometry;
using VectorDraw.Generics;
using VectorDraw.Generic;
using VectorDraw.Professional.vdPrimaries;
using VectorDraw.Professional.vdObjects;
using VectorDraw.Professional.vdFigures;
using VectorDraw.Professional.vdCollections;

namespace ReduceTris
{
    public class CRevit2vdl
    {
        VectorDraw.Professional.Control.VectorDrawBaseControl vec = new VectorDraw.Professional.Control.VectorDrawBaseControl();
        vdDocument doc;
        ProgressBar progressBar1 = new ProgressBar();
        public CRevit2vdl(string strFile, ProgressBar pgressBar)
        {
            progressBar1 = pgressBar;
            vec.EnsureDocument();
            vec.ActiveDocument.EnsureDefaults();
            doc = vec.ActiveDocument;
            vec.Progress += new VectorDraw.Professional.Control.ProgressEventHandler(vec_Progress);
            doc.Open(strFile);
            foreach (vdFigure vdf in doc.ActiveLayOut.Entities)
            {
                if (vdf is vdPolyface)
                {
                    vdPolyface vdp = vdf as vdPolyface;
                    gPoints gps = vdp.VertexList.Clone() as gPoints;
                    gps.RemoveEqualPoints();
                }
            }
        }

        void vec_Progress(object sender, long percent, string jobDescription)
        {
            progressBar1.Value = (int)percent;
            progressBar1.Update();
        }
        double GetArea3D(gPoint gp1, gPoint gp2, gPoint gp3)
        {
            try
            {
                gPoints gps = new gPoints();
                gps.Add(gp1);
                gps.Add(gp2);
                gps.Add(gp3);

                return Math.Abs(gps.Area3D(gps.GetNormal()));
            }
            catch
            { }
            return 0.0;

        }

        //bool Merge3DFace(vdPolyface vdp)
        //{

        //    while (true)
        //    {
        //        int icount = vdp.FaceList.Count;
        //        if (Merge3DFace1(vdp))
        //        {
        //            if (vdp.FaceList.Count == icount)
        //            {
        //                return true;
        //            }

        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }

        //}
        bool Merge3DFace(vdPolyface vdp)
        {
            
            int II = int.MaxValue;
            Int32Array i32 = vdp.FaceList;
            Dictionary<int, int> deleteID = new Dictionary<int, int>();

            Dictionary<int, int> kk1 = new Dictionary<int, int>();
            Dictionary<int, int> kk2 = new Dictionary<int, int>();

            //合并三角形
            #region 合并三角形
            for (int i = 0; i < i32.Count; i = i + 5)
            {
                if (i32[i + 4] != II && i32[i] == i32[i + 3]) //一定是没有被合并过的,说明是三角形
                {
                    int i1 = i32[i];
                    int i2 = i32[i + 1];
                    int i3 = i32[i + 2];
                    kk1.Clear();
                    kk1.Add(i1, i1);
                    kk1.Add(i2, i2);
                    kk1.Add(i3, i3);

                    for (int j = i + 5; j < i32.Count; j = j + 5)
                    {

                        if (i32[j + 4] != II && i32[j] == i32[j + 3])   //剔除已经处理过的
                        {
                            int j1 = i32[j];
                            int j2 = i32[j + 1];
                            int j3 = i32[j + 2];

                            //kk2.Clear();
                            //kk2.Add(j1, j1);
                            //kk2.Add(j2, j2);
                            //kk2.Add(j3, j3);


                            int k1 = 0;
                            int k2 = 0;
                            int k3 = 0;
                            int k4 = 0;

                            //找到公共边
                            if ((i1 == j1 || i1 == j2 || i1 == j3) && (i2 == j1 || i2 == j2 || i2 == j3))
                            {
                                k1 = i3;
                                k2 = i1;
                                k4 = i2;

                            }
                            else if ((i2 == j1 || i2 == j2 || i2 == j3) && (i3 == j1 || i3 == j2 || i3 == j3))
                            {
                                k1 = i1;
                                k2 = i2;
                                k4 = i3;

                            }
                            else if ((i1 == j1 || i1 == j2 || i1 == j3) && (i3 == j1 || i3 == j2 || i3 == j3))
                            {
                                k1 = i2;
                                k2 = i3;
                                k4 = i1;


                            }
                            else
                            {
                                continue;
                            }

                            if (!kk1.ContainsKey(j1))
                            {
                                k3 = j1;
                            }

                            if (!kk1.ContainsKey(j2))
                            {
                                k3 = j2;
                            }

                            if (!kk1.ContainsKey(j3))
                            {
                                k3 = j3;
                            }

                            //检查是否共面

                            gPoints gps3 = new gPoints();
                            //gps3.Add(vdp.VertexList[i1 - 1]);
                            //gps3.Add(vdp.VertexList[i2 - 1]);
                            //gps3.Add(vdp.VertexList[i3 - 1]);

                            gps3.Add(vdp.VertexList[k1 - 1]);
                            gps3.Add(vdp.VertexList[k2 - 1]);
                            gps3.Add(vdp.VertexList[k4 - 1]);


                            Vector vec = gps3.GetNormal();
                            double dist = Globals.DistancePointToPlane(vdp.VertexList[k3 - 1], vec, vdp.VertexList[k1 - 1]);

                            if (Math.Abs(dist) > Globals.VD_ZERO6)
                            {
                                continue;
                            }


                            //判断是否是凹多边形   凹多边形有显示问题

                            ////判断是否构成了三角形
                            gps3.InsertAt(3,vdp.VertexList[k3 - 1]);

                            //gPoints gps4=gps3.Clone() as gPoints;
                            //gps3.makeClosed();
                            //gps4.makeClosed();
                            //gps4.RemoveInLinePoints();

                            //if (gps3.Count != gps4.Count) continue;    


                            double area1 = GetArea3D(gps3[0], gps3[1], gps3[2]);
                            double area2 = GetArea3D(gps3[3], gps3[1], gps3[2]);
                            double area3 = GetArea3D(gps3[0], gps3[1], gps3[3]);
                            double area4 = GetArea3D(gps3[0], gps3[2], gps3[3]);

                            double area = Math.Max(area3, area4);
                            if (area1 + area2 <= area)  //凹多边形
                                continue;


                            i32[i] = k4;
                            i32[i + 1] = k1;
                            i32[i + 2] = k2;
                            i32[i + 3] = k3;//这里放到第3个点经常出现问题


                            i32[j + 4] = II;
                            break;
                        }
                    }
                }
            }
            #endregion
            #region 生成新的polyface
            Int32Array iii32 = new Int32Array();
            for (int i = 0; i < vdp.FaceList.Count; i = i + 5)
            {
                if (vdp.FaceList[i + 4] != II)
                {
                    iii32.Add(i32[i]);
                    iii32.Add(i32[i + 1]);
                    iii32.Add(i32[i + 2]);
                    iii32.Add(i32[i + 3]);
                    iii32.Add(i32[i + 4]);
                }

            }
            vdp.FaceList = iii32;
            vdp.Invalidate();
            #endregion
            return true;
        }
        //可以封成一个类

        public void SaveAs(string filename)
        {
            doc.FileProperties.CompressionMethod = vdFileProperties.Compression.BigDocumentSizeCompression;
            doc.SaveAs(filename);
        }
        public void Convert(vdDocument doc,ProgressBar pb,string outFileName)
        {
            this.doc = doc;
            Convert(outFileName);
        }

        void SetProgress(int Value)
        {
            if (this.progressBar1 == null) return;
            this.progressBar1.Value = Value;
            this.progressBar1.Update();
            Application.DoEvents();
        }
        public void Convert(string outFileName)
        {

            SetProgress(5);
            Dictionary<string, bool> kvs = new Dictionary<string, bool>();

            int ipr = 0;
            int count = doc.Blocks.Count;
            foreach (vdBlock vdb in doc.Blocks)   //处理块中的面片
            {
                ipr++;

                foreach (vdFigure vdf1 in vdb.Entities)
                {
                    if (vdf1 is vdPolyface)
                    {
                        vdPolyface vdff = vdf1 as vdPolyface;
                        bool ret = GetVDPBorderLine(vdff, null);   //处理圆柱
                        if (!ret)
                        {
                            Merge3DFace(vdff);    //合并面片到四边形

                        }
                    }
                }


                //while (true)   //合并圆柱体
                //{
                //    vdArray< vdFigure> tmps=new vdArray<vdFigure>();
                //    for (int i = 0; i < vdb.Entities.Count; i++)
                //    {
                //        vdFigure vdf = vdb.Entities[i];
                //        if (vdf is vdPolyface)
                //        {
                //            vdPolyface vdff1 = vdf as vdPolyface;
                //            for (int j = 0; j < vdb.Entities.Count; j++)
                //            {
                //                if (vdb.Entities[j] is vdPolyface)
                //                {
                //                    vdPolyface vdff2 = vdb.Entities[j] as vdPolyface;

                //                    vdff1.MergePolyface(vdff2);
                //                }

                //            }
                //        }
                //    }
                //}


                SetProgress(5 + (ipr * 65) / count);
            }


            ipr = 0;
            foreach (vdFigure vdf in doc.ActiveLayOut.Entities)  //合并处理处理当前的面片
            {

                ipr++;

                for (int i = vdf.XProperties.Count - 1; i >= 0; i--)
                {
                    vdXProperty vdx = vdf.XProperties[i];
                    if (vdx.Name.StartsWith("CylinderFaceData") || vdx.Name.StartsWith("POINTVECTOR"))
                    {
                        vdf.XProperties.RemoveItem(vdx);
                    }
                }

                if (vdf is vdPolyface)
                {
                    kvs.Clear();
                    vdPolyface vdp = vdf as vdPolyface;

                 
                    bool  ret = GetVDPBorderLine(vdp, vdp);  //处理圆柱体
                    
                    if (!ret)
                    {
                        Merge3DFace(vdp);   //合并到四边形
                    }
                }

                SetProgress(70 + (ipr * 30) / doc.ActiveLayOut.Entities.Count);
                
            }

            //圆柱片试图合并成圆柱

            doc.Purge();
            SaveAs(outFileName);

            MessageBox.Show("转化已完成！");
        }


        

        void ProInsert(vdInsert vdi)
        {

        }
        string GetPolyFace(vdPolyface vdf, vdXProperties vdxs)
        {
            foreach (vdXProperty vdx in vdxs)
            {
                if (vdx.Name.StartsWith(""))
                {
                    string str = vdx.PropValue.ToString();
                    string[] strs = str.Split(',');
                }
            }
            return "";

        }



        /// <summary>
        /// /处理polyface
        /// </summary>
        //bool <param name="vdp"></param>
        bool GetVDPBorderLine(vdPolyface vdp, vdFigure vdPf)
        {
            Vector vec = vdp.VertexList.GetNormal();

            if (vdp.VertexList.Is2D(0.0001, vec))
            {
                return false;
            }

            vdPolyface vdpOut = new vdPolyface();
            Dictionary<string, bool> kvs = new Dictionary<string, bool>();
            Int32Array iii32 = vdp.FaceList;
            for (int i = 0; i < iii32.Count; i++)
            {
                if (vdp.FaceList[i] < 0)
                {
                    if (i % 5 != 4)
                    {
                        vdp.FaceList[i] = -vdp.FaceList[i];
                    }
                }

            }
            iii32 = vdp.FaceList;
            for (int i = 0; i < iii32.Count; i = i + 5)
            {
                AddSide(kvs, iii32[i], iii32[i + 1]);
                AddSide(kvs, iii32[i + 1], iii32[i + 2]);
                AddSide(kvs, iii32[i + 2], iii32[i]);
            }
            //找到外边界
            Int32Array i32 = ParaseSide2List(kvs, vdp.VertexList.Count + 1);

            if (i32.Count < 5 ) return false;
            //找到向量
            int iii = 1;
            gPoints Points = vdp.VertexList;
            Points.makeClosed();
            gPoints gps = new gPoints();// Points.Clone() as gPoints;
            //if (i32.Count < 10) return false;
            foreach (int ii in i32)
            {
                gps.Add(Points[ii - 1]);
            }
            gps.RemoveEqualPoints(Globals.VD_ZERO5);

            //gps.RemoveInLinePoints();

            Int32Array ii32 = new Int32Array();
            gPoints gpss = new gPoints();


            #region  延长线段到最长

            gpss.Add(gps[0]);


            for (int i = 0; i < gps.Count - 1; i++)
            {
                if (i == 5)
                {
                    int j = 0;
                }

                if (i != gps.Count - 2)
                {

                    double dd = Globals.distPointFromLine(gps[i + 2], gps[i], gps[i + 1]);
                    if (Globals.distPointFromLine(gps[i + 2], gps[i], gps[i + 1]) < 0.2)
                    {
                        continue;
                    }
                    else
                    {
                        gpss.Add(gps[i + 1]);
                    }
                }

                if (i == gps.Count - 2)
                {

                    if (Globals.distPointFromLine(gps[1], gps[i], gps[i + 1]) < 0.2)
                    {
                        gpss.RemoveAt(0);
                    }
                    else
                    {
                        gpss.Add(gps[i + 1]);
                    }
                }


            }

            #endregion
            gpss.makeClosed();
            gpss.RemoveLast();
            //vdPolyline vdp11 = new vdPolyline(doc,new Vertexes(gpss));
            //doc.ActiveLayOut.Entities.Add(vdp11);

            //找到四条边中符合圆柱体标准的。

            if (gpss.Count % 2 != 0 || gpss.Count < 10) return false;

            int half = gpss.Count / 2;

            gPoints gEndSide1 = new gPoints();
            gPoints gEndSide2 = new gPoints();
            gPoints gParaSide1 = new gPoints();
            gPoints gParaSide2 = new gPoints();



            for (int i = 0; i < gpss.Count / 2; i++)
            {
                Vector v1 = new Vector(gpss[i], gpss[i + 1]);
                Vector v2 = new Vector(gpss[i + half], gpss[(half + i + 1) % gpss.Count]);
                v1.Cross(v2);
                if (v1.Length <Globals.VD_ZERO7)  //说明平行
                {
                    gEndSide1.RemoveAll();
                    gEndSide2.RemoveAll();
                    gParaSide1.RemoveAll();
                    gParaSide2.RemoveAll();

                    gParaSide1.Add(gpss[i]);
                    gParaSide1.Add(gpss[i + 1]);

                    
                    gParaSide2.Add(gpss[i + half]);
                    gParaSide2.Add(gpss[(half + i + 1) % gpss.Count]);

                    for (int j = i + 1; j < i + half; j++)
                    {
                        gEndSide1.Add(gpss[j]);
                    }




                    for (int j = i + half + 1; j < i + 2 * half; j++)
                    {
                        gEndSide2.Add(gpss[j % gpss.Count]);
                    }

                    gPoint sp1 = new gPoint();
                    gPoint sp2 = new gPoint();
                    double radius = 0.0;

                    //判断是个边是否符合圆柱体标准
                    if (!IS4SideCyln(gEndSide1, gEndSide2, gParaSide1, gParaSide2, ref sp1, ref sp2, out radius))  //不符合圆柱体标准 ，直接返回错误
                    {
                        continue;
                    }
                    gpss.RemoveAll();


                    //这里可以进行圆柱简化

                    gpss.AddRange(gEndSide1);
                    gpss.AddRange(gParaSide2);
                    gpss.AddRange(gEndSide2);
                    gpss.AddRange(gParaSide1);


                    //是否齐头圆柱，即没有切过的圆柱,如果齐圆柱头，特殊处理，此处暂时不变,

                    //

                    //


                    gpss.RemoveEqualPoints();
                    gpss.makeClosed();
                    gpss.RemoveLast();

                    half = gpss.Count / 2;

                    vdpOut.VertexList = gpss;
                    vdpOut.FaceList = new Int32Array();

                    for (int ii = 1; ii < half; ii++)
                    {
                        vdpOut.FaceList.Add(ii);
                        vdpOut.FaceList.Add(ii + 1);
                        vdpOut.FaceList.Add(gpss.Count - (ii + 1) + 1);
                        vdpOut.FaceList.Add(gpss.Count - ii + 1);
                        vdpOut.FaceList.Add(-1);
                    }


                    vdp.FaceList = vdpOut.FaceList;
                    vdp.VertexList = vdpOut.VertexList;

                    ////加一个标记，以后好合并
                    vdXProperty vdx = vdp.XProperties.Add("IsPart");
                    vdx.PropValue = true;
                  
                    break;


                }
            }


            //找到两个顶边，如果多个就扔掉了。

            //GetNonParaSide(vdp.VertexList, i32, orign, vector);
            return true;
            //Int32Array side1=

        }

        public gPoint getCircleCenFrom3Pts(gPoint p1, gPoint p2, gPoint p3)
        {



            gPoint pt = gPoint.MidPoint(p1, p2);

            gPoint gp1 = new gPoint();


            int ret = Globals.Intersection3DSegmentPlane(p2, p3, new Vector(p2, p1), pt, out gp1);
            if (ret == 0)  //预防有平行的情况
            {
                ret = Globals.Intersection3DSegmentPlane(p1, p3, new Vector(p2, p1), pt, out gp1);
            }

            gPoint point2 = gPoint.MidPoint(p2, p3);

            gPoint gp2 = new gPoint();
            ret = Globals.Intersection3DSegmentPlane(p1, p2, new Vector(p2, p3), point2, out gp2);
            if (ret == 0) //预防有平行的情况
            {
                ret = Globals.Intersection3DSegmentPlane(p1, p3, new Vector(p2, p3), point2, out gp2);
            }

            gPoint retpt = new gPoint();
            Globals.IntersectionLL3D(pt, gp1, point2, gp2, retpt);
            return retpt;
        }
        ///判断是个边是否符合圆柱体标准
        bool IS4SideCyln(gPoints gEndSide1, gPoints gEndSide2, gPoints gParaSide1, gPoints gParaSide2, ref gPoint center1, ref gPoint center2, out double radius)
        {
            int half = gEndSide1.Count / 2;
            radius = 0.0;
            gPoint gp1 = gEndSide1[half];
            gPoint gp2 = Globals.LineNearestTo(gParaSide1[0], gParaSide1[1], gp1, false);
            gPoint gp3 = Globals.LineNearestTo(gParaSide2[0], gParaSide2[1], gp1, false);

            gPoints gps1 = new gPoints();

            if (gp1.AreEqual(gp2) || gp1.AreEqual(gp3) || gp2.AreEqual(gp3)) return false;

            center1 = getCircleCenFrom3Pts(gp1, gp2, gp3);
            double dist = gp1.Distance3D(center1);

            gp1 = gEndSide2[half];
            gp2 = Globals.LineNearestTo(gParaSide1[0], gParaSide1[1], gp1, false);
            gp3 = Globals.LineNearestTo(gParaSide2[0], gParaSide2[1], gp1, false);

            if (gp1.AreEqual(gp2) || gp1.AreEqual(gp3) || gp2.AreEqual(gp3)) return false;

            center2 = getCircleCenFrom3Pts(gp1, gp2, gp3);

            //上边的距离是否满足半径要求
            for (int i = 0; i < gEndSide1.Count; i++)
            {
                double ddd = Globals.distPointFromLine(gEndSide1[i], center1, center2);
                if (Math.Abs((ddd - dist)) > Globals.VD_ZERO2)
                {
                    radius = 0;
                    return false;
                }
            }
            //下边的距离是否满足半径要求
            for (int i = 0; i < gEndSide2.Count; i++)
            {
                double ddd = Globals.distPointFromLine(gEndSide2[i], center1, center2);
                if (Math.Abs((ddd - dist)) > Globals.VD_ZERO2)
                {
                    radius = 0;
                    return false;
                }
            }

            radius = dist;

            return true;
        }





        /// <summary>
        /// 找到对应的点和向量
        /// </summary>
        /// <param name="i32">定点数组</param>
        /// <param name="points">坐标数组</param>
        /// <param name="vdPf">父图形，里面存点和向量</param>
        /// <param name="orgin">输出找到的点</param>
        /// <param name="vector">输出找到的向量</param>
        /// <returns>是否找到</returns>
        /// 
        bool FindPointVector(gPoints Points, vdPolyface vdp, vdFigure vdPf, ref  gPoint orign, ref  Vector vector)
        {
            int iii = 1;
            while (true)
            {
                string strName = "POINTVECTOR" + iii.ToString();
                iii++;
                vdXProperty vdx = vdPf.XProperties.FindName(strName);
                if (vdx != null)
                {
                    string strValue = (string)vdx.PropValue;
                    string[] strs = strValue.Split(new char[] { ',' });
                    orign = new gPoint(double.Parse(strs[0]), double.Parse(strs[1]), double.Parse(strs[2]));
                    vector = new Vector(double.Parse(strs[3]), double.Parse(strs[4]), double.Parse(strs[5]));



                    double d = -1.0;
                    gPoint gEnd = orign + vector;


                    int currenti = 0;
                    for (int i = 0; i < Points.Count; i++)
                    {

                        currenti = i;
                        gPoint gp = Points[i];
                        double dd = gp.DistanceFromLine(orign, gEnd);
                        if (d < 0)
                        {
                            d = dd;
                        }
                        else
                        {
                            if (Math.Abs(dd - d) > Globals.VD_ZERO5)
                            {
                                //MessageBox.Show("Error!");
                                break;
                            }

                        }
                    }
                    if (currenti == Points.Count - 1)
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        bool FindPointVector(Int32Array i32, gPoints Points, vdFigure vdPf, ref  gPoint orign, ref  Vector vector)
        {
            int iii = 1;
            while (true)
            {
                string strName = "POINTVECTOR" + iii.ToString();
                iii++;
                vdXProperty vdx = vdPf.XProperties.FindName(strName);
                if (vdx != null)
                {
                    string strValue = (string)vdx.PropValue;
                    string[] strs = strValue.Split(new char[] { ',' });
                    orign = new gPoint(double.Parse(strs[0]), double.Parse(strs[1]), double.Parse(strs[2]));
                    vector = new Vector(double.Parse(strs[3]), double.Parse(strs[4]), double.Parse(strs[5]));
                    double d = -1.0;

                    for (int i = 0; i < i32.Count; i++)
                    {
                        gPoint gEnd = orign + vector;
                        gPoint gp = Points[i32[i]];
                        double dd = gp.DistanceFromLine(orign, gEnd);
                        if (d < 0)
                        {
                            d = dd;
                        }
                        else
                        {
                            if (Math.Abs(dd - d) > Globals.VD_ZERO5)
                            {
                                break;
                            }

                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 处理边
        /// </summary>
        /// <param name="kvs"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        void AddSide(Dictionary<string, bool> kvs, int i, int j)
        {
            string strkey = "";
            if (i < j)
            {
                strkey = i + "," + j;
            }
            else
            {
                strkey = j + "," + i;
            }
            if (kvs.ContainsKey(strkey))
            {
                kvs.Remove(strkey);
            }
            else
            {
                kvs.Add(strkey, true);
            }
        }
        //生成队列
        Int32Array ParaseSide2List(Dictionary<string, bool> kvs, int count)
        {
            //生成两个字典
            //Dictionary<string, string> arr = new Dictionary<string, string>();
            //foreach (KeyValuePair<string, bool> kv in kvs)
            //{
            //    arr.Add(kv.Key, kv.Key);
            //}

            //Int32Array ii32 = new Int32Array();
            //int[] Points = new int[count];


            Dictionary<int, int> arr1 = new Dictionary<int, int>();
            Dictionary<int, int> arr2 = new Dictionary<int, int>();
            Dictionary<int, int> arr3 = new Dictionary<int, int>();
            Int32Array i32 = new Int32Array();
            int beginNode = -1;
            //找出非重复的边

            foreach (KeyValuePair<string, bool> kv in kvs)
            {
                string str = kv.Key;
                string[] strArr = str.Split(new char[] { ',' });
                int i1 = int.Parse(strArr[0]);
                int i2 = int.Parse(strArr[1]);
                if (beginNode == -1)
                {
                    beginNode = i1;
                    break;
                }

            }

            //找到外循环边

            i32.Add(beginNode);

            //从第一个开始循环往后加

            int currentNode = beginNode;
            while (true)
            {
                int len = i32.Count;
                int nextNode = -1;
                string strKey = "";
                int iEnd = -1;
                foreach (KeyValuePair<string, bool> kv in kvs)
                {
                    string str = kv.Key;
                    string[] strArr = str.Split(new char[] { ',' });
                    int i1 = int.Parse(strArr[0]);
                    int i2 = int.Parse(strArr[1]);

                    if (currentNode == i1)
                    {
                        iEnd = i2;
                        strKey = kv.Key;

                        break;
                    }
                    else if (currentNode == i2)
                    {
                        iEnd = i1;
                        strKey = kv.Key;

                        break;
                    }

                }
                if (strKey != "")
                {
                    i32.Add(iEnd);
                    currentNode = iEnd;
                    kvs.Remove(strKey);
                }




                if (len == i32.Count)  //没有再增加的了
                {
                    break;
                }
            }
            return i32;
        }

        Int32Array[] MergeList(vdPolyface vdp, Int32Array i32)
        {
            vdPolyface vdp1 = new vdPolyface();

            gTriangles gts = vdp.GetTriangles();
            gTriangle gt = gts[1];

            Vector vec1 = new Vector();
            Vector vec2 = new Vector();
            gPoint gp1 = new gPoint();
            gPoint gp2 = new gPoint();

            gt.GetPlane(out vec1, out gp1);

            //找到两个不共线的3三角形
            Int32Array[] aa = null;
            return aa;


        }


    }
}
