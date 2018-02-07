using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Exporter
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();
            this.ExportSetting = new ExportSetting();
        }

        public ExportSetting ExportSetting
        {
            get;
            set;
        }

        private void btnStartExport_Click(object sender, EventArgs e)
        {
            string message = string.Empty;
            if (!UI2Setting(ref message))
            {
                MessageBox.Show(message);
                return;
            }
            
            DialogResult = DialogResult.OK;
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            Setting2UI();
        }

        private void InitFileTypeCombo()
        {
            comboFileType.Items.Add(".bim");
            comboFileType.Items.Add(".sdp");
            comboFileType.Items.Add(".wfa");

            comboFileType.SelectedIndex = 0;
        }

        private void Setting2UI()
        {
            InitFileTypeCombo();

            chkUnit.Checked = this.ExportSetting.SystemSetting.IsModifyUnit;
            chkExportLink.Checked = this.ExportSetting.SystemSetting.IsExportLinkModel;
            chkExportProperty.Checked = this.ExportSetting.SystemSetting.IsExportProperty;
            chkExportGrid.Checked = this.ExportSetting.SystemSetting.IsExportGrid;
            chkWallSideArea.Checked = this.ExportSetting.SystemSetting.IsExportWallSideArea;
            chkOptimize.Checked = this.ExportSetting.SystemSetting.IsOptimizeCylinderFace;
            chkOriginMaterial.Checked = this.ExportSetting.SystemSetting.IsOriginMaterial;
            //chkUserDefineFormat.Checked = this.ExportSetting.SystemSetting.IsUserDefineFormat;
            chkTextureFile.Checked = this.ExportSetting.SystemSetting.IsExportTextureFile;
            chkRebar.Checked = this.ExportSetting.SystemSetting.IsExportRebar;
            chkMerge.Checked = this.ExportSetting.SystemSetting.IsMergeFace;
            chkMoveBlkProps.Checked = this.ExportSetting.SystemSetting.IsMoveBlkXpropertyToInsert;
            textFilePath.Text = this.ExportSetting.SystemSetting.ExportFilePath;

            textPropertyName.Text = this.ExportSetting.ParkingExportSetting.PropertyName;
            textWidth.Text = this.ExportSetting.ParkingExportSetting.Width.ToString();
            textHeight.Text = this.ExportSetting.ParkingExportSetting.Height.ToString();

            var fileExt = this.ExportSetting.SystemSetting.GetFileExtension();
            comboFileType.SelectedIndex = comboFileType.Items.IndexOf(fileExt);
        }

        private bool UI2Setting(ref string message)
        {
            this.ExportSetting.SystemSetting.IsModifyUnit = chkUnit.Checked;
            this.ExportSetting.SystemSetting.IsExportLinkModel = chkExportLink.Checked;
            this.ExportSetting.SystemSetting.IsExportProperty = chkExportProperty.Checked;
            this.ExportSetting.SystemSetting.IsExportGrid = chkExportGrid.Checked;
            this.ExportSetting.SystemSetting.IsOptimizeCylinderFace = chkOptimize.Checked;
            this.ExportSetting.SystemSetting.IsOriginMaterial = chkOriginMaterial.Checked;
            //this.ExportSetting.SystemSetting.IsUserDefineFormat = chkUserDefineFormat.Checked;
            this.ExportSetting.SystemSetting.IsExportTextureFile = chkTextureFile.Checked;
            this.ExportSetting.SystemSetting.IsExportRebar = chkRebar.Checked;
            this.ExportSetting.SystemSetting.IsMergeFace = chkMerge.Checked;
            this.ExportSetting.SystemSetting.IsMoveBlkXpropertyToInsert = chkMoveBlkProps.Checked;
            this.ExportSetting.SystemSetting.IsExportWallSideArea = chkWallSideArea.Checked;

            string strValue = textFilePath.Text.Trim();
            if (strValue.Length < 1)
            {
                message = "请先设置要到导出的文件路径";
                return false;
            }

            this.ExportSetting.SystemSetting.ExportFilePath = strValue;
            this.ExportSetting.ParkingExportSetting.PropertyName = textPropertyName.Text.Trim();

            double dValue = -1;
            if (!double.TryParse(textWidth.Text.Trim(), out dValue))
            {
                message = "编号宽设置不正确！";
                return false;
            }
            else
            {
                if (dValue <= 0)
                {
                    message = "编号宽不可小于0";
                    return false;
                }

                this.ExportSetting.ParkingExportSetting.Width = dValue;
            }

            if (!double.TryParse(textHeight.Text.Trim(), out dValue))
            {
                message = "编号高设置不正确！";
                return false;
            }
            else
            {
                if (dValue <= 0)
                {
                    message = "编号高不可小于0";
                    return false;
                }

                this.ExportSetting.ParkingExportSetting.Height = dValue;
            }

            this.ExportSetting.SystemSetting.DefaultLayerName = txtDefaultLayer.Text.Trim();

            return true;
        }

        private void btnSaveFile_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Filter = $"模型文件(*{this.ExportSetting.SystemSetting.GetFileExtension()})|*{this.ExportSetting.SystemSetting.GetFileExtension()}";
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            textFilePath.Text = dlg.FileName;
        }
        

        private void chkRebar_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRebar.Checked && this.ExportSetting.SystemSetting.FileType != SystemSetting.FileTypeEnum.bim)
                chkRebar.Checked = false;
        }

        private void comboFileType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var file = textFilePath.Text.Trim();
            if (string.IsNullOrEmpty(file))
                return;

            if (comboFileType.SelectedIndex != 0)
                chkRebar.Checked = false;

            var fileDir = Path.GetDirectoryName(file);
            var fileName = Path.GetFileNameWithoutExtension(file);

            textFilePath.Text = fileDir + "\\" + fileName + this.ExportSetting.SystemSetting.GetFileExtension();
        }
    }
}
