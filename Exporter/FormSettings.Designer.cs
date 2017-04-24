namespace Exporter
{
    partial class FormSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chkUnit = new System.Windows.Forms.CheckBox();
            this.chkExportLink = new System.Windows.Forms.CheckBox();
            this.chkExportProperty = new System.Windows.Forms.CheckBox();
            this.chkOptimize = new System.Windows.Forms.CheckBox();
            this.btnStartExport = new System.Windows.Forms.Button();
            this.textPropertyName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageSystem = new System.Windows.Forms.TabPage();
            this.chkUserDefineFormat = new System.Windows.Forms.CheckBox();
            this.chkExportGrid = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtDefaultLayer = new System.Windows.Forms.TextBox();
            this.chkOriginMaterial = new System.Windows.Forms.CheckBox();
            this.btnSaveFile = new System.Windows.Forms.Button();
            this.textFilePath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPageParking = new System.Windows.Forms.TabPage();
            this.label5 = new System.Windows.Forms.Label();
            this.textHeight = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textWidth = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkTextureFile = new System.Windows.Forms.CheckBox();
            this.tabControl1.SuspendLayout();
            this.tabPageSystem.SuspendLayout();
            this.tabPageParking.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkUnit
            // 
            this.chkUnit.AutoSize = true;
            this.chkUnit.Location = new System.Drawing.Point(6, 15);
            this.chkUnit.Name = "chkUnit";
            this.chkUnit.Size = new System.Drawing.Size(96, 16);
            this.chkUnit.TabIndex = 0;
            this.chkUnit.Text = "修改原始单位";
            this.chkUnit.UseVisualStyleBackColor = true;
            // 
            // chkExportLink
            // 
            this.chkExportLink.AutoSize = true;
            this.chkExportLink.Location = new System.Drawing.Point(6, 37);
            this.chkExportLink.Name = "chkExportLink";
            this.chkExportLink.Size = new System.Drawing.Size(96, 16);
            this.chkExportLink.TabIndex = 1;
            this.chkExportLink.Text = "导出链接模型";
            this.chkExportLink.UseVisualStyleBackColor = true;
            // 
            // chkExportProperty
            // 
            this.chkExportProperty.AutoSize = true;
            this.chkExportProperty.Checked = true;
            this.chkExportProperty.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportProperty.Location = new System.Drawing.Point(6, 59);
            this.chkExportProperty.Name = "chkExportProperty";
            this.chkExportProperty.Size = new System.Drawing.Size(96, 16);
            this.chkExportProperty.TabIndex = 2;
            this.chkExportProperty.Text = "导出属性数据";
            this.chkExportProperty.UseVisualStyleBackColor = true;
            // 
            // chkOptimize
            // 
            this.chkOptimize.AutoSize = true;
            this.chkOptimize.Checked = true;
            this.chkOptimize.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOptimize.Location = new System.Drawing.Point(6, 81);
            this.chkOptimize.Name = "chkOptimize";
            this.chkOptimize.Size = new System.Drawing.Size(96, 16);
            this.chkOptimize.TabIndex = 3;
            this.chkOptimize.Text = "启用数据优化";
            this.chkOptimize.UseVisualStyleBackColor = true;
            // 
            // btnStartExport
            // 
            this.btnStartExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartExport.Location = new System.Drawing.Point(296, 214);
            this.btnStartExport.Name = "btnStartExport";
            this.btnStartExport.Size = new System.Drawing.Size(110, 23);
            this.btnStartExport.TabIndex = 4;
            this.btnStartExport.Text = "开始导出";
            this.btnStartExport.UseVisualStyleBackColor = true;
            this.btnStartExport.Click += new System.EventHandler(this.btnStartExport_Click);
            // 
            // textPropertyName
            // 
            this.textPropertyName.Location = new System.Drawing.Point(98, 12);
            this.textPropertyName.Name = "textPropertyName";
            this.textPropertyName.Size = new System.Drawing.Size(240, 21);
            this.textPropertyName.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "属性名称：";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPageSystem);
            this.tabControl1.Controls.Add(this.tabPageParking);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(394, 196);
            this.tabControl1.TabIndex = 8;
            // 
            // tabPageSystem
            // 
            this.tabPageSystem.Controls.Add(this.chkTextureFile);
            this.tabPageSystem.Controls.Add(this.chkUserDefineFormat);
            this.tabPageSystem.Controls.Add(this.chkExportGrid);
            this.tabPageSystem.Controls.Add(this.label6);
            this.tabPageSystem.Controls.Add(this.txtDefaultLayer);
            this.tabPageSystem.Controls.Add(this.chkOriginMaterial);
            this.tabPageSystem.Controls.Add(this.btnSaveFile);
            this.tabPageSystem.Controls.Add(this.textFilePath);
            this.tabPageSystem.Controls.Add(this.label2);
            this.tabPageSystem.Controls.Add(this.chkUnit);
            this.tabPageSystem.Controls.Add(this.chkExportLink);
            this.tabPageSystem.Controls.Add(this.chkOptimize);
            this.tabPageSystem.Controls.Add(this.chkExportProperty);
            this.tabPageSystem.Location = new System.Drawing.Point(4, 22);
            this.tabPageSystem.Name = "tabPageSystem";
            this.tabPageSystem.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSystem.Size = new System.Drawing.Size(386, 170);
            this.tabPageSystem.TabIndex = 0;
            this.tabPageSystem.Text = "系统";
            this.tabPageSystem.UseVisualStyleBackColor = true;
            // 
            // chkUserDefineFormat
            // 
            this.chkUserDefineFormat.AutoSize = true;
            this.chkUserDefineFormat.Location = new System.Drawing.Point(108, 59);
            this.chkUserDefineFormat.Name = "chkUserDefineFormat";
            this.chkUserDefineFormat.Size = new System.Drawing.Size(84, 16);
            this.chkUserDefineFormat.TabIndex = 17;
            this.chkUserDefineFormat.Text = "自定义格式";
            this.chkUserDefineFormat.UseVisualStyleBackColor = true;
            this.chkUserDefineFormat.CheckedChanged += new System.EventHandler(this.chkUserDefineFormat_CheckedChanged);
            // 
            // chkExportGrid
            // 
            this.chkExportGrid.AutoSize = true;
            this.chkExportGrid.Location = new System.Drawing.Point(108, 37);
            this.chkExportGrid.Name = "chkExportGrid";
            this.chkExportGrid.Size = new System.Drawing.Size(96, 16);
            this.chkExportGrid.TabIndex = 16;
            this.chkExportGrid.Text = "导出轴网信息";
            this.chkExportGrid.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 115);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 15;
            this.label6.Text = "默认图层：";
            // 
            // txtDefaultLayer
            // 
            this.txtDefaultLayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtDefaultLayer.Location = new System.Drawing.Point(77, 112);
            this.txtDefaultLayer.Name = "txtDefaultLayer";
            this.txtDefaultLayer.Size = new System.Drawing.Size(241, 21);
            this.txtDefaultLayer.TabIndex = 14;
            // 
            // chkOriginMaterial
            // 
            this.chkOriginMaterial.AutoSize = true;
            this.chkOriginMaterial.Location = new System.Drawing.Point(108, 15);
            this.chkOriginMaterial.Name = "chkOriginMaterial";
            this.chkOriginMaterial.Size = new System.Drawing.Size(96, 16);
            this.chkOriginMaterial.TabIndex = 7;
            this.chkOriginMaterial.Text = "原始材质颜色";
            this.chkOriginMaterial.UseVisualStyleBackColor = true;
            // 
            // btnSaveFile
            // 
            this.btnSaveFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveFile.Location = new System.Drawing.Point(324, 139);
            this.btnSaveFile.Name = "btnSaveFile";
            this.btnSaveFile.Size = new System.Drawing.Size(56, 23);
            this.btnSaveFile.TabIndex = 6;
            this.btnSaveFile.Text = "浏览";
            this.btnSaveFile.UseVisualStyleBackColor = true;
            this.btnSaveFile.Click += new System.EventHandler(this.btnSaveFile_Click);
            // 
            // textFilePath
            // 
            this.textFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textFilePath.Location = new System.Drawing.Point(77, 139);
            this.textFilePath.Name = "textFilePath";
            this.textFilePath.ReadOnly = true;
            this.textFilePath.Size = new System.Drawing.Size(241, 21);
            this.textFilePath.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 144);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "文件路径：";
            // 
            // tabPageParking
            // 
            this.tabPageParking.Controls.Add(this.label5);
            this.tabPageParking.Controls.Add(this.textHeight);
            this.tabPageParking.Controls.Add(this.label4);
            this.tabPageParking.Controls.Add(this.textWidth);
            this.tabPageParking.Controls.Add(this.label3);
            this.tabPageParking.Controls.Add(this.label1);
            this.tabPageParking.Controls.Add(this.textPropertyName);
            this.tabPageParking.Location = new System.Drawing.Point(4, 22);
            this.tabPageParking.Name = "tabPageParking";
            this.tabPageParking.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageParking.Size = new System.Drawing.Size(386, 170);
            this.tabPageParking.TabIndex = 1;
            this.tabPageParking.Text = "车位优化";
            this.tabPageParking.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(223, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 12);
            this.label5.TabIndex = 11;
            this.label5.Text = "高";
            // 
            // textHeight
            // 
            this.textHeight.Location = new System.Drawing.Point(246, 39);
            this.textHeight.Name = "textHeight";
            this.textHeight.Size = new System.Drawing.Size(92, 21);
            this.textHeight.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(96, 43);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "宽";
            // 
            // textWidth
            // 
            this.textWidth.Location = new System.Drawing.Point(125, 39);
            this.textWidth.Name = "textWidth";
            this.textWidth.Size = new System.Drawing.Size(92, 21);
            this.textWidth.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "编号大小范围：";
            // 
            // chkTextureFile
            // 
            this.chkTextureFile.AutoSize = true;
            this.chkTextureFile.Location = new System.Drawing.Point(108, 81);
            this.chkTextureFile.Name = "chkTextureFile";
            this.chkTextureFile.Size = new System.Drawing.Size(96, 16);
            this.chkTextureFile.TabIndex = 18;
            this.chkTextureFile.Text = "导出材质贴图";
            this.chkTextureFile.UseVisualStyleBackColor = true;
            // 
            // FormSettings
            // 
            this.AcceptButton = this.btnStartExport;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(418, 249);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnStartExport);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "导出设置";
            this.Load += new System.EventHandler(this.FormSettings_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPageSystem.ResumeLayout(false);
            this.tabPageSystem.PerformLayout();
            this.tabPageParking.ResumeLayout(false);
            this.tabPageParking.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox chkUnit;
        private System.Windows.Forms.CheckBox chkExportLink;
        private System.Windows.Forms.CheckBox chkExportProperty;
        private System.Windows.Forms.CheckBox chkOptimize;
        private System.Windows.Forms.Button btnStartExport;
        private System.Windows.Forms.TextBox textPropertyName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageSystem;
        private System.Windows.Forms.Button btnSaveFile;
        private System.Windows.Forms.TextBox textFilePath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabPage tabPageParking;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textHeight;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textWidth;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkOriginMaterial;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtDefaultLayer;
        private System.Windows.Forms.CheckBox chkExportGrid;
        private System.Windows.Forms.CheckBox chkUserDefineFormat;
        private System.Windows.Forms.CheckBox chkTextureFile;
    }
}