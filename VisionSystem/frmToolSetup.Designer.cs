namespace VisionSystem
{
    partial class frmToolSetup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmToolSetup));
            this.tablePanel1 = new DevExpress.Utils.Layout.TablePanel();
            this.cogToolGroupEdit = new Cognex.VisionPro.ToolGroup.CogToolGroupEdit();
            this.tablePanel2 = new DevExpress.Utils.Layout.TablePanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.txtComment = new DevExpress.XtraEditors.TextEdit();
            this.panel2 = new System.Windows.Forms.Panel();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.cbModel = new DevExpress.XtraEditors.ComboBoxEdit();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbCamera = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
            this.btnOpenImage = new DevExpress.XtraEditors.SimpleButton();
            this.btnClose = new DevExpress.XtraEditors.SimpleButton();
            this.btnSave = new DevExpress.XtraEditors.SimpleButton();
            this.highlighter1 = new DevComponents.DotNetBar.Validator.Highlighter();
            ((System.ComponentModel.ISupportInitialize)(this.tablePanel1)).BeginInit();
            this.tablePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tablePanel2)).BeginInit();
            this.tablePanel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtComment.Properties)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cbModel.Properties)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cbCamera.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // tablePanel1
            // 
            this.tablePanel1.Columns.AddRange(new DevExpress.Utils.Layout.TablePanelColumn[] {
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 5F)});
            this.tablePanel1.Controls.Add(this.cogToolGroupEdit);
            this.tablePanel1.Controls.Add(this.tablePanel2);
            this.tablePanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePanel1.Location = new System.Drawing.Point(0, 0);
            this.tablePanel1.Name = "tablePanel1";
            this.tablePanel1.Rows.AddRange(new DevExpress.Utils.Layout.TablePanelRow[] {
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 67F),
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 26F)});
            this.tablePanel1.Size = new System.Drawing.Size(1934, 1024);
            this.tablePanel1.TabIndex = 0;
            // 
            // cogToolGroupEdit
            // 
            this.cogToolGroupEdit.AllowDrop = true;
            this.tablePanel1.SetColumn(this.cogToolGroupEdit, 0);
            this.cogToolGroupEdit.ContextMenuCustomizer = null;
            this.cogToolGroupEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cogToolGroupEdit.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cogToolGroupEdit.Location = new System.Drawing.Point(3, 70);
            this.cogToolGroupEdit.Name = "cogToolGroupEdit";
            this.tablePanel1.SetRow(this.cogToolGroupEdit, 1);
            this.cogToolGroupEdit.Size = new System.Drawing.Size(1928, 951);
            this.cogToolGroupEdit.TabIndex = 3;
            this.cogToolGroupEdit.ToolSyncObject = null;
            // 
            // tablePanel2
            // 
            this.tablePanel1.SetColumn(this.tablePanel2, 0);
            this.tablePanel2.Columns.AddRange(new DevExpress.Utils.Layout.TablePanelColumn[] {
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 130F),
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 100F),
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 190F),
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 200F),
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 200F),
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 130F),
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 130F)});
            this.tablePanel2.Controls.Add(this.panel3);
            this.tablePanel2.Controls.Add(this.panel2);
            this.tablePanel2.Controls.Add(this.panel1);
            this.tablePanel2.Controls.Add(this.btnOpenImage);
            this.tablePanel2.Controls.Add(this.btnClose);
            this.tablePanel2.Controls.Add(this.btnSave);
            this.tablePanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePanel2.Location = new System.Drawing.Point(3, 3);
            this.tablePanel2.Name = "tablePanel2";
            this.tablePanel1.SetRow(this.tablePanel2, 0);
            this.tablePanel2.Rows.AddRange(new DevExpress.Utils.Layout.TablePanelRow[] {
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 26F)});
            this.tablePanel2.Size = new System.Drawing.Size(1928, 61);
            this.tablePanel2.TabIndex = 2;
            // 
            // panel3
            // 
            this.tablePanel2.SetColumn(this.panel3, 4);
            this.panel3.Controls.Add(this.labelControl2);
            this.panel3.Controls.Add(this.txtComment);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(1471, 3);
            this.panel3.Name = "panel3";
            this.tablePanel2.SetRow(this.panel3, 0);
            this.panel3.Size = new System.Drawing.Size(194, 55);
            this.panel3.TabIndex = 34;
            // 
            // labelControl2
            // 
            this.labelControl2.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl2.Appearance.Options.UseFont = true;
            this.labelControl2.Appearance.Options.UseTextOptions = true;
            this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl2.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelControl2.Location = new System.Drawing.Point(0, 0);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(194, 22);
            this.labelControl2.TabIndex = 10;
            this.labelControl2.Text = "Comment";
            // 
            // txtComment
            // 
            this.txtComment.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtComment.EditValue = "";
            this.txtComment.Location = new System.Drawing.Point(0, 25);
            this.txtComment.Name = "txtComment";
            this.txtComment.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtComment.Properties.Appearance.ForeColor = System.Drawing.Color.DimGray;
            this.txtComment.Properties.Appearance.Options.UseFont = true;
            this.txtComment.Properties.Appearance.Options.UseForeColor = true;
            this.txtComment.Properties.Appearance.Options.UseTextOptions = true;
            this.txtComment.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.txtComment.Size = new System.Drawing.Size(194, 30);
            this.txtComment.TabIndex = 31;
            // 
            // panel2
            // 
            this.tablePanel2.SetColumn(this.panel2, 2);
            this.panel2.Controls.Add(this.labelControl1);
            this.panel2.Controls.Add(this.cbModel);
            this.panel2.Location = new System.Drawing.Point(1081, 3);
            this.panel2.Name = "panel2";
            this.tablePanel2.SetRow(this.panel2, 0);
            this.panel2.Size = new System.Drawing.Size(184, 55);
            this.panel2.TabIndex = 33;
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl1.Appearance.Options.UseFont = true;
            this.labelControl1.Appearance.Options.UseTextOptions = true;
            this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelControl1.Location = new System.Drawing.Point(0, 0);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(184, 22);
            this.labelControl1.TabIndex = 10;
            this.labelControl1.Text = "ModelName";
            // 
            // cbModel
            // 
            this.cbModel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cbModel.Location = new System.Drawing.Point(0, 25);
            this.cbModel.Name = "cbModel";
            this.cbModel.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbModel.Properties.Appearance.Options.UseFont = true;
            this.cbModel.Properties.AppearanceDropDown.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbModel.Properties.AppearanceDropDown.Options.UseFont = true;
            this.cbModel.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbModel.Size = new System.Drawing.Size(184, 30);
            this.cbModel.TabIndex = 3;
            // 
            // panel1
            // 
            this.tablePanel2.SetColumn(this.panel1, 3);
            this.panel1.Controls.Add(this.cbCamera);
            this.panel1.Controls.Add(this.labelControl7);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(1271, 3);
            this.panel1.Name = "panel1";
            this.tablePanel2.SetRow(this.panel1, 0);
            this.panel1.Size = new System.Drawing.Size(194, 55);
            this.panel1.TabIndex = 32;
            // 
            // cbCamera
            // 
            this.cbCamera.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cbCamera.Location = new System.Drawing.Point(0, 25);
            this.cbCamera.Name = "cbCamera";
            this.cbCamera.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbCamera.Properties.Appearance.Options.UseFont = true;
            this.cbCamera.Properties.AppearanceDropDown.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbCamera.Properties.AppearanceDropDown.Options.UseFont = true;
            this.cbCamera.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbCamera.Size = new System.Drawing.Size(194, 30);
            this.cbCamera.TabIndex = 11;
            // 
            // labelControl7
            // 
            this.labelControl7.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl7.Appearance.Options.UseFont = true;
            this.labelControl7.Appearance.Options.UseTextOptions = true;
            this.labelControl7.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.labelControl7.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl7.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelControl7.Location = new System.Drawing.Point(0, 0);
            this.labelControl7.Name = "labelControl7";
            this.labelControl7.Size = new System.Drawing.Size(194, 22);
            this.labelControl7.TabIndex = 10;
            this.labelControl7.Text = "Camera";
            // 
            // btnOpenImage
            // 
            this.btnOpenImage.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenImage.Appearance.Options.UseFont = true;
            this.btnOpenImage.Appearance.Options.UseTextOptions = true;
            this.btnOpenImage.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.btnOpenImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpenImage.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            this.btnOpenImage.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnOpenImage.ImageOptions.SvgImage")));
            this.btnOpenImage.Location = new System.Drawing.Point(3, 3);
            this.btnOpenImage.Name = "btnOpenImage";
            this.btnOpenImage.Size = new System.Drawing.Size(124, 55);
            this.btnOpenImage.TabIndex = 5;
            this.btnOpenImage.Text = "Open Image";
            this.btnOpenImage.Click += new System.EventHandler(this.btnOpenImage_Click);
            // 
            // btnClose
            // 
            this.btnClose.Appearance.Font = new System.Drawing.Font("Tahoma", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Appearance.Options.UseFont = true;
            this.tablePanel2.SetColumn(this.btnClose, 6);
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClose.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            this.btnClose.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnClose.ImageOptions.SvgImage")));
            this.btnClose.Location = new System.Drawing.Point(1801, 3);
            this.btnClose.Name = "btnClose";
            this.tablePanel2.SetRow(this.btnClose, 0);
            this.btnClose.Size = new System.Drawing.Size(124, 55);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnSave
            // 
            this.btnSave.Appearance.Font = new System.Drawing.Font("Tahoma", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Appearance.Options.UseFont = true;
            this.tablePanel2.SetColumn(this.btnSave, 5);
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSave.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            this.btnSave.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnSave.ImageOptions.SvgImage")));
            this.btnSave.Location = new System.Drawing.Point(1671, 3);
            this.btnSave.Name = "btnSave";
            this.tablePanel2.SetRow(this.btnSave, 0);
            this.btnSave.Size = new System.Drawing.Size(124, 55);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // highlighter1
            // 
            this.highlighter1.ContainerControl = this;
            this.highlighter1.FocusHighlightColor = DevComponents.DotNetBar.Validator.eHighlightColor.Orange;
            this.highlighter1.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            // 
            // frmToolSetup
            // 
            this.AllowDrop = true;
            this.Appearance.Options.UseFont = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1934, 1024);
            this.Controls.Add(this.tablePanel1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.IconOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("frmToolSetup.IconOptions.LargeImage")));
            this.Name = "frmToolSetup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Tool Setup";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmToolSetup_FormClosing);
            this.Load += new System.EventHandler(this.frmToolSetup_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.frmToolSetup_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.frmToolSetup_DragEnter);
            ((System.ComponentModel.ISupportInitialize)(this.tablePanel1)).EndInit();
            this.tablePanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tablePanel2)).EndInit();
            this.tablePanel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtComment.Properties)).EndInit();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cbModel.Properties)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cbCamera.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.Utils.Layout.TablePanel tablePanel1;
        //private Cognex.VisionPro.ToolGroup.CogToolGroupEdit cogToolGroupEdit1;
        private DevExpress.Utils.Layout.TablePanel tablePanel2;
        private DevComponents.DotNetBar.Validator.Highlighter highlighter1;
        private DevExpress.XtraEditors.SimpleButton btnClose;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private Cognex.VisionPro.ToolGroup.CogToolGroupEdit cogToolGroupEdit;
        private DevExpress.XtraEditors.SimpleButton btnOpenImage;
        private DevExpress.XtraEditors.ComboBoxEdit cbModel;
        private System.Windows.Forms.Panel panel1;
        private DevExpress.XtraEditors.LabelControl labelControl7;
        private System.Windows.Forms.Panel panel2;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private System.Windows.Forms.Panel panel3;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.TextEdit txtComment;
        private DevExpress.XtraEditors.ComboBoxEdit cbCamera;
    }
}