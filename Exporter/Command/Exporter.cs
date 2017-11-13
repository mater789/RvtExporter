using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Utility;
using System.Linq;

namespace Exporter
{
    [Transaction(TransactionMode.Manual)]
    public class ExportCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            var ap = commandData.Application.Application;

            UIDocument uidoc = uiapp.ActiveUIDocument;
            if (uidoc == null)
            {
                MessageBox.Show("当前没有打开的Revit文档！");
                return Result.Cancelled;
            }

            Document doc = uidoc.Document;
            if (uidoc.ActiveView as View3D == null)
            {
                MessageBox.Show("请在3D视图下使用此命令！");
                return Result.Cancelled;
            }

            string filePath = Path.GetDirectoryName(doc.PathName);
            string fileName = Path.GetFileNameWithoutExtension(doc.PathName);

            FormSettings dlg = new FormSettings();
            dlg.ExportSetting.SystemSetting.ExportFilePath = filePath + "\\" + fileName + Tools.FileExtention;
            DialogResult dr = dlg.ShowDialog(new WindowHandle(Process.GetCurrentProcess().MainWindowHandle));
            if (dr != DialogResult.OK)
                return Result.Cancelled;

            Exception ex;
            var assertSet = uiapp.Application.get_Assets(AssetType.Appearance);

            if (Export(assertSet, uidoc.ActiveView as View3D, dlg.ExportSetting, out ex))
            {
                TaskDialog.Show("导出", "导出完成！");
                return Result.Succeeded;
            }
            else
            {
                TaskDialog.Show("导出", "导出失败！");
                messages = ex.Message;
                return Result.Failed;
            }
        }

        

        public bool Export(AssetSet appearanceAssetSet, View3D view, ExportSetting setting, out Exception ex)
        {
            ex = new Exception();

            try
            {
                ConvertEntity converter = new ConvertEntity();
                converter.ExportSetting = setting;
                if (setting.SystemSetting.IsExportRebar)
                {
                    converter.Rebars = Tools.GetRebaresInDocument(view.Document);
                }
                else
                {
                    ElementColorOverride colorOverride = new ElementColorOverride();
                    if (!setting.SystemSetting.IsOriginMaterial)
                        colorOverride.ArrangeElemlentColor(view.Document, view);

                    RevitEnvironmentSetting envSetting = new RevitEnvironmentSetting(view.Document);
                    if (setting.SystemSetting.IsModifyUnit)
                        envSetting.ReadOriginUnitsAndSetNew();

                    ModelExportContext context = new ModelExportContext(view.Document);
                    context.BuiltInMaterialLibraryAsset = appearanceAssetSet;
                    context.IsPackageEntityToBlock = true;
                    context.IsExportProperty = setting.SystemSetting.IsExportProperty;
                    context.ExtraMaterial = colorOverride.GetMaterials();
                    context.ExtraElementColorSetting = colorOverride.GetElementColorSetting();
                    context.IsOptimisePipeEntity = true;
                    CustomExporter exporter = new CustomExporter(view.Document, context);

                    //exporter.IncludeFaces = false;

                    exporter.ShouldStopOnError = false;
                    exporter.Export(view);

                    if (setting.SystemSetting.IsModifyUnit)
                        envSetting.RecoverOriginUnits();

                    converter.OptimizeTriangle = setting.SystemSetting.IsOptimizeCylinderFace;
                    converter.Materials = context.Materials;
                    converter.ModelBlock = context.ModelSpaceBlock;
                    converter.DictBlocks = context.DictBlocks;
                    converter.Levels = Tools.GetLevelsFromDocument(view.Document);
                    if (setting.SystemSetting.IsExportGrid)
                        converter.Grids = Tools.GetGridFromDocument(view.Document);
                }

                converter.WndParent = new WindowHandle(Process.GetCurrentProcess().MainWindowHandle);
                converter.BeginConvert(setting.SystemSetting.ExportFilePath);
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }

            return true;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class TextureCommand : IExternalCommand
    {
        private string _priexFix = string.Empty;

        public void ReadAsset(Asset asset, StreamWriter objWriter)
        {
            _priexFix += "\t";
            for (int idx = 0; idx < asset.Size; idx++)
            {
                AssetProperty prop = asset[idx];
                ReadAssetProperty(prop, objWriter);
            }
            _priexFix = _priexFix.Substring(0, _priexFix.Length - 1);
        }

        public void ReadAssetProperty(AssetProperty prop, StreamWriter objWriter)
        {
            switch (prop.Type)
            {
                // Retrieve the value from simple type property is easy.
                // for example, retrieve bool property value.
                case AssetPropertyType.APT_Integer:
                    var AssetPropertyInt = prop as AssetPropertyInteger;
                    objWriter.WriteLine(_priexFix + AssetPropertyInt.Name + "= " + AssetPropertyInt.Value);
                    break;

                case AssetPropertyType.APT_Distance:
                    var AssetPropertyDistance = prop as AssetPropertyDistance;
                    objWriter.WriteLine(_priexFix + AssetPropertyDistance.Name + "= " + AssetPropertyDistance.Value);
                    break;
                case AssetPropertyType.APT_Float:
                    var assFlot = prop as AssetPropertyFloat;
                    objWriter.WriteLine(_priexFix + assFlot.Name + "= " + assFlot.Value);
                    break;
                case AssetPropertyType.APT_Double:
                    var AssetPropertyDouble = prop as AssetPropertyDouble;
                    objWriter.WriteLine(_priexFix + AssetPropertyDouble.Name + "= " + AssetPropertyDouble.Value);
                    break;

                case AssetPropertyType.APT_DoubleArray2d:
                    var AssetPropertyDoubleArray2d = prop as AssetPropertyDoubleArray2d;
                    string msg = AssetPropertyDoubleArray2d.Value.Cast<double>().Aggregate(string.Empty, (current, v) => current + (v + ", "));
                    objWriter.WriteLine(_priexFix + AssetPropertyDoubleArray2d.Name + "= " + msg);
                    break;
                case AssetPropertyType.APT_DoubleArray3d:
                    var arr3d = prop as AssetPropertyDoubleArray3d;
                    msg = arr3d.Value.Cast<double>().Aggregate(string.Empty, (current, v) => current + (v + ", "));
                    objWriter.WriteLine(_priexFix + arr3d.Name + "= " + msg);
                    break;
                case AssetPropertyType.APT_DoubleArray4d:
                    var AssetPropertyDoubleArray4d = prop as AssetPropertyDoubleArray4d;
                    msg = AssetPropertyDoubleArray4d.Value.Cast<double>().Aggregate(string.Empty, (current, v) => current + (v + ", "));
                    objWriter.WriteLine(_priexFix + AssetPropertyDoubleArray4d.Name + "= " + msg);
                    break;

                case AssetPropertyType.APT_String:
                    AssetPropertyString val = prop as AssetPropertyString;

                    objWriter.WriteLine(_priexFix + val.Name + "= " + val.Value);
                    break;
                case AssetPropertyType.APT_Boolean:
                    AssetPropertyBoolean boolProp = prop as AssetPropertyBoolean;
                    objWriter.WriteLine(_priexFix + boolProp.Name + "= " + boolProp.Value);
                    break;

                // When you retrieve the value from the data array property,
                // you may need to get which value the property stands for.
                // for example, the APT_Double44 may be a transform data.
                case AssetPropertyType.APT_Double44:
                    AssetPropertyDoubleArray4d transformProp = prop as AssetPropertyDoubleArray4d;
                    objWriter.WriteLine(_priexFix + transformProp.Name + "= " + transformProp.Value);
                    break;

                // The APT_List contains a list of sub asset properties with same type.
                case AssetPropertyType.APT_List:
                    AssetPropertyList propList = prop as AssetPropertyList;
                    IList<AssetProperty> subProps = propList.GetValue();
                    if (subProps.Count == 0)
                        break;
                    objWriter.WriteLine(_priexFix + propList.Name + " as propList");

                    _priexFix += "\t";
                    foreach (var ap in subProps)
                    {
                        ReadAssetProperty(ap, objWriter);
                    }
                    _priexFix = _priexFix.Substring(0, _priexFix.Length - 1);

                    break;

                case AssetPropertyType.APT_Asset:
                    Asset propAsset = prop as Asset;
                    objWriter.WriteLine(_priexFix + propAsset.Name + " as Asset");
                    ReadAsset(propAsset, objWriter);
                    break;
                case AssetPropertyType.APT_Enum:
                    var propEnum = prop as AssetPropertyEnum;
                    objWriter.WriteLine(_priexFix + propEnum.Name + "= " + propEnum.Value);
                    break;

                case AssetPropertyType.APT_Reference:
                    var propRef = prop as AssetPropertyReference;
                    objWriter.WriteLine(_priexFix + prop.Name + " as propReference");
                    break;
                default:
                    objWriter.WriteLine(_priexFix + "居然有啥都不是类型的" + prop.Type);
                    break;
            }

            // Get the connected properties.
            // please notice that the information of many texture stores here.
            if (prop.NumberOfConnectedProperties == 0)
                return;

            objWriter.WriteLine(_priexFix + "Connected Property: ");
            _priexFix += "\t";
            foreach (AssetProperty connectedProp in prop.GetAllConnectedProperties())
            {
                // Note: Usually, the connected property is an Asset.
                ReadAssetProperty(connectedProp, objWriter);
            }
            _priexFix = _priexFix.Substring(0, _priexFix.Length - 1);
        }

        private void ExportMaterialInfo(ExternalCommandData commandData)
        {
            var objDoc = commandData.Application.ActiveUIDocument.Document;
            var objApp = commandData.Application.Application;
            var collector = new FilteredElementCollector(objDoc);
            var classfilter = new ElementClassFilter(typeof (Material));
            collector.WherePasses(classfilter);
            var beamList = collector.ToElements();

            // 读取revit标准材质库
            var objlibraryAsset = objApp.get_Assets(AssetType.Appearance);

            //写入临时文件
            using (var objWriter = new StreamWriter(@"D:\aaa.txt"))
            {
                foreach (var objLoopItem in beamList)
                {
                    var objMaterial = objLoopItem as Material;
                    if (objMaterial != null)
                    {
                        if (objMaterial.Name != "木纹防火板")
                            continue;

                        var assetElementId = objMaterial.AppearanceAssetId;
                        if (assetElementId != ElementId.InvalidElementId)
                        {
                            objWriter.WriteLine(_priexFix + "材质：" + objMaterial.Name);

                            var objassetElement = objDoc.GetElement(assetElementId) as AppearanceAssetElement;
                            if (objassetElement != null)
                            {
                                var currentAsset = objassetElement.GetRenderingAsset();

                                // 检索不到材质外观时，为欧特克材质库材质
                                if (currentAsset.Size == 0)
                                {
                                    foreach (Asset objCurrentAsset in objlibraryAsset)
                                    {
                                        if (objCurrentAsset.Name == currentAsset.Name &&
                                            objCurrentAsset.LibraryName == currentAsset.LibraryName)
                                        {
                                            objWriter.WriteLine(_priexFix + "读取标准材质库...");
                                            ReadAsset(objCurrentAsset, objWriter);
                                        }
                                    }
                                }
                                else
                                {
                                    objWriter.WriteLine(_priexFix + "读取自定义材质...");
                                    ReadAsset(currentAsset, objWriter);
                                }
                            }
                        }
                    }
                }
                objWriter.Close();
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK)
                return Result.Cancelled;

            Stopwatch sw = new Stopwatch();
            using (Stream s = File.OpenRead(dlg.FileName))
            {
                //从文件中读取并反序列化到对象
                sw.Start();
                var data = ProtoBuf.Serializer.Deserialize<ModelSerializeEntity>(s);
                MessageBox.Show(sw.ElapsedMilliseconds.ToString());
            }

            sw.Stop();

            /*

            var strResult = File.ReadAllText(dlg.FileName);
            sw.Start();
            ModelSerializeEntity entity = LitJson.JsonMapper.ToObject<ModelSerializeEntity>(strResult);
            MessageBox.Show(sw.ElapsedMilliseconds.ToString());
            sw.Stop();
            if (entity == null)
                return Result.Failed;
*/
            return Result.Succeeded;
            

            /**/
            var doc = commandData.Application.ActiveUIDocument.Document;

            var classFilter = new ElementClassFilter(typeof (Level));
            var levelCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Levels);
            var logicalFilter = new LogicalAndFilter(classFilter, levelCategoryFilter);

            var collector = new FilteredElementCollector(doc);
            var levElems = collector.WherePasses(logicalFilter).ToElements();

            string result = string.Empty;
            foreach (var elem in levElems)
            {
                var level = elem as Level;
                result += level.Name + ": " + level.Elevation + "\n";
            }

            MessageBox.Show(result);

            return Result.Succeeded;
            


            /* 
            ExportMaterialInfo(commandData);
            return Result.Succeeded;
           */
        }
    }
}
