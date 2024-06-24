namespace VisionSystem
{
    partial class PrintParamControl
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
            this.gpDefect = new DevExpress.XtraEditors.GroupControl();
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.txtMIP = new DevExpress.XtraEditors.TextEdit();
            this.txtCount = new DevExpress.XtraEditors.TextEdit();
            ((System.ComponentModel.ISupportInitialize)(this.gpDefect)).BeginInit();
            this.gpDefect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtMIP.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCount.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // gpDefect
            // 
            this.gpDefect.Controls.Add(this.txtCount);
            this.gpDefect.Controls.Add(this.txtMIP);
            this.gpDefect.Controls.Add(this.labelControl5);
            this.gpDefect.Controls.Add(this.labelControl3);
            this.gpDefect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpDefect.Location = new System.Drawing.Point(0, 0);
            this.gpDefect.Name = "gpDefect";
            this.gpDefect.Size = new System.Drawing.Size(801, 671);
            this.gpDefect.TabIndex = 25;
            this.gpDefect.Text = "Basic Settings";
            // 
            // labelControl5
            // 
            this.labelControl5.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl5.Appearance.Options.UseFont = true;
            this.labelControl5.Appearance.Options.UseTextOptions = true;
            this.labelControl5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl5.Location = new System.Drawing.Point(6, 59);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(188, 25);
            this.labelControl5.TabIndex = 0;
            this.labelControl5.Text = "Count : ";
            // 
            // labelControl3
            // 
            this.labelControl3.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl3.Appearance.Options.UseFont = true;
            this.labelControl3.Appearance.Options.UseTextOptions = true;
            this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl3.Location = new System.Drawing.Point(6, 29);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(188, 25);
            this.labelControl3.TabIndex = 20;
            this.labelControl3.Text = "HMC P/N :";
            // 
            // txtMIP
            // 
            this.txtMIP.Location = new System.Drawing.Point(207, 26);
            this.txtMIP.Name = "txtMIP";
            this.txtMIP.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMIP.Properties.Appearance.Options.UseFont = true;
            this.txtMIP.Properties.Appearance.Options.UseTextOptions = true;
            this.txtMIP.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.txtMIP.Size = new System.Drawing.Size(119, 24);
            this.txtMIP.TabIndex = 22;
            // 
            // txtCount
            // 
            this.txtCount.Location = new System.Drawing.Point(207, 60);
            this.txtCount.Name = "txtCount";
            this.txtCount.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCount.Properties.Appearance.Options.UseFont = true;
            this.txtCount.Properties.Appearance.Options.UseTextOptions = true;
            this.txtCount.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.txtCount.Size = new System.Drawing.Size(119, 24);
            this.txtCount.TabIndex = 23;
            // 
            // PrintParamControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gpDefect);
            this.Name = "PrintParamControl";
            this.Size = new System.Drawing.Size(801, 671);
            ((System.ComponentModel.ISupportInitialize)(this.gpDefect)).EndInit();
            this.gpDefect.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtMIP.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCount.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.GroupControl gpDefect;
        private DevExpress.XtraEditors.LabelControl labelControl5;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.TextEdit txtMIP;
        private DevExpress.XtraEditors.TextEdit txtCount;
    }
}
