using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using DevExpress.XtraEditors;

using System.IO;
using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using System.Drawing.Imaging;

namespace VisionSystem
{
    public delegate void CamSetHandler(int nIdx);
    public delegate void CamGrabCompleteHandler(int nIdx);
    public delegate void CamInspCompleteHandler(int nIdx, bool[] bRes, string[] strData, DateTime dateTime, string strSerial, string strCode);
    public delegate void MessageHaldler(string strMsg, Color color, bool bShoPopup, bool bError);
    public delegate void PositionChangeHandler(int nIdx, GlovalVar.PositionType type);
    public delegate void CamDisconnectHandler(int nIdx);
    public delegate void CamBCRData(int nIdx, string strBCRData);
    public delegate void ResultImageListHandler(int nIdx);
    public delegate void SensorTrigger();
    public delegate void SnesorModelChange(int index);

    public partial class CAM : DevExpress.XtraEditors.XtraUserControl
    {
        public CamSetHandler _CamSetFunc;
        public CamGrabCompleteHandler _OnGrabComplete;
        public CamInspCompleteHandler _OnInspComplete;
        public CamBCRData _onBCRData;
        public MessageHaldler _OnMessage;
        public PositionChangeHandler _OnPositionChange;
        public PositionChangeHandler _OnCamDisconnect;
        public ResultImageListHandler _OnResultImageList;
        public SensorTrigger _OnSensorTrigger;
        public SnesorModelChange _OnSensorModelChange;

        public int _nIdx = -1;
        public CamSet _camSet = new CamSet();
        bool _bLive = false;
        public GlovalVar _var;

        IniFiles ini = new IniFiles();
        frmToolEdit _frmToolEdit;

        public VproInspection _vPro = null;
        public bool _bLicense = false;
        bool _bManual = false;

        public int _nPos = 0;
        bool _bJobLoad = false;

        public int _nTotal = 0;
        public int _nOK = 0;
        public int _nNG = 0;

        public GlovalVar.ModelParam _modelParam;
        public GlovalVar.GraphicParam[] _graphicParam;

        public GlovalVar.GraphicResParam _graphicResParam = new GlovalVar.GraphicResParam();

        bool _bInspRes = false;
        bool _bInspEnd = false;

        public DateTime _dateTime;

        bool[] _bUse = null;
        CheckEdit[] _chkUse = null;
        ComboBoxEdit[] _cbColor = null;
        ComboBoxEdit[] _cbColor2 = null;
        ComboBoxEdit[] _cbLine = null;
        TextEdit[] _txtPoisionX = null;
        TextEdit[] _txtPoisionY = null;

        public ICogImage _cogGrabImg = null;

        Font font = new Font("Tahoma", 11, FontStyle.Regular);
        Font font1 = new Font("Tahoma", 11, FontStyle.Bold);

        public bool _bCamUse = false;
        public bool _bPassMode = false;

        private int nGrabCnt = 0;

        List<ICogImage> _listOriginImg = new List<ICogImage>();
        List<ICogImage> _listResultImg = new List<ICogImage>();
        private bool _bUpdate = false;

        bool _bLoad = false;
        int _nRes = -1;

        bool _bSensor = false;
        frmNGPopup _ngPopup = new frmNGPopup();
        public bool _bpopup = false;

        public CAM()
        {
            InitializeComponent();
        }

        public List<ICogImage> GetOriginImg
        {
            get { return _listOriginImg; }
        }

        public List<ICogImage> GetResultImg
        {
            get { return _listResultImg; }
        }

        public bool isUpdate
        {
            get { return _bUpdate; }
            set { _bUpdate = value; }
        }

        public GlovalVar.ModelParam ModelParam
        {
            get { return _modelParam; }
            set { _modelParam = value; }
        }

        public void SetMenuPosition(int nWidth)
        {
            lblMenu.Location = new Point(nWidth - lblMenu.Width - 20, 32);
        }

        public void LoadSet()
        {
            Pnl.Dock = DockStyle.Fill;
            tpDisp.Dock = DockStyle.Fill;
            lblMenu.Location = new Point(this.Width - lblMenu.Width - 20, 32);
            lblConnect.Text = string.Format("#{0}", _nIdx + 1);

            if (_camSet._bConnect)
            {
                lblConnect.BackColor = Color.Lime;
                lblConnect.ForeColor = Color.Black;
                _OnMessage(string.Format("#{0} Camera Connected", _nIdx + 1), Color.GreenYellow, false, false);
            }
            else
            {
                lblConnect.BackColor = Color.Red;
                lblConnect.ForeColor = Color.White;
                _OnMessage(string.Format("#{0} Camera Disconnected", _nIdx + 1), Color.Red, false, false);
            }

            int.TryParse(ini.ReadIniFile("TotalCnt", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraCnt{0}.ini", _nIdx + 1)), out _nTotal);
            int.TryParse(ini.ReadIniFile("OkCnt", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraCnt{0}.ini", _nIdx + 1)), out _nOK);
            int.TryParse(ini.ReadIniFile("NGCnt", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraCnt{0}.ini", _nIdx + 1)), out _nNG);

            if (cogDisp.Image == null)
                LoadModelImage();

            ModelChange();

            if (_var._bUseSensor)
            {
                if (_nIdx == 1)
                    SetSensorGraphicview();
                else
                    SetGraphicview();
            }
            else
                SetGraphicview();

            //cogResDisp.Width = _var._nWidth;
            //cogResDisp.Width = _var._nHeight;
            SetCount();

            _bLoad = true;
        }

        public void SensorConnect()
        {
            Invoke(new EventHandler(delegate
            {
                _bSensor = true;

                _camSet._bConnect = true;
                lblConnect.BackColor = Color.Lime;
                lblConnect.ForeColor = Color.Black;
                _OnMessage(string.Format("#{0} Sensor Connected", _nIdx + 1), Color.GreenYellow, false, false);

            }));
        }

        public void SetControl()
        {
            if (_chkUse != null)
            {
                for (int i = 0; i < _chkUse.Length; i++)
                {
                    if (_chkUse[i] != null)
                    {
                        _chkUse[i].Dispose();
                        _chkUse[i] = null;
                    }
                    if (_cbColor[i] != null)
                    {
                        _cbColor[i].Dispose();
                        _cbColor[i] = null;
                    }
                    if (_cbColor2[i] != null)
                    {
                        _cbColor2[i].Dispose();
                        _cbColor2[i] = null;
                    }
                    if (_cbLine[i] != null)
                    {
                        _cbLine[i].Dispose();
                        _cbLine[i] = null;
                    }

                }
            }

            scBar.Controls.Clear();
        }

        public void SetSensorControl()
        {
            if (_chkUse != null)
            {
                for (int i = 0; i < _chkUse.Length; i++)
                {
                    if (_chkUse[i] != null)
                    {
                        _chkUse[i].Dispose();
                        _chkUse[i] = null;
                    }
                    if (_txtPoisionX[i] != null)
                    {
                        _txtPoisionX[i].Dispose();
                        _txtPoisionX[i] = null;
                    }
                    if (_txtPoisionY[i] != null)
                    {
                        _txtPoisionY[i].Dispose();
                        _txtPoisionY[i] = null;
                    }
                }
            }
            scSensorBar.Controls.Clear();
        }

        public void SetGraphicview()
        {
            if (_vPro.GetJob == null)
                return;

            int nCnt = _vPro.GetJob.Tools.Count;
            string[] strColor = new string[6] { "Green", "Yellow", "Blue", "Cyan", "Magenta", "Red" };
            string[] strLine = new string[5] { "1", "2", "3", "4", "5" };

            SetControl();
            //lblToolName.Text = _setting._strJobName;
            _bUse = new bool[nCnt];

            _chkUse = null;
            _cbColor = null;
            _cbColor2 = null;
            _cbLine = null;

            _chkUse = new CheckEdit[nCnt];
            _cbColor = new ComboBoxEdit[nCnt];
            _cbColor2 = new ComboBoxEdit[nCnt];
            _cbLine = new ComboBoxEdit[nCnt];

            for (int i = 0; i < nCnt; i++)
            {
                _bUse[i] = false;
                _chkUse[i] = new CheckEdit();

                if (!_vPro.GetJob.Tools[i].Name.Contains("CogDataAnalysisTool") && !_vPro.GetJob.Tools[i].Name.Contains("Image Source"))
                {
                    scBar.Controls.Add(_chkUse[i]);
                    _chkUse[i].Font = font1;
                    _chkUse[i].Location = new Point(15, 6 + ((i - 2) * 28));
                    _chkUse[i].Size = new Size(250, 24);
                    _chkUse[i].Text = _vPro.GetJob.Tools[i].Name;
                    _chkUse[i].CheckedChanged += new System.EventHandler(chkUse_CheckedChanged);


                    _cbColor[i] = new ComboBoxEdit();
                    scBar.Controls.Add(_cbColor[i]);
                    _cbColor[i].Font = font1;
                    _cbColor[i].Location = new Point(300, 5 + ((i - 2) * 28));
                    _cbColor[i].Size = new Size(100, 24);
                    _cbColor[i].Properties.AppearanceDropDown.Font = font1;
                    _cbColor[i].Properties.Items.AddRange(strColor);
                    _cbColor[i].SelectedIndex = 0;

                    _cbColor2[i] = new ComboBoxEdit();
                    scBar.Controls.Add(_cbColor2[i]);
                    _cbColor2[i].Font = font1;
                    _cbColor2[i].Location = new Point(420, 5 + ((i - 2) * 28));
                    _cbColor2[i].Size = new Size(100, 24);
                    _cbColor2[i].Properties.AppearanceDropDown.Font = font1;
                    _cbColor2[i].Properties.Items.AddRange(strColor);
                    _cbColor2[i].SelectedIndex = 0;

                    _cbLine[i] = new ComboBoxEdit();
                    scBar.Controls.Add(_cbLine[i]);
                    _cbLine[i].Font = font1;
                    _cbLine[i].Location = new Point(530, 5 + ((i - 2) * 28));
                    _cbLine[i].Size = new Size(40, 24);
                    _cbLine[i].Properties.AppearanceDropDown.Font = font1;
                    _cbLine[i].Properties.Items.AddRange(strLine);
                    _cbLine[i].SelectedIndex = 0;


                    if (_graphicParam != null)
                    {
                        _chkUse[i].Checked = _graphicParam[i].bUse;
                        _cbColor[i].SelectedIndex = _graphicParam[i].nColor;
                        _cbColor2[i].SelectedIndex = _graphicParam[i].nColor2;
                        _cbLine[i].SelectedIndex = _graphicParam[i].nLineThick;
                    }
                }
            }

            cbAlign.SelectedIndex = _graphicResParam.nAlign;

            if (_graphicResParam.nFontSize != 0)
                txtGraphicSize.Text = _graphicResParam.nFontSize.ToString();
            else
                txtGraphicSize.Text = "15";

            if (_graphicResParam.nWidth != 0)
                txtGraphicX.Text = _graphicResParam.nWidth.ToString();
            else
                txtGraphicX.Text = "50";

            if (_graphicResParam.nHeight != 0)
                txtGraphicY.Text = _graphicResParam.nHeight.ToString();
            else
                txtGraphicY.Text = "50";

            if (_graphicResParam.nFont != 0)
                txtFont.Text = _graphicResParam.nFont.ToString();
            else
                txtFont.Text = "15";

            if (_graphicResParam.nPosY != 0)
                txtPosY.Text = _graphicResParam.nPosY.ToString();
            else
                txtPosY.Text = "50";


            if (_graphicResParam.n2DWidth != 0)
                txt2DGraphicX.Text = _graphicResParam.n2DWidth.ToString();
            else
                txt2DGraphicX.Text = "50";

            if (_graphicResParam.n2DHeight != 0)
                txt2DGraphicY.Text = _graphicResParam.n2DHeight.ToString();
            else
                txt2DGraphicY.Text = "50";
        }

        public void SetSensorGraphicview()
        {

            int nCnt = 6;
            string[] _strPoisitionX = new string[6];
            string[] _strPoisitionY = new string[6];

            SetSensorControl();
            _chkUse = new CheckEdit[nCnt];
            _txtPoisionX = new TextEdit[nCnt];
            _txtPoisionY = new TextEdit[nCnt];

            for (int i = 0; i < nCnt; i++)
            {
                //_bUse[i] = false;
                _chkUse[i] = new CheckEdit();
                scSensorBar.Controls.Add(_chkUse[i]);
                _chkUse[i].Font = font1;
                _chkUse[i].Location = new Point(15, 6 + ((i) * 28));
                _chkUse[i].Size = new Size(250, 24);
                _chkUse[i].Text = (i + 1) + " Position";


                _txtPoisionX[i] = new TextEdit();
                scSensorBar.Controls.Add(_txtPoisionX[i]);
                _txtPoisionX[i].Font = font1;
                _txtPoisionX[i].Location = new Point(300, 5 + ((i) * 28));
                _txtPoisionX[i].Size = new Size(100, 24);


                _txtPoisionY[i] = new TextEdit();
                scSensorBar.Controls.Add(_txtPoisionY[i]);
                _txtPoisionY[i].Font = font1;
                _txtPoisionY[i].Location = new Point(420, 5 + ((i) * 28));
                _txtPoisionY[i].Size = new Size(100, 24);



                if (_graphicParam != null)
                {
                    _chkUse[i].Checked = _graphicParam[i].bUse;
                    _txtPoisionX[i].Text = _graphicParam[i].nPosX.ToString();
                    _txtPoisionY[i].Text = _graphicParam[i].nPosY.ToString();
                }
            }

            cbAlign.SelectedIndex = _graphicResParam.nAlign;

            if (_graphicResParam.nFontSize != 0)
                txtSensorGraphicSize.Text = _graphicResParam.nFontSize.ToString();
            else
                txtSensorGraphicSize.Text = "15";

            if (_graphicResParam.nWidth != 0)
                txtSensorGraphicX.Text = _graphicResParam.nWidth.ToString();
            else
                txtSensorGraphicX.Text = "50";

            if (_graphicResParam.nHeight != 0)
                txtSensorGraphicY.Text = _graphicResParam.nHeight.ToString();
            else
                txtSensorGraphicY.Text = "50";

            if (_graphicResParam.nFont != 0)
                txtSensorFont.Text = _graphicResParam.nFont.ToString();
            else
                txtSensorFont.Text = "15";

            if (_graphicResParam.nPosY != 0)
                txtSensorPosY.Text = _graphicResParam.nPosY.ToString();
            else
                txtSensorPosY.Text = "50";


            if (_graphicResParam.n2DWidth != 0)
                txtSensor2DGraphicX.Text = _graphicResParam.n2DWidth.ToString();
            else
                txtSensor2DGraphicX.Text = "50";

            if (_graphicResParam.n2DHeight != 0)
                txtSensor2DGraphicY.Text = _graphicResParam.n2DHeight.ToString();
            else
                txtSensor2DGraphicY.Text = "50";
        }
        private void chkUse_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckEdit).Checked)
                (sender as CheckEdit).ForeColor = Color.Yellow;
            else
                (sender as CheckEdit).ForeColor = Color.White;
        }

        public void SetCount()
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(delegate
                {
                    lblTotal.Text = _nTotal.ToString();
                    lblOK.Text = _nOK.ToString();
                    lblNG.Text = _nNG.ToString();
                }));
            }
            else
            {
                lblTotal.Text = _nTotal.ToString();
                lblOK.Text = _nOK.ToString();
                lblNG.Text = _nNG.ToString();
            }

            SaveCount();
        }

        public void SaveCount()
        {
            if (!string.IsNullOrEmpty(_var._strModelName))
            {
                var strPath = _var._strModelPath + "\\" + _var._strModelName;
                DirectoryInfo dr = new DirectoryInfo(strPath);

                if (!dr.Exists)
                    dr.Create();

                ini.WriteIniFile("TotalCnt", "Value", _nTotal.ToString(), strPath, string.Format("CameraCnt{0}.ini", _nIdx + 1));
                ini.WriteIniFile("OkCnt", "Value", _nOK.ToString(), strPath, string.Format("CameraCnt{0}.ini", _nIdx + 1));
                ini.WriteIniFile("NGCnt", "Value", _nNG.ToString(), strPath, string.Format("CameraCnt{0}.ini", _nIdx + 1));
            }
        }

        public void SensorData(string[] strResult, string[] strValue, ICogImage cogImg)
        {
            var bRes = true;
            var strTotalRes = "";
            var bTotalRes = new bool[4] { true, true, true, true };
            var strTotalData = new string[7];

            try
            {
                Invoke(new EventHandler(delegate
                {
                    var strRes = strResult;
                    var strData = strValue;

                    var coglblRes = new CogGraphicLabel();

                    cogDisp.InteractiveGraphics.Clear();
                    cogDisp.StaticGraphics.Clear();

                    cogResDisp.InteractiveGraphics.Clear();
                    cogResDisp.StaticGraphics.Clear();

                    cogDisp.AutoFit = true;
                    cogResDisp.AutoFit = true;
                    cogDisp.Image = cogImg;
                    cogResDisp.Image = cogDisp.Image;

                    int.TryParse(ini.ReadIniFile("ResAlign", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", 2)), out _graphicResParam.nAlign);
                    int.TryParse(ini.ReadIniFile("FontSize", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", 2)), out _graphicResParam.nFontSize);
                    int.TryParse(ini.ReadIniFile("ResGraphicX", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", 2)), out _graphicResParam.nWidth);
                    int.TryParse(ini.ReadIniFile("ResGraphicY", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", 2)), out _graphicResParam.nHeight);
                    int.TryParse(ini.ReadIniFile("Font", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", 2)), out _graphicResParam.nFont);
                    int.TryParse(ini.ReadIniFile("GraphicY", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", 2)), out _graphicResParam.nPosY);
                    bool.TryParse(ini.ReadIniFile("CamUse", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", 2)), out _graphicResParam.bCamUse);
                    int.TryParse(ini.ReadIniFile("Res2DGraphicX", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", 2)), out _graphicResParam.n2DWidth);
                    int.TryParse(ini.ReadIniFile("Res2DGraphicY", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", 2)), out _graphicResParam.n2DHeight);

                    using (Font font = new Font("Tahoma", _graphicResParam.nFontSize, FontStyle.Bold))
                        coglblRes.Font = font;

                    if (_graphicResParam.nAlign == 0)
                        coglblRes.Alignment = CogGraphicLabelAlignmentConstants.BaselineLeft;
                    else if (_graphicResParam.nAlign == 1)
                        coglblRes.Alignment = CogGraphicLabelAlignmentConstants.BaselineCenter;
                    else
                        coglblRes.Alignment = CogGraphicLabelAlignmentConstants.BaselineRight;


                    _bInspRes = true;

                    for (var i = 0; i < strRes.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(strRes[i]))
                        {
                            var coglblData = new CogGraphicLabel();
                            using (Font font = new Font("Tahoma", _graphicResParam.nFont, FontStyle.Bold))
                                coglblData.Font = font;

                            if (_graphicResParam.nAlign == 0)
                                coglblData.Alignment = CogGraphicLabelAlignmentConstants.BaselineLeft;
                            else if (_graphicResParam.nAlign == 1)
                                coglblData.Alignment = CogGraphicLabelAlignmentConstants.BaselineCenter;
                            else
                                coglblData.Alignment = CogGraphicLabelAlignmentConstants.BaselineRight;

                            coglblData.Color = (strRes[i] == "OK") ? CogColorConstants.Green : CogColorConstants.Red;
                            coglblData.SetXYText(Convert.ToDouble(_graphicParam[i].nPosX), Convert.ToDouble(_graphicParam[i].nPosY), string.Format("{0}:{1}mm", i + 1, strData[i]));

                            if (strRes[i] == "NG")
                            {
                                //bTotalRes[0] = false;
                                _bInspRes = _bInspRes && false;
                                //bRes = false;
                                strTotalRes += "0";
                            }
                            else
                            {
                                _bInspRes = _bInspRes && true;
                                strTotalRes += "1";
                            }

                            cogDisp.StaticGraphics.Add(coglblData, "");
                            cogResDisp.StaticGraphics.Add(coglblData, "");
                        }
                        strTotalData[1] += strData[i] + ";";
                    }

                    bTotalRes[0] = _bInspRes;
                    bRes = _bInspRes;

                    _OnMessage("Sensor Data :" + strTotalData[1], Color.GreenYellow, false, false);

                    if (bRes)
                    {
                        coglblRes.Color = CogColorConstants.Green;
                        coglblRes.Text = "OK";
                        if (_var._strModelName != "01ST")
                            _nOK++;
                    }
                    else
                    {
                        coglblRes.Color = CogColorConstants.Red;
                        coglblRes.Text = "NG";
                        if (_var._strModelName != "01ST")
                            _nNG++;
                    }
                    if (_var._strModelName != "01ST")
                        _nTotal++;
                    coglblRes.X = _graphicResParam.nWidth;
                    coglblRes.Y = _graphicResParam.nHeight;

                    cogDisp.StaticGraphics.Add(coglblRes, "");
                    cogResDisp.StaticGraphics.Add(coglblRes, "");

                    Pnl.Invalidate();

                    strTotalData[0] = "1";
                    strTotalData[6] = strTotalRes;

                    //if (_OnInspComplete != null)
                    //{
                    //    Thread threadComplete = new Thread(() => _OnInspComplete(_nIdx, bTotalRes, strTotalData, _dateTime, _var._strLotNo, _modelParam.strCode));
                    //    threadComplete.Start();

                    //    lblStatus_Change(2);
                    //}
                    _OnInspComplete(_nIdx, bTotalRes, strTotalData, _dateTime, _var._strLotNo, _modelParam.strCode);
                    lblStatus_Change(2);

                    SetCount();

                }));
            }
            catch (Exception ex)
            {
                _OnMessage("Sensor Recieve Data Error : " + ex.Message, Color.Red, false, false);
                _OnInspComplete(_nIdx, bTotalRes, strTotalData, _dateTime, _var._strLotNo, _modelParam.strCode);
                lblStatus_Change(2);
            }
        }
        [STAThread]
        public void GrabFunc(int nSel, Bitmap bmpImg, ICogImage cogImg)
        {
            int nIdx = nSel;
            try
            {
                Invoke(new EventHandler(delegate
                {
                    cogDisp.AutoFit = true;
                    cogResDisp.AutoFit = true;
                    ICogImage cogGrab = cogImg;
                    nGrabCnt++;

                    if (bmpImg != null)
                    {
                        using (Bitmap bmpGrab = (Bitmap)bmpImg.Clone())
                        {
                            CogImage24PlanarColor cogImage = new CogImage24PlanarColor(bmpGrab);
                            cogGrab = cogImage;

                            cogImage.Dispose();
                            cogImage = null;
                        }
                    }

                    if (cogGrab != null)
                    {
                        if (!_bManual)
                        {
                            if (!_bPassMode)
                            {
                                if (!_bLive)
                                {
                                    if (_bJobLoad)
                                    {
                                        Thread threadRun = new Thread(() => _vPro.VJobrun(_nIdx, cogGrab, _graphicParam, _modelParam));
                                        threadRun.Start();
                                    }
                                    else
                                    {
                                        if (_OnMessage != null)
                                            _OnMessage("ob file not loaded", Color.Red, false, false);
                                    }
                                }
                            }
                            else
                            {
                                //_nOK++;

                                if (_OnInspComplete != null)
                                {
                                    var bRes = new bool[4] { true, true, true, true };
                                    var strData = new string[7] { "1", "", "", "", "", "", "" };
                                    _OnInspComplete(_nIdx, bRes, strData, _dateTime, _var._strLotNo, _modelParam.strCode);

                                    lblStatus_Change(1);

                                    SaveListImg();
                                    _bUpdate = true;
                                }

                                SetCount();
                            }
                        }
                        else
                        {
                            //_bInspRes = true;

                            //if (_var._bOriginImageSave)
                            //    SaveOriginImage();

                            //_bManual = false;
                        }
                        cogDisp.Image = cogGrab;
                        cogResDisp.Image = cogGrab;
                        _cogGrabImg = cogGrab;

                        //if (_bManual)
                        //{
                        //    Inspection();
                        //}
                    }
                }));

            }
            catch (Exception ex)
            {
                _OnMessage(string.Format("#{0} camera Grabfunc Err> : ", _nIdx + 1) + ex.ToString(), Color.Red, false, false);
            }
        }
        private void OnJobMessage(string strData, Color _Color, bool _bshow, bool _bshow2, string _strType)
        {
            if (_OnMessage != null)
                _OnMessage(string.Format("#{0} Caemra Job Err :{1}", _nIdx + 1, strData), _Color, false, false);
        }
        private void SaveListImg()
        {
            try
            {
                _listOriginImg.Add(cogDisp.Image);

                if (_listOriginImg.Count > 20)
                    _listOriginImg.RemoveAt(0);

                _listResultImg.Add(new CogImage24PlanarColor((Bitmap)cogDisp.CreateContentBitmap(Cognex.VisionPro.Display.CogDisplayContentBitmapConstants.Image)));

                if (_listResultImg.Count > 20)
                    _listResultImg.RemoveAt(0);
            }
            catch { }
        }

        public void Grab(bool bManual)
        {
            try
            {
                if (chkCamUse.Checked)
                {
                    if (_OnInspComplete != null)
                    {
                        var bRes = new bool[4] { true, true, true, true };
                        var bResult = new string[7] { "1", "", "", "", "", "", "1" };
                        _OnInspComplete(_nIdx, bRes, bResult, _dateTime, _var._strLotNo, _modelParam.strCode);
                    }
                    cogDisp.Image = null;
                    cogResDisp.Image = null;
                    SetCount();

                    return;
                }
                else
                {
                    cogDisp.Image = null;
                    cogResDisp.Image = null;

                    if (_nIdx == -1)
                        return;

                    cogDisp.InteractiveGraphics.Clear();
                    cogDisp.StaticGraphics.Clear();

                    cogResDisp.InteractiveGraphics.Clear();
                    cogResDisp.StaticGraphics.Clear();


                    if (!_camSet._bConnect)
                    {
                        if (_OnMessage != null)
                            _OnMessage(string.Format("#{0} Camera Disconnected", _nIdx + 1), Color.Red, false, false);
                        return;
                    }

                    nGrabCnt = 0;
                    Delay(_modelParam.nGrabdelay);

                    _bManual = bManual;

                    lblStatus_Change(0);
                    _camSet.Grab(_nIdx, _modelParam.dExpose);
                }

            }
            catch (Exception ex)
            {
                if (_OnMessage != null)
                    _OnMessage(string.Format("#{0} Camera Grab Error : {1}", _nIdx + 1, ex.Message), Color.Red, false, false);
            }
        }

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
        public void CamDisconnect()
        {
            _camSet.CamDisconnect();
        }
        public void LiveView()
        {
            if (!_camSet._bConnect)
            {
                MessageBox.Show("camera is not connected.", "Disconnected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                cogDisp.StaticGraphics.Clear();
                cogDisp.InteractiveGraphics.Clear();

                if (!_bLive)
                {
                    _bLive = true;
                    _camSet.LiveView(_nIdx, cogDisp, _bLive);

                    CogGraphicLabel lblLive = new CogGraphicLabel();
                    lblLive.SetXYText(50, 50, "Live");
                    using (Font font = new Font("Tahoma", 20, FontStyle.Bold))
                        lblLive.Font = font;
                    lblLive.Color = CogColorConstants.Green;
                    lblLive.Alignment = CogGraphicLabelAlignmentConstants.BaselineLeft;

                    cogDisp.StaticGraphics.Add(lblLive, "Live");

                    CogLine cogLineWidth = new CogLine();
                    CogLine cogLineHeight = new CogLine();

                    cogLineWidth.LineWidthInScreenPixels = 2;
                    cogLineWidth.Color = CogColorConstants.Green;

                    cogLineHeight.LineWidthInScreenPixels = 2;
                    cogLineHeight.Color = CogColorConstants.Green;

                    cogLineWidth.SetXYRotation(0, cogResDisp.Image.Height / 2.0, 0);
                    cogLineHeight.SetXYRotation(cogResDisp.Image.Width / 2.0, 0, (90 * Math.PI) / 180);

                    cogDisp.StaticGraphics.Add(cogLineWidth, "");
                    cogDisp.StaticGraphics.Add(cogLineHeight, "");
                }
                else
                {
                    _camSet.LiveView(_nIdx, cogDisp, false);
                    Thread.Sleep(500);
                    _bLive = false;

                    cogDisp.StaticGraphics.Clear();
                }
            }
            catch (Exception ex)
            {
                if (_OnMessage != null)
                    _OnMessage("Live Error : " + ex.Message, Color.Red, false, false);
            }
        }

        public Bitmap OpenImage()
        {
            Bitmap bmpImg = null;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Image Open";
            ofd.InitialDirectory = _var._strSaveImagePath;
            ofd.Filter = "Image File (*.bmp,*.jpg,*.png) | *.bmp;*.jpg;*.png; | All Files (*.*) | *.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _nRes = -1;
                Pnl.Invalidate();
                bmpImg = new Bitmap(ofd.FileName);
            }

            return bmpImg;
        }

        public void ToolEdit(int index)
        {
            if (_frmToolEdit != null)
            {
                _frmToolEdit.Dispose();
                _frmToolEdit = null;
            }

            _frmToolEdit = new frmToolEdit(this, _nIdx, index);
            _vPro.VJoblode(string.Format("{0}\\vpp\\Camera_{1:D2}\\{2:D2}.vpp", Application.StartupPath, _nIdx + 1, index + 1), (index + 1).ToString());
            _frmToolEdit.LoadSet(cogDisp.Image, _nIdx + 1, index + 1);
            _frmToolEdit.Show();
        }

        private void btnControl_Click(object sender, EventArgs e)
        {
            if (flyChk.IsPopupOpen)
                flyChk.HideBeakForm();

            if (!fypnl.IsPopupOpen)
                fypnl.ShowPopup();
            else
                fypnl.HidePopup();
        }

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

        private void btnOpenImg_Click(object sender, EventArgs e)
        {
            if (_bSensor) return;

            Bitmap bmpImg = OpenImage();

            try
            {
                if (bmpImg != null)
                {
                    cogDisp.StaticGraphics.Clear();
                    cogDisp.InteractiveGraphics.Clear();
                    cogDisp.Image = null;

                    cogDisp.AutoFit = true;
                    cogDisp.Image = new CogImage24PlanarColor(bmpImg);

                    cogResDisp.StaticGraphics.Clear();
                    cogResDisp.InteractiveGraphics.Clear();
                    cogResDisp.Image = null;

                    cogResDisp.AutoFit = true;
                    cogResDisp.Image = new CogImage24PlanarColor(bmpImg);
                    bmpImg.Dispose();
                }
            }
            catch { }
        }

        private void btnToolEdit_Click(object sender, EventArgs e)
        {
            if (_bSensor)
                return;

            if (!_bJobLoad)
            {
                MessageBox.Show("No inspection file", "No file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (fypnl.IsPopupOpen)
                fypnl.HidePopup();

            Delay(500);

            if (_vPro.GetJob != null)
                ToolEdit(Convert.ToInt32(_modelParam.strCode) - 1);
        }

        private void btnGrab_Click(object sender, EventArgs e)
        {
            _dateTime = DateTime.Now;
            if (_bSensor)
            {
                if (_OnSensorTrigger != null)
                    _OnSensorTrigger();
            }
            else
            {
                Grab(true);
                _nRes = -1;
                Pnl.Invalidate();
            }
        }

        private void btnLive_Click(object sender, EventArgs e)
        {
            if (_bSensor) return;

            _nRes = -1;
            Pnl.Invalidate();

            LiveView();
        }

        private void btnInspection_Click(object sender, EventArgs e)
        {
            if (_bSensor)
            {
                if (_OnSensorTrigger != null)
                    _OnSensorTrigger();
            }
            else
            {
                if (cogDisp.Image != null)
                    Inspection();
            }
        }

        public void Inspection()
        {
            try
            {
                if (_nIdx == -1)
                    return;

                if (cogDisp.Image == null)
                {
                    _OnMessage("<Inspection Err> No image to examine", Color.Red, false, false);
                    return;
                }

                if (!_bJobLoad)
                {
                    _OnMessage("<Inspection Err> No inspection file", Color.Red, false, false);
                    return;
                }

                InitDisp(false);

                ModelChange();

                if (_bJobLoad)
                {
                    Thread threadJobRun = new Thread(() => _vPro.VJobrun(_nIdx, cogDisp.Image, _graphicParam, _modelParam));
                    threadJobRun.Start();
                }
            }
            catch (Exception ex)
            {
                if (_OnMessage != null)
                    _OnMessage("Inspection Error : " + ex.Message, Color.Red, false, false);
            }
        }


        public void CamInit(bool _bCount)
        {
            InitDisp(false);
            if (_bCount)
            {
                _nTotal = 0;
                _nOK = 0;
                _nNG = 0;
            }

            _nRes = -1;
            Pnl.Invalidate();

            SetCount();
            SaveCount();

            cogDisp.Image = null;
            cogResDisp.Image = null;
        }
        public void InitDisp(bool _bClear)
        {
            cogDisp.InteractiveGraphics.Clear();
            cogDisp.StaticGraphics.Clear();
            if (_bClear)
                cogDisp.Image = null;

            cogResDisp.InteractiveGraphics.Clear();
            cogResDisp.StaticGraphics.Clear();
            if (_bClear)
                cogResDisp.Image = null;
        }

        private void btnMaster_Click(object sender, EventArgs e)
        {
            if (cogDisp.Image == null)
            {
                MessageBox.Show(Str.NoMasterImgMsg, "No Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show(Str.SaveMasterImgMsg, "Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            var strPath = string.Format("{0}\\Camera_{1:D2}", _var._strMasterImagePath, _nIdx + 1);
            var strFileName = "";
            DirectoryInfo dr = new DirectoryInfo(strPath);
            if (!dr.Exists)
            {
                dr.Create();
                strFileName = string.Format("{0:D2}", 1);
            }
            else
            {
                var strFolderFile = dr.GetFiles("*.bmp");

                if (string.IsNullOrEmpty(_modelParam.strCode))
                    strFileName = string.Format("{0:D2}", strFolderFile.Length + 1);
                else
                    strFileName = _modelParam.strCode;
            }

            CogImageFileBMP cogBMP = new CogImageFileBMP();
            cogBMP.Open(strPath + "\\" + strFileName + ".bmp", CogImageFileModeConstants.Write);
            cogBMP.Append(cogDisp.Image);
            cogBMP.Close();
            cogBMP.Dispose();

            MessageBox.Show(Str.SaveCompleteMasterImg, "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void Reset()
        {
            cogDisp.InteractiveGraphics.Clear();
            cogDisp.StaticGraphics.Clear();
            cogDisp.Image = null;

            cogResDisp.InteractiveGraphics.Clear();
            cogResDisp.StaticGraphics.Clear();
            cogResDisp.Image = null;

            lblTotal.Text = "0;";
            lblOK.Text = "0";
            lblNG.Text = "0";
        }

        private void btnCamSet_Click(object sender, EventArgs e)
        {
            if (_bSensor)
                return;

            fypnl.HideBeakForm();

            Delay(100);

            if (_CamSetFunc != null)
                _CamSetFunc(_nIdx);
        }

        private void btnMasterFileSave_Click(object sender, EventArgs e)
        {
            if (fypnl.IsPopupOpen)
                fypnl.HideBeakForm();

            CogImage24PlanarColor cogImg = new CogImage24PlanarColor((Bitmap)cogDisp.CreateContentBitmap(Cognex.VisionPro.Display.CogDisplayContentBitmapConstants.Image));
            string strPath = string.Format("{0}\\Camera_{1:D2}", _var._strMasterImagePath, _nIdx);
            DirectoryInfo dr = new DirectoryInfo(strPath);
            var strFileName = "";

            if (!dr.Exists)
            {
                dr.Create();
                strFileName = "01";
            }
            else
            {
                var strFolderFile = dr.GetFiles("*.bmp");

                if (string.IsNullOrEmpty(_modelParam.strCode))
                    strFileName = string.Format("{0:D2}", strFolderFile.Length + 1);
                else
                    strFileName = _modelParam.strCode;
            }

            using (CogImageFileBMP cogBMP = new CogImageFileBMP())
            {
                cogBMP.Open(string.Format("{0}\\{1}.bmp", strPath, strFileName), CogImageFileModeConstants.Write);
                cogBMP.Append(cogImg);
                cogBMP.Close();
            }

            cogImg.Dispose();
            cogImg = null;

            MessageBox.Show(Str.SaveCompleteMasterImg, "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtMasterName.Text = "";

        }

        public void ModelChange()
        {
            if (_vPro == null)
            {
                _vPro = new VproInspection();
                _vPro._OnJobLoad = OnJobLoad;
                _vPro._OnInspData = OnInspectionData;
                _vPro._OnMessage = OnJobMessage;
                _vPro._OnInspGraphic = OnInspectionGraphic;
                _vPro._OnInspRegionGraphic = OnInspRegionGraphic;
            }
            if (_modelParam.strCode != null)
            {
                if (_modelParam.strCode == "")
                {
                    if (_OnMessage != null)
                        _OnMessage(string.Format("#{0} Camera job file loading error", _nIdx + 1), Color.Red, false, false);
                }
                else if (_modelParam.strCode.Substring(0, 1) != "K")
                {
                    _bJobLoad = _vPro.VJoblode(string.Format("{0}\\vpp\\Camera_{1:D2}\\{2}.vpp", Application.StartupPath, _nIdx + 1, _modelParam.strCode), _modelParam.strCode);
                    if (!_bJobLoad)
                        _OnMessage(string.Format("#{0} Camera job file Not Load", _nIdx + 1), Color.Red, false, false);
                    else
                        chkPass.Checked = _bPassMode;
                }

                else
                {
                    OnJobLoad("");
                    _OnSensorModelChange(Convert.ToInt32(_modelParam.strCode.Substring(1, 1)));
                }
            }
        }

        public void LoadModelImage()
        {
            if (string.IsNullOrEmpty(_modelParam.strCode))
                return;

            try
            {
                var strPath = string.Format("{0}\\Camera_{1:D2}", _var._strMasterImagePath, _nIdx + 1);

                if (!string.IsNullOrEmpty(_modelParam.strCode))
                {
                    var stream = new FileInfo(strPath + "\\" + _modelParam.strCode + ".bmp").Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);

                    if (stream != null)
                    {
                        using (var bmp = new Bitmap(stream))
                        {
                            cogDisp.AutoFit = true;
                            cogDisp.Image = new CogImage24PlanarColor(bmp);

                            cogResDisp.AutoFit = true;
                            cogResDisp.Image = new CogImage24PlanarColor(bmp);
                        }

                        stream.Close();
                    }
                }
            }
            catch { }
        }

        private void OnJobLoad(string strResult)
        {
            string strRes = strResult;

            if (strRes == "")
            {
                Invoke(new EventHandler(delegate
                {

                    if (_var._bUseSensor)
                    {
                        if (_nIdx == 1)
                        {
                            _graphicParam = null;
                            _graphicParam = new GlovalVar.GraphicParam[6];

                            for (int i = 0; i < 6; i++)
                            {
                                _graphicParam[i] = new GlovalVar.GraphicParam();
                                bool.TryParse(ini.ReadIniFile(string.Format("GraphicUse{0}", i + 1), "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1)), out _graphicParam[i].bUse);
                                int.TryParse(ini.ReadIniFile(string.Format("GraphicPosX{0}", i + 1), "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1)), out _graphicParam[i].nPosX);
                                int.TryParse(ini.ReadIniFile(string.Format("GraphicPosY{0}", i + 1), "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1)), out _graphicParam[i].nPosY);
                            }
                            int.TryParse(ini.ReadIniFile("ResAlign", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.nAlign);
                            int.TryParse(ini.ReadIniFile("FontSize", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.nFontSize);
                            int.TryParse(ini.ReadIniFile("ResGraphicX", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.nWidth);
                            int.TryParse(ini.ReadIniFile("ResGraphicY", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.nHeight);
                            int.TryParse(ini.ReadIniFile("Font", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.nFont);
                            int.TryParse(ini.ReadIniFile("GraphicY", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.nPosY);
                            bool.TryParse(ini.ReadIniFile("CamUse", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.bCamUse);
                            int.TryParse(ini.ReadIniFile("Res2DGraphicX", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.n2DWidth);
                            int.TryParse(ini.ReadIniFile("Res2DGraphicY", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.n2DHeight);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(_modelParam.strCode))
                            {
                                int nCnt = _vPro.GetJobCount;
                                _var._strVppNo = _modelParam.strCode;
                                _graphicParam = null;
                                _graphicParam = new GlovalVar.GraphicParam[nCnt];

                                for (int i = 0; i < nCnt; i++)
                                {
                                    _graphicParam[i] = new GlovalVar.GraphicParam();

                                    bool.TryParse(ini.ReadIniFile(string.Format("GraphicUse{0}", i + 1), "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicParam[i].bUse);
                                    int.TryParse(ini.ReadIniFile(string.Format("GraphicColor{0}", i + 1), "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicParam[i].nColor);
                                    int.TryParse(ini.ReadIniFile(string.Format("GraphicColor2{0}", i + 1), "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicParam[i].nColor2);
                                    int.TryParse(ini.ReadIniFile(string.Format("GraphicLine{0}", i + 1), "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicParam[i].nLineThick);
                                }
                                int.TryParse(ini.ReadIniFile("ResAlign", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.nAlign);
                                int.TryParse(ini.ReadIniFile("FontSize", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.nFontSize);
                                int.TryParse(ini.ReadIniFile("ResGraphicX", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.nWidth);
                                int.TryParse(ini.ReadIniFile("ResGraphicY", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.nHeight);
                                int.TryParse(ini.ReadIniFile("Font", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.nFont);
                                int.TryParse(ini.ReadIniFile("GraphicY", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.nPosY);
                                bool.TryParse(ini.ReadIniFile("CamUse", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.bCamUse);
                                int.TryParse(ini.ReadIniFile("Res2DGraphicX", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.n2DWidth);
                                int.TryParse(ini.ReadIniFile("Res2DGraphicY", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.n2DHeight);

                                if (_modelParam.bBCRInsp)
                                {
                                    txt2DGraphicX.Visible = true;
                                    txt2DGraphicY.Visible = true;
                                    labelControl7.Visible = true;
                                    labelControl8.Visible = true;
                                }
                                else
                                {
                                    txt2DGraphicX.Visible = false;
                                    txt2DGraphicY.Visible = false;
                                    labelControl7.Visible = false;
                                    labelControl8.Visible = false;
                                }

                                chkCamUse.Checked = _graphicResParam.bCamUse;
                                if (_graphicResParam.bCamUse)
                                {

                                    lblConnect.BackColor = Color.LightGray;
                                    chkCamUse.ForeColor = Color.Yellow;
                                }
                                else
                                {
                                    chkCamUse.ForeColor = Color.White;
                                    if (_camSet._bConnect)
                                        lblConnect.BackColor = Color.Lime;
                                    else
                                        lblConnect.BackColor = Color.Red;

                                }
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(_modelParam.strCode))
                        {
                            int nCnt = _vPro.GetJobCount;
                            _var._strVppNo = _modelParam.strCode;
                            _graphicParam = null;
                            _graphicParam = new GlovalVar.GraphicParam[nCnt];

                            for (int i = 0; i < nCnt; i++)
                            {
                                _graphicParam[i] = new GlovalVar.GraphicParam();

                                bool.TryParse(ini.ReadIniFile(string.Format("GraphicUse{0}", i + 1), "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicParam[i].bUse);
                                int.TryParse(ini.ReadIniFile(string.Format("GraphicColor{0}", i + 1), "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicParam[i].nColor);
                                int.TryParse(ini.ReadIniFile(string.Format("GraphicColor2{0}", i + 1), "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicParam[i].nColor2);
                                int.TryParse(ini.ReadIniFile(string.Format("GraphicLine{0}", i + 1), "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicParam[i].nLineThick);
                            }
                            int.TryParse(ini.ReadIniFile("ResAlign", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.nAlign);
                            int.TryParse(ini.ReadIniFile("FontSize", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.nFontSize);
                            int.TryParse(ini.ReadIniFile("ResGraphicX", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.nWidth);
                            int.TryParse(ini.ReadIniFile("ResGraphicY", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.nHeight);
                            int.TryParse(ini.ReadIniFile("Font", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.nFont);
                            int.TryParse(ini.ReadIniFile("GraphicY", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.nPosY);
                            bool.TryParse(ini.ReadIniFile("CamUse", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.bCamUse);
                            int.TryParse(ini.ReadIniFile("Res2DGraphicX", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.n2DWidth);
                            int.TryParse(ini.ReadIniFile("Res2DGraphicY", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1)), out _graphicResParam.n2DHeight);

                            if (_modelParam.bBCRInsp)
                            {
                                txt2DGraphicX.Visible = true;
                                txt2DGraphicY.Visible = true;
                                labelControl7.Visible = true;
                                labelControl8.Visible = true;
                            }
                            else
                            {
                                txt2DGraphicX.Visible = false;
                                txt2DGraphicY.Visible = false;
                                labelControl7.Visible = false;
                                labelControl8.Visible = false;
                            }

                            chkCamUse.Checked = _graphicResParam.bCamUse;
                            if (_graphicResParam.bCamUse)
                            {

                                lblConnect.BackColor = Color.LightGray;
                                chkCamUse.ForeColor = Color.Yellow;
                            }
                            else
                            {
                                chkCamUse.ForeColor = Color.White;
                                if (_camSet._bConnect)
                                    lblConnect.BackColor = Color.Lime;
                                else
                                    lblConnect.BackColor = Color.Red;

                            }
                        }
                    }
                }));

                _bJobLoad = true;
                //_OnMessage(string.Format("#{0} Camera job file load completed", _nIdx + 1), Color.GreenYellow, false, false);
            }
            else
            {
                _bJobLoad = false;
                _OnMessage(string.Format("#{0} Camera job file loading error : {1}", _nIdx + 1, strRes), Color.Red, false, false);
            }
        }

        private void OnInspectionData(string[] strValue)
        {
            string[] strData = strValue;
            bool[] bRes = new bool[4];
            try
            {
                _bInspRes = false;

                lblStatus_Change(1);
                CogGraphicLabel[] coglbl = new CogGraphicLabel[6];

                if (strValue == null)
                {
                    if (_modelParam.nDefectCnt > nGrabCnt)
                    {
                        ReInspection();
                        return;
                    }
                    else
                    {
                        strData = new string[7];
                        strData[0] = "2";
                        strData[1] = "";
                        strData[2] = "";
                        strData[3] = "";
                        strData[4] = "";
                        strData[5] = "";
                        strData[6] = "";
                        bRes[0] = false;
                        bRes[1] = false;    // 바코드 결과
                        bRes[2] = false;    // 핀 마스터 비교 결과
                        bRes[3] = false;
                        _bInspRes = false;
                        _nRes = 2;
                        _nNG++;
                    }
                }
                else
                {
                    for (int i = 0; i < 6; i++)
                        coglbl[i] = new CogGraphicLabel();
                    bRes[0] = ChkDefectInspection(strData[0]);
                    bRes[1] = Chk2DInspection(strData[1], ref coglbl[1]);
                    GetAlignData(strData[0], ref strData[2], ref strData[3], ref strData[4], ref coglbl[2], ref coglbl[3], ref coglbl[4]);
                    bRes[2] = GetPinData(strData[0], strData[5], ref coglbl[5]);
                    bRes[3] = GetPointData(strData[6]);
                }

                _bInspRes = (bRes[0] && bRes[1] && bRes[2] && bRes[3]) ? true : false;

                #region 검사 OK NG 결과 포인트 설정
                using (Font font = new Font("Tahoma", _graphicResParam.nFontSize, FontStyle.Bold))
                    coglbl[0].Font = font;

                if (_graphicResParam.nAlign == 0)
                    coglbl[0].Alignment = CogGraphicLabelAlignmentConstants.BaselineLeft;
                else if (_modelParam.nAlign == 1)
                    coglbl[0].Alignment = CogGraphicLabelAlignmentConstants.BaselineCenter;
                else
                    coglbl[0].Alignment = CogGraphicLabelAlignmentConstants.BaselineRight;

                if (_bInspRes)
                {
                    _nRes = 1;
                    coglbl[0].Text = "OK";
                    coglbl[0].Color = CogColorConstants.Green;

                }
                else
                {
                    _nRes = 2;
                    coglbl[0].Text = "NG";
                    coglbl[0].Color = CogColorConstants.Red;
                }
                #endregion
                var nCnt = 1;

                #region Keyence Label
                if (_var._bUseSensor)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (coglbl[i] != null)
                        {
                            if (i == 0)
                            {
                                coglbl[i].X = _graphicResParam.nWidth;
                                coglbl[i].Y = _graphicResParam.nHeight;
                            }
                            else
                            {
                                coglbl[i].X = _graphicResParam.nWidth;
                                coglbl[i].Y = _graphicResParam.nHeight + (nCnt * _graphicResParam.nPosY);
                            }

                            cogDisp.StaticGraphics.Add(coglbl[i], "");
                            cogResDisp.StaticGraphics.Add(coglbl[i], "");
                            nCnt++;
                        }
                    }
                }
                else
                {
                    if (cogDisp.Image != null)
                    {

                        for (int i = 0; i < 6; i++)
                        {
                            if (coglbl[i] != null)
                            {
                                if (i == 0)
                                {
                                    coglbl[i].X = (cogDisp.Image.Width) * _graphicResParam.nWidth / 100;
                                    coglbl[i].Y = (cogDisp.Image.Height) * _graphicResParam.nHeight / 100;
                                }
                                else if (i == 1)
                                {

                                    coglbl[i].X = (cogDisp.Image.Width) * _graphicResParam.n2DWidth / 100;
                                    coglbl[i].Y = (cogDisp.Image.Height) * _graphicResParam.n2DHeight / 100;
                                }
                                else
                                {
                                    coglbl[i].X = (cogDisp.Image.Width) * 10 / 100;
                                    coglbl[i].Y = (cogDisp.Image.Height) * ((nCnt * _graphicResParam.nPosY)) / 100;
                                }


                                cogDisp.StaticGraphics.Add(coglbl[i], "");
                                cogResDisp.StaticGraphics.Add(coglbl[i], "");
                                nCnt++;
                            }


                        }
                    }
                }
                #endregion
                //_OnInspComplete(_nIdx, bRes, strData, _dateTime, _var._strLotNo, _modelParam.strCode);
                //lblStatus_Change(2);
                if (_OnInspComplete != null)
                {
                    Thread threadComplete = new Thread(() => _OnInspComplete(_nIdx, bRes, strData, _dateTime, _var._strLotNo, _modelParam.strCode));
                    threadComplete.Start();
                    lblStatus_Change(2);
                }

                Pnl.Invalidate();

                if (_modelParam.nDefectCnt >= nGrabCnt)
                {
                    if (!_bInspRes)
                    {
                        ReInspection();
                        return;
                    }
                }
                else
                {
                    if (_bInspRes)
                    {
                        if (_var._strModelName != "01ST")
                            _nOK++;
                    }
                    else
                    {
                        if (_var._strModelName != "01ST")
                            _nNG++;
                        _bInspEnd = true;
                    }
                }
                if (_var._strModelName != "01ST")
                    _nTotal++;



                SetCount();
            }
            catch (Exception ex)
            {
                _bInspRes = false;
                _OnInspComplete(_nIdx, bRes, strData, _dateTime, _var._strLotNo, _modelParam.strCode);
                lblStatus_Change(3);
            }
        }

        #region Pin Data
        private bool GetPinData(string strRes, string strData, ref CogGraphicLabel coglbl)
        {
            bool bRes = true;
            if (_modelParam.bPinChange)
            {
                int.TryParse(strRes, out var nRes);

                if (_graphicResParam.nAlign == 0)
                    coglbl.Alignment = CogGraphicLabelAlignmentConstants.BaselineLeft;
                else if (_graphicResParam.nAlign == 1)
                    coglbl.Alignment = CogGraphicLabelAlignmentConstants.BaselineCenter;
                else
                    coglbl.Alignment = CogGraphicLabelAlignmentConstants.BaselineRight;

                using (Font font = new Font("Tahoma", _graphicResParam.nFont, FontStyle.Bold))
                    coglbl.Font = font;

                coglbl.Text = string.Format("Pin Data (Master : {0}, Inspection : {1})", _modelParam.strPinMaster, strData);
                if (_modelParam.strPinMaster != "")
                {
                    if (_modelParam.strPinMaster == strData)
                        bRes = true;
                    else
                        bRes = false;
                }
                else
                    bRes = true;

                if (nRes > 0 && bRes)
                    coglbl.Color = CogColorConstants.Green;
                else
                    coglbl.Color = CogColorConstants.Red;

            }
            else
                coglbl = null;

            return bRes;
        }

        private bool GetPointData(string strData)
        {
            bool bRes = true;
            if (!_modelParam.bDefectInsp)
            {
                if (strData == "1")
                    bRes = true;
                else
                    bRes = false;
            }
            return bRes;
        }

        #endregion

        #region 로봇 좌표 데이터 
        private void GetAlignData(string strRes, ref string strDataX, ref string strDataY, ref string strDataR, ref CogGraphicLabel coglblX, ref CogGraphicLabel coglblY, ref CogGraphicLabel coglblR)
        {
            if (_modelParam.bAlingInsp)
            {
                int.TryParse(strRes, out var nRes);

                if (nRes > 0)
                {
                    if (_graphicResParam.nAlign == 0)
                    {
                        coglblX.Alignment = CogGraphicLabelAlignmentConstants.BaselineLeft;
                        coglblY.Alignment = CogGraphicLabelAlignmentConstants.BaselineLeft;
                        coglblR.Alignment = CogGraphicLabelAlignmentConstants.BaselineLeft;
                    }
                    else if (_graphicResParam.nAlign == 1)
                    {
                        coglblX.Alignment = CogGraphicLabelAlignmentConstants.BaselineCenter;
                        coglblY.Alignment = CogGraphicLabelAlignmentConstants.BaselineCenter;
                        coglblR.Alignment = CogGraphicLabelAlignmentConstants.BaselineCenter;
                    }
                    else
                    {
                        coglblX.Alignment = CogGraphicLabelAlignmentConstants.BaselineRight;
                        coglblY.Alignment = CogGraphicLabelAlignmentConstants.BaselineRight;
                        coglblR.Alignment = CogGraphicLabelAlignmentConstants.BaselineRight;
                    }
                    double dX, dY, dR = 0;
                    double dX1, dY1, dR1 = 0;
                    if (_modelParam.bXYreversal)
                    {
                        if (_modelParam.bXreversal)
                        {
                            dY = (Convert.ToDouble(string.Format("{0:F2}", Convert.ToDouble(strDataX))) - _modelParam.dAlignMasterX) * -1;
                        }
                        else
                        {
                            dY = (Convert.ToDouble(string.Format("{0:F2}", Convert.ToDouble(strDataX))) - _modelParam.dAlignMasterX);
                        }
                        if (_modelParam.bYreversal)
                        {
                            dX = (Convert.ToDouble(string.Format("{0:F2}", Convert.ToDouble(strDataY))) - _modelParam.dAlignMasterY) * -1;
                        }
                        else
                        {
                            dX = (Convert.ToDouble(string.Format("{0:F2}", Convert.ToDouble(strDataY))) - _modelParam.dAlignMasterY);
                        }
                    }
                    else
                    {
                        if (_modelParam.bXreversal)
                        {
                            dX = (Convert.ToDouble(string.Format("{0:F2}", Convert.ToDouble(strDataX))) - _modelParam.dAlignMasterX) * -1;
                        }
                        else
                        {
                            dX = (Convert.ToDouble(string.Format("{0:F2}", Convert.ToDouble(strDataX))) - _modelParam.dAlignMasterX);
                        }
                        if (_modelParam.bYreversal)
                        {
                            dY = (Convert.ToDouble(string.Format("{0:F2}", Convert.ToDouble(strDataY))) - _modelParam.dAlignMasterY) * -1;
                        }
                        else
                        {
                            dY = (Convert.ToDouble(string.Format("{0:F2}", Convert.ToDouble(strDataY))) - _modelParam.dAlignMasterY);
                        }
                    }
                    if (_modelParam.bZreversal)
                    {
                        dR = RadToAngle((Convert.ToDouble(string.Format("{0:F8}", Convert.ToDouble(strDataR))) - _modelParam.dAlignMasterR) * -1);
                    }
                    else
                    {
                        dR = RadToAngle((Convert.ToDouble(string.Format("{0:F8}", Convert.ToDouble(strDataR))) - _modelParam.dAlignMasterR));
                    }

                    dX1 = dX;
                    dY1 = dY;
                    dR1 = dR;

                    dX = Math.Round((dX * _modelParam.dResoluton + _modelParam.dAlignOffsetX), 2);
                    dY = Math.Round((dY * _modelParam.dResoluton + _modelParam.dAlignOffsetY), 2);
                    dR = Math.Round((dR + _modelParam.dAlignOffsetR), 7);

                    var strX = string.Format("{0}", (int)(dX * 1000));
                    var strY = string.Format("{0}", (int)(dY * 1000));
                    var strR = string.Format("{0}", (int)(dR * 1000));
                    strDataX = strX;
                    strDataY = strY;
                    strDataR = strR;
                    coglblX.Text = string.Format("X : {0:F3}({1:F3},{2:F3})", dX, _modelParam.dAlignMasterX, dX1);
                    coglblY.Text = string.Format("Y : {0:F3}({1:F3},{2:F3})", dY, _modelParam.dAlignMasterY, dY1);
                    coglblR.Text = string.Format("R : {0:F5}({1:F9},{2:F9})", dR, _modelParam.dAlignMasterR, dR1);

                    using (Font font = new Font("Tahoma", _graphicResParam.nFont, FontStyle.Bold))
                    {
                        coglblX.Font = font;
                        coglblY.Font = font;
                        coglblR.Font = font;
                    }

                    coglblX.Color = CogColorConstants.Green;
                    coglblY.Color = CogColorConstants.Green;
                    coglblR.Color = CogColorConstants.Green;
                }
                else
                {
                    coglblX = null;
                    coglblY = null;
                    coglblR = null;
                }
            }
            else
            {
                coglblX = null;
                coglblY = null;
                coglblR = null;
            }
        }


        public double RadToAngle(double Rad)
        {
            return (Rad * 180.0) / Math.PI;
        }
        #endregion

        #region 결과 처리 OK or NG
        private bool ChkDefectInspection(string strResult)
        {
            bool bRes = true;
            if (_modelParam.bDefectInsp)
            {
                int.TryParse(strResult, out var nRes);

                if (nRes == 1)
                    bRes = true;
                else
                    bRes = false;
            }

            return bRes;
        }
        #endregion

        #region 2D 데이터 처리
        private bool Chk2DInspection(string str2DData, ref CogGraphicLabel coglbl)
        {
            bool bRes = true;

            if (_modelParam.bBCRInsp)
            {
                using (Font font = new Font("Tahoma", _graphicResParam.nFont, FontStyle.Bold))
                    coglbl.Font = font;

                if (_graphicResParam.nAlign == 0)
                    coglbl.Alignment = CogGraphicLabelAlignmentConstants.BaselineLeft;
                else if (_graphicResParam.nAlign == 1)
                    coglbl.Alignment = CogGraphicLabelAlignmentConstants.BaselineCenter;
                else
                    coglbl.Alignment = CogGraphicLabelAlignmentConstants.BaselineRight;
                if (str2DData.Contains('?'))
                {
                    bRes = false;
                }

                if (!string.IsNullOrEmpty(_modelParam.strBCRData) && !string.IsNullOrEmpty(_modelParam.strBCRLen))
                {

                    var strParse = _modelParam.strBCRLen.Split('~');
                    int.TryParse(strParse[0], out int nStart);
                    int.TryParse(strParse[1], out int nEnd);
                    if (!str2DData.Contains("Err"))
                    {
                        //var strData = str2DData.Substring(nStart, nEnd);
                        if (_modelParam.strBCRData == "PassData")
                        {
                            CogGraphicLabel cogPassData = new CogGraphicLabel();
                            using (Font font = new Font("Tahoma", _graphicResParam.nFont, FontStyle.Bold))
                            {
                                cogPassData.Font = font;
                                cogPassData.Text = _var._strPassData;
                                cogPassData.X = 100;
                                cogPassData.Y = 100;
                            }

                            if (_var._strPassData == str2DData.Substring(nStart, nEnd - nStart))
                                coglbl.Color = CogColorConstants.Green;
                            else
                            {
                                bRes = false;
                                coglbl.Color = CogColorConstants.Red;
                            }
                            coglbl.Text = "Data : " + str2DData.Substring(nStart, nEnd - nStart) + "(" + _var._strPassData + ")";
                        }
                        else
                        {
                            if (_modelParam.strBCRData == str2DData.Substring(nStart, nEnd - nStart))
                            {
                                coglbl.Color = CogColorConstants.Green;
                            }
                            else
                            {
                                bRes = false;
                                coglbl.Color = CogColorConstants.Red;
                            }
                            coglbl.Text = "Data : " + str2DData + "(" + str2DData.Substring(nStart, nEnd - nStart) + ")";
                        }
                    }
                    else
                    {
                        bRes = false;
                        coglbl.Color = CogColorConstants.Red;

                        if (_modelParam.strBCRData == "PassData")
                        {
                            CogGraphicLabel cogPassData = new CogGraphicLabel();
                            using (Font font = new Font("Tahoma", _graphicResParam.nFont, FontStyle.Bold))
                            {
                                cogPassData.Font = font;
                                cogPassData.Text = _var._strPassData;
                                cogPassData.X = 100;
                                cogPassData.Y = 100;
                            }
                            coglbl.Text = "Data : " + str2DData + "(" + _var._strPassData + ")";
                        }
                        else
                            coglbl.Text = "Data : " + str2DData + "(" + _modelParam.strBCRData + ")";
                    }
                }
                else
                {
                    coglbl.Text = "Data : " + str2DData;
                    coglbl.Color = CogColorConstants.Green;
                }

            }
            else
                coglbl = null;

            return bRes;
        }
        #endregion

        private void ReInspection()
        {
            if (string.IsNullOrEmpty(_modelParam.strExposeInc))
            {
                //if (_OnMessage != null)
                //    _OnMessage("Set the exposure value for the re-examination camera.", Color.Red, false, false);
            }
            else
            {
                string[] strExpose = _modelParam.strExposeInc.Split(',');
                double.TryParse(strExpose[nGrabCnt - 1], out double dExpose);
                _camSet.Grab(_nIdx, dExpose);
            }
        }

        private void OnInspectionGraphic(List<CogPointMarker> cogPoint, List<CogLine> cogLine, List<CogCircle> cogCircle, List<CogCompositeShape> cogShape, List<CogEllipse> cogEllipse, List<ICogRegion> cogResRegion, List<CogCompositeShape> cogBlobShape, string[] strData)
        {
            List<CogPointMarker> cogPoints = cogPoint;
            List<CogLine> cogLines = cogLine;
            List<CogCircle> cogCircles = cogCircle;
            List<CogCompositeShape> cogShapes = cogShape;
            List<CogCompositeShape> cogBlobShapes = cogBlobShape;
            List<CogEllipse> cogEllipses = cogEllipse;
            List<ICogRegion> cogRegion = cogResRegion;

            try
            {
                if (cogDisp.Image != null)
                {
                    if (cogRegion != null)
                    {
                        for (int i = 0; i < cogRegion.Count; i++)
                        {
                            if (cogRegion[i] != null)
                            {
                                cogDisp.InteractiveGraphics.Add(cogRegion[i] as ICogGraphicInteractive, "Region", false);
                                cogResDisp.InteractiveGraphics.Add(cogRegion[i] as ICogGraphicInteractive, "Region", false);
                            }
                        }
                    }

                    if (cogPoints != null)
                    {
                        for (int i = 0; i < cogPoints.Count; i++)
                        {
                            if (cogPoints[i] != null)
                            {
                                cogDisp.InteractiveGraphics.Add(cogPoints[i], "Point", false);
                                cogResDisp.InteractiveGraphics.Add(cogPoints[i], "Point", false);
                            }
                        }
                    }

                    if (cogLines != null)
                    {
                        for (int i = 0; i < cogLines.Count; i++)
                        {
                            if (cogLines[i] != null)
                            {
                                cogDisp.InteractiveGraphics.Add(cogLines[i], "Line", false);
                                cogResDisp.InteractiveGraphics.Add(cogLines[i], "Line", false);
                            }
                        }
                    }

                    if (cogCircles != null)
                    {
                        for (int i = 0; i < cogCircles.Count; i++)
                        {
                            if (cogCircles[i] != null)
                            {
                                cogDisp.InteractiveGraphics.Add(cogCircles[i], "Circle", false);
                                cogResDisp.InteractiveGraphics.Add(cogCircles[i], "Circle", false);
                            }
                        }
                    }

                    if (cogShapes != null)
                    {
                        for (int i = 0; i < cogShapes.Count; i++)
                        {
                            if (cogShapes[i] != null)
                            {
                                cogDisp.InteractiveGraphics.Add(cogShapes[i], "Shape", false);
                                cogResDisp.InteractiveGraphics.Add(cogShapes[i], "Shape", false);
                            }
                        }
                    }

                    if (cogBlobShapes != null)
                    {
                        for (int i = 0; i < cogBlobShapes.Count; i++)
                        {
                            if (cogBlobShapes[i] != null)
                            {
                                cogDisp.InteractiveGraphics.Add(cogBlobShapes[i], "Shape", false);
                                cogResDisp.InteractiveGraphics.Add(cogBlobShapes[i], "Shape", false);
                            }
                        }
                    }

                    if (cogEllipses != null)
                    {
                        for (int i = 0; i < cogEllipses.Count; i++)
                        {
                            if (cogEllipses[i] != null)
                            {
                                cogDisp.InteractiveGraphics.Add(cogEllipses[i], "Ellipses", false);
                                cogResDisp.InteractiveGraphics.Add(cogEllipses[i], "Ellipses", false);
                            }
                        }
                    }


                    Delay(200);
                    SaveListImg();

                    //if (_var._bOriginImageSave)
                    //{
                    //    SaveOriginImage();
                    //}

                    //if (_var._bResultImageSave)
                    //{
                    //    SaveResultImage();
                    //}

                    _bUpdate = true;
                }
            }
            catch { }


        }

        private void OnInspRegionGraphic(ICogGraphic[] Region)
        {
            try
            {
                for (int i = 0; i < Region.Length; i++)     // 검사 영역 그려주고
                {
                    cogDisp.StaticGraphics.Add(Region[i], "Region");
                    cogResDisp.StaticGraphics.Add(Region[i], "Region");
                    //cogDisp.InteractiveGraphics.Add(Region[i] as ICogGraphicInteractive, "Region", false);
                    //cogResDisp.InteractiveGraphics.Add(Region[i] as ICogGraphicInteractive, "Region", false);
                }
            }
            catch { }
        }

        public void SaveOriginImage(bool TotalResult)
        {
            try
            {
                bool bRes = _bInspRes;
                string strTotalResult = "";
                string strRes = bRes ? "OK" : "NG";
                string strInspDate = _dateTime.ToString("yyyyMMdd");
                string strInspTime = _dateTime.ToString("HHmmss");

                if (TotalResult)
                    strTotalResult = "OK";
                else
                    strTotalResult = "NG";

                string strPath = _var._strSaveImagePath + "\\OriginImage\\" + strInspDate.Substring(0, 4) + "\\" + strInspDate.Substring(4, 2) + "\\" + strInspDate.Substring(6, 2) + "\\" + strTotalResult + "\\"
                    + _var._strModelName + "_" + _var._strLotNo;

                DirectoryInfo dr = new DirectoryInfo(strPath);
                if (!dr.Exists)
                    dr.Create();

                if (cogResDisp.Image != null)
                {
                    ICogImage cogImg = cogResDisp.Image;

                    if (_var._nOriginImageFormat == 0) // bmp
                    {
                        CogImageFileBMP cogBMP = new CogImageFileBMP();
                        cogBMP.Open(strPath + "\\" + strRes + "_Cam" + string.Format("{0:D2}", (_nIdx + 1)) + "_" + _var._strModelName + "_" + _var._strLotNo + "_" + strInspTime + ".bmp", CogImageFileModeConstants.Write);
                        cogBMP.Append(cogImg);
                        cogBMP.Close();
                        cogBMP.Dispose();
                    }
                    else
                    {
                        CogImageFileJPEG cogJPG = new CogImageFileJPEG();
                        cogJPG.Open(strPath + "\\" + strRes + "_Cam" + string.Format("{0:D2}", (_nIdx + 1)) + "_" + _var._strModelName + "_" + _var._strLotNo + "_" + strInspTime + ".jpg", CogImageFileModeConstants.Write);
                        cogJPG.Append(cogImg);
                        cogJPG.Close();
                        cogJPG.Dispose();
                    }
                }

            }
            catch { }
        }

        public void ShowNGPopup(bool show)
        {
            try
            {
                string MasterPath = "";
                string ImagePath = "";
                string Code = "";
                string strInspDate = _dateTime.ToString("yyyyMMdd");
                string strInspTime = _dateTime.ToString("HHmmss");

                Invoke(new EventHandler(delegate
                {
                    ImagePath = _var._strSaveResultImagePath + "\\ResultImage\\" + strInspDate.Substring(0, 4) + "\\" + strInspDate.Substring(4, 2) + "\\" + strInspDate.Substring(6, 2) + "\\NG\\" + _var._strModelName + "_" + _var._strLotNo;
                    if (_var._nResultImageFormat == 0)
                        ImagePath += "\\NG_Cam" + string.Format("{0:D2}", (_nIdx + 1)) + "_" + _var._strModelName + "_" + _var._strLotNo + "_" + strInspTime + ".bmp";
                    else
                        ImagePath += "\\NG_Cam" + string.Format("{0:D2}", (_nIdx + 1)) + "_" + _var._strModelName + "_" + _var._strLotNo + "_" + strInspTime + ".jpg";
                    Code = ini.ReadIniFile("Code", "Value", _var._strModelPath + "\\" + _var._strModelName, string.Format("CAM{0}.ini", _nIdx + 1));
                    MasterPath = _var._strMasterImagePath + "\\Camera_" + string.Format("{0:D2}", (_nIdx + 1)) + "\\" + Code + ".bmp";
                    if (show)
                    {
                        _ngPopup.Show();
                        _ngPopup.NGPopImage(MasterPath, ImagePath);
                        _ngPopup.Focus();
                    }
                    else
                        _ngPopup.Hide();
                }));
            }
            catch (Exception ex)
            {

            }
        }
        public void SaveResultImage(bool TotalResult)
        {
            try
            {
                bool bRes = _bInspRes;
                string strRes = bRes ? "OK" : "NG";
                string strTotalResult = "";
                //_dateTime = DateTime.Now;
                string strInspDate = _dateTime.ToString("yyyyMMdd");
                string strInspTime = _dateTime.ToString("HHmmss");   //센서만 시간을 거슬러 간다 ?
                if (TotalResult)
                    strTotalResult = "OK";
                else
                    strTotalResult = "NG";

                string strPath = _var._strSaveResultImagePath + "\\ResultImage\\" + strInspDate.Substring(0, 4) + "\\" + strInspDate.Substring(4, 2) + "\\" + strInspDate.Substring(6, 2) + "\\" + strTotalResult + "\\" + _var._strModelName + "_" + _var._strLotNo;
                DirectoryInfo dr = new DirectoryInfo(strPath);
                if (!dr.Exists)
                    dr.Create();


                if (cogResDisp.Image != null)
                {
                    CogImage24PlanarColor cogSaveImg = new CogImage24PlanarColor((Bitmap)cogResDisp.CreateContentBitmap(Cognex.VisionPro.Display.CogDisplayContentBitmapConstants.Display));

                    if (_var._nResultImageFormat == 0) // bmp
                    {
                        CogImageFileBMP cogBMP = new CogImageFileBMP();

                        cogBMP.Open(strPath + "\\" + strRes + "_Cam" + string.Format("{0:D2}", (_nIdx + 1)) + "_" + _var._strModelName + "_" + _var._strLotNo + "_" + strInspTime + ".bmp", CogImageFileModeConstants.Write);
                        cogBMP.Append(cogSaveImg);
                        cogBMP.Close();
                        cogBMP.Dispose();
                    }
                    else
                    {
                        CogImageFileJPEG cogJPG = new CogImageFileJPEG();
                        cogJPG.Open(strPath + "\\" + strRes + "_Cam" + string.Format("{0:D2}", (_nIdx + 1)) + "_" + _var._strModelName + "_" + _var._strLotNo + "_" + strInspTime + ".jpg", CogImageFileModeConstants.Write);
                        cogJPG.Append(cogSaveImg);
                        cogJPG.Close();
                        cogJPG.Dispose();
                    }

                    cogSaveImg.Dispose();
                    cogSaveImg = null;
                }
            }
            catch { }

        }

        private void DeleteOriginImage()
        {
            //드라이브 용량

        }

        private void DeleteResultImage()
        {
            //드라이브 용량 


        }
        private void btnMoveLeft_Click(object sender, EventArgs e)
        {
            if (fypnl.IsPopupOpen)
                fypnl.HideBeakForm();

            if (_OnPositionChange != null)
                _OnPositionChange(_nPos, GlovalVar.PositionType.Previous);
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            if (fypnl.IsPopupOpen)
                fypnl.HideBeakForm();

            if (_OnPositionChange != null)
                _OnPositionChange(_nPos, GlovalVar.PositionType.Next);
        }

        private void btnChk_Click(object sender, EventArgs e)
        {
            //if (_bSensor) return;

            if (_var._bUseSensor == true)
            {
                if (_nIdx == 1)
                {
                    if (!flySensorChk.IsPopupOpen)
                        flySensorChk.ShowBeakForm();
                    else
                        flySensorChk.HideBeakForm();
                }
                else
                {
                    if (!flyChk.IsPopupOpen)
                        flyChk.ShowBeakForm();
                    else
                        flyChk.HideBeakForm();
                }
            }
            else
            {
                if (!flyChk.IsPopupOpen)
                    flyChk.ShowBeakForm();
                else
                    flyChk.HideBeakForm();
            }
        }

        private void btnChkClose_Click(object sender, EventArgs e)
        {
            if (flyChk.IsPopupOpen)
                flyChk.HideBeakForm();
        }

        private void chkAll_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAll.Checked)
            {
                for (int i = 0; i < _chkUse.Length; i++)
                    _chkUse[i].Checked = true;
            }
            else
            {
                for (int i = 0; i < _chkUse.Length; i++)
                    _chkUse[i].Checked = false;
            }
        }

        private void btnChkSave_Click(object sender, EventArgs e)
        {
            try
            {
                _graphicParam = null;
                _graphicParam = new GlovalVar.GraphicParam[_chkUse.Length];
                for (int i = 0; i < _chkUse.Length; i++)
                {
                    if (_graphicParam[i].bUse != null)
                    {
                        _graphicParam[i].bUse = _chkUse[i].Checked;
                        ini.WriteIniFile(string.Format("GraphicUse{0}", i + 1), "Value", _chkUse[i].Checked.ToString(), _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1));
                    }
                    if (_graphicParam[i].nColor != null)
                    {
                        if (_cbColor[i] != null)
                        {
                            _graphicParam[i].nColor = _cbColor[i].SelectedIndex;
                            ini.WriteIniFile(string.Format("GraphicColor{0}", i + 1), "Value", _cbColor[i].SelectedIndex.ToString(), _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1));
                        }
                    }
                    if (_graphicParam[i].nColor2 != null)
                    {
                        if (_cbColor2[i] != null)
                        {
                            _graphicParam[i].nColor2 = _cbColor2[i].SelectedIndex;
                            ini.WriteIniFile(string.Format("GraphicColor2{0}", i + 1), "Value", _cbColor2[i].SelectedIndex.ToString(), _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1));
                        }
                    }
                    if (_graphicParam[i].nLineThick != null)
                    {
                        if (_cbLine[i] != null)
                        {
                            _graphicParam[i].nLineThick = _cbLine[i].SelectedIndex;
                            ini.WriteIniFile(string.Format("GraphicLine{0}", i + 1), "Value", _cbLine[i].SelectedIndex.ToString(), _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1));
                        }
                    }
                }

                _graphicResParam.nAlign = cbAlign.SelectedIndex;
                int.TryParse(txtGraphicSize.Text, out _graphicResParam.nFontSize);
                int.TryParse(txtGraphicX.Text, out _graphicResParam.nWidth);
                int.TryParse(txtGraphicY.Text, out _graphicResParam.nHeight);
                int.TryParse(txtFont.Text, out _graphicResParam.nFont);
                int.TryParse(txtPosY.Text, out _graphicResParam.nPosY);
                int.TryParse(txt2DGraphicX.Text, out _graphicResParam.n2DWidth);
                int.TryParse(txt2DGraphicY.Text, out _graphicResParam.n2DHeight);

                ini.WriteIniFile("ResAlign", "Value", cbAlign.SelectedIndex.ToString(), _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1));
                ini.WriteIniFile("FontSize", "Value", txtGraphicSize.Text, _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1));
                ini.WriteIniFile("ResGraphicX", "Value", txtGraphicX.Text, _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1));
                ini.WriteIniFile("ResGraphicY", "Value", txtGraphicY.Text, _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1));
                ini.WriteIniFile("Font", "Value", txtFont.Text, _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1));
                ini.WriteIniFile("GraphicY", "Value", txtPosY.Text, _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1));
                ini.WriteIniFile("Res2DGraphicX", "Value", txt2DGraphicX.Text, _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1));
                ini.WriteIniFile("Res2DGraphicY", "Value", txt2DGraphicY.Text, _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1));

                if (_OnMessage != null)
                    _OnMessage("Parameter Saved", Color.GreenYellow, true, false);
            }
            catch (Exception ex)
            {
                _OnMessage("Graphic Result Param Save Error : " + ex.Message, Color.Red, false, false);
            }
        }

        private void btnCamExpose_Click(object sender, EventArgs e)
        {
            if (_bSensor)
                return;

            if (!_camSet._bConnect)
            {
                MessageBox.Show("카메라가 연결되지 않았습니다.", "Camera Disconnected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!flyChk.IsPopupOpen)
            {
                double[] dValue = _camSet.GetCamParam();

                if (_camSet._nCamType == 0)
                {
                    spExpose.Text = string.Format("{0:F1}", dValue[0]);
                    spBright.Text = string.Format("{0:F3}", dValue[1]);
                    spContract.Text = string.Format("{0:F3}", dValue[2]);

                    BarExpose.Properties.Minimum = 0;
                    BarExpose.Properties.Maximum = 1000;

                    BarExpose.Value = (int)(dValue[0] * 10.0);
                }
                else
                {
                    spExpose.Text = string.Format("{0:D}", (int)dValue[0]);
                    spBright.Text = string.Format("{0:D}", (int)dValue[1]);
                    spContract.Text = string.Format("{0:D}", (int)dValue[2]);

                    BarExpose.Properties.Minimum = (int)dValue[3];
                    BarExpose.Properties.Maximum = (int)dValue[4];
                }

                fypnl.HidePopup();
                flyExpose.ShowPopup();
            }
        }

        private void btnexloseClose_Click(object sender, EventArgs e)
        {
            flyExpose.HidePopup();
            fypnl.ShowPopup();
        }

        private void BarExpose_EditValueChanged(object sender, EventArgs e)
        {
            int nValue = BarExpose.Value;
            decimal dValue = (decimal)(nValue * 0.1);

            spExpose.Text = string.Format("{0:F1}", dValue);
            //_modelParam.dExpose = (double)dValue;
            _camSet.SetCamParam(0, (double)dValue);
        }

        private void BarBrightness_EditValueChanged(object sender, EventArgs e)
        {
            int nValue = BarBrightness.Value;
            decimal dValue = (decimal)(nValue * 0.001);

            spBright.Text = string.Format("{0:F3}", dValue);
        }

        private void BarConstract_EditValueChanged(object sender, EventArgs e)
        {
            int nValue = BarConstract.Value;
            decimal dValue = (decimal)(nValue * 0.001);

            spContract.Text = string.Format("{0:F3}", dValue);
        }

        private void spExpose_EditValueChanged(object sender, EventArgs e)
        {
            double dValue = (double)spExpose.Value;
            BarExpose.Value = (int)(dValue * 10);
        }

        private void spBright_EditValueChanged(object sender, EventArgs e)
        {
            double dValue = (double)spBright.Value;
            _camSet.SetCamParam((int)GlovalVar.CamParamType.Brightness, dValue);
        }

        private void spContract_EditValueChanged(object sender, EventArgs e)
        {
            double dValue = (double)spContract.Value;
            _camSet.SetCamParam((int)GlovalVar.CamParamType.Constrast, dValue);
        }

        private void chkPass_CheckedChanged(object sender, EventArgs e)
        {
            if (chkPass.Checked)
            {
                chkCamUse.Checked = false;

                _bPassMode = chkPass.Checked;

                lblConnect.BackColor = Color.Yellow;
                chkPass.ForeColor = Color.Yellow;
            }
            else
            {
                chkPass.ForeColor = Color.White;
                _bPassMode = chkPass.Checked;

                if (_camSet._bConnect)
                    lblConnect.BackColor = Color.Lime;
                else
                    lblConnect.BackColor = Color.Red;
            }
        }

        private void chkCamUse_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btmImgList_Click(object sender, EventArgs e)
        {
            if (_OnResultImageList != null)
            {
                fypnl.HidePopup();

                _bUpdate = true;
                _OnResultImageList(_nIdx);
            }
            //if (_bpopup)
            //    _ngPopup.Show();
        }

        private void Pnl_Paint(object sender, PaintEventArgs e)
        {
            if (!_bLoad)
                return;

            if (cogDisp.Image == null)
                return;

            if (_nRes == -1)
                return;

            base.OnPaint(e);
            int borderWidth = 2;
            Color borderColor = Color.Lime;
            if (_nRes == 2)
                borderColor = Color.Red;
            else if (_nRes == -1)
                borderColor = Color.FromArgb(38, 38, 38);

            ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle, borderColor, borderWidth, ButtonBorderStyle.Solid, borderColor, borderWidth,
            ButtonBorderStyle.Solid, borderColor, borderWidth, ButtonBorderStyle.Solid,
            borderColor, borderWidth, ButtonBorderStyle.Solid);
        }

        private void chkCamUse_Click(object sender, EventArgs e)
        {
            if (!chkCamUse.Checked)
            {
                lblConnect.BackColor = Color.LightGray;
                chkCamUse.ForeColor = Color.Yellow;
            }
            else
            {
                chkCamUse.ForeColor = Color.White;

                if (_camSet._bConnect)
                    lblConnect.BackColor = Color.Lime;
                else
                    lblConnect.BackColor = Color.Red;
            }

            _bCamUse = !chkCamUse.Checked;
            ini.WriteIniFile("CamUse", "Value", (!chkCamUse.Checked).ToString(), _var._strModelPath + "\\" + _var._strModelName, string.Format("CameraGraphicParam{0}.ini", _nIdx + 1));
        }
        public void lblStatus_Change(int i)
        {
            Invoke(new EventHandler(delegate
            {
                switch (i)
                {
                    case 0:
                        lblStatus.Text = "이미지 취득 중";
                        break;
                    case 1:
                        lblStatus.Text = "검사 중";
                        break;
                    case 2:
                        lblStatus.Text = " ";
                        break;
                    case 3:
                        lblStatus.Text = "검사 오류";
                        break;
                    case 4:
                        lblStatus.Text = "Tool 오류";
                        break;
                }
            }));
        }
        private void btnSensorChkSave_Click(object sender, EventArgs e)
        {
            try
            {
                _graphicParam = null;
                _graphicParam = new GlovalVar.GraphicParam[_chkUse.Length];
                for (int i = 0; i < 6; i++)
                {
                    if (_graphicParam[i].bUse != null)
                    {
                        _graphicParam[i].bUse = _chkUse[i].Checked;
                        ini.WriteIniFile(string.Format("GraphicUse{0}", i + 1), "Value", _chkUse[i].Checked.ToString(), _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1));
                    }
                    if (_graphicParam[i].nPosX != null)
                    {
                        if (_txtPoisionX[i] != null)
                        {
                            _graphicParam[i].nPosX = Convert.ToInt32(_txtPoisionX[i].Text);
                            ini.WriteIniFile(string.Format("GraphicPosX{0}", i + 1), "Value", _txtPoisionX[i].Text, _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1));
                        }
                    }
                    if (_graphicParam[i].nPosY != null)
                    {
                        if (_txtPoisionY[i] != null)
                        {
                            _graphicParam[i].nPosY = Convert.ToInt32(_txtPoisionY[i].Text);
                            ini.WriteIniFile(string.Format("GraphicPosY{0}", i + 1), "Value", _txtPoisionY[i].Text, _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1));
                        }
                    }
                }

                _graphicResParam.nAlign = cbSensorAlign.SelectedIndex;
                int.TryParse(txtSensorGraphicSize.Text, out _graphicResParam.nFontSize);
                int.TryParse(txtSensorGraphicX.Text, out _graphicResParam.nWidth);
                int.TryParse(txtSensorGraphicY.Text, out _graphicResParam.nHeight);
                int.TryParse(txtSensorFont.Text, out _graphicResParam.nFont);
                int.TryParse(txtSensor2DGraphicX.Text, out _graphicResParam.n2DWidth);
                int.TryParse(txtSensor2DGraphicY.Text, out _graphicResParam.n2DHeight);

                ini.WriteIniFile("ResAlign", "Value", cbSensorAlign.SelectedIndex.ToString(), _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1));
                ini.WriteIniFile("FontSize", "Value", txtSensorGraphicSize.Text, _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1));
                ini.WriteIniFile("ResGraphicX", "Value", txtSensorGraphicX.Text, _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1));
                ini.WriteIniFile("ResGraphicY", "Value", txtSensorGraphicY.Text, _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1));
                ini.WriteIniFile("Font", "Value", txtSensorFont.Text, _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1));
                ini.WriteIniFile("Res2DGraphicX", "Value", txtSensor2DGraphicX.Text, _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1));
                ini.WriteIniFile("Res2DGraphicY", "Value", txtSensor2DGraphicY.Text, _var._strModelPath + "\\" + _var._strModelName, string.Format("SensorGraphicParam{0}.ini", _nIdx + 1));

                if (_OnMessage != null)
                    _OnMessage("Parameter Saved", Color.GreenYellow, true, false);
            }
            catch (Exception ex)
            {
                _OnMessage("Graphic Result Param Save Error : " + ex.Message, Color.Red, false, false);
            }
        }

        private void btnSensorChkClose_Click(object sender, EventArgs e)
        {
            if (flySensorChk.IsPopupOpen)
                flySensorChk.HideBeakForm();
        }

        private void scSensorBar_Click(object sender, EventArgs e)
        {

        }
    }
}
