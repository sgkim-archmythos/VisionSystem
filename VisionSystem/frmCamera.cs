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
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.PMAlign;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;

using Basler.Pylon;

namespace VisionSystem
{
    public partial class frmCamera : DevExpress.XtraEditors.XtraForm
    {
        frmMain _frmMain;
        //frmWait _frmWait;
        IniFiles ini = new IniFiles();

        bool _bFinding = false;
        bool _bLive = false;
        int _nLiveIndex = 0;
        public int _nIdx = -1;

       // bool _bCamConnect = false;
        
        public frmCamera(frmMain MainFrm)
        {
            InitializeComponent();
            _frmMain = MainFrm;
        }

        public void InitControl()
        {
            cbCamNo.Properties.Items.Clear();
            cbCogSerial.Properties.Items.Clear();
            cbCogFormat.Properties.Items.Clear();

            cbBaslerSerial.Properties.Items.Clear();
            lblBalserIP.Text = "-";

            radCognex.Checked = false;
            radBalser.Checked = false;

            tabCam.Visible = false;
            btnGrab.Enabled = false;
            btnLive.Enabled = false;
            lblConnect.BackColor = Color.Red;
            lblConnect.ForeColor = Color.White;
            lblConnect.Text = "Disconnected";
        }

        public void LoadSet()
        {
            InitControl();
            //int nCnt = 0;
            //for (int i=0; i<20; i++)
            //{
            //    //if (_frmMain._camSet[i]._bConnect)
            //    //{
            //    //    nCnt++;
            //    //}
            //    nCnt = _frmMain._mFrameGrabbers.Count;
            //    if (!(_frmMain._camParam[i].strCopy != "-1"))
            //        btnConnect.Visible = false;
            //}
            //if (_frmMain._camParam[_nIdx].strCopy != "-1")
            //{
            //    btnConnect.Enabled = false;
            //    btnGrab.Enabled = false;
            //    btnLive.Enabled = false;
            //}
               

            //if (nCnt != 0)
            //{
                for (int i = 0; i < 20; i++)
                {
                    if (_frmMain._camParam[i].strCopy == "-1")
                        cbCamNo.Properties.Items.Add(string.Format("#{0} Camera", i + 1));
                }
            //}

            if (_frmMain._camParam[_nIdx].strCopy != "-1" && _frmMain._camParam[_nIdx].strCopy != null)
            {
                cbCamNo.SelectedIndex = Convert.ToInt32(_frmMain._camParam[_nIdx].strCopy);
                btnConnect.Enabled = false;
            }
            else
                btnConnect.Enabled = true;

            _frmMain._camSet[_nIdx]._GrabFunc = GrabFunc;

            if (_frmMain._camSet[_nIdx]._bConnect)
            {
                lblConnect.Text = "Connected";
                lblConnect.BackColor = Color.Lime;
                lblConnect.ForeColor = Color.Black;
                btnGrab.Enabled = true;
                btnLive.Enabled = true;

                btnConnect.Text = "Disconnect";
            }
            else
            {
                lblConnect.Text = "Disconnected";
                lblConnect.BackColor = Color.Red;
                lblConnect.ForeColor = Color.White;

                btnConnect.Text = "Connect";
            }

            SetCameraInfo();
            setParam();
            ActiveControl = labelControl1;
        }

        private void setParam()
        {
            if (_frmMain._camParam != null)
            {
                if (_nIdx != -1)
                {
                    //if (!_frmMain._camSet[_nIdx]._bConnect)
                    //    return;

                    if (string.IsNullOrEmpty(_frmMain._camParam[_nIdx].strCamType))
                        return;

                    if (int.Parse(_frmMain._camParam[_nIdx].strCamType) == 0)
                    {
                        radCognex.Checked = true;
                        for (int i = 0; i < _frmMain._mFrameGrabbers.Count; i++) {
                            if(_frmMain._mFrameGrabbers[i].SerialNumber == _frmMain._camParam[_nIdx].strCamSerial)
                                cbCogSerial.SelectedIndex = i;
                        }
                        cbCogFormat.SelectedIndex = int.Parse(_frmMain._camParam[_nIdx].strCamFormat);
                        txtCogExpose.Text = _frmMain._camParam[_nIdx].dExpose.ToString();
                        txtCogBright.Text = _frmMain._camParam[_nIdx].dBright.ToString();
                        txtCogContrast.Text = _frmMain._camParam[_nIdx].dContract.ToString();
                        txtCogTimeout.Text = _frmMain._camParam[_nIdx].nTimeout.ToString();
                        chkCognexTime.Checked = _frmMain._camParam[_nIdx].bTiime;

                        txtCogExpose.Enabled = true;
                        txtCogBright.Enabled = true;
                        txtCogContrast.Enabled = true;
                        txtCogTimeout.Enabled = true;
                        chkCognexTime.Enabled = true;
                    }
                    else if (int.Parse(_frmMain._camParam[_nIdx].strCamType) == 1)
                    {
                        radBalser.Checked = true;
                        cbBaslerSerial.SelectedIndex = int.Parse(_frmMain._camParam[_nIdx].strCamSerial);
                        lblBalserIP.Text = _frmMain._camParam[_nIdx].strIP;
                        txtBalserExpose.Text = _frmMain._camParam[_nIdx].dExpose.ToString();
                        txtBaslerContrast.Text = _frmMain._camParam[_nIdx].dContract.ToString();
                        txtBalserTimeout.Text = _frmMain._camParam[_nIdx].nTimeout.ToString();

                        txtBalserExpose.Enabled = true;
                        txtBaslerContrast.Enabled = true;
                        txtBalserTimeout.Enabled = true;
                        sliderExpose.Enabled = true;
                        sliderGain.Enabled = true;
                    }
                    else
                    {
                        radHIK.Checked = true;
                        cbHIKSerial.SelectedIndex = int.Parse(_frmMain._camParam[_nIdx].strCamSerial);
                        lblHIKIP.Text = _frmMain._camParam[_nIdx].strIP;
                        txtHIKExpose.Text = _frmMain._camParam[_nIdx].dExpose.ToString();
                        txtBalserExpose.Enabled = true;
                        sliderHIKExpose.Enabled = true;
                        sliderHIKGain.Enabled = true;
                    }
                }
            }
        }

        private static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }

            return DateTime.Now;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (_bLive)
                _frmMain._camSet[_nIdx].LiveView(_nIdx, cogView, false);

            _frmMain._CAM[_nIdx]._camSet._GrabFunc = _frmMain.GrabFunc;
            Close();
        }

        private void radCognex_CheckedChanged(object sender, EventArgs e)
        {
            tabCam.Visible = true;
            if (radCognex.Checked)
            {
                radCognex.ForeColor = Color.Yellow;

                tabCam.TabPages[0].PageVisible = true;
                tabCam.TabPages[1].PageVisible = false;
                tabCam.TabPages[2].PageVisible = false;

                btnConnect.Visible = true;
                btnGrab.Visible = true;
                btnLive.Visible = true;
                cogView.Image = null;

                radBalser.ForeColor = Color.White;
                radHIK.ForeColor = Color.White;
            }
            else if (radBalser.Checked)
            {
                radBalser.ForeColor = Color.Yellow;
                tabCam.TabPages[0].PageVisible = false;
                tabCam.TabPages[1].PageVisible = true;
                tabCam.TabPages[2].PageVisible = false;
                btnConnect.Visible = true;
                btnGrab.Visible = true;
                btnLive.Visible = true;
                radCognex.ForeColor = Color.White;
                radHIK.ForeColor = Color.White;
            }
            else
            {
                radHIK.ForeColor = Color.Yellow;
                tabCam.TabPages[0].PageVisible = false;
                tabCam.TabPages[1].PageVisible = false;
                tabCam.TabPages[2].PageVisible = true;

                btnConnect.Visible = true;
                btnGrab.Visible = true;
                btnLive.Visible = true;

                radCognex.ForeColor = Color.White;
                radBalser.ForeColor = Color.White;
            }
        }

        public string SetCameraInfo()
        {
            string strRes = "";
            try
            {
                int nCnt = 0;
                int nCamCnt = _frmMain._mFrameGrabbers.Count;

                cbCogSerial.Properties.Items.Clear();
                cbCogFormat.Properties.Items.Clear();
                cbBaslerSerial.Properties.Items.Clear();
                cbHIKSerial.Properties.Items.Clear();

                for (int i = 0; i < nCamCnt; i++)
                {
                    if (_frmMain._camSet[i]._cams == null)
                        cbCogSerial.Properties.Items.Add(string.Format("{0}:{1}", _frmMain._mFrameGrabbers[i].Name, _frmMain._mFrameGrabbers[i].SerialNumber));
                }

                nCnt = nCamCnt;
                nCamCnt = _frmMain._cameras.Count;
                string strSerial;

                for (int i = 0; i < nCamCnt; i++)
                {
                    if (_frmMain._camSet[i]._AcqFifo == null)
                    {
                        strSerial = _frmMain._cam.GetSerial(_frmMain._cameras[i]);
                        cbBaslerSerial.Properties.Items.Add(strSerial);

                        if (cbCogSerial.Properties.Items.Count > 0)
                        {
                            for (int j = 0; j < cbCogSerial.Properties.Items.Count; j++)
                            {
                                if (!cbCogSerial.Properties.Items[j].ToString().Contains(strSerial))
                                    nCnt++;
                            }
                        }
                    }
                }

                nCamCnt = (int)_frmMain.m_stDeviceList.nDeviceNum;
                strSerial = "";

                for (int i=0; i<nCamCnt; i++)
                {
                    MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(_frmMain.m_stDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));

                    if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                    {
                        MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                        cbHIKSerial.Properties.Items.Add(gigeInfo.chSerialNumber);
                    }
                }

                lblCamCnt.Text = nCnt.ToString();
                _frmMain._nFindCam = nCnt;
            }
            catch (Exception ex)
            {
                strRes = "Camera Set Error  : " + ex.Message;
                _frmMain.AddMsg("Camera Set Error  : " + ex.Message, Color.Red, false, false, frmMain.MsgType.Alarm);
            }

            return strRes;
        }

        private void btnCamSearch_Click(object sender, EventArgs e)
        {
            if (!_bFinding)
            {
                _bFinding = true;

                splashScreenManager1.ShowWaitForm();
            }

            Delay(200);

            _frmMain.InitCam();
            SetCameraInfo();
            //_frmMain.ScreenSet();

            splashScreenManager1.CloseWaitForm();
            _bFinding = false;
        }

        private void chkCognexTime_CheckedChanged(object sender, EventArgs e)
        {
            if (chkCognexTime.Checked)
            {
                using (Font font = new Font("Tahoma", 10, FontStyle.Bold))
                    chkCognexTime.Font = font;

                chkCognexTime.ForeColor = Color.Yellow;
            }
            else
            {
                using (Font font = new Font("Tahoma", 10, FontStyle.Regular))
                    chkCognexTime.Font = font;

                chkCognexTime.ForeColor = Color.White;
            }

            if (_frmMain._camSet[_nIdx]._bConnect)
            {
                _frmMain._camSet[_nIdx]._AcqFifo.TimeoutEnabled = chkCognexTime.Enabled;
                _frmMain._camSet[_nIdx]._AcqFifo.Prepare();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnConnect.Text == "Connect")
                {
                    if (radCognex.Checked)
                    {
                        if (cbCamNo.SelectedIndex != -1)
                        {
                            if (_frmMain._camSet[cbCamNo.SelectedIndex]._bConnect)
                            {
                                _frmMain._camSet[cbCamNo.SelectedIndex].LiveView(_nIdx, cogView, false);

                                txtCogExpose.Enabled = true;
                                txtCogBright.Enabled = true;
                                txtCogContrast.Enabled = true;
                                txtCogTimeout.Enabled = true;
                                chkCognexTime.Enabled = true;

                                double[] dParam = _frmMain._camSet[_nIdx].GetCamParam();

                                if (dParam.Length >2)
                                {
                                    txtCogExpose.Text = string.Format("{0:F3}", dParam[0]);
                                    txtCogBright.Text = string.Format("{0:F3}", dParam[1]);
                                    txtCogContrast.Text = string.Format("{0:F3}", dParam[2]);
                                }
                                
                                txtCogTimeout.Text = _frmMain._camSet[_nIdx]._AcqFifo.Timeout.ToString();
                                chkCognexTime.Checked = _frmMain._camSet[_nIdx]._AcqFifo.TimeoutEnabled;
                                btnGrab.Enabled = true;
                                btnLive.Enabled = true;

                                _frmMain._camSet[_nIdx]._bConnect = true;
                                _frmMain._camSet[_nIdx]._GrabFunc = GrabFunc;

                                //_bCamConnect = true;
                                lblConnect.Text = "Connected";
                                lblConnect.BackColor = Color.Lime;
                                lblConnect.ForeColor = Color.Black;

                                btnConnect.Text = "Disconnect";
                            }
                            else
                            {
                                //_bCamConnect = false;
                                lblConnect.Text = "Disconnected";
                                lblConnect.BackColor = Color.Red;
                                lblConnect.ForeColor = Color.White;
                            }
                        }
                        else
                        {
                            _frmMain._camSet[_nIdx].LiveView(_nIdx, cogView, false);
                            _frmMain._camSet[_nIdx]._nIdx = _nIdx;

                            if (_frmMain._camParam[_nIdx].strCamType == "0")
                            {
                                if (_frmMain._camSet[_nIdx]._bConnect)
                                {
                                    MessageBox.Show("The camera is already connecting", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                            }

                            if (cbCogSerial.SelectedIndex == -1)
                            {
                                MessageBox.Show("Please select a camera Serial", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            if (cbCogFormat.SelectedIndex == -1)
                            {
                                MessageBox.Show("Please select a camera VideoFormat", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            _frmMain._camParam[_nIdx].strCamType = "0";
                            //_frmMain._camParam[_nIdx].strCamSerial = cbCogSerial.SelectedIndex.ToString();
                            string[] _CamSerial = cbCogSerial.SelectedItem.ToString().Split(':');
                            _frmMain._camParam[_nIdx].strCamSerial = _CamSerial[_CamSerial.Length-1];
                            _frmMain._camParam[_nIdx].strCamFormat = cbCogFormat.SelectedIndex.ToString();
                            _frmMain._camSet[_nIdx].CamConnect(_nIdx, _frmMain._camParam, _frmMain._mFrameGrabbers, _frmMain._cameras, _frmMain.m_stDeviceList, _frmMain._ModelParam[_nIdx]);
                            _frmMain._camParam[_nIdx].strCopy = cbCamNo.SelectedIndex.ToString();

                            if (_frmMain._camSet[_nIdx]._bConnect)
                            {
                                txtCogExpose.Enabled = true;
                                txtCogBright.Enabled = true;
                                txtCogContrast.Enabled = true;
                                txtCogTimeout.Enabled = true;
                                chkCognexTime.Enabled = true;

                                double[] dParam = _frmMain._camSet[_nIdx].GetCamParam();

                                if (dParam.Length > 2)
                                {
                                    txtCogExpose.Text = string.Format("{0:F3}", dParam[0]);
                                    txtCogBright.Text = string.Format("{0:F3}", dParam[1]);
                                    txtCogContrast.Text = string.Format("{0:F3}", dParam[2]);
                                }

                                txtCogTimeout.Text = _frmMain._camSet[_nIdx]._AcqFifo.Timeout.ToString();
                                chkCognexTime.Checked = _frmMain._camSet[_nIdx]._AcqFifo.TimeoutEnabled;
                                btnGrab.Enabled = true;
                                btnLive.Enabled = true;

                                _frmMain._camSet[_nIdx]._GrabFunc = GrabFunc;
                                lblConnect.Text = "Connected";
                                lblConnect.BackColor = Color.Lime;
                                lblConnect.ForeColor = Color.Black;

                                btnConnect.Text = "Disconnect";
                            }
                            else
                            {
                                lblConnect.Text = "Disconnected";
                                lblConnect.BackColor = Color.Red;
                                lblConnect.ForeColor = Color.White;
                            }
                        }
                    }
                    else if (radBalser.Checked)
                    {
                        if (cbCamNo.SelectedIndex != -1)
                        {
                            if (_frmMain._camSet[cbCamNo.SelectedIndex]._bConnect)
                            {
                                _frmMain._camSet[cbCamNo.SelectedIndex].LiveView(_nIdx, cogView, false);
                                txtBalserExpose.Enabled = true;
                                txtBaslerContrast.Enabled = true;
                                txtBalserTimeout.Enabled = true;
                                sliderExpose.Enabled = true;
                                sliderGain.Enabled = true;


                                double[] dParam = _frmMain._camSet[_nIdx].GetCamParam();

                                if (dParam.Length > 2)
                                {
                                    sliderExpose.Minimum = (int)dParam[3];
                                    sliderExpose.Maximum = (int)dParam[4];

                                    sliderGain.Minimum = (int)dParam[7];
                                    sliderGain.Maximum = (int)dParam[8];

                                    txtBalserExpose.Text = dParam[0].ToString();
                                    txtBalserTimeout.Text = dParam[1].ToString();
                                    txtBaslerContrast.Text = dParam[2].ToString();
                                }

                                btnGrab.Enabled = true;
                                btnLive.Enabled = true;

                                _frmMain._camSet[_nIdx]._bConnect = true;
                                _frmMain._camSet[_nIdx]._GrabFunc = GrabFunc;

                                lblConnect.Text = "Connected";
                                lblConnect.BackColor = Color.Lime;
                                lblConnect.ForeColor = Color.Black;

                                btnConnect.Text = "Disconnect";
                            }
                            else
                            {
                                lblConnect.Text = "Disconnected";
                                lblConnect.BackColor = Color.Red;
                                lblConnect.ForeColor = Color.White;
                            }
                        }
                        else
                        {
                            _frmMain._camSet[_nIdx].LiveView(_nIdx, cogView, false);

                            if (_frmMain._camParam[_nIdx].strCamType == "1")
                            {
                                if (_frmMain._camSet[_nIdx]._bConnect)
                                {
                                    MessageBox.Show("The camera is already connecting", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                            }

                            if (cbBaslerSerial.SelectedIndex == -1)
                            {
                                MessageBox.Show("Please select a camera Serial", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            _frmMain._camParam[_nIdx].strCamType = "1";
                            _frmMain._camParam[_nIdx].strCamSerial = cbBaslerSerial.SelectedIndex.ToString();
                            _frmMain._camParam[_nIdx].strIP = lblBalserIP.Text;
                            _frmMain._camParam[_nIdx].strCamFormat = "0";
                            _frmMain._camParam[_nIdx].strCopy = cbCamNo.SelectedIndex.ToString();

                            _frmMain._camSet[_nIdx].CamConnect(_nIdx, _frmMain._camParam, _frmMain._mFrameGrabbers, _frmMain._cameras, _frmMain.m_stDeviceList, _frmMain._ModelParam[_nIdx]);

                            if (_frmMain._camSet[_nIdx]._bConnect)
                            {
                                txtBalserExpose.Enabled = true;
                                txtBaslerContrast.Enabled = true;
                                txtBalserTimeout.Enabled = true;
                                sliderExpose.Enabled = true;
                                sliderGain.Enabled = true;

                                double[] dParam = _frmMain._camSet[_nIdx].GetCamParam();

                                if (dParam.Length > 2)
                                {
                                    sliderExpose.Minimum = (int)dParam[3];
                                    sliderExpose.Maximum = (int)dParam[4];

                                    sliderGain.Minimum = (int)dParam[7];
                                    sliderGain.Maximum = (int)dParam[8];

                                    txtBalserExpose.Text = dParam[0].ToString();
                                    txtBalserTimeout.Text = dParam[1].ToString();
                                    txtBaslerContrast.Text = dParam[2].ToString();
                                }

                                btnGrab.Enabled = true;
                                btnLive.Enabled = true;

                                _frmMain._camSet[_nIdx]._GrabFunc = GrabFunc;

                                lblConnect.Text = "Connected";
                                lblConnect.BackColor = Color.Lime;
                                lblConnect.ForeColor = Color.Black;
                                btnConnect.Text = "Disconnect";
                                _frmMain.AddMsg(string.Format("Camera #{0} Connected", _nIdx + 1), Color.GreenYellow, true, false, frmMain.MsgType.Alarm);
                            }
                            else
                            {
                                //_bCamConnect = false;
                                lblConnect.Text = "Disconnected";
                                lblConnect.BackColor = Color.Red;
                                lblConnect.ForeColor = Color.White;
                                _frmMain.AddMsg(string.Format("Camera #{0} Disconnected", _nIdx + 1), Color.Red, true, true, frmMain.MsgType.Alarm);
                            }
                        }
                    }
                    else
                    {
                        if (cbCamNo.SelectedIndex != -1)
                        {
                            if (_frmMain._camSet[_nIdx]._bConnect)
                            {
                                _frmMain._camSet[cbCamNo.SelectedIndex].LiveView(_nIdx, cogView, false);

                                txtHIKExpose.Enabled = true;

                                double[] dParam = _frmMain._camSet[_nIdx].GetCamParam();

                                if (dParam.Length >= 2)
                                {
                                    txtHIKExpose.Text = string.Format("{0:D}", (int)dParam[0]);
                                }
                                
                                btnGrab.Enabled = true;
                                btnLive.Enabled = true;

                                _frmMain._camSet[_nIdx]._bConnect = true;
                                _frmMain._camSet[_nIdx]._GrabFunc = GrabFunc;

                                lblConnect.Text = "Connected";
                                lblConnect.BackColor = Color.Lime;
                                lblConnect.ForeColor = Color.Black;

                                btnConnect.Text = "Disconnect";
                            }
                            else
                            {
                                lblConnect.Text = "Disconnected";
                                lblConnect.BackColor = Color.Red;
                                lblConnect.ForeColor = Color.White;
                            }
                        }
                        else
                        {
                            _frmMain._camSet[_nIdx].LiveView(_nIdx, cogView, false);
                            _frmMain._camSet[_nIdx]._nIdx = _nIdx;

                            if (_frmMain._camParam[_nIdx].strCamType == "2")
                            {
                                if (_frmMain._camSet[_nIdx]._bConnect)
                                {
                                    MessageBox.Show("The camera is already connecting", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                            }

                            if (cbHIKSerial.SelectedIndex == -1)
                            {
                                MessageBox.Show("Please select a camera Serial", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            _frmMain._camParam[_nIdx].strCamType = "2";
                            _frmMain._camParam[_nIdx].strCamSerial = cbHIKSerial.SelectedIndex.ToString();
                            _frmMain._camParam[_nIdx].strIP = lblHIKIP.Text;
                            _frmMain._camSet[_nIdx].CamConnect(_nIdx, _frmMain._camParam, _frmMain._mFrameGrabbers, _frmMain._cameras, _frmMain.m_stDeviceList, _frmMain._ModelParam[_nIdx]);
                            _frmMain._camParam[_nIdx].strCopy = cbCamNo.SelectedIndex.ToString();

                            if (_frmMain._camSet[_nIdx]._bConnect)
                            {
                                txtHIKExpose.Enabled = true;
                                double[] dParam = _frmMain._camSet[_nIdx].GetCamParam();

                                if (dParam.Length >= 2)
                                {
                                    txtHIKExpose.Text = string.Format("{0:D}", (int)dParam[0]);
                                    //sliderHIKExpose.Minimum = (int)dParam[3];
                                    //sliderHIKExpose.Maximum = (int)dParam[4];
                                    sliderHIKExpose.Value = (int)dParam[0];
                                }

                                btnGrab.Enabled = true;
                                btnLive.Enabled = true;

                                _frmMain._camSet[_nIdx]._GrabFunc = GrabFunc;

                                lblConnect.Text = "Connected";
                                lblConnect.BackColor = Color.Lime;
                                lblConnect.ForeColor = Color.Black;

                                btnConnect.Text = "Disconnect";
                            }
                            else
                            {
                                lblConnect.Text = "Disconnected";
                                lblConnect.BackColor = Color.Red;
                                lblConnect.ForeColor = Color.White;
                            }
                        }
                    }
                }
                else
                {
                    if (_frmMain._camSet[_nIdx]._bConnect)
                    {
                        //_frmMain._camSet[_nIdx]._bConnect = false;
                        _frmMain._camSet[_nIdx].CamDisconnect();

                        for (int i=0; i<30; i++)
                        {
                            if (_frmMain._camParam[i].strCopy == _nIdx.ToString())
                            {
                                _frmMain._camSet[_nIdx]._bConnect = false;
                                _frmMain._CAM[i].LoadSet();
                            }
                        }

                        cbCogSerial.SelectedIndex = -1;
                        cbCogFormat.SelectedIndex = -1;
                        txtCogExpose.Text = "";
                        txtCogBright.Text = "";
                        txtCogContrast.Text = "";
                        txtCogTimeout.Text = "";
                        chkCognexTime.Checked = false;

                        cbBaslerSerial.SelectedIndex = -1;
                        lblBalserIP.Text = "";
                        txtBalserExpose.Text = "";
                        txtBaslerContrast.Text = "";
                        txtBalserTimeout.Text = "";

                        cbHIKSerial.SelectedIndex = -1;
                        lblHIKIP.Text = "";
                        txtHIKExpose.Text = "";
                        txtHIKContrast.Text = "";
                        txtHIKTimeout.Text = "";
                        //_bCamConnect = false;

                        btnConnect.Text = "Connect";
                        lblConnect.Text = "Disconnected";
                        lblConnect.BackColor = Color.Red;
                        lblConnect.ForeColor = Color.White;
                    }
                    else
                    {
                        _frmMain._camSet[_nIdx].CamDisconnect();
                        btnConnect.Text = "Connect";
                        lblConnect.Text = "Disconnected";
                        lblConnect.BackColor = Color.Red;
                        lblConnect.ForeColor = Color.White;
                    }

                }
            }
            catch (Exception ex)
            {
                _frmMain.AddMsg("Camera Connect Error : " + ex.Message, Color.Red, false, false, frmMain.MsgType.Alarm);
            }
        }

        private void GrabFunc(int nIdx, Bitmap bmpImg, ICogImage cogGrab)
        {
            try
            {
                if (bmpImg != null)
                {
                    Bitmap bmpGrab = (Bitmap)bmpImg.Clone();

                    if (bmpGrab.PixelFormat.ToString().ToLower().Contains("mono"))
                    {
                        CogImage8Grey cogImg = new CogImage8Grey(bmpGrab);
                        cogView.Image = cogImg;

                        cogImg.Dispose();
                        cogImg = null;
                    }
                    else
                    {
                        CogImage24PlanarColor cogImg = new CogImage24PlanarColor(bmpGrab);
                        cogView.Image = cogImg;

                        cogImg.Dispose();
                        cogImg = null;
                    }

                    bmpGrab.Dispose();
                    bmpGrab = null;
                }

                if (cogGrab != null)
                    cogView.Image = cogGrab;
            }
            catch( Exception ex) { _frmMain.AddMsg("Camera GrabFunc Error : " + ex.Message, Color.Red, false, false, frmMain.MsgType.Alarm); }
        }

        private void cbBaslerSerial_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbBaslerSerial.SelectedIndex == -1)
            {
                lblBalserIP.Text = "-";
            }
            else
            {
                try
                {
                    lblBalserIP.Text = _frmMain._cam.GetAddress(_frmMain._cameras[cbBaslerSerial.SelectedIndex]);
                }
                catch(Exception ex)
                {
                    _frmMain.AddMsg(ex.Message, Color.Red, true, true, frmMain.MsgType.Alarm);
                }
            }
        }

        private void sliderExpose_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                txtBalserExpose.Text = sliderExpose.Value.ToString();

                int.TryParse(txtBalserExpose.Text, out int nValue);

                if (nValue < sliderExpose.Minimum || nValue > sliderExpose.Maximum)
                {
                    MessageBox.Show(new Form { TopMost = true }, string.Format("The exposure value must be greater than {0} or less than {1}", sliderExpose.Minimum, sliderExpose.Maximum));
                }
                else
                {
                    sliderExpose.Value = nValue;

                    if (_frmMain._camSet[_nIdx]._bConnect)
                    {
                        _frmMain._cam.SetExposure(_frmMain._camSet[_nIdx]._cams, nValue);
                    }
                }
            }
            catch (Exception ex)
            {
                _frmMain.AddMsg("Basler Camera Set Expose Error : " + ex.Message, Color.Red, true, true, frmMain.MsgType.Alarm);
            }
        }

        private void sliderGain_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                txtBaslerContrast.Text = sliderGain.Value.ToString();

                int.TryParse(txtBaslerContrast.Text, out int nValue);

                if (nValue < sliderGain.Minimum || nValue > sliderGain.Maximum)
                {
                    MessageBox.Show(new Form { TopMost = true }, string.Format("The gain value must be greater than {0} or less than {1}", sliderGain.Minimum, sliderGain.Maximum));
                }
                else
                {
                    sliderGain.Value = nValue;
                    if (_frmMain._camSet[_nIdx]._bConnect)
                    {
                        _frmMain._cam.SetGain(_frmMain._camSet[_nIdx]._cams, nValue);
                    }
                }
            }
            catch (Exception ex)
            {
                _frmMain.AddMsg("Basler Camera Set Gain Error : " + ex.Message, Color.Red, true, true, frmMain.MsgType.Alarm);
            }
        }

        private void txtBalserExpose_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    double.TryParse(txtBalserExpose.Text, out double dValue);

                    if ((int)dValue < sliderExpose.Minimum || (int)dValue > sliderExpose.Maximum)
                    {
                        MessageBox.Show(new Form { TopMost = true }, string.Format("The exposure value must be greater than {0} or less than {1}", sliderExpose.Minimum, sliderExpose.Maximum));
                    }
                    else
                    {
                        sliderExpose.Value = (int)dValue;

                        if (_frmMain._camSet[_nIdx]._bConnect)
                        {
                            _frmMain._cam.SetExposure(_frmMain._camSet[_nIdx]._cams, dValue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _frmMain.AddMsg("Basler Camera Set Expose Error : " + ex.Message, Color.Red, true, true, frmMain.MsgType.Alarm);
                }
            }
        }

        private void txtBaslerContrast_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    double.TryParse(txtBaslerContrast.Text, out double dValue);

                    if ((int)dValue < sliderGain.Minimum || (int)dValue > sliderGain.Maximum)
                    {
                        MessageBox.Show(string.Format("The gain value must be greater than {0} or less than {1}", sliderGain.Minimum, sliderGain.Maximum));
                    }
                    else
                    {
                        sliderGain.Value = (int)dValue;
                        if (_frmMain._camSet[_nIdx]._bConnect)
                        {
                            _frmMain._cam.SetGain(_frmMain._camSet[_nIdx]._cams, (long)dValue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _frmMain.AddMsg("Basler Camera Set Contrast Error : " + ex.Message, Color.Red, true, true, frmMain.MsgType.Alarm);
                }
            }
        }

        private void btnGrab_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_frmMain._camSet[_nIdx]._bConnect)
                {
                    MessageBox.Show("Camera Disconnected");
                    return;
                }

                double dExpose = 0;
                if (radCognex.Checked)
                    double.TryParse(txtCogExpose.Text, out dExpose);
                else if (radBalser.Checked)
                    double.TryParse(txtBalserExpose.Text, out dExpose);
                else
                    double.TryParse(txtHIKExpose.Text, out dExpose);
                //_frmMain._camSet[_nIdx].White_Balance(_nIdx);
                _frmMain._camSet[_nIdx].Grab(_nIdx, dExpose);
            }
            catch (Exception ex)
            {
                MessageBox.Show(new Form { TopMost = true }, "Camera Grab Error : " + ex.Message);
            }
        }

        private void txtCogExpose_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    if (_frmMain._camSet[_nIdx]._bConnect)
                    {
                        double.TryParse(txtCogExpose.Text, out double dValue);
                        txtCogExpose.Text = string.Format("{0:F3}", dValue);
                        _frmMain._camSet[_nIdx]._AcqFifo.OwnedExposureParams.Exposure = dValue;
                        _frmMain._camSet[_nIdx]._AcqFifo.Prepare();
                    }
                }
                catch(Exception ex)
                {
                    _frmMain.AddMsg("Cognex FrameGrabber Camera Set Expose Error : " + ex.Message, Color.Red, true, true, frmMain.MsgType.Alarm);
                }
            }
        }

        private void txtCogBright_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    if (_frmMain._camSet[_nIdx]._bConnect)
                    {
                        double.TryParse(txtCogBright.Text, out double dValue);
                        txtCogBright.Text = string.Format("{0:F3}", dValue);
                        _frmMain._camSet[_nIdx]._AcqFifo.OwnedBrightnessParams.Brightness = dValue;
                        _frmMain._camSet[_nIdx]._AcqFifo.Prepare();
                    }
                }
                catch (Exception ex)
                {
                    _frmMain.AddMsg("Cognex FrameGrabber Camera Set Brightness Error : " + ex.Message, Color.Red, true, true, frmMain.MsgType.Alarm);
                }
            }
        }

        private void txtCogContrast_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    if (_frmMain._camSet[_nIdx]._bConnect)
                    {
                        double.TryParse(txtCogContrast.Text, out double dValue);
                        txtCogContrast.Text = string.Format("{0:F3}", dValue);
                        _frmMain._camSet[_nIdx]._AcqFifo.OwnedContrastParams.Contrast = dValue;
                        _frmMain._camSet[_nIdx]._AcqFifo.Prepare();
                    }
                }
                catch (Exception ex)
                {
                    _frmMain.AddMsg("Cognex FrameGrabber Camera Set Contrast Error : " + ex.Message, Color.Red, true, true, frmMain.MsgType.Alarm);
                }
            }
        }

        private void txtCogTimeout_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    if (_frmMain._camSet[_nIdx]._bConnect)
                    {
                        int.TryParse(txtCogTimeout.Text, out int nValue);
                        _frmMain._camSet[_nIdx]._AcqFifo.Timeout = nValue;
                        _frmMain._camSet[_nIdx]._AcqFifo.Prepare();
                    }
                }
                catch (Exception ex)
                {
                    _frmMain.AddMsg("Cognex FrameGrabber Camera Set Timeout Error : " + ex.Message, Color.Red, true, true, frmMain.MsgType.Alarm);
                }
            }
        }

        private void txtBalserTimeout_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    int.TryParse(txtBalserTimeout.Text, out int nValue);

                    if (nValue == 1000)
                    {
                        MessageBox.Show(string.Format("The Timeout value must be greater than {0}", 1000));
                    }
                    else
                    {
                        if (_frmMain._camSet[_nIdx]._bConnect)
                        {
                            _frmMain._cam.SetTimeout(_frmMain._camSet[_nIdx]._cams, nValue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _frmMain.AddMsg("Basler Camera Set Timeout Error : " + ex.Message, Color.Red, true, true, frmMain.MsgType.Alarm);
                }
            }
        }

        private void btnLive_Click(object sender, EventArgs e)
        {
            if (!_frmMain._camSet[_nIdx]._bConnect)
            {
                MessageBox.Show("Camera Disconnected");
                return;
            }

            _bLive = !_bLive;
            _frmMain._camSet[_nIdx].LiveView(_nIdx, cogView, _bLive);

            try
            {
                if (_bLive)
                {
                    CogGraphicLabel lblLive = new CogGraphicLabel();
                    lblLive.SetXYText(50, 70, "Live");
                    using (Font font = new Font("Tahoma", 20, FontStyle.Bold))
                        lblLive.Font = font;
                    lblLive.Color = CogColorConstants.Green;
                    lblLive.Alignment = CogGraphicLabelAlignmentConstants.BaselineLeft;

                    cogView.StaticGraphics.Add(lblLive, "Live");
                }
                else
                    cogView.StaticGraphics.Clear();
            }
            catch(Exception ex)
            {
                _frmMain.AddMsg("Live Error : " + ex.Message, Color.Red, true, true, frmMain.MsgType.Alarm);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (lblCamCnt.Text == "0")
            {
                MessageBox.Show("No Camera Connected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (radCognex.Checked)
            {
                if (cbCogSerial.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a camera Serial", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (cbCogFormat.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a camera VideoFormat", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _frmMain._camSet[_nIdx]._nCamType = 0;
                _frmMain._camParam[_nIdx].strCamType = "0";
                string[] _CamSerial = cbCogSerial.SelectedItem.ToString().Split(':');
                _frmMain._camParam[_nIdx].strCamSerial = _CamSerial[_CamSerial.Length - 1];
                //_frmMain._camParam[_nIdx].strCamSerial = cbCogSerial.SelectedItem.ToString().Substring(cbCogSerial.SelectedItem.ToString().Length - 11, 11);
                _frmMain._camParam[_nIdx].strCamFormat = cbCogFormat.SelectedIndex.ToString();
                _frmMain._camParam[_nIdx].strIP = "";
                //_frmMain._camParam[_nIdx].bConnect = _frmMain._camSet[_nIdx]._bConnect;
                double.TryParse(txtCogExpose.Text, out _frmMain._camParam[_nIdx].dExpose);
                double.TryParse(txtCogBright.Text, out _frmMain._camParam[_nIdx].dBright);
                double.TryParse(txtCogContrast.Text, out _frmMain._camParam[_nIdx].dContract);
                int.TryParse(txtCogTimeout.Text, out _frmMain._camParam[_nIdx].nTimeout);
                _frmMain._camParam[_nIdx].bTiime = chkCognexTime.Checked;
                _frmMain._camParam[_nIdx].strCopy = cbCamNo.SelectedIndex.ToString();

                _frmMain._camSet[_nIdx].SetCamParam(0, _frmMain._camParam[_nIdx].dExpose);
                //_frmMain._camSet[_nIdx]._AcqFifo.OwnedExposureParams.Exposure = _frmMain._camParam[_nIdx].dExpose;
                //_frmMain._camSet[_nIdx]._AcqFifo.Prepare();
            }
            else if (radBalser.Checked)
            {
                if (cbBaslerSerial.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a camera Serial", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _frmMain._camSet[_nIdx]._nCamType = 1;
                _frmMain._camParam[_nIdx].strCamType = "1";
                _frmMain._camParam[_nIdx].strCamSerial = cbBaslerSerial.SelectedIndex.ToString();
                _frmMain._camParam[_nIdx].strCamFormat = "0";
                _frmMain._camParam[_nIdx].strIP = lblBalserIP.Text;
                //_frmMain._camParam[_nIdx].bConnect = _frmMain._camSet[_nIdx]._bConnect;
                double.TryParse(txtBalserExpose.Text, out _frmMain._camParam[_nIdx].dExpose);
                _frmMain._camParam[_nIdx].dBright = 0;
                double.TryParse(txtBaslerContrast.Text, out _frmMain._camParam[_nIdx].dContract);
                int.TryParse(txtBalserTimeout.Text, out _frmMain._camParam[_nIdx].nTimeout);
                _frmMain._camParam[_nIdx].bTiime = true;
                _frmMain._camParam[_nIdx].strCopy = cbCamNo.SelectedIndex.ToString();
            }
            else
            {
                if (cbHIKSerial.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a camera Serial", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _frmMain._camSet[_nIdx]._nCamType = 2;
                _frmMain._camParam[_nIdx].strCamType = "2";
                _frmMain._camParam[_nIdx].strCamSerial = cbHIKSerial.SelectedIndex.ToString();
                _frmMain._camParam[_nIdx].strCamFormat = "0";
                _frmMain._camParam[_nIdx].strIP = lblHIKIP.Text;
                //_frmMain._camParam[_nIdx].bConnect = _frmMain._camSet[_nIdx]._bConnect;
                double.TryParse(txtHIKExpose.Text, out _frmMain._camParam[_nIdx].dExpose);
                //_frmMain._camParam[_nIdx].dBright = 0;
                //double.TryParse(txtBaslerContrast.Text, out _frmMain._camParam[_nIdx].dContract);
                //int.TryParse(txtBalserTimeout.Text, out _frmMain._camParam[_nIdx].nTimeout);
                //_frmMain._camParam[_nIdx].bTiime = true;
                _frmMain._camParam[_nIdx].strCopy = cbCamNo.SelectedIndex.ToString();
            }

            try
            {
                ini.WriteIniFile("CamType", "Value", _frmMain._camParam[_nIdx].strCamType, Application.StartupPath + "\\CameraInfo", string.Format("CameraInfo{0:D2}.ini", _nIdx + 1));
                ini.WriteIniFile("CamSerial", "Value", _frmMain._camParam[_nIdx].strCamSerial, Application.StartupPath + "\\CameraInfo", string.Format("CameraInfo{0:D2}.ini", _nIdx + 1));
                ini.WriteIniFile("CamFormat", "Value", _frmMain._camParam[_nIdx].strCamFormat, Application.StartupPath + "\\CameraInfo", string.Format("CameraInfo{0:D2}.ini", _nIdx + 1));
                ini.WriteIniFile("CamIP", "Value", _frmMain._camParam[_nIdx].strIP.ToString(), Application.StartupPath + "\\CameraInfo", string.Format("CameraInfo{0:D2}.ini", _nIdx + 1));
                ini.WriteIniFile("CamExpose", "Value", _frmMain._camParam[_nIdx].dExpose.ToString(), Application.StartupPath + "\\CameraInfo", string.Format("CameraInfo{0:D2}.ini", _nIdx + 1));
                ini.WriteIniFile("CamBright", "Value", _frmMain._camParam[_nIdx].dBright.ToString(), Application.StartupPath + "\\CameraInfo", string.Format("CameraInfo{0:D2}.ini", _nIdx + 1));
                ini.WriteIniFile("CamContrast", "Value", _frmMain._camParam[_nIdx].dContract.ToString(), Application.StartupPath + "\\CameraInfo", string.Format("CameraInfo{0:D2}.ini", _nIdx + 1));
                ini.WriteIniFile("CamTimeout", "Value", _frmMain._camParam[_nIdx].nTimeout.ToString(), Application.StartupPath + "\\CameraInfo", string.Format("CameraInfo{0:D2}.ini", _nIdx + 1));
                ini.WriteIniFile("CamTimeoutUse", "Value", _frmMain._camParam[_nIdx].bTiime.ToString(), Application.StartupPath + "\\CameraInfo", string.Format("CameraInfo{0:D2}.ini", _nIdx + 1));
                ini.WriteIniFile("CamCopy", "Value", _frmMain._camParam[_nIdx].strCopy, Application.StartupPath + "\\CameraInfo", string.Format("CameraInfo{0:D2}.ini", _nIdx + 1));
                //ini.WriteIniFile("ConnectStatus", "Value", _frmMain._camParam[_nIdx].bConnect.ToString(), Application.StartupPath + "\\CameraInfo", string.Format("CameraInfo{0}.ini", _nIdx + 1));

                _frmMain._CAM[_nIdx]._var = _frmMain._var;
                _frmMain._CAM[_nIdx]._camSet = _frmMain._camSet[_nIdx];
                //_frmMain._CAM[_nIdx]._camSet._GrabFunc = _frmMain.GrabFunc;
                _frmMain._CAM[_nIdx].LoadSet();

                MessageBox.Show("saved the camera information.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }   
            catch(Exception ex)
            {
                _frmMain.AddMsg("Error saving camera settings" + ex.Message, Color.Red, false, false, frmMain.MsgType.Alarm);
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (cbCamNo.SelectedIndex == -1)
            {
                MessageBox.Show("Please select the camera you want to copy.", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    return;
            }

            _frmMain._camParam[_nIdx] = _frmMain._camParam[cbCamNo.SelectedIndex];
            _frmMain._camParam[_nIdx].strCopy = cbCamNo.SelectedIndex.ToString();            

            _frmMain._camSet[_nIdx] = _frmMain._camSet[cbCamNo.SelectedIndex];
           
            //_frmMain._CAM[_nIdx]._camSet._GrabFunc = _frmMain.GrabFunc;
            setParam();
            _frmMain._camSet[_nIdx].CamConnect(_nIdx, _frmMain._camParam, _frmMain._mFrameGrabbers, _frmMain._cameras, _frmMain.m_stDeviceList, _frmMain._ModelParam[_nIdx]);

            if (_frmMain._camSet[_nIdx]._bConnect)
            {
                txtCogExpose.Enabled = true;
                txtCogBright.Enabled = true;
                txtCogContrast.Enabled = true;
                txtCogTimeout.Enabled = true;
                chkCognexTime.Enabled = true;

                double[] dParam = _frmMain._camSet[_nIdx].GetCamParam();

                    if (dParam.Length > 2)
                {
                    txtCogExpose.Text = string.Format("{0:F3}", dParam[0]);
                    txtCogBright.Text = string.Format("{0:F3}", dParam[1]);
                    txtCogContrast.Text = string.Format("{0:F3}", dParam[2]);
                }
                btnGrab.Enabled = true;
                btnLive.Enabled = true;

                _frmMain._camSet[_nIdx]._GrabFunc = GrabFunc;
                lblConnect.Text = "Connected";
                lblConnect.BackColor = Color.Lime;
                lblConnect.ForeColor = Color.Black;

                btnConnect.Visible = false;
            }
            else
            {
                lblConnect.Text = "Disconnected";
                lblConnect.BackColor = Color.Red;
                lblConnect.ForeColor = Color.White;

                btnConnect.Visible = false;
            }
        }

        private void txtCogExpose_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back) || e.KeyChar == '.'))    //숫자와 백스페이스를 제외한 나머지를 바로 처리
                e.Handled = true;
        }

        private void txtCogTimeout_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))    //숫자와 백스페이스를 제외한 나머지를 바로 처리
                e.Handled = true;
        }

        private void cbCogSerial_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbCogSerial.SelectedIndex == -1)
                return;

            try
            {
                btnConnect.Visible = true;

                if (radCognex.Checked)
                {
                    cbCogFormat.Properties.Items.Clear();
                    int nSel = cbCogSerial.SelectedIndex;
                    CogStringCollection cogCollection = _frmMain._mFrameGrabbers[nSel].AvailableVideoFormats;

                    for (int i = 0; i < cogCollection.Count; i++)
                        cbCogFormat.Properties.Items.Add(cogCollection[i].ToString());
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cbHIKSerial_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbHIKSerial.SelectedIndex == -1)
                {
                    lblHIKIP.Text = "-";
                }
                else
                {
                    try
                    {
                        MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(_frmMain.m_stDeviceList.pDeviceInfo[cbHIKSerial.SelectedIndex], typeof(MyCamera.MV_CC_DEVICE_INFO));
                        MyCamera.MV_CC_DEVICE_INFO.SPECIAL_INFO info = device.SpecialInfo;
                        if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                            lblHIKIP.Text = string.Format("{0}.{1}.{2}.{3}", info.stGigEInfo[11].ToString(), info.stGigEInfo[10].ToString(), info.stGigEInfo[9].ToString(), info.stGigEInfo[8].ToString());
                    }
                    catch (Exception ex)
                    {
                        _frmMain.AddMsg(ex.Message, Color.Red, true, true, frmMain.MsgType.Alarm);
                    }
                }
            }
            catch { }
        }

        private void txtHIKExpose_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int.TryParse(txtHIKExpose.Text, out int nValue);

                if (nValue <sliderHIKExpose.Minimum)
                {
                    MessageBox.Show(string.Format("Range {0} ~ {1}", sliderHIKExpose.Minimum, sliderHIKExpose.Maximum), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                sliderHIKExpose.Value = nValue;
                _frmMain._camSet[_nIdx].SetCamParam(0, (double)nValue);
            }
        }

        private void sliderHIKExpose_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                txtHIKExpose.Text = sliderHIKExpose.Value.ToString();
                _frmMain._camSet[_nIdx].SetCamParam(0, (double)sliderHIKExpose.Value);
            }
            catch { }
        }
    }
}