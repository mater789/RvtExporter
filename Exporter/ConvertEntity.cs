using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using VectorDraw.Professional.vdFigures;
using VectorDraw.Professional.vdPrimaries;
using VectorDraw.Professional.vdCollections;
using VectorDraw.Geometry;
//using Autodesk.Revit.DB;
using VectorDraw.Professional.vdObjects;
using Newtonsoft.Json;
using System.Drawing;

namespace Exporter
{
    public class ConvertEntity
    {
        private VectorDraw.Professional.Control.VectorDrawBaseControl vDraw = new VectorDraw.Professional.Control.VectorDrawBaseControl();
        private FormProgress m_formProgress = null;
        private bool IsZipFile = false;

        private static string _gGridLayerName = "GRID__";
        private static string _gPropertyGroupSeparator = "--------";

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

        public List<RebarData> Rebars { get; set; }

        public List<LevelData> Levels { get; set; }

        public Dictionary<string, BlockData> DictBlocks { get; set; }

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
        }

        void vDraw_Progress(object sender, long percent, string jobDescription)
        {
            Application.DoEvents();
            if (m_formProgress != null && !m_formProgress.IsDisposed)
                m_formProgress.SetProgress((int)percent);
        }

        public void BeginConvert()
        {
            if (ExportSetting.SystemSetting.IsExportTextureFile)
                ProcessMaterialMapFile();

            // 特别指非bim格式
            if (ExportSetting.SystemSetting.IsUserDefineFormat)
            {

                var ser = new ModelSerializeEntity
                {
                    Blocks = DictBlocks,
                    Materials = Materials,
                    Levels = Levels
                };

                if (File.Exists(ExportSetting.SystemSetting.ExportFilePath))
                    File.Delete(ExportSetting.SystemSetting.ExportFilePath);


                if (ExportSetting.SystemSetting.FileType == SystemSetting.FileTypeEnum.bfa)
                {
                    var str = JsonConvert.SerializeObject(ser);
                    File.WriteAllText(ExportSetting.SystemSetting.ExportFilePath, str);
                }
                else
                {
                    using (FileStream fs = File.OpenWrite(ExportSetting.SystemSetting.ExportFilePath))
                    {
                        if (IsZipFile)
                        {
                            MemoryStream ms = new MemoryStream();
                            ProtoBuf.Serializer.Serialize(ms, ser);

                            var buffer = ms.ToArray();
                            var compressedzipStream = new DeflateStream(fs, CompressionMode.Compress, true);
                            compressedzipStream.Write(buffer, 0, buffer.Length);
                            compressedzipStream.Close();
                        }
                        else
                        {
                            ProtoBuf.Serializer.Serialize(fs, ser);
                        }
                    }
                }
            }
            // 指bim格式
            else
            {
                InitRenderProperty();

                if (m_formProgress == null || m_formProgress.IsDisposed)
                    m_formProgress = new FormProgress();

                m_formProgress.Show(this.WndParent);

                if (this.ExportSetting.SystemSetting.IsExportRebar)
                {
                    AddRebarData();
                }
                else
                {
                    InitMaterialLayersAandImages();
                    InitEntityInModelBlock();
                    AddGridPolyline();
                    AddLevelData();

                    if (ExportSetting.SystemSetting.IsOptimizeCylinderFace)
                    {
                        m_formProgress.Text = "正在优化数据...";
                        ReduceTris.CRevit2vdl redTri = new ReduceTris.CRevit2vdl
                        {
                            FamilySketchDictionary = this.FamilySketchDictionary,
                            InstanceLocationDictionary = this.InstanceLocationCurveDictionary
                        };
                        redTri.Convert(vDraw.ActiveDocument, m_formProgress.progressBar);
                    }

                    if (this.ExportSetting.SystemSetting.IsMergeFace)
                    {
                        MergeFace.Merge(vDraw, m_formProgress);
                    }

                    if (this.ExportSetting != null && this.ExportSetting.ParkingExportSetting.PropertyName.Length > 0)
                    {
                        ReplaceParkingNumber replaceNumber = new ReplaceParkingNumber(vDraw);
                        replaceNumber.Setting = this.ExportSetting.ParkingExportSetting;
                        replaceNumber.ReplaceText();
                    }
                }

                vDraw.ActiveDocument.ActiveLayOut.Entities.Sort(new CompareMethod());

                if (this.ExportSetting.SystemSetting.IsMoveBlkXpropertyToInsert)
                    MoveProperty();

                RemoveEmptyFigure();

                vDraw.Progress += vDraw_Progress;
                m_formProgress.Text = "正在保存";
                bool bResult = vDraw.ActiveDocument.SaveAs(ExportSetting.SystemSetting.ExportFilePath);
                m_formProgress?.Close();
                vDraw.Progress -= vDraw_Progress;
            }
        }

        private void RemoveEmptyFigure()
        {
            (from vdFigure vdf in vDraw.ActiveDocument.ActiveLayOut.Entities
             where vdf.BoundingBox == null || vdf.BoundingBox.IsEmpty
             select vdf)
            .ToList()
            .ForEach(f =>
            {
                vDraw.ActiveDocument.ActiveLayOut.Entities.RemoveItem(f);
                f.Deleted = true;
            });
        }

        private void MoveProperty()
        {
            List<string> list1 = new List<string>
            {
                "工序编码",
                "合约类型编码1",
                "合约类型编码2",
                "合约类型代码1",
                "合约类型代码2",
                "清单编码",
                //"结构材质",
                "类型名称"
            };


            foreach (vdFigure vdl in this.vDraw.ActiveDocument.ActiveLayOut.Entities)
            {
                if (vdl is vdInsert)
                {
                    vdInsert vdi = vdl as vdInsert;
                    vdBlock vdb = vdi.Block;
                    foreach (string str in list1)
                    {
                        var prop = GetFigurePropByName(vdb, str);
                        if (prop != null)
                        {
                            vdi.Props += str + "=" + prop + "\r\n";
                        }
                    }
                    vdb.Props = string.Empty;
                }
            }
        }

        private string GetFigurePropByName(vdPrimary vdp, string propName)
        {
            if (vdp == null)
                return null;

            if (string.IsNullOrEmpty(vdp.Props))
                return null;

            var propStrings = vdp.Props.Split(new char[2] { '\r', '\n' });
            foreach (var propStr in propStrings)
            {
                if (string.IsNullOrEmpty(propStr))
                    continue;

                var index = propStr.IndexOf('=');
                if (index < 0)
                    continue;

                var name = propStr.Substring(0, index);
                var prop = propStr.Substring(index + 1, propStr.Length - index - 1);

                if (string.Compare(propName, name) == 0)
                    return prop;
            }

            return null;
        }

        private void AddRebarData()
        {
            if (Rebars == null)
                return;

            foreach (RebarData rd in Rebars)
            {
                rd.ConvertToMM();

                var pl = CreatePolylineFromPoints(rd.CurvePoints);
                pl.PenColor.ColorIndex = 1;
                SetEntityLayer(pl, "Rebar_CenterLine");
                var pf = CreatePolyFaceFromCurve(pl, 20);
                SetEntityLayer(pf, "Rebar");

                if (m_formProgress != null && !m_formProgress.IsDisposed)
                {
                    int cur = Rebars.IndexOf(rd) + 1;
                    int total = Rebars.Count;

                    if (cur % 100 == 0)
                    {
                        double percent = 1.0 * cur / total;
                        m_formProgress.Text = Rebars.IndexOf(rd).ToString() + "/" + Rebars.Count.ToString();
                        m_formProgress.SetProgress((int)(percent * 100));
                    }
                }

                if (rd.RepeatDistance > 0 && rd.RepeatCount > 1)
                {
                    for (int i = 0; i < rd.RepeatCount - 1; i++)
                    {
                        Matrix mtx = new Matrix();
                        mtx.TranslateMatrix(new gPoint(rd.DistributionVector.X, rd.DistributionVector.Y, rd.DistributionVector.Z) * rd.RepeatDistance * (i + 1));

                        var plClone = pl.Clone(vDraw.ActiveDocument) as vdPolyline;
                        plClone.Transformby(mtx);
                        vDraw.ActiveDocument.ActiveLayOut.Entities.Add(plClone);

                        var pfCl = CreatePolyFaceFromCurve(plClone, 20);
                        SetEntityLayer(pfCl, "Rebar");
                    }
                }
            }
        }

        private vdPolyface CreatePolyFaceFromCurve(vdPolyline curvePath, double diameter)
        {
            var pf = new vdPolyface();
            pf.SetUnRegisterDocument(vDraw.ActiveDocument);
            pf.setDocumentDefaults();

            var circle = new vdCircle { Radius = diameter / 2.0 };
            
            vdCurve path = curvePath;
            if (curvePath.VertexList.Count == 2)
            {
                var line = new vdLine();
                line.SetUnRegisterDocument(vDraw.ActiveDocument);
                line.setDocumentDefaults();
                line.StartPoint = curvePath.getStartPoint();
                line.EndPoint = curvePath.getEndPoint();
                path = line;
            }

            pf.Generate3dPathSection(path, circle, new gPoint(0, 0, 0), 6, 1);
            pf.SmoothAngle = 45;
            pf.PenColor.ColorIndex = 2;
            vDraw.ActiveDocument.ActiveLayOut.Entities.Add(pf);

            return pf;
        }

        private vdPolyline CreatePolylineFromPoints(List<PointData> points)
        {
            vdPolyline pl = new vdPolyline();
            pl.SetUnRegisterDocument(vDraw.ActiveDocument);
            pl.setDocumentDefaults();

            pl.VertexList = Tools.GetVertexesFromPoints(points);
            vDraw.ActiveDocument.ActiveLayOut.Entities.Add(pl);

            return pl;
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
                vdXProperty vdx1 = new vdXProperty
                {
                    Name = lv.Name,
                    PropValue = (Tools.Ft2MmScale * lv.Height).ToString()
                };
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
                throw new Exception("创建文件夹失败。材质贴图文件无法导出。", ex);
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
            vDraw.ActiveDocument.GlobalRenderProperties.CustomRenderType = typeof(VectorDraw.Render.opengllist);
            vDraw.ActiveDocument.ActiveLayOut.RenderMode = VectorDraw.Render.vdRender.Mode.Shade;
        }

        private void InitMaterialLayersAandImages()
        {
            foreach (MaterialData material in Materials)
            {
                vdColor color = new vdColor
                {
                    Red = material.Color.Red,
                    Green = material.Color.Green,
                    Blue = material.Color.Blue,
                    AlphaBlending = (byte)(255.0 * (1 - material.Transparency / 100.0))
                };
                vdLayer layer = new vdLayer(vDraw.ActiveDocument, material.Name, color);
                vDraw.ActiveDocument.Layers.AddItem(layer);

                if (string.IsNullOrEmpty(material.GetValidMapFile()))
                    continue;

                var fileName = Path.GetFileNameWithoutExtension(ExportSetting.SystemSetting.ExportFilePath);
                var folderName = Path.GetDirectoryName(ExportSetting.SystemSetting.ExportFilePath);
                var imageFilePath = folderName + "\\" + fileName + "_texture\\" + material.GetValidMapFile();
                AddImage(imageFilePath);
            }
        }

        private vdImageDef AddImage(string imageFilePath)
        {
            if (!File.Exists(imageFilePath))
                return null;

            var imageName = Path.GetFileNameWithoutExtension(imageFilePath);
            var imgDef = vDraw.ActiveDocument.Images.FindName(imageName);
            if (imgDef == null)
            {
                var img = Image.FromFile(imageFilePath);
                if (img == null)
                    return null;

                imgDef = new vdImageDef(vDraw.ActiveDocument, imageName);
                vDraw.ActiveDocument.Images.AddItem(imgDef);
                imgDef.Image.SelectImage(img);
                imgDef.EmbedImage();
                imgDef.Update();
            }

            return imgDef;
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
                //MessageBox.Show("存在同名块: " + blkData.Name + " 跳过！");
                return null;
            }

            block = vDraw.ActiveDocument.Blocks.Add(blkData.Name);
            if (block == null)
            {
                //MessageBox.Show("添加块: " + blkData.Name + " 失败！ 跳过！");
                return null;
            }

            foreach (MeshData mesh in blkData.Meshs)
                AddMeshToEntities(block.Entities, mesh);

            foreach (InsertData ins in blkData.Inserts)
                AddInsertToEntities(block.Entities, ins);

            if (blkData.IsPipe)
            {
                var ptStart = new gPoint(blkData.PipeInfo.PtStart.X, blkData.PipeInfo.PtStart.Y, blkData.PipeInfo.PtStart.Z);
                ptStart = ptStart * Tools.Ft2MmScale;
                var ptEnd = new gPoint(blkData.PipeInfo.PtEnd.X, blkData.PipeInfo.PtEnd.Y, blkData.PipeInfo.PtEnd.Z);
                ptEnd = ptEnd * Tools.Ft2MmScale;
                AddPipeToEntities(block.Entities, blkData.PipeInfo.Diameter * Tools.Ft2MmScale, ptStart, ptEnd, blkData.PipeInfo.MaterialName);

                if (!blkData.DictProperties.ContainsKey("PipeInfo"))
                {
                    var pipeProps = new List<PropertyData>
                    {
                        new PropertyData { GroupName = "PipeInfo", Name = "PipeStartPoint", Value = ptStart.ToSplitString() },
                        new PropertyData { GroupName = "PipeInfo", Name = "PipeEndPoint", Value = ptEnd.ToSplitString() },
                        new PropertyData { GroupName = "PipeInfo", Name = "PipeDiameter", Value = (blkData.PipeInfo.Diameter * Tools.Ft2MmScale).ToString() }
                    };

                    blkData.DictProperties.Add("PipeInfo", pipeProps);
                }
            }

            AddXPropertiesToEntity(blkData.DictProperties, block);

            block.Update();

            return block;
        }

        private void AddPipeToEntities(vdEntities entities, double diameter, gPoint ptStart, gPoint ptEnd, string layerName)
        {
            var pf = new vdPolyface();
            pf.SetUnRegisterDocument(vDraw.ActiveDocument);
            pf.setDocumentDefaults();

            var line = new vdLine(ptStart, ptEnd);
            var circle = new vdCircle
            {
                Radius = diameter / 2.0
            };

            pf.Generate3dPathSection(line, circle, new gPoint(0, 0, 0), 6, 1);
            pf.Layer = vDraw.ActiveDocument.Layers.FindName(layerName);
            pf.SmoothAngle = 45;
            entities.Add(pf);
        }

        private Matrix ChangeTransMatrix(TransformData trans)
        {
            Matrix mtx = new Matrix
            {
                A00 = trans.BasisX.X,
                A10 = trans.BasisX.Y,
                A20 = trans.BasisX.Z,

                A01 = trans.BasisY.X,
                A11 = trans.BasisY.Y,
                A21 = trans.BasisY.Z,

                A02 = trans.BasisZ.X,
                A12 = trans.BasisZ.Y,
                A22 = trans.BasisZ.Z,

                A03 = trans.Origin.X,
                A13 = trans.Origin.Y,
                A23 = trans.Origin.Z,

                A30 = 0,
                A31 = 0,
                A32 = 0,
                A33 = 1
            };

            return mtx;
        }

        private vdInsert AddInsertToEntities(vdEntities entities, InsertData ins)
        {
            //var blk = vDraw.ActiveDocument.Blocks.FindName(ins.BlockRef.Name) ?? CreateBlock(ins.BlockRef);
            var blk = vDraw.ActiveDocument.Blocks.FindName(ins.BlockName) ?? CreateBlock(DictBlocks[ins.BlockName]);
            if (blk == null)
            {
                //MessageBox.Show("块创建失败！");
                return null;
            }

            vdInsert insEntity = new vdInsert(vDraw.ActiveDocument) { Block = blk };
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

        private void AddXPropertiesToEntity(Dictionary<string, List<PropertyData>> dictProperty, vdPrimary vdf)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string groupName in dictProperty.Keys)
            {
                List<PropertyData> listData = dictProperty[groupName];
                sb.Append(groupName + "=" + _gPropertyGroupSeparator + "\r\n");

                foreach (PropertyData prop in listData)
                {
                    var propValue = string.IsNullOrEmpty(prop.Value) ? "" : prop.Value;
                    sb.Append(prop.Name + "=" + propValue + "\r\n");
                }
            }

            vdf.Props = sb.ToString();
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

            if (ExportSetting.SystemSetting.IsExportTextureFile)
            {
                var material = Materials.Find(m => m.Name == mesh.MaterialName);
                if (material != null && !string.IsNullOrEmpty(material.GetValidMapFile()))
                {
                    var img = vDraw.ActiveDocument.Images.FindName(Path.GetFileNameWithoutExtension(material.GetValidMapFile()));
                    if (img != null)
                    {
                        onepolyface.PenColor.SystemColor = Color.Gray;
                        onepolyface.PenColor.MaterialImage = img;

                        Matrix mtx = new Matrix();
                        mtx.IdentityMatrix();
                        mtx.ScaleMatrix(0.001, 0.001, 1);
                        onepolyface.PenColor.MaterialMatrix = mtx;
                    }
                }
            }

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
                pl.VertexList = Tools.GetVertexesFromCurves(grid.Curves);

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
    }
}
