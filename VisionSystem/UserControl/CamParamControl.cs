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
using System.Threading;

using Cognex.VisionPro;

namespace VisionSystem
{
    public delegate void SendValueHandler(int nIdx, string strValue);
    public delegate void InsertVppHandler(int nIdx, int nIdx2);
    public partial class CamParamControl : DevExpress.XtraEditors.XtraUserControl
    {
        public SendValueHandler OnSendData;
        public InsertVppHandler OnLoadVpp;

        IniFiles ini = new IniFiles();
        frmMain _frmMain;
        CodeControl[] _codeControl = null;
        int _nIdx = 0;
        public string _strModel = "";
        bool _bLoadSame = false;
        DirectoryInfo[] _vppDirectoryInfos;
        int _nCodeCnt = 0;
        public string strAlign;
        public int nFontSize;
        public int nPosX;
        public int nPosY;


        public CamParamControl(frmMain MainFrm, int nidx, string strModel)
        {
            InitializeComponent();
            _nIdx = nidx;
            _frmMain = MainFrm;
            _strModel = strModel;
        }


        public void LoadSet(bool bModelSame)
        {
            txtGrabDelay.Text = ini.ReadIniFile("GrabDelay", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
            txtExpose.Text = ini.ReadIniFile("Expose", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
            lblCode.Text = ini.ReadIniFile("Code", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
            txtDefectCnt.Text = ini.ReadIniFile("DefectGrabCnt", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
            txtExposeIncrease.Text = ini.ReadIniFile("ExposeIncrease", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));

            bool.TryParse(ini.ReadIniFile("DefectInsp", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1)), out var bDefect);
            swDefect.IsOn = bDefect;

            bool.TryParse(ini.ReadIniFile("DefectBCR", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1)), out var bBCR);
            swBCR.IsOn = bBCR;

            bool.TryParse(ini.ReadIniFile("DefectBCRSwap", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1)), out var bBCRSwap);
            swBCRSwap.IsOn = bBCRSwap;

            bool.TryParse(ini.ReadIniFile("DefectAlign", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1)), out var bAlign);
            swAlign.IsOn = bAlign;

            if (swAlign.IsOn)
                pnlAlign.Visible = true;
                
            else
                pnlAlign.Visible = false;

            txtBCRData.Text = ini.ReadIniFile("BCRData", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
            txtBCRLen.Text = ini.ReadIniFile("BCRLen", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));

            bool.TryParse(ini.ReadIniFile("PinChange", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1)), out var bPin);
            swPin.IsOn = bPin;

            if (swPin.IsOn)
                txtPinMaster.Visible = true;
            else
                txtPinMaster.Visible = false;

            txtPinProhibit.Text = ini.ReadIniFile("PinProhibit", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
            txtAlignMasterX.Text = ini.ReadIniFile("AlignMasterX", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
            txtAlignMasterY.Text = ini.ReadIniFile("AlignMasterY", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
            txtAlignMasterR.Text = ini.ReadIniFile("AlignMasterR", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
            txtAlignOffsetX.Text = ini.ReadIniFile("AlignOffsetX", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
            txtAlignOffsetY.Text = ini.ReadIniFile("AlignOffsetY", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
            txtAlignOffsetR.Text = ini.ReadIniFile("AlignOffsetR", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
            txtResolution.Text = ini.ReadIniFile("Resolution", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
            txtPinMaster.Text = ini.ReadIniFile("PinMaster", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));

            bool.TryParse(ini.ReadIniFile("Xreversal", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1)), out var bXreversal);
            swXreversal.IsOn = bXreversal;
            bool.TryParse(ini.ReadIniFile("Yreversal", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1)), out var bYreversal);
            swYreversal.IsOn = bYreversal;
            bool.TryParse(ini.ReadIniFile("Zreversal", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1)), out var bZreversal);
            swZreversal.IsOn = bZreversal;
            bool.TryParse(ini.ReadIniFile("XYreversal", "Value", _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1)), out var bXYreversal);
            swXYreversal.IsOn = bXYreversal;

            _frmMain._ModelParam[_nIdx].bXreversal = swXreversal.IsOn;
            _frmMain._ModelParam[_nIdx].bYreversal = swYreversal.IsOn;
            _frmMain._ModelParam[_nIdx].bZreversal = swZreversal.IsOn;
            _frmMain._ModelParam[_nIdx].bXYreversal = swXYreversal.IsOn;

            LoadCodeTable();
        }

        private void LoadCodeTable()
        {
            try
            {
                Invoke(new EventHandler(delegate
                {
                    panel1.Visible = false;
                    panel1.Controls.Clear();

                    gpCamera.Text = string.Format("Camera_{0:D2}", _nIdx + 1);
                    DirectoryInfo dr = new DirectoryInfo(_frmMain._var._strMasterImagePath);
                    if (!dr.Exists)
                        return;

                    _vppDirectoryInfos = dr.GetDirectories();

                    FileInfo[] fi = null;
                    for (int i = 0; i < _vppDirectoryInfos.Length; i++)
                    {
                        if (_vppDirectoryInfos[i].Name.Contains(string.Format("Camera_{0:D2}", _nIdx + 1)))
                        {
                            fi = _vppDirectoryInfos[i].GetFiles("*.bmp");
                            _nCodeCnt = fi.Length;

                            _codeControl = null;
                            _codeControl = new CodeControl[_nCodeCnt];

                            for (int j = 0; j < _nCodeCnt; j++)
                            {
                                _codeControl[j] = new CodeControl(j);
                                _codeControl[j].onClick = OnClick;
                                _codeControl[j].OnInsertVpp = OnInsertVpp;
                                panel1.Controls.Add(_codeControl[j]);
                                _codeControl[j].Location = new Point(1, 1 + (j * _codeControl[j].Height) + 8);
                                _codeControl[j].LoadSet(fi[j]);

                                if (lblCode.Text == _codeControl[j].lblCodeName.Text)
                                    _codeControl[j].radUse.Checked = true;
                            }
                            panel1.Visible = true;
                            break;
                        }
                    }

                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Set Code Table Error : " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnClick(int nIdx, string strCode)
        {
            lblCode.Text = strCode;
            StatusChange(nIdx);
        }

        private void OnInsertVpp(int nIdx)
        {
            if (OnLoadVpp != null)
                OnLoadVpp(_nIdx, nIdx);
        }

        private void StatusChange(int nIdx)
        {
            try
            {
                for (int i = 0; i < _nCodeCnt; i++)
                {
                    if (i != nIdx)
                    {
                        _codeControl[i].radUse.Checked = false;
                        _codeControl[i].ActiveControl = null;
                    }
                    else
                    {
                        _codeControl[i].radUse.Checked = true;
                        _codeControl[i].ActiveControl = _codeControl[i].pnl;
                    }
                }
            }
            catch { }
        }

        public bool SaveSet()
        {
            try
            {
                int.TryParse(txtGrabDelay.Text, out _frmMain._ModelParam[_nIdx].nGrabdelay);
                double.TryParse(txtExpose.Text, out _frmMain._ModelParam[_nIdx].dExpose);
                _frmMain._ModelParam[_nIdx].strCode = lblCode.Text;
                int.TryParse(txtDefectCnt.Text, out _frmMain._ModelParam[_nIdx].nDefectCnt);
                _frmMain._ModelParam[_nIdx].strExposeInc = txtExposeIncrease.Text;
                _frmMain._ModelParam[_nIdx].bDefectInsp = swDefect.IsOn;
                _frmMain._ModelParam[_nIdx].bBCRInsp = swBCR.IsOn;
                _frmMain._ModelParam[_nIdx].bBCRInspSwap = swBCRSwap.IsOn;
                _frmMain._ModelParam[_nIdx].bAlingInsp = swAlign.IsOn;
                _frmMain._ModelParam[_nIdx].strBCRData = txtBCRData.Text;
                _frmMain._ModelParam[_nIdx].strBCRLen = txtBCRLen.Text;
                _frmMain._ModelParam[_nIdx].bPinChange = swPin.IsOn;
                double.TryParse(txtAlignMasterX.Text, out _frmMain._ModelParam[_nIdx].dAlignMasterX);
                double.TryParse(txtAlignMasterY.Text, out _frmMain._ModelParam[_nIdx].dAlignMasterY);
                double.TryParse(txtAlignMasterR.Text, out _frmMain._ModelParam[_nIdx].dAlignMasterR);
                double.TryParse(txtAlignOffsetX.Text, out _frmMain._ModelParam[_nIdx].dAlignOffsetX);
                double.TryParse(txtAlignOffsetY.Text, out _frmMain._ModelParam[_nIdx].dAlignOffsetY);
                double.TryParse(txtAlignOffsetR.Text, out _frmMain._ModelParam[_nIdx].dAlignOffsetR);
                double.TryParse(txtResolution.Text, out _frmMain._ModelParam[_nIdx].dResoluton);
                _frmMain._ModelParam[_nIdx].strPinMaster = txtPinMaster.Text;
                _frmMain._ModelParam[_nIdx].strPinProhibit = txtPinProhibit.Text;
                _frmMain._ModelParam[_nIdx].bXreversal = swXreversal.IsOn;
                _frmMain._ModelParam[_nIdx].bYreversal = swYreversal.IsOn;
                _frmMain._ModelParam[_nIdx].bZreversal = swZreversal.IsOn;
                _frmMain._ModelParam[_nIdx].bXYreversal = swXYreversal.IsOn;
                _frmMain._CAM[_nIdx]._modelParam = _frmMain._ModelParam[_nIdx];

                ini.WriteIniFile("GrabDelay", "Value", txtGrabDelay.Text, _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("Expose", "Value", txtExpose.Text, _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("Code", "Value", lblCode.Text, _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("DefectGrabCnt", "Value", txtDefectCnt.Text, _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("ExposeIncrease", "Value", txtExposeIncrease.Text, _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("DefectInsp", "Value", swDefect.IsOn.ToString(), _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("DefectBCR", "Value", swBCR.IsOn.ToString(), _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("DefectBCRSwap", "Value", swBCRSwap.IsOn.ToString(), _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("DefectAlign", "Value", swAlign.IsOn.ToString(), _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("BCRData", "Value", txtBCRData.Text, _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("BCRLen", "Value", txtBCRLen.Text, _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("PinChange", "Value", swPin.IsOn.ToString(), _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("PinProhibit", "Value", txtPinProhibit.Text, _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("AlignMasterX", "Value", txtAlignMasterX.Text, _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("AlignMasterY", "Value", txtAlignMasterY.Text, _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("AlignMasterR", "Value", txtAlignMasterR.Text, _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("AlignOffsetX", "Value", txtAlignOffsetX.Text, _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("AlignOffsetY", "Value", txtAlignOffsetY.Text, _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("AlignOffsetR", "Value", txtAlignOffsetR.Text, _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("Resolution", "Value", txtResolution.Text, _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("PinMaster", "Value", txtPinMaster.Text, _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("Xreversal", "Value", swXreversal.IsOn.ToString(), _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("Yreversal", "Value", swYreversal.IsOn.ToString(), _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("Zreversal", "Value", swZreversal.IsOn.ToString(), _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
                ini.WriteIniFile("XYreversal", "Value", swXYreversal.IsOn.ToString(), _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));

                _frmMain.AddMsg(string.Format("#{0} Camera Recipe Save Complete", _nIdx), Color.GreenYellow, false, false, frmMain.MsgType.Alarm);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void btnCode_Click(object sender, EventArgs e)
        {
            if (!fypnl.IsPopupOpen)
            {
                LoadCodeTable();

                fypnl.ShowBeakForm();
                var nCodeNo = GetCodeNo();
                if (nCodeNo > -1)
                    StatusChange(GetCodeNo());
            }
            else
                fypnl.HideBeakForm();
        }

        private int GetCodeNo()
        {
            if (lblCode.Text == "")
                return -1;

            var nSel = 0;
            var strCode = lblCode.Text;
            for (int i = 0; i < _nCodeCnt; i++)
            {
                if (_codeControl[i].lblCodeName.Text.Contains(strCode))
                {
                    nSel = i;
                    break;
                }
            }

            return nSel;
        }

        private void OnDoubleClidk(string strCode)
        {
            lblCode.Text = strCode;
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            try
            {
                if (fypnl.IsPopupOpen)
                    fypnl.HideBeakForm();

                if (_frmMain._var._listCode.Count == 0)
                    return;

                for (int i = 0; i < _frmMain._var._listCode.Count; i++)
                {
                    if (_codeControl[i] != null)
                    {
                        if (_codeControl[i].radUse.Checked)
                        {
                            lblCode.Text = _codeControl[i].lblCodeName.Text;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //   MessageBox.Show("Set Code Table Error : " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (fypnl.IsPopupOpen)
            {
                for (int i = 0; i < _nCodeCnt; i++)
                    _codeControl[i].radUse.Checked = false;

                fypnl.HideBeakForm();
            }

        }

        private void swDefect_Toggled(object sender, EventArgs e)
        {

            //ini.WriteIniFile("DefectInsp", "Value", swDefect.IsOn.ToString(), _frmMain._var._strModelPath + "\\" + _strModel, string.Format("CAM{0}.ini", _nIdx + 1));
        }

        private void swAlign_EditValueChanged(object sender, EventArgs e)
        {
            if (swAlign.IsOn)
            {
                pnlAlign.Visible = true;
                labelControl25.Visible = true;
                txtResolution.Visible = true;
            }
            else
            {
                pnlAlign.Visible = false;
                labelControl25.Visible = false;
                txtResolution.Visible = false;
            }
        }

        private void swPin_EditValueChanged(object sender, EventArgs e)
        {
            if (swPin.IsOn)
                txtPinMaster.Visible = true;
            else
                txtPinMaster.Visible = false;
        }

        private void swBCR_EditValueChanged(object sender, EventArgs e)
        {
            if (swBCR.IsOn)
            {
                labelControl31.Visible = true;
                swBCRSwap.Visible = true;
            }
            else
            {
                labelControl31.Visible = false;
                swBCRSwap.Visible = false;
            }
        }
    }
}
