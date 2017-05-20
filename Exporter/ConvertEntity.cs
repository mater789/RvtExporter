using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using VectorDraw.Professional.vdFigures;
using VectorDraw.Professional.vdPrimaries;
using VectorDraw.Professional.vdCollections;
using VectorDraw.Geometry;
using Autodesk.Revit.DB;
using VectorDraw.Professional.vdObjects;

namespace Exporter
{
    internal class ConvertEntity
    {
        private VectorDraw.Professional.Control.VectorDrawBaseControl vDraw = new VectorDraw.Professional.Control.VectorDrawBaseControl();
        private FormProgress m_formProgress = null;

        private static string _gGridLayerName = "GRID__";

        public IWin32Window WndParent
        {
            get;
            set;
        }

        public List<MaterialData> Materials
        {
            get;
            set;
        }

        public BlockData ModelBlock
        {
            get;
            set;
        }

        public List<LevelData> Levels { get; set; }

        public Dictionary<string, BlockData> DictBlocks { get; set; }

        private bool m_OptimizeTriangle = false;
        public bool OptimizeTriangle
        {
            get { return m_OptimizeTriangle; }
            set { m_OptimizeTriangle = value; }
        }

        public ExportSetting ExportSetting
        {
            get;
            set;
        }

        private Dictionary<string, List<SketchData>> m_dictFamilySketch;
        public Dictionary<string, List<SketchData>> FamilySketchDictionary
        {
            get { return m_dictFamilySketch; }
            set
            {
                m_dictFamilySketch = value;
                foreach (var data in m_dictFamilySketch.Values)
                {
                    foreach (var sketch in data)
                        sketch.ConvertToMM();
                }
            }
        }

        private List<GridData> m_listGrids;

        public List<GridData> Grids
        {
            get { return m_listGrids; }
            set
            {
                m_listGrids = value;
                foreach (var grid in m_listGrids)
                    grid.Curves.ForEach(curve => curve.ConvertToMM());
            }
        }

        private Dictionary<string, LocationData> m_dictLocationCurve;

        public Dictionary<string, LocationData> InstanceLocationCurveDictionary
        {
            get { return m_dictLocationCurve; }
            set
            {
                m_dictLocationCurve = value;
                foreach (LocationData data in m_dictLocationCurve.Values)
                    data.LocationCurve.ConvertToMM();

                m_dictLocationCurve = value;
            }
        }

        public ConvertEntity()
        {
            vDraw.Progress += vDraw_Progress;
        }

        void vDraw_Progress(object sender, long percent, string jobDescription)
        {
            Application.DoEvents();
            if (m_formProgress != null && !m_formProgress.IsDisposed)
                m_formProgress.SetProgress((int)percent);
        }


        public void BeginConvert(string fileName)
        {
            if (ExportSetting.SystemSetting.IsUserDefineFormat)
            {
                try
                {
                    if (ExportSetting.SystemSetting.IsExportTextureFile)
                        ProcessMaterialMapFile();

                    var ser = new ModelSerializeEntity();
                    ser.Blocks = DictBlocks;
                    ser.Materials = Materials;
                    ser.Levels = Levels;

                    if (File.Exists(fileName))
                        File.Delete(fileName);

                    using (Stream s = File.OpenWrite(fileName))
                    {
                        ProtoBuf.Serializer.Serialize<ModelSerializeEntity>(s, ser);
                    }

                    //var str = JsonConvert.SerializeObject(ser);
                    //using (var objWriter = new StreamWriter(ExportSetting.SystemSetting.ExportFilePath))
                    //    objWriter.Write(str);

                    MessageBox.Show("导出完成");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导出失败！" + ex.Message);
                }
            }
            else
            {
                InitRenderProperty();
                InitMaterialLayers();
                InitEntityInModelBlock();
                AddGridPolyline();
                AddLevelData();

                if (m_formProgress == null || m_formProgress.IsDisposed)
                    m_formProgress = new FormProgress();

                m_formProgress.Show(this.WndParent);

                if (m_OptimizeTriangle)
                {
                    m_formProgress.Text = "正在优化数据...";
                    ReduceTris.CRevit2vdl redTri = new ReduceTris.CRevit2vdl();
                    redTri.FamilySketchDictionary = this.FamilySketchDictionary;
                    redTri.InstanceLocationDictionary = this.InstanceLocationCurveDictionary;
                    redTri.Convert(vDraw.ActiveDocument, m_formProgress.progressBar, fileName);
                }

                if (this.ExportSetting != null && this.ExportSetting.ParkingExportSetting.PropertyName.Length > 0)
                {
                    /*
                    ReplaceParkingNumber replaceNumber = new ReplaceParkingNumber(vDraw);
                    replaceNumber.Setting = this.ExportSetting.ParkingExportSetting;
                    replaceNumber.ReplaceText();
                    */
                }

                vDraw.ActiveDocument.ActiveLayOut.Entities.Sort(new CompareMethod());

                m_formProgress.Text = "正在保存";
                bool bResult = vDraw.ActiveDocument.SaveAs(fileName);
                m_formProgress.Close();

                MessageBox.Show(bResult ? "导出完成！" : "导出失败！");
            }
        }

        private void AddLevelData()
        {
            if (Levels == null)
                return;

            vdLayer layer = vDraw.ActiveDocument.Layers.FindName(_gGridLayerName);
            if (layer == null)
            {
                layer = new vdLayer(vDraw.ActiveDocument, _gGridLayerName);
                vDraw.ActiveDocument.Layers.Add(layer);
            }

            foreach (var lv in Levels)
            {
                vdXProperty vdx1 = new vdXProperty();
                vdx1.Name = lv.Name;
                vdx1.PropValue = (Tools.Ft2MmScale*lv.Height).ToString();
                layer.XProperties.AddItem(vdx1);
            }
        }

        private void ProcessMaterialMapFile()
        {
            var dirPath = Path.GetDirectoryName(ExportSetting.SystemSetting.ExportFilePath);
            var fileName = Path.GetFileNameWithoutExtension(ExportSetting.SystemSetting.ExportFilePath);
            var dirRes = dirPath + "\\" + fileName + "_texture";

            try
            {
                if (!Directory.Exists(dirRes))
                    Directory.CreateDirectory(dirRes);
            }
            catch (Exception ex)
            {
                MessageBox.Show("创建文件夹失败。材质贴图文件无法导出。" + ex.Message);
                return;
            }

            foreach (var mtl in Materials)
            {
                var diffuseMapFile = ProcessMapFile(mtl.DiffuseMap, dirRes);
                if (string.IsNullOrEmpty(diffuseMapFile))
                    mtl.DiffuseMap = string.Empty;
                else
                    mtl.DiffuseMap = Path.GetFileName(diffuseMapFile);

                var bumpMapFile = ProcessMapFile(mtl.BumpMap, dirRes);
                if (string.IsNullOrEmpty(bumpMapFile))
                    mtl.BumpMap = string.Empty;
                else
                    mtl.BumpMap = Path.GetFileName(bumpMapFile);
            }

        }

        private string ProcessMapFile(string fileString, string destFilePath)
        {
            try
            {
                var filePathName = string.Empty;
                if (string.IsNullOrEmpty(fileString))
                    return string.Empty;

                if (fileString.Contains('|'))
                {
                    var paths = fileString.Split('|');
                    if (paths.Length < 1)
                        return string.Empty;

                    fileString = paths.Last();
                }

                var fileName = Path.GetFileName(fileString);

                if (fileString.Contains(':'))
                {
                    if (File.Exists(fileString))
                        filePathName = fileString;
                }
                else
                {
                    filePathName = GetRelateFilePath(fileName);
                }

                if (string.IsNullOrEmpty(filePathName))
                    return string.Empty;


                var fileDestName = destFilePath + "\\" + fileName;
                if (!File.Exists(fileDestName))
                {
                    File.Copy(filePathName, fileDestName, true);
                }

                return fileDestName;
            }
            catch (Exception)
            {
                MessageBox.Show("获取贴图文件");
                return string.Empty;
            }
        }

        private string GetRelateFilePath(string fileName)
        {
            foreach (var pa in Tools.TextureLibPaths)
            {
                var path = pa + "\\" + fileName;
                if (File.Exists(path))
                    return path;
            }

            return string.Empty;
        }

        private void InitRenderProperty()
        {
            vDraw.EnsureDocument();
            vDraw.ActiveDocument.EnsureDefaults();
            vDraw.ActiveDocument.GlobalRenderProperties.TimerBreakForDraw = 300;
            vDraw.ActiveDocument.FocalLength = 100.0;
            vDraw.ActiveDocument.GlobalRenderProperties.CustomRenderTypeName = "VectorDraw.Render.opengllist#VectorDraw.Professional.dll";
            vDraw.ActiveDocument.ActiveLayOut.RenderMode = VectorDraw.Render.vdRender.Mode.Shade;
        }

        private void InitMaterialLayers()
        {
            foreach (MaterialData material in Materials)
            {
                vdColor color = new vdColor();
                color.Red = material.Color.Red;
                color.Green = material.Color.Green;
                color.Blue = material.Color.Blue;
                color.AlphaBlending = (byte)(255.0 * (1 - material.Transparency / 100.0));
                vdLayer layer = new vdLayer(vDraw.ActiveDocument, material.Name, color);
                vDraw.ActiveDocument.Layers.AddItem(layer);
            }
        }

        private void InitEntityInModelBlock()
        {
            if (ModelBlock == null)
                return;

            foreach (MeshData mesh in ModelBlock.Meshs)
            {
                vdPolyface vdp = AddMeshToEntities(vDraw.ActiveDocument.ActiveLayOut.Entities, mesh);
                if (vdp != null && ExportSetting.SystemSetting.DefaultLayerName.Length > 1)
                    SetEntityLayer(vdp, ExportSetting.SystemSetting.DefaultLayerName);
            }

            foreach (InsertData ins in ModelBlock.Inserts)
            {
                vdInsert vdi = AddInsertToEntities(vDraw.ActiveDocument.ActiveLayOut.Entities, ins);
                if (vdi != null && ExportSetting.SystemSetting.DefaultLayerName.Length > 1)
                    SetEntityLayer(vdi, ExportSetting.SystemSetting.DefaultLayerName);
            }

            vDraw.ActiveDocument.CommandAction.Zoom("E", null, null);
        }

        private void SetEntityLayer(vdFigure vdf, string layerName)
        {
            vdLayer layer = vDraw.ActiveDocument.Layers.FindName(layerName);
            if (layer == null)
            {
                layer = new vdLayer(vDraw.ActiveDocument, layerName);
                vDraw.ActiveDocument.Layers.Add(layer);
            }

            vdf.Layer = layer;
        }

        private vdBlock CreateBlock(BlockData blkData)
        {
            vdBlock block = vDraw.ActiveDocument.Blocks.FindName(blkData.Name);
            if (block != null)
            {
                MessageBox.Show("存在同名块: " + blkData.Name + " 跳过！");
                return null;
            }

            block = vDraw.ActiveDocument.Blocks.Add(blkData.Name);
            if (block == null)
            {
                MessageBox.Show("添加块: " + blkData.Name + " 失败！ 跳过！");
                return null;
            }

            foreach (MeshData mesh in blkData.Meshs)
                AddMeshToEntities(block.Entities, mesh);

            foreach (InsertData ins in blkData.Inserts)
                AddInsertToEntities(block.Entities, ins);

            if (blkData.IsPipe)
            {
                var ptStart = new gPoint(blkData.PipeInfo.PtStart.X, blkData.PipeInfo.PtStart.Y, blkData.PipeInfo.PtStart.Z);
                var ptEnd = new gPoint(blkData.PipeInfo.PtEnd.X, blkData.PipeInfo.PtEnd.Y, blkData.PipeInfo.PtEnd.Z);
                AddPipeToEntities(block.Entities, blkData.PipeInfo.Diameter, ptStart, ptEnd, blkData.PipeInfo.MaterialName);
            }

            block.Update();

            return block;
        }

        private void AddPipeToEntities(vdEntities entities, double diameter, gPoint ptStart, gPoint ptEnd, string layerName)
        {
            var pf = new vdPolyface();
            pf.SetUnRegisterDocument(vDraw.ActiveDocument);
            pf.setDocumentDefaults();

            var line = new vdLine(ptStart * Tools.Ft2MmScale, ptEnd * Tools.Ft2MmScale);
            var circle = new vdCircle();
            circle.Radius = diameter * Tools.Ft2MmScale / 2.0;

            pf.Generate3dPathSection(line, circle, new gPoint(0, 0, 0), 10, 1);
            pf.Layer = vDraw.ActiveDocument.Layers.FindName(layerName);
            pf.SmoothAngle = 45;
            entities.Add(pf);
        }

        private Matrix ChangeTransMatrix(TransformData trans)
        {
            Matrix mtx = new Matrix();

            mtx.A00 = trans.BasisX.X;
            mtx.A10 = trans.BasisX.Y;
            mtx.A20 = trans.BasisX.Z;

            mtx.A01 = trans.BasisY.X;
            mtx.A11 = trans.BasisY.Y;
            mtx.A21 = trans.BasisY.Z;

            mtx.A02 = trans.BasisZ.X;
            mtx.A12 = trans.BasisZ.Y;
            mtx.A22 = trans.BasisZ.Z;

            mtx.A03 = trans.Origin.X;
            mtx.A13 = trans.Origin.Y;
            mtx.A23 = trans.Origin.Z;

            mtx.A30 = 0;
            mtx.A31 = 0;
            mtx.A32 = 0;
            mtx.A33 = 1;

            return mtx;
        }

        private vdInsert AddInsertToEntities(vdEntities entities, InsertData ins)
        {
            //var blk = vDraw.ActiveDocument.Blocks.FindName(ins.BlockRef.Name) ?? CreateBlock(ins.BlockRef);
            var blk = vDraw.ActiveDocument.Blocks.FindName(ins.BlockName) ?? CreateBlock(DictBlocks[ins.BlockName]);
            if (blk == null)
            {
                MessageBox.Show("块创建失败！");
                return null;
            }

            vdInsert insEntity = new vdInsert(vDraw.ActiveDocument);
            insEntity.Block = blk;
            AddXProperties(insEntity, ins);

            Matrix mtx = ChangeTransMatrix(ins.TransMatrix);
            insEntity.Transformby(mtx);
            insEntity.InsertionPoint *= Tools.Ft2MmScale;
            entities.Add(insEntity);
            return insEntity;
        }

        private void AddXProperties(vdInsert insEntity, InsertData ins)
        {
            if (ins.DictProperties.Count < 1)
                return;

            AddXPropertiesToEntity(ins.DictProperties, insEntity);
        }

        private void AddXPropertiesToEntity(Dictionary<string, List<PropertyData>> dictProperty, vdFigure vdf)
        {

            foreach (string groupName in dictProperty.Keys)
            {
                List<PropertyData> listData = dictProperty[groupName];

                vdXProperty vdx = new vdXProperty();
                vdx.Name = groupName;
                vdx.PropValue = "--------";
                vdf.XProperties.AddItem(vdx);

                foreach (PropertyData prop in listData)
                {
                    vdXProperty vdx1 = new vdXProperty();
                    vdx1.Name = prop.Name;
                    vdx1.PropValue = prop.Value ?? string.Empty;
                    vdf.XProperties.AddItem(vdx1);
                }
            }
        }

        private vdPolyface AddMeshToEntities(vdEntities entities, MeshData mesh)
        {
            vdPolyface onepolyface = new vdPolyface();
            onepolyface.SetUnRegisterDocument(vDraw.ActiveDocument);
            onepolyface.setDocumentDefaults();

            // 顶点数组输入
            foreach (PointData point in mesh.Vertexes)
                onepolyface.VertexList.Add(point.X * Tools.Ft2MmScale, point.Y * Tools.Ft2MmScale, point.Z * Tools.Ft2MmScale);

            // 面片索引输入
            foreach (TriangleIndexData index in mesh.TriangleIndexes)
                onepolyface.AddFaceItem(index.V1 + 1, index.V2 + 1, index.V3 + 1, index.V1 + 1, -1);

            onepolyface.SmoothAngle = 45;
            onepolyface.Layer = vDraw.ActiveDocument.Layers.FindName(mesh.MaterialName);
            entities.Add(onepolyface);
            onepolyface.Invalidate();
            onepolyface.Update();

            return onepolyface;
        }

        private void AddGridPolyline()
        {
            if (Grids == null)
                return;

            foreach (var grid in Grids)
            {
                vdPolyline pl = new vdPolyline();
                pl.SetUnRegisterDocument(vDraw.ActiveDocument);
                pl.setDocumentDefaults();
                pl.VertexList = GetVertexesFromCurves(grid.Curves);

                vDraw.ActiveDocument.ActiveLayOut.Entities.Add(pl);
                SetEntityLayer(pl, _gGridLayerName);
                AddXPropertiesToEntity(grid.Properties, pl);
                AddGridText(pl.getStartPoint(), grid.Name, grid);
                AddGridText(pl.getEndPoint(), grid.Name, grid);
            }
        }

        private void AddGridText(gPoint position, string name, GridData grid)
        {
            vdText txt = new vdText();
            txt.SetUnRegisterDocument(vDraw.ActiveDocument);
            txt.setDocumentDefaults();
            txt.InsertionPoint = position;
            txt.TextString = name;
            txt.Height = 500;
            txt.AlignToView = true;
            txt.PenColor.ColorIndex = 2;

            vDraw.ActiveDocument.ActiveLayOut.Entities.Add(txt);
            SetEntityLayer(txt, _gGridLayerName);
            AddXPropertiesToEntity(grid.Properties, txt);
        }

        /*
            Revit中导出的多段轴网信息很奇怪，首先，对于直线段，直线的起始、终止点和点击时的顺序是相反的，但是多段线的顺序是正确的，所以整个
            排序就变成了“终点”-“起点”-“终点”-“起点”的方式，所以在导出的时候，程序处理时反着顺序导出，就可以正常得到“起点”-“终点”
            的顺序了。其次，如果遇上了圆弧段，圆弧段的起点、终点的顺序是和点击的顺序是一致的，所以遇到圆弧段，输出的起点和终点交换，这样就
            可以正确输出了。下面的两个方法就是用来处理这个。
        */

        private Vertexes GetVertexesFromCurves(List<CurveData> curves)
        {
            Vertexes vs = new Vertexes();
            for (int i = curves.Count - 1; i >= 0; i--)
            {
                Vertex vStart, vEnd;
                GetVertexFromSingleCurve(curves[i], out vStart, out vEnd);

                vs.Add(vStart);
                if (i == 0)
                    vs.Add(vEnd);
            }

            return vs;
        }

        private void GetVertexFromSingleCurve(CurveData curve, out Vertex vStart, out Vertex vEnd)
        {
            vStart = new Vertex(curve.Points[0].X, curve.Points[0].Y, curve.Points[0].Z);
            vEnd = new Vertex(curve.Points[1].X, curve.Points[1].Y, curve.Points[1].Z);

            if (curve.IsArc)
            {
                var vtmp = vStart;
                vStart = vEnd;
                vEnd = vtmp;

                int dDirection = curve.Normal.Z > 0 ? 1 : -1;
                double angle = curve.StartParameter - curve.EndParameter;

                // 凸度的定义是：圆弧段圆心角四分之一的正切值。正负决定圆弧方向
                vStart.Bulge = dDirection * Math.Tan(angle / 4);
            }
        }
    }
}
