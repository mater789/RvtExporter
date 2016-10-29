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

namespace Exporter
{
    [Transaction(TransactionMode.Manual)]
    public class ExportCommand : IExternalCommand
    {
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
            if (uidoc.ActiveView as View3D == null)
            {
                MessageBox.Show("请在3D视图下使用此命令！");
                return Result.Cancelled;
            }

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

            List<MaterialData> listMat = colorOverride.GetMaterials();
            Dictionary<ElementId, string> matList = colorOverride.GetElementColorSetting();

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
            converter.BeginConvert(dlg.ExportSetting.SystemSetting.ExportFilePath);
                      
            return Result.Succeeded;
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
