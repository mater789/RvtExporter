using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.Utility;
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

        public static void GetMaterialTexture(Material mtl, AssetSet builtinLibrary, out string diffuseTexture, out string bumpTexture, out double scaleX, out double scaleY)
        {
            diffuseTexture = string.Empty;
            bumpTexture = string.Empty;
            scaleX = 1.0;
            scaleY = 1.0;

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
                ReadTextureFromAsset(curAsset, out diffuseTexture, out bumpTexture, out scaleX, out scaleY);
        }

        public static void ReadTextureFromAsset(Asset asset, out string diffuseTexture, out string bumpTexture, out double scaleX, out double scaleY)
        {
            diffuseTexture = string.Empty;
            bumpTexture = string.Empty;
            scaleX = 1.0;
            scaleY = 1.0;

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
                    bumpTexture = GetStringValueFromAsset(asset["hardwood_imperfections_shader"],out tmpx, out tmpy);
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
    }
}
