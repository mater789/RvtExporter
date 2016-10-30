using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using VectorDraw.Geometry;
using VectorDraw.Professional.vdPrimaries;

namespace Exporter
{
    class CompareMethod : IComparer<vdFigure>
    {
        //重写int比较器，|x|>|y|返回正数，|x|=|y|返回0，|x|<|y|返回负数   
        public int Compare(vdFigure x, vdFigure y)
        {

            double x1 = getfigurevolume(x);
            double y1 = getfigurevolume(y);

            if (x1 > y1)
                return -1;
            else
            {
                return 1;
            }
        }

        double getfigurevolume(vdFigure vdf)
        {
            Box b = vdf.BoundingBox;

            return (b.Max.z - b.Min.z) + (b.Max.y - b.Min.y) + (b.Max.x - b.Min.x);

        }
    }

    public static class Tools
    {
        public static double Ft2MmScale = 304.8;

        public static PointData ToPointData(this XYZ pt)
        {
            return new PointData(pt.X, pt.Y, pt.Z);
        }

        public static CurveData ToCurveData(this Curve curve)
        {
            CurveData cd = new CurveData();

            cd.Points.Add(curve.GetEndPoint(0).ToPointData());
            cd.Points.Add(curve.GetEndPoint(1).ToPointData());
            cd.StartParameter = curve.GetEndParameter(0);
            cd.EndParameter = curve.GetEndParameter(1);

            var arc = curve as Arc;
            cd.IsArc = arc != null;

            if (cd.IsArc)
            {
                cd.Normal = arc.Normal.ToPointData();
                cd.Center = arc.Center.ToPointData();
                cd.Radius = arc.Radius;
            }

            return cd;
        }

        public static Dictionary<string, List<PropertyData>> GetPropertyFromElement(Element elem)
        {
            var dictProperties = new Dictionary<string, List<PropertyData>>();

            // 属性中添加族和类型信息
            var internalProps = new List<PropertyData>();
            internalProps.Add(new PropertyData { GroupName = "#Internal", Name = "#name", Value = elem.Name });
            if (elem.Category != null)
                internalProps.Add(new PropertyData { GroupName = "#Internal", Name = "#category", Value = elem.Category.Name });
            internalProps.Add(new PropertyData { GroupName = "#Internal", Name = "#guid", Value = elem.UniqueId });
            var fname = GetFamilyName(elem);
            if (!string.IsNullOrEmpty(fname))
                internalProps.Add(new PropertyData { GroupName = "#Internal", Name = "#family", Value = fname });
            dictProperties["#Internal"] = internalProps;

            if (elem.Parameters != null)
            {
                foreach (Parameter param in elem.Parameters)
                {
                    string groupName = LabelUtils.GetLabelFor(param.Definition.ParameterGroup);
                    if (string.IsNullOrEmpty(groupName))
                        groupName = param.Definition.ParameterGroup.ToString();

                    PropertyData proData = new PropertyData();
                    proData.Name = param.Definition.Name;
                    proData.GroupName = groupName;
                    proData.Value = param.StorageType == StorageType.String ? param.AsString() : param.AsValueString();

                    if (dictProperties.ContainsKey(groupName))
                    {
                        dictProperties[groupName].Add(proData);
                    }
                    else
                    {
                        List<PropertyData> listTmp = new List<PropertyData>();
                        listTmp.Add(proData);
                        dictProperties.Add(groupName, listTmp);
                    }
                }
            }

            return dictProperties;
        }

        private static string GetFamilyName(Element elem)
        {
            if (elem == null)
                return string.Empty;

            var fname = string.Empty;
            try
            {
                if (elem is FamilyInstance)
                    fname = (elem as FamilyInstance).Symbol.Family.Name;
            }
            catch
            { }

            return fname;
        }
    }
}
