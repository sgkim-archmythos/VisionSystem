using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Cognex.VisionPro;

namespace VisionSystem
{
    
    public partial class frmMDI : DevExpress.XtraEditors.XtraForm
    {
        frmMain _frmMain;
        int _nIdx;
        IniFiles ini = new IniFiles();

        public frmMDI(frmMain MainFrm, int nIdx)
        {
            InitializeComponent();
            MdiParent = MainFrm;
            _frmMain = MainFrm;
            _nIdx = nIdx;
            this.Text = "Camera #" + (_nIdx + 1).ToString();

            //AllowDrop = true;
        }

        protected override bool GetAllowSkin()
        {
            if (this.DesignMode) return false;
            return true;
        }
        private void frmMDI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.MdiFormClosing)
            {
                if (MessageBox.Show(Str.FormDel, "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                if (_frmMain._var._nScreenCnt > 0)
                {
                    _frmMain._var._nScreenCnt--;
                    ini.WriteIniFile("ScreenCnt", "Value", _frmMain._var._nScreenCnt.ToString(), _frmMain._var._strConfigPath, "Config.ini");
                }
            }
        }

        private void frmMDI_Resize(object sender, EventArgs e)
        {
            try
            {
                _frmMain._CAM[_nIdx].SetMenuPosition(this.Width);
            }
            catch { }
        }
    }
}