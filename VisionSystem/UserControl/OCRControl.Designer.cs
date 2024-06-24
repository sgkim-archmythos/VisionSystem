
namespace VisionSystem
{
    partial class OCRControl
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OCRControl));
            this.tablePanel1 = new DevExpress.Utils.Layout.TablePanel();
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.btnAutoTune = new DevExpress.XtraEditors.SimpleButton();
            this.textEdit1 = new DevExpress.XtraEditors.TextEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.btnLineExtract = new DevExpress.XtraEditors.SimpleButton();
            this.cogDisp = new Cognex.VisionPro.Display.CogDisplay();
            this.groupControl4 = new DevExpress.XtraEditors.GroupControl();
            this.cbBlobRegion = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl17 = new DevExpress.XtraEditors.LabelControl();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.lblTitle8 = new DevExpress.XtraEditors.LabelControl();
            this.txtSize8 = new DevExpress.XtraEditors.TextEdit();
            this.txtSize7 = new DevExpress.XtraEditors.TextEdit();
            this.lblTitle7 = new DevExpress.XtraEditors.LabelControl();
            this.txtSize6 = new DevExpress.XtraEditors.TextEdit();
            this.lblTitle6 = new DevExpress.XtraEditors.LabelControl();
            this.txtSize5 = new DevExpress.XtraEditors.TextEdit();
            this.lblTitle5 = new DevExpress.XtraEditors.LabelControl();
            this.txtSize4 = new DevExpress.XtraEditors.TextEdit();
            this.lblTitle4 = new DevExpress.XtraEditors.LabelControl();
            this.txtSize3 = new DevExpress.XtraEditors.TextEdit();
            this.lblTitle3 = new DevExpress.XtraEditors.LabelControl();
            this.txtSize2 = new DevExpress.XtraEditors.TextEdit();
            this.lblTitle2 = new DevExpress.XtraEditors.LabelControl();
            this.txtSize1 = new DevExpress.XtraEditors.TextEdit();
            this.lblTitle1 = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.tablePanel1)).BeginInit();
            this.tablePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cogDisp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl4)).BeginInit();
            this.groupControl4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cbBlobRegion.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSize8.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSize7.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSize6.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSize5.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSize4.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSize3.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSize2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSize1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // tablePanel1
            // 
            this.tablePanel1.Columns.AddRange(new DevExpress.Utils.Layout.TablePanelColumn[] {
            new DevExpress.Utils.Layout.TablePanelColumn(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 55F)});
            this.tablePanel1.Controls.Add(this.panelControl1);
            this.tablePanel1.Controls.Add(this.groupControl4);
            this.tablePanel1.Controls.Add(this.groupControl1);
            this.tablePanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePanel1.Location = new System.Drawing.Point(0, 0);
            this.tablePanel1.Name = "tablePanel1";
            this.tablePanel1.Rows.AddRange(new DevExpress.Utils.Layout.TablePanelRow[] {
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 349F),
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 63F),
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Absolute, 227F),
            new DevExpress.Utils.Layout.TablePanelRow(DevExpress.Utils.Layout.TablePanelEntityStyle.Relative, 100F)});
            this.tablePanel1.Size = new System.Drawing.Size(373, 648);
            this.tablePanel1.TabIndex = 0;
            // 
            // groupControl1
            // 
            this.groupControl1.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupControl1.AppearanceCaption.Options.UseFont = true;
            this.tablePanel1.SetColumn(this.groupControl1, 0);
            this.groupControl1.Controls.Add(this.cogDisp);
            this.groupControl1.Controls.Add(this.btnAutoTune);
            this.groupControl1.Controls.Add(this.textEdit1);
            this.groupControl1.Controls.Add(this.labelControl1);
            this.groupControl1.Controls.Add(this.btnLineExtract);
            this.groupControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupControl1.Location = new System.Drawing.Point(3, 3);
            this.groupControl1.Name = "groupControl1";
            this.tablePanel1.SetRow(this.groupControl1, 0);
            this.groupControl1.Size = new System.Drawing.Size(367, 343);
            this.groupControl1.TabIndex = 0;
            this.groupControl1.Text = "조정";
            // 
            // btnAutoTune
            // 
            this.btnAutoTune.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAutoTune.Appearance.Options.UseFont = true;
            this.btnAutoTune.Location = new System.Drawing.Point(257, 26);
            this.btnAutoTune.Name = "btnAutoTune";
            this.btnAutoTune.Size = new System.Drawing.Size(105, 23);
            this.btnAutoTune.TabIndex = 3;
            this.btnAutoTune.Text = "자동 세그먼트";
            // 
            // textEdit1
            // 
            this.textEdit1.Location = new System.Drawing.Point(105, 52);
            this.textEdit1.Name = "textEdit1";
            this.textEdit1.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textEdit1.Properties.Appearance.Options.UseFont = true;
            this.textEdit1.Size = new System.Drawing.Size(257, 24);
            this.textEdit1.TabIndex = 2;
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl1.Appearance.Options.UseFont = true;
            this.labelControl1.Location = new System.Drawing.Point(8, 54);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(91, 19);
            this.labelControl1.TabIndex = 1;
            this.labelControl1.Text = "예상 텍스트 : ";
            // 
            // btnLineExtract
            // 
            this.btnLineExtract.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLineExtract.Appearance.Options.UseFont = true;
            this.btnLineExtract.Location = new System.Drawing.Point(5, 26);
            this.btnLineExtract.Name = "btnLineExtract";
            this.btnLineExtract.Size = new System.Drawing.Size(75, 23);
            this.btnLineExtract.TabIndex = 0;
            this.btnLineExtract.Text = "라인 추출";
            // 
            // cogDisp
            // 
            this.cogDisp.ColorMapLowerClipColor = System.Drawing.Color.Black;
            this.cogDisp.ColorMapLowerRoiLimit = 0D;
            this.cogDisp.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
            this.cogDisp.ColorMapUpperClipColor = System.Drawing.Color.Black;
            this.cogDisp.ColorMapUpperRoiLimit = 1D;
            this.cogDisp.DoubleTapZoomCycleLength = 2;
            this.cogDisp.DoubleTapZoomSensitivity = 2.5D;
            this.cogDisp.Location = new System.Drawing.Point(4, 82);
            this.cogDisp.MouseWheelMode = Cognex.VisionPro.Display.CogDisplayMouseWheelModeConstants.Zoom1;
            this.cogDisp.MouseWheelSensitivity = 1D;
            this.cogDisp.Name = "cogDisp";
            this.cogDisp.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("cogDisp.OcxState")));
            this.cogDisp.Size = new System.Drawing.Size(358, 259);
            this.cogDisp.TabIndex = 4;
            // 
            // groupControl4
            // 
            this.groupControl4.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupControl4.AppearanceCaption.Options.UseFont = true;
            this.tablePanel1.SetColumn(this.groupControl4, 0);
            this.groupControl4.Controls.Add(this.cbBlobRegion);
            this.groupControl4.Controls.Add(this.labelControl17);
            this.groupControl4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupControl4.Location = new System.Drawing.Point(3, 352);
            this.groupControl4.Name = "groupControl4";
            this.tablePanel1.SetRow(this.groupControl4, 1);
            this.groupControl4.Size = new System.Drawing.Size(367, 57);
            this.groupControl4.TabIndex = 20;
            this.groupControl4.Text = "영역 설정";
            // 
            // cbBlobRegion
            // 
            this.cbBlobRegion.EditValue = "없음-전체 이미지 사용";
            this.cbBlobRegion.Location = new System.Drawing.Point(80, 28);
            this.cbBlobRegion.Name = "cbBlobRegion";
            this.cbBlobRegion.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbBlobRegion.Properties.Appearance.Options.UseFont = true;
            this.cbBlobRegion.Properties.AppearanceDropDown.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbBlobRegion.Properties.AppearanceDropDown.Options.UseFont = true;
            this.cbBlobRegion.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cbBlobRegion.Properties.Items.AddRange(new object[] {
            "CogCircle",
            "CogEllipse",
            "CogPolygon",
            "CogRectangle",
            "CogRectangleAffine",
            "CogCircularAnnulusSection",
            "CogEllipticalAnnulusSection",
            "없음-전체 이미지 사용"});
            this.cbBlobRegion.Size = new System.Drawing.Size(259, 24);
            this.cbBlobRegion.TabIndex = 59;
            // 
            // labelControl17
            // 
            this.labelControl17.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl17.Appearance.Options.UseFont = true;
            this.labelControl17.Location = new System.Drawing.Point(6, 32);
            this.labelControl17.Name = "labelControl17";
            this.labelControl17.Size = new System.Drawing.Size(69, 18);
            this.labelControl17.TabIndex = 58;
            this.labelControl17.Text = "영역 형태 : ";
            // 
            // panelControl1
            // 
            this.panelControl1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.tablePanel1.SetColumn(this.panelControl1, 0);
            this.panelControl1.Controls.Add(this.lblTitle8);
            this.panelControl1.Controls.Add(this.txtSize8);
            this.panelControl1.Controls.Add(this.txtSize7);
            this.panelControl1.Controls.Add(this.lblTitle7);
            this.panelControl1.Controls.Add(this.txtSize6);
            this.panelControl1.Controls.Add(this.lblTitle6);
            this.panelControl1.Controls.Add(this.txtSize5);
            this.panelControl1.Controls.Add(this.lblTitle5);
            this.panelControl1.Controls.Add(this.txtSize4);
            this.panelControl1.Controls.Add(this.lblTitle4);
            this.panelControl1.Controls.Add(this.txtSize3);
            this.panelControl1.Controls.Add(this.lblTitle3);
            this.panelControl1.Controls.Add(this.txtSize2);
            this.panelControl1.Controls.Add(this.lblTitle2);
            this.panelControl1.Controls.Add(this.txtSize1);
            this.panelControl1.Controls.Add(this.lblTitle1);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl1.Location = new System.Drawing.Point(3, 415);
            this.panelControl1.Name = "panelControl1";
            this.tablePanel1.SetRow(this.panelControl1, 2);
            this.panelControl1.Size = new System.Drawing.Size(367, 221);
            this.panelControl1.TabIndex = 67;
            // 
            // lblTitle8
            // 
            this.lblTitle8.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle8.Appearance.Options.UseFont = true;
            this.lblTitle8.Appearance.Options.UseTextOptions = true;
            this.lblTitle8.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblTitle8.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTitle8.Location = new System.Drawing.Point(6, 196);
            this.lblTitle8.Name = "lblTitle8";
            this.lblTitle8.Size = new System.Drawing.Size(68, 18);
            this.lblTitle8.TabIndex = 32;
            this.lblTitle8.Text = "기울이기 : ";
            // 
            // txtSize8
            // 
            this.txtSize8.Location = new System.Drawing.Point(89, 193);
            this.txtSize8.Name = "txtSize8";
            this.txtSize8.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSize8.Properties.Appearance.Options.UseFont = true;
            this.txtSize8.Properties.Appearance.Options.UseTextOptions = true;
            this.txtSize8.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.txtSize8.Size = new System.Drawing.Size(114, 24);
            this.txtSize8.TabIndex = 33;
            // 
            // txtSize7
            // 
            this.txtSize7.Location = new System.Drawing.Point(89, 166);
            this.txtSize7.Name = "txtSize7";
            this.txtSize7.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSize7.Properties.Appearance.Options.UseFont = true;
            this.txtSize7.Properties.Appearance.Options.UseTextOptions = true;
            this.txtSize7.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.txtSize7.Size = new System.Drawing.Size(114, 24);
            this.txtSize7.TabIndex = 31;
            // 
            // lblTitle7
            // 
            this.lblTitle7.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle7.Appearance.Options.UseFont = true;
            this.lblTitle7.Appearance.Options.UseTextOptions = true;
            this.lblTitle7.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblTitle7.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTitle7.Location = new System.Drawing.Point(6, 169);
            this.lblTitle7.Name = "lblTitle7";
            this.lblTitle7.Size = new System.Drawing.Size(68, 18);
            this.lblTitle7.TabIndex = 30;
            this.lblTitle7.Text = "기울이기 : ";
            // 
            // txtSize6
            // 
            this.txtSize6.Location = new System.Drawing.Point(89, 139);
            this.txtSize6.Name = "txtSize6";
            this.txtSize6.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSize6.Properties.Appearance.Options.UseFont = true;
            this.txtSize6.Properties.Appearance.Options.UseTextOptions = true;
            this.txtSize6.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.txtSize6.Size = new System.Drawing.Size(114, 24);
            this.txtSize6.TabIndex = 29;
            // 
            // lblTitle6
            // 
            this.lblTitle6.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle6.Appearance.Options.UseFont = true;
            this.lblTitle6.Appearance.Options.UseTextOptions = true;
            this.lblTitle6.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblTitle6.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTitle6.Location = new System.Drawing.Point(7, 142);
            this.lblTitle6.Name = "lblTitle6";
            this.lblTitle6.Size = new System.Drawing.Size(68, 18);
            this.lblTitle6.TabIndex = 28;
            this.lblTitle6.Text = "기울이기 : ";
            // 
            // txtSize5
            // 
            this.txtSize5.Location = new System.Drawing.Point(89, 112);
            this.txtSize5.Name = "txtSize5";
            this.txtSize5.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSize5.Properties.Appearance.Options.UseFont = true;
            this.txtSize5.Properties.Appearance.Options.UseTextOptions = true;
            this.txtSize5.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.txtSize5.Size = new System.Drawing.Size(114, 24);
            this.txtSize5.TabIndex = 27;
            // 
            // lblTitle5
            // 
            this.lblTitle5.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle5.Appearance.Options.UseFont = true;
            this.lblTitle5.Appearance.Options.UseTextOptions = true;
            this.lblTitle5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblTitle5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTitle5.Location = new System.Drawing.Point(7, 115);
            this.lblTitle5.Name = "lblTitle5";
            this.lblTitle5.Size = new System.Drawing.Size(68, 18);
            this.lblTitle5.TabIndex = 26;
            this.lblTitle5.Text = "기울이기 : ";
            // 
            // txtSize4
            // 
            this.txtSize4.Location = new System.Drawing.Point(89, 85);
            this.txtSize4.Name = "txtSize4";
            this.txtSize4.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSize4.Properties.Appearance.Options.UseFont = true;
            this.txtSize4.Properties.Appearance.Options.UseTextOptions = true;
            this.txtSize4.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.txtSize4.Size = new System.Drawing.Size(114, 24);
            this.txtSize4.TabIndex = 25;
            // 
            // lblTitle4
            // 
            this.lblTitle4.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle4.Appearance.Options.UseFont = true;
            this.lblTitle4.Appearance.Options.UseTextOptions = true;
            this.lblTitle4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblTitle4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTitle4.Location = new System.Drawing.Point(7, 88);
            this.lblTitle4.Name = "lblTitle4";
            this.lblTitle4.Size = new System.Drawing.Size(68, 18);
            this.lblTitle4.TabIndex = 24;
            this.lblTitle4.Text = "기울이기 : ";
            // 
            // txtSize3
            // 
            this.txtSize3.Location = new System.Drawing.Point(89, 58);
            this.txtSize3.Name = "txtSize3";
            this.txtSize3.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSize3.Properties.Appearance.Options.UseFont = true;
            this.txtSize3.Properties.Appearance.Options.UseTextOptions = true;
            this.txtSize3.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.txtSize3.Size = new System.Drawing.Size(114, 24);
            this.txtSize3.TabIndex = 23;
            // 
            // lblTitle3
            // 
            this.lblTitle3.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle3.Appearance.Options.UseFont = true;
            this.lblTitle3.Appearance.Options.UseTextOptions = true;
            this.lblTitle3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblTitle3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTitle3.Location = new System.Drawing.Point(7, 61);
            this.lblTitle3.Name = "lblTitle3";
            this.lblTitle3.Size = new System.Drawing.Size(68, 18);
            this.lblTitle3.TabIndex = 22;
            this.lblTitle3.Text = "기울이기 : ";
            // 
            // txtSize2
            // 
            this.txtSize2.Location = new System.Drawing.Point(89, 31);
            this.txtSize2.Name = "txtSize2";
            this.txtSize2.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSize2.Properties.Appearance.Options.UseFont = true;
            this.txtSize2.Properties.Appearance.Options.UseTextOptions = true;
            this.txtSize2.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.txtSize2.Size = new System.Drawing.Size(114, 24);
            this.txtSize2.TabIndex = 21;
            // 
            // lblTitle2
            // 
            this.lblTitle2.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle2.Appearance.Options.UseFont = true;
            this.lblTitle2.Appearance.Options.UseTextOptions = true;
            this.lblTitle2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblTitle2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTitle2.Location = new System.Drawing.Point(6, 34);
            this.lblTitle2.Name = "lblTitle2";
            this.lblTitle2.Size = new System.Drawing.Size(68, 18);
            this.lblTitle2.TabIndex = 20;
            this.lblTitle2.Text = "기울이기 : ";
            // 
            // txtSize1
            // 
            this.txtSize1.Location = new System.Drawing.Point(89, 4);
            this.txtSize1.Name = "txtSize1";
            this.txtSize1.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSize1.Properties.Appearance.Options.UseFont = true;
            this.txtSize1.Properties.Appearance.Options.UseTextOptions = true;
            this.txtSize1.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.txtSize1.Size = new System.Drawing.Size(114, 24);
            this.txtSize1.TabIndex = 19;
            // 
            // lblTitle1
            // 
            this.lblTitle1.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle1.Appearance.Options.UseFont = true;
            this.lblTitle1.Appearance.Options.UseTextOptions = true;
            this.lblTitle1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblTitle1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTitle1.Location = new System.Drawing.Point(6, 7);
            this.lblTitle1.Name = "lblTitle1";
            this.lblTitle1.Size = new System.Drawing.Size(68, 18);
            this.lblTitle1.TabIndex = 18;
            this.lblTitle1.Text = "기울이기 : ";
            // 
            // OCRControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.Controls.Add(this.tablePanel1);
            this.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "OCRControl";
            this.Size = new System.Drawing.Size(373, 648);
            ((System.ComponentModel.ISupportInitialize)(this.tablePanel1)).EndInit();
            this.tablePanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            this.groupControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cogDisp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl4)).EndInit();
            this.groupControl4.ResumeLayout(false);
            this.groupControl4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cbBlobRegion.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtSize8.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSize7.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSize6.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSize5.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSize4.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSize3.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSize2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSize1.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.Utils.Layout.TablePanel tablePanel1;
        private DevExpress.XtraEditors.GroupControl groupControl1;
        private DevExpress.XtraEditors.SimpleButton btnAutoTune;
        private DevExpress.XtraEditors.TextEdit textEdit1;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.SimpleButton btnLineExtract;
        private Cognex.VisionPro.Display.CogDisplay cogDisp;
        private DevExpress.XtraEditors.GroupControl groupControl4;
        private DevExpress.XtraEditors.ComboBoxEdit cbBlobRegion;
        private DevExpress.XtraEditors.LabelControl labelControl17;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.LabelControl lblTitle8;
        private DevExpress.XtraEditors.TextEdit txtSize8;
        private DevExpress.XtraEditors.TextEdit txtSize7;
        private DevExpress.XtraEditors.LabelControl lblTitle7;
        private DevExpress.XtraEditors.TextEdit txtSize6;
        private DevExpress.XtraEditors.LabelControl lblTitle6;
        private DevExpress.XtraEditors.TextEdit txtSize5;
        private DevExpress.XtraEditors.LabelControl lblTitle5;
        private DevExpress.XtraEditors.TextEdit txtSize4;
        private DevExpress.XtraEditors.LabelControl lblTitle4;
        private DevExpress.XtraEditors.TextEdit txtSize3;
        private DevExpress.XtraEditors.LabelControl lblTitle3;
        private DevExpress.XtraEditors.TextEdit txtSize2;
        private DevExpress.XtraEditors.LabelControl lblTitle2;
        private DevExpress.XtraEditors.TextEdit txtSize1;
        private DevExpress.XtraEditors.LabelControl lblTitle1;
    }
}
