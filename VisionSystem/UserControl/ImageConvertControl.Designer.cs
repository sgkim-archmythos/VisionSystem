
namespace VisionSystem
{
    partial class ImageConvertControl
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
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.rad12 = new System.Windows.Forms.RadioButton();
            this.rad11 = new System.Windows.Forms.RadioButton();
            this.rad10 = new System.Windows.Forms.RadioButton();
            this.rad9 = new System.Windows.Forms.RadioButton();
            this.rad8 = new System.Windows.Forms.RadioButton();
            this.rad7 = new System.Windows.Forms.RadioButton();
            this.rad6 = new System.Windows.Forms.RadioButton();
            this.rad5 = new System.Windows.Forms.RadioButton();
            this.rad4 = new System.Windows.Forms.RadioButton();
            this.rad3 = new System.Windows.Forms.RadioButton();
            this.rad2 = new System.Windows.Forms.RadioButton();
            this.rad1 = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupControl1
            // 
            this.groupControl1.Controls.Add(this.rad12);
            this.groupControl1.Controls.Add(this.rad11);
            this.groupControl1.Controls.Add(this.rad10);
            this.groupControl1.Controls.Add(this.rad9);
            this.groupControl1.Controls.Add(this.rad8);
            this.groupControl1.Controls.Add(this.rad7);
            this.groupControl1.Controls.Add(this.rad6);
            this.groupControl1.Controls.Add(this.rad5);
            this.groupControl1.Controls.Add(this.rad4);
            this.groupControl1.Controls.Add(this.rad3);
            this.groupControl1.Controls.Add(this.rad2);
            this.groupControl1.Controls.Add(this.rad1);
            this.groupControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupControl1.Location = new System.Drawing.Point(0, 0);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(338, 446);
            this.groupControl1.TabIndex = 2;
            this.groupControl1.Text = "Execution mode";
            // 
            // rad12
            // 
            this.rad12.AutoSize = true;
            this.rad12.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rad12.Location = new System.Drawing.Point(14, 339);
            this.rad12.Name = "rad12";
            this.rad12.Size = new System.Drawing.Size(130, 23);
            this.rad12.TabIndex = 11;
            this.rad12.TabStop = true;
            this.rad12.Tag = "11";
            this.rad12.Text = "범위에서 마스크";
            this.rad12.UseVisualStyleBackColor = true;
            this.rad12.CheckedChanged += new System.EventHandler(this.rad1_CheckedChanged);
            // 
            // rad11
            // 
            this.rad11.AutoSize = true;
            this.rad11.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rad11.Location = new System.Drawing.Point(14, 311);
            this.rad11.Name = "rad11";
            this.rad11.Size = new System.Drawing.Size(116, 23);
            this.rad11.TabIndex = 10;
            this.rad11.TabStop = true;
            this.rad11.Tag = "10";
            this.rad11.Text = "범위에서 픽셀";
            this.rad11.UseVisualStyleBackColor = true;
            this.rad11.CheckedChanged += new System.EventHandler(this.rad1_CheckedChanged);
            // 
            // rad10
            // 
            this.rad10.AutoSize = true;
            this.rad10.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rad10.Location = new System.Drawing.Point(14, 283);
            this.rad10.Name = "rad10";
            this.rad10.Size = new System.Drawing.Size(206, 23);
            this.rad10.TabIndex = 9;
            this.rad10.TabStop = true;
            this.rad10.Tag = "9";
            this.rad10.Text = "Weighted RGB부터 밝기";
            this.rad10.UseVisualStyleBackColor = true;
            this.rad10.CheckedChanged += new System.EventHandler(this.rad1_CheckedChanged);
            // 
            // rad9
            // 
            this.rad9.AutoSize = true;
            this.rad9.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rad9.Location = new System.Drawing.Point(14, 255);
            this.rad9.Name = "rad9";
            this.rad9.Size = new System.Drawing.Size(62, 23);
            this.rad9.TabIndex = 8;
            this.rad9.TabStop = true;
            this.rad9.Tag = "8";
            this.rad9.Text = "RGB";
            this.rad9.UseVisualStyleBackColor = true;
            this.rad9.CheckedChanged += new System.EventHandler(this.rad1_CheckedChanged);
            // 
            // rad8
            // 
            this.rad8.AutoSize = true;
            this.rad8.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rad8.Location = new System.Drawing.Point(14, 227);
            this.rad8.Name = "rad8";
            this.rad8.Size = new System.Drawing.Size(84, 23);
            this.rad8.TabIndex = 7;
            this.rad8.TabStop = true;
            this.rad8.Tag = "7";
            this.rad8.Text = "플레인 2";
            this.rad8.UseVisualStyleBackColor = true;
            this.rad8.CheckedChanged += new System.EventHandler(this.rad1_CheckedChanged);
            // 
            // rad7
            // 
            this.rad7.AutoSize = true;
            this.rad7.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rad7.Location = new System.Drawing.Point(14, 199);
            this.rad7.Name = "rad7";
            this.rad7.Size = new System.Drawing.Size(84, 23);
            this.rad7.TabIndex = 6;
            this.rad7.TabStop = true;
            this.rad7.Tag = "6";
            this.rad7.Text = "플레인 1";
            this.rad7.UseVisualStyleBackColor = true;
            this.rad7.CheckedChanged += new System.EventHandler(this.rad1_CheckedChanged);
            // 
            // rad6
            // 
            this.rad6.AutoSize = true;
            this.rad6.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rad6.Location = new System.Drawing.Point(14, 171);
            this.rad6.Name = "rad6";
            this.rad6.Size = new System.Drawing.Size(84, 23);
            this.rad6.TabIndex = 5;
            this.rad6.TabStop = true;
            this.rad6.Tag = "5";
            this.rad6.Text = "플레인 0";
            this.rad6.UseVisualStyleBackColor = true;
            this.rad6.CheckedChanged += new System.EventHandler(this.rad1_CheckedChanged);
            // 
            // rad5
            // 
            this.rad5.AutoSize = true;
            this.rad5.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rad5.Location = new System.Drawing.Point(14, 143);
            this.rad5.Name = "rad5";
            this.rad5.Size = new System.Drawing.Size(145, 23);
            this.rad5.TabIndex = 4;
            this.rad5.TabStop = true;
            this.rad5.Tag = "4";
            this.rad5.Text = "베이어로부터 HSI";
            this.rad5.UseVisualStyleBackColor = true;
            this.rad5.CheckedChanged += new System.EventHandler(this.rad1_CheckedChanged);
            // 
            // rad4
            // 
            this.rad4.AutoSize = true;
            this.rad4.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rad4.Location = new System.Drawing.Point(14, 115);
            this.rad4.Name = "rad4";
            this.rad4.Size = new System.Drawing.Size(151, 23);
            this.rad4.TabIndex = 3;
            this.rad4.TabStop = true;
            this.rad4.Tag = "3";
            this.rad4.Text = "베이어로부터 RGB";
            this.rad4.UseVisualStyleBackColor = true;
            this.rad4.CheckedChanged += new System.EventHandler(this.rad1_CheckedChanged);
            // 
            // rad3
            // 
            this.rad3.AutoSize = true;
            this.rad3.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rad3.Location = new System.Drawing.Point(14, 87);
            this.rad3.Name = "rad3";
            this.rad3.Size = new System.Drawing.Size(144, 23);
            this.rad3.TabIndex = 2;
            this.rad3.TabStop = true;
            this.rad3.Tag = "2";
            this.rad3.Text = "베이어로부터 밝기";
            this.rad3.UseVisualStyleBackColor = true;
            this.rad3.CheckedChanged += new System.EventHandler(this.rad1_CheckedChanged);
            // 
            // rad2
            // 
            this.rad2.AutoSize = true;
            this.rad2.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rad2.Location = new System.Drawing.Point(14, 59);
            this.rad2.Name = "rad2";
            this.rad2.Size = new System.Drawing.Size(56, 23);
            this.rad2.TabIndex = 1;
            this.rad2.TabStop = true;
            this.rad2.Tag = "1";
            this.rad2.Text = "HSI";
            this.rad2.UseVisualStyleBackColor = true;
            this.rad2.CheckedChanged += new System.EventHandler(this.rad1_CheckedChanged);
            // 
            // rad1
            // 
            this.rad1.AutoSize = true;
            this.rad1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rad1.Location = new System.Drawing.Point(14, 31);
            this.rad1.Name = "rad1";
            this.rad1.Size = new System.Drawing.Size(55, 23);
            this.rad1.TabIndex = 0;
            this.rad1.TabStop = true;
            this.rad1.Tag = "0";
            this.rad1.Text = "밝기";
            this.rad1.UseVisualStyleBackColor = true;
            this.rad1.CheckedChanged += new System.EventHandler(this.rad1_CheckedChanged);
            // 
            // ImageConvertControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.Controls.Add(this.groupControl1);
            this.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "ImageConvertControl";
            this.Size = new System.Drawing.Size(338, 446);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            this.groupControl1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.GroupControl groupControl1;
        private System.Windows.Forms.RadioButton rad12;
        private System.Windows.Forms.RadioButton rad11;
        private System.Windows.Forms.RadioButton rad10;
        private System.Windows.Forms.RadioButton rad9;
        private System.Windows.Forms.RadioButton rad8;
        private System.Windows.Forms.RadioButton rad7;
        private System.Windows.Forms.RadioButton rad6;
        private System.Windows.Forms.RadioButton rad5;
        private System.Windows.Forms.RadioButton rad4;
        private System.Windows.Forms.RadioButton rad3;
        private System.Windows.Forms.RadioButton rad2;
        private System.Windows.Forms.RadioButton rad1;
    }
}
