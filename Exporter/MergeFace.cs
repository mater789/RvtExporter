using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorDraw.Generics;
using VectorDraw.Professional.Control;
using VectorDraw.Professional.vdFigures;
using VectorDraw.Professional.vdPrimaries;

namespace Exporter
{
    public class MergeFace
    {
        /// <summary>
        /// 旧的合并方法，已经弃用
        /// </summary>
        /// <param name="v"></param>
        /// <param name="p"></param>
        public static void Merge1(VectorDrawBaseControl v, FormProgress p)
        {
            p.Text = "合并面片...";
            int ii = 0;
            int count = v.ActiveDocument.Blocks.Count;

            vdArray<vdBlock> blocks = new vdArray<vdBlock>();
            foreach (vdBlock vdb in v.ActiveDocument.Blocks)
            {
                if (vdb.Entities.Count == 0)
                {
                    blocks.AddItem(vdb);
                }

                int percent = (int)((++ii * 1.0 / count) * 100);

                p.SetProgress(percent);

                Dictionary<string, vdPolyface> layFaces = new Dictionary<string, vdPolyface>();
                Dictionary<int, int> ids = new Dictionary<int, int>();
                foreach (vdFigure vdf in vdb.Entities)
                {
                    if (vdf is vdPolyface)
                    {
                        vdPolyface vdp = vdf as vdPolyface;

                        if (!layFaces.ContainsKey(vdp.Layer.Name))
                        {
                            layFaces.Add(vdp.Layer.Name, vdp);
                            ids.Add(vdp.Id, vdp.Id);
                        }
                        else
                        {
                            layFaces[vdp.Layer.Name].MergePolyface1(vdp);
                        }
                    }
                }
                for (int i = vdb.Entities.Count - 1; i >= 0; i--)
                {
                    vdFigure vdf = vdb.Entities[i];
                    if (vdf is vdPolyface && ids.ContainsKey(vdf.Id))
                    {
                        vdPolyface vdp1 = vdf as vdPolyface;
                        if (vdp1.FaceList.Count > 0)
                        {
                            vdp1.ClearVerticies();
                        }
                        else
                        {
                            vdb.Entities.RemoveItem(vdf);
                            vdf.Dispose();
                            vdf = null;
                        }
                    }
                    else if (vdf is vdPolyface)
                    {
                        vdb.Entities.RemoveItem(vdf);
                        vdf.Dispose();
                        vdf = null;
                    }
                }
            }
            foreach (vdBlock vdb in blocks)
            {
                v.ActiveDocument.Blocks.RemoveItem(vdb);
                vdb.Dispose();
                //vdb = null;
            }
            GC.Collect();
        }

        public static void Merge(VectorDrawBaseControl v, FormProgress p)
        {
            p.Text = "合并面片...";

            int ii = 0;
            int count = v.ActiveDocument.Blocks.Count;
            vdArray<vdBlock> blocks = new vdArray<vdBlock>();
            bool LargeNum = false;

            foreach (vdBlock vdb in v.ActiveDocument.Blocks)
            {

                if (vdb.Entities.Count == 0)
                {
                    blocks.AddItem(vdb);
                }
                ii++;
                int value = (ii * 100) / count;
                Dictionary<string, vdPolyface> layFaces = new Dictionary<string, vdPolyface>();
                Dictionary<int, int> ids = new Dictionary<int, int>();
                int icount = 0;

                p.SetProgress(value);

                //if (vdb.Entities.Count > 5000)
                //{
                //    LargeNum = true;
                //}
                //else
                //{
                //    LargeNum = false;
                //}

                foreach (vdFigure vdf in vdb.Entities)
                {
                    if (vdf is vdPolyface)
                    {
                        vdPolyface vdp = vdf as vdPolyface;

                        if (!layFaces.ContainsKey(vdp.Layer.Name))
                        {
                            layFaces.Add(vdp.Layer.Name, vdp);
                            ids.Add(vdp.Id, vdp.Id);
                        }
                        else
                        {
                            layFaces[vdp.Layer.Name].MergePolyface1(vdp);
                        }

                        //if (LargeNum)
                        //{
                        //    icount++;
                        //}
                    }
                }

                icount = 0;
                for (int i = vdb.Entities.Count - 1; i >= 0; i--)
                {
                    vdFigure vdf = vdb.Entities[i];
                    if (vdf is vdPolyface && ids.ContainsKey(vdf.Id))
                    {
                        vdPolyface vdp1 = vdf as vdPolyface;
                        if (vdp1.FaceList.Count > 0)
                        {
                            vdp1.ClearVerticies1();
                        }
                        else
                        {
                            vdb.Entities.RemoveItem(vdf);
                            vdf.Dispose();
                            vdf = null;
                        }
                    }
                    else if (vdf is vdPolyface)
                    {
                        vdb.Entities.RemoveItem(vdf);
                        vdf.Dispose();
                        vdf = null;
                    }

                    //if (LargeNum)
                    //{
                    //    icount++;
                    //}
                }

                //vdb.Update();

            }
            foreach (vdBlock vdb in blocks)
            {
                v.ActiveDocument.Blocks.RemoveItem(vdb);
                vdb.Dispose();
                //vdb = null;
            }


            GC.Collect();
        }



    }
}
