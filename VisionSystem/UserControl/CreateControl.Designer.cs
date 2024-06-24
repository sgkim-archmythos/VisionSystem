
namespace VisionSystem
{
    partial class CreateControl
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
            this.groupControl6 = new DevExpress.XtraEditors.GroupControl();
            this.txtValue5 = new DevExpress.XtraEditors.TextEdit();
            this.txtValue4 = new DevExpress.XtraEditors.TextEdit();
            this.lblTitle5 = new DevExpress.XtraEditors.LabelControl();
            this.lblTitle4 = new DevExpress.XtraEditors.LabelControl();
            this.txtValue3 = new DevExpress.XtraEditors.TextEdit();
            this.txtValue2 = new DevExpress.XtraEditors.TextEdit();
            this.txtValue1 = new DevExpress.XtraEditors.TextEdit();
            this.lblTitle3 = new DevExpress.XtraEditors.LabelControl();
            this.lblTitle2 = new DevExpress.XtraEditors.LabelControl();
            this.lblTitle1 = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtValue5.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtValue4.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtValue3.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtValue2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtValue1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // groupControl6
            // 
            this.groupControl6.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupControl6.AppearanceCaption.Options.UseFont = true;
            this.groupControl6.Controls.Add(this.txtValue5);
            this.groupControl6.Controls.Add(this.txtValue4);
            this.groupControl6.Controls.Add(this.lblTitle5);
            this.groupControl6.Controls.Add(this.lblTitle4);
            this.groupControl6.Controls.Add(this.txtValue3);
            this.groupControl6.Controls.Add(this.txtValue2);
            this.groupControl6.Controls.Add(this.txtValue1);
            this.groupControl6.Controls.Add(this.lblTitle3);
            this.groupControl6.Controls.Add(this.lblTitle2);
            this.groupControl6.Controls.Add(this.lblTitle1);
            this.groupControl6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupControl6.Location = new System.Drawing.Point(0, 0);
            this.groupControl6.Name = "groupControl6";
            this.groupControl6.Size = new System.Drawing.Size(373, 179);
            this.groupControl6.TabIndex = 10;
            this.groupControl6.Text = "실행 매개 변수";
            // 
            // txtValue5
            // 
            this.txtValue5.Location = new System.Drawing.Point(84, 147);
            this.txtValue5.Name = "txtValue5";
            // 
            // 
            // 
            this.txtValue5.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtValue5.Properties.Appearance.Options.UseFont = true;
            this.txtValue5.Properties.Appearance.Options.UseTextOptions = true;
            this.txtValue5.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.txtValue5.Size = new System.Drawing.Size(124, 26);
            this.txtValue5.TabIndex = 67;
            this.txtValue5.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtValue1_KeyDown);
            // 
            // txtValue4
            // 
            this.txtValue4.Location = new System.Drawing.Point(84, 117);
            this.txtValue4.Name = "txtValue4";
            // 
            // 
            // 
            this.txtValue4.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtValue4.Properties.Appearance.Options.UseFont = true;
            this.txtValue4.Properties.Appearance.Options.UseTextOptions = true;
            this.txtValue4.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.txtValue4.Size = new System.Drawing.Size(124, 26);
            this.txtValue4.TabIndex = 66;
            this.txtValue4.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtValue1_KeyDown);
            // 
            // lblTitle5
            // 
            this.lblTitle5.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle5.Appearance.Options.UseFont = true;
            this.lblTitle5.Appearance.Options.UseTextOptions = true;
            this.lblTitle5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblTitle5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTitle5.Location = new System.Drawing.Point(10, 151);
            this.lblTitle5.Name = "lblTitle5";
            this.lblTitle5.Size = new System.Drawing.Size(63, 18);
            this.lblTitle5.TabIndex = 64;
            this.lblTitle5.Text = "회전 :";
            // 
            // lblTitle4
            // 
            this.lblTitle4.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle4.Appearance.Options.UseFont = true;
            this.lblTitle4.Appearance.Options.UseTextOptions = true;
            this.lblTitle4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblTitle4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTitle4.Location = new System.Drawing.Point(10, 121);
            this.lblTitle4.Name = "lblTitle4";
            this.lblTitle4.Size = new System.Drawing.Size(63, 18);
            this.lblTitle4.TabIndex = 63;
            this.lblTitle4.Text = "반지름 Y :";
            // 
            // txtValue3
            // 
            this.txtValue3.Location = new System.Drawing.Point(84, 87);
            this.txtValue3.Name = "txtValue3";
            // 
            // 
            // 
            this.txtValue3.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtValue3.Properties.Appearance.Options.UseFont = true;
            this.txtValue3.Properties.Appearance.Options.UseTextOptions = true;
            this.txtValue3.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.txtValue3.Size = new System.Drawing.Size(124, 26);
            this.txtValue3.TabIndex = 62;
            this.txtValue3.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtValue1_KeyDown);
            // 
            // txtValue2
            // 
            this.txtValue2.Location = new System.Drawing.Point(84, 57);
            this.txtValue2.Name = "txtValue2";
            // 
            // 
            // 
            this.txtValue2.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtValue2.Properties.Appearance.Options.UseFont = true;
            this.txtValue2.Properties.Appearance.Options.UseTextOptions = true;
            this.txtValue2.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.txtValue2.Size = new System.Drawing.Size(124, 26);
            this.txtValue2.TabIndex = 61;
            this.txtValue2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtValue1_KeyDown);
            // 
            // txtValue1
            // 
            this.txtValue1.Location = new System.Drawing.Point(84, 27);
            this.txtValue1.Name = "txtValue1";
            // 
            // 
            // 
            this.txtValue1.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtValue1.Properties.Appearance.Options.UseFont = true;
            this.txtValue1.Properties.Appearance.Options.UseTextOptions = true;
            this.txtValue1.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.txtValue1.Size = new System.Drawing.Size(124, 26);
            this.txtValue1.TabIndex = 60;
            this.txtValue1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtValue1_KeyDown);
            // 
            // lblTitle3
            // 
            this.lblTitle3.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle3.Appearance.Options.UseFont = true;
            this.lblTitle3.Appearance.Options.UseTextOptions = true;
            this.lblTitle3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblTitle3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTitle3.Location = new System.Drawing.Point(10, 91);
            this.lblTitle3.Name = "lblTitle3";
            this.lblTitle3.Size = new System.Drawing.Size(63, 18);
            this.lblTitle3.TabIndex = 4;
            this.lblTitle3.Text = "반지름 X :";
            // 
            // lblTitle2
            // 
            this.lblTitle2.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle2.Appearance.Options.UseFont = true;
            this.lblTitle2.Appearance.Options.UseTextOptions = true;
            this.lblTitle2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblTitle2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTitle2.Location = new System.Drawing.Point(10, 61);
            this.lblTitle2.Name = "lblTitle2";
            this.lblTitle2.Size = new System.Drawing.Size(63, 18);
            this.lblTitle2.TabIndex = 2;
            this.lblTitle2.Text = "중심 Y :";
            // 
            // lblTitle1
            // 
            this.lblTitle1.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle1.Appearance.Options.UseFont = true;
            this.lblTitle1.Appearance.Options.UseTextOptions = true;
            this.lblTitle1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.lblTitle1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTitle1.Location = new System.Drawing.Point(10, 31);
            this.lblTitle1.Name = "lblTitle1";
            this.lblTitle1.Size = new System.Drawing.Size(63, 18);
            this.lblTitle1.TabIndex = 0;
            this.lblTitle1.Text = "중심 X :";
            // 
            // CreateControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.Controls.Add(this.groupControl6);
            this.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "CreateControl";
            this.Size = new System.Drawing.Size(373, 179);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtValue5.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtValue4.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtValue3.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtValue2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtValue1.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.GroupControl groupControl6;
        private DevExpress.XtraEditors.LabelControl lblTitle3;
        private DevExpress.XtraEditors.LabelControl lblTitle2;
        private DevExpress.XtraEditors.LabelControl lblTitle1;
        private DevExpress.XtraEditors.TextEdit txtValue3;
        private DevExpress.XtraEditors.TextEdit txtValue2;
        private DevExpress.XtraEditors.TextEdit txtValue1;
        private DevExpress.XtraEditors.TextEdit txtValue5;
        private DevExpress.XtraEditors.TextEdit txtValue4;
        private DevExpress.XtraEditors.LabelControl lblTitle5;
        private DevExpress.XtraEditors.LabelControl lblTitle4;
    }
}
