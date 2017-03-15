using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Exporter
{
    class ElementColorOverride
    {
        public Document CurrentDocument
        {
            get;
            set;
        }

        public View3D CurrentView
        {
            get;
            set;
        }

        private List<MaterialData> m_materials = new List<MaterialData>();
        private Dictionary<ElementId, string> m_dictElemColor = new Dictionary<ElementId, string>();


        public bool ArrangeElemlentColor(Document doc, View3D view)
        {
            this.CurrentDocument = doc;
            this.CurrentView = view;

            m_materials.Clear();
            m_dictElemColor.Clear();

            GatherFilterColorOverride();
            GatherCategoryColorOverride();

            return true;
        }

        public List<MaterialData> GetMaterials()
        {
            return m_materials;
        }

        public Dictionary<ElementId, string> GetElementColorSetting()
        {
            return m_dictElemColor;
        }

        private void AddMaterial(string name, Color color)
        {
            MaterialData tmp = m_materials.Find(mat => mat.Name.ToUpper() == name.ToUpper());
            if (tmp != null)
                return;

            MaterialData material = new MaterialData();
            material.Color = new ColorData(color.Red, color.Green, color.Blue);
            material.Name = name;
            m_materials.Add(material);
        }

        private bool CanPassFilterRules(ParameterFilterElement filter, ElementId elemId)
        {
            Element elem = CurrentDocument.GetElement(elemId);
            if (elem == null)
                return false;

            bool bPassed = true;
            foreach (FilterRule rule in filter.GetRules())
            {
                if (!rule.ElementPasses(elem))
                {
                    bPassed = false;
                    break;
                }
            }

            return bPassed;
        }

        private void GatherFilterColorOverride()
        {
            if (CurrentDocument == null || CurrentView == null)
                return;

            ICollection<ElementId> filterIds = null;

            try
            {
                filterIds = CurrentView.GetFilters();
                if (filterIds == null || filterIds.Count < 1)
                    return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            foreach (ElementId idFilter in filterIds)
            {
                try
                {
                    ParameterFilterElement filter = CurrentDocument.GetElement(idFilter) as ParameterFilterElement;
                    if (filter == null)
                        continue;

                    OverrideGraphicSettings overrideSettings = CurrentView.GetFilterOverrides(idFilter);
                    if (overrideSettings == null)
                        continue;

                    if (!overrideSettings.ProjectionFillColor.IsValid)
                        continue;

                    AddMaterial(filter.Name, overrideSettings.ProjectionFillColor);

                    List<ElementId> listElem = new List<ElementId>();
                    var cids = filter.GetCategories();
                    if (cids == null)
                        continue;

                    foreach (ElementId catID in cids)
                    {
                        FilteredElementCollector collector = (new FilteredElementCollector(CurrentDocument, CurrentView.Id)).OfCategoryId(catID);
                        var lstid = collector.ToElementIds() as List<ElementId>;
                        if (lstid == null)
                            continue;

                        listElem.AddRange(lstid);
                    }

                    List<ElementId> elemPassed = new List<ElementId>();
                    foreach (ElementId elemId in listElem)
                        if (CanPassFilterRules(filter, elemId))
                            elemPassed.Add(elemId);

                    foreach (ElementId elemId in elemPassed)
                    {
                        if (!m_dictElemColor.ContainsKey(elemId))
                            m_dictElemColor.Add(elemId, filter.Name);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
                
            }
        }

        private void GatherCategoryColorOverride()
        {
            if (CurrentDocument == null || CurrentView == null)
                return;

            foreach (Category cat in CurrentDocument.Settings.Categories)
            {
                OverrideGraphicSettings setting = CurrentView.GetCategoryOverrides(cat.Id);
                if (setting == null)
                    continue;

                if (!setting.ProjectionFillColor.IsValid)
                    continue;

                string name = "Category_" + cat.Name;
                AddMaterial(name, setting.ProjectionFillColor);

                FilteredElementCollector collector = (new FilteredElementCollector(CurrentDocument, CurrentView.Id)).OfCategoryId(cat.Id);
                List<ElementId> elemIDs = collector.ToElementIds() as List<ElementId>;

                foreach (ElementId elemId in elemIDs)
                {
                    if (!m_dictElemColor.ContainsKey(elemId))
                        m_dictElemColor.Add(elemId, name);
                }
            }
        }
    }
}
