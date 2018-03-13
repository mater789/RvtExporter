using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace Exporter
{
    [Transaction(TransactionMode.Manual)]
    public class SolidJoinCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc.Document;

            var decaldatas = Tools.GetDecalsFromDocument(doc);
            decaldatas.ForEach(d => MessageBox.Show(d.MapFileName));
            
            return Result.Succeeded;
        }


    }
}
