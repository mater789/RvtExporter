using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using System.IO;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.Utility;

namespace Exporter
{
    class ModelExportContext : IExportContext
    {
        #region 属性

        private Document m_doc = null;
        private bool m_bIsCanceled = false;

        private ElementId m_curMaterialId = ElementId.InvalidElementId;
        private List<ElementId> m_listMaterialID = new List<ElementId>();

        /// <summary>
        /// 提前收集好的材质信息
        /// </summary>
        public List<MaterialData> ExtraMaterial
        {
            get;
            set;
        }

        public AssetSet BuiltInMaterialLibraryAsset { get; set; }

        /// <summary>
        /// 额外的实体颜色设置
        /// </summary>
        public Dictionary<ElementId, string> ExtraElementColorSetting
        {
            get;
            set;
        }

        public List<LightData> Lights { get; set; }

        /// <summary>
        /// 所使用的块列表
        /// </summary>
        private Dictionary<string, BlockData> m_dictBlock = new Dictionary<string, BlockData>();

        public Dictionary<string, BlockData> DictBlocks { get { return m_dictBlock; } }

        /// <summary>
        /// 整个模型涉及到的材质列表
        /// </summary>
        private List<MaterialData> m_listMaterial = new List<MaterialData>();
        public List<MaterialData> Materials
        {
            get { return m_listMaterial; }
            set { m_listMaterial = value; }
        }

        /// <summary>
        /// 是否将模型空间中的实体包装成为单独的块。
        /// </summary>
        private bool m_bPackageEntityToBlock = true;
        public bool IsPackageEntityToBlock
        {
            get { return m_bPackageEntityToBlock; }
            set { m_bPackageEntityToBlock = value; }
        }

        /// <summary>
        /// 当前ElementId
        /// </summary>
        private Stack<ElementId> m_stackElement = new Stack<ElementId>();
        ElementId CurrentElementId
        {
            get
            {
                return (m_stackElement.Count > 0) ? m_stackElement.Peek() : ElementId.InvalidElementId;
            }
        }

        /// <summary>
        /// 当前Element
        /// </summary>
        Element CurrentElement
        {
            get
            {
                return m_doc.GetElement(CurrentElementId);
            }
        }

        /// <summary>
        /// 当前的变换
        /// </summary>
        private Stack<Transform> m_stackTrans = new Stack<Transform>();
        Transform CurrentTransform
        {
            get
            {
                return m_stackTrans.Peek();
            }
        }


        /// <summary>
        /// 最终输出的一个模型空间block
        /// </summary>
        public BlockData ModelSpaceBlock
        {
            get;
            set;
        }

        /// <summary>
        /// 嵌套块的堆栈
        /// </summary>
        Stack<BlockData> m_stackBlock = new Stack<BlockData>();

        /// <summary>
        /// 堆栈中的当前块
        /// </summary>
        BlockData CurrentBlockData
        {
            get
            {
                return m_stackBlock.Count < 1 ? null : m_stackBlock.Peek();
            }
        }

        private bool m_bIsExportProperty = true;
        /// <summary>
        /// 是否要导出属性信息
        /// </summary>
        public bool IsExportProperty
        {
            set
            {
                m_bIsExportProperty = value;
            }

            get
            {
                return m_bIsExportProperty;
            }
        }

        private bool m_bIsExportCylinderProperty = false;
        /// <summary>
        /// 导出属性时，是否要导出圆柱面的向量信息。
        /// </summary>
        public bool IsExportCylinderProperty
        {
            set
            {
                m_bIsExportCylinderProperty = value;
            }
            get
            {
                return m_bIsExportCylinderProperty;
            }
        }

        /// <summary>
        /// 用来判断是不是有个新Element开始导出了
        /// </summary>
        private bool m_bIsNewElementBegin = false;


        private bool m_bIsExportLinkModel = false;
        /// <summary>
        /// 是否导出链接模型
        /// </summary>
        public bool IsExportLinkModel
        {
            get { return m_bIsExportLinkModel; }
            set { m_bIsExportLinkModel = value; }
        }

        /// <summary>
        /// 当前Element的材质信息
        /// </summary>
        private MaterialData m_curElementMaterialData = null;

        public Dictionary<string, LocationData> InstanceLocation { get; set; }

        #endregion


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="doc"></param>
        public ModelExportContext(Document doc)
        {
            this.m_doc = doc;
            this.m_stackTrans.Push(Transform.Identity);
        }

        /// <summary>
        /// 从polymesh中获取三角面片信息
        /// </summary>
        /// <param name="polyMesh"></param>
        /// <returns></returns>
        private List<TriangleData> GetTriangleDataFromPolymesh(PolymeshTopology polyMesh, bool bIsOrignPosition = false)
        {
            List<TriangleData> listTri = new List<TriangleData>();

            XYZ point;
            for (int i = 0; i < polyMesh.NumberOfFacets; i++)
            {
                point = polyMesh.GetPoint(polyMesh.GetFacet(i).V1);
                if (!bIsOrignPosition)
                    point = CurrentTransform.OfPoint(point);
                PointData pt1 = new PointData(point.X, point.Y, point.Z);

                point = polyMesh.GetPoint(polyMesh.GetFacet(i).V2);
                if (!bIsOrignPosition)
                    point = CurrentTransform.OfPoint(point);
                PointData pt2 = new PointData(point.X, point.Y, point.Z);

                point = polyMesh.GetPoint(polyMesh.GetFacet(i).V3);
                if (!bIsOrignPosition)
                    point = CurrentTransform.OfPoint(point);
                PointData pt3 = new PointData(point.X, point.Y, point.Z);

                listTri.Add(new TriangleData(pt1, pt2, pt3));
            }

            return listTri;
        }

        /// <summary>
        /// 从polymesh中获取三角形顶点索引数据
        /// </summary>
        /// <param name="polymesh"></param>
        /// <returns></returns>
        private List<TriangleIndexData> GetTriangleIndexFromPolymesh(PolymeshTopology polymesh)
        {
            List<TriangleIndexData> listIndex = new List<TriangleIndexData>();
            for (int i = 0; i < polymesh.NumberOfFacets; i++)
                listIndex.Add(new TriangleIndexData(polymesh.GetFacet(i).V1, polymesh.GetFacet(i).V2, polymesh.GetFacet(i).V3));

            return listIndex;
        }

        /// <summary>
        /// 从polymesh中获取顶点列表
        /// </summary>
        /// <param name="polymesh"></param>
        /// <returns></returns>
        private List<PointData> GetVertexesFromPolymesh(PolymeshTopology polymesh)
        {
            List<PointData> listPoint = new List<PointData>();
            for (int i = 0; i < polymesh.NumberOfPoints; i++)
                listPoint.Add(new PointData(polymesh.GetPoint(i).X, polymesh.GetPoint(i).Y, polymesh.GetPoint(i).Z));

            return listPoint;
        }

        private MeshData GetMeshDataFromPolymesh(PolymeshTopology polymesh)
        {
            var mesh = new MeshData();
            mesh.TriangleIndexes = new List<TriangleIndexData>();
            mesh.Vertexes = new List<PointData>();
            mesh.Normals = new List<PointData>();
            mesh.TextureUVs = new List<TextureUV>();

            for (int i = 0; i < polymesh.NumberOfFacets; i++)
            {
                var facet = polymesh.GetFacet(i);
                mesh.TriangleIndexes.Add(new TriangleIndexData(facet.V1, facet.V2, facet.V3));
            }

            var oneNormalPerpoint = polymesh.DistributionOfNormals == DistributionOfNormals.AtEachPoint;
            for (int i = 0; i < polymesh.NumberOfPoints; i++)
            {
                mesh.Vertexes.Add(new PointData(polymesh.GetPoint(i).X, polymesh.GetPoint(i).Y, polymesh.GetPoint(i).Z));
                if (oneNormalPerpoint)
                {
                    var normal = polymesh.GetNormal(i);
                    //mesh.Normals.Add(new PointData(normal.X, normal.Y, normal.Z));
                }
            }

            if (polymesh.DistributionOfNormals == DistributionOfNormals.OnePerFace)
            {
                var normal = polymesh.GetNormal(0);
                //mesh.Normals.Add(new PointData(normal.X, normal.Y, normal.Z));
            }

            for (int i = 0; i < polymesh.NumberOfUVs; i++)
            {
                var tmpuv = polymesh.GetUV(i);
                mesh.TextureUVs.Add(new TextureUV(tmpuv.U, tmpuv.V));
            }

            return mesh;
        }

        private string GetFamilyName(Element elem)
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
        /// 从实体中得到属性
        /// </summary>
        /// <param name="elem"></param>
        /// <returns>返回的是一个 属性组名-同组下的属性列表 字典</returns>
        private Dictionary<string, List<PropertyData>> GetPropertiesAndLocationFromElement(Element elem)
        {
            Dictionary<string, List<PropertyData>> dictProperties = new Dictionary<string, List<PropertyData>>();
            if (!m_bIsExportProperty)
                return dictProperties;

            // 属性中添加族和类型信息
            var internalProps = new List<PropertyData>();
            internalProps.Add(new PropertyData { GroupName = "#Internal", Name = "#name", Value = CurrentElement.Name });
            if (CurrentElement.Category != null)
                internalProps.Add(new PropertyData { GroupName = "#Internal", Name = "#category", Value = CurrentElement.Category.Name });
            internalProps.Add(new PropertyData { GroupName = "#Internal", Name = "#guid", Value = CurrentElement.UniqueId });
            var fname = GetFamilyName(elem);
            if (!string.IsNullOrEmpty(fname) && !(CurrentElement is MEPCurve))
                internalProps.Add(new PropertyData { GroupName = "#Internal", Name = "#family", Value = fname });
            dictProperties["#Internal"] = internalProps;

            // 读取位置信息
            if (InstanceLocation == null)
                InstanceLocation = new Dictionary<string, LocationData>();

            if (!(elem is MEPCurve) && elem.Location is LocationCurve && elem is FamilyInstance)
            {
                var curve = (elem.Location as LocationCurve).Curve;
                var fi = (elem as FamilyInstance);
                var curvedata = new CurveData();
                if (curve is Arc)
                {
                    var arc = (Arc)curve;
                    curvedata.IsArc = true;
                    curvedata.Center = new PointData(arc.Center.X, arc.Center.Y, arc.Center.Z);
                    curvedata.Normal = new PointData(arc.Normal.X, arc.Normal.Y, arc.Normal.Z);
                    curvedata.StartParameter = arc.GetEndParameter(0);
                    curvedata.EndParameter = arc.GetEndParameter(1);
                    curvedata.Radius = arc.Radius;
                    var pt = arc.GetEndPoint(0);
                    curvedata.Points.Add(new PointData(pt.X, pt.Y, pt.Z));
                    pt = arc.GetEndPoint(1);
                    curvedata.Points.Add(new PointData(pt.X, pt.Y, pt.Z));
                }
                else
                {
                    curvedata.IsArc = false;
                    var points = curve.Tessellate();
                    foreach (var point in points)
                        curvedata.Points.Add(new PointData(point.X, point.Y, point.Z));
                }

                var faceV = new PointData(fi.FacingOrientation.X, fi.FacingOrientation.Y, fi.FacingOrientation.Z);
                var handV = new PointData(fi.HandOrientation.X, fi.HandOrientation.Y, fi.HandOrientation.Z);

                InstanceLocation[CurrentElement.UniqueId] = new LocationData { LocationCurve = curvedata, FaceVec = faceV, HandVec = handV };
            }


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

            if (m_bIsExportCylinderProperty)
            {
                List<Solid> slds = GetSolidFromElement(elem);
                List<string> cyFacesInfo = new List<string>();
                foreach (Solid sld in slds)
                    cyFacesInfo.AddRange(GetCylinderFaceInfoFromSolid(sld));
                dictProperties["CylinderFaceData"] = GetUniqueCylinderFacePropertyData(cyFacesInfo);
            }

            return dictProperties;
        }

        /// <summary>
        /// 去重
        /// </summary>
        /// <param name="listString"></param>
        /// <returns></returns>
        private List<PropertyData> GetUniqueCylinderFacePropertyData(List<string> listString)
        {
            int nCount = 1;
            List<PropertyData> listProp = new List<PropertyData>();
            List<string> result = new List<string>();
            foreach (string str in listString)
            {
                if (!result.Contains(str))
                {
                    result.Add(str);

                    PropertyData proData = new PropertyData();
                    proData.Name = "POINTVECTOR" + nCount.ToString();
                    proData.GroupName = "CylinderFaceData";
                    proData.Value = str;

                    listProp.Add(proData);
                    nCount++;
                }
            }

            return listProp;
        }

        private void AddMaterial(ElementId materialID)
        {
            AddMaterial(m_doc.GetElement(materialID) as Material);
        }

        private void AddMaterial(Material material)
        {
            if (material == null)
            {
                Color col = new Color(255, 255, 255);
                AddMaterial("NoMaterial", col);
            }
            else
            {
                if (m_listMaterial.Any(mtl => mtl.Name == material.Name))
                    return;

                var diffuserMap = string.Empty;
                var bumpMap = string.Empty;
                double scaleX, scaleY, metallic, smoothness;
                Tools.GetMaterialTexture(material, BuiltInMaterialLibraryAsset, out diffuserMap, out bumpMap, out scaleX, out scaleY, out metallic, out smoothness);
                AddMaterial(material.Name, material.Color, material.Transparency, diffuserMap, bumpMap, scaleX, scaleY, metallic, smoothness);
            }
        }

        private void AddMaterial(string name, Color color, int transparency = 0, string diffuseMap = "", string bumpMap = "", double scaleX = 1.0, double scaleY = 1.0, double metallic = 0.0, double smoothness = 0.5)
        {
            if (m_listMaterial.Any(mtl => mtl.Name == name))
                return;

            MaterialData matdata = new MaterialData();
            matdata.Transparency = transparency;
            matdata.Name = name;
            matdata.DiffuseMap = diffuseMap;
            matdata.BumpMap = bumpMap;
            matdata.TextureScaleX = scaleX;
            matdata.TextureScaleY = scaleY;
            matdata.Metallic = metallic;
            matdata.Smoothness = smoothness;


            if (color.IsValid)
            {
                matdata.Color.Blue = color.Blue;
                matdata.Color.Green = color.Green;
                matdata.Color.Red = color.Red;
            }

            AddMaterial(matdata);
        }

        private void AddMaterial(MaterialData materialData)
        {
            MaterialData matData = m_listMaterial.Find(mat => mat.Name == materialData.Name);
            if (matData != null)
                return;

            m_listMaterial.Add(materialData);
        }

        private TransformData GetTransData(Transform trans)
        {
            return new TransformData
            {
                BasisX = new PointData(trans.BasisX.X, trans.BasisX.Y, trans.BasisX.Z),
                BasisY = new PointData(trans.BasisY.X, trans.BasisY.Y, trans.BasisY.Z),
                BasisZ = new PointData(trans.BasisZ.X, trans.BasisZ.Y, trans.BasisZ.Z),
                Origin = new PointData(trans.Origin.X, trans.Origin.Y, trans.Origin.Z)
            };
        }

        /// <summary>
        /// 处理圆管管道
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="block"></param>
        /// <param name="materialName"></param>
        /// <returns></returns>
        private bool ProcessRoundedPipeEntity(Element elem, BlockData block, string materialName)
        {
            MEPCurve curve = elem as MEPCurve;
            if (curve == null)
                return false;

            LocationCurve locationCurve = curve.Location as LocationCurve;
            if (locationCurve == null)
                return false;

            double diameter = 0.0;
            try
            {
                diameter = curve.Diameter;
            }
            catch (System.Exception ex)
            {
                return false;
            }

            Line line = locationCurve.Curve as Line;
            if (line == null)
                return false;

            // 如果这个块已经被设置过~ 那么跳过
            if (block.IsPipe && block.PipeInfo != null)
                return true;

            if (elem is PipeInsulation) // 如果是管道的外包层 (外包层的坐标总和它依赖的管道坐标不准，所以判断出如果是外包层，那么直接用它所依赖的管道的坐标)
            {
                var pi = elem as PipeInsulation;
                var hostPipe = m_doc.GetElement(pi.HostElementId);
                if (hostPipe == null)
                    return false;

                MEPCurve hostCurve = hostPipe as MEPCurve;
                if (hostCurve == null)
                    return false;

                line = (hostCurve.Location as LocationCurve).Curve as Line;
                if (line == null)
                    return false;
            }

            XYZ ptStart = line.GetEndPoint(0);
            XYZ ptEnd = line.GetEndPoint(1);

            block.IsPipe = true;
            block.PipeInfo = new PipeInfo()
            {
                Diameter = diameter,
                PtStart = new PointData(ptStart.X, ptStart.Y, ptStart.Z),
                PtEnd = new PointData(ptEnd.X, ptEnd.Y, ptEnd.Z),
                MaterialName = materialName
            };

            return true;
        }

        /// <summary>
        /// 从实体中得到Solid
        /// </summary>
        /// <param name="geoElement"></param>
        /// <returns></returns>
        private List<Solid> GetSolidFromElement(Element elem)
        {
            Options opt = new Options();
            opt.DetailLevel = ViewDetailLevel.Fine;
            GeometryElement gElem = elem.get_Geometry(opt);

            List<Solid> listData = new List<Solid>();
            foreach (GeometryObject obj in gElem)
            {
                if (obj is Solid)
                {
                    Solid solid = obj as Solid;
                    if (solid.Faces.Size > 1)
                        listData.Add(solid);
                }
                else if (obj is GeometryInstance)
                {
                    GeometryInstance geoInstance = obj as GeometryInstance;
                    GeometryElement geo = geoInstance.GetInstanceGeometry();
                    foreach (GeometryObject obj2 in geo)
                    {
                        if (obj2 is Solid)
                        {
                            Solid solid2 = obj2 as Solid;
                            if (solid2.Faces.Size > 1)
                                listData.Add(solid2);
                        }
                    }
                }
            }

            return listData;
        }

        /// <summary>
        /// 从solid中得到圆柱面信息
        /// </summary>
        /// <param name="sld"></param>
        /// <returns></returns>
        private List<string> GetCylinderFaceInfoFromSolid(Solid sld)
        {
            List<string> faces = new List<string>();
            if (sld == null)
                return faces;

            foreach (Face fc in sld.Faces)
            {
                if (fc is CylindricalFace)
                {
                    CylindricalFace cfc = fc as CylindricalFace;
                    string strTmp = cfc.Origin.X + "," +
                        cfc.Origin.Y + "," +
                        cfc.Origin.Z + "," +
                        cfc.Axis.X + "," +
                        cfc.Axis.Y + "," +
                        cfc.Axis.Z;
                    faces.Add(strTmp);
                }
            }

            return faces;
        }

        #region IExportContext 接口实现

        public bool Start()
        {
            m_listMaterialID.Clear();
            m_dictBlock.Clear();
            m_stackBlock.Clear();

            ModelSpaceBlock = new BlockData();
            ModelSpaceBlock.Name = "MODEL-SPACE";
            m_dictBlock.Add("MODEL-SPACE", ModelSpaceBlock);
            m_stackBlock.Push(ModelSpaceBlock);

            return true;
        }

        public void Finish()
        {

        }

        private MaterialData m_curOriginMaterialData = null;
        public void OnMaterial(MaterialNode node)
        {
            m_curOriginMaterialData = null;
            m_curMaterialId = node.MaterialId;
            if (node.Color.IsValid)
            {
                m_curOriginMaterialData = new MaterialData();
                m_curOriginMaterialData.Color.Red = node.Color.Red;
                m_curOriginMaterialData.Color.Green = node.Color.Green;
                m_curOriginMaterialData.Color.Blue = node.Color.Blue;
                m_curOriginMaterialData.Transparency = (int)(255 * node.Transparency);
                m_curOriginMaterialData.Name = "Color_" + node.Color.Red.ToString() + "-" +
                    node.Color.Green.ToString() + "-" +
                    node.Color.Blue.ToString() + "-" +
                    m_curOriginMaterialData.Transparency.ToString();
            }
        }

        /// <summary>
        /// 用来保存模型空间的直接块
        /// </summary>
        private BlockData _userCreateBlock = null;

        public void OnPolymesh(PolymeshTopology polyMesh)
        {
            // 如果是个模型空间的直接实体，包装一层块
            if (m_bPackageEntityToBlock && CurrentBlockData == ModelSpaceBlock)
            {
                if (m_bIsNewElementBegin)
                {
                    m_bIsNewElementBegin = false;

                    _userCreateBlock = new BlockData();

                    string elementName = CurrentElement == null ? string.Empty : CurrentElement.Name;
                    _userCreateBlock.Name = "D__" + elementName + "#" + CurrentElementId.ToString();

                    InsertData ins = new InsertData();
                    //ins.BlockRef = userCreateBlock;
                    ins.BlockName = _userCreateBlock.Name;
                    ins.TransMatrix = GetTransData(Transform.Identity);
                    ins.DictProperties = GetPropertiesAndLocationFromElement(CurrentElement);
                    CurrentBlockData.Inserts.Add(ins);
                    m_dictBlock.Add(_userCreateBlock.Name, _userCreateBlock);
                }
            }

            MeshData mesh = GetMeshDataFromPolymesh(polyMesh);

            // 以下的材质获取，优先选用 m_curElementMaterialData，再者选用m_curMaterialId指代的material，最后选用m_curOriginMaterialData，如果再为空则使用“NoMaterial”
            string materialName = string.Empty;
            if (m_curElementMaterialData != null)
            {
                AddMaterial(m_curElementMaterialData);
                materialName = m_curElementMaterialData.Name;
            }
            else
            {
                Material material = m_doc.GetElement(m_curMaterialId) as Material;
                if (material != null)
                {
                    AddMaterial(material);
                    materialName = material.Name;
                }
                else
                {
                    if (m_curOriginMaterialData != null)
                    {
                        AddMaterial(m_curOriginMaterialData);
                        materialName = m_curOriginMaterialData.Name;
                    }
                    else
                    {
                        AddMaterial(material);  // 其实是个空材质null
                        materialName = "NoMaterial";
                    }
                }
            }
            mesh.MaterialName = materialName;

            // 如果是模型空间的直接实体，包装成为一个特殊块
            if (m_bPackageEntityToBlock && CurrentBlockData == ModelSpaceBlock && _userCreateBlock != null)
            {
                // 如果处理管道失败
                if (!ProcessRoundedPipeEntity(CurrentElement, _userCreateBlock, materialName))
                    _userCreateBlock.Meshs.Add(mesh);
            }
            else
                CurrentBlockData.Meshs.Add(mesh);
        }

        public bool IsCanceled()
        {
            return m_bIsCanceled;
        }
        /*
        public void OnDaylightPortal(DaylightPortalNode node)
        {

        }*/

        public void OnRPC(RPCNode node)
        {

        }

        public RenderNodeAction OnViewBegin(ViewNode node)
        {
            return RenderNodeAction.Proceed;
        }

        public void OnViewEnd(ElementId elementId)
        {

        }

        public RenderNodeAction OnElementBegin(ElementId elementId)
        {
            try
            {
                m_stackElement.Push(elementId);

                /*
                var blkName = CurrentElement.Name + "#" + elementId.IntegerValue;
                var blk = new BlockData();
                blk.Name = blkName;
                m_dictBlock.Add(blkName, blk);

                var insert = new InsertData();
                insert.BlockName = blkName;
                insert.TransMatrix = GetTransData(Transform.Identity);
                insert.DictProperties = GetPropertiesAndLocationFromElement(CurrentElement);
                CurrentBlockData.Inserts.Add(insert);

                m_stackBlock.Push(blk);
                _insertStack.Push(insert);
                */



                // 新Element标记修改为true
                if (m_bPackageEntityToBlock)
                    m_bIsNewElementBegin = true;

                if (ExtraElementColorSetting != null)
                {
                    if (ExtraElementColorSetting.ContainsKey(elementId))
                        m_curElementMaterialData = ExtraMaterial.Find(mat => mat.Name == ExtraElementColorSetting[elementId]);
                }

                return RenderNodeAction.Proceed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return RenderNodeAction.Skip;
            }
        }

        public void OnElementEnd(ElementId elementId)
        {
            m_curElementMaterialData = null;
            m_stackElement.Pop();

            /*
            m_stackBlock.Pop();
            _insertStack.Pop();
            */
        }

        private Stack<InsertData> _insertStack = new Stack<InsertData>();

        public RenderNodeAction OnInstanceBegin(InstanceNode node)
        {
            m_stackTrans.Push(m_stackTrans.Peek().Multiply(node.GetTransform()));

            FamilySymbol fs = m_doc.GetElement(node.GetSymbolId()) as FamilySymbol;
            //string name = fs == null ? "null" : fs.Name;
            string blockName = node.NodeName + "#" + node.GetSymbolId().ToString();
            if (m_curElementMaterialData != null)   // 如果设置了颜色，则使用不同的块
                blockName += "#" + m_curElementMaterialData.Name;

            BlockData blk = null;
            bool bBlkAlreadyExist = m_dictBlock.TryGetValue(blockName, out blk);
            if (!bBlkAlreadyExist)
            {
                blk = new BlockData();
                blk.Name = blockName;
                m_dictBlock.Add(blk.Name, blk);
            }
            else
            {
                blk = new BlockData();
                blk.Name = blockName + "__tmpBlk__";
                m_dictBlock.Add(blk.Name, blk);
            }

            InsertData insertData = new InsertData();
            //insertData.BlockRef = blk;
            insertData.BlockName = blk.Name;
            insertData.TransMatrix = GetTransData(node.GetTransform());
            if (CurrentBlockData == ModelSpaceBlock)
                insertData.DictProperties = GetPropertiesAndLocationFromElement(CurrentElement);
            CurrentBlockData.Inserts.Add(insertData);

            _insertStack.Push(insertData);

            m_stackBlock.Push(blk);

            return RenderNodeAction.Proceed;
        }

        public void OnInstanceEnd(InstanceNode node)
        {
            var curIns = _insertStack.Peek();

            m_stackTrans.Pop();
            m_stackBlock.Pop();

            if (!m_dictBlock.ContainsKey(curIns.BlockName))
                return;

            var curBlk = m_dictBlock[curIns.BlockName];
            if (curBlk == null)
                return;


            if (curBlk.Meshs.Count < 1 && curBlk.Inserts.Count < 1 && !curBlk.IsPipe)
            {
                var has = CurrentBlockData.Inserts.Remove(curIns);
                m_dictBlock.Remove(curIns.BlockName);
            }
            else if (curIns.BlockName.EndsWith("__tmpBlk__"))
            {
                m_dictBlock.Remove(curIns.BlockName);

                var extName = curIns.BlockName.Substring(0, curIns.BlockName.Length - 10);
                if (m_dictBlock.ContainsKey(extName))
                    curIns.BlockName = extName;
            }
        }

        public RenderNodeAction OnFaceBegin(FaceNode node)
        {
            return RenderNodeAction.Proceed;
        }

        public void OnFaceEnd(FaceNode node)
        {
        }

        List<string> m_visitedLinks = new List<string>();
        public RenderNodeAction OnLinkBegin(LinkNode node)
        {
            m_stackTrans.Push(m_stackTrans.Peek().Multiply(node.GetTransform()));

            if (!m_bIsExportLinkModel)
                return RenderNodeAction.Skip;

            string pathName = node.GetDocument().PathName;
            string fileName = Path.GetFileNameWithoutExtension(pathName);
            string blockName = "link_" + fileName + "#" + node.GetSymbolId().ToString();

            bool bAlreadyVisited = false;
            if (m_visitedLinks.Contains(pathName))
                bAlreadyVisited = true;
            else
                m_visitedLinks.Add(pathName);

            BlockData blk = null;
            bool bBlkAlreadyExist = m_dictBlock.TryGetValue(blockName, out blk);
            if (!bBlkAlreadyExist)
            {
                blk = new BlockData();
                blk.Name = blockName;
                m_dictBlock.Add(blk.Name, blk);
            }

            if (!bAlreadyVisited)
            {
                InsertData insertData = new InsertData();
                //insertData.BlockRef = blk;
                insertData.BlockName = blk.Name;
                insertData.TransMatrix = GetTransData(node.GetTransform());
                if (CurrentBlockData == ModelSpaceBlock)
                    insertData.DictProperties = GetPropertiesAndLocationFromElement(CurrentElement);
                CurrentBlockData.Inserts.Add(insertData);
            }

            m_stackBlock.Push(blk);

            return (bBlkAlreadyExist || bAlreadyVisited) ? RenderNodeAction.Skip : RenderNodeAction.Proceed;
        }

        public void OnLinkEnd(LinkNode node)
        {
            m_stackTrans.Pop();

            if (m_bIsExportLinkModel)
                m_stackBlock.Pop();
        }

        public void OnLight(LightNode node)
        {
            if (Lights == null)
                Lights = new List<LightData>();

            if (CurrentElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_LightingFixtures)
            {
                var curIns = _insertStack.Peek();
                if (curIns == null)
                    return;

                var lightData = Autodesk.Revit.DB.Lighting.LightType.GetLightTypeFromInstance(m_doc, CurrentElement.Id);
                if (lightData == null)
                    return;

                var data = new LightData();
                data.Name = CurrentElement.Name + "__Light";
                data.Lumen = lightData.GetInitialIntensity().InitialIntensityValue;
                data.ColorTemperature = lightData.GetInitialColor().TemperatureValue;
                data.Color = new ColorData(lightData.ColorFilter.Red, lightData.ColorFilter.Green, lightData.ColorFilter.Blue);
                data.TransData = GetTransData(node.GetTransform());

                curIns.Light = data;
            }
        }

        #endregion
    }
}
