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
using Cognex.VisionPro.ImageFile;

namespace VisionSystem
{
    public delegate void OnCamVppLoadEvent(int nIdx,int nIdx2);
    public partial class frmModel : DevExpress.XtraEditors.XtraForm
    {
        public OnCamVppLoadEvent OnCamtVpp;

        frmMain _frmMain;
        frmToolSetup _frmToolSetup;
        IniFiles ini = new IniFiles();
        CodeControl[] _codeControl = null;
        bool _bPass = false;

        CogToolGroup _toolGrp = new CogToolGroup();
        CogImageConvertTool _cogImageConvertTool = new CogImageConvertTool();
        ICogImage _cogImg = null;

        CamParamControl[] _camParam = new CamParamControl[30];
        PrintParamControl[] _PrintParam = new PrintParamControl[30];
        public frmModel(frmMain MainFrm)
        {
            InitializeComponent();
            _frmMain = MainFrm;

            ActiveControl = labelControl1;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ShowPanel(SimpleButton btn1, SimpleButton btn2, SimpleButton btn3, SimpleButton btn4)
        {
            tabModel.Visible = false;
            int.TryParse(btn1.Tag.ToString(), out int nNo1);
            int.TryParse(btn2.Tag.ToString(), out int nNo2);
            int.TryParse(btn3.Tag.ToString(), out int nNo3);
            int.TryParse(btn4.Tag.ToString(), out int nNo4);

            tabModel.TabPages[nNo1].PageVisible = true;
            tabModel.TabPages[nNo2].PageVisible = false;
            tabModel.TabPages[nNo3].PageVisible = false;
            tabModel.TabPages[nNo4].PageVisible = false;

            txtAddModelName.Text = "";
            cbModel.SelectedIndex = -1;
            cbDelModel.SelectedIndex = -1;
            txtPassword.Text = "";

            _bPass = false;
            if (flyModel.IsPopupOpen)
                flyModel.HideBeakForm();

            flyModel.OwnerControl = btn1;

            tabModel.Visible = true;
            flyModel.ShowBeakForm();

            if (btn1.Tag.ToString() == "0")
                txtAddModelName.Focus();
            else if (btn1.Tag.ToString() == "1")
                cbModel.Focus();
            else if (btn1.Tag.ToString() == "2")
                cbDelModel.Focus();
            else if (btn1.Tag.ToString() == "3")
            {
                btnToolCreate.DialogResult = DialogResult.Yes;
                txtPassword.Focus();
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
        private void btnToolCreate_Click(object sender, EventArgs e)
        {
            ShowPanel(btnToolCreate, btnModelAdd, btnModelCopy, btnModelDel);

        }

        private void Loading()
        {
            InitControl();

            try
            {
                foreach (string str in _frmMain._var._listModel)
                {
                    dg_List.Rows.Add((dg_List.Rows.Count + 1).ToString(), str);
                    cbModel.Properties.Items.Add(str);
                    cbDelModel.Properties.Items.Add(str);
                }

                for (int i = 0; i < tabCam.TabPages.Count; i++)
                {
                    if (_frmMain._var._nScreenCnt > i)
                        tabCam.TabPages[i].PageVisible = true;
                    else
                        tabCam.TabPages[i].PageVisible = false;
                }

                if (_frmMain._var._listModel.Count > 0)
                {
                    if (_frmMain._var._strModelNo == "")
                        lblModel.Text = _frmMain._var._listModel[0];
                    else
                    {
                        for (int i = 0; i < _frmMain._var._listModel.Count; i++)
                        {
                            if (_frmMain._var._listModel[i].Contains(_frmMain._var._strModelNo))
                            {
                                lblModel.Text = _frmMain._var._listModel[i];
                                dg_List.CurrentCell = dg_List.Rows[i].Cells[0];
                                Model_Load(lblModel.Text, true);
                                break;
                            }
                        }
                    }
                }
                
            }
            catch { }
        }
        private void frmModel_Load(object sender, EventArgs e)
        {
            Loading();
            
        }
        private void Model_Load(string model, bool bLoad)
        {
            if (_frmMain._var._bUsePrint == false)
            {
                for (int i = 0; i < _frmMain._var._nScreenCnt; i++)
                {
                    if (_camParam[i] == null)
                    {
                        _camParam[i] = new CamParamControl(_frmMain, i, model);

                        tabCam.TabPages[i].Text = string.Format("Camera{0}", i + 1);
                        tabCam.TabPages[i].Controls.Add(_camParam[i]);
                    }
                    _camParam[i]._strModel = model;
                    _camParam[i].Dock = DockStyle.Fill;
                    _camParam[i].LoadSet(bLoad);
                    _camParam[i].OnLoadVpp = OnLoadVpp;

                    if (i == 0)
                        tabCam.SelectedTabPageIndex = 0;
                }
            }
            else
            {
                btnToolCreate.Visible = false;
                if(_PrintParam[0] == null)
                    _PrintParam[0] = new PrintParamControl(_frmMain,0, model);

                tabCam.TabPages[0].Text = string.Format("Print{0}",1);
                tabCam.TabPages[0].Controls.Add(_PrintParam[0]);

                _PrintParam[0]._strModel = model;
                _PrintParam[0].Dock = DockStyle.Fill;
                _PrintParam[0].LoadSet();
                tabCam.SelectedTabPageIndex = 1;
            }
        }
        private void OnLoadVpp(int index1, int index2)
        {
            _frmMain._CAM[index1].ToolEdit(index2);
        }

        private void InitControl()
        {
            dg_List.Columns.Clear();
            dg_List.Columns.Add("No", "No");
            dg_List.Columns.Add("ModelName", "ModelName");

            dg_List.Columns[0].Width = 46;
            dg_List.Columns[1].Width = 248;

            dg_List.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            for (int i = 0; i < 2; i++)
            {
                dg_List.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dg_List.Columns[i].ReadOnly = true;
                dg_List.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }            
        }

        private void AddModel()
        {
            dg_List.Rows.Clear();

            for (int i=0; i<_frmMain._var._listModel.Count; i++)
                dg_List.Rows.Add((dg_List.Rows.Count + 1).ToString(), _frmMain._var._listModel[i]);
        }

        private void btnModelAdd_Click(object sender, EventArgs e)
        {
            ShowPanel(btnModelAdd, btnToolCreate, btnModelCopy, btnModelDel);
        }

        private void btnModelCopy_Click(object sender, EventArgs e)
        {
            ShowPanel(btnModelCopy, btnModelAdd, btnToolCreate, btnModelDel);
        }

        private void btnModelDel_Click(object sender, EventArgs e)
        {
            ShowPanel(btnModelDel, btnModelCopy, btnModelAdd, btnToolCreate);
        }


        private void btnCheck_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Please Input Password", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_frmMain._var._strAdminPW == txtPassword.Text)
            {
                if (flyModel.IsPopupOpen)
                    flyModel.HideBeakForm();

                btnModelAdd.Enabled = true;
                btnModelCopy.Enabled = true;
                btnModelDel.Enabled = true;

                if (_frmToolSetup != null)
                {
                    _frmToolSetup.Dispose();
                    _frmToolSetup = null;
                }

                splashScreenManager1.ShowWaitForm();

                _frmToolSetup = new frmToolSetup(this, _frmMain);
                _frmToolSetup.Show();

                splashScreenManager1.CloseWaitForm();
            }
        }

        private void btnNewModelClose_Click(object sender, EventArgs e)
        {
            btnToolCreate.DialogResult = DialogResult.No;

            if (flyModel.IsPopupOpen)
                flyModel.HideBeakForm();
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnCheck.PerformClick();
        }

        private void btnNewModel_Click(object sender, EventArgs e)
        {
            if (txtAddModelName.Text == "")
            {
                MessageBox.Show("Please Input Model Name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            flyModel.HideBeakForm();

            try
            {
                string strPath = string.Format("{0}\\vpp\\Camera_{1:D2}", Application.StartupPath, tabCam.SelectedTabPageIndex);
                DirectoryInfo dr = new DirectoryInfo(strPath);
                var strFileName = "";

                if (!dr.Exists)
                {
                    dr.Create();
                    strFileName = "01";
                }
                else
                {
                    var strFiles = dr.GetFiles("*.vpp");
                    strFileName = string.Format("{0:D2}", strFiles.Length + 1);
                }

                CogSerializer.SaveObjectToFile(_toolGrp, strPath + "\\" + strFileName + ".vpp", typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter), CogSerializationOptionsConstants.Minimum);

                var strImgPath = string.Format("{0}\\Camera_{1:D2}", _frmMain._var._strMasterImagePath, tabCam.SelectedTabPageIndex);
                dr = new DirectoryInfo(strImgPath);
                if (!dr.Exists)
                    dr.Create();

                FileInfo fi = new FileInfo(string.Format("{0}\\{1}.bmp", strImgPath, strFileName));
                if (!fi.Exists)
                {
                    using (CogImageFileBMP cogBMP = new CogImageFileBMP())
                    {
                        cogBMP.Open(string.Format("{0}\\{1}.bmp", strImgPath, strFileName), CogImageFileModeConstants.Write);
                        cogBMP.Append(_cogImageConvertTool.OutputImage);
                        cogBMP.Close();
                    }
                }

                ini.WriteIniFile(strFileName, "Value", "", strPath, "Comment.ini");

                _frmMain.AddMsg("New Working file saved", Color.GreenYellow, true, false, frmMain.MsgType.Alarm);
            }
            catch { }
        }

        private void txtAddModelName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnNewModel.PerformClick();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (cbModel.SelectedIndex == -1)
            {
                MessageBox.Show("Please select the model you want to copy.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtCopyModel.Text == "")
            {
                
                return;
            }

            for (int i=0; i< _frmMain._var._listModel.Count; i++)
            {
                if (_frmMain._var._listModel[i] == txtCopyModel.Text)
                {
                    MessageBox.Show("Duplicate model name found." + "\r\n" + "Please re - enter", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            flyModel.HideBeakForm();
                
            _frmMain._var._listModel.Add(txtCopyModel.Text);
            AddModel();
            cbModel.Properties.Items.Add(txtCopyModel.Text);
            cbDelModel.Properties.Items.Add(txtCopyModel.Text);

            string strTemp = "";
            for (int i = 0; i < _frmMain._var._listModel.Count; i++)
                strTemp += _frmMain._var._listModel[i] + ",";

            ini.WriteIniFile("Modellist", "Value", strTemp, _frmMain._var._strModelPath, "Modellist.ini");
            ModelCopy(cbModel.SelectedItem.ToString(), txtCopyModel.Text);
            cbModel.SelectedIndex = -1;

            DirectoryInfo dr = new DirectoryInfo(_frmMain._var._strModelPath + "\\" + txtCopyModel.Text);
            if (!dr.Exists)
                dr.Create();

            txtCopyModel.Text = "";
        }

        private void ModelCopy(string strOriginPath, string strTargetPath)
        {
            try
            {
                DirectoryInfo dr = new DirectoryInfo(_frmMain._var._strModelPath + "\\" + strTargetPath);

                if (!dr.Exists)
                    dr.Create();

                string[] strFiles = Directory.GetFiles(_frmMain._var._strModelPath + "\\" + strOriginPath);

                string strFileName = "";
                foreach (string strFile in strFiles)
                {
                    strFileName = Path.GetFileName(strFile);
                    File.Copy(strFile, _frmMain._var._strModelPath + "\\" + strTargetPath + "\\" + strFileName, true);
                }
            }
            catch (Exception ex)
            {
                _frmMain.AddMsg("Model Copy Error : " + ex.Message, Color.Red, true, true, frmMain.MsgType.Alarm);
            }
        }

        private void ModelDelete(string strTargetPath)
        {
            try
            {
                string[] strFolders = Directory.GetDirectories(_frmMain._var._strModelPath + "\\");

                foreach(string stfFoler in strFolders)
                {
                    if (stfFoler.Contains(strTargetPath))
                    {
                        Directory.Delete(stfFoler, true);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _frmMain.AddMsg("Model Delete Error : " + ex.Message, Color.Red, true, true, frmMain.MsgType.Alarm);
            }
        }

        private void txtCopyModel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnCopy.PerformClick();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (cbDelModel.SelectedIndex == -1)
            {
                MessageBox.Show("Please select the model you want to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete the selected model?") == DialogResult.No)
                return;

            flyModel.HideBeakForm();
            int nSel = cbDelModel.SelectedIndex;
            string strFolderName = _frmMain._var._listModel[nSel];
            _frmMain._var._listModel.RemoveAt(nSel);
            AddModel();

            cbModel.Properties.Items.RemoveAt(nSel);
            cbDelModel.Properties.Items.RemoveAt(nSel);

            string strTemp = "";
            for (int i = 0; i < _frmMain._var._listModel.Count; i++)
                strTemp += _frmMain._var._listModel[i] + ",";
            
            ini.WriteIniFile("Modellist", "Value", strTemp, _frmMain._var._strModelPath, "Modellist.ini");
            ModelDelete(cbDelModel.SelectedItem.ToString());
            cbDelModel.SelectedIndex = -1;

            if (lblModel.Text == strFolderName)
            {
                lblModel.Text = "";
                _frmMain.lblModel.Caption = "";

                _frmMain._var._strModelNo = "";
                ini.WriteIniFile("ModelNo", "Value", _frmMain._var._strModelNo, _frmMain._var._strConfigPath, "Config.ini");
            }
            
            Loading();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                //splashScreenManager1.ShowWaitForm();
                if (!_frmMain._var._bUsePrint)
                {
                    bool[] bSave = new bool[_frmMain._var._nScreenCnt];
                    for (int i = 0; i < _frmMain._var._nScreenCnt; i++)
                    {
                        bSave[i] = _camParam[i].SaveSet();
                        _frmMain._CAM[i]._modelParam = _frmMain._ModelParam[i];
                        _frmMain._CAM[i]._var = _frmMain._var;
                        _frmMain._CAM[i].LoadSet();
                    }

                    for (int i = 0; i < bSave.Length; i++)
                    {
                        if (!bSave[i])
                        {
                            _frmMain.AddMsg("Failed to save", Color.Red, true, true, frmMain.MsgType.Alarm);
                            //MessageBox.Show("Failed to save", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    _frmMain.AddMsg("Reicpe Saved", Color.Green, true, false, frmMain.MsgType.Alarm);
                    //MessageBox.Show("Reicpe Saved", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else {
                    _PrintParam[0].SaveSet();
                }
            }
            catch (Exception ex)
            {
                _frmMain.AddMsg("Model Parameters Save Error : " + ex.Message, Color.Red, true, true, frmMain.MsgType.Alarm);
            }

            //splashScreenManager1.CloseWaitForm();
        }

        private void dg_List_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int nSel = dg_List.CurrentRow.Index;
            if (nSel > -1)
            {
                lblModel.Text = dg_List.Rows[nSel].Cells[1].FormattedValue.ToString();

                var bModelSame = _frmMain._var._strModelNo == lblModel.Text ? true : false;
                //splashScreenManager1.ShowWaitForm();
                Model_Load(lblModel.Text, bModelSame);
                //splashScreenManager1.CloseWaitForm();
            }
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            if (txtPassword.Text == "")
            {
                lblPassInput.Visible = false;
                return;
            }

            if (txtPassword.Text == _frmMain._var._strAdminPW)
            {
                lblPassInput.Text = "Match";
                lblPassInput.ForeColor = Color.Lime;
            }
            else
            {
                lblPassInput.Text = "Not Match";
                lblPassInput.ForeColor = Color.Red;
            }

            lblPassInput.Visible = true;
        }

        private void btnCopyVppClose_Click(object sender, EventArgs e)
        {
            if (flyVpp.IsPopupOpen)
                flyVpp.HideBeakForm();
        }

        private void btnNewVppClose_Click(object sender, EventArgs e)
        {
            if (flyVpp.IsPopupOpen)
                flyVpp.HideBeakForm();
        }

        private void btnNewVpp_Click(object sender, EventArgs e)
        {
            if (txtAddVppName.Text == "")
            {
                MessageBox.Show("Please Input Vpp Name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            flyVpp.HideBeakForm();

            if (lblModel.Text == "" || lblModel.Text == "-")
            {
                _frmMain._var._strModelNo = "01";
                _frmMain.lblModel.Caption = "01_" + txtAddModelName.Text;
                lblModel.Text = _frmMain.lblModel.Caption;

                ini.WriteIniFile("ModelNo", "Value", _frmMain._var._strModelNo, _frmMain._var._strConfigPath, "Config.ini");
            }
            else
                lblModel.Text = string.Format("{0:D2}_{1}", dg_List.RowCount + 1, txtAddModelName.Text);

            _frmMain._var._listModel.Add(lblModel.Text);
            AddModel();
            cbModel.Properties.Items.Add(lblModel.Text);
            cbDelModel.Properties.Items.Add(lblModel.Text);

            string strTemp = "";
            for (int i = 0; i < _frmMain._var._listModel.Count; i++)
                strTemp += _frmMain._var._listModel[i] + ",";

            ini.WriteIniFile("Modellist", "Value", strTemp, _frmMain._var._strModelPath, "Modellist.ini");

            DirectoryInfo dr = new DirectoryInfo(_frmMain._var._strModelPath + "\\" + lblModel.Text);
            if (!dr.Exists)
                dr.Create();

            txtAddModelName.Text = "";
        }
    }
}