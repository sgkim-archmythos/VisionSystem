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
using System.Data.OleDb;
using System.IO;

using Cognex.VisionPro;

namespace VisionSystem
{
    public partial class frmPrintList : DevExpress.XtraEditors.XtraForm
    {
        frmMain _frmMain;
        Bitmap _bmpMaster = null;
        Bitmap _bmpOrigin = null;
        Bitmap _bmpResult = null;
        IniFiles ini = new IniFiles();

        public frmPrintList(frmMain MainFrm)
        {
            InitializeComponent();
            _frmMain = MainFrm;

            this.Width = _frmMain.Width;
            this.Height = _frmMain.Height;
        }

        private void frmJobList_Load(object sender, EventArgs e)
        {
            InitControl();
            SearchData();
            //ActiveControl = labelControl1;
        }

        private void LoadParam()
        {

        }

        private void InitControl()
        {
            date1.DateTime = DateTime.Now;
            date2.DateTime = DateTime.Now;

            cbCode.Properties.Items.Clear();
            cbCode.Properties.Items.Add("All");

            if (_frmMain._var._listModel.Count > 0)
            {
                for (int i = 0; i < _frmMain._var._listModel.Count; i++)
                    cbCode.Properties.Items.Add(_frmMain._var._listModel[i]);

                cbCode.SelectedIndex = 0;
            }
        }

        private void SearchData()
        {
            if (_frmMain._var._bUsePrint)
            {
                if (_frmMain._PrintOleDB.State == ConnectionState.Open)
                {
                    try
                    {
                        using (var cmd = new OleDbCommand())
                        {
                            dg_List.DataSource = null;

                            cmd.CommandType = CommandType.Text;
                            cmd.Connection = _frmMain._PrintOleDB;

                            var strQuery = "Select Model, LotID, MotorID, Date, PrintData, PrintType"; //from Data where [Date] >= ? and [Date] <= ?";


                            strQuery += " from Data where [Date] >= ? and [Date] <= ?";

                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@Date", Convert.ToDateTime(date1.DateTime.ToString("yyyy-MM-dd 00:00:00")));
                            cmd.Parameters.Add("@Date", Convert.ToDateTime(date2.DateTime.ToString("yyyy-MM-dd 23:59:59")));

                            if (cbCode.SelectedIndex > 0)
                            {
                                strQuery += " and [Model] = ?";
                                cmd.Parameters.AddWithValue("@Model", string.Format("%{0}%", cbCode.SelectedItem.ToString()));

                            }

                            if (!string.IsNullOrEmpty(txtLotID.Text))
                            {
                                strQuery += " and [LotID] like ?";
                                cmd.Parameters.AddWithValue("@LotID", string.Format("%{0}%", txtLotID.Text.Trim()));
                            }

                            strQuery += " order by date desc";
                            cmd.CommandText = strQuery;

                            using (var adapter = new OleDbDataAdapter())
                            {
                                using (var ds = new DataSet())
                                {
                                    adapter.SelectCommand = cmd;
                                    adapter.SelectCommand.ExecuteNonQuery();

                                    ds.Clear();
                                    adapter.Fill(ds);

                                    dg_List.DataSource = ds.Tables[0];

                                    int nWidth = 0;

                                    for (int i = 0; i < dg_List.ColumnCount; i++)
                                    {
                                        dg_List.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

                                        if (dg_List.Columns[i].HeaderText == "Model")
                                            dg_List.Columns[i].Width = 100;
                                        else if (dg_List.Columns[i].HeaderText == "LotID")
                                            dg_List.Columns[i].Width = 150;
                                        else if (dg_List.Columns[i].HeaderText == "MoterID")
                                            dg_List.Columns[i].Width = 150;
                                        else if (dg_List.Columns[i].HeaderText == "Date")
                                            dg_List.Columns[i].Width = 250;
                                        else if (dg_List.Columns[i].HeaderText == "PrintData")
                                            dg_List.Columns[i].Width = 1000;
                                        else if (dg_List.Columns[i].HeaderText == "PrintType")
                                            dg_List.Columns[i].Width = 200;
                                    }

                                    //flySearch.HidePopup();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Data Search Error : " + ex.Message);
                    }
                    flySearch.HidePopup();
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            SearchData();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void dg_List_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                for (int i = 0; i < dg_List.ColumnCount; i++)
                {
                    if (dg_List.Columns[i].Name == "Result")
                    {
                        if (dg_List.Rows[e.RowIndex].Cells[i].Value.ToString() == "OK")
                        {
                            dg_List.Rows[e.RowIndex].Cells[i].Style.BackColor = Color.Lime;
                            dg_List.Rows[e.RowIndex].Cells[i].Style.ForeColor = Color.Black;
                        }
                        else if (dg_List.Rows[e.RowIndex].Cells[i].Value.ToString() == "NG")
                        {
                            dg_List.Rows[e.RowIndex].Cells[i].Style.BackColor = Color.Red;
                            dg_List.Rows[e.RowIndex].Cells[i].Style.ForeColor = Color.White;
                        }
                    }
                }
            }
            catch { }
        }

        private void dg_List_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int nSel = dg_List.CurrentRow.Index;

            if (nSel == -1)
                return;

            try
            {
                string strModel = dg_List.Rows[nSel].Cells[0].FormattedValue.ToString();
                string strLotID = dg_List.Rows[nSel].Cells[1].FormattedValue.ToString();
                string strCode = dg_List.Rows[nSel].Cells[2].FormattedValue.ToString();
                string strDate = Convert.ToString(Convert.ToDateTime(dg_List.Rows[nSel].Cells[3].FormattedValue.ToString()).ToString("yyyyMMdd"));
                string strTime = string.Format("{0}{1}{2}{3}", dg_List.Rows[nSel].Cells[4].FormattedValue.ToString().Substring(0, 2), dg_List.Rows[nSel].Cells[4].FormattedValue.ToString().Substring(2, 2), dg_List.Rows[nSel].Cells[4].FormattedValue.ToString().Substring(4, 2), dg_List.Rows[nSel].Cells[4].FormattedValue.ToString().Substring(6, 3));
                string strCamNo = dg_List.Rows[nSel].Cells[5].FormattedValue.ToString();

                string strRes = "";
                for (var i = 0; i < dg_List.Rows[nSel].Cells.Count; i++)
                {
                    if (dg_List.Rows[nSel].Cells[i].FormattedValue.ToString() == "OK" || dg_List.Rows[nSel].Cells[i].FormattedValue.ToString() == "NG")
                        strRes = dg_List.Rows[nSel].Cells[i].FormattedValue.ToString();
                }

                string strOriginFormat = _frmMain._var._nOriginImageFormat == 0 ? ".bmp" : ".jpg";
                string strResultFormat = _frmMain._var._nOriginImageFormat == 0 ? ".bmp" : ".jpg";


                _bmpMaster = null;
                _bmpOrigin = null;
                _bmpResult = null;
                var strImgPath = string.Format("{0}\\Camera_{1:D2}\\{2}.bmp", _frmMain._var._strMasterImagePath, Convert.ToInt32(strCamNo), strCode);
                FileInfo fi = new FileInfo(strImgPath);
                if (fi.Exists)
                    _bmpMaster = new Bitmap(Bitmap.FromFile(strImgPath));

                strImgPath = _frmMain._var._strSaveImagePath + "\\OriginImage\\" + strDate.Substring(0, 4) + "\\" + strDate.Substring(4, 2) + "\\" + strDate.Substring(6, 2) + "\\" + strModel + "_" + strLotID +
                    "\\" + strRes + "_Cam" + strCamNo + "_" + strModel + "_" + strLotID + "_" + strTime + strOriginFormat;

                fi = new FileInfo(strImgPath);
                if (fi.Exists)
                    _bmpOrigin = new Bitmap(strImgPath);

                strImgPath = _frmMain._var._strSaveImagePath + "\\ResultImage\\" + strDate.Substring(0, 4) + "\\" + strDate.Substring(4, 2) + "\\" + strDate.Substring(6, 2) + "\\" + strModel + "_" + strLotID +
                    "\\" + strRes + "_Cam" + strCamNo + "_" + strModel + "_" + strLotID + "_" + strTime + strResultFormat;
                fi = new FileInfo(strImgPath);
                if (fi.Exists)
                    _bmpResult = new Bitmap(strImgPath);

            }
            catch (Exception ex) { }
        }

        private void btnSaveTocvs_Click(object sender, EventArgs e)
        {
            if (dg_List.Rows.Count > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.InitialDirectory = "C:\\";
                sfd.Title = "SaveToCSV";
                sfd.Filter = "csv File(*.csv) | *.csv | All Files(*.*) | *.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                    SaveToCSV(sfd.FileName);
            }
            else
            {
                _frmMain.AddMsg("There is no data to save.", Color.Red, true, true, frmMain.MsgType.Alarm);
            }
        }

        private void SaveToCSV(string strPath)
        {
            try
            {
                DirectoryInfo dr = new DirectoryInfo(System.IO.Path.GetDirectoryName(strPath));
                if (!dr.Exists)
                    dr.Create();

                using (StreamWriter System_Log = new StreamWriter(strPath, append: true))
                {
                    for (int i = 0; i < dg_List.RowCount; i++)
                        System_Log.WriteLine(string.Format("{0},{1},{2},{3},{4}", i + 1, dg_List.Rows[i].Cells[1].FormattedValue.ToString(), dg_List.Rows[i].Cells[2].FormattedValue.ToString(), dg_List.Rows[i].Cells[3].FormattedValue.ToString(), dg_List.Rows[i].Cells[4].FormattedValue.ToString()));

                    System_Log.Close();

                    _frmMain.AddMsg("File Saved", Color.GreenYellow, false, false, frmMain.MsgType.Alarm);
                }
            }
            catch (Exception ex)
            {
                _frmMain.AddMsg("File Save Error : " + ex.Message, Color.Red, false, false, frmMain.MsgType.Alarm);
            }
        }

        private void chk2D_CheckedChanged(object sender, EventArgs e)
        {
            CheckEdit chkEdit = (sender as CheckEdit);

            if (chkEdit.Checked)
                chkEdit.ForeColor = Color.Yellow;
            else
                chkEdit.ForeColor = Color.White;

            var strPath = Application.StartupPath + "\\JobParam";

            if (chkEdit.TabIndex == 0)
                ini.WriteIniFile("Data", "Value", chkEdit.Checked.ToString(), strPath, "JobConfig.ini");
            else if (chkEdit.TabIndex == 1)
                ini.WriteIniFile("Align", "Value", chkEdit.Checked.ToString(), strPath, "JobConfig.ini");
            else
                ini.WriteIniFile("PinData", "Value", chkEdit.Checked.ToString(), strPath, "JobConfig.ini");
        }

        private void btnSearchMenu_Click(object sender, EventArgs e)
        {
            if (flySearch.IsPopupOpen)
                flySearch.HidePopup();
            else
                flySearch.ShowPopup();

        }
        private void btnPrint_Click(object sender, EventArgs e)
        {
            int nSel = dg_List.CurrentRow.Index;

            if (nSel == -1)
                return;

            try
            {
                string strModel = dg_List.Rows[nSel].Cells[0].FormattedValue.ToString();
                string strLotID = dg_List.Rows[nSel].Cells[2].FormattedValue.ToString();
                string strprintData = dg_List.Rows[nSel].Cells[4].FormattedValue.ToString();

                _frmMain.SetEVHistoryPrint(strModel,strLotID, strprintData);
            }
            catch { }
        }
    }
}