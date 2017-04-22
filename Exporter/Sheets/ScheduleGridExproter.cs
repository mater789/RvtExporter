using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Utility;
using System.Linq;

namespace Exporter
{
    [Transaction(TransactionMode.Manual)]
    public class ScheduleGridExproter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            if (uiDoc == null)
            {
                TaskDialog.Show("提示", "当前没有打开的文档");
                return Result.Cancelled;
            }

            Document doc = uiDoc.Document;

            var classFilter = new ElementClassFilter(typeof (ViewSchedule));
            var collector = new FilteredElementCollector(doc);
            var elems = collector.WherePasses(classFilter).ToElements();

            string result = string.Empty;
            foreach (var elem in elems)
            {
                var schedule = elem as ViewSchedule;
                if (schedule == null)
                    continue;

                result += schedule.Name + "\n";
            }

            TaskDialog.Show("result", result);

            return Result.Succeeded;
        }
    }
}
