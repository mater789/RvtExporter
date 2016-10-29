using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Diagnostics;
using SmartDP.Collaboration.Revit;

namespace Exporter
{
    [Transaction(TransactionMode.Manual)]
    public class ExportCommand : IExternalCommand, IExternalCommandAvailability
    {
        public bool IsCommandAvailable(Autodesk.Revit.UI.UIApplication applicationData, Autodesk.Revit.DB.CategorySet selectedCategories)
        {
            return true;
            //return RevitApp.Instance.IsLogin;
        }

        public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            if (uidoc == null)
            {
                MessageBox.Show("当前没有打开的Revit文档！");
                return Result.Cancelled;
            }

            Document doc = uidoc.Document;
            if (!(uidoc.ActiveView is View3D))
            {
                MessageBox.Show("请在3D视图下使用此命令！");
                return Result.Cancelled;
            }

            //var dicProfileData = GetProfileDictFromDocument(doc);

            Process process = Process.GetCurrentProcess();
            IntPtr h = process.MainWindowHandle;

            string filePath = System.IO.Path.GetDirectoryName(doc.PathName);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(doc.PathName);

            FormSettings dlg = new FormSettings();
            dlg.ExportSetting.SystemSetting.ExportFilePath = filePath + "\\" + fileName + ".vdcl";
            DialogResult dr = dlg.ShowDialog(new WindowHandle(h));
            if (dr != DialogResult.OK)
                return Result.Cancelled;

            ElementColorOverride colorOverride = new ElementColorOverride();
            if (!dlg.ExportSetting.SystemSetting.IsOriginMaterial)
                colorOverride.ArrangeElemlentColor(doc, uidoc.ActiveView as View3D);

            RevitEnvironmentSetting setting = new RevitEnvironmentSetting(doc);
            if (dlg.ExportSetting.SystemSetting.IsModifyUnit)               
                setting.ReadOriginUnitsAndSetNew();

            ModelExportContext context = new ModelExportContext(doc);
            context.IsPackageEntityToBlock = true;
            context.IsExportProperty = dlg.ExportSetting.SystemSetting.IsExportProperty;
            context.ExtraMaterial = colorOverride.GetMaterials();
            context.ExtraElementColorSetting = colorOverride.GetElementColorSetting();
            CustomExporter exporter = new CustomExporter(doc, context);

            exporter.IncludeFaces = false;
            exporter.ShouldStopOnError = false;
            exporter.Export(doc.ActiveView as View3D);

            if (dlg.ExportSetting.SystemSetting.IsModifyUnit)
                setting.RecoverOriginUnits();

            ConvertEntity converter = new ConvertEntity();
            converter.OptimizeTriangle = dlg.ExportSetting.SystemSetting.IsOptimizeCylinderFace;
            converter.ExportSetting = dlg.ExportSetting;
            converter.Materials = context.Materials;
            converter.ModelBlock = context.ModelSpaceBlock;
            converter.WndParent = new WindowHandle(h);
            if (dlg.ExportSetting.SystemSetting.IsExportGrid)
                converter.Grids = GetGridFromDocument(doc);
            //converter.FamilySketchDictionary = dicProfileData;
            converter.InstanceLocationCurveDictionary = context.InstanceLocation;
            converter.BeginConvert(dlg.ExportSetting.SystemSetting.ExportFilePath);
                      
            return Result.Succeeded;
        }

        /// <summary>
        /// 获取文档中所有可读族的拉伸体信息
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private Dictionary<string, List<SketchData>> GetProfileDictFromDocument(Document doc)
        {
            Dictionary<string, List<SketchData>> dictFamilyProfile = new Dictionary<string, List<SketchData>>();

            FilteredElementCollector elemTypeCtor = (new FilteredElementCollector(doc)).WhereElementIsNotElementType();
            var elems = elemTypeCtor.ToElements();
            foreach (var elem in elems)
            {
                var fml = elem as Family;
                if (fml != null)
                {
                    try
                    {
                        var fmldoc = doc.EditFamily(fml);
                        if (fmldoc != null)
                        {
                            var fec = (new FilteredElementCollector(fmldoc)).WhereElementIsNotElementType();
                            var tmpElems = fec.ToElements();
                            SketchData sketchData = null;
                            foreach (var tmpElem in tmpElems)
                            {
                                if (tmpElem is GenericForm)
                                {
                                    if (tmpElem is Sweep)
                                        sketchData = GetDataFromProfile((tmpElem as Sweep).ProfileSketch);
                                    else if (tmpElem is Extrusion)
                                        sketchData = GetDataFromProfile((tmpElem as Extrusion).Sketch);
                                }
                            }

                            if (sketchData != null)
                            {
                                if (dictFamilyProfile.ContainsKey(fml.Name))
                                    dictFamilyProfile[fml.Name].Add(sketchData);
                                else
                                    dictFamilyProfile[fml.Name] = new List<SketchData> { sketchData };
                            }

                            fmldoc.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            return dictFamilyProfile;
        }

        /// <summary>
        /// 获取指定拉伸体的拉伸面信息
        /// </summary>
        /// <param name="skt"></param>
        /// <returns></returns>
        private SketchData GetDataFromProfile(Sketch skt)
        {
            if (skt == null)
                return null;

            var sketchData = new SketchData();
            foreach (object curArr in skt.Profile)
            {
                if (curArr is CurveArray)
                {
                    foreach (var cur in (CurveArray) curArr)
                    {
                        var curve = (Curve) cur;
                        if (curve != null && curve.IsBound)
                        {
                            CurveData curvedata = new CurveData();

                            if (curve is Arc)
                            {
                                var arc = (Arc) curve;
                                curvedata.IsArc = true;
                                curvedata.Center = new PointData(arc.Center.X, arc.Center.Y, arc.Center.Z);
                                curvedata.Normal = new PointData(arc.Normal.X, arc.Normal.Y, arc.Normal.Z);
                                curvedata.StartParameter = arc.GetEndParameter(0);
                                curvedata.EndParameter = arc.GetEndParameter(1);
                                curvedata.Radius = arc.Radius;
                                var pt = arc.GetEndPoint(0);
                                curvedata.Points.Add(new PointData(pt.X, pt.Y, pt.Z));
                                pt = arc.GetEndPoint(1);
                                curvedata.Points.Add(new PointData(pt.X, pt.Y, pt.Z));
                            }
                            else
                            {
                                curvedata.IsArc = false;
                                var points = curve.Tessellate();
                                foreach (var point in points)
                                    curvedata.Points.Add(new PointData(point.X, point.Y, point.Z));
                            }

                            sketchData.Curves.Add(curvedata);
                        }
                    }
                }
            }

            if (sketchData.Curves.Count < 1)
                return null;

            var plan = skt.SketchPlane.GetPlane();
            sketchData.Normal = new PointData(plan.Normal.X, plan.Normal.Y, plan.Normal.Z);
            sketchData.Origin = new PointData(plan.Origin.X, plan.Origin.Y, plan.Origin.Z);
            sketchData.XVector = new PointData(plan.XVec.X, plan.XVec.Y, plan.XVec.Z);
            sketchData.YVector = new PointData(plan.YVec.X, plan.YVec.Y, plan.YVec.Z);

            return sketchData;
        }

        /// <summary>
        /// 获取文档中所有的轴网信息
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>返回的是个轴号和pLine线列表字段</returns>
        private List<GridData> GetGridFromDocument(Document doc)
        {
            var result = new List<GridData>();

            var gridIds = (new FilteredElementCollector(doc)).OfClass(typeof(Grid)).ToElementIds();
            var mGridIds = (new FilteredElementCollector(doc)).OfClass(typeof(MultiSegmentGrid)).ToElementIds();

            foreach (var id in mGridIds)
            {
                

                var mgrid = doc.GetElement(id) as MultiSegmentGrid;
                List<CurveData> curves = new List<CurveData>();
                if (mgrid != null)
                {
                    var subIds = mgrid.GetGridIds();
                    foreach (var subId in subIds)
                    {
                        gridIds.Remove(subId);

                        var grid = doc.GetElement(subId) as Grid;
                        if (grid != null)
                            curves.Add(grid.Curve.ToCurveData());
                    }
                }

                if (curves.Count > 0)
                {
                    result.Add(new GridData
                    {
                        Name = mgrid.Name,
                        Curves = curves,
                        Properties = Tools.GetPropertyFromElement(mgrid)
                    });
                }
            }

            foreach (var id in gridIds)
            {
                var grid = doc.GetElement(id) as Grid;
                if (grid != null)
                {
                    result.Add(new GridData
                    {
                        Name = grid.Name,
                        Curves = new List<CurveData> { grid.Curve.ToCurveData()},
                        Properties = Tools.GetPropertyFromElement(grid)
                    });
                }
            }

            return result;
        }
    }

    public class WindowHandle : System.Windows.Forms.IWin32Window
    {
        IntPtr _hwnd;

        public WindowHandle(IntPtr h)
        {
            _hwnd = h;
        }

        public IntPtr Handle
        {
            get
            {
                return _hwnd;
            }
        }
    }
}
