using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exporter
{
    public class ExportSetting
    {
        public ExportSetting()
        {
            this.SystemSetting = new SystemSetting();
            this.ParkingExportSetting = new ParkingExportSetting();
        }

        public SystemSetting SystemSetting
        {
            get;
            set;
        }

        public ParkingExportSetting ParkingExportSetting
        {
            get;
            set;
        }
    }

    public class ParkingExportSetting
    {
        public ParkingExportSetting()
        {
            this.PropertyName = "";
            this.Width = 720.0;
            this.Height = 480;
        }

        public string PropertyName
        {
            get;
            set;
        }

        public double Width
        {
            get;
            set;
        }

        public double Height
        {
            get;
            set;
        }
    }


    public class SystemSetting
    {
        public SystemSetting()
        {
            this.IsModifyUnit = true;
            this.IsExportLinkModel = false;
            this.IsExportProperty = true;
            this.IsExportGrid = false;
            this.IsOptimizeCylinderFace = true;
            this.IsOriginMaterial = false;
            this.IsUserDefineFormat = false;
            this.IsExportTextureFile = false;
            this.IsExportRebar = false;
            this.IsMergeFace = true;
            this.ExportFilePath = string.Empty;
            this.DefaultLayerName = string.Empty;
        }

        public bool IsModifyUnit { get; set; }

        public bool IsExportLinkModel { get; set; }

        public bool IsExportProperty { get; set; }

        public bool IsExportGrid { get; set; }

        public bool IsOptimizeCylinderFace { get; set; }

        public bool IsOriginMaterial { get; set; }

        public bool IsUserDefineFormat { get; set; }

        public bool IsExportTextureFile { get; set; }
        
        public bool IsMergeFace { get; set; }

        public string DefaultLayerName { get; set; }

        public string ExportFilePath { get; set; }

        public bool IsExportRebar { get; set; }
    }
}
