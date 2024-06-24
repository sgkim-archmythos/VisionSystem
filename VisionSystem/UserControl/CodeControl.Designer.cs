namespace VisionSystem
{
    partial class CodeControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CodeControl));
            this.tablePanel1 = new DevExpress.Utils.Layout.TablePanel();
            this.lblComment = new DevExpress.XtraEditors.LabelControl();
            this.cogDisp = new Cognex.VisionPro.CogRecordDisplay();
            this.panelControl3 = new DevExpress.XtraEditors.PanelControl();
            this.lblCodeName = new DevExpress.XtraEditors.LabelControl();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.radUse = new System.Windows.Forms.RadioButton();
            this.panelControl2 = new DevExpress.XtraEditors.PanelControl();
            this.pnl = new DevExpress.XtraEditors.PanelControl();
            this.highlighter1 = new DevComponents.DotNetBar.Validator.Highlighter();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnvppInsert = new DevExpress.XtraEditors.SimpleButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.tablePanel1)).BeginInit();
            this.tablePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cogDisp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl3)).BeginInit();
            this.panelControl3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnl)).BeginInit();
            this.pnl.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tablePanel1
            // 
            this.tablePanel1.Appearance.BorderColor = System.Drawing.Color.Lime;
            this.tablePanel1.Appearance.Options.UseBorderColor = true;
            this.tablePanel1.Columns.AddRange(new DevExpress.Utils.Layout.TablePanelColumn[] {
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 50F),
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 87F),
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 205F),
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 50F)});
            this.tablePanel1.Controls.Add(this.lblComment);
            this.tablePanel1.Controls.Add(this.cogDisp);
            this.tablePanel1.Controls.Add(this.panelControl3);
            this.tablePanel1.Controls.Add(this.panelControl1);
            this.tablePanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePanel1.Location = new System.Drawing.Point(2, 2);
            this.tablePanel1.Name = "tablePanel1";
            this.tablePanel1.Rows.AddRange(new DevExpress.Utils.Layout.TablePanelRow[] {
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 26F)});
            this.tablePanel1.Size = new System.Drawing.Size(486, 185);
            this.tablePanel1.TabIndex = 0;
            // 
            // lblComment
            // 
            this.lblComment.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblComment.Appearance.Options.UseFont = true;
            this.lblComment.Appearance.Options.UseTextOptions = true;
            this.lblComment.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lblComment.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.lblComment.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.tablePanel1.SetColumn(this.lblComment, 3);
            this.lblComment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblComment.Location = new System.Drawing.Point(345, 3);
            this.lblComment.Name = "lblComment";
            this.tablePanel1.SetRow(this.lblComment, 0);
            this.lblComment.Size = new System.Drawing.Size(138, 179);
            this.lblComment.TabIndex = 3;
            this.lblComment.Text = "Comment";
            this.lblComment.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lblCodeName_MouseClick);
            // 
            // cogDisp
            // 
            this.cogDisp.ColorMapLowerClipColor = System.Drawing.Color.Black;
            this.cogDisp.ColorMapLowerRoiLimit = 0D;
            this.cogDisp.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
            this.cogDisp.ColorMapUpperClipColor = System.Drawing.Color.Black;
            this.cogDisp.ColorMapUpperRoiLimit = 1D;
            this.tablePanel1.SetColumn(this.cogDisp, 2);
            this.cogDisp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cogDisp.DoubleTapZoomCycleLength = 2;
            this.cogDisp.DoubleTapZoomSensitivity = 2.5D;
            this.cogDisp.Location = new System.Drawing.Point(140, 3);
            this.cogDisp.MouseWheelMode = Cognex.VisionPro.Display.CogDisplayMouseWheelModeConstants.None;
            this.cogDisp.MouseWheelSensitivity = 1D;
            this.cogDisp.Name = "cogDisp";
            this.cogDisp.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("cogDisp.OcxState")));
            this.tablePanel1.SetRow(this.cogDisp, 0);
            this.cogDisp.Size = new System.Drawing.Size(199, 179);
            this.cogDisp.TabIndex = 2;
            this.cogDisp.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cogDisp_MouseDown);
            // 
            // panelControl3
            // 
            this.tablePanel1.SetColumn(this.panelControl3, 1);
            this.panelControl3.Controls.Add(this.panel1);
            this.panelControl3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl3.Location = new System.Drawing.Point(53, 3);
            this.panelControl3.Name = "panelControl3";
            this.tablePanel1.SetRow(this.panelControl3, 0);
            this.panelControl3.Size = new System.Drawing.Size(81, 179);
            this.panelControl3.TabIndex = 1;
            // 
            // lblCodeName
            // 
            this.lblCodeName.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCodeName.Appearance.Options.UseFont = true;
            this.lblCodeName.Appearance.Options.UseTextOptions = true;
            this.lblCodeName.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lblCodeName.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.lblCodeName.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblCodeName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCodeName.Location = new System.Drawing.Point(3, 3);
            this.lblCodeName.Name = "lblCodeName";
            this.lblCodeName.Size = new System.Drawing.Size(71, 134);
            this.lblCodeName.TabIndex = 2;
            this.lblCodeName.Text = "Test";
            this.lblCodeName.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lblCodeName_MouseClick);
            // 
            // panelControl1
            // 
            this.tablePanel1.SetColumn(this.panelControl1, 0);
            this.panelControl1.Controls.Add(this.radUse);
            this.panelControl1.Controls.Add(this.panelControl2);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl1.Location = new System.Drawing.Point(3, 3);
            this.panelControl1.Name = "panelControl1";
            this.tablePanel1.SetRow(this.panelControl1, 0);
            this.panelControl1.Size = new System.Drawing.Size(44, 179);
            this.panelControl1.TabIndex = 0;
            // 
            // radUse
            // 
            this.radUse.AutoSize = true;
            this.radUse.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radUse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radUse.Location = new System.Drawing.Point(2, 2);
            this.radUse.Name = "radUse";
            this.radUse.Size = new System.Drawing.Size(40, 175);
            this.radUse.TabIndex = 2;
            this.radUse.TabStop = true;
            this.radUse.UseVisualStyleBackColor = true;
            this.radUse.CheckedChanged += new System.EventHandler(this.radUse_CheckedChanged);
            // 
            // panelControl2
            // 
            this.panelControl2.Location = new System.Drawing.Point(50, 5);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(44, 100);
            this.panelControl2.TabIndex = 1;
            // 
            // pnl
            // 
            this.pnl.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.pnl.Controls.Add(this.tablePanel1);
            this.pnl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.highlighter1.SetHighlightOnFocus(this.pnl, true);
            this.pnl.Location = new System.Drawing.Point(0, 0);
            this.pnl.Name = "pnl";
            this.pnl.Size = new System.Drawing.Size(490, 189);
            this.pnl.TabIndex = 1;
            // 
            // highlighter1
            // 
            this.highlighter1.ContainerControl = this;
            this.highlighter1.FocusHighlightColor = DevComponents.DotNetBar.Validator.eHighlightColor.Orange;
            this.highlighter1.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(2, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(77, 175);
            this.panel1.TabIndex = 3;
            // 
            // btnvppInsert
            // 
            this.btnvppInsert.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnvppInsert.Appearance.Options.UseFont = true;
            this.btnvppInsert.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnvppInsert.Location = new System.Drawing.Point(3, 143);
            this.btnvppInsert.Name = "btnvppInsert";
            this.btnvppInsert.Size = new System.Drawing.Size(71, 29);
            this.btnvppInsert.TabIndex = 3;
            this.btnvppInsert.Text = "수정";
            this.btnvppInsert.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblCodeName, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnvppInsert, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(77, 175);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // CodeControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnl);
            this.MaximumSize = new System.Drawing.Size(490, 189);
            this.MinimumSize = new System.Drawing.Size(490, 189);
            this.Name = "CodeControl";
            this.Size = new System.Drawing.Size(490, 189);
            ((System.ComponentModel.ISupportInitialize)(this.tablePanel1)).EndInit();
            this.tablePanel1.ResumeLayout(false);
            this.tablePanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cogDisp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl3)).EndInit();
            this.panelControl3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnl)).EndInit();
            this.pnl.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.Utils.Layout.TablePanel tablePanel1;
        private DevExpress.XtraEditors.PanelControl panelControl3;
        private DevExpress.XtraEditors.PanelControl panelControl2;
        public DevExpress.XtraEditors.LabelControl lblCodeName;
        public System.Windows.Forms.RadioButton radUse;
        public Cognex.VisionPro.CogRecordDisplay cogDisp;
        public DevExpress.XtraEditors.LabelControl lblComment;
        private DevComponents.DotNetBar.Validator.Highlighter highlighter1;
        public DevExpress.XtraEditors.PanelControl panelControl1;
        public DevExpress.XtraEditors.PanelControl pnl;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private DevExpress.XtraEditors.SimpleButton btnvppInsert;
    }
}
