using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Diagnostics;
using DevComponents.DotNetBar;
using System.Text.RegularExpressions;

using System.IO.Ports;

using DevExpress.XtraSplashScreen;
using Basler.Pylon;
using Cognex.VisionPro.FGGigE;
using System.Data.OleDb;
using Cognex.VisionPro;
using S7.Net;
using MvCamCtrl.NET;

using DevExpress.Utils.Layout;
using System.Globalization;
using Cognex.VisionPro.ToolGroup;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.Display;
using System.Drawing.Imaging;

namespace VisionSystem
{

    public partial class frmMain : DevExpress.XtraEditors.XtraForm
    {
        WaitForm1 waitForm = new WaitForm1();
        Color green = Color.GreenYellow;
        Color yellow = Color.Yellow;
        Color red = Color.Red;
        Color white = Color.White;

        FluentSplashScreenOptions op;
        //public frmSplash _frmSplash = null;  // 플래시
        frmCamera _frmCamera;  // 카메라 설정
        frmModel _frmModel;
        frmJobList _frmJobList;
        frmPrintList _frmPrintList;
        frmComm _frmComm;
        //frmToolEdit _frmToolEdit;
        //frmWait _frmWait;
        //frmImageSet _frmImageSet;
        IniFiles ini = new IniFiles();
        frmMDI[] _frmMDI = null;

        public GlovalVar _var = new GlovalVar(); // 전역변수

        //private byte[] _mxRead = null;
        public CAM[] _CAM = new CAM[30];
        public PinCheck _pin = new PinCheck();
        public PrintControl _PrintControl = new PrintControl();

        public CamSet[] _camSet = null;
        bool _bTimeThread; // 현재 시간 
        public int _nCamCnt = 30;
        public bool _bCommConnect = false;

        public MXplc _mxPlc = null;
        public SiemensPLC _siemensPLC = null;
        public ioBoard _ioBoard = null;
        //public LSplcComm _LSplc = null;
        public XGCommSocket _LSplc = null;

        bool _bChkDrive = false;
        public OleDbConnection _OleDB = null; //데이터 저장
        public OleDbConnection _PrintOleDB = null; //데이터 저장
        bool _bLicense = false;

        public int _nCamInfo = 0;
        public int _nFindCam = 0;

        public CogFrameGrabbers _mFrameGrabbers = null;

        public PylonCam _cam = null;
        public List<Camera> _cameras = null;

        public MyCamera.MV_CC_DEVICE_INFO_LIST m_stDeviceList;


        int _nPassChkType = 0;

        bool _bPassmode = false;

        public string _strRecvData = "";
        public byte[] _Readbytes = null;
        public ushort[] _ReadShort = null;

        public GlovalVar.CamPram[] _camParam = null;
        public GlovalVar.ModelParam[] _ModelParam = null;
        public GlovalVar.PLCPram _plcParam = new GlovalVar.PLCPram();

        bool _bOriginalDelete = false;
        bool _bResultDelete = false;

        bool[] _bInspRes = null;
        bool[] _bResult = null;
        string[] _strCamData = null;

        private GlovalVar.AdminMode _adminMode = GlovalVar.AdminMode.Camera;

        bool _bImgList = false;

        CogDisplay[] _cogDisp = new CogDisplay[20];
        Panel[] _Pnl = new Panel[20];
        int _nDispNo = -1;
        int _nImgListCamNo = -1;

        SerialComm _serial = new SerialComm(0);
        bool _bLight = false;

        TCPComponents.TCPSimpleClient _print = new TCPComponents.TCPSimpleClient();

        SocketUDPClient _RobotSned = null;
        SocketServer _RobotRecv = null;

        bool _bRobot = false;
        string tempModelData = "";

        GlovalVar.RobotParam _robotParam = new GlovalVar.RobotParam();
        GlovalVar.LightParam _LightParam = new GlovalVar.LightParam();

        bool _bReTryConn = false;
        string _strResult = "";
        string _strBlob = "";
        string _strTrigger = "";
        public int _indexCountSum = 0;

        keyenceLib _Sensor = new keyenceLib();
        GlovalVar.SensorParam _sensorParam = new GlovalVar.SensorParam();
        GlovalVar.PrintParam _PrintParam = new GlovalVar.PrintParam();

        public bool bLabelStatus = false;
        public bool bRibeenStatus = false;
        DateTime _time;
        string[] _pinShowData;

        public enum MsgType
        {
            Log,
            Alarm
        }

        public frmMain()
        {
            InitializeComponent();

            InitCam();
        }

        protected override bool GetAllowSkin()
        {
            if (this.DesignMode) return false;
            return true;
        }


        private void frmMain_Load(object sender, EventArgs e)
        {
            Loading();

        }

        private void LicenseChk()
        {
            try
            {
                //Visionpro 9.3 이상
                //CogLicense.CheckForExpiredDongle(false);
                //_bLicense = true;
                //lblLic.ItemAppearance.Normal.ForeColor = Color.Lime;

                //Visionpro 9.3 이하
                //CogStringCollection licensedFeatures = CogMisc.GetLicensedFeatures(false);

                //if (licensedFeatures.Count == 0)
                //{
                //    _bLicense = false;
                //    lblLic.ItemAppearance.Normal.ForeColor = Color.Red;
                //    AddMsg(Str.Licensenotactive, red, false, false, MsgType.Alarm);
                //}
                //else
                //{
                //    _bLicense = true;
                //    AddMsg(Str.LicenseActive, green, false, false, MsgType.Alarm);
                //    lblLic.ItemAppearance.Normal.ForeColor = Color.Lime;
                //}
            }
            catch (Exception ex)
            {
                //ex.ToString();
                _bLicense = false;
                lblLic.ItemAppearance.Normal.ForeColor = Color.Red;
                AddMsg(Str.Licensenotactive, red, false, false, MsgType.Log);
            }
        }

        private void Loading()
        {
            string[] strVersion = Application.ProductVersion.Split('.');
            this.Text = string.Format("Vision Inspection [Ver {0}.{1}.{2}]", strVersion[0], strVersion[1], strVersion[2]);

            _var._nWidth = (Screen.PrimaryScreen.Bounds.Width <= 1920) ? Screen.PrimaryScreen.Bounds.Width - 5 : 1936 - 5;
            _var._nHeight = (Screen.PrimaryScreen.Bounds.Height <= 1080) ? Screen.PrimaryScreen.Bounds.Height - 121 : 1096 - 121;


            op = null;
            op = new FluentSplashScreenOptions();

            op.Title = "";
            op.Subtitle = "";
            using (Font font = new Font("Tahoma", 10, FontStyle.Bold))
            {
                op.AppearanceLeftFooter.Font = font;
                op.AppearanceRightFooter.Font = font;
            }

            using (Font font = new Font("Tahoma", 30, FontStyle.Regular))
            {
                op.AppearanceTitle.Font = font;
            }

            op.AppearanceLeftFooter.ForeColor = Color.FromArgb(255, 255, 192);
            op.AppearanceRightFooter.ForeColor = Color.FromArgb(255, 255, 192);
            op.LeftFooter = this.Text;
            op.RightFooter = "Copyright © 2022 Vasim Inc.\nAll Rights reserved.";

            op.LoadingIndicatorType = FluentLoadingIndicatorType.Dots;
            op.Opacity = 130;
            op.OpacityColor = Color.Black;
            SplashScreenManager.ShowFluentSplashScreen(op, parentForm: this, useFadeIn: true, useFadeOut: true);
            Delay(100);

            op.Title = Str.Licensecheck;
            SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, op);

            ChangeLanguage();

            InitControl();

            op.Title = Str.Settime;
            SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, op);
            _bTimeThread = true;
            Thread threadTime = new Thread(CurrentTime);
            threadTime.Start();
            Delay(50);

            op.Title = Str.chkDrive;
            SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, op);
            _bChkDrive = true;
            Thread threadChkDrive = new Thread(DriveSize);
            threadChkDrive.Start();
            Delay(50);

            op.Title = Str.loadParam;
            SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, op);
            LoadSet();                                                                                                      //카메라 로드 
            Delay(50);

            op.Title = Str.PrepareProgram;
            SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, op);
            Thread threadCreateForm = new Thread(CreateForm);                                                               //카메라 모델 리스트 로드 
            threadCreateForm.Start();
            Delay(50);

            if (_var._strAddfunction == "Print")
            {
                //바코드 프린트 활성 시 주석 해제
                _var._bUsePrint = true;
                op.Title = "프린트 연결 중...";
                SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, op);
                lblPrint.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                //btnAlltrigger.Visibility = DevExpress.XtraBars.BarItemVisibility.Never; 
                //listModel.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
                lblLic.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
                btnSetCam.Visible = false;
                btnAlign.Visible = false;
                btnRobot.Visible = true;
                btnRobot.Text = "Print Setting";
                _var._strHeight = "1";
                _var._strWidth = "1";
                _var._nScreenCnt = 1;
                Thread threadPrintConnection = new Thread(Print_Connection);
                threadPrintConnection.Start();
                Delay(50);
            }
            if (_var._strAddfunction == "Sensor")
            {
                //키엔스 활성화 시 주석 해제
                _var._bUseSensor = true;
                btnRobot.Visible = true;
                op.Title = Str.SensorComm;
                lblSensor.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                _Sensor.OnSensorInfo = OnSensorInfo;
                _Sensor.OnProgramSet = OnProgramSet;
                _Sensor.OnProgramList = OnProgramList;
                //_Sensor.OnThresholdValue = OnThresholdValue;
                _Sensor.OnData = Ondata;
                SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, op);
                Thread threadSensorConnect = new Thread(SensorConnect);
                threadSensorConnect.Start();
                Delay(50);
            }
            if (_var._strAddfunction == "PinChange")
            {
                _var._bUsePinchange = true;
            }

            op.Title = Str.loadrecipe;
            SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, op);
            LoadRecipe();                                                                                                   //카메라별 데이터 로드
            Delay(50);

            op.Title = Str.Historyload;
            SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, op);
            Thread threadLoadData = new Thread(LoadData);                                                                   //History 활성화
            threadLoadData.Start();
            Delay(50);

            op.Title = Str.Camconn;
            SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, op);
            CamConnecting();
            Delay(50);

            op.Title = Str.Commconn;
            SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, op);
            Thread threadCommConnect = new Thread(CommConnect);                                                              //카메라 연결
            threadCommConnect.Start();
            Delay(50);



            //op.Title = "로봇 연결 중...";
            //SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, op);
            //RobotConnecting();
            //Delay(50);

            //op.Title = "조명 연결 중...";
            //SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, op);
            //LightConnecting();
            //Delay(50);



            op.Title = Str.programstart;
            SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, op);
            Delay(100);

            SplashScreenManager.CloseForm(false);

            ActiveControl = lblresultImgsave;
            this.WindowState = FormWindowState.Maximized;
        }

        private void OnSensorInfo(string strDeviceName, string strHeadModelName, string strHeadVersion, string strHeadSerial, string strAmpModelName, string strAmpVersion, string strAmpSerial)
        {
            try
            {
                Invoke(new EventHandler(delegate
                {
                    lblKeyenceDev.Text = strDeviceName;
                    lblKeyenceHeadModel.Text = strHeadModelName;
                    lblKeyenceHeadVersion.Text = strHeadVersion;
                    lblKeyenceHeadSerial.Text = strHeadSerial;
                    lblKeyenceAmpModel.Text = strAmpModelName;
                    lblkeyenceAmpVersion.Text = strAmpVersion;
                    lblKeyenceAmpSerial.Text = strAmpSerial;
                }));
            }
            catch { }
        }

        private void OnProgramSet(string strProgramNo, string strProgramName, string strMode, string strExternalTrigger, string strExternalTriggerTime, Bitmap MasterImg)
        {
            try
            {
                Invoke(new EventHandler(delegate
                {
                    lblKeyenceProgNo.Text = strProgramNo;
                    lblKeyenceProgName.Text = strProgramName;
                    lblkeyenceDetectMode.Text = strMode;
                    lblKeyenceExtTrigger.Text = strExternalTrigger;
                    lblKeyenceTriggerCycle.Text = strExternalTriggerTime;
                    cogSensorMasterImg.AutoFit = true;
                    cogSensorMasterImg.Image = new CogImage24PlanarColor((Bitmap)MasterImg.Clone());
                }));

            }
            catch { }
        }

        private void OnProgramList(List<string> listProgram)
        {
            try
            {
                Invoke(new EventHandler(delegate
                {
                    listProd.Items.Clear();

                    foreach (string program in listProgram)
                        listProd.Items.Add(program);
                }));
            }
            catch { }
        }

        private void OnThresholdValue(double dHigh, double dLow)
        {
            try
            {
                Invoke(new EventHandler(delegate
                {
                    txtKeyenceLow.Text = dHigh.ToString();
                    txtKeyenceLow.Text = dLow.ToString();
                }));
            }
            catch { }
        }

        private void Ondata(string strRes, string strValue, ICogImage cogImg)
        {
            try
            {
                Invoke(new EventHandler(delegate
                {
                    lblkeyenceRes.BackColor = Color.Lime;
                    lblkeyenceRes.ForeColor = Color.Black;

                    var strResult = "OK";
                    var strData = "";

                    var strTempRes = strRes.Split(',');
                    var strTempValue = strValue.Split(',');
                    for (var i = 0; i < strTempRes.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(strTempRes[i]))
                        {
                            if (i != 0)
                                strData += " ,";
                            if (strTempRes[i] != "OK")
                            {
                                lblkeyenceRes.BackColor = Color.Red;
                                lblkeyenceRes.ForeColor = Color.White;

                                strResult = "NG";
                            }

                            strData += strTempValue[i];
                        }
                    }


                    lblkeyenceRes.Text = strResult;
                    lblKeyenceValue.Text = strData;
                    cogSensorInspImg.AutoFit = true;
                    cogSensorInspImg.Image = cogImg;

                    _CAM[1].SensorData(strTempRes, strTempValue, cogImg);
                }));

            }
            catch (Exception ex)
            {
                AddMsg("Sensor Recieve Data Error : " + ex.Message, red, false, false, MsgType.Alarm);
            }
        }

        private void LightConnecting()
        {
            try
            {
                if (_serial.IsOpen)
                    _serial.CloseComm();

                if (string.IsNullOrEmpty(_LightParam.strPortName) || string.IsNullOrEmpty(_LightParam.strBaudrate))
                {
                    AddMsg("조명 연결 설정 정보를 입력하여 주십시오.", red, false, false, MsgType.Alarm);
                    return;
                }

                if (_serial.OpenComm(_LightParam.strPortName, int.Parse(_LightParam.strBaudrate), 8, "1", "None", "None"))
                {
                    _bLight = true;
                    SetLightValue();
                    lblLight.ItemAppearance.Normal.ForeColor = Color.Lime;
                    AddMsg("조명이 연결 되었습니다.", green, false, false, MsgType.Alarm);
                }
                else
                {
                    AddMsg("조명이 되지 않았습니다.", red, false, false, MsgType.Alarm);
                }
            }
            catch (Exception ex)
            {
                AddMsg("조명 연결 오류 : " + ex.ToString(), red, false, false, MsgType.Alarm);
            }
        }

        private void RobotConnecting()
        {
            try
            {
                if (string.IsNullOrEmpty(_robotParam.strIP) && string.IsNullOrEmpty(_robotParam.strPort))
                {
                    AddMsg("로봇 연결 설정 정보를 입력하여 주십시오.", red, false, false, MsgType.Alarm);
                    return;
                }

                if (_RobotSned == null)
                {
                    _RobotSned = new SocketUDPClient(0);
                    _RobotSned.MessageHandler = OnSendMsg;
                }


                if (_RobotRecv == null)
                {
                    _RobotRecv = new SocketServer();
                    _RobotRecv.StartServer("10.22.65.120", 8002);
                    _RobotRecv.DataReceivedHandler = OnRobotDataAvailable;
                }

                var nRes = _RobotSned.ConnectToServer(_robotParam.strIP, int.Parse(_robotParam.strPort));
                if (nRes == 0)
                {
                    _bRobot = true;
                    lblRobot.ItemAppearance.Normal.ForeColor = Color.Lime;
                    AddMsg("Robot Connected", green, false, false, MsgType.Alarm);
                }
                else
                    AddMsg("Robot Disconnected", red, false, false, MsgType.Alarm);
            }
            catch (Exception ex)
            {
                AddMsg("로봇 연결 오류 : " + ex.ToString(), red, false, false, MsgType.Alarm);
            }
        }

        private void OnSendMsg(string strMsg)
        {
            AddMsg(strMsg, white, false, false, MsgType.Log);
        }

        private void OnRobotMessage(string strMsg)
        {
            AddMsg(strMsg, white, false, false, MsgType.Log);
        }

        private void OnRobotConnect(string strMsg)
        {
            Invoke(new EventHandler(delegate
            {
                lblRobot.ItemAppearance.Normal.ForeColor = Color.Lime;
            }));

            AddMsg(strMsg, green, false, false, MsgType.Alarm);
        }


        private void InitControl()
        {
            for (int i = 0; i < 20; i++)
            {
                _Pnl[i] = new Panel();
                _cogDisp[i] = new CogDisplay();

                _Pnl[i] = Controls.Find(string.Format("Pnl{0}", i + 1), true).FirstOrDefault() as Panel;
                _cogDisp[i] = Controls.Find(string.Format("cogDisp{0}", i + 1), true).FirstOrDefault() as CogDisplay;

                _Pnl[i].Tag = i;
                _cogDisp[i].Tag = i;

                _cogDisp[i].AutoFit = true;
                _cogDisp[i].Image = null;
            }

            AddPort();

            _serial.DataReceivedHandler = OnLightDataRecive;
            _serial.MessageHandler = OnLightMessage;
        }

        private void OnLightMessage(string strMsg)
        {
            AddMsg("Light Error Message : " + strMsg, red, false, false, MsgType.Alarm);
        }

        private void OnLightDataRecive(byte[] receiveData)
        {
            var strData = Encoding.UTF8.GetString(receiveData);
            //AddMsg("Light Receive Data : " + strData, white, false, false, MsgType.Log);
        }
        private void OnRobotDataAvailable(string strData)
        {
            AddMsg("Robot ReciveData : " + strData, white, false, false, MsgType.Log);

            _strTrigger = strData;
            if (string.IsNullOrEmpty(_strTrigger))
                return;

            if (_strTrigger.Trim() == _robotParam.strTrigger1)
            {
                LightOnOff(true);

                if (_CAM[0] != null)
                {
                    Thread threagTrigger = new Thread(() => _CAM[0].Grab(false));
                    threagTrigger.Start();
                }

                Delay(200);
                LightOnOff(false);
            }
            else if (_strTrigger.Trim() == _robotParam.strTrigger2)
            {
                RobotDataSend(_strResult);
            }
            //else if (strData.Trim() == "S3")
            //{
            //    //LightOnOff(true);
            //    if (_CAM[1] != null)
            //    {
            //        Thread threagTrigger = new Thread(() => _CAM[1].Grab(false));
            //        threagTrigger.Start();
            //    }

            //    //Delay(200);
            //    //LightOnOff(false);
            //}

            //Delay(100);
        }

        private void OnRobotError(object sender, string strErrMsg)
        {
            AddMsg("Robot Error Message : " + strErrMsg, red, false, false, MsgType.Alarm);
        }

        private void AddPort()
        {
            try
            {
                string[] PortNames = SerialPort.GetPortNames();

                if (PortNames.Length == 0)
                    return;

                cbLightPort.Properties.Items.AddRange(PortNames);
            }
            catch { }
        }

        private void ShowLog()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            bool bStart = true;

            while (bStart)
            {
                if (sw.ElapsedMilliseconds >= 500)
                {
                    Invoke(new EventHandler(delegate
                    {
                        try
                        {
                            if (!flyLog.IsPopupOpen)
                            {
                                flyLog.ShowPopup();
                                sw.Reset();
                                sw.Start();
                            }
                        }
                        catch { }

                    }));
                }

                if (sw.ElapsedMilliseconds > 2000)
                {
                    Invoke(new EventHandler(delegate
                    {
                        try
                        {
                            if (flyLog.IsPopupOpen)
                            {
                                flyLog.HidePopup();
                                bStart = false;
                            }
                        }
                        catch { }

                    }));
                }
                Thread.Sleep(100);
            }
        }

        private void OnResultImageList(int nIdx)
        {
            if (_nImgListCamNo != nIdx)
            {
                _bImgList = true;
                _nImgListCamNo = nIdx;

                Delay(50);

                Thread threadImgList = new Thread(() => ShowImgList(_nImgListCamNo));
                threadImgList.Start();

                Delay(100);

                if (!flyImgList.IsPopupOpen)
                    flyImgList.ShowPopup();
            }
        }

        private void ShowImgList(int nIdx)
        {
            var nCamNp = nIdx;
            var listResultImg = new List<ICogImage>();

            while (true)
            {
                if (!_bImgList)
                    return;

                if (_CAM[nCamNp].isUpdate)
                {
                    _CAM[nCamNp].isUpdate = false;

                    listResultImg = _CAM[nCamNp].GetResultImg;

                    if (listResultImg != null)
                        ShowResultImgList(listResultImg);
                }

                Thread.Sleep(100);
            }
        }

        private void ShowResultImgList(List<ICogImage> listResultImg)
        {
            Invoke(new EventHandler(delegate
            {
                for (int i = 0; i < listResultImg.Count; i++)
                {
                    _cogDisp[i].AutoFit = true;
                    _cogDisp[i].Image = listResultImg[i];
                }
            }));
        }
        private void ChkInspection(string[] strCamData, DateTime InspdateTime, string strSerial, string strCode)
        {
            int nCnt = 0;
            bool _btotalResult = false;
            
            foreach (bool bInsp in _bInspRes)
            {
                if (!bInsp)
                    return;
            }

            for (int i = 0; i < _var._nScreenCnt; i++)
            {
                if (_bResult[i])
                    nCnt++;
            }
            if (_var._bUsePinchange)
            {
                string _strPinData = "";
                for(int i=0;i<_var._nScreenCnt; i++)
                {
                    _strPinData = _pinShowData[i]+ _strPinData;
                }
                _pin.ledonoff(_strPinData);
            }
            
            if (_var._strModelName != "01ST")
                _var._nTotalCnt++;
            if (nCnt == _var._nScreenCnt)
            {
                if (_var._strModelName != "01ST")
                    _var._nOKCnt++;
                Delay(50);
                SendTotalResult(true);
                _btotalResult = true;
            }
            else
            {
                if (_var._strModelName != "01ST")
                    _var._nNGCnt++;
                Delay(50);
                SendTotalResult(false);
                _btotalResult = false;
            }

            Createfield(_btotalResult, _bResult, strCamData, InspdateTime, strSerial, strCode);

            for (int i = 0; i < _var._nScreenCnt; i++)
            {
                if (_var._bOriginImageSave)
                    _CAM[i].SaveOriginImage(_btotalResult);
                if (_var._bResultImageSave)
                    _CAM[i].SaveResultImage(_btotalResult);
                if (_var._strModelName != "01ST")
                {
                    if (!_bResult[i])
                        _CAM[i].ShowNGPopup(true);
                    else
                        _CAM[i].ShowNGPopup(false);
                }
            }
            for (int i = 0; i < _var._nScreenCnt; i++)
            {
                _bInspRes[i] = false;
                strCamData[i] = "";
            }


            SetCount();
        }
        private void CamInspComplete(int nSel, bool[] bResult, string[] strValue, DateTime InspdateTime, string strSerial, string strCode)
        {
            try
            {
                int nIdx = nSel;
                string[] strData = strValue;
                DateTime dateTime = _time;
                bool bRes = false;
                if (_CAM[nIdx]._bCamUse)
                {
                    _bResult[nIdx] = true;
                    _bInspRes[nIdx] = true;
                    bRes = true;
                }
                else
                {
                    bRes = (bResult[0] && bResult[1] && bResult[2] && bResult[3]) ? true : false;
                    VisionComplete(nIdx, bRes, strData, dateTime, strSerial, strCode);


                    _bResult[nIdx] = bRes;
                    _bInspRes[nIdx] = true;

                }

                _pinShowData[nIdx] = strData[6];
                for (int i = 0; i < strData.Length; i++)
                {
                    if (i != strData.Length - 1)
                    {
                        if (i == 0)
                        {
                            if (bRes)
                                _strCamData[nIdx] = "1,";
                            else
                                _strCamData[nIdx] = "2,";
                        }
                        else
                            _strCamData[nIdx] += strData[i] + ",";
                    }
                    else
                        _strCamData[nIdx] += strData[i];
                }
                ChkInspection(_strCamData, dateTime, strSerial, strCode);
            }
            catch (Exception ex)
            {
                AddMsg("CamInspComplete Err : " + ex.ToString(), red, false, false, MsgType.Alarm);
            }
        }

        private void SendTotalResult(bool bResult)                      // 전체 결과
        {
            if (_plcParam.nCommType == (int)GlovalVar.PLCType.MX)
            {
                if (_mxPlc != null)
                {
                    Thread threadInspEnd = new Thread(() => _mxPlc.TotalResult(bResult));
                    threadInspEnd.Start();
                }

                //InsertData(nIdx, bRes, strData, dateTime, strSerial, strCode);
            }
            else if (_plcParam.nCommType == (int)GlovalVar.PLCType.Simens)
            {

            }
            else if (_plcParam.nCommType == (int)GlovalVar.PLCType.IO)
            {

            }
            else
            {
                if (_LSplc != null)
                {
                    //Thread threadInspEnd = new Thread(() => _LSplc.TotalResult(bResult));
                    //threadInspEnd.Start();
                    _LSplc.TotalResult(bResult);
                   
                }
            }
        }

        private void VisionComplete(int nSel, bool bResult, string[] strValue, DateTime InspdateTime, string strSerial, string strCode)
        {
            int nIdx = nSel;
            bool bRes = bResult;
            string[] strData = strValue;
            DateTime dateTime = InspdateTime;

            if (_plcParam.nCommType == (int)GlovalVar.PLCType.MX)
            {
                if (_mxPlc != null)
                {
                    //Thread threadInspEnd = new Thread(() => _mxPlc.InspCompete(nIdx, strData, bRes, _ModelParam[nIdx], _var));
                    //threadInspEnd.Start();
                    _mxPlc.InspCompete(nIdx, strData, bRes, _ModelParam[nIdx], _var);
                }

            }
            else if (_plcParam.nCommType == (int)GlovalVar.PLCType.Simens)
            {

            }
            else if (_plcParam.nCommType == (int)GlovalVar.PLCType.IO)
            {

            }
            else
            {
                if (_LSplc != null)
                {
                    //Thread threadInspEnd = new Thread(() => _LSplc.InspCompete(nIdx, strData, bRes, _ModelParam[nIdx], _var));
                    //threadInspEnd.Start();
                    _LSplc.InspCompete(nIdx, strData, bRes, _ModelParam[nIdx], _var);

                    //완료 신호
                    if (_plcParam.bIndividualTrigger)
                    {

                        string[] _strSplitIndividualData = _plcParam.strIndividualTrigger.Split(',');
                        _indexCountSum = 0;
                        for (int i = 0; i < _strSplitIndividualData.Length; i++)
                        {
                            _indexCountSum += int.Parse(_strSplitIndividualData[i]);
                            if (nSel == _indexCountSum - 1)
                            {
                                //Thread threadIospEndSignal = new Thread(() => _LSplc.InspCompeteSingle());                 // 완료 신호 
                                //threadIospEndSignal.Start();
                                _LSplc.InspCompeteSingle();
                            }
                        }
                    }
                    else
                    {
                        if (nSel == _var._nScreenCnt - 1)
                        {
                            //Thread threadIospEndSignal = new Thread(() => _LSplc.InspCompeteSingle());
                            //threadIospEndSignal.Start();
                            _LSplc.InspCompeteSingle();
                        }
                    }

                }
                //if (!_CAM[nIdx]._bCamUse)
                //    InsertData(nIdx, bRes, strData, dateTime, strSerial, strCode);
            }

            if (bRes)
                AddMsg(string.Format("#{0} Camera {1} : OK", nSel + 1, Str.InspResult), white, false, false, MsgType.Log);
            else
                AddMsg(string.Format("#{0} Camera {1} : NG", nSel + 1, Str.InspResult), white, false, false, MsgType.Log);
        }

        #region History Insert Data

        private void Createfield(bool bTotalResult, bool[] bResult, string[] strValue, DateTime InspdateTime, string strSerial, string strCode)
        {
            if (_OleDB.State == ConnectionState.Open)
            {
                try
                {
                    #region Command Select
                    string strCommand = "";
                    strCommand += "INSERT INTO Data([Model],[LotID],[Date],[TotalResult]";
                    for (int i = 0; i < _var._nScreenCnt; i++)
                    {
                        strCommand += ",[Cam" + string.Format("{0:D2}", i + 1) + "]";
                        if (_ModelParam[i].bAlingInsp)
                        {
                            strCommand += ",[Cam" + string.Format("{0:D2}", i + 1) + "_AlignX]";
                            strCommand += ",[Cam" + string.Format("{0:D2}", i + 1) + "_AlignY]";
                            strCommand += ",[Cam" + string.Format("{0:D2}", i + 1) + "_AlignZ]";
                        }
                        if (_ModelParam[i].bBCRInsp)
                        {
                            strCommand += ",[Cam" + string.Format("{0:D2}", i + 1) + "_BCR]";
                        }
                        if (_ModelParam[i].bPinChange)
                        {
                            strCommand += ",[Cam" + string.Format("{0:D2}", i + 1) + "_Pin]";
                        }
                        if (_var._bUseSensor)
                        {
                            if (i == 1)
                            {
                                strCommand += ",[Cam" + string.Format("{0:D2}", i + 1) + "_1PointValue]";
                                strCommand += ",[Cam" + string.Format("{0:D2}", i + 1) + "_2PointValue]";
                                strCommand += ",[Cam" + string.Format("{0:D2}", i + 1) + "_3PointValue]";
                                strCommand += ",[Cam" + string.Format("{0:D2}", i + 1) + "_4PointValue]";
                                strCommand += ",[Cam" + string.Format("{0:D2}", i + 1) + "_5PointValue]";
                                strCommand += ",[Cam" + string.Format("{0:D2}", i + 1) + "_6PointValue]";
                            }
                        }
                    }
                    strCommand += ") VALUES (?,?,?,?";
                    for (int j = 0; j < _var._nScreenCnt; j++)
                    {
                        strCommand += ",?";
                        if (_ModelParam[j].bAlingInsp)
                            strCommand += ",?,?,?";
                        if (_ModelParam[j].bBCRInsp)
                            strCommand += ",?";
                        if (_ModelParam[j].bPinChange)
                            strCommand += ",?";
                        if (_var._bUseSensor)
                        {
                            if (j == 1)
                                strCommand += ",?,?,?,?,?,?";
                        }
                    }
                    strCommand += ")";
                    #endregion
                    using (OleDbCommand insertCommand = new OleDbCommand(strCommand, _OleDB))
                    {
                        insertCommand.Parameters.AddWithValue("@Model", _var._strModelName);
                        insertCommand.Parameters.AddWithValue("@LotID", strSerial);
                        insertCommand.Parameters.AddWithValue("@Date", InspdateTime.ToString("yyyy-MM-dd HH:mm:ss"));

                        if (bTotalResult == true)
                            insertCommand.Parameters.AddWithValue("@TotalResult", "OK");
                        else
                            insertCommand.Parameters.AddWithValue("@TotalResult", "NG");
                        for (int i = 0; i < _var._nScreenCnt; i++)
                        {
                            if (strValue[i] != null)
                            {
                                string[] strsplitValue = strValue[i].Split(',');

                                if (strsplitValue[0] == "1")
                                    insertCommand.Parameters.AddWithValue(("@Cam" + string.Format("{0:D2}", i + 1)), "OK");
                                else
                                    insertCommand.Parameters.AddWithValue(("@Cam" + string.Format("{0:D2}", i + 1)), "NG");
                                if (_ModelParam[i].bBCRInsp)
                                    insertCommand.Parameters.AddWithValue(("@Cam" + string.Format("{0:D2}", i + 1) + "_BCR"), strsplitValue[1]);
                                if (_ModelParam[i].bAlingInsp)
                                {
                                    double.TryParse(strsplitValue[2], out var dX);
                                    double.TryParse(strsplitValue[3], out var dY);
                                    double.TryParse(strsplitValue[4], out var dZ);
                                    insertCommand.Parameters.AddWithValue(("@Cam" + string.Format("{0:D2}", i + 1) + "_AlignX"), string.Format("{0:F3}", dX * 0.001));
                                    insertCommand.Parameters.AddWithValue(("@Cam" + string.Format("{0:D2}", i + 1) + "_AlignY"), string.Format("{0:F3}", dY * 0.001));
                                    insertCommand.Parameters.AddWithValue(("@Cam" + string.Format("{0:D2}", i + 1) + "_AlignZ"), string.Format("{0:F3}", dZ * 0.001));
                                }
                                if (_ModelParam[i].bPinChange)
                                    insertCommand.Parameters.AddWithValue("@Cam" + string.Format("{0:D2}", i + 1) + "Pin", strsplitValue[5]);
                                if (_var._bUseSensor)
                                {
                                    if (i == 1)
                                    {
                                        string[] SensorData = new string[6] { "0", "0", "0", "0", "0", "0" };
                                        AddMsg("Sersor Data Histroy : " + strsplitValue[1], Color.GreenYellow, false, false, MsgType.Log);

                                        SensorData = strsplitValue[1].Split(';');
                                        if (SensorData.Length >= 6)
                                        {
                                            insertCommand.Parameters.AddWithValue(("@Cam" + string.Format("{0:D2}", i + 1) + "_1PointValue"), SensorData[0]);
                                            insertCommand.Parameters.AddWithValue(("@Cam" + string.Format("{0:D2}", i + 1) + "_2PointValue"), SensorData[1]);
                                            insertCommand.Parameters.AddWithValue(("@Cam" + string.Format("{0:D2}", i + 1) + "_3PointValue"), SensorData[2]);
                                            insertCommand.Parameters.AddWithValue(("@Cam" + string.Format("{0:D2}", i + 1) + "_4PointValue"), SensorData[3]);
                                            insertCommand.Parameters.AddWithValue(("@Cam" + string.Format("{0:D2}", i + 1) + "_5PointValue"), SensorData[4]);
                                            insertCommand.Parameters.AddWithValue(("@Cam" + string.Format("{0:D2}", i + 1) + "_6PointValue"), SensorData[5]);
                                        }
                                        else if (SensorData.Length < 6)
                                        {
                                            insertCommand.Parameters.AddWithValue(("@Cam" + string.Format("{0:D2}", i + 1) + "_1PointValue"), SensorData[0]);
                                            insertCommand.Parameters.AddWithValue(("@Cam" + string.Format("{0:D2}", i + 1) + "_2PointValue"), SensorData[1]);
                                            insertCommand.Parameters.AddWithValue(("@Cam" + string.Format("{0:D2}", i + 1) + "_3PointValue"), SensorData[2]);
                                            insertCommand.Parameters.AddWithValue(("@Cam" + string.Format("{0:D2}", i + 1) + "_4PointValue"), "0");
                                            insertCommand.Parameters.AddWithValue(("@Cam" + string.Format("{0:D2}", i + 1) + "_5PointValue"), "0");
                                            insertCommand.Parameters.AddWithValue(("@Cam" + string.Format("{0:D2}", i + 1) + "_6PointValue"), "0");
                                        }

                                    }
                                }
                            }
                        }
                        insertCommand.ExecuteNonQuery();
                    }

                    AddMsg("History 등록 완료", white, false, false, MsgType.Log);
                }
                catch (Exception ex)
                {
                    AddMsg(Str.Datainserterr + ex.Message, red, false, false, MsgType.Log);
                }
            }
        }

        #region ACCESS check Field list
        private void CheckHistoryList()
        {
            addfield("TotalResult");
            for (int i = 0; i < _var._nScreenCnt; i++)
            {
                addfield("Cam" + string.Format("{0:D2}", i + 1));
                if (_ModelParam[i].bAlingInsp)
                {
                    addfield("Cam" + string.Format("{0:D2}", i + 1) + "_AlignX");
                    addfield("Cam" + string.Format("{0:D2}", i + 1) + "_AlignY");
                    addfield("Cam" + string.Format("{0:D2}", i + 1) + "_AlignZ");
                }
                if (_ModelParam[i].bBCRInsp)
                    addfield("Cam" + string.Format("{0:D2}", i + 1) + "_BCR");
                if (_ModelParam[i].bPinChange)
                    addfield("Cam" + string.Format("{0:D2}", i + 1) + "_Pin");
                if (_var._bUseSensor)
                {
                    if (i == 1)
                    {
                        addfield("Cam" + string.Format("{0:D2}", i + 1) + "_1PointValue");
                        addfield("Cam" + string.Format("{0:D2}", i + 1) + "_2PointValue");
                        addfield("Cam" + string.Format("{0:D2}", i + 1) + "_3PointValue");
                        addfield("Cam" + string.Format("{0:D2}", i + 1) + "_4PointValue");
                        addfield("Cam" + string.Format("{0:D2}", i + 1) + "_5PointValue");
                        addfield("Cam" + string.Format("{0:D2}", i + 1) + "_6PointValue");
                    }
                }
            }
        }
        private void addfield(string strName)
        {
            bool _bstatus = false;
            if (_OleDB.State == ConnectionState.Open)
            {
                _bstatus = checkField(strName);
                try
                {
                    if (!_bstatus)
                    {
                        string Qurey = "";
                        if (strName == "Date")
                            Qurey = "alter table Data add " + strName + " string";
                        else

                            Qurey = "alter table Data add " + strName + " string";
                        using (OleDbCommand insertCommand = new OleDbCommand(Qurey, _OleDB))
                        {
                            insertCommand.ExecuteScalar();
                        }
                    }
                }
                catch (Exception ex) { }
            }
        }
        private bool checkField(string strName)
        {
            bool _bcheckField = true;
            if (_OleDB.State == ConnectionState.Open)
            {
                try
                {
                    string Qurey = "select top 1 " + strName + " From Data";
                    using (OleDbCommand insertCommand = new OleDbCommand(Qurey, _OleDB))
                    {
                        try
                        {
                            var x = insertCommand.ExecuteScalar();
                        }
                        catch (Exception e)
                        {
                            _bcheckField = false;
                        }
                    }
                }
                catch (Exception ex) { _bcheckField = false; }
            }
            return _bcheckField;
        }
        #endregion
        private void InsertData(int nSel, bool bResult, string[] strValue, DateTime InspdateTime, string strSerial, string strCode)
        {
            if (_OleDB.State == ConnectionState.Open)
            {
                try
                {
                    using (OleDbCommand insertCommand = new OleDbCommand("INSERT INTO Data ([Model],[LotID],[Code],[Date],[Time],[CameraNo],[Result], [Data], [AlignX], [AlignY], [AlignZ], [PinData]) VALUES (?,?,?,?,?,?,?,?,?,?,?,?)", _OleDB))
                    {
                        string strRes = bResult ? "OK" : "NG";
                        insertCommand.Parameters.AddWithValue("@Model", _var._strModelName);
                        insertCommand.Parameters.AddWithValue("@LotID", strSerial);
                        insertCommand.Parameters.AddWithValue("@Code", strCode);
                        insertCommand.Parameters.AddWithValue("@Date", Convert.ToDateTime(InspdateTime.ToString("yyyy-MM-dd")));
                        insertCommand.Parameters.AddWithValue("@Time", InspdateTime.ToString("HHmmssfff"));
                        insertCommand.Parameters.AddWithValue("@CameraNo", (nSel + 1).ToString());
                        insertCommand.Parameters.AddWithValue("@Result", strRes);
                        insertCommand.Parameters.AddWithValue("@Data", "");

                        double.TryParse(strValue[2], out var dX);
                        double.TryParse(strValue[3], out var dY);
                        double.TryParse(strValue[4], out var dZ);
                        insertCommand.Parameters.AddWithValue("@AlignX", string.Format("{0:F3}", dX * 0.001));
                        insertCommand.Parameters.AddWithValue("@AlignY", string.Format("{0:F3}", dY * 0.001));
                        insertCommand.Parameters.AddWithValue("@AlignZ", string.Format("{0:F3}", dZ * 0.001));

                        if (!string.IsNullOrEmpty(strValue[5]))
                            insertCommand.Parameters.AddWithValue("@PinData", strValue[5]);
                        else
                            insertCommand.Parameters.AddWithValue("@PinData", "");

                        insertCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    AddMsg(Str.Datainserterr + ex.Message, red, false, false, MsgType.Log);
                }
            }
        }
        #endregion

        private void CamGrabComplete(int nSel)
        {
            //int nIdx = nSel;

            //if (_plcParam.nCommType == (int)GlovalVar.PLCType.MX)
            //{
            //    if (_mxPlc != null)
            //    {
            //        Thread threadGrabEnd = new Thread(() => _mxPlc.GrabComplete(nIdx));
            //        threadGrabEnd.Start();
            //    }
            //}
            //else if (_plcParam.nCommType == (int)GlovalVar.PLCType.MX)
            //{
            //    //Thread threadGrabEnd = new Thread(() => _siemensPLC.GrabComplete(nIdx));
            //    //threadGrabEnd.Start();
            //}
            //else if (_plcParam.nCommType == (int)GlovalVar.PLCType.MX)
            //{
            //    //Thread threadGrabEnd = new Thread(() => _ioBoard.GrabComplete(nIdx));
            //    //threadGrabEnd.Start();
            //}
            //else
            //{
            //    if (_LSplc != null)
            //    {
            //        Thread threadGrabEnd = new Thread(() => _LSplc.GrabComplete(nIdx));
            //        threadGrabEnd.Start();
            //    }
            //}

            //if (_bPassmode)
            //{
            //    if (_var._bOriginImageSave)
            //    {
            //        //_CAM[nIdx].SavePassOriginImage();
            //        Thread ThreadSaveOriginImg = new Thread(_CAM[nIdx].SavePassOriginImage);
            //        ThreadSaveOriginImg.Start();
            //    }

            //    Invoke(new EventHandler(delegate
            //    {
            //        if (_plcParam.nCommType == (int)GlovalVar.PLCType.MX)
            //        {
            //            if (_mxPlc != null)
            //            {
            //                Thread threadInspEnd = new Thread(() => _mxPlc.InspCompete(nIdx, true));
            //                threadInspEnd.Start();
            //            }

            //            InsertData(nIdx, true, null, DateTime.Now, lblLotNo.Caption, _ModelParam[nIdx].strCode);
            //        }
            //        else if (_plcParam.nCommType == (int)GlovalVar.PLCType.Simens)
            //        {

            //        }
            //        else if (_plcParam.nCommType == (int)GlovalVar.PLCType.IO)
            //        {

            //        }
            //        else
            //        {
            //            if (_LSplc != null)
            //            {
            //                Thread threadInspEnd = new Thread(() => _LSplc.InspCompete(nIdx, true));
            //                threadInspEnd.Start();
            //            }

            //            InsertData(nIdx, true, null, DateTime.Now, lblLotNo.Caption, _ModelParam[nIdx].strCode);
            //        }
            //    }));

            //    _bInspRes[nIdx] = true;
            //    _bResult[nIdx] = true;

            //    ChkInspection();
            //    //SetCount();

            //}

            //AddMsg(string.Format("#{0} Camera {1}", nSel +1, Str.Grabcomplete), green, false, false);
        }
        #region Count 
        private void SetCount()
        {
            Invoke(new EventHandler(delegate
            {
                lblTotalCnt.Caption = _var._nTotalCnt.ToString();
                lblOKCnt.Caption = _var._nOKCnt.ToString();
                lblNGCnt.Caption = _var._nNGCnt.ToString();
            }));

            SaveCount();
        }

        private void SaveCount()
        {
            ini.WriteIniFile("TotalCnt", "Value", _var._nTotalCnt.ToString(), _var._strConfigPath, "Config.ini");
            ini.WriteIniFile("OkCnt", "Value", _var._nOKCnt.ToString(), _var._strConfigPath, "Config.ini");
            ini.WriteIniFile("NGCnt", "Value", _var._nNGCnt.ToString(), _var._strConfigPath, "Config.ini");
        }
        #endregion

        #region CAM Function

        private void CamConnecting()
        {
            try
            {
                _CAM = new CAM[_nCamCnt];
                if (_var._bUsePinchange == true)
                    _frmMDI = new frmMDI[_nCamCnt + 1];
                else
                    _frmMDI = new frmMDI[_nCamCnt];

                for (int i = 0; i < _var._nScreenCnt; i++)
                {
                    op.Title = string.Format("Camera #{0} {1}", i + 1, Str.connecting);
                    SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, op);

                    CamConn(i);
                }
                if (_var._bUsePinchange)
                    PinchangeConn(_var._nScreenCnt);


            }
            catch (Exception ex)
            {
                AddMsg(Str.CamConnerr + ex.Message, red, false, false, MsgType.Alarm);
            }
        }
        private void CamConn(int nSel)
        {
            int nIdx = nSel;

            Invoke(new EventHandler(delegate
            {
                if (!_var._bUsePrint)
                {
                    _frmMDI[nIdx] = new frmMDI(this, nIdx);
                    _CAM[nIdx] = new CAM();
                    _CAM[nIdx]._CamSetFunc = CamSetFunc;
                    _CAM[nIdx]._OnGrabComplete = CamGrabComplete;
                    _CAM[nIdx]._OnInspComplete = CamInspComplete;
                    _CAM[nIdx]._OnMessage = Cam_OnMessage;
                    _CAM[nIdx]._OnPositionChange = OnPositionChange;
                    _CAM[nIdx]._OnResultImageList = OnResultImageList;
                    //_CAM[nIdx]._OnSensorTrigger = OnSensorTrigger();
                    _CAM[nIdx]._OnSensorTrigger = OnSensorTrigger;
                    _CAM[nIdx]._OnSensorModelChange = OnSensorModelChange;
                    _CAM[nIdx]._nIdx = nIdx;
                    _CAM[nIdx]._nPos = nIdx;
                    _CAM[nIdx]._modelParam = _ModelParam[nIdx];
                    _CAM[nIdx]._var = _var;

                    if (!string.IsNullOrEmpty(_camParam[nIdx].strCopy))
                    {
                        if (_camParam[nIdx].strCopy != "-1")
                        {
                            int.TryParse(_camParam[nIdx].strCopy, out int nCopy);
                            _camSet[nIdx] = _camSet[nCopy];
                            _camSet[nIdx]._bCamCopy = true;
                        }
                        else
                        {
                            if (_camSet[nIdx].CamConnect(nIdx, _camParam, _mFrameGrabbers, _cameras, m_stDeviceList, _ModelParam[nIdx]) != "")
                            {
                                SetCamStatus(false);
                            }
                        }
                    }
                    else
                    {
                        SetCamStatus(false);
                    }

                    _CAM[nIdx]._camSet = _camSet[nIdx];
                    _CAM[nIdx]._camSet._GrabFunc = GrabFunc;
                    _frmMDI[nIdx].Controls.Add(_CAM[nIdx]);
                    _CAM[nIdx].Dock = DockStyle.Fill;
                    _CAM[nIdx]._modelParam = _ModelParam[nIdx];
                    _CAM[nIdx].LoadSet();
                    _frmMDI[nIdx].Show();
                }
                else
                {
                    _frmMDI[0] = new frmMDI(this, 0);
                    _frmMDI[0].Controls.Add(_PrintControl);
                    _PrintControl._PrintParam = _PrintParam;
                    _PrintControl.Dock = DockStyle.Fill;
                    _frmMDI[0].Show();
                }

            }));
        }

        private void PinchangeConn(int nSel)
        {
            int nIdx = nSel;

            Invoke(new EventHandler(delegate
            {

                _frmMDI[nIdx] = new frmMDI(this, nIdx);
                _pin = new PinCheck();
                _frmMDI[nIdx].Controls.Add(_pin);
                _pin.Dock = DockStyle.Fill;
                _frmMDI[nIdx].Show();
            }));
        }
        public void GrabFunc(int nSel, Bitmap bmpImg, ICogImage cogImg)
        {
            int nidx = nSel;
            ICogImage cogGrabImg = null;
            Bitmap bmpGrab = null;
            try
            {
                Invoke(new EventHandler(delegate
                {

                    if (bmpImg != null)
                        bmpGrab = (Bitmap)bmpImg.Clone();

                    if (cogImg != null)
                        cogGrabImg = cogImg;

                    if (cogGrabImg == null && bmpGrab == null)
                    {
                        SetCamStatus(false);
                        AddMsg("Cam Disconnected", red, false, false, MsgType.Alarm);
                        return;
                    }

                    Thread threadCamGrab = new Thread(() => _CAM[nidx].GrabFunc(nidx, bmpGrab, cogGrabImg));
                    threadCamGrab.Start();
                    //_CAM[nidx].GrabFunc(nidx, bmpGrab, cogGrabImg);


                    if (_bPassmode)
                    {
                        Thread.Sleep(100);

                        VisionComplete(nidx, true, null, DateTime.Now, lblLotNo.Caption, _ModelParam[nidx].strCode);

                    }
                }));
            }
            catch (Exception ex)
            {
                AddMsg("GrabFunc Error : " + ex.ToString(), red, false, false, MsgType.Alarm);
            }
        }
        public void InitCam()
        {
            try
            {
                LicenseChk();

                _camSet = new CamSet[_nCamCnt];

                for (int i = 0; i < _nCamCnt; i++)
                {
                    if (_camSet[i] == null)
                    {
                        _camSet[i] = new CamSet();
                        _camSet[i]._cam = _cam;
                        _camSet[i]._nIdx = i;
                    }
                }

                try
                {
                    _mFrameGrabbers = null;
                    _mFrameGrabbers = new CogFrameGrabbers();
                }
                catch { }

                try
                {
                    _cam = new PylonCam();

                    if (_cameras != null)
                        _cameras.Clear();

                    _cam.Init();
                    _cameras = _cam.Connect();
                }
                catch { }

                try
                {
                    m_stDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();

                    m_stDeviceList.nDeviceNum = 0;
                    int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_stDeviceList);
                }
                catch { }

                if (_mFrameGrabbers.Count == 0 && _cameras.Count == 0)
                    AddMsg(Str.NoCam, red, false, false, MsgType.Alarm);


            }
            catch (Exception ex)
            {
                AddMsg(Str.initcamerr + ex.Message, red, false, false, MsgType.Alarm);
            }
        }
        private void CamSetOnMessage(string strMsg, Color color)
        {
            AddMsg(strMsg, color, false, false, MsgType.Alarm);
        }
        private void CamSetFunc(int nIdx)
        {
            if (_frmCamera != null)
            {
                _frmCamera.Dispose();
                _frmCamera = null;
            }

            _frmCamera = new frmCamera(this);
            _frmCamera._nIdx = nIdx;
            _frmCamera.LoadSet();
            _frmCamera.Show();
            //_frmCamera.TopMost = true;
        }
        #endregion

        #region History Data Connection
        public void LoadData()
        {
            try
            {
                string strConn = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Application.StartupPath + @"\Data\Data.accdb";
                _OleDB = new OleDbConnection(strConn);
                _OleDB.Open();
                if (_OleDB.State == ConnectionState.Open)
                    AddMsg(Str.dataOpen, green, false, false, MsgType.Alarm);

                CheckHistoryList();

                if (_var._bUsePrint == true)
                {
                    string strConn2 = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Application.StartupPath + @"\Data\Print.accdb";
                    _PrintOleDB = new OleDbConnection(strConn2);
                    _PrintOleDB.Open();
                    if (_PrintOleDB.State == ConnectionState.Open)
                        AddMsg(Str.dataOpen, green, false, false, MsgType.Alarm);
                }
            }
            catch (Exception ex)
            {
                AddMsg(Str.dataloaderr + ex.Message, red, false, false, MsgType.Alarm);
            }
        }
        #endregion
        private DateTime Delay(int MS)
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
        private void DriveSize()
        {
            DriveInfo[] drives;
            Stopwatch sw = new Stopwatch();
            string TotalSize = "";
            string freeSize = "";
            string usage = "";
            int nCnt = 0;
            double[] dUsage = new double[4] { 0, 80, 90, 70 };

            sw.Start();
            while (true)
            {
                if (!_bChkDrive)
                    return;

                drives = DriveInfo.GetDrives();

                try
                {
                    Invoke(new EventHandler(delegate
                    {
                        for (int i = 0; i < drives.Length; i++)
                        {
                            if (i < 2)
                            {
                                if (drives[i].DriveType == DriveType.Fixed)
                                {
                                    if (drives[i].Name.Substring(0, 1) == _var._strSaveImagePath.Substring(0, 1)) 
                                    {
                                        lblDrive1.Caption = string.Format("{0} : ", drives[i].Name.Substring(0, 1));
                                        lblDrive1.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                                        Bar1.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                                    }
                                    else if (drives[i].Name.Substring(0, 1) == _var._strSaveResultImagePath.Substring(0, 1))
                                    {
                                        lblDrive2.Caption = string.Format("{0} : ", drives[i].Name.Substring(0, 1));
                                        lblDrive2.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                                        Bar2.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                                    }
                                }
                            }
                        }

                        if (sw.ElapsedMilliseconds >= 3000)
                        {
                            sw.Reset();
                            sw.Start();

                            foreach (DriveInfo dr in drives)
                            {
                                if (nCnt > 3)
                                    break;
                                if (dr.DriveType == DriveType.Fixed)
                                {
                                    TotalSize = Convert.ToInt32(dr.TotalSize / 1024 / 1024 / 1024).ToString();
                                    freeSize = Convert.ToInt32(dr.AvailableFreeSpace / 1024 / 1024 / 1024).ToString();
                                    usage = (Convert.ToInt32(TotalSize) - Convert.ToInt32(freeSize)).ToString();
                                    dUsage[1] = _var._nDiskOriginalDelete;
                                    dUsage[2] = _var._nDiskResultDelete;
                                    dUsage[3] = _var._nDiskAutoDelete;


                                    Invoke(new EventHandler(delegate
                                    {
                                        if (dr.Name.Substring(0, 1) == _var._strSaveImagePath.Substring(0, 1))
                                            //if (nCnt==0)
                                            {
                                            dUsage[0] = ((Convert.ToInt32(usage) * 1.0) / (Convert.ToInt32(TotalSize) * 1.0)) * 100;
                                            //Bar1.Name = _var._strSaveImagePath.Substring(0, 2);
                                            Bar1.EditValue = (int)dUsage[0];

                                            if (IsAutoDelete(_var._AutoDeleteTime1, _var._AutoDeleteTime2))
                                            {
                                                if (dUsage[0] > dUsage[3])
                                                {
                                                    if (!_bOriginalDelete)
                                                    {
                                                        _bOriginalDelete = true;
                                                        Thread threadOriginalDelete = new Thread(OriginalImageDelete);
                                                        threadOriginalDelete.Start();
                                                    }
                                                }
                                            }
                                        }
                                        else if (dr.Name.Substring(0, 1) == _var._strSaveResultImagePath.Substring(0, 1))
                                        //else if (nCnt==1)
                                        {
                                            dUsage[0] = ((Convert.ToInt32(usage) * 1.0) / (Convert.ToInt32(TotalSize) * 1.0)) * 100;
                                            //Bar2.Name = _var._strSaveResultImagePath.Substring(0, 2); 
                                            Bar2.EditValue = (int)dUsage[0];

                                            if (IsAutoDelete(_var._AutoDeleteTime1, _var._AutoDeleteTime2))
                                            {
                                                if (dUsage[0] > dUsage[3])
                                                {
                                                    if (!_bResultDelete)
                                                    {
                                                        _bResultDelete = true;
                                                        Thread threadResultDelete = new Thread(ResultImageDelete);
                                                        threadResultDelete.Start();
                                                    }
                                                }
                                            }
                                        }

                                        bool IsAutoDelete(string StartTime, string EndTime)
                                        {
                                            long DelStartTime = long.Parse(StartTime.Replace(":", ""));
                                            long DelEndTime = long.Parse(EndTime.Replace(":", ""));
                                            long NowTime = long.Parse(String.Format(DateTime.Now.ToString("HHmmss")));
                                            if (DelStartTime < NowTime && NowTime < DelEndTime)
                                            {
                                                return true;                                                
                                            }
                                            else
                                            {
                                                return false;
                                            }
                                        }

                                        

                                        

                                        
                                            
                                        


                                        //if (dUsage[0] > dUsage[1])
                                        //{
                                        //    if (dr.Name.Substring(0, 1) == _var._strSaveImagePath.Substring(0, 1))
                                        //    {
                                        //        if (!_bOriginalDelete)
                                        //        {
                                        //            _bOriginalDelete = true;
                                        //            Thread threadDelete = new Thread(OriginalImageDelete);
                                        //            threadDelete.Start();
                                        //        }
                                        //    }
                                        //}
                                        //if (dUsage[0] > dUsage[2])
                                        //{
                                        //    if (dr.Name.Substring(0, 1) == _var._strSaveResultImagePath.Substring(0, 1))
                                        //    {
                                        //        if (!_bResultDelete)
                                        //        {
                                        //            _bResultDelete = true;
                                        //            Thread threadDelete = new Thread(ResultImageDelete);
                                        //            threadDelete.Start();
                                        //        }
                                        //    }
                                        //}

                                    }));
                                }
                                nCnt++;
                            }
                            nCnt = 0;
                        }
                    }));
                }
                catch { }

                Thread.Sleep(50);
            }
        }
        private void CurrentTime()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (_bTimeThread)
            {
                if (sw.ElapsedMilliseconds >= 1000)
                {
                    sw.Reset();
                    sw.Start();

                    Invoke(new EventHandler(delegate
                    {
                        lblTime.Caption = DateTime.Now.ToString("HH:mm:ss");
                        try
                        {
                            if (DateTime.Now.ToString("HH:mm:ss") == _var._tsReset)
                            {
                                lblTotalCnt.Caption = "0";
                                lblOKCnt.Caption = "0";
                                lblNGCnt.Caption = "0";
                                _var._nTotalCnt = 0;
                                _var._nOKCnt = 0;
                                _var._nNGCnt = 0;

                                SaveCount();

                                for (int i = 0; i < _var._nScreenCnt; i++)
                                {
                                    _CAM[i]._nTotal = 0;
                                    _CAM[i]._nOK = 0;
                                    _CAM[i]._nNG = 0;
                                    _CAM[i].SetCount();
                                    _CAM[i].SaveCount();
                                }
                            }
                        }
                        catch { }
                    }));
                }
                Thread.Sleep(1);
            }
        }


        #region Communcation 연결
        public void CommConnect()   //Communication Connect(MX, Simens, IO)
        {
            Invoke(new EventHandler(delegate
            {
                try
                {
                    if (_plcParam.nCommType > -1)
                    {
                        if (_plcParam.nCommType == (int)GlovalVar.PLCType.MX)
                        {
                            lblPLC.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                            lblPLC.Caption = "PLC";

                            if (_mxPlc == null)
                            {
                                _mxPlc = new MXplc();
                                _mxPlc.LoadSet();
                                _mxPlc._plcParam = new GlovalVar.PLCPram();
                                _mxPlc._OnConnect = OnConnect;
                                _mxPlc._OnDisconnect = OnDisconnect;
                                _mxPlc._OnTrigger = OnTrigger;
                                _mxPlc._OnModelChange = OnModelChange;
                                _mxPlc._OnLotIDReceive = OnLotIDReceive;
                                _mxPlc._OnMessage = OnPLCMessage;
                                _mxPlc._OnRecvData = OnRecvData;
                            }

                            _bCommConnect = _mxPlc.Connect(_plcParam);

                            if (_bCommConnect)
                            {
                                lblPLC.ItemAppearance.Normal.ForeColor = Color.Lime;
                                //ChangeImage(picComm, "Green");
                                AddMsg(Str.plcconn, green, false, false, MsgType.Alarm);
                            }
                            else
                            {
                                AddMsg(Str.plcdisconn, red, false, false, MsgType.Alarm);
                            }

                        }
                        else if (_plcParam.nCommType == (int)GlovalVar.PLCType.Simens)
                        {
                            lblPLC.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                            lblPLC.Caption = "PLC";

                            if (_siemensPLC == null)
                            {
                                _siemensPLC = new SiemensPLC();
                                _siemensPLC._plcParam = new GlovalVar.PLCPram();
                                _siemensPLC._OnConnect = OnConnect;
                                _siemensPLC._OnDisconnect = OnDisconnect;
                                _siemensPLC._OnTrigger = OnTrigger;
                                _siemensPLC._OnModelChange = OnModelChange;
                                _siemensPLC._OnLotIDReceive = OnLotIDReceive;
                                _siemensPLC._OnMessage = OnPLCMessage;
                            }

                            _bCommConnect = _siemensPLC.Connect(_plcParam);

                            if (_bCommConnect)
                            {
                                lblPLC.ItemAppearance.Normal.ForeColor = Color.Lime;
                                AddMsg(Str.plcconn, green, false, false, MsgType.Alarm);
                            }
                            else
                                AddMsg(Str.plcdisconn, red, false, false, MsgType.Alarm);
                        }
                        else if (_plcParam.nCommType == (int)GlovalVar.PLCType.IO)
                        {
                            lblPLC.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                            lblPLC.Caption = "IO";

                            if (_ioBoard == null)
                            {
                                _ioBoard = new ioBoard();
                                _ioBoard._plcParam = new GlovalVar.PLCPram();
                                _ioBoard._OnConnect = OnConnect;
                                _ioBoard._OnDisconnect = OnDisconnect;
                                _ioBoard._OnTrigger = OnTrigger;
                                _ioBoard._OnModelChange = OnModelChange;
                                _ioBoard._OnLotIDReceive = OnLotIDReceive;
                                _ioBoard._OnMessage = OnPLCMessage;
                            }

                            _bCommConnect = _ioBoard.Connect(_plcParam);

                            if (_bCommConnect)
                            {
                                lblPLC.ItemAppearance.Normal.ForeColor = Color.Lime;
                                AddMsg(Str.IOconn, green, false, false, MsgType.Alarm);
                            }
                            else
                                AddMsg(Str.IOdisconn, red, false, false, MsgType.Alarm);

                        }
                        else
                        {
                            lblPLC.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                            lblPLC.Caption = "PLC";

                            if (_LSplc == null)
                            {
                                _LSplc = new XGCommSocket();
                                //_LSplc.LoadSet();
                                _LSplc._plcParam = new GlovalVar.PLCPram();
                                _LSplc._OnConnect = OnConnect;
                                _LSplc._OnDisconnect = OnDisconnect;
                                _LSplc._OnTrigger = OnTrigger;
                                _LSplc._OnModelChange = OnModelChange;
                                _LSplc._OnLotIDReceive = OnLotIDReceive;
                                _LSplc._OnPassData = OnPassDataConfirm;
                                _LSplc._OnTime = OnTimeCheck;
                                _LSplc._OnMessage = OnPLCMessage;
                                _LSplc._OnRecvData = OnRecvData;
                                _LSplc._nScreenCnt = _var._nScreenCnt;
                            }

                            _bCommConnect = _LSplc.Open(_plcParam);

                            if (_bCommConnect)
                            {
                                lblPLC.ItemAppearance.Normal.ForeColor = Color.Lime;
                                AddMsg(Str.plcconn, green, false, false, MsgType.Alarm);
                            }
                            else
                            {
                                AddMsg(Str.plcdisconn, red, false, false, MsgType.Alarm);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddMsg("Communication Connect Error : " + ex.Message, red, false, false, MsgType.Alarm);
                }
            }));
        }
        #endregion
        public void ChangeImage(object picture, string strColoe)    //상태 이미지 변경
        {
            PictureBox ptbox = picture as PictureBox;

            try
            {
                if (InvokeRequired)
                {
                    Invoke(new EventHandler(delegate
                    {
                        if (strColoe == "Red")
                        {
                            if (ptbox.Image != Properties.Resources.Red)
                                ptbox.Image = Properties.Resources.Red;

                        }
                        else if (strColoe == "Green")
                        {
                            if (ptbox.Image != Properties.Resources.Green)
                                ptbox.Image = Properties.Resources.Green;
                        }
                        else if (strColoe == "Gray")
                        {
                            if (ptbox.Image != Properties.Resources.Gray)
                                ptbox.Image = Properties.Resources.Gray;
                        }
                    }));
                }
                else
                {
                    if (strColoe == "Red")
                    {
                        if (ptbox.Image != Properties.Resources.Red)
                            ptbox.Image = Properties.Resources.Red;

                    }
                    else if (strColoe == "Green")
                    {
                        if (ptbox.Image != Properties.Resources.Green)
                            ptbox.Image = Properties.Resources.Green;
                    }
                    else if (strColoe == "Gray")
                    {
                        if (ptbox.Image != Properties.Resources.Gray)
                            ptbox.Image = Properties.Resources.Gray;
                    }
                }
            }

            catch { }
        }
        private void CreateForm()
        {
            Invoke(new EventHandler(delegate
            {
                _frmCamera = new frmCamera(this);
                _frmModel = new frmModel(this);
                _PrintControl._var = _var;
                listModel.Strings.Clear();
                for (int i = 0; i < _var._listModel.Count; i++)
                    listModel.Strings.Add(_var._listModel[i]);
                string strRes = _frmCamera.SetCameraInfo();
            }));
        }

        #region CAM,Communication 데이터 및 저장 위치 로드
        private void LoadSet()
        {
            try
            {
                FileInfo fi = new FileInfo("C:\\ProgramData\\PassWord" + "\\AdminPW.pwd");
                if (fi.Exists)
                    _var._strAdminPW = ini.ReadIniFile("AdminPW", "Value", "C:\\ProgramData\\PassWord", "AdminPW.pwd");
                else
                    _var._strAdminPW = "0000";

                fi = new FileInfo("C:\\ProgramData\\PassWord" + "\\OperatorPW.pwd");
                if (fi.Exists)
                    _var._strOPPW = ini.ReadIniFile("OperatorPW", "Value", "C:\\ProgramData\\PassWord", "OperatorPW.pwd");
                else
                    _var._strOPPW = "9999";

                DirectoryInfo dr = null;

                //dr = new DirectoryInfo(_var._strOriginalImgPath);
                //if (!dr.Exists)
                //    dr.Create();

                dr = new DirectoryInfo(_var._strCameraInfoPath);
                if (!dr.Exists)
                    dr.Create();

                dr = new DirectoryInfo(_var._strMasterImagePath);
                if (!dr.Exists)
                    dr.Create();

                dr = new DirectoryInfo(_var._strModelPath);
                if (!dr.Exists)
                    dr.Create();

                string[] strCamFile = Directory.GetFiles(_var._strCameraInfoPath, "*.ini");
                Array.Sort(strCamFile);
                string strTemp = "";

                _camParam = new GlovalVar.CamPram[_nCamCnt];

                _nCamInfo = strCamFile.Length;
                string strFileName = "";
                for (int i = 0; i < _nCamInfo; i++)
                {
                    strFileName = Path.GetFileName(strCamFile[i]);

                    _camParam[i].strCamType = ini.ReadIniFile("CamType", "Value", _var._strCameraInfoPath, strFileName);
                    _camParam[i].strCamSerial = ini.ReadIniFile("CamSerial", "Value", _var._strCameraInfoPath, strFileName);
                    _camParam[i].strCamFormat = ini.ReadIniFile("CamFormat", "Value", _var._strCameraInfoPath, strFileName);
                    _camParam[i].strIP = ini.ReadIniFile("CamIP", "Value", _var._strCameraInfoPath, strFileName);
                    double.TryParse(ini.ReadIniFile("CamExpose", "Value", _var._strCameraInfoPath, strFileName), out _camParam[i].dExpose);
                    double.TryParse(ini.ReadIniFile("CamBright", "Value", _var._strCameraInfoPath, strFileName), out _camParam[i].dBright);
                    double.TryParse(ini.ReadIniFile("CamContrast", "Value", _var._strCameraInfoPath, strFileName), out _camParam[i].dContract);

                    _camParam[i].nTimeout = 1000;
                    strTemp = ini.ReadIniFile("CamTimeout", "Value", _var._strCameraInfoPath, strFileName);
                    if (strTemp != "")
                        int.TryParse(strTemp, out _camParam[i].nTimeout);

                    bool.TryParse(ini.ReadIniFile("CamTimeoutUse", "Value", _var._strCameraInfoPath, strFileName), out _camParam[i].bTiime);
                    _camParam[i].strCopy = ini.ReadIniFile("CamCopy", "Value", _var._strCameraInfoPath, strFileName);
                }

                dr = null;
                dr = new DirectoryInfo(_var._strCommInfoPath);   //plc 정보
                if (!dr.Exists)
                    dr.Create();

                //string strTemp = "";
                _plcParam.nCommType = -1;
                strTemp = ini.ReadIniFile("CommType", "Value", _var._strCommInfoPath, "Communication.ini");
                if (strTemp != "")
                    int.TryParse(strTemp, out _plcParam.nCommType);

                _plcParam.strPLCIP = ini.ReadIniFile("IP", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strPLCPort = ini.ReadIniFile("Port", "Value", _var._strCommInfoPath, "Communication.ini");
                short.TryParse(ini.ReadIniFile("Rack", "Value", _var._strCommInfoPath, "Communication.ini"), out _plcParam.shRack);
                short.TryParse(ini.ReadIniFile("Slot", "Value", _var._strCommInfoPath, "Communication.ini"), out _plcParam.shSlot);
                _plcParam.strHeartBeatAddr = ini.ReadIniFile("HeartBeatAddr", "Value", _var._strCommInfoPath, "Communication.ini");

                _plcParam.nHearBeatInterval = 1000;
                strTemp = ini.ReadIniFile("HeartBeatInterval", "Value", _var._strCommInfoPath, "Communication.ini");

                if (strTemp != "")
                    int.TryParse(strTemp, out _plcParam.nHearBeatInterval);

                _plcParam.strStartSignal = ini.ReadIniFile("StartSignal", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strOKSignal = ini.ReadIniFile("OKSignal", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strNGSignal = ini.ReadIniFile("NGSignal", "Value", _var._strCommInfoPath, "Communication.ini");

                _plcParam.nReadInterval = 100;
                strTemp = ini.ReadIniFile("ReadInterval", "Value", _var._strCommInfoPath, "Communication.ini");
                if (strTemp != "")
                    int.TryParse(strTemp, out _plcParam.nReadInterval);
                bool.TryParse(ini.ReadIniFile("ReadSwap", "Value", _var._strCommInfoPath, "Communication.ini"), out _plcParam.bReadSwap);
                bool.TryParse(ini.ReadIniFile("WriteSwap", "Value", _var._strCommInfoPath, "Communication.ini"), out _plcParam.bWriteSwap);
                bool.TryParse(ini.ReadIniFile("FunctionSwap", "Value", _var._strCommInfoPath, "Communication.ini"), out _plcParam.bUseSpecialFunction);
                bool.TryParse(ini.ReadIniFile("CommunciationType", "Value", _var._strCommInfoPath, "Communication.ini"), out _plcParam.bCommunicationType);

                _plcParam.strReadStartAddr = ini.ReadIniFile("ReadStartAddr", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strReadLength = ini.ReadIniFile("ReadLength", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strReadStartTrigger = ini.ReadIniFile("ReadTriggerStar", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strReadEndTrigger = ini.ReadIniFile("ReadTriggerEnd", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strReadModelStart = ini.ReadIniFile("ReadModelStart", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strReadModelEnd = ini.ReadIniFile("ReadModelEnd", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strReadLotStart = ini.ReadIniFile("ReadLotStart", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strReadLotEnd = ini.ReadIniFile("ReadLotEnd", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strReadCamPassStart = ini.ReadIniFile("ReadCamPassStart", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strReadCamPassEnd = ini.ReadIniFile("ReadCamPassEnd", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strWriteStartAddr = ini.ReadIniFile("WriteStartAddr", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strWriteTriggerStart = ini.ReadIniFile("WriteTriggerStart", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strWriteModelStart = ini.ReadIniFile("WriteModelStart", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strWriteLotStart = ini.ReadIniFile("WriteLotStart", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strWriteResStart = ini.ReadIniFile("WriteResStart", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strWriteTotalRes = ini.ReadIniFile("WriteTotalRes", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strWrite2DRes = ini.ReadIniFile("Write2DRes", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strWrite2DData = ini.ReadIniFile("Write2DData", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strWrite2DLen = ini.ReadIniFile("Write2DDLen", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strWriteAlignX = ini.ReadIniFile("WriteAlignX", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strWriteAlignY = ini.ReadIniFile("WriteAlignY", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strWriteAlignZ = ini.ReadIniFile("WriteAlignZ", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strWritePinChange = ini.ReadIniFile("WritePinChange", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strWritePinProhibit = ini.ReadIniFile("WritePinProhibit", "Value", _var._strCommInfoPath, "Communication.ini");
                bool.TryParse(ini.ReadIniFile("TriggerMode", "Value", _var._strCommInfoPath, "Communication.ini"), out _plcParam.bIndividualTrigger);
                _plcParam.strWriteCamPointRes = ini.ReadIniFile("WriteCamPointRes", "Value", _var._strCommInfoPath, "Communication.ini");
                _plcParam.strIndividualTrigger = ini.ReadIniFile("TriggerModeData", "Value", _var._strCommInfoPath, "Communication.ini");

                _var._strWidth = ini.ReadIniFile("WidthCnt", "Value", _var._strConfigPath, "Config.ini");
                _var._strHeight = ini.ReadIniFile("HeightCnt", "Value", _var._strConfigPath, "Config.ini");
                int.TryParse(ini.ReadIniFile("ScreenCnt", "Value", _var._strConfigPath, "Config.ini"), out _var._nScreenCnt);

                _var._strAddfunction = ini.ReadIniFile("function", "Value", _var._strConfigPath, "Config.ini");

                _bResult = new bool[_var._nScreenCnt];
                _bInspRes = new bool[_var._nScreenCnt];
                _strCamData = new string[_var._nScreenCnt];
                _pinShowData = new string[_var._nScreenCnt];

                for (int i = 0; i < _var._nScreenCnt; i++)
                {
                    _bResult[i] = false;
                    _bInspRes[i] = false;
                }

                _var._strModelNo = ini.ReadIniFile("ModelNo", "Value", _var._strConfigPath, "Config.ini");
                _var._strLotNo = ini.ReadIniFile("LotNo", "Value", _var._strConfigPath, "Config.ini");


                strTemp = ini.ReadIniFile("Codelist", "Value", _var._strConfigPath, "Config.ini");
                if (strTemp != "")
                {
                    string[] strCode = strTemp.Split(',');
                    foreach (string str in strCode)
                    {
                        if (str != "")
                            _var._listCode.Add(str);
                    }
                }

                strTemp = ini.ReadIniFile("CodeComment", "Value", _var._strConfigPath, "Config.ini");
                if (strTemp != "")
                {
                    string[] strComment = strTemp.Split(',');
                    foreach (string str in strComment)
                    {
                        if (str != "")
                            _var._listCodeComment.Add(str);
                    }
                }

                txtWidth.Text = _var._strWidth;
                txtHeight.Text = _var._strHeight;

                string[] strFile = Directory.GetFiles(_var._strModelPath, "*.ini");
                string[] strValue = null;

                lblModel.Caption = "";
                lblLotNo.Caption = "";
                for (int i = 0; i < strFile.Length; i++)
                {
                    if (strFile[i].Contains("Modellist"))
                    {
                        strValue = ini.ReadIniFile("Modellist", "Value", _var._strModelPath, "Modellist.ini").Split(',');

                        foreach (string str in strValue)
                        {
                            if (!string.IsNullOrEmpty(str))
                                _var._listModel.Add(str);
                        }

                        if (_var._strModelNo != "")
                        {
                            for (int j = 0; j < _var._listModel.Count; j++)
                            {
                                if (_var._listModel[j].Contains(_var._strModelNo))
                                {
                                    lblModel.Caption = _var._listModel[j];
                                    _var._strModelName = lblModel.Caption;
                                }
                            }
                        }
                        break;
                    }
                }

                lblLotNo.Caption = _var._strLotNo;

                int.TryParse(ini.ReadIniFile("TotalCnt", "Value", _var._strConfigPath, "Config.ini"), out _var._nTotalCnt);
                int.TryParse(ini.ReadIniFile("OKCnt", "Value", _var._strConfigPath, "Config.ini"), out _var._nOKCnt);
                int.TryParse(ini.ReadIniFile("NGCnt", "Value", _var._strConfigPath, "Config.ini"), out _var._nNGCnt);



                lblTotalCnt.Caption = _var._nTotalCnt.ToString();
                lblOKCnt.Caption = _var._nOKCnt.ToString();
                lblNGCnt.Caption = _var._nNGCnt.ToString();

                #region Keyence Data Load
                _sensorParam.strIP = ini.ReadIniFile("SensorIP", "Value", _var._strConfigPath, "Sensor.ini");
                _sensorParam.strPort = ini.ReadIniFile("SensorPort", "Value", _var._strConfigPath, "Sensor.ini");
                double.TryParse(ini.ReadIniFile("SensorLimitLow", "Value", _var._strConfigPath, "Sensor.ini"), out _sensorParam.dLimitLow);
                double.TryParse(ini.ReadIniFile("SensorLimitHigh", "Value", _var._strConfigPath, "Sensor.ini"), out _sensorParam.dLimitLow);
                int.TryParse(ini.ReadIniFile("SensorTriggerTime", "Value", _var._strConfigPath, "Sensor.ini"), out _sensorParam.nTriggerCycleTime);
                int.TryParse(ini.ReadIniFile("SensorModelNo", "Value", _var._strConfigPath, "Sensor.ini"), out _sensorParam.nModelNo);

                txtKeyenceIP.Text = _sensorParam.strIP;
                txtKeyencePort.Text = _sensorParam.strPort;
                txtKeyenceLow.Text = _sensorParam.dLimitLow.ToString();
                txtkeyenceHigh.Text = _sensorParam.dLimitHigh.ToString();
                #endregion

                #region Print Data Load

                _PrintParam.strIP = ini.ReadIniFile("PrintIP", "Value", _var._strConfigPath, "Print.ini");
                _PrintParam.strPort = ini.ReadIniFile("PrintPort", "Value", _var._strConfigPath, "Print.ini");
                _PrintParam.strOriginalX = ini.ReadIniFile("PrintOriginalX", "Value", _var._strConfigPath, "Print.ini");
                _PrintParam.strOriginalY = ini.ReadIniFile("PrintOriginalY", "Value", _var._strConfigPath, "Print.ini");
                _PrintParam.str1DX = ini.ReadIniFile("Print1DX", "Value", _var._strConfigPath, "Print.ini");
                _PrintParam.str1DY = ini.ReadIniFile("Print1DY", "Value", _var._strConfigPath, "Print.ini");
                _PrintParam.str1DWidth = ini.ReadIniFile("Print1DWidth", "Value", _var._strConfigPath, "Print.ini");
                _PrintParam.str1DHeight = ini.ReadIniFile("Print1DHeight", "Value", _var._strConfigPath, "Print.ini");
                _PrintParam.strSerialX = ini.ReadIniFile("PrintSerialX", "Value", _var._strConfigPath, "Print.ini");
                _PrintParam.strSerialY = ini.ReadIniFile("PrintSerialY", "Value", _var._strConfigPath, "Print.ini");
                _PrintParam.strSerialHeight = ini.ReadIniFile("PrintSerialHeight", "Value", _var._strConfigPath, "Print.ini");
                _PrintParam.strSerialWidth = ini.ReadIniFile("PrintSerialWidth", "Value", _var._strConfigPath, "Print.ini");
                _PrintParam.str2DX = ini.ReadIniFile("Print2DX", "Value", _var._strConfigPath, "Print.ini");
                _PrintParam.str2DY = ini.ReadIniFile("Print2DY", "Value", _var._strConfigPath, "Print.ini");
                _PrintParam.str2DSize = ini.ReadIniFile("Print2DSize", "Value", _var._strConfigPath, "Print.ini");
                _PrintParam.strMIPX = ini.ReadIniFile("PrintMIPX", "Value", _var._strConfigPath, "Print.ini");
                _PrintParam.strMIPY = ini.ReadIniFile("PrintMIPY", "Value", _var._strConfigPath, "Print.ini");
                _PrintParam.strMIPHeight = ini.ReadIniFile("PrintMIPHeight", "Value", _var._strConfigPath, "Print.ini");
                _PrintParam.strMIPWidth = ini.ReadIniFile("PrintMIPWidth", "Value", _var._strConfigPath, "Print.ini");

                txtPrintIP.Text = _PrintParam.strIP;
                txtPrintPort.Text = _PrintParam.strPort;
                txtOriginalX.Text = _PrintParam.strOriginalX;
                txtOriginalY.Text = _PrintParam.strOriginalY;
                txtPrint1DX.Text = _PrintParam.str1DX;
                txtPrint1DY.Text = _PrintParam.str1DY;
                txtPrint1DWidth.Text = _PrintParam.str1DWidth;
                txtPrint1DHeight.Text = _PrintParam.str1DHeight;
                txtPrintSeiralX.Text = _PrintParam.strSerialX;
                txtPrintSeiralY.Text = _PrintParam.strSerialY;
                txtPrintSerialWidth.Text = _PrintParam.strSerialWidth;
                txtPrintSerialHeight.Text = _PrintParam.strSerialHeight;
                txtPrint2DDataX.Text = _PrintParam.str2DX;
                txtPrint2DDataY.Text = _PrintParam.str2DY;
                txtPrint2DDataSize.Text = _PrintParam.str2DSize;
                txtPrintMIPX.Text = _PrintParam.strMIPX;
                txtPrintMIPY.Text = _PrintParam.strMIPY;
                txtPrintMIPWidth.Text = _PrintParam.strMIPWidth;
                txtPrintMIPHeight.Text = _PrintParam.strMIPHeight;
                #endregion

                int.TryParse(_LightParam.strLightValue1, out var nLightValue);
                barLight.Value = nLightValue;
                txtLightValue.Text = nLightValue.ToString();

                AddMsg(Str.paramload, green, false, false, MsgType.Alarm);
            }
            catch (Exception ex)
            {
                AddMsg(Str.paramloaderr + ex.Message, red, false, false, MsgType.Alarm);
            }
        }
        public void LoadRecipe()
        {
            _ModelParam = new GlovalVar.ModelParam[_nCamCnt];

            string strValue = "";

            try
            {
                if (!_var._bUsePrint)
                {
                    for (int j = 0; j < _var._nScreenCnt; j++)
                    {
                        strValue = ini.ReadIniFile("InspCnt", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1));
                        if (strValue == "")
                            _ModelParam[j].nInspCnt = 1;
                        else
                            int.TryParse(strValue, out _ModelParam[j].nInspCnt);

                        int.TryParse(ini.ReadIniFile("GrabDelay", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].nGrabdelay);
                        double.TryParse(ini.ReadIniFile("Expose", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].dExpose);
                        _ModelParam[j].strCode = ini.ReadIniFile("Code", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1));
                        int.TryParse(ini.ReadIniFile("DefectGrabCnt", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].nDefectCnt);
                        _ModelParam[j].strExposeInc = ini.ReadIniFile("ExposeIncrease", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1));
                        bool.TryParse(ini.ReadIniFile("DefectInsp", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].bDefectInsp);
                        bool.TryParse(ini.ReadIniFile("DefectBCR", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].bBCRInsp);
                        bool.TryParse(ini.ReadIniFile("DefectBCRSwap", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].bBCRInspSwap);
                        bool.TryParse(ini.ReadIniFile("DefectAlign", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].bAlingInsp);
                        _ModelParam[j].strBCRData = ini.ReadIniFile("BCRData", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1));
                        _ModelParam[j].strBCRLen = ini.ReadIniFile("BCRLen", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1));
                        bool.TryParse(ini.ReadIniFile("PinChange", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].bPinChange);
                        int.TryParse(ini.ReadIniFile("PinCount", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].nPinCnt);

                        double.TryParse(ini.ReadIniFile("AlignMasterX", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].dAlignMasterX);
                        double.TryParse(ini.ReadIniFile("AlignMasterY", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].dAlignMasterY);
                        double.TryParse(ini.ReadIniFile("AlignMasterR", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].dAlignMasterR);
                        double.TryParse(ini.ReadIniFile("AlignOffsetX", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].dAlignOffsetX);
                        double.TryParse(ini.ReadIniFile("AlignOffsetY", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].dAlignOffsetY);
                        double.TryParse(ini.ReadIniFile("AlignOffsetR", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].dAlignOffsetR);
                        double.TryParse(ini.ReadIniFile("Resolution", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].dResoluton);
                        bool.TryParse(ini.ReadIniFile("Xreversal", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].bXreversal);
                        bool.TryParse(ini.ReadIniFile("Yreversal", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].bYreversal);
                        bool.TryParse(ini.ReadIniFile("Zreversal", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].bZreversal);
                        bool.TryParse(ini.ReadIniFile("XYreversal", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].bXYreversal);
                        _ModelParam[j].strPinMaster = ini.ReadIniFile("PinMaster", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1));

                        int.TryParse(ini.ReadIniFile("ResAlign", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1)), out _ModelParam[j].nAlign);
                        strValue = ini.ReadIniFile("FontSize", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1));
                        if (strValue == "")
                            _ModelParam[j].nFontSize = 12;
                        else
                            int.TryParse(strValue, out _ModelParam[j].nFontSize);

                        strValue = ini.ReadIniFile("ResGraphicX", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1));
                        if (strValue == "")
                            _ModelParam[j].nPosX = 10;
                        else
                            int.TryParse(strValue, out _ModelParam[j].nPosX);

                        strValue = ini.ReadIniFile("ResGraphicY", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("CAM{0}.ini", j + 1));
                        if (strValue == "")
                            _ModelParam[j].nPosY = 10;
                        else
                            int.TryParse(strValue, out _ModelParam[j].nPosY);

                        bool.TryParse(ini.ReadIniFile("OriginImageSave", "Value", _var._strConfigPath, "Config.ini"), out _var._bOriginImageSave);
                        bool.TryParse(ini.ReadIniFile("ResultImageSave", "Value", _var._strConfigPath, "Config.ini"), out _var._bResultImageSave);
                        bool.TryParse(ini.ReadIniFile("AutoImageDelete", "Value", _var._strConfigPath, "Config.ini"), out _var._bAutoImageDelete);
                        int.TryParse(ini.ReadIniFile("OriginImageFormat", "Value", _var._strConfigPath, "Config.ini"), out _var._nOriginImageFormat);
                        int.TryParse(ini.ReadIniFile("ResultImageFormat", "Value", _var._strConfigPath, "Config.ini"), out _var._nResultImageFormat);
                        int.TryParse(ini.ReadIniFile("OriginalDelete", "Value", _var._strConfigPath, "Config.ini"), out _var._nDiskOriginalDelete);
                        int.TryParse(ini.ReadIniFile("ResultDelete", "Value", _var._strConfigPath, "Config.ini"), out _var._nDiskResultDelete);
                        int.TryParse(ini.ReadIniFile("AutoDelete", "Value", _var._strConfigPath, "Config.ini"), out _var._nDiskAutoDelete);
                        _var._tsReset = ini.ReadIniFile("ResetTime", "Value", _var._strConfigPath, "Config.ini");
                        _var._AutoDeleteTime1 = ini.ReadIniFile("AutoDeleteTime1", "Value", _var._strConfigPath, "Config.ini");
                        _var._AutoDeleteTime2 = ini.ReadIniFile("AutoDeleteTime2", "Value", _var._strConfigPath, "Config.ini");
                        _var._strSaveImagePath = ini.ReadIniFile("SaveImagePath", "Value", _var._strConfigPath, "Config.ini");
                        _var._strSaveResultImagePath = ini.ReadIniFile("SaveResultImagePath", "Value", _var._strConfigPath, "Config.ini");

                        if (_var._tsReset == "")
                            _var._tsReset = "07:00:00";
                        if (_var._AutoDeleteTime1 == "")
                            _var._AutoDeleteTime1 = "07:10:00";
                        if (_var._AutoDeleteTime2 == "")
                            _var._AutoDeleteTime2 = "07:50:00";
                        if (_var._nDiskOriginalDelete == 0)
                            _var._nDiskOriginalDelete = 80;
                        if (_var._nDiskResultDelete == 0)
                            _var._nDiskResultDelete = 80;
                        if (_var._nDiskAutoDelete == 0)
                            _var._nDiskAutoDelete = 70;

                        swOriginSave.IsOn = _var._bOriginImageSave;
                        swResultSave.IsOn = _var._bResultImageSave;

                        if (_var._nOriginImageFormat == 0)
                            radOriginBMP.Checked = true;
                        else
                            radOriginJPG.Checked = true;

                        if (_var._nResultImageFormat == 0)
                            radResultBMP.Checked = true;
                        else
                            radResultJPG.Checked = true;

                        txtSaveImagePath.Text = _var._strSaveImagePath;
                        txtSaveResultImagePath.Text = _var._strSaveResultImagePath;
                        txtOriginalDiskDelete.Text = _var._nDiskOriginalDelete.ToString();
                        txtResultDiskDelete.Text = _var._nDiskResultDelete.ToString();
                        txtAutoDiskDelete.Text = _var._nDiskAutoDelete.ToString();
                        ResetTimer.Time = Convert.ToDateTime(_var._tsReset);
                        AutoDeleteTimer1.Time = Convert.ToDateTime(_var._AutoDeleteTime1);
                        AutoDeleteTimer2.Time = Convert.ToDateTime(_var._AutoDeleteTime2);
                    }
                }
                else
                {
                    int.TryParse(ini.ReadIniFile("Count", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("PRINT{0}.ini", 1)), out _PrintParam.iCount);
                    _PrintParam.strHMCData = ini.ReadIniFile("MIP", "Value", _var._strModelPath + "\\" + lblModel.Caption, string.Format("PRINT{0}.ini", 1));
                }
            }
            catch (Exception ex)
            {
                AddMsg("Recipe Load Error : " + ex.Message, red, false, false, MsgType.Alarm);
            }
        }
        #endregion

        #region Log & Alarm
        private void OnPLCMessage(string strMsg, Color color, string strMsgType)
        {
            if (strMsgType == "Log")
                AddMsg(string.Format("PLC message : {0}", strMsg), color, false, false, MsgType.Log);
            else
                AddMsg(string.Format("PLC message : {0}", strMsg), color, false, false, MsgType.Alarm);
        }
        private void Cam_OnMessage(string strMsg, Color color, bool bShowPopup, bool bError)
        {
            AddMsg(strMsg, color, bShowPopup, bError, MsgType.Alarm);
        }
        private void labelControl1_DoubleClick(object sender, EventArgs e)
        {
            listAlarmMsg.Clear();
        }
        private void labelControl3_DoubleClick(object sender, EventArgs e)
        {
            listMsg.Clear();
        }
        public void AddMsg(string strMsg, Color color, bool bShowMsg, bool bError, MsgType msgType)
        {
            try
            {
                string strLogMsg = string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss.fff"), strMsg);

                if (InvokeRequired)
                {
                    Invoke(new EventHandler(delegate
                    {
                        if (msgType == MsgType.Log)
                        {
                            if (listMsg.Lines.Count<object>() > 100)
                                listMsg.Clear();

                            listMsg.SelectionStart = listMsg.TextLength;
                            listMsg.SelectionColor = color;
                            listMsg.AppendText(strLogMsg + "\r\n");
                            listMsg.ScrollToCaret();
                        }
                        else
                        {
                            if (listAlarmMsg.Lines.Count<object>() > 100)
                                listAlarmMsg.Clear();

                            listAlarmMsg.SelectionStart = listAlarmMsg.TextLength;
                            listAlarmMsg.SelectionColor = color;
                            listAlarmMsg.AppendText(strLogMsg + "\r\n");
                            listAlarmMsg.ScrollToCaret();
                        }

                        if (bShowMsg)
                        {
                            if (bError)
                                MessageBox.Show(new Form { TopMost = true }, strMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            else
                                MessageBox.Show(new Form { TopMost = true }, strMsg, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }));
                }
                else
                {
                    if (msgType == MsgType.Log)
                    {
                        if (listMsg.Lines.Count<object>() > 100)
                            listMsg.Clear();

                        listMsg.SelectionStart = listMsg.TextLength;
                        listMsg.SelectionColor = color;
                        listMsg.AppendText(strLogMsg + "\r\n");
                        listMsg.ScrollToCaret();
                    }
                    else
                    {
                        if (listAlarmMsg.Lines.Count<object>() > 100)
                            listAlarmMsg.Clear();

                        listAlarmMsg.SelectionStart = listAlarmMsg.TextLength;
                        listAlarmMsg.SelectionColor = color;
                        listAlarmMsg.AppendText(strLogMsg + "\r\n");
                        listAlarmMsg.ScrollToCaret();
                    }

                    if (bShowMsg)
                    {
                        if (bError)
                            MessageBox.Show(strMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        else
                            MessageBox.Show(strMsg, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                var strPath = (msgType == MsgType.Log) ? Application.StartupPath + "\\Message\\Log" : Application.StartupPath + "\\Message\\Alarm";
                Thread threadLogSave = new Thread(() => Log_Save(strLogMsg, strPath + "\\" + DateTime.Now.ToString("yyyy"), msgType));
                threadLogSave.Start();
            }
            catch { }
        }
        private void Log_Save(string Data, string Path, MsgType msgType)
        {
            try
            {
                var strMsg = Data;
                var strPath = Path;
                var Type = msgType;

                DirectoryInfo dr = new DirectoryInfo(strPath);
                if (!dr.Exists)
                    dr.Create();

                if (InvokeRequired)
                {
                    Invoke(new EventHandler(delegate
                    {
                        using (StreamWriter System_Log = new StreamWriter(strPath + "\\" + DateTime.Now.ToString("yyyy_MM_dd") + ".txt", append: true))
                        {
                            System_Log.WriteLine(Data);
                            System_Log.Close();
                        }
                    }));
                }
                else
                {
                    using (StreamWriter System_Log = new StreamWriter(strPath + "\\" + DateTime.Now.ToString("yyyy_MM_dd") + ".txt", append: true))
                    {
                        System_Log.WriteLine(Data);
                        System_Log.Close();
                    }
                }
            }
            catch { }
        }
        #endregion

        #region Program Close
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show(Str.programexit, Str.exit, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                e.Cancel = true;
            else
                ProgramClose();
        }

        private void ProgramClose()
        {
            Hide();

            ini.WriteIniFile("function", "Value", _var._strAddfunction, _var._strConfigPath, "Config.ini");
            FileInfo file = new FileInfo(Application.StartupPath + "\\VisionSystem.exe");
            string strFileDate = "";
            if (file.Exists)
            {
                strFileDate = file.LastWriteTime.ToString("yyMMddHHmm");
            }

            op = null;
            op = new FluentSplashScreenOptions();
            op.Title = "";
            op.Subtitle = "";
            op.LeftFooter = this.Text;
            op.RightFooter = "Copyright © 2022 Vasim Inc.\nAll Rights reserved.";

            using (Font font = new Font("Tahoma", 10, FontStyle.Bold))
            {
                op.AppearanceLeftFooter.Font = font;
                op.AppearanceRightFooter.Font = font;
            }

            using (Font font = new Font("Tahoma", 30, FontStyle.Regular))
            {
                op.AppearanceTitle.Font = font;
            }

            op.AppearanceLeftFooter.ForeColor = Color.FromArgb(255, 255, 192);
            op.AppearanceRightFooter.ForeColor = Color.FromArgb(255, 255, 192);

            op.LoadingIndicatorType = FluentLoadingIndicatorType.Dots;
            op.Opacity = 130;
            op.OpacityColor = Color.Black;
            SplashScreenManager.ShowFluentSplashScreen(op, parentForm: this, useFadeIn: true, useFadeOut: true);
            Delay(100);

            _bReTryConn = false;
            _bChkDrive = false;
            _bTimeThread = false;

            LightOnOff(false);
            _serial.CloseComm();

            Application.ExitThread();

            op.Title = Str.camconndis;
            SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, op);
            if (!_var._bUsePrint)
            {
                for (int i = 0; i < _var._nScreenCnt; i++)
                    _CAM[i].CamDisconnect();
            }
            Delay(100);

            op.Title = Str.commdis;
            SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, op);
            if (_bCommConnect)
            {
                if (_plcParam.nCommType == (int)GlovalVar.PLCType.MX)
                    _mxPlc.Disconnect();
                else if (_plcParam.nCommType == (int)GlovalVar.PLCType.Simens)
                    _siemensPLC.Disconnect();
                else if (_plcParam.nCommType == (int)GlovalVar.PLCType.IO)
                    _ioBoard.Disconnect();
                else
                    _LSplc.Disconnect();
            }
            Delay(100);

            foreach (ICogFrameGrabber grabber in _mFrameGrabbers)
                grabber.Disconnect(false);

            foreach (Camera cam in _cameras)
                cam.Close();

            op.Title = Str.programclose;
            SplashScreenManager.Default.SendCommand(FluentSplashScreenCommand.UpdateOptions, op);
            Delay(500);



            SplashScreenManager.CloseForm(false);


            Environment.Exit(0);
        }
        #endregion

        #region Button Function
        private void btnSetCam_Click(object sender, EventArgs e)
        {
            if (flyCamCnt.IsPopupOpen)
                flyCamCnt.HideBeakForm();

            if (flyPass.IsPopupOpen)
            {
                flyPass.HideBeakForm();
            }
            else
            {
                txtPassword.Text = "";
                _nPassChkType = (int)GlovalVar.PasswordType.Admin;
                flyPass.OwnerControl = btnSetCam;
                flyPass.ShowBeakForm();

                txtPassword.Focus();
                _adminMode = GlovalVar.AdminMode.Camera;
            }
        }

        private void btnSetModel_Click(object sender, EventArgs e)
        {
            if (flyPass.IsPopupOpen)
            {
                flyPass.HideBeakForm();
            }
            else
            {
                txtPassword.Text = "";
                flyPass.OwnerControl = btnSetModel;
                flyPass.ShowBeakForm();
                _nPassChkType = (int)GlovalVar.PasswordType.All;

                txtPassword.Focus();
                _adminMode = GlovalVar.AdminMode.Model;
            }
        }

        private void bntSetComm_Click(object sender, EventArgs e)
        {
            if (flyPass.IsPopupOpen)
            {
                flyPass.HideBeakForm();
            }
            else
            {
                txtPassword.Text = "";
                flyPass.OwnerControl = bntSetComm;
                flyPass.ShowBeakForm();
                _nPassChkType = (int)GlovalVar.PasswordType.Admin;

                txtPassword.Focus();
                _adminMode = GlovalVar.AdminMode.Communication;
            }
        }
        private void btnImgpnlClose_Click(object sender, EventArgs e)
        {
            if (flyImageSet.IsPopupOpen)
                flyImageSet.HideBeakForm();
        }
        private void btnLog_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!flyLog.IsPopupOpen)
                flyLog.ShowPopup();
            else
                flyLog.HidePopup();
        }
        private void btnView_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
        }
        private void btnMenu_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!flyAdmin.IsPopupOpen)
                flyAdmin.ShowPopup();
            else
                flyAdmin.HidePopup();
        }
        private void bntRecord_Click(object sender, EventArgs e)
        {
            if (flyAdmin.IsPopupOpen)
                flyAdmin.HidePopup();

            Delay(500);

            splashScreenManager1.ShowWaitForm();

            if (_frmJobList != null)
            {
                _frmJobList.Dispose();
                _frmJobList = null;
            }

            if (!_var._bUsePrint)
            {
                _frmJobList = new frmJobList(this);
                _frmJobList.WindowState = FormWindowState.Maximized;
                _frmJobList.Show();
            }
            else
            {
                _frmPrintList = new frmPrintList(this);
                _frmPrintList.WindowState = FormWindowState.Maximized;
                _frmPrintList.Show();
            }

            Delay(100);
            splashScreenManager1.CloseWaitForm();
        }
        private void btnImgSet_Click(object sender, EventArgs e)
        {
            //OnTrigger(0);
            //return;

            if (!flyImageSet.IsPopupOpen)
                flyImageSet.ShowBeakForm();
            else
                flyImageSet.HideBeakForm();
        }
        private void btnConfig_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!flyPass.IsPopupOpen)
                flyPass.ShowPopup();
            else
                flyPass.HidePopup();
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            flyPass.HideBeakForm();
        }
        #endregion

        #region PLC 연결 상태 
        private void SetCamStatus(bool bStatus)
        {
            try
            {
                if (_plcParam.nCommType == (int)GlovalVar.PLCType.MX)
                {

                }
                else if (_plcParam.nCommType == (int)GlovalVar.PLCType.Simens)
                {

                }
                else if (_plcParam.nCommType == (int)GlovalVar.PLCType.IO)
                {

                }
                else
                {
                    if (_LSplc != null)
                        _LSplc._bCamStatus = bStatus;
                }
            }
            catch { }

        }
        private void OnConnect(string strMsg)
        {
            lblPLC.ItemAppearance.Normal.ForeColor = Color.Lime;
            AddMsg(strMsg, green, false, false, MsgType.Alarm);
        }
        private void OnDisconnect(string strMsg)
        {
            lblPLC.ItemAppearance.Normal.ForeColor = Color.Red;
            AddMsg(strMsg, red, false, false, MsgType.Alarm);
        }
        #endregion

        #region 트리거 신호
        private void OnTrigger(int nCameraNo)
        {
            int nCamNo = nCameraNo;

            try
            {
                // 시간 초기화
                if (_var._bUseSensor)
                {
                    _CAM[nCamNo]._dateTime = _time;
                    if (nCamNo != 1)
                    {
                        if (_CAM[nCamNo] != null)
                        {
                            _bInspRes[nCamNo] = false;
                            _CAM[nCamNo].CamInit(false);

                            _CAM[nCamNo].Grab(false);
                            //Thread threagTrigger = new Thread(() => _CAM[nCamNo].Grab(false));
                            //threagTrigger.Start();

                            AddMsg(string.Format("#{0} Camera Trigger On", nCamNo + 1), yellow, false, false, MsgType.Log);
                        }
                    }
                    if (nCamNo == 1)
                    {
                        Thread.Sleep(2000);
                        _Sensor.TriggerOn();
                        AddMsg(string.Format("#{0} Camera Sensor Trigger On", nCamNo + 1), yellow, false, false, MsgType.Log);
                        return;
                    }
                }
                else if (_var._bUsePrint)
                {
                    string[] strPrint_Data = new string[4];
                    strPrint_Data[0] = _PrintParam.iCount.ToString();
                    strPrint_Data[1] = _PrintParam.strHMCData;
                    strPrint_Data[2] = lblLotNo.Caption;
                    strPrint_Data[3] = _var._strPassData;
                    SetEVAutoPrint(strPrint_Data);
                    AddMsg(string.Format("#{0} Print Trigger On", nCamNo + 1), yellow, false, false, MsgType.Log);
                }
                else
                {
                    _CAM[nCamNo]._dateTime = _time;
                    if (_CAM[nCamNo] != null)
                    {

                        if (nCamNo == 0)
                        {

                            for (int i = 0; i < _var._nScreenCnt; i++)
                            {
                                _CAM[i].InitDisp(true);                        // 화면 초기화
                                _bResult[i] = false;
                                _bInspRes[i] = false;
                                _CAM[i]._dateTime = _time;
                            }
                        }

                        _bInspRes[nCamNo] = false;
                        _CAM[nCamNo].CamInit(false);
                        _CAM[nCamNo].Grab(false);

                        AddMsg(string.Format("#{0} Camera Trigger On", nCamNo + 1), yellow, false, false, MsgType.Log);
                    }
                }
            }
            catch (Exception ex)
            {
                AddMsg("OnTirgger Error : " + ex.ToString(), Color.Red, false, false, MsgType.Alarm);
            }
        }
        #endregion

        #region 데이터 Load
        public void OnModelChange(string strModelName)
        {

            string strModel = "";
            //if (strModelName.Length < 3)
            //{
            //    string[] strTempModel = strModelName.Split('_');
            //    var strTemp = strTempModel[0];
            //    int.TryParse(strTemp, out var nModelNo);
            //    strModel = String.Format("{0:D2}", nModelNo);
            //}
            //else
            //    strModel = strModelName;
            strModel = strModelName;
            try
            {
                if (tempModelData != strModel)
                {
                    if (_var._listModel.Count == 0)
                        AddMsg("등록된 모델이 없습니다.", red, false, false, MsgType.Log);

                    for (int i = 0; i < _var._listModel.Count; i++)
                    {
                        if (_var._listModel[i] == strModel)
                        {
                            _var._strModelNo = _var._listModel[i];
                            Invoke(new EventHandler(delegate
                            {
                                lblModel.Caption = _var._listModel[i];
                                _var._strModelName = _var._listModel[i];
                                LoadRecipe();
                            }));
                            if (!_var._bUsePrint)
                            {
                                for (int j = 0; j < _var._nScreenCnt; j++)
                                {
                                    _CAM[j]._var = _var;
                                    _CAM[j]._modelParam = _ModelParam[j];
                                    _CAM[j].ModelChange();
                                    _CAM[j]._bPassMode = false;
                                }

                                ini.WriteIniFile("ModelNo", "Value", _var._strModelNo, _var._strConfigPath, "Config.ini");
                            }
                            else
                            {
                                _PrintControl._PrintParam = _PrintParam;
                            }
                        }
                    }
                    tempModelData = strModel;
                }

            }
            catch (Exception ex)
            {
                AddMsg("모델 변경 오류 : " + ex.Message, red, false, false, MsgType.Alarm);
            }
        }
        private void OnPassDataConfirm(string strData)
        {
            Invoke(new EventHandler(delegate
            {
                _var._strPassData = strData;
            }));
        }
        private void OnTimeCheck()
        {
            Invoke(new EventHandler(delegate
            {
                _time = DateTime.Now;
            }));
        }
        private void OnLotIDReceive(string strLotNo)
        {
            Invoke(new EventHandler(delegate
            {
                lblLotNo.Caption = strLotNo;
                _var._strLotNo = strLotNo;
            }));


            for (int i = 0; i < _var._nScreenCnt; i++)
            {
                if (!_var._bUsePrint)
                    _CAM[i]._var = _var;
                //else
                //    _PrintControl._var = _var;
            }

            ini.WriteIniFile("LotNo", "Value", _var._strLotNo, _var._strConfigPath, "Config.ini");
            AddMsg(string.Format("Lot No : {0}", strLotNo), yellow, false, false, MsgType.Log);
        }
        #endregion

        #region 통신 데이터 
        private void OnRecvData(string strMsg, byte[] bytes, ushort[] usData)
        {
            _strRecvData = strMsg;

            _Readbytes = null;
            _Readbytes = bytes;

            _ReadShort = null;
            _ReadShort = usData;
        }
        #endregion 

        private void Reset()
        {
            for (int i = 0; i < _CAM.Length; i++)
            {
                if (_CAM[i] != null)
                    _CAM[i].ResetBackColor();
            }
        }

        #region Password Function
        private void btnChkPass_Click(object sender, EventArgs e)
        {
            ChkPass();
        }
        private void ChkPass()
        {
            if (_nPassChkType == (int)GlovalVar.PasswordType.Admin)
            {
                if (txtPassword.Text != _var._strAdminPW)
                {
                    MessageBox.Show(Str.PWnotMatch, "Not Match", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Text = "";
                    return;
                }
            }
            else if (_nPassChkType == (int)GlovalVar.PasswordType.Operator)
            {
                if (txtPassword.Text != _var._strOPPW)
                {
                    MessageBox.Show(Str.PWnotMatch, "Not Match", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Text = "";
                    return;
                }
            }
            else
            {
                if ((txtPassword.Text != _var._strOPPW) && (txtPassword.Text != _var._strAdminPW))
                {
                    MessageBox.Show(Str.PWnotMatch, "Not Match", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Text = "";
                    return;
                }
            }


            ChkPassNext();
        }
        private void ChkPassNext()
        {
            if (_adminMode == GlovalVar.AdminMode.Model)
            {
                flyPass.HideBeakForm();
                flyAdmin.HidePopup();
                Delay(500);

                splashScreenManager1.ShowWaitForm();

                if (_frmModel != null)
                {
                    _frmModel.Dispose();
                    _frmModel = null;
                }

                _frmModel = new frmModel(this);
                _frmModel.Show();

                Delay(200);
                splashScreenManager1.CloseWaitForm();

            }
            else if (_adminMode == GlovalVar.AdminMode.Communication)
            {
                flyPass.HideBeakForm();
                flyAdmin.HidePopup();
                Delay(500);

                splashScreenManager1.ShowWaitForm();

                if (_frmComm != null)
                {
                    _frmComm.Dispose();
                    _frmComm = null;
                }

                _frmComm = new frmComm(this);
                _frmComm.Show();

                splashScreenManager1.CloseWaitForm();
            }
            else
            {
                flyPass.HideBeakForm();

                if (!flyCamCnt.IsPopupOpen)
                {
                    txtCamCnt.Text = "";
                    flyCamCnt.ShowBeakForm();
                    txtCamCnt.Focus();
                }
            }
        }
        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                ChkPass();
        }
        private void btnPasswordChange_Click(object sender, EventArgs e)
        {
            txtCurrentPW.Text = "";
            txtChangePW1.Text = "";
            txtChangePW2.Text = "";

            lblMatch1.Visible = false;
            lblMatch2.Visible = false;

            if (!flyChangePW.IsPopupOpen)
            {
                txtCurrentPW.Focus();
                flyChangePW.ShowPopup();
            }
            else
                flyChangePW.HidePopup();
        }
        private void btnPWClose_Click(object sender, EventArgs e)
        {
            if (flyChangePW.IsPopupOpen)
                flyChangePW.HidePopup();
        }
        private void btnChange_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Str.changePWmsg, Str.changePW, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            string strPw = (_nPassChkType == (int)GlovalVar.PasswordType.Admin) ? _var._strAdminPW : _var._strOPPW;

            if (txtCurrentPW.Text == strPw)
            {
                if (txtChangePW1.Text == txtChangePW2.Text)
                {
                    if (_nPassChkType == (int)GlovalVar.PasswordType.Admin)
                    {
                        _var._strAdminPW = txtChangePW1.Text;
                        ini.WriteIniFile("AdminPW", "Value", _var._strAdminPW, "C:\\ProgramData\\PassWord", "AdminPW.pwd");
                        AddMsg("Admin Password Changed", white, true, false, MsgType.Alarm);
                    }
                    else
                    {
                        _var._strOPPW = txtChangePW1.Text;
                        ini.WriteIniFile("OperatorPW", "Value", _var._strOPPW, "C:\\ProgramData\\PassWord", "OperatorPW.pwd");
                        AddMsg("Operator Password Changed", white, true, false, MsgType.Alarm);
                    }
                }
                else
                    MessageBox.Show(Str.PWnotMatch, "Not Match", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
                MessageBox.Show(Str.PWnotMatch, "Not Match", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void txtChangePW1_TextChanged(object sender, EventArgs e)
        {
            if (txtChangePW1.Text == "" || txtChangePW2.Text == "")
            {
                lblMatch2.Visible = false;
                return;
            }

            if (txtChangePW1.Text == txtChangePW2.Text)
            {
                lblMatch2.Text = "Match";
                lblMatch2.ForeColor = Color.Lime;
            }
            else
            {
                lblMatch2.Text = "Not Match";
                lblMatch2.ForeColor = Color.Red;
            }

            lblMatch2.Visible = true;
        }
        private void txtCurrentPW_TextChanged(object sender, EventArgs e)
        {
            if (txtCurrentPW.Text == "")
            {
                lblMatch1.Visible = false;
                return;
            }

            string strPW = (_nPassChkType == (int)GlovalVar.PasswordType.Admin) ? _var._strAdminPW : _var._strOPPW;

            if (txtCurrentPW.Text == strPW)
            {
                lblMatch1.BackColor = Color.Lime;
                lblMatch1.Text = "Match";
            }
            else
            {
                lblMatch1.BackColor = Color.Red;
                lblMatch1.Text = "Not Match";
            }

            lblMatch1.Visible = true;
        }
        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))    //숫자와 백스페이스를 제외한 나머지를 바로 처리
                e.Handled = true;
        }
        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            if (txtPassword.Text == "")
            {
                lblPassInput.Visible = false;
                return;
            }
            bool bMatch = false;

            if (_nPassChkType == (int)GlovalVar.PasswordType.Admin)
            {
                if (txtPassword.Text == _var._strAdminPW)
                    bMatch = true;
            }
            else if (_nPassChkType == (int)GlovalVar.PasswordType.Operator)
            {
                if (txtPassword.Text == _var._strOPPW)
                    bMatch = true;
            }
            else
            {
                if ((txtPassword.Text == _var._strOPPW) || (txtPassword.Text == _var._strAdminPW))
                    bMatch = true;
            }

            if (bMatch)
            {
                lblPassInput.ForeColor = Color.Lime;
                lblPassInput.Text = "Match";
            }
            else
            {
                lblPassInput.ForeColor = Color.Red;
                lblPassInput.Text = "Not Match";
            }

            lblPassInput.Visible = true;
        }
        #endregion

        #region 화면 정렬 및 추가 
        private void btnAlign_Click(object sender, EventArgs e)
        {
            if (!flyAlign.IsPopupOpen)
                flyAlign.ShowBeakForm();
            else
                flyAlign.HideBeakForm();
        }
        private void btnAlignClose_Click(object sender, EventArgs e)
        {
            if (flyAdmin.IsPopupOpen)
                flyAdmin.HidePopup();
        }
        private void btnApply_Click(object sender, EventArgs e)
        {
            if (txtWidth.Text == "" || txtHeight.Text == "")
            {
                MessageBox.Show(Str.enterquantity, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _var._strWidth = txtWidth.Text;
            _var._strHeight = txtHeight.Text;

            ini.WriteIniFile("WidthCnt", "Value", txtWidth.Text, _var._strConfigPath, "Config.ini");
            ini.WriteIniFile("HeightCnt", "Value", txtHeight.Text, _var._strConfigPath, "Config.ini");

            if (flyAdmin.IsPopupOpen)
                flyAdmin.HidePopup();

            ScreenAlign();
        }
        private void ScreenAlign()
        {
            try
            {
                _var._nWidth = this.ClientRectangle.Size.Width - 5;
                _var._nHeight = this.ClientRectangle.Size.Height - 90;

                if (_var._nScreenCnt > 0)
                {
                    if (_var._nScreenCnt == 1)
                        _frmMDI[0].WindowState = FormWindowState.Maximized;
                    else
                    {

                        if (string.IsNullOrEmpty(_var._strWidth) || string.IsNullOrEmpty(_var._strHeight))
                            return;

                        for (var i = 0; i < _var._nScreenCnt; i++)
                            _frmMDI[i].WindowState = FormWindowState.Normal;

                        int.TryParse(_var._strWidth, out int nWidth);
                        int.TryParse(_var._strHeight, out int nHeight);
                        int nCol = 0, nRow = 0;

                        int nFormWidth = (_var._nWidth / nWidth);
                        int nFormHeight = (_var._nHeight / nHeight);

                        for (int i = 0; i < _var._nScreenCnt; i++)
                        {
                            _frmMDI[i].Width = nFormWidth;
                            _frmMDI[i].Height = nFormHeight;

                            if (nCol == 0)
                                _frmMDI[i].Location = new Point(nCol * (nFormWidth + 5), nRow * nFormHeight);
                            else
                                _frmMDI[i].Location = new Point((nCol * nFormWidth), nRow * nFormHeight);

                            if (nCol >= nWidth - 1)
                            {
                                nCol = 0;
                                nRow++;
                            }
                            else
                                nCol++;
                        }
                        if (_var._bUsePinchange)
                        {
                            _frmMDI[_var._nScreenCnt].Width = nFormWidth;
                            _frmMDI[_var._nScreenCnt].Height = nFormHeight;

                            if (nCol == 0)
                                _frmMDI[_var._nScreenCnt].Location = new Point(nCol * (nFormWidth + 5), nRow * nFormHeight);
                            else
                                _frmMDI[_var._nScreenCnt].Location = new Point((nCol * nFormWidth), nRow * nFormHeight);

                            if (nCol >= nWidth - 1)
                            {
                                nCol = 0;
                                nRow++;
                            }
                            else
                                nCol++;
                        }
                    }
                }
            }
            catch { }

        }
        public void OnPositionChange(int nIdx, GlovalVar.PositionType type)
        {
            if (_var._nScreenCnt < 2)
                return;

            try
            {
                Point point = new Point();

                if (type == GlovalVar.PositionType.Next)
                {
                    point.X = _frmMDI[0].Location.X;
                    point.Y = _frmMDI[0].Location.Y;
                }
                else
                {

                }

                if (nIdx == _var._nScreenCnt - 1)
                {
                    point.X = _frmMDI[0].Location.X;
                    point.Y = _frmMDI[0].Location.Y;

                    _frmMDI[0].Location = new Point(_frmMDI[nIdx].Location.X, _frmMDI[nIdx].Location.Y);
                    _frmMDI[nIdx].Location = point;
                }
                else
                {
                    if (type == GlovalVar.PositionType.Next)
                    {
                        if (nIdx + 1 == _var._nScreenCnt)
                        {
                            point.X = _frmMDI[0].Location.X;
                            point.Y = _frmMDI[0].Location.Y;

                            _frmMDI[0].Location = new Point(_frmMDI[nIdx].Location.X, _frmMDI[nIdx].Location.Y);
                            _frmMDI[nIdx].Location = point;
                        }
                        else
                        {
                            point.X = _frmMDI[nIdx + 1].Location.X;
                            point.Y = _frmMDI[nIdx + 1].Location.Y;

                            _frmMDI[nIdx + 1].Location = new Point(_frmMDI[nIdx].Location.X, _frmMDI[nIdx].Location.Y);
                            _frmMDI[nIdx].Location = point;
                        }
                    }
                    else
                    {
                        if (nIdx == 0)
                        {
                            point.X = _frmMDI[_var._nScreenCnt - 1].Location.X;
                            point.Y = _frmMDI[_var._nScreenCnt - 1].Location.Y;

                            _frmMDI[_var._nScreenCnt - 1].Location = new Point(_frmMDI[nIdx].Location.X, _frmMDI[nIdx].Location.Y);
                            _frmMDI[nIdx].Location = point;
                        }
                        else
                        {
                            point.X = _frmMDI[nIdx - 1].Location.X;
                            point.Y = _frmMDI[nIdx - 1].Location.Y;

                            _frmMDI[nIdx - 1].Location = new Point(_frmMDI[nIdx].Location.X, _frmMDI[nIdx].Location.Y);
                            _frmMDI[nIdx].Location = point;
                        }

                    }
                }
            }
            catch { }
        }
        private void btnCamClose_Click(object sender, EventArgs e)
        {
            if (flyCamCnt.IsPopupOpen)
                flyAdmin.HideBeakForm();

            Delay(200);

            if (flyAdmin.IsPopupOpen)
                flyAdmin.HidePopup();
        }
        private void btnCamAdd_Click(object sender, EventArgs e)
        {
            int.TryParse(txtCamCnt.Text, out int nCnt);

            if (nCnt == 0)
            {
                MessageBox.Show(Str.entercamcnt, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                int nScreenCnt = _var._nScreenCnt;
                if (!_var._bUsePrint)
                {
                    for (int i = nScreenCnt; i < nScreenCnt + nCnt; i++)
                    {
                        if (_frmMDI[i] != null)
                        {
                            _frmMDI[i].Dispose();
                            _frmMDI[i] = null;
                        }

                        _frmMDI[i] = new frmMDI(this, i);

                        if (_CAM[i] != null)
                        {
                            _CAM[i].Dispose();
                            _CAM[i] = null;
                        }

                        _CAM[i] = new CAM();
                        _CAM[i]._CamSetFunc = CamSetFunc;
                        _CAM[i]._OnGrabComplete = CamGrabComplete;
                        _CAM[i]._OnInspComplete = CamInspComplete;
                        _CAM[i]._OnMessage = Cam_OnMessage;
                        _CAM[i]._OnPositionChange = OnPositionChange;
                        _CAM[i]._OnResultImageList = OnResultImageList;
                        //_CAM[i]._OnSensorTrigger = OnSensorTrigger();
                        _CAM[i]._OnSensorTrigger = OnSensorTrigger;
                        _CAM[i]._OnSensorModelChange = OnSensorModelChange;

                        _CAM[i]._var = _var;
                        _CAM[i]._var._bOriginImageSave = _var._bOriginImageSave;
                        _CAM[i]._var._bResultImageSave = _var._bResultImageSave;
                        _CAM[i]._var._bAutoImageDelete = _var._bAutoImageDelete;
                        _CAM[i]._var._nOriginImageFormat = _var._nOriginImageFormat;
                        _CAM[i]._var._nResultImageFormat = _var._nResultImageFormat;
                        _CAM[i]._var._strSaveImagePath = _var._strSaveImagePath;
                        _CAM[i]._modelParam = _ModelParam[i];

                        _CAM[i]._nIdx = i;
                        _CAM[i]._var = _var;

                        _CAM[i].LoadSet();

                        _frmMDI[i].Controls.Add(_CAM[i]);
                        _CAM[i].Dock = DockStyle.Fill;
                        _CAM[i]._modelParam = _ModelParam[i];
                        _frmMDI[i].Show();
                        _var._nScreenCnt++;



                        ini.WriteIniFile("ScreenCnt", "Value", _var._nScreenCnt.ToString(), _var._strConfigPath, "Config.ini");
                    }
                }

                //if (_mxPlc)
                if (_LSplc != null)
                    _LSplc._nScreenCnt = _var._nScreenCnt;

                _bResult = null;
                _bInspRes = null;
                _bResult = new bool[_var._nScreenCnt];
                _bInspRes = new bool[_var._nScreenCnt];

                for (int i = 0; i < _var._nScreenCnt; i++)
                {
                    _bResult[i] = false;
                    _bInspRes[i] = false;
                }

                if (flyCamCnt.IsPopupOpen)
                    flyCamCnt.HideBeakForm();

                if (flyAdmin.IsPopupOpen)
                    flyAdmin.HidePopup();

            }
            catch (Exception ex)
            {
                AddMsg("Add Camera Error : " + ex.ToString(), red, false, false, MsgType.Alarm);
            }
        }
        private void txtCamCnt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnCamAdd.PerformClick();
        }
        #endregion

        #region Image 저장 
        private void btnSaveImagePath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                txtSaveImagePath.Text = dialog.SelectedPath;
        }
        private void radOriginBMP_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
                (sender as RadioButton).ForeColor = Color.Yellow;
            else
                (sender as RadioButton).ForeColor = Color.White;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                _var._bOriginImageSave = swOriginSave.IsOn;
                _var._bResultImageSave = swResultSave.IsOn;

                if (radOriginBMP.Checked)
                    _var._nOriginImageFormat = 0;
                else
                    _var._nOriginImageFormat = 1;

                if (radResultBMP.Checked)
                    _var._nResultImageFormat = 0;
                else
                    _var._nResultImageFormat = 1;


                _var._strSaveImagePath = txtSaveImagePath.Text;
                _var._strSaveResultImagePath = txtSaveResultImagePath.Text;
                int.TryParse(txtOriginalDiskDelete.Text, out _var._nDiskOriginalDelete);
                int.TryParse(txtResultDiskDelete.Text, out _var._nDiskResultDelete);
                int.TryParse(txtAutoDiskDelete.Text, out _var._nDiskAutoDelete);
                _var._tsReset = ResetTimer.Time.ToString("HH:mm:ss");
                _var._AutoDeleteTime1 = AutoDeleteTimer1.Time.ToString("HH:mm:ss");
                _var._AutoDeleteTime2 = AutoDeleteTimer2.Time.ToString("HH:mm:ss");

                ini.WriteIniFile("OriginImageSave", "Value", _var._bOriginImageSave.ToString(), _var._strConfigPath, "Config.ini");
                ini.WriteIniFile("ResultImageSave", "Value", _var._bResultImageSave.ToString(), _var._strConfigPath, "Config.ini");
                ini.WriteIniFile("AutoImageDelete", "Value", _var._bAutoImageDelete.ToString(), _var._strConfigPath, "Config.ini");
                ini.WriteIniFile("OriginImageFormat", "Value", _var._nOriginImageFormat.ToString(), _var._strConfigPath, "Config.ini");
                ini.WriteIniFile("ResultImageFormat", "Value", _var._nResultImageFormat.ToString(), _var._strConfigPath, "Config.ini");
                ini.WriteIniFile("OriginalDelete", "Value", _var._nDiskOriginalDelete.ToString(), _var._strConfigPath, "Config.ini");
                ini.WriteIniFile("ResultDelete", "Value", _var._nDiskResultDelete.ToString(), _var._strConfigPath, "Config.ini");
                ini.WriteIniFile("AutoDelete", "Value", _var._nDiskAutoDelete.ToString(), _var._strConfigPath, "Config.ini");
                ini.WriteIniFile("SaveImagePath", "Value", _var._strSaveImagePath, _var._strConfigPath, "Config.ini");
                ini.WriteIniFile("SaveResultImagePath", "Value", _var._strSaveResultImagePath, _var._strConfigPath, "Config.ini");
                ini.WriteIniFile("ResetTime", "Value", _var._tsReset, _var._strConfigPath, "Config.ini");
                ini.WriteIniFile("AutoDeleteTime1", "Value", _var._AutoDeleteTime1, _var._strConfigPath, "Config.ini");
                ini.WriteIniFile("AutoDeleteTime2", "Value", _var._AutoDeleteTime2, _var._strConfigPath, "Config.ini");

                for (int i = 0; i < _var._nScreenCnt; i++)
                {
                    if (_CAM[i] != null)
                    {
                        _CAM[i]._var._bOriginImageSave = _var._bOriginImageSave;
                        _CAM[i]._var._bResultImageSave = _var._bResultImageSave;
                        _CAM[i]._var._bAutoImageDelete = _var._bAutoImageDelete;
                        _CAM[i]._var._nOriginImageFormat = _var._nOriginImageFormat;
                        _CAM[i]._var._nResultImageFormat = _var._nResultImageFormat;
                        _CAM[i]._var._nDiskOriginalDelete = _var._nDiskOriginalDelete;
                        _CAM[i]._var._strSaveImagePath = _var._strSaveImagePath;
                    }
                }

                AddMsg(Str.imgParamsave, green, false, false, MsgType.Alarm);
                MessageBox.Show(Str.imgParamsave);
            }
            catch (Exception ex)
            {
                AddMsg(Str.imgparamnotsave + ex.Message, red, true, true, MsgType.Alarm);
            }
        }
        #endregion

        #region 데이터 Clear
        private void btnReset_Click(object sender, EventArgs e)
        {
            if (flyAdmin.IsPopupOpen)
                flyAdmin.HidePopup();

            Delay(100);
            if (!_var._bUsePrint)
            {
                for (int i = 0; i < _var._nScreenCnt; i++)
                {
                    _CAM[i].CamInit(true);
                    _CAM[i].ResetBackColor();
                }
            }
            lblTotalCnt.Caption = "0";
            lblOKCnt.Caption = "0";
            lblNGCnt.Caption = "0";

            _var._nTotalCnt = 0;
            _var._nOKCnt = 0;
            _var._nNGCnt = 0;

            if (_bInspRes != null)
            {
                for (var i = 0; i < _var._nScreenCnt; i++)
                    _bInspRes[i] = false;
            }

            SaveCount();
        }
        #endregion 

        #region 언어
        public void ChangeLanguage()
        {
            if (_var._Language == GlovalVar.Language.Korean)
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ko-KR");
                SetTextLanguage();
            }
            else
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                SetTextLanguage();
            }
        }

        private void SetTextLanguage()
        {
            lblModelTitle.Caption = Str.Model;
            lblLotTitle.Caption = Str.LotNo;
            btnAdmin.Caption = Str.Menu;
            btnLog.Caption = Str.Log;
            btnReset.Text = Str.Reset;
            btnAlign.Text = Str.Align;
            bntRecord.Text = Str.Record;
            btnSetCam.Text = Str.AddCamera;
            btnSetModel.Text = Str.ModelSetting;
            bntSetComm.Text = Str.CommunicationSetting;

            lblWidth.Text = Str.Width;
            lblHeight.Text = Str.Height;
            btnApply.Text = Str.Apply;
            btnAlignClose.Text = Str.Close;
            btnChange.Text = Str.Change;
            btnPWClose.Text = Str.Close;
            btnChkPass.Text = Str.Check;
            btnClose.Text = Str.Close;
            btnPasswordChange.Text = Str.PasswordChange;
            lblPW.Text = Str.Password;
            lblCamCnt.Text = Str.CameraCount;
            btnCamAdd.Text = Str.Add;
            btnImgSet.Text = Str.SaveImgSet;
            btnCamClose.Text = Str.Close;
            btnSave.Text = Str.Save;
            lblCurrentPW.Text = Str.Currentpassword;
            lblEnterPW.Text = Str.EnterPassword;
            lblReenterPW.Text = Str.ReenterPasswrod;

            lblImgPath.Text = Str.ImageSavePath;
            lblOriginImgsave.Text = Str.OriginImgSave;
            lblOriginImgformat.Text = Str.OriginalImageSaveFormat;
            lblresultImgsave.Text = Str.ResultImageSave;
            lblresultImgformat.Text = Str.ResultImageSaveFormat;

            waitForm.progressPanel1.Caption = Str.WaitMsg;
            waitForm.progressPanel1.Description = Str.Loading;
        }
        #endregion

        private void btnBypass_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string strMsg = "";
            if (!_bPassmode)
                strMsg = "ByPass 모드로 설정 하시겠습니까?";
            else
                strMsg = "ByPass 모드를 하제 하시겠습니까?";

            if (MessageBox.Show(strMsg, "ByPass", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            _bPassmode = !_bPassmode;
            if (_bPassmode)
            {
                btnBypass.ItemAppearance.Normal.BackColor = Color.FromArgb(255, 255, 128);
                btnBypass.ItemAppearance.Normal.ForeColor = Color.Black;
                AddMsg("패스 모드가 설정 되었습니다.", white, false, false, MsgType.Alarm);
            }
            else
            {
                btnBypass.ItemAppearance.Normal.BackColor = Color.FromArgb(50, 50, 50);
                btnBypass.ItemAppearance.Normal.ForeColor = Color.White;
                AddMsg("패스 모드가 해제 되었습니다.", white, false, false, MsgType.Alarm);
            }
        }
        private void frmMain_Resize(object sender, EventArgs e)
        {
            ScreenAlign();
        }

        #region Image 20 History
        private void cogDisplay1_DoubleClick(object sender, EventArgs e)
        {
            if (_nImgListCamNo == -1)
                return;

            try
            {
                int.TryParse((sender as CogDisplay).Tag.ToString(), out _nDispNo);

                for (var i = 0; i < 20; i++)
                    _Pnl[i].Invalidate();

                cogDispOrigin.AutoFit = true;
                cogDispResult.AutoFit = true;

                if (_CAM[_nImgListCamNo].GetOriginImg != null)
                    cogDispOrigin.Image = _CAM[_nImgListCamNo].GetOriginImg[_nDispNo];

                if (_CAM[_nImgListCamNo].GetResultImg != null)
                    cogDispResult.Image = _CAM[_nImgListCamNo].GetResultImg[_nDispNo];

                if (cogDispOrigin.Image == null && cogDispResult.Image == null)
                    return;

                if (!flyResImg.IsPopupOpen)
                    flyResImg.ShowBeakForm();
            }
            catch { }
        }
        private void Pnl1_Paint(object sender, PaintEventArgs e)
        {
            if (_nDispNo == -1)
                return;

            if (_cogDisp[_nDispNo].Image == null)
                return;

            Invoke(new EventHandler(delegate
            {
                base.OnPaint(e);
                int borderWidth = 2;
                Color borderColor = Color.Lime;

                if ((sender as Panel).Tag.ToString() != _nDispNo.ToString())
                {
                    borderWidth = 1;
                    borderColor = Color.Black;
                }

                ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle, borderColor, borderWidth, ButtonBorderStyle.Solid, borderColor, borderWidth,
                ButtonBorderStyle.Solid, borderColor, borderWidth, ButtonBorderStyle.Solid,
                borderColor, borderWidth, ButtonBorderStyle.Solid);
            }));

        }
        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (_nImgListCamNo == -1)
                return;

            try
            {
                if (_nDispNo == 0)
                    _nDispNo = 19;
                else
                    _nDispNo--;


                for (var i = 0; i < 20; i++)
                    _Pnl[i].Invalidate();

                cogDispOrigin.AutoFit = true;
                cogDispResult.AutoFit = true;

                if (_CAM[_nImgListCamNo].GetOriginImg != null)
                    cogDispOrigin.Image = _CAM[_nImgListCamNo].GetOriginImg[_nDispNo];

                if (_CAM[_nImgListCamNo].GetResultImg != null)
                    cogDispResult.Image = _CAM[_nImgListCamNo].GetResultImg[_nDispNo];
            }
            catch { }
        }
        private void btnNext_Click(object sender, EventArgs e)
        {
            if (_nImgListCamNo == -1)
                return;

            try
            {
                if (_nDispNo == 19)
                    _nDispNo = 0;
                else
                    _nDispNo++;


                for (var i = 0; i < 20; i++)
                    _Pnl[i].Invalidate();

                cogDispOrigin.AutoFit = true;
                cogDispResult.AutoFit = true;

                if (_CAM[_nImgListCamNo].GetOriginImg != null)
                    cogDispOrigin.Image = _CAM[_nImgListCamNo].GetOriginImg[_nDispNo];

                if (_CAM[_nImgListCamNo].GetResultImg != null)
                    cogDispResult.Image = _CAM[_nImgListCamNo].GetResultImg[_nDispNo];
            }
            catch { }
        }
        private void btnImgListClose_Click(object sender, EventArgs e)
        {
            _bImgList = false;
            _nImgListCamNo = -1;
            flyImgList.HidePopup();
        }
        private void btnListClose_Click(object sender, EventArgs e)
        {
            flyResImg.HideBeakForm();
        }
        #endregion

        #region Robot
        private void btnRobotClose_Click(object sender, EventArgs e)
        {
            flyRobot.HideBeakForm();
        }

        private void btnRobot_Click(object sender, EventArgs e)
        {
            if (_var._bUseSensor)
            {
                if (!flyKeyence.IsPopupOpen)
                    flyKeyence.ShowBeakForm();
                else
                    flyKeyence.HideBeakForm();
            }
            else if (_var._bUsePrint)
            {
                if (!flyPrint.IsPopupOpen)
                    flyPrint.ShowBeakForm();
                else
                    flyPrint.HideBeakForm();
            }

            //if (flyRobot.IsPopupOpen)
            //    flyRobot.HideBeakForm();
            //else
            //{
            //    txtData.Text = "";
            //    flyRobot.ShowBeakForm();
            //}
        }

        private void btnRobotSave_Click(object sender, EventArgs e)
        {
            _robotParam.strIP = txtRobotIP.Text;
            _robotParam.strPort = txtRobotPort.Text;
            _robotParam.strTrigger1 = txtTrigger1.Text;
            _robotParam.strTrigger2 = txtTrigger2.Text;

            _LightParam.strPortName = cbLightPort.EditValue.ToString();
            _LightParam.strBaudrate = cbLightBaud.EditValue.ToString();
            if (cbLightChannel.SelectedIndex == 0)
            {
                _LightParam.strChannel1 = string.Format("{0:D2}", cbLightChannel.SelectedIndex + 1);
                _LightParam.strLightValue1 = string.Format("{0:D3}", barLight.Value);
            }
            else
            {
                _LightParam.strChannel2 = string.Format("{0:D2}", cbLightChannel.SelectedIndex + 1);
                _LightParam.strLightValue2 = string.Format("{0:D3}", barLight.Value);
            }

            ini.WriteIniFile("RobotIP", "Value", _robotParam.strIP, _var._strConfigPath, "Config.ini");
            ini.WriteIniFile("RobotPort", "Value", _robotParam.strPort, _var._strConfigPath, "Config.ini");
            ini.WriteIniFile("RobotTrigger1", "Value", _robotParam.strTrigger1, _var._strConfigPath, "Config.ini");
            ini.WriteIniFile("RobotTrigger2", "Value", _robotParam.strTrigger2, _var._strConfigPath, "Config.ini");

            ini.WriteIniFile("LightPort", "Value", _LightParam.strPortName, _var._strConfigPath, "Config.ini");
            ini.WriteIniFile("LightBaudrate", "Value", _LightParam.strBaudrate, _var._strConfigPath, "Config.ini");
            ini.WriteIniFile("LightChannel1", "Value", _LightParam.strChannel1, _var._strConfigPath, "Config.ini");
            ini.WriteIniFile("LightValue1", "Value", _LightParam.strLightValue1, _var._strConfigPath, "Config.ini");
            ini.WriteIniFile("LightChannel2", "Value", _LightParam.strChannel2, _var._strConfigPath, "Config.ini");
            ini.WriteIniFile("LightValue2", "Value", _LightParam.strLightValue2, _var._strConfigPath, "Config.ini");

            AddMsg("로봇 및 조명 설정 변경", white, false, false, MsgType.Alarm);
        }

        private void btnRobotConn_Click(object sender, EventArgs e)
        {
            _robotParam.strIP = txtRobotIP.Text;
            _robotParam.strPort = txtRobotPort.Text;

            RobotConnecting();
        }

        private void btnDataSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtData.Text))
            {
                MessageBox.Show("전송할 데이터를 입력하여 주십시오");
                return;
            }

            RobotDataSend(txtData.Text);
            //txtData.Text = "";
        }

        private void RobotDataSend(string strData)
        {
            if (_bRobot)
                _RobotSned.SendMsg(strData + "\x0A" + "\x0D");
            else
                AddMsg("Robot Data Send Error :  Robot Disconnected", red, false, false, MsgType.Log);
        }
        #endregion

        #region Light
        private void btnLightConn_Click(object sender, EventArgs e)
        {
            _LightParam.strPortName = cbLightPort.EditValue.ToString();
            _LightParam.strBaudrate = cbLightBaud.EditValue.ToString();

            LightConnecting();
        }
        private void barLight_EditValueChanged(object sender, EventArgs e)
        {

            txtLightValue.Text = barLight.Value.ToString();

            if (cbLightChannel.SelectedIndex == 0)
                _LightParam.strLightValue1 = string.Format("{0:D3}", barLight.Value);
            else
                _LightParam.strLightValue2 = string.Format("{0:D3}", barLight.Value);
            SetLightValue();
        }
        private void LightOnOff(bool bOn)
        {
            if (!_bLight)
                return;

            try
            {
                if (string.IsNullOrEmpty(_LightParam.strChannel1) || string.IsNullOrEmpty(_LightParam.strChannel2))
                {
                    AddMsg("채널을 선택하여 주십시오", red, false, false, MsgType.Alarm);
                    return;
                }

                var strStatus = bOn ? "1" : "0";
                var strMsg = bOn ? "조명 ON" : "조명 OFF";
                _serial.Send(string.Format("]{0}{1}", _LightParam.strChannel1, strStatus));
                _serial.Send(string.Format("]{0}{1}", _LightParam.strChannel2, strStatus));

                AddMsg(strMsg, white, false, false, MsgType.Log);
            }
            catch (Exception ex)
            {
                AddMsg("조명값 변경 오류 : " + ex.ToString(), red, false, false, MsgType.Alarm);
            }
        }
        private void SetLightValue()
        {
            if (!_bLight)
                return;

            try
            {
                if (string.IsNullOrEmpty(_LightParam.strChannel1) && string.IsNullOrEmpty(_LightParam.strLightValue1))
                {
                    if (string.IsNullOrEmpty(_LightParam.strChannel1))
                    {
                        AddMsg("채널을 선택하여 주십시오", red, false, false, MsgType.Alarm);
                        return;
                    }

                    if (string.IsNullOrEmpty(_LightParam.strLightValue1))
                    {
                        AddMsg("조명값을 입력하여 주십시오.", red, false, false, MsgType.Alarm);
                        return;
                    }
                }

                _serial.Send(string.Format("[{0}{1}", _LightParam.strChannel1, _LightParam.strLightValue1));
                //AddMsg("조명값 변경 : " + _LightParam.strLightValue1, white, false, false, MsgType.Alarm);

                if (string.IsNullOrEmpty(_LightParam.strChannel2) && string.IsNullOrEmpty(_LightParam.strLightValue2))
                {
                    if (string.IsNullOrEmpty(_LightParam.strChannel2))
                    {
                        AddMsg("채널을 선택하여 주십시오", red, false, false, MsgType.Alarm);
                        return;
                    }

                    if (string.IsNullOrEmpty(_LightParam.strLightValue2))
                    {
                        AddMsg("조명값을 입력하여 주십시오.", red, false, false, MsgType.Alarm);
                        return;
                    }
                }

                _serial.Send(string.Format("[{0}{1}", _LightParam.strChannel2, _LightParam.strLightValue2));
                //AddMsg("조명값 변경 : " + _LightParam.strLightValue2, white, false, false, MsgType.Alarm);
            }
            catch (Exception ex)
            {
                AddMsg("조명값 변경 오류 : " + ex.ToString(), red, false, false, MsgType.Alarm);
            }
        }
        private void txtLightValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtLightValue.Text))
            {
                MessageBox.Show("조명값을 입력하여 주십시오.", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                int.TryParse(txtLightValue.Text, out var nValue);
                if (cbLightChannel.SelectedIndex == 0)
                    _LightParam.strLightValue1 = string.Format("{0:D3}", nValue);
                else
                    _LightParam.strLightValue2 = string.Format("{0:D3}", nValue);
                barLight.Value = nValue;
                SetLightValue();
            }
        }
        private void swLight_Toggled(object sender, EventArgs e)
        {
            LightOnOff(swLight.IsOn);
        }
        private void cbLightPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbLightPort.SelectedIndex == -1)
                return;

            _LightParam.strPortName = cbLightPort.EditValue.ToString();
        }
        private void cbLightBaud_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbLightBaud.SelectedIndex == -1)
                return;

            _LightParam.strBaudrate = cbLightBaud.EditValue.ToString();
        }
        private void cbLightChannel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbLightChannel.SelectedIndex == -1)
                return;

            if (cbLightChannel.SelectedIndex == 0)
            {
                _LightParam.strChannel1 = string.Format("{0:D2}", cbLightChannel.SelectedIndex + 1);
                int.TryParse(_LightParam.strLightValue1, out var nLightValue);
                txtLightValue.Text = nLightValue.ToString();
                barLight.Value = nLightValue;
            }
            else
            {
                _LightParam.strChannel2 = string.Format("{0:D2}", cbLightChannel.SelectedIndex + 1);
                int.TryParse(_LightParam.strLightValue2, out var nLightValue);
                txtLightValue.Text = nLightValue.ToString();
                barLight.Value = nLightValue;
            }
        }
        #endregion

        bool _bTest = false;

        #region Manual Menu
        private void btnAlltrigger_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _time = DateTime.Now;
            _var._strLotNo = _time.ToString("yyyyMMddHHmmss");
            lblLotNo.Caption = _var._strLotNo;


            for (int i = 0; i < _var._nScreenCnt; i++)
            {
                if (_var._bUseSensor)
                {
                    if (i == 1)
                    {
                        Thread.Sleep(1000);
                        _CAM[i]._dateTime = _time;
                        _Sensor.TriggerOn();
                    }
                    else
                    {
                        _CAM[i].Grab(false);
                        Thread.Sleep(200);
                    }
                }
                else if (_var._bUsePrint)
                {
                    OnTrigger(i);
                }
                else
                {
                    if (i == 0)
                    {
                        for (int j = 0; j < _var._nScreenCnt; j++)
                        {
                            _CAM[j].InitDisp(true);                        // 화면 초기화
                            _bResult[j] = false;
                            _bInspRes[j] = false;
                            _CAM[j]._dateTime = _time;
                            OnTrigger(i);
                        }
                    }
                    _CAM[i].Grab(false);
                    Thread.Sleep(200);
                }
            }
        }

        private void listModel_ListItemClick(object sender, DevExpress.XtraBars.ListItemClickEventArgs e)
        {
            OnModelChange(listModel.Strings[listModel.DataIndex]);
            for (int i = 0; i < _var._nScreenCnt; i++)
            {
                if (_var._bUseSensor)
                {
                    if (i == 1)
                        _CAM[i].SetSensorGraphicview();
                    else
                        _CAM[i].SetControl();
                }
                else if (_var._bUsePrint)
                {

                }
                else
                {
                    _CAM[i].SetControl();
                    _CAM[i].SetGraphicview();
                }


            }
        }
        #endregion

        #region keyence
        private void btnKeyenceSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (_var._bUseSensor)
                {
                    _sensorParam.strIP = txtKeyenceIP.Text;
                    _sensorParam.strPort = txtKeyencePort.Text;

                    //if (cbToolList.SelectedIndex > -1)
                    //{
                    //    double.TryParse(txtkeyenceHigh.Text, out var dHigh);
                    //    double.TryParse(txtKeyenceLow.Text, out var dLow);
                    //    _Sensor.SetThresholdValue(cbToolList.SelectedIndex, dHigh, dLow);
                    //}

                    ini.WriteIniFile("SensorIP", "Value", _sensorParam.strIP, _var._strConfigPath, "Sensor.ini");
                    ini.WriteIniFile("SensorPort", "Value", _sensorParam.strPort, _var._strConfigPath, "Sensor.ini");
                    ini.WriteIniFile("SensorLimitLow", "Value", _sensorParam.dLimitLow.ToString(), _var._strConfigPath, "Sensor.ini");
                    ini.WriteIniFile("SensorLimitHigh", "Value", _sensorParam.dLimitHigh.ToString(), _var._strConfigPath, "Sensor.ini");
                    ini.WriteIniFile("SensorTriggerTime", "Value", _sensorParam.nTriggerCycleTime.ToString(), _var._strConfigPath, "Sensor.ini");

                    AddMsg("Sensor 설정 저장 완료", green, false, false, MsgType.Alarm);
                }
            }
            catch (Exception ex)
            {
                AddMsg("Sensor 설정 저장 오류 : " + ex.Message, red, false, false, MsgType.Alarm);
            }

        }
        private void btnKeyenceClose_Click(object sender, EventArgs e)
        {
            flyKeyence.HideBeakForm();
        }
        private void btnKeyenctConnect_Click(object sender, EventArgs e)
        {
            SensorConnect();
        }
        private void SensorConnect()
        {
            if (string.IsNullOrEmpty(txtKeyenceIP.Text))
            {
                AddMsg("Sensor IP를 입력하여 주십시오", red, false, false, MsgType.Alarm);
                return;
            }
            if (string.IsNullOrEmpty(txtKeyencePort.Text))
            {
                AddMsg("Sensor Port를 입력하여 주십시오", red, false, false, MsgType.Alarm);
                return;
            }
            try
            {
                if (_Sensor.Connect(txtKeyenceIP.Text, txtKeyencePort.Text))
                {
                    lblKeyenceStatus.Text = "Connected";
                    lblKeyenceStatus.BackColor = Color.Lime;

                    lblSensor.ItemAppearance.Normal.ForeColor = Color.Lime;

                    AddMsg(Str.SensorConnected, green, false, false, MsgType.Alarm);

                    _Sensor.ChangeProdgram(_sensorParam.nModelNo);

                    GetToolList();
                    _CAM[1].SensorConnect();
                }
                else
                {
                    lblKeyenceStatus.Text = "Disconnected";
                    lblKeyenceStatus.BackColor = Color.Red;
                    lblSensor.ItemAppearance.Normal.ForeColor = Color.Red;
                    AddMsg(Str.SensorDisconnected, red, false, false, MsgType.Alarm);
                }
            }
            catch (Exception ex)
            {
                AddMsg("Sensor Connect Error : " + ex.ToString(), red, false, false, MsgType.Alarm);
            }
        }
        private void GetToolList()
        {
            var strTool = _Sensor.GetToolList();

            cbToolList.Properties.Items.Clear();
            if (strTool != null)
            {
                foreach (string strName in strTool)
                    cbToolList.Properties.Items.Add(strName);

                cbToolList.SelectedIndex = 0;
            }
        }
        private void simpleButton2_Click(object sender, EventArgs e)
        {
            if (listProd.SelectedIndex == -1) return;

            KeyenceChangeProdgram(listProd.SelectedIndex);
        }
        private void KeyenceChangeProdgram(int index)
        {
            _Sensor.ChangeProdgram(index);
            GetToolList();
        }
        private void btnKeyenceZeroOffset_Click(object sender, EventArgs e)
        {
            _Sensor.ZeroOffset("Zero");
        }
        private void btnZerooffsetClear_Click(object sender, EventArgs e)
        {
            _Sensor.ZeroOffset("Clear");
        }
        private void txtKeyenceLow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (cbToolList.SelectedIndex == -1)
                    return;

                var strTag = (sender as TextBox).Tag.ToString();
                double.TryParse(txtkeyenceHigh.Text, out var dHigh);
                double.TryParse(txtKeyenceLow.Text, out var dLow);

                _Sensor.SetThresholdValue(cbToolList.SelectedIndex, dHigh, dLow);
            }
        }
        private void txtkeyenceHigh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (cbToolList.SelectedIndex == -1)
                    return;

                var strTag = (sender as TextBox).Tag.ToString();
                double.TryParse(txtkeyenceHigh.Text, out var dHigh);
                double.TryParse(txtKeyenceLow.Text, out var dLow);

                _Sensor.SetThresholdValue(cbToolList.SelectedIndex, dHigh, dLow);
            }
        }
        private void btnTrigger_Click(object sender, EventArgs e)
        {
            OnSensorTrigger();
        }
        private void OnSensorTrigger()
        {
            _Sensor.TriggerOn();
        }
        private void OnSensorModelChange(int index)
        {
            try
            {
                _Sensor.ChangeProdgram(index);
            }
            catch { }
        }
        private void cbToolList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbToolList.SelectedIndex > -1)
            {
                var dValue = _Sensor.GetThresholdValue(cbToolList.SelectedIndex);

                if (dValue != null)
                {
                    txtkeyenceHigh.Text = dValue[0].ToString();
                    txtKeyenceLow.Text = dValue[1].ToString();
                }
                else
                {
                    txtkeyenceHigh.Text = "0";
                    txtKeyenceLow.Text = "0";
                }
            }
        }
        #endregion

        #region Print 
        private void btnPrintSave_Click(object sender, EventArgs e)
        {
            _PrintParam.strIP = txtPrintIP.Text;
            _PrintParam.strPort = txtPrintPort.Text;
            _PrintParam.strOriginalX = txtOriginalX.Text;
            _PrintParam.strOriginalY = txtOriginalY.Text;
            _PrintParam.str1DX = txtPrint1DX.Text;
            _PrintParam.str1DY = txtPrint1DY.Text;
            _PrintParam.str1DWidth = txtPrint1DWidth.Text;
            _PrintParam.str1DHeight = txtPrint1DHeight.Text;
            _PrintParam.strSerialX = txtPrintSeiralX.Text;
            _PrintParam.strSerialY = txtPrintSeiralY.Text;
            _PrintParam.strSerialWidth = txtPrintSerialWidth.Text;
            _PrintParam.strSerialHeight = txtPrintSerialHeight.Text;
            _PrintParam.str2DX = txtPrint2DDataX.Text;
            _PrintParam.str2DY = txtPrint2DDataY.Text;
            _PrintParam.str2DSize = txtPrint2DDataSize.Text;
            _PrintParam.strMIPX = txtPrintMIPX.Text;
            _PrintParam.strMIPY = txtPrintMIPY.Text;
            _PrintParam.strMIPWidth = txtPrintMIPWidth.Text;
            _PrintParam.strMIPHeight = txtPrintMIPHeight.Text;

            ini.WriteIniFile("PrintIP", "Value", _PrintParam.strIP, _var._strConfigPath, "Print.ini");
            ini.WriteIniFile("PrintPort", "Value", _PrintParam.strPort, _var._strConfigPath, "Print.ini");
            ini.WriteIniFile("PrintOriginalX", "Value", _PrintParam.strOriginalX, _var._strConfigPath, "Print.ini");
            ini.WriteIniFile("PrintOriginalY", "Value", _PrintParam.strOriginalY, _var._strConfigPath, "Print.ini");
            ini.WriteIniFile("Print1DX", "Value", _PrintParam.str1DX, _var._strConfigPath, "Print.ini");
            ini.WriteIniFile("Print1DY", "Value", _PrintParam.str1DY, _var._strConfigPath, "Print.ini");
            ini.WriteIniFile("Print1DWidth", "Value", _PrintParam.str1DWidth, _var._strConfigPath, "Print.ini");
            ini.WriteIniFile("Print1DHeight", "Value", _PrintParam.str1DHeight, _var._strConfigPath, "Print.ini");
            ini.WriteIniFile("PrintSerialX", "Value", _PrintParam.strSerialX, _var._strConfigPath, "Print.ini");
            ini.WriteIniFile("PrintSerialY", "Value", _PrintParam.strSerialY, _var._strConfigPath, "Print.ini");
            ini.WriteIniFile("PrintSerialWidth", "Value", _PrintParam.strSerialWidth, _var._strConfigPath, "Print.ini");
            ini.WriteIniFile("PrintSerialHeight", "Value", _PrintParam.strSerialHeight, _var._strConfigPath, "Print.ini");
            ini.WriteIniFile("Print2DX", "Value", _PrintParam.str2DX, _var._strConfigPath, "Print.ini");
            ini.WriteIniFile("Print2DY", "Value", _PrintParam.str2DY, _var._strConfigPath, "Print.ini");
            ini.WriteIniFile("Print2DSize", "Value", _PrintParam.str2DSize, _var._strConfigPath, "Print.ini");
            ini.WriteIniFile("PrintMIPX", "Value", _PrintParam.strMIPX, _var._strConfigPath, "Print.ini");
            ini.WriteIniFile("PrintMIPY", "Value", _PrintParam.strMIPY, _var._strConfigPath, "Print.ini");
            ini.WriteIniFile("PrintMIPWidth", "Value", _PrintParam.strMIPWidth, _var._strConfigPath, "Print.ini");
            ini.WriteIniFile("PrintMIPHeight", "Value", _PrintParam.strMIPHeight, _var._strConfigPath, "Print.ini");

            MessageBox.Show("Print Setting Complete");
            AddMsg("프린트 설정 저장 완료", green, false, false, MsgType.Log);
        }
        private void btnPrintClose_Click(object sender, EventArgs e)
        {
            flyPrint.HideBeakForm();
        }
        private void Print_Connection()
        {
            _print.Host = txtPrintIP.Text;
            _print.Port = int.Parse(txtPrintPort.Text);
            if (!_print.Connected)
                _print.Connect();

            Thread threadRead = new Thread(SetHostStatus);
            threadRead.Start();

            _print.OnConnect += new TCPComponents.TCPSimpleClient.OnConnectDelegate(this.OnConnect);
            _print.OnDisconnect += new TCPComponents.TCPSimpleClient.OnDisconnectDelegate(this.OnDisconnect);
            _print.OnDataAvailable += new TCPComponents.TCPSimpleClient.OnDataAvailableDelegate(this.OnDataAvailable);

            _PrintControl._OnMaunalPrint = SetEVMaunalPrint;
            _PrintControl._OnRePrint = SetRePrint;

            if (_print.Connected)
            {
                lblPrintStatus.Text = "Connection";
                lblPrintStatus.ForeColor = Color.White;
                lblPrintStatus.BackColor = Color.Lime;
                lblPrint.ItemAppearance.Normal.ForeColor = Color.Lime;

            }
            else
            {
                lblPrintStatus.Text = "Disconnection";
                lblPrintStatus.ForeColor = Color.White;
                lblPrintStatus.BackColor = Color.Red;
                lblPrint.ItemAppearance.Normal.ForeColor = Color.Red;
            }
        }
        private void OnConnect(object sender)
        {
            try
            {
                Invoke(new EventHandler(delegate
                {
                    lblPrintStatus.Text = "Connection";
                    lblPrintStatus.ForeColor = Color.White;
                    lblPrintStatus.BackColor = Color.Lime;
                    lblPrint.ItemAppearance.Normal.ForeColor = Color.Lime;
                }));

            }
            catch { }

        }
        private void OnDisconnect(object sneder)
        {
            try
            {
                Invoke(new EventHandler(delegate
                {
                    lblPrintStatus.Text = "Disconnection";
                    lblPrintStatus.ForeColor = Color.White;
                    lblPrintStatus.BackColor = Color.Red;
                    lblPrint.ItemAppearance.Normal.ForeColor = Color.Red;
                }));

            }
            catch { }
        }
        private void OnDataAvailable(object sender, string data, byte[] _data)
        { OnSendPrintMsg(data); }
        private void OnSendPrintMsg(string strMsg)
        {
            CheckStatus(strMsg.Trim());
        }
        private void PrintDataSend(string strData)
        {
            if (_var._bUsePrint)
            {
                _print.Send(strData);
                //_LSplc.InspCompeteSingle();
            }
            else
                AddMsg("Print Data Send Error :  Print Disconnected", red, false, false, MsgType.Log);
        }

        #region Print Function 
        public void SetEVAutoPrint(string[] strPrintData)                 // 프린트 프린트
        {
            try
            {
                int _iPrintCount = int.Parse(strPrintData[0]);
                // 데이터 추가 및 완료 신호 추가 필요 10.02
                string matrix_Data = "[)> _1E06_1DVSFZO_1DP" + strPrintData[1].Replace("-", "") + "_1DSAAAA_1DT" + DateTime.Now.ToString("yyMMdd") + "MT01P" + strPrintData[3] + "_1D_1E_04";

                string Data = "^XA^FWR^LS" + _PrintParam.strOriginalX + "^LT" + _PrintParam.strOriginalY;
                Data += "^FS^FO" + _PrintParam.str1DX + "," + _PrintParam.str1DY + "^BY" + _PrintParam.str1DWidth + ",3,10^BCB," + _PrintParam.str1DHeight + ",N,N,N,^FD" + strPrintData[3] + "^FS";
                Data += "^FO" + _PrintParam.strSerialX + "," + _PrintParam.strSerialY + "^A@B," + _PrintParam.strSerialHeight + "," + _PrintParam.strSerialWidth + ",Z: D.FNT^FD" + strPrintData[3] + "^FS";
                Data += "^FO" + _PrintParam.strMIPHeight + "," + _PrintParam.strMIPWidth + "^A@N," + _PrintParam.strSerialHeight + "," + _PrintParam.strSerialWidth + ",Z:D.FNT^FDPSPM(PM Synchronous Motor)^FS";
                Data += "^FO" + _PrintParam.strMIPX + "," + _PrintParam.strMIPY + "^A@N," + _PrintParam.strSerialHeight + "," + _PrintParam.strSerialWidth + ",Z:D.FNT^FD" + strPrintData[1] + "^FS";
                Data += "^FO" + _PrintParam.str2DX + "," + _PrintParam.str2DY + "^BXN," + _PrintParam.str2DSize + ",200,,,,,1^FH^FD";
                Data += matrix_Data + "^FS^XZ";

                string[] _insertData = new string[5];

                _insertData[0] = _var._strModelName;
                _insertData[1] = strPrintData[2];
                _insertData[2] = strPrintData[3];
                _insertData[3] = matrix_Data;
                _insertData[4] = "Auto";
                _PrintControl.PrintData_Write(_insertData[0], _insertData[1], _insertData[2], _insertData[3], _insertData[4]);
                _PrintControl.ViewMotorData(strPrintData[3]);

                string ViewData = matrix_Data.Replace("_1E_04", "");
                ViewData = ViewData.Replace("_1E", "");
                ViewData = ViewData.Replace("_1D", "");
                _PrintControl.ViewData(ViewData);

                for (int i = 0; i < _iPrintCount; i++)
                {
                    if (_var._bUsePrint)
                        PrintDataSend(Data);
                }
                _LSplc.InspCompeteSingle();
            }
            catch (Exception ex) { AddMsg("SetEVAutoPrint Err : " + ex.ToString(), red, false, false, MsgType.Alarm); }
        }
        public void SetEVMaunalPrint(string[] PrintDataList)                 // 프린트 프린트
        {
            try
            {
                //PrintDataList[0] Serial
                //PrintDataList[1] MIP Data
                // 데이터 추가 및 완료 신호 추가 필요 10.02
                string HCMPNData = ini.ReadIniFile("MIP", "Value", _var._strModelPath + "\\" + PrintDataList[1], string.Format("PRINT{0}.ini", 1)); ;
                string matrix_Data = "[)> _1E06_1DVSFZO_1DP" + HCMPNData.Replace("-", "") + "_1DSAAAA_1DT" + DateTime.Now.ToString("yyMMdd") + "MT01P" + PrintDataList[2] + "_1D_1E_04";

                string Data = "^XA^FWR^LS" + _PrintParam.strOriginalX + "^LT" + _PrintParam.strOriginalY;
                Data += "^FS^FO" + _PrintParam.str1DX + "," + _PrintParam.str1DY + "^BY" + _PrintParam.str1DWidth + ",3,10^BCB," + _PrintParam.str1DHeight + ",N,N,N,^FD" + PrintDataList[2] + "^FS";
                Data += "^FO" + _PrintParam.strSerialX + "," + _PrintParam.strSerialY + "^A@B," + _PrintParam.strSerialHeight + "," + _PrintParam.strSerialWidth + ",Z: D.FNT^FD" + PrintDataList[2] + "^FS";
                Data += "^FO" + _PrintParam.strMIPHeight + "," + _PrintParam.strMIPWidth + "^A@N," + _PrintParam.strSerialHeight + "," + _PrintParam.strSerialWidth + ",Z:D.FNT^FDPSPM(PM Synchronous Motor)^FS";
                Data += "^FO" + _PrintParam.strMIPX + "," + _PrintParam.strMIPY + "^A@N," + _PrintParam.strSerialHeight + "," + _PrintParam.strSerialWidth + ",Z:D.FNT^FD" + HCMPNData + "^FS";
                Data += "^FO" + _PrintParam.str2DX + "," + _PrintParam.str2DY + "^BXN," + _PrintParam.str2DSize + ",200,,,,,1^FH^FD";
                Data += matrix_Data + "^FS^XZ";

                string[] _insertData = new string[5];

                _insertData[0] = PrintDataList[1];
                _insertData[1] = DateTime.Now.ToString("yyMMddHHmmss");
                _insertData[2] = PrintDataList[2];
                _insertData[3] = matrix_Data;
                _insertData[4] = "Maunal";
                _PrintControl.PrintData_Write(_insertData[0], _insertData[1], _insertData[2], _insertData[3], _insertData[4]);

                string ViewData = matrix_Data.Replace("_1E_04", "");
                ViewData = ViewData.Replace("_1E", "");
                ViewData = ViewData.Replace("_1D", "");
                _PrintControl.ViewData(ViewData);
                for (int i = 0; i < int.Parse(PrintDataList[0]); i++)
                {
                    if (_var._bUsePrint)
                        PrintDataSend(Data);
                }


            }
            catch (Exception ex) { AddMsg("SetEVMaunalPrint Err : " + ex.ToString(), red, false, false, MsgType.Alarm); }
        }
        public string PrintModelMIP(string strModel)
        {
            string strMIPData = "";
            strMIPData = ini.ReadIniFile("MIP", "Value", _var._strModelPath + "\\" + strModel, string.Format("PRINT{0}.ini", 1));
            return strMIPData;
        }
        public void SetRePrint(string[] PrintDataList)
        {
            try
            {
                PrintDataList[1] = PrintModelMIP(PrintDataList[1]);

                string matrix_Data = "[)> _1E06_1DVSFZO_1DP" + PrintDataList[1].Replace("-", "") + "_1DSAAAA_1DT" + DateTime.Now.ToString("yyMMdd") + "MT01P" + PrintDataList[2] + "_1D_1E_04";

                string Data = "^XA^FWR^LS" + _PrintParam.strOriginalX + "^LT" + _PrintParam.strOriginalY;
                Data += "^FS^FO" + _PrintParam.str1DX + "," + _PrintParam.str1DY + "^BY" + _PrintParam.str1DWidth + ",3,10^BCB," + _PrintParam.str1DHeight + ",N,N,N,^FD" + PrintDataList[2] + "^FS";
                Data += "^FO" + _PrintParam.strSerialX + "," + _PrintParam.strSerialY + "^A@B," + _PrintParam.strSerialHeight + "," + _PrintParam.strSerialWidth + ",Z: D.FNT^FD" + PrintDataList[2] + "^FS";
                Data += "^FO" + _PrintParam.strMIPHeight + "," + _PrintParam.strMIPWidth + "^A@N," + _PrintParam.strSerialHeight + "," + _PrintParam.strSerialWidth + ",Z:D.FNT^FDPSPM(PM Synchronous Motor)^FS";
                Data += "^FO" + _PrintParam.strMIPX + "," + _PrintParam.strMIPY + "^A@N," + _PrintParam.strSerialHeight + "," + _PrintParam.strSerialWidth + ",Z:D.FNT^FD" + PrintDataList[1] + "^FS";
                Data += "^FO" + _PrintParam.str2DX + "," + _PrintParam.str2DY + "^BXN," + _PrintParam.str2DSize + ",200,,,,,1^FH^FD";
                Data += matrix_Data + "^FS^XZ";

                string[] _insertData = new string[5];

                _insertData[0] = _var._strModelName;
                _insertData[1] = DateTime.Now.ToString("yyMMddHHmmss");
                _insertData[2] = PrintDataList[2];
                _insertData[3] = matrix_Data;
                _insertData[4] = "RePrint";
                _PrintControl.PrintData_Write(_insertData[0], _insertData[1], _insertData[2], _insertData[3], _insertData[4]);

                string ViewData = matrix_Data.Replace("_1E_04", "");
                ViewData = ViewData.Replace("_1E", "");
                ViewData = ViewData.Replace("_1D", "");
                _PrintControl.ViewData(ViewData);
                for (int i = 0; i < int.Parse(PrintDataList[0]); i++)
                {
                    if (_var._bUsePrint)
                        PrintDataSend(Data);
                }


            }
            catch (Exception ex) { AddMsg("SetEVMaunalPrint Err : " + ex.ToString(), red, false, false, MsgType.Alarm); }
        }
        public void SetEVHistoryPrint(string stModel, string stLotid, string strPrintData)                 // 프린트 프린트
        {
            try
            {
                string[] _insertData = new string[4];

                _insertData[0] = stModel;
                _insertData[1] = stLotid;
                _insertData[2] = strPrintData;
                _insertData[3] = "History";

                string Data = "^XA^FWR^LS" + _PrintParam.strOriginalX + "^LT" + _PrintParam.strOriginalY;
                Data += "^FS^FO" + _PrintParam.str1DX + "," + _PrintParam.str1DY + "^BY" + _PrintParam.str1DWidth + ",3,10^BCB," + _PrintParam.str1DHeight + ",N,N,N,^FD" + stLotid + "^FS";
                Data += "^FO" + _PrintParam.strSerialX + "," + _PrintParam.strSerialY + "^A@B," + _PrintParam.strSerialHeight + "," + _PrintParam.strSerialWidth + ",Z: D.FNT^FD" + stLotid + "^FS";
                Data += "^FO" + _PrintParam.strMIPHeight + "," + _PrintParam.strMIPWidth + "^A@N," + _PrintParam.strSerialHeight + "," + _PrintParam.strSerialWidth + ",Z:D.FNT^FDPSPM(PM Synchronous Motor)^FS";
                Data += "^FO" + _PrintParam.strMIPX + "," + _PrintParam.strMIPY + "^A@N," + _PrintParam.strSerialHeight + "," + _PrintParam.strSerialWidth + ",Z:D.FNT^FD" + strPrintData.Substring(21, 5) + "-" + strPrintData.Substring(26, 5) + "^FS";
                Data += "^FO" + _PrintParam.str2DX + "," + _PrintParam.str2DY + "^BXN," + _PrintParam.str2DSize + ",200,,,,,1^FH^FD";
                Data += strPrintData + "^FS^XZ";

                if (_var._bUsePrint)
                    PrintDataSend(Data);


            }
            catch (Exception ex) { AddMsg("SetEVMaunalPrint Err : " + ex.ToString(), red, false, false, MsgType.Alarm); }
        }
        #endregion
        public void SetReprint()                                     // 프린트 재 프린트  
        {
            try
            {
                if (_var._bUsePrint)
                    PrintDataSend("~PR");
            }
            catch (Exception ex) { AddMsg("SetReprint Err : " + ex.ToString(), red, false, false, MsgType.Alarm); }
        }
        public void SetHostStatus()
        {                                // PC 프린트 연결 상태 
            try
            {
                while (true)
                {
                    if (_var._bUsePrint)
                        PrintDataSend("~HS");

                    Thread.Sleep(60000);
                }
            }
            catch (Exception ex) { AddMsg("SetHostStatus Err : " + ex.ToString(), red, false, false, MsgType.Alarm); }
        }
        public void SetCancelAll()                                  // 프린트 작업 취소  
        {
            try
            {
                if (_var._bUsePrint)
                    PrintDataSend("~JA");
            }
            catch (Exception ex) { AddMsg("SetCancelAll Err : " + ex.ToString(), red, false, false, MsgType.Alarm); }
        }
        private void CheckStatus(string strData)                     // 프린트 소모품 확인 완료 
        {
            try
            {
                Invoke(new EventHandler(delegate
                {


                    // 상태 확인 함수 단일 프린트 결과 확인 완료 
                    char cSpliter = Convert.ToChar(3);
                    string[] strHostStatus = strData.Split(cSpliter);
                    string[] strString1 = strHostStatus[0].Split(',');
                    string[] strString2 = strHostStatus[1].Split(',');

                    if (strString1[1] == "1")
                    {
                        bLabelStatus = false;
                        AddMsg("라벨 확인", red, false, false, MsgType.Alarm);         //프린트 라벨 확인
                        _PrintControl.lblLabel_status(false);

                    }
                    else
                    {
                        bLabelStatus = true;
                        _PrintControl.lblLabel_status(true);
                    }

                    if (strString2[3] == "1")
                    {
                        //리본 없음
                        bRibeenStatus = false;
                        AddMsg("리본 확인", red, false, false, MsgType.Alarm);        //프린트 리본 확인
                        _PrintControl.lblRibben_status(false);
                    }
                    else
                    {
                        bRibeenStatus = true;
                        _PrintControl.lblRibben_status(true);
                    }
                }));
            }
            catch (Exception ex)
            {
                AddMsg(this.Name + " CheckStatus Function Error : " + ex.Message, red, false, false, MsgType.Alarm);
            }
        }
        private void btnPrintConnect_Click(object sender, EventArgs e)
        {
            Print_Connection();
        }
        private void btnPrintDisconnect_Click(object sender, EventArgs e)
        {
            _print.Disconnect();
        }
        #endregion

        #region File Delete
        public void ImageBackup(string path)
        {
            string strOriginImagePath = path;

            DirectoryInfo dirInfo = new DirectoryInfo(strOriginImagePath);
            DirectoryInfo[] dirInfoYear;
            DirectoryInfo[] dirInfoMonth;
            DirectoryInfo[] dirInfoDay;
            DirectoryInfo[] dirInfoModelData;
            DirectoryInfo[] dirInfoCam;

            try
            {
                dirInfoYear = dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);                   //경로내 년도 파일
                dirInfoMonth = dirInfoYear[0].GetDirectories("*", SearchOption.TopDirectoryOnly);           //년도 파일 내 월 파일

                if (dirInfoMonth.Length == 0)
                    dirInfoYear[0].Delete(true);
                else
                {
                    dirInfoDay = dirInfoMonth[0].GetDirectories("*", SearchOption.TopDirectoryOnly);           //월 파일 내 일 파일

                    if (dirInfoDay.Length == 0)
                        dirInfoMonth[0].Delete(true);
                    else
                    {
                        dirInfoModelData = dirInfoDay[0].GetDirectories("*", SearchOption.TopDirectoryOnly);           //일 파일 내 Model Data 파일

                        if (dirInfoModelData.Length == 0)
                            dirInfoDay[0].Delete(true);
                        else
                        {
                            FileCopyToJPG(dirInfoModelData[0].FullName);

                            DirectoryInfo dirDel = new DirectoryInfo(dirInfoModelData[0].FullName);
                            dirDel.Delete(true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddMsg("OriginImageBackup Err : " + ex.ToString(), Color.Red, false, false, MsgType.Alarm);
            }
        }
        public void ImageDelete(string path)
        {
            string strImagePath = path;

            DirectoryInfo dirInfo = new DirectoryInfo(strImagePath);
            DirectoryInfo[] dirInfoYear;
            DirectoryInfo[] dirInfoMonth;
            DirectoryInfo[] dirInfoDay;
            DirectoryInfo[] dirResult;
            DirectoryInfo[] dirInfoModelData;
            

            try
            {
                dirInfoYear = dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);                   //경로내 년도 파일
                dirInfoMonth = dirInfoYear[0].GetDirectories("*", SearchOption.TopDirectoryOnly);           //년도 파일 내 월 파일
                
                if (dirInfoMonth.Length == 0)
                    dirInfoYear[0].Delete(true);
                else
                {
                    dirInfoDay = dirInfoMonth[0].GetDirectories("*", SearchOption.TopDirectoryOnly);           //월 파일 내 일 파일

                    if (dirInfoDay.Length == 0)
                        dirInfoMonth[0].Delete(true);
                    else
                    {
                        dirResult = dirInfoDay[0].GetDirectories("*", SearchOption.TopDirectoryOnly);           //일 파일 내 Model Data 파일

                        if (dirResult.Length == 0)
                            dirInfoDay[0].Delete(true);
                        else
                        {
                            dirInfoModelData = dirResult[0].GetDirectories("*", SearchOption.TopDirectoryOnly);
                            if (dirInfoModelData.Length == 0)
                                dirResult[0].Delete(true);
                            else
                            {
                                foreach (DirectoryInfo dirmodel in dirInfoModelData)
                                    dirmodel.Delete(true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddMsg("ImageDelete Err : " + ex.ToString(), Color.Red, false, false, MsgType.Alarm);
            }
        }
        public void FileCopyToJPG(string parentPath)
        {
            DirectoryInfo parentPathInfo = new DirectoryInfo(parentPath);
            string _PathInfo = parentPath.Replace("OriginImage", "ConvertImage");

            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 60L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            double width, height = 0;
            //

            try
            {
                FileInfo[] bmpFiles = parentPathInfo.GetFiles();
                Directory.CreateDirectory(_PathInfo);

                foreach (FileInfo bmpFile in bmpFiles)
                {

                    if (bmpFile.Extension.ToLower() == ".bmp")
                    {
                        byte[] raw = File.ReadAllBytes(bmpFile.FullName);
                        string jpgFileFullName = _PathInfo + "\\" + bmpFile.Name.Substring(0, bmpFile.Name.LastIndexOf('.')) + ".jpg";
                        FileInfo jpgFile = new FileInfo(jpgFileFullName);
                        using (Image img = Image.FromStream(new MemoryStream(raw)))
                        {
                            if (!jpgFile.Exists)
                            {
                                Bitmap bmp1 = new Bitmap(img);
                                bmp1.Save(jpgFileFullName, jpgEncoder, myEncoderParameters);
                            }
                        }
                    }
                    else if (bmpFile.Extension.ToLower() == ".jpg")
                    {
                        FileInfo jpgFile = new FileInfo(bmpFile.FullName);

                        jpgFile.CopyTo(_PathInfo + "\\" + bmpFile.Name);
                    }

                    File.Delete(bmpFile.FullName);
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show("<ERROR > <AutoImagesBackup >FileCopyToJPG Function : " + ex.Message);
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        #endregion

        private void btnBackupDelete_Click(object sender, EventArgs e)
        {
            //결과 삭제
            ResultImageDelete();
        }
        private void ResultImageDelete()
        {
            ImageDelete(_var._strSaveResultImagePath + "\\ResultImage\\");
            _bResultDelete = false;
            //MessageBox.Show("RESULTIMAGES DELETE START");
        }
        private void OriginalImageDelete()
        {
            ImageDelete(_var._strSaveImagePath + "\\OriginImage\\");
            _bOriginalDelete = false;
            //MessageBox.Show("ORIGINALIMAGES DELETE START");
        }
        private void btnOriginalDelete_Click(object sender, EventArgs e)
        {
            //원본 삭제
            OriginalImageDelete();
        }

        private void btnSaveResultImagePath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                txtSaveResultImagePath.Text = dialog.SelectedPath;
        }

    }
}
