using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.Utility;
using VectorDraw.Geometry;
using VectorDraw.Professional.vdPrimaries;
using System.IO;
using System.Xml;

namespace Exporter
{
    public enum MatType
    {
        eStandardFBX,
        eCeramic,
        eConcrete,
        eDecal,
        eDecalAppearance,
        eFabric,
        eGeneric,
        eGlazing,
        eHardwood,
        eMasonaryCMU,
        eMetal,
        eMetallicPaint,
        eMirror,
        ePlasticVinyl,
        eSolidGlass,
        eStone,
        eWallPaint,
        eWater,
        eUnknownMat,
    }

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
        public static readonly string FileExtention = ".bim";

        public static List<string> TextureLibPaths = new List<string>
        {
            @"C:\Program Files (x86)\Common Files\Autodesk Shared\Materials\Textures\3\Mats",
            @"C:\Program Files (x86)\Common Files\Autodesk Shared\Materials\Textures\2\Mats",
            @"C:\Program Files (x86)\Common Files\Autodesk Shared\Materials\Textures\1\Mats"
        };

        public static PointData ToPointData(this XYZ pt)
        {
            return new PointData(pt.X, pt.Y, pt.Z);
        }

        public static void GetMaterialTexture(Material mtl, AssetSet builtinLibrary, out string diffuseTexture, out string bumpTexture, out double scaleX, out double scaleY,
            out double metallic, out double smoothness)
        {
            diffuseTexture = string.Empty;
            bumpTexture = string.Empty;
            scaleX = 1.0;
            scaleY = 1.0;
            metallic = 0.0;
            smoothness = 0.5;

            var assetElementId = mtl.AppearanceAssetId;
            if (assetElementId == ElementId.InvalidElementId)
                return;

            var objassetElement = mtl.Document.GetElement(assetElementId) as AppearanceAssetElement;
            if (objassetElement == null)
                return;

            var curAsset = objassetElement.GetRenderingAsset();
            if (curAsset.Size == 0)
            {
                curAsset = (from Asset asset in builtinLibrary
                            where asset.Name == curAsset.Name && asset.LibraryName == curAsset.LibraryName
                            select asset).FirstOrDefault();
            }

            if (curAsset != null)
                ReadTextureFromAsset(curAsset, out diffuseTexture, out bumpTexture, out scaleX, out scaleY, out metallic, out smoothness);
        }

        public static void ReadTextureFromAsset(Asset asset, out string diffuseTexture, out string bumpTexture, out double scaleX, out double scaleY,
            out double metallic, out double smoothness)
        {
            diffuseTexture = string.Empty;
            bumpTexture = string.Empty;
            scaleX = 1.0;
            scaleY = 1.0;
            metallic = 0.0;
            smoothness = 0.5;

            double tmpx, tmpy;
            var matType = GetMaterialType(asset);
            switch (matType)
            {
                case MatType.eStandardFBX:
                    break;
                case MatType.eCeramic:
                    diffuseTexture = GetStringValueFromAsset(asset["ceramic_color"], out scaleX, out scaleY);
                    bumpTexture = GetStringValueFromAsset(asset["ceramic_pattern_map"], out tmpx, out tmpy);
                    break;
                case MatType.eConcrete:
                    diffuseTexture = GetStringValueFromAsset(asset["concrete_color"], out scaleX, out scaleY);
                    bumpTexture = GetStringValueFromAsset(asset["broom_straight"], out tmpx, out tmpy);
                    if (string.IsNullOrEmpty(bumpTexture))
                        bumpTexture = GetStringValueFromAsset(asset["broom_curved"], out tmpx, out tmpy);
                    if (string.IsNullOrEmpty(bumpTexture))
                        bumpTexture = GetStringValueFromAsset(asset["concrete_bump_map"], out tmpx, out tmpy);
                    break;
                case MatType.eDecal:
                    break;
                case MatType.eDecalAppearance:
                    break;
                case MatType.eFabric:
                    diffuseTexture = GetStringValueFromAsset(asset["fabric_color"], out scaleX, out scaleY);
                    bumpTexture = GetStringValueFromAsset(asset["fabric_bump_map"], out tmpx, out tmpy);
                    break;
                case MatType.eGeneric:
                    diffuseTexture = GetStringValueFromAsset(asset["generic_diffuse"], out scaleX, out scaleY);
                    bumpTexture = GetStringValueFromAsset(asset["generic_bump_map"], out tmpx, out tmpy);
                    break;
                case MatType.eGlazing:
                    break;
                case MatType.eHardwood:
                    diffuseTexture = GetStringValueFromAsset(asset["hardwood_color"], out scaleX, out scaleY);
                    bumpTexture = GetStringValueFromAsset(asset["hardwood_imperfections_shader"], out tmpx, out tmpy);
                    metallic = 1.0;
                    break;
                case MatType.eMasonaryCMU:
                    diffuseTexture = GetStringValueFromAsset(asset["masonrycmu_color"], out scaleX, out scaleY);
                    bumpTexture = GetStringValueFromAsset(asset["masonrycmu_pattern_map"], out tmpx, out tmpy);
                    break;
                case MatType.eMetal:
                    diffuseTexture = GetStringValueFromAsset(asset["metal_color"], out scaleX, out scaleY);
                    bumpTexture = GetStringValueFromAsset(asset["metal_pattern_shader"], out tmpx, out tmpy);
                    metallic = 1.0;
                    break;
                case MatType.eMetallicPaint:
                    diffuseTexture = GetStringValueFromAsset(asset["metallicpaint_base_color"], out scaleX, out scaleY);
                    metallic = 1.0;
                    break;
                case MatType.eMirror:
                    break;
                case MatType.ePlasticVinyl:
                    diffuseTexture = GetStringValueFromAsset(asset["plasticvinyl_color"], out scaleX, out scaleY);
                    bumpTexture = GetStringValueFromAsset(asset["plasticvinyl_bump_map"], out tmpx, out tmpy);
                    break;
                case MatType.eSolidGlass:
                    bumpTexture = GetStringValueFromAsset(asset["solidglass_bump_map"], out tmpx, out tmpy);
                    smoothness = 1.0;
                    break;
                case MatType.eStone:
                    diffuseTexture = GetStringValueFromAsset(asset["stone_color"], out scaleX, out scaleY);
                    bumpTexture = GetStringValueFromAsset(asset["stone_pattern_map"], out tmpx, out tmpy);
                    metallic = 1.0;
                    break;
                case MatType.eWallPaint:
                    break;
                case MatType.eWater:
                    break;
                default:
                    break;
            }





            /*
            var description = (asset["description"] as AssetPropertyString)?.Value;
            if (string.IsNullOrEmpty(description))
                return;

            double tmpx, tmpy;

            switch (description)
            {
                case "Generic material.":
                case "heavy cardboard used for mounting and architectural model-building":
                case "Medium gray and rippled edge roofing shingles":
                    diffuseTexture = GetStringValueFromAsset(asset["generic_diffuse"], out scaleX, out scaleY);
                    bumpTexture = GetStringValueFromAsset(asset["generic_bump_map"], out tmpx, out tmpy);
                    break;
                case "Hardwood material.":
                case "3 1/4\" dark brown stained white ash flooring.  Image courtesy of Mercier Wood Flooring Inc.":
                    diffuseTexture = GetStringValueFromAsset(asset["hardwood_color"], out scaleX, out scaleY);
                    bumpTexture = GetStringValueFromAsset(asset["hardwood_imperfections_shader"], out tmpx, out tmpy);
                    break;
                case "Masonry and CMU material.":
                    diffuseTexture = GetStringValueFromAsset(asset["masonrycmu_color"], out scaleX, out scaleY);
                    bumpTexture = GetStringValueFromAsset(asset["masonrycmu_pattern_map"], out tmpx, out tmpy);
                    break;
                case "Plastic and vinyl material.":
                    diffuseTexture = GetStringValueFromAsset(asset["plasticvinyl_color"], out scaleX, out scaleY);
                    bumpTexture = GetStringValueFromAsset(asset["plasticvinyl_bump_map"], out tmpx, out tmpy);
                    break;
                case "Stone material.":
                    diffuseTexture = GetStringValueFromAsset(asset["stone_color"], out scaleX, out scaleY);
                    bumpTexture = GetStringValueFromAsset(asset["stone_bump_map"], out tmpx, out tmpy);
                    break;
                case "Images courtesy of Precast/Prestressed Concrete Institute (PCI).":
                    diffuseTexture = GetStringValueFromAsset(asset["concrete_bump_map"], out scaleX, out scaleY);
                    bumpTexture = diffuseTexture;
                    break;
                default:
                    break;
            }
            */
        }

        public static string GetStringValueFromAsset(AssetProperty assetProp, out double scaleX, out double scaleY)
        {
            scaleX = 1.0;
            scaleY = 1.0;

            if (assetProp == null)
                return string.Empty;

            if (assetProp.NumberOfConnectedProperties < 1)
                return string.Empty;

            var connectedAsset = assetProp.GetConnectedProperty(0) as Asset;
            if (connectedAsset == null)
                return string.Empty;

            double x, y;
            string res = string.Empty;
            GetMapNameAndScale(connectedAsset, out res, out x, out y);
            if (string.IsNullOrEmpty(res))
                return string.Empty;

            scaleX = x;
            scaleY = y;

            return res;
        }

        public static void GetMapNameAndScale(Asset asset, out string mapName, out double scaleX, out double scaleY)
        {
            mapName = string.Empty;
            scaleX = 1.0;
            scaleY = 1.0;

            var strAsset = asset["unifiedbitmap_Bitmap"] as AssetPropertyString;
            if (strAsset == null)
                return;

            mapName = strAsset.Value;

            scaleX = GetScaleValueX(asset);
            scaleY = GetScaleValueY(asset);
        }

        public static double GetScaleValueX(Asset asset)
        {
            var dstAsset = asset["unifiedbitmap_RealWorldScaleX"] as AssetPropertyDistance ??
                           asset["texture_RealWorldScaleX"] as AssetPropertyDistance;

            return dstAsset == null ? 1.0 : dstAsset.Value;
        }

        public static double GetScaleValueY(Asset asset)
        {
            var dstAsset = asset["unifiedbitmap_RealWorldScaleY"] as AssetPropertyDistance ??
                           asset["texture_RealWorldScaleY"] as AssetPropertyDistance;

            return dstAsset == null ? 1.0 : dstAsset.Value;
        }

        private static string GetProteinLibraryPath()
        {
            string str = (((Environment.GetEnvironmentVariable("ILBDIR") ?? Environment.GetEnvironmentVariable("ILLDIR")) ?? Environment.GetEnvironmentVariable("ILLDIR")) ?? Environment.GetEnvironmentVariable("CM2014DIR")) ?? Environment.GetEnvironmentVariable("CM2015DIR");
            if (str == null)
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86) + "\\Autodesk Shared\\Materials\\";
                if (Directory.Exists(path))
                    str = path;
            }

            if (str == null)
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles) + "\\Autodesk Shared\\Materials\\";
                if (Directory.Exists(path))
                    str = path;
            }

            return str;
        }

        public static MatType GetMaterialType(Asset asset)
        {
            string str = "";
            for (int index = 0; index < asset.Size; ++index)
            {
                AssetProperty assetProperty = asset[index];
                if (assetProperty.Name == "ImplementationOGS")
                {
                    str = (assetProperty as AssetPropertyString).Value;
                    break;
                }
            }
            string n = "";
            string path = GetProteinLibraryPath() + "2016\\assetlibrary_base.fbm\\" + str;
            if (File.Exists(path))
            {
                XmlDocument xmlDocument = new XmlDocument();
                string filename = path;
                xmlDocument.Load(filename);
                foreach (XmlNode xmlNode in xmlDocument.DocumentElement.SelectNodes("/implementation/bindings/desc"))
                {
                    n = xmlNode.Attributes[1].Value;
                    if (n != "")
                        break;
                }
            }

            return GetMaterialType(n);
        }

        public static MatType GetMaterialType(string tagString)
        {
            if (tagString == "Ceramic")
                return MatType.eCeramic;
            if (tagString == "Concrete")
                return MatType.eConcrete;
            if (tagString == "Decal")
                return MatType.eDecal;
            if (tagString == "DecalAppearance")
                return MatType.eDecalAppearance;
            if (tagString == "Fabric")
                return MatType.eFabric;
            if (tagString == "Generic")
                return MatType.eGeneric;
            if (tagString == "Glazing")
                return MatType.eGlazing;
            if (tagString == "Hardwood" || tagString == "Wood")
                return MatType.eHardwood;
            if (tagString == "MasonryCMU")
                return MatType.eMasonaryCMU;
            if (tagString == "Metal")
                return MatType.eMetal;
            if (tagString == "MetallicPaint")
                return MatType.eMetallicPaint;
            if (tagString == "Mirror")
                return MatType.eMirror;
            if (tagString == "PlasticVinyl" || tagString == "Plastic")
                return MatType.ePlasticVinyl;
            if (tagString == "SolidGlass" || tagString == "Glass")
                return MatType.eSolidGlass;
            if (tagString == "Stone")
                return MatType.eStone;
            if (tagString == "WallPaint" || tagString == "Paint")
                return MatType.eWallPaint;

            return tagString == "Water" ? MatType.eWater : MatType.eUnknownMat;
        }

        public static List<LevelData> GetLevelsFromDocument(Document doc)
        {
            if (doc == null)
                return null;

            var classFilter = new ElementClassFilter(typeof(Level));
            var levelCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Levels);
            var logicalFilter = new LogicalAndFilter(classFilter, levelCategoryFilter);

            var collector = new FilteredElementCollector(doc);
            var levElems = collector.WherePasses(logicalFilter).ToElements();

            var res = new List<LevelData>();
            foreach (var elem in levElems)
            {
                var level = elem as Level;
                res.Add(new LevelData
                {
                    Name = level.Name,
                    Height = level.Elevation
                });
            }

            res.Sort((a, b) => a.Height.CompareTo(b.Height));
            return res;
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

        /// <summary>
        /// 获取文档中所有的轴网信息
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>返回的是个轴号和pLine线列表字段</returns>
        public static List<GridData> GetGridFromDocument(Document doc)
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
                        Curves = new List<CurveData> { grid.Curve.ToCurveData() },
                        Properties = Tools.GetPropertyFromElement(grid)
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// 获取文档中所有的钢筋信息
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static List<RebarData> GetRebaresInDocument(Document doc)
        {
            var result = new List<RebarData>();
            var rebares = GetElementInDocument<Rebar>(doc);
            foreach (var rb in rebares)
            {
                var rd = new RebarData();
                rd.CurvePoints = new List<PointData>();
                rd.Properties = GetPropertyFromElement(rb);

                if (rb.LayoutRule == RebarLayoutRule.Single)
                {
                }
                else if (rb.LayoutRule == RebarLayoutRule.NumberWithSpacing)
                {
                    var line = rb.GetDistributionPath();
                    rd.DistributionVector = line.Direction.ToPointData();
                    rd.RepeatDistance = rb.MaxSpacing;
                    rd.RepeatCount = rb.NumberOfBarPositions;
                }

#if _Revit2016
                var curves = rb.GetCenterlineCurves(false, false, false);
#elif _Revit2017
                var curves = rb.GetCenterlineCurves(false, false, false);
#endif
                foreach (var curve in curves)
                {
                    if (curve is Line)
                    {
                        rd.CurvePoints.Add(curve.GetEndPoint(0).ToPointData());
                        rd.CurvePoints.Add(curve.GetEndPoint(1).ToPointData());
                    }
                    else if (curve is Arc)
                    {
                        var pts = curve.Tessellate();
                        rd.CurvePoints.AddRange(pts.Select(pt => pt.ToPointData()).ToList());
                    }
                }

                result.Add(rd);
            }

            return result;
        }

        public static List<T> GetElementInDocument<T>(Document doc)
        {
            if (doc == null)
                throw new ArgumentNullException(nameof(doc));

            var classFilter = new ElementClassFilter(typeof(T));
            //var levelCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Levels);
            //var logicalFilter = new LogicalAndFilter(classFilter, levelCategoryFilter);

            var collector = new FilteredElementCollector(doc);
            var levElems = collector.WherePasses(classFilter).ToElements();

            return levElems.Cast<T>().ToList();
        }

        public static List<T> GetNotNativeElementInDocument<T>(Document doc)
        {
            if (doc == null)
                throw new ArgumentNullException(nameof(doc));

            return (new FilteredElementCollector(doc)).OfClass(typeof(SpatialElement)).OfType<T>().ToList();
        }

        public static List<FamilyInstance> GetFamilyInstanceInDocument(Document doc, BuiltInCategory cate)
        {
            if (doc == null)
                throw new ArgumentNullException(nameof(doc));

            var categoryFilter = new ElementCategoryFilter(cate);
            var collector = new FilteredElementCollector(doc);
            var elems = collector.WherePasses(categoryFilter).ToElements();
            return elems.OfType<FamilyInstance>().ToList();
        }

        public static Vertexes GetVertexesFromPoints(List<PointData> points)
        {
            Vertexes vtxs = new Vertexes();
            gPoints gps = new gPoints();
            points.ForEach(pt => gps.Add(new gPoint(pt.X, pt.Y, pt.Z)));
            gps.RemoveEqualPoints(0.01);

            vtxs.AddRange(gps);
            return vtxs;
        }

        /*
            Revit中导出的多段轴网信息很奇怪，首先，对于直线段，直线的起始、终止点和点击时的顺序是相反的，但是多段线的顺序是正确的，所以整个
            排序就变成了“终点”-“起点”-“终点”-“起点”的方式，所以在导出的时候，程序处理时反着顺序导出，就可以正常得到“起点”-“终点”
            的顺序了。其次，如果遇上了圆弧段，圆弧段的起点、终点的顺序是和点击的顺序是一致的，所以遇到圆弧段，输出的起点和终点交换，这样就
            可以正确输出了。下面的两个方法就是用来处理这个。
        */

        public static Vertexes GetVertexesFromCurves(List<CurveData> curves)
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

        public static void GetVertexFromSingleCurve(CurveData curve, out Vertex vStart, out Vertex vEnd)
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

        public static string ToSplitString(this gPoint pt)
        {
            return pt.x + "," + pt.y + "," + pt.z;
        }

    }
}
