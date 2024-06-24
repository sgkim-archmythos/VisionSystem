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

namespace VisionSystem
{
    public partial class PrintParamControl : DevExpress.XtraEditors.XtraUserControl
    {
        IniFiles ini = new IniFiles();
        frmMain _frmMain;
        public string _strModel = "";
        int _nIdx = 0;


        public PrintParamControl(frmMain MainFrm, int nidx, string strModel)
        {
            InitializeComponent();
            _frmMain = MainFrm;
            _nIdx = nidx;
            _strModel = strModel;
        }

        public bool SaveSet()
        {
            try
            {

                ini.WriteIniFile("MIP", "Value", txtMIP.Text, _frmMain._var._strModelPath + "\\" + _strModel, string.Format("PRINT{0}.ini", 1));
                ini.WriteIniFile("Count", "Value", txtCount.Text, _frmMain._var._strModelPath + "\\" + _strModel, string.Format("PRINT{0}.ini", 1));


                _frmMain.AddMsg(string.Format("#{0} Camera Recipe Save Complete", 1), Color.GreenYellow, false, false, frmMain.MsgType.Alarm);
                MessageBox.Show("Print Information Save");
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void LoadSet()
        {
            txtMIP.Text = ini.ReadIniFile("MIP", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("PRINT{0}.ini", 1));
            txtCount.Text = ini.ReadIniFile("Count", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("PRINT{0}.ini", 1));
        }

        
    }
}
