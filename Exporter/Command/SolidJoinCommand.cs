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

            var walls = Tools.GetElementInDocument<Wall>(doc);
            if (walls.Count < 2)
            {
                TaskDialog.Show("wall", "当前图形的墙少于两个！");
                return Result.Succeeded;
            }

            Transaction t = new Transaction(doc, "join");
            t.Start();
            for (int i = 0; i < walls.Count - 1; i++)
            {
                for (int j = i + 1; j < walls.Count; j++)
                {
                    ProcessJoinElement(doc, walls[i], walls[j]);
                }
            }
            t.Commit();

            return Result.Succeeded;
        }

        public bool ProcessJoinElement(Document doc, Element elem1, Element elem2)
        {
            if (elem1 == elem2)
                return true;

            if (elem1 == null || elem2 == null)
                return false;

            try
            {
                if (JoinGeometryUtils.AreElementsJoined(doc, elem1, elem2))
                {
                    if (JoinGeometryUtils.IsCuttingElementInJoin(doc, elem1, elem2) && GetCompareWeight(elem1) < GetCompareWeight(elem2)
                        || JoinGeometryUtils.IsCuttingElementInJoin(doc, elem2, elem1) && GetCompareWeight(elem2) < GetCompareWeight(elem1))
                        JoinGeometryUtils.SwitchJoinOrder(doc, elem1, elem2);
                }
                else
                {
                    if (GetCompareWeight(elem1) >= GetCompareWeight(elem2))
                        JoinGeometryUtils.JoinGeometry(doc, elem1, elem2);
                    else
                        JoinGeometryUtils.JoinGeometry(doc, elem2, elem1);
                }

                return true;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("异常", ex.Message);
                return false;
            }
        }

        private int GetCompareWeight(Element elem)
        {
            if (elem.Name.Contains("140"))
                return 1;
            else if (elem.Name.Contains("300"))
                return 2;
            else
                return 1;
        }
    }
}
