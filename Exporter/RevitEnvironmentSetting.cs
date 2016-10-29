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
    class RevitEnvironmentSetting
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
                foNew.UnitSymbol = UnitSymbolType.UST_MM;
                foNew.Accuracy = 0.01;
                m_units.SetFormatOptions(UnitType.UT_Length, foNew);

                areaFoOrigin = m_units.GetFormatOptions(UnitType.UT_Area);
                foNew = new FormatOptions(areaFoOrigin);
                foNew.DisplayUnits = DisplayUnitType.DUT_SQUARE_METERS;
                foNew.UnitSymbol = UnitSymbolType.UST_M_SUP_2;
                foNew.Accuracy = 0.0001;
                m_units.SetFormatOptions(UnitType.UT_Area, foNew);

                volumeFoOrigin = m_units.GetFormatOptions(UnitType.UT_Volume);
                foNew = new FormatOptions(volumeFoOrigin);
                foNew.DisplayUnits = DisplayUnitType.DUT_CUBIC_METERS;
                foNew.UnitSymbol = UnitSymbolType.UST_M_SUP_3;
                foNew.Accuracy = 0.0001;
                m_units.SetFormatOptions(UnitType.UT_Volume, foNew);

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
