using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace Exporter
{
    class Utils
    {
        public static string GetGroupName(BuiltInParameterGroup group)
        {
            string strName = string.Empty;
            switch (group)
            {
                case BuiltInParameterGroup.PG_REFERENCE:
                    break;
                case BuiltInParameterGroup.PG_GEOMETRY_POSITIONING:
                    break;
                case BuiltInParameterGroup.PG_DIVISION_GEOMETRY:
                    break;
                case BuiltInParameterGroup.PG_SEGMENTS_FITTINGS:
                    break;
                case BuiltInParameterGroup.PG_CONTINUOUSRAIL_END_TOP_EXTENSION:
                    break;
                case BuiltInParameterGroup.PG_CONTINUOUSRAIL_BEGIN_BOTTOM_EXTENSION:
                    break;
                case BuiltInParameterGroup.PG_STAIRS_WINDERS:
                    break;
                case BuiltInParameterGroup.PG_STAIRS_SUPPORTS:
                    break;
                case BuiltInParameterGroup.PG_STAIRS_OPEN_END_CONNECTION:
                    break;
                case BuiltInParameterGroup.PG_RAILING_SYSTEM_SECONDARY_FAMILY_HANDRAILS:
                    break;
                case BuiltInParameterGroup.PG_TERMINTATION:
                    break;
                case BuiltInParameterGroup.PG_STAIRS_TREADS_RISERS:
                    break;
                case BuiltInParameterGroup.PG_STAIRS_CALCULATOR_RULES:
                    break;
                case BuiltInParameterGroup.PG_SPLIT_PROFILE_DIMENSIONS:
                    break;
                case BuiltInParameterGroup.PG_LENGTH:
                    break;
                case BuiltInParameterGroup.PG_NODES:
                    break;
                case BuiltInParameterGroup.PG_ANALYTICAL_PROPERTIES:
                    break;
                case BuiltInParameterGroup.PG_ANALYTICAL_ALIGNMENT:
                    break;
                case BuiltInParameterGroup.PG_SYSTEMTYPE_RISEDROP:
                    break;
                case BuiltInParameterGroup.PG_LINING:
                    strName = "内衬";
                    break;
                case BuiltInParameterGroup.PG_INSULATION:
                    strName = "绝缘层";
                    break;
                case BuiltInParameterGroup.PG_OVERALL_LEGEND:
                    break;
                case BuiltInParameterGroup.PG_VISIBILITY:
                    break;
                case BuiltInParameterGroup.PG_SUPPORT:
                    break;
                case BuiltInParameterGroup.PG_RAILING_SYSTEM_SEGMENT_V_GRID:
                    break;
                case BuiltInParameterGroup.PG_RAILING_SYSTEM_SEGMENT_U_GRID:
                    break;
                case BuiltInParameterGroup.PG_RAILING_SYSTEM_SEGMENT_POSTS:
                    break;
                case BuiltInParameterGroup.PG_RAILING_SYSTEM_SEGMENT_PATTERN_REMAINDER:
                    break;
                case BuiltInParameterGroup.PG_RAILING_SYSTEM_SEGMENT_PATTERN_REPEAT:
                    break;
                case BuiltInParameterGroup.PG_RAILING_SYSTEM_FAMILY_SEGMENT_PATTERN:
                    break;
                case BuiltInParameterGroup.PG_RAILING_SYSTEM_FAMILY_HANDRAILS:
                    break;
                case BuiltInParameterGroup.PG_RAILING_SYSTEM_FAMILY_TOP_RAIL:
                    break;
                case BuiltInParameterGroup.PG_CONCEPTUAL_ENERGY_DATA_BUILDING_SERVICES:
                    break;
                case BuiltInParameterGroup.PG_DATA:
                    break;
                case BuiltInParameterGroup.PG_ELECTRICAL_CIRCUITING:
                    strName = "电气 - 线路";
                    break;
                case BuiltInParameterGroup.PG_GENERAL:
                    break;
                case BuiltInParameterGroup.PG_FLEXIBLE:
                    break;
                case BuiltInParameterGroup.PG_ENERGY_ANALYSIS_CONCEPTUAL_MODEL:
                    break;
                case BuiltInParameterGroup.PG_ENERGY_ANALYSIS_DETAILED_MODEL:
                    break;
                case BuiltInParameterGroup.PG_ENERGY_ANALYSIS_DETAILED_AND_CONCEPTUAL_MODELS:
                    break;
                case BuiltInParameterGroup.PG_FITTING:
                    break;
                case BuiltInParameterGroup.PG_CONCEPTUAL_ENERGY_DATA:
                    break;
                case BuiltInParameterGroup.PG_AREA:
                    break;
                case BuiltInParameterGroup.PG_ADSK_MODEL_PROPERTIES:
                    break;
                case BuiltInParameterGroup.PG_CURTAIN_GRID_V:
                    break;
                case BuiltInParameterGroup.PG_CURTAIN_GRID_U:
                    break;
                case BuiltInParameterGroup.PG_DISPLAY:
                    break;
                case BuiltInParameterGroup.PG_ANALYSIS_RESULTS:
                    break;
                case BuiltInParameterGroup.PG_SLAB_SHAPE_EDIT:
                    break;
                case BuiltInParameterGroup.PG_LIGHT_PHOTOMETRICS:
                    break;
                case BuiltInParameterGroup.PG_PATTERN_APPLICATION:
                    break;
                case BuiltInParameterGroup.PG_GREEN_BUILDING:
                    break;
                case BuiltInParameterGroup.PG_PROFILE_2:
                    break;
                case BuiltInParameterGroup.PG_PROFILE_1:
                    break;
                case BuiltInParameterGroup.PG_PROFILE:
                    break;
                case BuiltInParameterGroup.PG_TRUSS_FAMILY_BOTTOM_CHORD:
                    break;
                case BuiltInParameterGroup.PG_TRUSS_FAMILY_TOP_CHORD:
                    break;
                case BuiltInParameterGroup.PG_TRUSS_FAMILY_DIAG_WEB:
                    break;
                case BuiltInParameterGroup.PG_TRUSS_FAMILY_VERT_WEB:
                    break;
                case BuiltInParameterGroup.PG_TITLE:
                    break;
                case BuiltInParameterGroup.PG_FIRE_PROTECTION:
                    break;
                case BuiltInParameterGroup.PG_ROTATION_ABOUT:
                    break;
                case BuiltInParameterGroup.PG_TRANSLATION_IN:
                    break;
                case BuiltInParameterGroup.PG_ANALYTICAL_MODEL:
                    break;
                case BuiltInParameterGroup.PG_REBAR_ARRAY:
                    break;
                case BuiltInParameterGroup.PG_REBAR_SYSTEM_LAYERS:
                    break;
                case BuiltInParameterGroup.PG_ORIENTATION:
                    break;
                case BuiltInParameterGroup.PG_CURTAIN_GRID:
                    break;
                case BuiltInParameterGroup.PG_CURTAIN_MULLION_2:
                    break;
                case BuiltInParameterGroup.PG_CURTAIN_MULLION_HORIZ:
                    break;
                case BuiltInParameterGroup.PG_CURTAIN_MULLION_1:
                    break;
                case BuiltInParameterGroup.PG_CURTAIN_MULLION_VERT:
                    break;
                case BuiltInParameterGroup.PG_CURTAIN_GRID_2:
                    break;
                case BuiltInParameterGroup.PG_CURTAIN_GRID_HORIZ:
                    break;
                case BuiltInParameterGroup.PG_CURTAIN_GRID_1:
                    break;
                case BuiltInParameterGroup.PG_CURTAIN_GRID_VERT:
                    break;
                case BuiltInParameterGroup.PG_IFC:
                    break;
                case BuiltInParameterGroup.PG_AELECTRICAL:
                    break;
                case BuiltInParameterGroup.PG_ENERGY_ANALYSIS:
                    break;
                case BuiltInParameterGroup.PG_STRUCTURAL_ANALYSIS:
                    break;
                case BuiltInParameterGroup.PG_MECHANICAL_AIRFLOW:
                    strName = "机械 - 流量";
                    break;
                case BuiltInParameterGroup.PG_MECHANICAL_LOADS:
                    break;
                case BuiltInParameterGroup.PG_ELECTRICAL_LOADS:
                    strName = "电气 - 负荷";
                    break;
                case BuiltInParameterGroup.PG_ELECTRICAL_LIGHTING:
                    strName = "电气 - 照明";
                    break;
                case BuiltInParameterGroup.PG_TEXT:
                    break;
                case BuiltInParameterGroup.PG_VIEW_CAMERA:
                    break;
                case BuiltInParameterGroup.PG_VIEW_EXTENTS:
                    break;
                case BuiltInParameterGroup.PG_PATTERN:
                    break;
                case BuiltInParameterGroup.PG_CONSTRAINTS:
                    strName = "限制条件";
                    break;
                case BuiltInParameterGroup.PG_PHASING:
                    strName = "阶段化";
                    break;
                case BuiltInParameterGroup.PG_MECHANICAL:
                    strName = "机械";
                    break;
                case BuiltInParameterGroup.PG_STRUCTURAL:
                    strName = "结构";
                    break;
                case BuiltInParameterGroup.PG_PLUMBING:
                    strName = "管道";
                    break;
                case BuiltInParameterGroup.PG_ELECTRICAL:
                    strName = "电气";
                    break;
                case BuiltInParameterGroup.PG_STAIR_STRINGERS:
                    break;
                case BuiltInParameterGroup.PG_STAIR_RISERS:
                    break;
                case BuiltInParameterGroup.PG_STAIR_TREADS:
                    break;
                case BuiltInParameterGroup.PG_MATERIALS  :
                    strName = "材质和装饰";
                    break;
                case BuiltInParameterGroup.PG_GRAPHICS  :
                case BuiltInParameterGroup.PG_CONSTRUCTION  :
                    strName = "构造";
                    break;
                case BuiltInParameterGroup.PG_GEOMETRY:
                    strName = "尺寸标注";
                    break;
                case BuiltInParameterGroup.PG_IDENTITY_DATA:
                    strName = "标识数据";
                    break;
                case BuiltInParameterGroup.INVALID:
                    strName = "其它";
                    break;
                default:
                    break;
            }

            return strName;
        }
    }
}
