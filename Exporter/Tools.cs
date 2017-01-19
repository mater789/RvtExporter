using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
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
    }
}
