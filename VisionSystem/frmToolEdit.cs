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
using System.Threading;
using System.IO;

using Cognex.VisionPro;
using Cognex.VisionPro.ToolGroup;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.Dimensioning;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.Blob;
using Cognex.VisionPro.ColorMatch;
using Cognex.VisionPro.ColorExtractor;
using Cognex.VisionPro.ColorSegmenter;
using Cognex.VisionPro.CompositeColorMatch;
using Cognex.VisionPro.OCRMax;
using Cognex.VisionPro.OCVMax;
using Cognex.VisionPro.LineMax;
using Cognex.VisionPro.ResultsAnalysis;
using Cognex.VisionPro.ID;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.CNLSearch;
using Cognex.VisionPro.Display;

namespace VisionSystem
{
    public partial class frmToolEdit : DevExpress.XtraEditors.XtraForm
    {
        CAM _cam;
        int _nIdx = 0;
        int _index = 0;

        ToolEdit _toolEdit = new ToolEdit();

        IniFiles ini = new IniFiles();
        CogImageConvertTool _CogImageConvertTool = null;
        SettingInspection _setting = new SettingInspection();

        int _nCurrentIndx = -1;
        bool _bLoad = false;

        Font font = new Font("굴림", 9, FontStyle.Bold);

        ToolType _toolType = ToolType.Light;

        CogDisplay[] _cogDisp = new CogDisplay[20];
        Panel[] _Pnl = new Panel[20];

        bool _bSave = false;

        int _nDispNo = -1;
        int _nImgListCnt = 0;

        int _nWidth = 0;
        int _nHeight = 0;
        int _iFilmCount = 0;
        int _iImageCount = 0;

        CogDataAnalysisTool _DataAnalysisTool = new CogDataAnalysisTool();
        OpenFileDialog ofd = new OpenFileDialog();

        private enum PanelType
        {
            ToolEdit,
            ToolDetail,
            ToolGroup
        }

        private enum ToolRes
        {
            Green,
            Red,
            Gray,
            Yellow
        }

        private enum ControlType
        {
            PMAlign,
            Blob
        }

        public frmToolEdit(CAM cam, int index, int nIdx)
        {
            InitializeComponent();
            _cam = cam;
            _index = index;
            _nIdx = nIdx;

            lblDate.Text = "-";
            _bSave = false;

            Delay(200);

            for (int i = 0; i < 10; i++)
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

            this.ResizeRedraw = true;
        }


        private void GetFileLastWriteTime()
        {
            DateTime dt = File.GetLastWriteTime(string.Format("{0}\\vpp\\Camera_{1:D2}\\{2}.vpp", Application.StartupPath, _nIdx + 1, lblToolName.Text));
            lblDate.Text = dt.ToString("yy-MM-dd HH:mm:ss");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FormClose()
        {
            try
            {

            }
            catch { }
        }

        public void LoadSet(ICogImage cogImg, int _iCamNum, int _iVppNum)
        {
            try
            {
                _nCurrentIndx = -1;
                string[] strName = typeof(ToolEdit.ToolList).GetEnumNames();
                for (int i = 0; i < strName.Length; i++)
                {
                    tabToolList.TabPages[i].Text = strName[i];
                    tabToolList.TabPages[i].PageVisible = false;
                }
                lblCamName.Text = string.Format("{0:D2}", Convert.ToInt32(_iCamNum));
                lblToolName.Text = string.Format("{0:D2}", Convert.ToInt32(_iVppNum));

                _setting.VJoblode(string.Format("{0}\\vpp\\Camera_{1:D2}\\{2:D2}.vpp", Application.StartupPath, _iCamNum, _iVppNum), (_iVppNum).ToString());
                SetControl();
                _nCurrentIndx = _iVppNum;
                cogToolGroup.Subject = _setting.GetJob;

                GetFileLastWriteTime();
                ToolDisp.Tool = null;

                ToolLoading(cogImg);

                _setting.GetJob.Run();
                RunStatus();

                btnAllTool.PerformClick();
                btnAllTool.PerformClick();

                Thread threadShowPopup = new Thread(ShowDisp);
                threadShowPopup.Start();

                _bSave = false;
            }
            catch (Exception ex)
            {
                _cam._OnMessage("ToolEdit Load Error : " + ex.Message, Color.Red, false, false);
            }
        }

        private void ShowDisp()
        {
            Delay(1000);
            _toolType = ToolType.Light;
            ToolLoad(_nCurrentIndx, _toolType);
            ShowPopup(true);
        }

        private void ToolLoading(ICogImage cogImg)
        {
            if (_setting.GetJob.Tools.Count == 0)
                return;

            for (var i = 0; i < _setting.GetJob.Tools.Count; i++)
            {
                var nIdx = i;
                if (_setting.GetJob.Tools[nIdx].GetType() == typeof(CogImageConvertTool))
                {
                    _nCurrentIndx = nIdx;
                    _CogImageConvertTool = _setting.GetJob.Tools[nIdx] as CogImageConvertTool;

                    if (cogImg != null)
                    {
                        _CogImageConvertTool.InputImage = cogImg;

                        cogInspImg.AutoFit = true;
                        cogInspImg.Image = cogImg;
                    }
                    else
                    {
                        FileInfo fi = new FileInfo(string.Format("{0}\\Camera_{1:D2}\\{2}.bmp", _cam._var._strMasterImagePath, _index + 1, _cam._modelParam.strCode));

                        if (fi.Exists)
                        {
                            using (Bitmap bmpImg = new Bitmap(Bitmap.FromFile(fi.FullName)))
                            {
                                if (_CogImageConvertTool != null)
                                {
                                    _CogImageConvertTool.InputImage = new CogImage24PlanarColor(bmpImg);
                                    cogInspImg.AutoFit = true;
                                    cogInspImg.Image = _CogImageConvertTool.InputImage;
                                }
                            }
                        }
                    }
                    Thread threadLoad = new Thread(() => imageConvertControl.LoadSet(_CogImageConvertTool));
                    threadLoad.Start();
                    Delay(100);
                }
            }
        }

        private void SetControl()
        {
            listcontrol.Items.Clear();

            try
            {
                if (_setting.GetJob == null)
                {
                    MessageBox.Show("The task file was not loaded", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var nCnt = _setting.GetJob.Tools.Count;
                var nListCnt = 0;

                for (int i = 0; i < nCnt; i++)
                {
                    if (!_setting.GetJob.Tools[i].Name.Contains("Source"))
                    {
                        listcontrol.Items.Add(_setting.GetJob.Tools[i].Name);
                        listcontrol.Items[nListCnt++].ImageOptions.ImageIndex = (int)ToolRes.Gray;
                    }
                }
            }
            catch (Exception ex)
            {
                _cam._OnMessage("ToolEdit Init Error : " + ex.Message, Color.Red, false, false);
            }
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

        private void InitImgList()
        {
            for (var i = 0; i < 10; i++)
                _cogDisp[i].Image = null;
        }

        private void btnOpenImg_Click(object sender, EventArgs e)
        {


            ofd.Multiselect = true;
            ofd.Title = "Image Open";
            ofd.InitialDirectory = _cam._var._strSaveImagePath;
            ofd.Filter = "Image File (*.bmp,*.jpg,*.png) | *.bmp;*.jpg;*.png; | All Files (*.*) | *.*";

            //FolderBrowserDialog _fbd = new FolderBrowserDialog();

            //if (_fbd.ShowDialog() == DialogResult.OK)
            //{
            //    DirectoryInfo _di = new DirectoryInfo(_fbd.SelectedPath);

            //    splashScreenManager1.ShowWaitForm();

            //    string index_Data = string.Format("*_Cam{0}*_{1:D2}_*", _index + 1, _cam._modelParam.strCode);

            //    string[] _strPath = Directory.GetFiles(_fbd.SelectedPath, index_Data,SearchOption.AllDirectories);

            //    _nDispNo = -1;
            //    InitImgList();
            //    _nDispNo = 0;
            //    _nImgListCnt = 0;

            //    for (var i = 0; i < _strPath.Length; i++)
            //    {
            //        using (FileStream fs = new FileStream(_strPath[i], FileMode.Open, FileAccess.Read, FileShare.Read))
            //        {
            //            _cogDisp[i].AutoFit = true;
            //            _cogDisp[i].Image = new CogImage24PlanarColor(new Bitmap(Image.FromStream(fs)));
            //        }

            //        _nImgListCnt++;

            //        if (_nImgListCnt == 20)
            //            break;
            //    }

            //    _Pnl[_nDispNo].Focus();

            //    for (var i = 0; i < 20; i++)
            //        _Pnl[i].Invalidate();

            //    string strMsg = "";

            //    try
            //    {
            //        if (_CogImageConvertTool != null)
            //        {
            //            _CogImageConvertTool.InputImage = _cogDisp[_nDispNo].Image;
            //            cogInspImg.AutoFit = true;
            //            cogInspImg.Image = _CogImageConvertTool.InputImage;
            //            JobRun();
            //            splashScreenManager1.CloseWaitForm();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        splashScreenManager1.CloseWaitForm();
            //        _cam._OnMessage("Image Open Error : " + ex.Message, Color.Red, false, false);
            //    }

            //    pnlImage.Visible = true;
            //}


            if (ofd.ShowDialog() == DialogResult.OK)
            {
                splashScreenManager1.ShowWaitForm();

                _nDispNo = -1;
                InitImgList();
                _nDispNo = 0;
                _nImgListCnt = 0;
                _iFilmCount = 0;

                for (var i = 0; i < ofd.FileNames.Length; i++)
                {
                    using (FileStream fs = new FileStream(ofd.FileNames[i], FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        _cogDisp[i].AutoFit = true;
                        _cogDisp[i].Image = new CogImage24PlanarColor(new Bitmap(Image.FromStream(fs)));
                    }

                    _nImgListCnt++;

                    if (_nImgListCnt == 10)
                        break;
                }

                _Pnl[_nDispNo].Focus();

                for (var i = 0; i < 10; i++)
                    _Pnl[i].Invalidate();

                ImageCount(ofd.FileNames.Length, 0);

                string strMsg = "";

                try
                {
                    if (_CogImageConvertTool != null)
                    {
                        _CogImageConvertTool.InputImage = _cogDisp[_nDispNo].Image;
                        cogInspImg.AutoFit = true;
                        cogInspImg.Image = _CogImageConvertTool.InputImage;
                        JobRun();
                        splashScreenManager1.CloseWaitForm();
                    }
                }
                catch (Exception ex)
                {
                    splashScreenManager1.CloseWaitForm();
                    _cam._OnMessage("Image Open Error : " + ex.Message, Color.Red, false, false);
                }

                pnlImage.Visible = true;
            }



        }
        private void stmpList(int _istart)
        {

        }
        private void JobRun()
        {
            try
            {
                if (_setting.GetJob == null)
                    return;

                _setting.GetJob.Run();

                if (_setting.GetJob.RunStatus.Result == CogToolResultConstants.Accept)
                {
                    for (int i = 0; i < listcontrol.ItemCount; i++)
                        listcontrol.Items[i].ImageOptions.ImageIndex = (int)ToolRes.Green;
                }
                else
                {
                    for (int i = 0; i < listcontrol.ItemCount; i++)
                        listcontrol.Items[i].ImageOptions.ImageIndex = (int)ToolRes.Red;

                    for (int i = 0; i < _setting.GetJob.Tools.Count; i++)
                    {
                        if (_setting.GetJob.Tools[i].RunStatus.Result == CogToolResultConstants.Error)
                        {
                            listcontrol.Items[i].ImageOptions.ImageIndex = (int)ToolRes.Red;
                            //MessageBox.Show(string.Format("{0} RunStatus Error : {1}", _setting.GetJob.Tools[i].Name, _setting.GetJob.Tools[i].RunStatus.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        }
                        else if (_setting.GetJob.Tools[i].RunStatus.Result == CogToolResultConstants.Accept)
                            listcontrol.Items[i].ImageOptions.ImageIndex = (int)ToolRes.Green;
                        else if (_setting.GetJob.Tools[i].RunStatus.Result == CogToolResultConstants.Warning)
                            listcontrol.Items[i].ImageOptions.ImageIndex = (int)ToolRes.Yellow;
                    }
                }
            }
            catch (Exception ex)
            {
                _cam._OnMessage("Image Open Error : " + ex.Message, Color.Red, false, false);
            }
        }

        private enum ToolType
        {
            Light,
            Detail,
            All
        }

        private void listcontrol_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                int nSel = listcontrol.SelectedIndex;
                if (nSel == -1)
                    return;

                if (_setting.GetJob == null)
                    return;

                if (_nCurrentIndx == nSel + 1)
                    return;

                if (!splashScreenManager1.IsSplashFormVisible)
                    splashScreenManager1.ShowWaitForm();

                ShowPopup(false);
                Delay(500);

                _nCurrentIndx = nSel + 1;

                _toolType = ToolType.Light;
                ToolLoad(_nCurrentIndx, ToolType.Light);

                ShowPopup(true);
                Delay(500);

                if (splashScreenManager1.IsSplashFormVisible)
                    splashScreenManager1.CloseWaitForm();
            }
            catch { }
        }

        private void ShowPopup(bool bShow)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(delegate
                {
                    ShowPopup(bShow);
                }));
            }
            else
            {
                if (!bShow)
                {
                    flyDetail.HidePopup();
                    flyToolGroup.HidePopup();
                    flyToolEdit.HidePopup();
                }
                else
                {
                    if (_toolType == ToolType.Light)
                        flyToolEdit.ShowPopup();
                    else if (_toolType == ToolType.Detail)
                        flyDetail.ShowPopup();
                    else
                        flyDetail.ShowPopup();
                }
            }
        }
        private void SetToolPage(string strToolnName)
        {
            var nNo = 0;
            var strNames = typeof(ToolEdit.ToolList).GetEnumNames();
            var strTemp = strToolnName.Split('.');
            try
            {
                Invoke(new EventHandler(delegate
                {
                    for (int i = 0; i < strNames.Length; i++)
                    {
                        tabToolList.TabPages[i].PageVisible = false;

                        if (strNames[i].Contains(strTemp[strTemp.Length - 1]))
                            nNo = i;
                    }

                    tabToolList.TabPages[nNo].PageVisible = true;
                }));
            }
            catch { }
        }

        private void ToolLoad(int nSelectNo, ToolType toolType)
        {
            var nSel = nSelectNo;
            ToolType type = toolType;

            _bLoad = false;

            Thread threadSetPage = new Thread(() => SetToolPage(_setting.GetJob.Tools[nSel].GetType().ToString()));
            threadSetPage.Start();
            if (InvokeRequired)
            {
                Invoke(new EventHandler(delegate
               {
                   ToolLoad(nSelectNo, toolType);
               }));
            }
            else
            {

                try
                {
                    pnlDeltailTool.Controls.Clear();

                    if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogImageConvertTool))
                    {
                        ToolDisp.Tool = _setting.GetJob.Tools[nSel] as CogImageConvertTool;

                        if (_toolType == ToolType.Detail)
                        {
                            var cogImageConvertEditV2 = new CogImageConvertEditV2();
                            pnlDeltailTool.Controls.Add(cogImageConvertEditV2);
                            cogImageConvertEditV2.Dock = DockStyle.Fill;
                            cogImageConvertEditV2.Subject = _setting.GetJob.Tools[nSel] as CogImageConvertTool;
                            cogImageConvertEditV2.Font = font;
                            cogImageConvertEditV2.ForeColor = Color.Black;
                        }
                        else
                            imageConvertControl.LoadSet(_setting.GetJob.Tools[nSel] as CogImageConvertTool);
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogIPOneImageTool))
                    {
                        _toolType = ToolType.Detail;
                        var cogIPOneImageEditV2 = new CogIPOneImageEditV2();
                        pnlDeltailTool.Controls.Add(cogIPOneImageEditV2);
                        cogIPOneImageEditV2.Dock = DockStyle.Fill;
                        cogIPOneImageEditV2.Font = font;
                        cogIPOneImageEditV2.ForeColor = Color.Black;
                        cogIPOneImageEditV2.Subject = _setting.GetJob.Tools[nSel] as CogIPOneImageTool;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogBlobTool))
                    {
                        ToolDisp.Tool = _setting.GetJob.Tools[nSel] as CogBlobTool;

                        if (_toolType == ToolType.Detail)
                        {
                            var cogBlobEditV2 = new CogBlobEditV2();
                            pnlDeltailTool.Controls.Add(cogBlobEditV2);
                            cogBlobEditV2.Dock = DockStyle.Fill;
                            cogBlobEditV2.Subject = _setting.GetJob.Tools[nSel] as CogBlobTool;
                            cogBlobEditV2.Font = font;
                            cogBlobEditV2.ForeColor = Color.Black;
                        }
                        else
                            blobControl.LoadSet(_setting.GetJob.Tools[nSel] as CogBlobTool);
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogCaliperTool))
                    {
                        ToolDisp.Tool = _setting.GetJob.Tools[nSel] as CogCaliperTool;

                        if (_toolType == ToolType.Detail)
                        {
                            CogCaliperEditV2 cogCaliperEditV2 = new CogCaliperEditV2();
                            pnlDeltailTool.Controls.Add(cogCaliperEditV2);
                            cogCaliperEditV2.Dock = DockStyle.Fill;
                            cogCaliperEditV2.Font = font;
                            cogCaliperEditV2.ForeColor = Color.Black;
                            cogCaliperEditV2.Subject = _setting.GetJob.Tools[nSel] as CogCaliperTool;
                        }
                        else
                            caliperControl.LoadSet(_setting.GetJob.Tools[nSel] as CogCaliperTool);
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogDataAnalysisTool))
                    {
                        _toolType = ToolType.Detail;

                        CogDataAnalysisEditV2 CogDataAnalysisEditV = new CogDataAnalysisEditV2();
                        pnlDeltailTool.Controls.Add(CogDataAnalysisEditV);
                        CogDataAnalysisEditV.Dock = DockStyle.Fill;
                        CogDataAnalysisEditV.Font = font;
                        CogDataAnalysisEditV.ForeColor = Color.Black;
                        CogDataAnalysisEditV.Subject = _setting.GetJob.Tools[nSel] as CogDataAnalysisTool;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogPMAlignTool))
                    {
                        ToolDisp.Tool = _setting.GetJob.Tools[nSel] as CogPMAlignTool;

                        if (_toolType == ToolType.Detail)
                        {
                            CogPMAlignEditV2 cogPMAlignEditV2 = new CogPMAlignEditV2();
                            pnlDeltailTool.Controls.Add(cogPMAlignEditV2);
                            cogPMAlignEditV2.Dock = DockStyle.Fill;
                            cogPMAlignEditV2.Subject = (_setting.GetJob.Tools[nSel] as CogPMAlignTool);
                        }
                        else
                            pmAlignControl.LoadSet(_setting.GetJob.Tools[nSel] as CogPMAlignTool);
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogResultsAnalysisTool))
                    {
                        _toolType = ToolType.Detail;

                        CogResultsAnalysisEdit cogResultsAnalysisEdit = new CogResultsAnalysisEdit();
                        pnlDeltailTool.Controls.Add(cogResultsAnalysisEdit);
                        cogResultsAnalysisEdit.Font = font;
                        cogResultsAnalysisEdit.ForeColor = Color.Black;
                        cogResultsAnalysisEdit.Dock = DockStyle.Fill;
                        cogResultsAnalysisEdit.Subject = (_setting.GetJob.Tools[nSel] as CogResultsAnalysisTool);
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogCreateCircleTool))
                    {
                        ToolDisp.Tool = _setting.GetJob.Tools[nSel] as CogCreateCircleTool;

                        if (_toolType == ToolType.Detail)
                        {
                            CogCreateCircleEditV2 cogCreateCircleEditV2 = new CogCreateCircleEditV2();
                            pnlDeltailTool.Controls.Add(cogCreateCircleEditV2);
                            cogCreateCircleEditV2.Font = font;
                            cogCreateCircleEditV2.ForeColor = Color.Black;
                            cogCreateCircleEditV2.Dock = DockStyle.Fill;
                            cogCreateCircleEditV2.Subject = (_setting.GetJob.Tools[nSel] as CogCreateCircleTool);
                        }
                        else
                            createCircle.LoadSet(_setting.GetJob.Tools[nSel] as CogCreateCircleTool);
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogCreateEllipseTool))
                    {
                        ToolDisp.Tool = _setting.GetJob.Tools[nSel] as CogCreateEllipseTool;

                        if (_toolType == ToolType.Detail)
                        {
                            CogCreateEllipseEditV2 cogCreateEllipseEditV2 = new CogCreateEllipseEditV2();
                            pnlDeltailTool.Controls.Add(cogCreateEllipseEditV2);
                            cogCreateEllipseEditV2.Dock = DockStyle.Fill;
                            cogCreateEllipseEditV2.Font = font;
                            cogCreateEllipseEditV2.ForeColor = Color.Black;
                            cogCreateEllipseEditV2.Subject = (_setting.GetJob.Tools[nSel] as CogCreateEllipseTool);
                        }
                        else
                            createEllipse.LoadSet(_setting.GetJob.Tools[nSel] as CogCreateEllipseTool);
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogCreateLineTool))
                    {
                        ToolDisp.Tool = _setting.GetJob.Tools[nSel] as CogCreateLineTool;

                        if (_toolType == ToolType.Detail)
                        {
                            CogCreateLineEditV2 cogCreateLineEditV2 = new CogCreateLineEditV2();
                            pnlDeltailTool.Controls.Add(cogCreateLineEditV2);
                            cogCreateLineEditV2.Dock = DockStyle.Fill;
                            cogCreateLineEditV2.Font = font;
                            cogCreateLineEditV2.ForeColor = Color.Black;
                            cogCreateLineEditV2.Subject = (_setting.GetJob.Tools[nSel] as CogCreateLineTool);
                        }
                        else
                            createLine.LoadSet(_setting.GetJob.Tools[nSel] as CogCreateLineTool);
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogCreateSegmentAvgSegsTool))
                    {
                        ToolDisp.Tool = _setting.GetJob.Tools[nSel] as CogCreateSegmentAvgSegsTool;

                        if (_toolType == ToolType.Detail)
                        {
                            CogCreateSegmentAvgSegsEditV2 cogCreateSegmentAvgSegsEditV2 = new CogCreateSegmentAvgSegsEditV2();
                            pnlDeltailTool.Controls.Add(cogCreateSegmentAvgSegsEditV2);
                            cogCreateSegmentAvgSegsEditV2.Dock = DockStyle.Fill;
                            cogCreateSegmentAvgSegsEditV2.Font = font;
                            cogCreateSegmentAvgSegsEditV2.ForeColor = Color.Black;
                            cogCreateSegmentAvgSegsEditV2.Subject = _setting.GetJob.Tools[nSel] as CogCreateSegmentAvgSegsTool;
                        }
                        else
                            createSegmentAvg.LoadSet(_setting.GetJob.Tools[nSel] as CogCreateSegmentAvgSegsTool);
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogCreateSegmentTool))
                    {
                        ToolDisp.Tool = _setting.GetJob.Tools[nSel] as CogCreateSegmentTool;

                        if (_toolType == ToolType.Detail)
                        {
                            CogCreateSegmentEditV2 cogCreateSegmentEditV2 = new CogCreateSegmentEditV2();
                            pnlDeltailTool.Controls.Add(cogCreateSegmentEditV2);
                            cogCreateSegmentEditV2.Dock = DockStyle.Fill;
                            cogCreateSegmentEditV2.Font = font;
                            cogCreateSegmentEditV2.ForeColor = Color.Black;
                            cogCreateSegmentEditV2.Subject = _setting.GetJob.Tools[nSel] as CogCreateSegmentTool;
                        }
                        else
                            createSegment.LoadSet(_setting.GetJob.Tools[nSel] as CogCreateSegmentTool);
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogFindCircleTool))
                    {
                        ToolDisp.Tool = _setting.GetJob.Tools[nSel] as CogFindCircleTool;

                        if (_toolType == ToolType.Detail)
                        {
                            CogFindCircleEditV2 cogFindCircleEditV2 = new CogFindCircleEditV2();
                            pnlDeltailTool.Controls.Add(cogFindCircleEditV2);
                            cogFindCircleEditV2.Dock = DockStyle.Fill;
                            cogFindCircleEditV2.Font = font;
                            cogFindCircleEditV2.ForeColor = Color.Black;
                            cogFindCircleEditV2.Subject = _setting.GetJob.Tools[nSel] as CogFindCircleTool;
                        }
                        else
                            findCircleControl.LoadSet(_setting.GetJob.Tools[nSel] as CogFindCircleTool);
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogFindCornerTool))
                    {
                        ToolDisp.Tool = _setting.GetJob.Tools[nSel] as CogFindCornerTool;

                        if (_toolType == ToolType.Detail)
                        {
                            CogFindCornerEditV2 cogFindCornerEditV2 = new CogFindCornerEditV2();
                            pnlDeltailTool.Controls.Add(cogFindCornerEditV2);
                            cogFindCornerEditV2.Dock = DockStyle.Fill;
                            cogFindCornerEditV2.Font = font;
                            cogFindCornerEditV2.ForeColor = Color.Black;
                            cogFindCornerEditV2.Subject = _setting.GetJob.Tools[nSel] as CogFindCornerTool;
                        }
                        else
                            findCornerControl.LoadSet(_setting.GetJob.Tools[nSel] as CogFindCornerTool);
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogFindEllipseTool))
                    {
                        ToolDisp.Tool = _setting.GetJob.Tools[nSel] as CogFindEllipseTool;

                        if (_toolType == ToolType.Detail)
                        {
                            CogFindEllipseEditV2 cogFindEllipseEditV2 = new CogFindEllipseEditV2();
                            pnlDeltailTool.Controls.Add(cogFindEllipseEditV2);
                            cogFindEllipseEditV2.Dock = DockStyle.Fill;
                            cogFindEllipseEditV2.Font = font;
                            cogFindEllipseEditV2.ForeColor = Color.Black;
                            cogFindEllipseEditV2.Subject = _setting.GetJob.Tools[nSel] as CogFindEllipseTool;
                        }
                        else
                            findEllipseControl.LoadSet(_setting.GetJob.Tools[nSel] as CogFindEllipseTool);
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogFindLineTool))
                    {
                        ToolDisp.Tool = _setting.GetJob.Tools[nSel] as CogFindLineTool;

                        if (_toolType == ToolType.Detail)
                        {
                            CogFindLineEditV2 cogFindLineEditV2 = new CogFindLineEditV2();
                            pnlDeltailTool.Controls.Add(cogFindLineEditV2);
                            cogFindLineEditV2.Dock = DockStyle.Fill;
                            cogFindLineEditV2.Font = font;
                            cogFindLineEditV2.ForeColor = Color.Black;
                            cogFindLineEditV2.Subject = _setting.GetJob.Tools[nSel] as CogFindLineTool;
                        }
                        else
                            findLineControl.LoadSet(_setting.GetJob.Tools[nSel] as CogFindLineTool);
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogFitCircleTool))
                    {
                        _toolType = ToolType.Detail;

                        CogFitCircleEditV2 cogFitCircleEditV2 = new CogFitCircleEditV2();
                        pnlDeltailTool.Controls.Add(cogFitCircleEditV2);
                        cogFitCircleEditV2.Dock = DockStyle.Fill;
                        cogFitCircleEditV2.Font = font;
                        cogFitCircleEditV2.ForeColor = Color.Black;
                        cogFitCircleEditV2.Subject = _setting.GetJob.Tools[nSel] as CogFitCircleTool;

                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogFitEllipseTool))
                    {
                        _toolType = ToolType.Detail;

                        CogFitEllipseEditV2 cogFitEllipseEditV2 = new CogFitEllipseEditV2();
                        pnlDeltailTool.Controls.Add(cogFitEllipseEditV2);
                        cogFitEllipseEditV2.Dock = DockStyle.Fill;
                        cogFitEllipseEditV2.Font = font;
                        cogFitEllipseEditV2.ForeColor = Color.Black;
                        cogFitEllipseEditV2.Subject = _setting.GetJob.Tools[nSel] as CogFitEllipseTool;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogFitLineTool))
                    {
                        _toolType = ToolType.Detail;

                        CogFitLineEditV2 cogFitLineEditV2 = new CogFitLineEditV2();
                        pnlDeltailTool.Controls.Add(cogFitLineEditV2);
                        cogFitLineEditV2.Dock = DockStyle.Fill;
                        cogFitLineEditV2.Font = font;
                        cogFitLineEditV2.ForeColor = Color.Black;
                        cogFitLineEditV2.Subject = _setting.GetJob.Tools[nSel] as CogFitLineTool;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogLineMaxTool))
                    {
                        _toolType = ToolType.Detail;

                        CogLineMaxEditV2 cogLineMaxEditV2 = new CogLineMaxEditV2();
                        pnlDeltailTool.Controls.Add(cogLineMaxEditV2);
                        cogLineMaxEditV2.Dock = DockStyle.Fill;
                        cogLineMaxEditV2.Font = font;
                        cogLineMaxEditV2.ForeColor = Color.Black;
                        cogLineMaxEditV2.Subject = _setting.GetJob.Tools[nSel] as CogLineMaxTool;
                        ToolDisp.Tool = _setting.GetJob.Tools[nSel] as CogLineMaxTool;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogIntersectCircleCircleTool))
                    {
                        _toolType = ToolType.Detail;

                        CogIntersectCircleCircleEditV2 cogIntersectCircleCircleEditV2 = new CogIntersectCircleCircleEditV2();
                        pnlDeltailTool.Controls.Add(cogIntersectCircleCircleEditV2);
                        cogIntersectCircleCircleEditV2.Dock = DockStyle.Fill;
                        cogIntersectCircleCircleEditV2.Font = font;
                        cogIntersectCircleCircleEditV2.ForeColor = Color.Black;
                        cogIntersectCircleCircleEditV2.Subject = _setting.GetJob.Tools[nSel] as CogIntersectCircleCircleTool;
                        ToolDisp.Tool = cogIntersectCircleCircleEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogIntersectLineCircleTool))
                    {
                        _toolType = ToolType.Detail;

                        CogIntersectLineCircleEditV2 cogIntersectLineCircleEditV2 = new CogIntersectLineCircleEditV2();
                        pnlDeltailTool.Controls.Add(cogIntersectLineCircleEditV2);
                        cogIntersectLineCircleEditV2.Dock = DockStyle.Fill;
                        cogIntersectLineCircleEditV2.Font = font;
                        cogIntersectLineCircleEditV2.ForeColor = Color.Black;
                        cogIntersectLineCircleEditV2.Subject = _setting.GetJob.Tools[nSel] as CogIntersectLineCircleTool;
                        ToolDisp.Tool = cogIntersectLineCircleEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogIntersectLineEllipseTool))
                    {
                        _toolType = ToolType.Detail;

                        CogIntersectLineEllipseEditV2 cogIntersectLineEllipseEditV2 = new CogIntersectLineEllipseEditV2();
                        pnlDeltailTool.Controls.Add(cogIntersectLineEllipseEditV2);
                        cogIntersectLineEllipseEditV2.Dock = DockStyle.Fill;
                        cogIntersectLineEllipseEditV2.Font = font;
                        cogIntersectLineEllipseEditV2.ForeColor = Color.Black;
                        cogIntersectLineEllipseEditV2.Subject = _setting.GetJob.Tools[nSel] as CogIntersectLineEllipseTool;
                        ToolDisp.Tool = cogIntersectLineEllipseEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogIntersectLineLineTool))
                    {
                        _toolType = ToolType.Detail;

                        CogIntersectLineLineEditV2 cogIntersectLineLineEditV2 = new CogIntersectLineLineEditV2();
                        pnlDeltailTool.Controls.Add(cogIntersectLineLineEditV2);
                        cogIntersectLineLineEditV2.Dock = DockStyle.Fill;
                        cogIntersectLineLineEditV2.Font = font;
                        cogIntersectLineLineEditV2.ForeColor = Color.Black;
                        cogIntersectLineLineEditV2.Subject = _setting.GetJob.Tools[nSel] as CogIntersectLineLineTool;
                        ToolDisp.Tool = cogIntersectLineLineEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogIntersectSegmentCircleTool))
                    {
                        _toolType = ToolType.Detail;

                        CogIntersectSegmentCircleEditV2 cogIntersectSegmentCircleEditV2 = new CogIntersectSegmentCircleEditV2();
                        pnlDeltailTool.Controls.Add(cogIntersectSegmentCircleEditV2);
                        cogIntersectSegmentCircleEditV2.Dock = DockStyle.Fill;
                        cogIntersectSegmentCircleEditV2.Font = font;
                        cogIntersectSegmentCircleEditV2.ForeColor = Color.Black;
                        cogIntersectSegmentCircleEditV2.Subject = _setting.GetJob.Tools[nSel] as CogIntersectSegmentCircleTool;
                        ToolDisp.Tool = cogIntersectSegmentCircleEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogIntersectSegmentEllipseTool))
                    {
                        _toolType = ToolType.Detail;

                        CogIntersectSegmentEllipseEditV2 cogIntersectSegmentEllipseEditV2 = new CogIntersectSegmentEllipseEditV2();
                        pnlDeltailTool.Controls.Add(cogIntersectSegmentEllipseEditV2);
                        cogIntersectSegmentEllipseEditV2.Dock = DockStyle.Fill;
                        cogIntersectSegmentEllipseEditV2.Font = font;
                        cogIntersectSegmentEllipseEditV2.ForeColor = Color.Black;
                        cogIntersectSegmentEllipseEditV2.Subject = _setting.GetJob.Tools[nSel] as CogIntersectSegmentEllipseTool;
                        ToolDisp.Tool = cogIntersectSegmentEllipseEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogIntersectSegmentLineTool))
                    {
                        _toolType = ToolType.Detail;

                        CogIntersectSegmentLineEditV2 cogIntersectSegmentLineEditV2 = new CogIntersectSegmentLineEditV2();
                        pnlDeltailTool.Controls.Add(cogIntersectSegmentLineEditV2);
                        cogIntersectSegmentLineEditV2.Dock = DockStyle.Fill;
                        cogIntersectSegmentLineEditV2.Font = font;
                        cogIntersectSegmentLineEditV2.ForeColor = Color.Black;
                        cogIntersectSegmentLineEditV2.Subject = _setting.GetJob.Tools[nSel] as CogIntersectSegmentLineTool;
                        ToolDisp.Tool = cogIntersectSegmentLineEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogIntersectSegmentSegmentTool))
                    {
                        _toolType = ToolType.Detail;

                        CogIntersectSegmentSegmentEditV2 cogIntersectSegmentSegmentEditV2 = new CogIntersectSegmentSegmentEditV2();
                        pnlDeltailTool.Controls.Add(cogIntersectSegmentSegmentEditV2);
                        cogIntersectSegmentSegmentEditV2.Dock = DockStyle.Fill;
                        cogIntersectSegmentSegmentEditV2.Font = font;
                        cogIntersectSegmentSegmentEditV2.ForeColor = Color.Black;
                        cogIntersectSegmentSegmentEditV2.Subject = _setting.GetJob.Tools[nSel] as CogIntersectSegmentSegmentTool;
                        ToolDisp.Tool = cogIntersectSegmentSegmentEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogAngleLineLineTool))
                    {
                        _toolType = ToolType.Detail;

                        CogAngleLineLineEditV2 cogAngleLineLineEditV2 = new CogAngleLineLineEditV2();
                        pnlDeltailTool.Controls.Add(cogAngleLineLineEditV2);
                        cogAngleLineLineEditV2.Dock = DockStyle.Fill;
                        cogAngleLineLineEditV2.Font = font;
                        cogAngleLineLineEditV2.ForeColor = Color.Black;
                        cogAngleLineLineEditV2.Subject = _setting.GetJob.Tools[nSel] as CogAngleLineLineTool;
                        ToolDisp.Tool = cogAngleLineLineEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogAnglePointPointTool))
                    {
                        _toolType = ToolType.Detail;

                        CogAnglePointPointEditV2 cogAnglePointPointEditV2 = new CogAnglePointPointEditV2();
                        pnlDeltailTool.Controls.Add(cogAnglePointPointEditV2);
                        cogAnglePointPointEditV2.Dock = DockStyle.Fill;
                        cogAnglePointPointEditV2.Font = font;
                        cogAnglePointPointEditV2.ForeColor = Color.Black;
                        cogAnglePointPointEditV2.Subject = _setting.GetJob.Tools[nSel] as CogAnglePointPointTool;
                        ToolDisp.Tool = cogAnglePointPointEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogDistanceCircleCircleTool))
                    {
                        _toolType = ToolType.Detail;

                        CogDistanceCircleCircleEditV2 cogDistanceCircleCircleEditV2 = new CogDistanceCircleCircleEditV2();
                        pnlDeltailTool.Controls.Add(cogDistanceCircleCircleEditV2);
                        cogDistanceCircleCircleEditV2.Dock = DockStyle.Fill;
                        cogDistanceCircleCircleEditV2.Font = font;
                        cogDistanceCircleCircleEditV2.ForeColor = Color.Black;
                        cogDistanceCircleCircleEditV2.Subject = _setting.GetJob.Tools[nSel] as CogDistanceCircleCircleTool;
                        ToolDisp.Tool = cogDistanceCircleCircleEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogDistanceLineCircleTool))
                    {
                        _toolType = ToolType.Detail;

                        CogDistanceLineCircleEditV2 cogDistanceLineCircleEditV2 = new CogDistanceLineCircleEditV2();
                        pnlDeltailTool.Controls.Add(cogDistanceLineCircleEditV2);
                        cogDistanceLineCircleEditV2.Dock = DockStyle.Fill;
                        cogDistanceLineCircleEditV2.Font = font;
                        cogDistanceLineCircleEditV2.ForeColor = Color.Black;
                        cogDistanceLineCircleEditV2.Subject = _setting.GetJob.Tools[nSel] as CogDistanceLineCircleTool;
                        ToolDisp.Tool = cogDistanceLineCircleEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogDistanceLineEllipseTool))
                    {
                        _toolType = ToolType.Detail;

                        CogDistanceLineEllipseEditV2 cogDistanceLineEllipseEditV2 = new CogDistanceLineEllipseEditV2();
                        pnlDeltailTool.Controls.Add(cogDistanceLineEllipseEditV2);
                        cogDistanceLineEllipseEditV2.Dock = DockStyle.Fill;
                        cogDistanceLineEllipseEditV2.Font = font;
                        cogDistanceLineEllipseEditV2.ForeColor = Color.Black;
                        cogDistanceLineEllipseEditV2.Subject = _setting.GetJob.Tools[nSel] as CogDistanceLineEllipseTool;
                        ToolDisp.Tool = _setting.GetJob.Tools[nSel] as CogDistanceLineEllipseTool;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogDistancePointCircleTool))
                    {
                        _toolType = ToolType.Detail;

                        CogDistancePointCircleEditV2 cogDistancePointCircleEditV2 = new CogDistancePointCircleEditV2();
                        pnlDeltailTool.Controls.Add(cogDistancePointCircleEditV2);
                        cogDistancePointCircleEditV2.Dock = DockStyle.Fill;
                        cogDistancePointCircleEditV2.Font = font;
                        cogDistancePointCircleEditV2.ForeColor = Color.Black;
                        cogDistancePointCircleEditV2.Subject = _setting.GetJob.Tools[nSel] as CogDistancePointCircleTool;
                        ToolDisp.Tool = cogDistancePointCircleEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogDistancePointEllipseTool))
                    {
                        CogDistancePointEllipseEditV2 cogDistancePointEllipseEditV2 = new CogDistancePointEllipseEditV2();
                        pnlDeltailTool.Controls.Add(cogDistancePointEllipseEditV2);
                        cogDistancePointEllipseEditV2.Dock = DockStyle.Fill;
                        cogDistancePointEllipseEditV2.Font = font;
                        cogDistancePointEllipseEditV2.ForeColor = Color.Black;
                        cogDistancePointEllipseEditV2.Subject = _setting.GetJob.Tools[nSel] as CogDistancePointEllipseTool;
                        ToolDisp.Tool = cogDistancePointEllipseEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogDistancePointLineTool))
                    {
                        _toolType = ToolType.Detail;

                        CogDistancePointLineEditV2 cogDistancePointLineEditV2 = new CogDistancePointLineEditV2();
                        pnlDeltailTool.Controls.Add(cogDistancePointLineEditV2);
                        cogDistancePointLineEditV2.Dock = DockStyle.Fill;
                        cogDistancePointLineEditV2.Font = font;
                        cogDistancePointLineEditV2.ForeColor = Color.Black;
                        cogDistancePointLineEditV2.Subject = _setting.GetJob.Tools[nSel] as CogDistancePointLineTool;
                        ToolDisp.Tool = cogDistancePointLineEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogDistancePointPointTool))
                    {
                        _toolType = ToolType.Detail;

                        CogDistancePointPointEditV2 cogDistancePointPointEditV2 = new CogDistancePointPointEditV2();
                        pnlDeltailTool.Controls.Add(cogDistancePointPointEditV2);
                        cogDistancePointPointEditV2.Dock = DockStyle.Fill;
                        cogDistancePointPointEditV2.Font = font;
                        cogDistancePointPointEditV2.ForeColor = Color.Black;
                        cogDistancePointPointEditV2.Subject = _setting.GetJob.Tools[nSel] as CogDistancePointPointTool;
                        ToolDisp.Tool = cogDistancePointPointEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogDistancePointSegmentTool))
                    {
                        _toolType = ToolType.Detail;

                        CogDistancePointSegmentEditV2 cogDistancePointSegmentEditV2 = new CogDistancePointSegmentEditV2();
                        pnlDeltailTool.Controls.Add(cogDistancePointSegmentEditV2);
                        cogDistancePointSegmentEditV2.Dock = DockStyle.Fill;
                        cogDistancePointSegmentEditV2.Font = font;
                        cogDistancePointSegmentEditV2.ForeColor = Color.Black;
                        cogDistancePointSegmentEditV2.Subject = _setting.GetJob.Tools[nSel] as CogDistancePointSegmentTool;
                        ToolDisp.Tool = cogDistancePointSegmentEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogDistanceSegmentCircleTool))
                    {
                        _toolType = ToolType.Detail;

                        CogDistanceSegmentCircleEditV2 cogDistanceSegmentCircleEditV2 = new CogDistanceSegmentCircleEditV2();
                        pnlDeltailTool.Controls.Add(cogDistanceSegmentCircleEditV2);
                        cogDistanceSegmentCircleEditV2.Dock = DockStyle.Fill;
                        cogDistanceSegmentCircleEditV2.Font = font;
                        cogDistanceSegmentCircleEditV2.ForeColor = Color.Black;
                        cogDistanceSegmentCircleEditV2.Subject = _setting.GetJob.Tools[nSel] as CogDistanceSegmentCircleTool;
                        ToolDisp.Tool = cogDistanceSegmentCircleEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogDistanceSegmentEllipseTool))
                    {
                        _toolType = ToolType.Detail;

                        CogDistanceSegmentEllipseEditV2 cogDistanceSegmentEllipseEditV2 = new CogDistanceSegmentEllipseEditV2();
                        pnlDeltailTool.Controls.Add(cogDistanceSegmentEllipseEditV2);
                        cogDistanceSegmentEllipseEditV2.Dock = DockStyle.Fill;
                        cogDistanceSegmentEllipseEditV2.Font = font;
                        cogDistanceSegmentEllipseEditV2.ForeColor = Color.Black;
                        cogDistanceSegmentEllipseEditV2.Subject = _setting.GetJob.Tools[nSel] as CogDistanceSegmentEllipseTool;
                        ToolDisp.Tool = cogDistanceSegmentEllipseEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogDistanceSegmentLineTool))
                    {
                        _toolType = ToolType.Detail;

                        CogDistanceSegmentLineEditV2 cogDistanceSegmentLineEditV2 = new CogDistanceSegmentLineEditV2();
                        pnlDeltailTool.Controls.Add(cogDistanceSegmentLineEditV2);
                        cogDistanceSegmentLineEditV2.Dock = DockStyle.Fill;
                        cogDistanceSegmentLineEditV2.Font = font;
                        cogDistanceSegmentLineEditV2.ForeColor = Color.Black;
                        cogDistanceSegmentLineEditV2.Subject = _setting.GetJob.Tools[nSel] as CogDistanceSegmentLineTool;
                        ToolDisp.Tool = cogDistanceSegmentLineEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogDistanceSegmentSegmentTool))
                    {
                        _toolType = ToolType.Detail;

                        CogDistanceSegmentSegmentEditV2 cogDistanceSegmentSegmentEditV2 = new CogDistanceSegmentSegmentEditV2();
                        pnlDeltailTool.Controls.Add(cogDistanceSegmentSegmentEditV2);
                        cogDistanceSegmentSegmentEditV2.Dock = DockStyle.Fill;
                        cogDistanceSegmentSegmentEditV2.Font = font;
                        cogDistanceSegmentSegmentEditV2.ForeColor = Color.Black;
                        cogDistanceSegmentSegmentEditV2.Subject = _setting.GetJob.Tools[nSel] as CogDistanceSegmentSegmentTool;
                        ToolDisp.Tool = cogDistanceSegmentSegmentEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogIDTool))
                    {
                        _toolType = ToolType.Detail;

                        CogIDEditV2 cogIDEditV2 = new CogIDEditV2();
                        pnlDeltailTool.Controls.Add(cogIDEditV2);
                        cogIDEditV2.Dock = DockStyle.Fill;
                        cogIDEditV2.Font = font;
                        cogIDEditV2.ForeColor = Color.Black;
                        cogIDEditV2.Subject = _setting.GetJob.Tools[nSel] as CogIDTool;
                        ToolDisp.Tool = cogIDEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogOCRMaxTool))
                    {
                        _toolType = ToolType.Detail;

                        CogOCRMaxEditV2 cogOCRMaxEditV2 = new CogOCRMaxEditV2();
                        pnlDeltailTool.Controls.Add(cogOCRMaxEditV2);
                        cogOCRMaxEditV2.Dock = DockStyle.Fill;
                        cogOCRMaxEditV2.Font = font;
                        cogOCRMaxEditV2.ForeColor = Color.Black;
                        cogOCRMaxEditV2.Subject = _setting.GetJob.Tools[nSel] as CogOCRMaxTool;
                        ToolDisp.Tool = cogOCRMaxEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogOCVMaxTool))
                    {
                        _toolType = ToolType.Detail;

                        CogOCVMaxEdit cogOCVMaxEdit = new CogOCVMaxEdit();
                        pnlDeltailTool.Controls.Add(cogOCVMaxEdit);
                        cogOCVMaxEdit.Dock = DockStyle.Fill;
                        cogOCVMaxEdit.Font = font;
                        cogOCVMaxEdit.ForeColor = Color.Black;
                        cogOCVMaxEdit.Subject = _setting.GetJob.Tools[nSel] as CogOCVMaxTool;
                        ToolDisp.Tool = cogOCVMaxEdit.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogColorExtractorTool))
                    {
                        _toolType = ToolType.Detail;

                        CogColorExtractorEditV2 cogColorExtractorEditV2 = new CogColorExtractorEditV2();
                        pnlDeltailTool.Controls.Add(cogColorExtractorEditV2);
                        cogColorExtractorEditV2.Dock = DockStyle.Fill;
                        cogColorExtractorEditV2.Font = font;
                        cogColorExtractorEditV2.ForeColor = Color.Black;
                        cogColorExtractorEditV2.Subject = _setting.GetJob.Tools[nSel] as CogColorExtractorTool;
                        ToolDisp.Tool = cogColorExtractorEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogColorMatchTool))
                    {
                        _toolType = ToolType.Detail;

                        CogColorMatchEditV2 cogColorMatchEditV2 = new CogColorMatchEditV2();
                        pnlDeltailTool.Controls.Add(cogColorMatchEditV2);
                        cogColorMatchEditV2.Dock = DockStyle.Fill;
                        cogColorMatchEditV2.Font = font;
                        cogColorMatchEditV2.ForeColor = Color.Black;
                        cogColorMatchEditV2.Subject = _setting.GetJob.Tools[nSel] as CogColorMatchTool;
                        ToolDisp.Tool = cogColorMatchEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogColorSegmenterTool))
                    {
                        _toolType = ToolType.Detail;

                        CogColorSegmenterEditV2 cogColorSegmenterEditV2 = new CogColorSegmenterEditV2();
                        pnlDeltailTool.Controls.Add(cogColorSegmenterEditV2);
                        cogColorSegmenterEditV2.Dock = DockStyle.Fill;
                        cogColorSegmenterEditV2.Font = font;
                        cogColorSegmenterEditV2.ForeColor = Color.Black;
                        cogColorSegmenterEditV2.Subject = _setting.GetJob.Tools[nSel] as CogColorSegmenterTool;
                        ToolDisp.Tool = cogColorSegmenterEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogCompositeColorMatchTool))
                    {
                        _toolType = ToolType.Detail;

                        CogCompositeColorMatchEditV2 cogCompositeColorMatchEditV2 = new CogCompositeColorMatchEditV2();
                        pnlDeltailTool.Controls.Add(cogCompositeColorMatchEditV2);
                        cogCompositeColorMatchEditV2.Dock = DockStyle.Fill;
                        cogCompositeColorMatchEditV2.Font = font;
                        cogCompositeColorMatchEditV2.ForeColor = Color.Black;
                        cogCompositeColorMatchEditV2.Subject = _setting.GetJob.Tools[nSel] as CogCompositeColorMatchTool;
                        ToolDisp.Tool = cogCompositeColorMatchEditV2.Subject;
                    }
                    else if (_setting.GetJob.Tools[nSel].GetType() == typeof(CogToolBlock))
                    {
                        _toolType = ToolType.Detail;

                        CogToolBlockEditV2 cogToolBlockEditV = new CogToolBlockEditV2();
                        pnlDeltailTool.Controls.Add(cogToolBlockEditV);
                        cogToolBlockEditV.Dock = DockStyle.Fill;
                        cogToolBlockEditV.Font = font;
                        cogToolBlockEditV.ForeColor = Color.Black;
                        cogToolBlockEditV.Subject = _setting.GetJob.Tools[nSel] as CogToolBlock;
                        ToolDisp.Tool = cogToolBlockEditV.Subject;
                    }

                    _bLoad = true;
                }
                catch (Exception ex) { }
            }
        }
        private void ToolDataLoad()
        {

        }
        private double AngleToRad(double Angle)
        {
            return (Angle * Math.PI) / 180;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (_setting.GetJob != null)
                {
                    _setting.GetJob = cogToolGroup.Subject;
                }

                CogSerializer.SaveObjectToFile(_setting.GetJob, string.Format("{0}\\vpp\\Camera_{1:D2}\\{2:D2}.vpp", Application.StartupPath, (lblCamName.Text), Convert.ToInt32(lblToolName.Text)), typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter), CogSerializationOptionsConstants.Minimum);
                SetControl();
                _setting.FileLoad(string.Format("{0}\\vpp\\Camera_{1:D2}\\{2:D2}.vpp", Application.StartupPath, lblCamName.Text, Convert.ToInt32(lblToolName.Text)));
                _cam._vPro._OnJobLoad("");
                //_setting._OnSettingJobLoad("");
                _cam.SetControl();
                _cam.SetGraphicview();
                MessageBox.Show("Job File Saved", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                GetFileLastWriteTime();
                _bSave = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Job File Save Error : " + ex.Message, "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        private void frmToolEdit_FormClosing(object sender, FormClosingEventArgs e)
        {
            FormClose();
        }


        private void btnRun_Click(object sender, EventArgs e)
        {
            try
            {
                if (_CogImageConvertTool != null)
                {
                    if (_nImgListCnt != 0)
                    {
                        if (_nImgListCnt != _nDispNo + 1)
                            _nDispNo++;
                        else
                            _nDispNo = 0;

                        _Pnl[_nDispNo].Focus();

                        _CogImageConvertTool.InputImage = _cogDisp[_nDispNo].Image;
                        cogInspImg.Image = _cogDisp[_nDispNo].Image;

                        for (var i = 0; i < 10; i++)
                            _Pnl[i].Invalidate();
                    }

                    if (_CogImageConvertTool.InputImage != null)
                    {
                        _setting.GetJob = cogToolGroup.Subject;
                        _setting.GetJob.Run();
                        SetControl();
                        RunStatus();
                        TooRegionView();
                    }
                }
            }
            catch (Exception ex)
            {
                _cam._OnMessage("Manual Run Error : " + ex.Message, Color.Red, false, false);
            }
        }
        private void TooRegionView()
        {
            ICogRegion[] toolsregion = new ICogRegion[cogToolGroup.Subject.Tools.Count - 3];
            string[] regionName = new string[cogToolGroup.Subject.Tools.Count - 3];
            ICogGraphic[] toolgraphic;
            double[] ToolResultsValue;

            for (int i = 0; i < cogToolGroup.Subject.Tools.Count; i++)
                _DataAnalysisTool = cogToolGroup.Subject.Tools[i] as CogDataAnalysisTool;
            try
            {
                ToolResultsValue = new double[_DataAnalysisTool.Results.Count];
            }
            catch
            {
                ToolResultsValue = new double[0];
            }

            CogPointMarker _marker = new CogPointMarker();

            GetToolRegion(ref toolsregion, ref regionName);      // 각 툴의 검사 영역을 가지고 온다음~

            toolgraphic = GetToolGraphic(toolsregion, regionName);     // 각 채널의 결과에 따라 색을 입히고 던지자

            //for (int i = 0; i < toolgraphic.Length; i++)
            //{
            //    if (toolgraphic[i] == null)
            //        toolgraphic[i] = _marker as ICogGraphic;
            //}
            if (toolgraphic != null)
            {
                try
                {
                    for (int i = 0; i < toolgraphic.Length; i++)     // 검사 영역 그려주고
                    {
                        if (toolgraphic[i] != null)
                            cogInspImg.InteractiveGraphics.Add(toolgraphic[i] as ICogGraphicInteractive, "Region", false);
                    }
                }
                catch { }
            }

        }
        private ICogRegion[] GetToolRegion(ref ICogRegion[] _toolregion, ref string[] regionName)
        {
            _toolregion = new ICogRegion[cogToolGroup.Subject.Tools.Count - 3];
            regionName = new string[cogToolGroup.Subject.Tools.Count - 3];

            CogBlobTool _blob = new CogBlobTool();
            CogPMAlignTool _pmalign = new CogPMAlignTool();
            CogCaliperTool _caliper = new CogCaliperTool();
            CogFindCircleTool _findcircle = new CogFindCircleTool();
            CogFindLineTool _findline = new CogFindLineTool();
            CogIDTool _ID = new CogIDTool();
            CogOCRMaxTool _OCR = new CogOCRMaxTool();
            CogHistogramTool _jistogram = new CogHistogramTool();
            CogCNLSearchTool _CNLserch = new CogCNLSearchTool();


            try
            {
                for (int i = 2; i < cogToolGroup.Subject.Tools.Count - 1; i++)
                {
                    if (cogToolGroup.Subject.Tools[i].GetType() == _blob.GetType())   // 타입 비교~ 1
                    {
                        _blob = cogToolGroup.Subject.Tools[i] as CogBlobTool;

                        if (_blob.Region != null)
                        {
                            _toolregion[i - 2] = _blob.Region;
                            regionName[i - 2] = _blob.Name;
                        }
                    }

                    else if (cogToolGroup.Subject.Tools[i].GetType() == _jistogram.GetType())   // 타입 비교~ 2
                    {
                        _jistogram = cogToolGroup.Subject.Tools[i] as CogHistogramTool;

                        if (_jistogram.Region != null)
                        {
                            _toolregion[i - 2] = _jistogram.Region;
                            regionName[i - 2] = _jistogram.Name;
                        }
                    }

                    else if (cogToolGroup.Subject.Tools[i].GetType() == _findcircle.GetType())   // 타입 비교~ 2
                    {
                        _findcircle = cogToolGroup.Subject.Tools[i] as CogFindCircleTool;

                        if (_findcircle.Results != null)
                        {
                            _toolregion[i - 2] = _findcircle.Results.GetCircle() as ICogRegion;
                            regionName[i - 2] = _findcircle.Name;
                        }
                    }

                    else if (cogToolGroup.Subject.Tools[i].GetType() == _pmalign.GetType())   // 타입 비교~ 3
                    {
                        _pmalign = cogToolGroup.Subject.Tools[i] as CogPMAlignTool;

                        if (_pmalign.SearchRegion != null)
                        {
                            _toolregion[i - 2] = _pmalign.SearchRegion;
                            regionName[i - 2] = _pmalign.Name;
                        }
                    }

                    else if (cogToolGroup.Subject.Tools[i].GetType() == _caliper.GetType())   // 타입 비교~ 4
                    {
                        _caliper = cogToolGroup.Subject.Tools[i] as CogCaliperTool;

                        if (_caliper.Region != null)
                        {
                            _toolregion[i - 2] = _caliper.Region;
                            regionName[i - 2] = _caliper.Name;
                        }
                    }

                    else if (cogToolGroup.Subject.Tools[i].GetType() == _findline.GetType())   // 타입 비교~ 7
                    {
                        _findline = cogToolGroup.Subject.Tools[i] as CogFindLineTool;

                        if (_findline.Results != null)
                        {
                            if (_findline.Results.GetLine() != null)
                            {
                                _toolregion[i - 2] = _findline.Results.GetLine() as ICogRegion;
                                regionName[i - 2] = _findline.Name;
                            }

                            else if (_findline.Results.GetLineSegment() != null)
                            {
                                _toolregion[i - 2] = _findline.Results.GetLineSegment() as ICogRegion;
                                regionName[i - 2] = _findline.Name;
                            }
                        }
                    }

                    else if (cogToolGroup.Subject.Tools[i].GetType() == _ID.GetType())   // 타입 비교~ 10
                    {
                        _ID = cogToolGroup.Subject.Tools[i] as CogIDTool;

                        if (_ID.Region != null)
                        {
                            _toolregion[i - 2] = _ID.Region;
                            regionName[i - 2] = _ID.Name;
                        }
                    }

                    else if (cogToolGroup.Subject.Tools[i].GetType() == _OCR.GetType())   // 타입 비교~ 10
                    {
                        _OCR = cogToolGroup.Subject.Tools[i] as CogOCRMaxTool;

                        if (_OCR.Region != null)
                        {
                            _toolregion[i - 2] = _OCR.Region;
                            regionName[i - 2] = _OCR.Name;
                        }
                    }

                    else if (cogToolGroup.Subject.Tools[i].GetType() == _CNLserch.GetType())   // 타입 비교~ 10
                    {
                        _CNLserch = cogToolGroup.Subject.Tools[i] as CogCNLSearchTool;

                        if (_CNLserch.SearchRegion != null)
                        {
                            _toolregion[i - 2] = _CNLserch.SearchRegion;
                            regionName[i - 2] = _CNLserch.Name;
                        }
                    }
                }
            }
            catch
            { }

            return _toolregion;
        }
        private ICogGraphic[] GetToolGraphic(ICogRegion[] regions, string[] regionName)
        {

            ICogGraphic[] Toolgraphic = new ICogGraphic[regions.Length];
            int toolcount = _DataAnalysisTool.RunParams.Count;

            try
            {
                for (int i = 0; i < toolcount; i++)     // 데이타 결과 툴의 채널 갯수~
                {
                    for (int n = 0; n < regions.Length; n++)   // 영역을 그릴수 있는 툴의 갯수~
                    {
                        if (regions[n] != null)
                        {
                            if (_DataAnalysisTool.RunParams[i].Name == regionName[n])    // 채널과 툴의 이름 비교~
                            {
                                Toolgraphic[n] = regions[n] as ICogGraphic;      // 드디어 그림 그릴놈을 만들어주고
                                Toolgraphic[n].LineWidthInScreenPixels = 3;     // 영역 두께 

                                if (_DataAnalysisTool.Results != null)
                                {
                                    if (_DataAnalysisTool.Results[i] != null)
                                    {
                                        if (_DataAnalysisTool.Results[i].Pass)      // 결과에 따라 색도 칠하고
                                            Toolgraphic[n].Color = CogColorConstants.Green;
                                        else
                                            Toolgraphic[n].Color = CogColorConstants.Red;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            { }
            return Toolgraphic;
        }

        private void RunStatus()
        {
            try
            {
                for (int i = 0; i < listcontrol.ItemCount; i++)
                    listcontrol.Items[i].ImageOptions.ImageIndex = (int)ToolRes.Red;

                if (_setting.GetJob.RunStatus.Result != CogToolResultConstants.Error)
                {
                    for (int i = 0; i < listcontrol.ItemCount; i++)
                        listcontrol.Items[i].ImageOptions.ImageIndex = (int)ToolRes.Green;
                }
                else
                {
                    for (int i = 0; i < _setting.GetJob.Tools.Count; i++)
                    {
                        if (_setting.GetJob.Tools[i].RunStatus.Result == CogToolResultConstants.Error)
                        {
                            listcontrol.Items[i].ImageOptions.ImageIndex = (int)ToolRes.Red;
                            //break;
                        }
                        else if (_setting.GetJob.Tools[i].RunStatus.Result == CogToolResultConstants.Accept || _setting.GetJob.Tools[i].RunStatus.Result == CogToolResultConstants.Reject)
                            listcontrol.Items[i].ImageOptions.ImageIndex = (int)ToolRes.Green;
                        else if (_setting.GetJob.Tools[i].RunStatus.Result == CogToolResultConstants.Warning)
                            listcontrol.Items[i].ImageOptions.ImageIndex = (int)ToolRes.Yellow;
                    }
                }
            }
            catch { }

        }

        private void btnAllTool_Click(object sender, EventArgs e)
        {
            if (!flyToolGroup.IsPopupOpen)
            {
                _toolType = ToolType.All;
                flyToolEdit.HidePopup();
                flyDetail.HidePopup();
                flyToolGroup.ShowPopup();
            }
            else
            {
                _toolType = ToolType.Light;
                flyToolGroup.HidePopup();
                flyDetail.HidePopup();
                flyToolEdit.ShowPopup();
            }
        }

        private void btnToolDetail_Click(object sender, EventArgs e)
        {
            int nSel = listcontrol.SelectedIndex;
            if (nSel == -1)
                return;

            try
            {
                if (!splashScreenManager1.IsSplashFormVisible)
                    splashScreenManager1.ShowWaitForm();

                ShowPopup(false);

                _toolType = ToolType.Detail;
                ToolLoad(nSel, ToolType.Detail);

                if (splashScreenManager1.IsSplashFormVisible)
                    splashScreenManager1.CloseWaitForm();

                ShowPopup(true);
            }
            catch { }
        }

        private void btnDetaliClose_Click(object sender, EventArgs e)
        {
            ShowPopup(false);
            _toolType = ToolType.Light;
            ShowPopup(true);
        }

        private void txtGraphicSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))    //숫자와 백스페이스를 제외한 나머지를 바로 처리
                e.Handled = true;
        }

        private void frmToolEdit_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
                else e.Effect = DragDropEffects.None;
            }
            catch { }
        }

        private void frmToolEdit_DragDrop(object sender, DragEventArgs e)
        {
            if (_CogImageConvertTool == null)
                return;

            try
            {
                string[] strFilePath = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string str in strFilePath)
                {
                    using (Bitmap bmp = new Bitmap(Bitmap.FromFile(str)))
                        _CogImageConvertTool.InputImage = new CogImage24PlanarColor(bmp);
                }
            }
            catch { }
        }


        private void cogDisp1_DoubleClick(object sender, EventArgs e)
        {
            int.TryParse((sender as CogDisplay).Tag.ToString(), out _nDispNo);
            _Pnl[_nDispNo].Focus();

            for (var i = 0; i < 10; i++)
                _Pnl[i].Invalidate();

            try
            {
                _CogImageConvertTool.InputImage = _cogDisp[_nDispNo].Image;
                cogInspImg.AutoFit = true;
                cogInspImg.Image = _CogImageConvertTool.InputImage;
                JobRun();
            }
            catch { }
        }

        private void Pnl1_Paint(object sender, PaintEventArgs e)
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
        }

        private void frmToolEdit_ResizeEnd(object sender, EventArgs e)
        {
            Resize();
        }

        private void Resize()
        {
            if (_nWidth != this.Width || _nHeight != this.Height)
            {
                _nWidth = this.Width;
                _nHeight = this.Height;

                ShowPopup(false);
                Delay(500);

                flyToolEdit.Width = pnlMain.Width;
                flyDetail.Width = pnlMain.Width;
                flyToolGroup.Width = pnlMain.Width;

                ShowPopup(true);
            }
        }

        private void frmToolEdit_Load(object sender, EventArgs e)
        {
            flyToolEdit.Width = pnlMain.Width;
            flyDetail.Width = pnlMain.Width;
            flyToolGroup.Width = pnlMain.Width;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _iFilmCount = 0;
            Image_Filmstream(_iFilmCount);
            ImageCount(ofd.FileNames.Length, _iFilmCount);
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            int iMax = ofd.FileNames.Length / 10;
            int iMaxcheck = ofd.FileNames.Length % 10;
            if (iMaxcheck == 0 && ofd.FileNames.Length > 10)
                _iFilmCount = iMax - 1;
            else
                _iFilmCount = iMax;

            Image_Filmstream(_iFilmCount);
            ImageCount(ofd.FileNames.Length, _iFilmCount);
        }

        private void btnBefore_Click(object sender, EventArgs e)
        {
            if (_iFilmCount > 0)
            {
                _iFilmCount--;
                Image_Filmstream(_iFilmCount);
                ImageCount(ofd.FileNames.Length, _iFilmCount);
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (ofd.FileNames.Length % 10 != 0)
            {
                if (ofd.FileNames.Length / 10 > _iFilmCount)
                    _iFilmCount++;
            }
            else
            {
                if (ofd.FileNames.Length / 10 > _iFilmCount + 1)
                    _iFilmCount++;

            }
            Image_Filmstream(_iFilmCount);
            ImageCount(ofd.FileNames.Length, _iFilmCount);
        }

        private void Image_Filmstream(int istart)
        {
            try
            {
                InitImgList();
                for (var i = 0; i < (ofd.FileNames.Length - istart * 10); i++)
                {
                    using (FileStream fs = new FileStream(ofd.FileNames[i + (istart * 10)], FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        _cogDisp[i].AutoFit = true;
                        _cogDisp[i].Image = new CogImage24PlanarColor(new Bitmap(Image.FromStream(fs)));
                    }

                    _nImgListCnt++;

                    if (_nImgListCnt == 10)
                        break;
                }


                _Pnl[_nDispNo].Focus();


                for (var i = 0; i < 10; i++)
                    _Pnl[i].Invalidate();

                string strMsg = "";

                try
                {
                    if (_CogImageConvertTool != null)
                    {
                        _CogImageConvertTool.InputImage = _cogDisp[_nDispNo].Image;
                        cogInspImg.AutoFit = true;
                        cogInspImg.Image = _CogImageConvertTool.InputImage;
                        JobRun();
                    }
                }
                catch (Exception ex)
                {
                    _cam._OnMessage("Image Open Error : " + ex.Message, Color.Red, false, false);
                }
            }
            catch (Exception ex)
            {
                _cam._OnMessage("Image_Filmstream Error : " + ex.Message, Color.Red, false, false);
            }
        }

        private void ImageCount(int iMax, int index)
        {
            Invoke(new EventHandler(delegate
            {
                lblImageFilmCount.Text = iMax.ToString() + " 이미지 中 " + (index * 10 + 1).ToString() + " ~ " + (index * 10 + 10).ToString();
            }));
        }

        private void pnlDeltailTool_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
