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
            this.IsExportTextureFile = false;
            this.IsExportRebar = false;
            this.IsExportWallSideArea = false;
            this.IsMergeFace = true;
            this.IsMoveBlkXpropertyToInsert = true;
            this.ExportFilePath = string.Empty;
            this.DefaultLayerName = string.Empty;
            _fileType = FileTypeEnum.bim;
        }

        public enum FileTypeEnum
        {
            bim = 0,
            sdp,
            bfa
        }

        public bool IsModifyUnit { get; set; }

        public bool IsExportLinkModel { get; set; }

        public bool IsExportProperty { get; set; }

        public bool IsExportGrid { get; set; }

        public bool IsExportWallSideArea { get; set; }

        public bool IsOptimizeCylinderFace { get; set; }

        public bool IsOriginMaterial { get; set; }

        public bool IsUserDefineFormat => this.FileType != FileTypeEnum.bim;

        public bool IsExportTextureFile { get; set; }

        public bool IsMoveBlkXpropertyToInsert { get; set; }
        
        public bool IsMergeFace { get; set; }

        public string DefaultLayerName { get; set; }

        public string ExportFilePath { get; set; }

        public bool IsExportRebar { get; set; }

        private FileTypeEnum _fileType;
        public FileTypeEnum FileType
        {
            get
            {
                return _fileType;
            }

            set
            {
                _fileType = value;
                if (FileType != FileTypeEnum.bim)
                    IsExportRebar = false;
            }
        }

        public string GetFileExtension()
        {
            switch (this.FileType)
            {
                case FileTypeEnum.bim:
                    return ".bim";
                case FileTypeEnum.sdp:
                    return ".sdp";
                case FileTypeEnum.bfa:
                    return ".bfa";
                default:
                    return ".bim";
            }
        }
    }
}
