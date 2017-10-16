using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Windows.Forms;

namespace Exporter
{
    public class RevitEnvironmentSetting
    {
        private Document m_doc = null;
        private Units m_units = null;

        FormatOptions lengthFoOrigin, areaFoOrigin, volumeFoOrigin;

        public RevitEnvironmentSetting(Document doc)
        {
            m_doc = doc;
        }

        public bool ReadOriginUnitsAndSetNew()
        {
            try
            {
                m_units = m_doc.GetUnits();
                lengthFoOrigin = m_units.GetFormatOptions(UnitType.UT_Length);
                FormatOptions foNew = new FormatOptions(lengthFoOrigin);
                foNew.DisplayUnits = DisplayUnitType.DUT_MILLIMETERS;
                foNew.UnitSymbol = UnitSymbolType.UST_NONE; //UnitSymbolType.UST_MM;
                foNew.Accuracy = 0.01;
                m_units.SetFormatOptions(UnitType.UT_Length, foNew);

                areaFoOrigin = m_units.GetFormatOptions(UnitType.UT_Area);
                FormatOptions areaFo = new FormatOptions(areaFoOrigin);
                areaFo.DisplayUnits = DisplayUnitType.DUT_SQUARE_METERS;
                areaFo.UnitSymbol = UnitSymbolType.UST_NONE;  //UnitSymbolType.UST_M_SUP_2;
                areaFo.Accuracy = 0.000001;
                m_units.SetFormatOptions(UnitType.UT_Area, areaFo);

                volumeFoOrigin = m_units.GetFormatOptions(UnitType.UT_Volume);
                FormatOptions volumeFo = new FormatOptions(volumeFoOrigin);
                volumeFo.DisplayUnits = DisplayUnitType.DUT_CUBIC_METERS;
                volumeFo.UnitSymbol = UnitSymbolType.UST_NONE; //UnitSymbolType.UST_M_SUP_3;
                volumeFo.Accuracy = 0.000000001;
                m_units.SetFormatOptions(UnitType.UT_Volume, volumeFo);

                using (Transaction tran = new Transaction(m_doc, "SetUnits"))
                {
                    tran.Start();
                    m_doc.SetUnits(m_units);
                    tran.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("设置项目单位失败！" + ex.Message);
                return false;
            }
        }

        public bool RecoverOriginUnits()
        {
            try
            {
                m_units.SetFormatOptions(UnitType.UT_Length, lengthFoOrigin);
                m_units.SetFormatOptions(UnitType.UT_Area, areaFoOrigin);
                m_units.SetFormatOptions(UnitType.UT_Volume, volumeFoOrigin);

                using (Autodesk.Revit.DB.Transaction tran = new Autodesk.Revit.DB.Transaction(m_doc, "SetUnits"))
                {
                    tran.Start();
                    m_doc.SetUnits(m_units);
                    tran.Commit();
                }

                return true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("还原项目单位失败！" + ex.Message);
                return false;
            }
        }
    }
}
