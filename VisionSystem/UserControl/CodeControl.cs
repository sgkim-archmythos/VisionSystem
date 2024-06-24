using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;

using System.IO;

using Cognex.VisionPro;

namespace VisionSystem
{
    public delegate void OnClickEvent(int nIdx, string strCode);
    public delegate void OnInsertVppEvent(int nIdx);
    public partial class CodeControl : DevExpress.XtraEditors.XtraUserControl
    {
        public OnClickEvent onClick;
        public OnInsertVppEvent OnInsertVpp;

        int _nIdx = 0;
        GlovalVar _var = new GlovalVar();
        IniFiles ini = new IniFiles();
        public CodeControl(int nIdx)
        {
            InitializeComponent();
            _nIdx = nIdx;
        }

        public void LoadSet(FileInfo fi)
        {
            try
            {
                var strFileName = Path.GetFileNameWithoutExtension(fi.FullName);
                var strPath = Path.GetDirectoryName(fi.FullName);
                lblCodeName.Text = strFileName;
                lblComment.Text = ini.ReadIniFile(strFileName,"Value", strPath, "Comment.ini");

                FileStream fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                Image img = Image.FromStream(fs);
                using (Bitmap bmpimg = new Bitmap(img))
                    cogDisp.Image = new CogImage24PlanarColor(bmpimg);

                img.Dispose();
                fs.Close();
                fs.Dispose();
            }
            catch { }
        }

        private void radUse_CheckedChanged(object sender, EventArgs e)
        {
            if (radUse.Checked)
            {
                lblCodeName.ForeColor = Color.Yellow;
                lblComment.ForeColor = Color.Yellow;

                using (Font font = new Font("Tahoma", 11, FontStyle.Bold))
                {
                    lblCodeName.Font = font;
                    lblComment.Font = font;
                }

                if (onClick != null)
                    onClick(_nIdx, lblCodeName.Text);
            }
            else
            {
                lblCodeName.ForeColor = Color.White;
                lblComment.ForeColor = Color.White;

                using (Font font = new Font("Tahoma", 11, FontStyle.Regular))
                {
                    lblCodeName.Font = font;
                    lblComment.Font = font;
                }
            }
        }

        private void lblCodeName_MouseClick(object sender, MouseEventArgs e)
        {
            radUse.PerformClick();
        }

        private void cogDisp_MouseDown(object sender, MouseEventArgs e)
        {
            radUse.PerformClick();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if(OnInsertVpp != null)
            {
                OnInsertVpp(_nIdx);
            }
        }
    }
}
