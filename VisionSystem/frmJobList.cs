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
    public partial class frmJobList : DevExpress.XtraEditors.XtraForm
    {
        frmMain _frmMain;
        Bitmap _bmpMaster = null;
        Bitmap _bmpOrigin = null;
        Bitmap _bmpResult = null;
        IniFiles ini = new IniFiles();

        public frmJobList(frmMain MainFrm)
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
            ActiveControl = labelControl1;
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

            cbResult.SelectedIndex = 0;
        }

        private void SearchData()
        {
            if (_frmMain._var._bUsePrint)
            {
            }
            else
            {
                if (_frmMain._OleDB.State == ConnectionState.Open)
                {
                    try
                    {
                        if (!chkold.Checked)
                        {
                            using (var cmd = new OleDbCommand())
                            {
                                dg_List.DataSource = null;

                                cmd.CommandType = CommandType.Text;
                                cmd.Connection = _frmMain._OleDB;

                                var strQuery = "Select Model, LotID, Date, TotalResult"; //from Data where [Date] >= ? and [Date] <= 
                                for (int i = 0; i < _frmMain._var._nScreenCnt; i++)
                                {
                                    strQuery += ", Cam" + string.Format("{0:D2}", i + 1);
                                    if (_frmMain._ModelParam[i].bAlingInsp)
                                    {
                                        strQuery += ", Cam" + string.Format("{0:D2}", i + 1) + "_AlignX";
                                        strQuery += ", Cam" + string.Format("{0:D2}", i + 1) + "_AlignY";
                                        strQuery += ", Cam" + string.Format("{0:D2}", i + 1) + "_AlignZ";
                                    }
                                    if (_frmMain._ModelParam[i].bBCRInsp)
                                    {
                                        strQuery += ", Cam" + string.Format("{0:D2}", i + 1) + "_BCR";
                                    }
                                    if (_frmMain._ModelParam[i].bPinChange)
                                    {
                                        strQuery += ", Cam" + string.Format("{0:D2}", i + 1) + "_Pin";
                                    }
                                    if (_frmMain._var._bUseSensor)
                                    {
                                        if (i == 1)
                                        {
                                            strQuery += ", Cam" + string.Format("{0:D2}", i + 1) + "_1PointValue";
                                            strQuery += ", Cam" + string.Format("{0:D2}", i + 1) + "_2PointValue";
                                            strQuery += ", Cam" + string.Format("{0:D2}", i + 1) + "_3PointValue";
                                            strQuery += ", Cam" + string.Format("{0:D2}", i + 1) + "_4PointValue";
                                            strQuery += ", Cam" + string.Format("{0:D2}", i + 1) + "_5PointValue";
                                            strQuery += ", Cam" + string.Format("{0:D2}", i + 1) + "_6PointValue";

                                        }
                                    }

                                }

                                strQuery += " from Data where [Date] >= ? and [Date] <= ?";

                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("@Date", Convert.ToDateTime(date1.DateTime.ToString("yyyy-MM-dd 00:00:00")));
                                cmd.Parameters.Add("@Date", Convert.ToDateTime(date2.DateTime.ToString("yyyy-MM-dd 23:59:59")));

                                if (cbCode.SelectedIndex > 0)
                                {
                                    strQuery += " and [Model] like ?";
                                    cmd.Parameters.AddWithValue("@Model", string.Format("%{0}%", cbCode.SelectedItem.ToString()));
                                }

                                if (!string.IsNullOrEmpty(txtLotID.Text))
                                {
                                    strQuery += " and [LotID] like ?";
                                    cmd.Parameters.AddWithValue("@LotID", string.Format("%{0}%", txtLotID.Text.Trim()));
                                }
                                if (cbResult.SelectedIndex > 0)
                                {
                                    strQuery += " and [TotalResult] like ?";
                                    cmd.Parameters.AddWithValue("@TotalResult", string.Format("{0}", cbResult.SelectedItem.ToString()));
                                }
                                strQuery += " order by date, time desc";
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

                                        int iTotalCount = 0;
                                        int[] camCount = new int[_frmMain._var._nScreenCnt];
                                        string[] SumValue = new string[_frmMain._var._nScreenCnt + 1];

                                        #region 카운트 확인 및 퍼센트 확인 
                                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                        {
                                            for (int j = 0; j < ds.Tables[0].Columns.Count; j++)
                                            {
                                                if (ds.Tables[0].Columns[j].ColumnName == "TotalResult")
                                                {
                                                    if (ds.Tables[0].Rows[i][j].ToString() == "OK")
                                                    {
                                                        iTotalCount++;
                                                    }
                                                }
                                                for (int k = 0; k < _frmMain._var._nScreenCnt; k++)
                                                {
                                                    if (ds.Tables[0].Columns[j].ColumnName == "Cam" + string.Format("{0:D2}", k + 1))
                                                    {
                                                        if (ds.Tables[0].Rows[i][j].ToString() == "OK")
                                                        {
                                                            camCount[k]++;
                                                        }
                                                    }
                                                }

                                            }
                                        }

                                        for (int j = 0; j < ds.Tables[0].Columns.Count; j++)
                                        {
                                            if (ds.Tables[0].Columns[j].ColumnName == "TotalResult")
                                            {
                                                SumValue[0] = string.Format("{0} ({1:N2})", (float)ds.Tables[0].Rows.Count, (float)((float)iTotalCount / (float)ds.Tables[0].Rows.Count) * 100) + "%";
                                            }
                                            for (int k = 0; k < _frmMain._var._nScreenCnt; k++)
                                            {
                                                if (ds.Tables[0].Columns[j].ColumnName == "Cam" + string.Format("{0:D2}", k + 1))
                                                {
                                                    SumValue[k + 1] = string.Format("{0:N2}", (float)((float)camCount[k] / (float)ds.Tables[0].Rows.Count) * 100) + "%";
                                                }
                                            }
                                        }
                                        string[] tableData = new string[ds.Tables[0].Columns.Count];
                                        for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                                        {
                                            if (ds.Tables[0].Columns[i].ColumnName == "TotalResult")
                                                tableData[i] = SumValue[0];

                                            for (int k = 0; k < _frmMain._var._nScreenCnt; k++)
                                            {
                                                if (ds.Tables[0].Columns[i].ColumnName == "Cam" + string.Format("{0:D2}", k + 1))
                                                {

                                                    tableData[i] = SumValue[k + 1];
                                                }
                                            }
                                        }
                                        ds.Tables[0].Rows.Add(tableData);
                                        #endregion

                                        #region Size 조정
                                        int nWidth = 0;

                                        nWidth = (int)((dg_List.Width - 20 - 100) / dg_List.ColumnCount - 1);

                                        for (int i = 0; i < dg_List.ColumnCount; i++)
                                        {
                                            dg_List.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

                                            if (dg_List.Columns[i].HeaderText == "Data" || dg_List.Columns[i].HeaderText == "Data(2D)")
                                            {
                                                dg_List.Columns[i].HeaderText = "Data(2D)";
                                                dg_List.Columns[i].Width = 350;
                                            }
                                            else if (dg_List.Columns[i].HeaderText == "Model")
                                                dg_List.Columns[i].Width = 100;
                                            else if (dg_List.Columns[i].HeaderText == "Date")
                                                dg_List.Columns[i].Width = 200;
                                            else if (dg_List.Columns[i].HeaderText == "LotID")
                                                dg_List.Columns[i].Width = 150;
                                            else if (dg_List.Columns[i].HeaderText == "TotalResult")
                                                dg_List.Columns[i].Width = 150;
                                            else
                                                dg_List.Columns[i].Width = nWidth;
                                            for (int j = 0; j < _frmMain._var._nScreenCnt; j++)
                                            {
                                                if (dg_List.Columns[i].HeaderText == "Cam" + string.Format("{0:D2}", j + 1))
                                                {
                                                    dg_List.Columns[i].Width = 100;
                                                }
                                                if (dg_List.Columns[i].HeaderText == "Cam" + string.Format("{0:D2}", j + 1) + "_BCR")
                                                {
                                                    string _str2D = dg_List.Rows[0].Cells[i].Value.ToString();
                                                    int i2DSize = _str2D.Length;
                                                    dg_List.Columns[i].Width = 100 + (i2DSize * 10);
                                                }
                                                if (dg_List.Columns[i].HeaderText.Contains("PointValue"))
                                                    dg_List.Columns[i].Width = 150;
                                            }
                                        }
                                        #endregion
                                        //flySearch.HidePopup();
                                    }
                                }
                            }

                        }
                        else
                        {
                            using (var cmd = new OleDbCommand())
                            {
                                dg_List.DataSource = null;

                                cmd.CommandType = CommandType.Text;
                                cmd.Connection = _frmMain._OleDB;

                                var strQuery = "Select Model, LotID, Code, Date, Time, CameraNo"; //from Data where [Date] >= ? and [Date] <= ?";

                                strQuery += ", Result from Data where [Date] >= ? and [Date] <= ?";

                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("@Date", Convert.ToDateTime(date1.DateTime.ToString("yyyy-MM-dd")));
                                cmd.Parameters.Add("@Date", Convert.ToDateTime(date2.DateTime.ToString("yyyy-MM-dd")));

                                if (cbCode.SelectedIndex > 0)
                                {
                                    strQuery += " and [Model] = ?";
                                    cmd.Parameters.AddWithValue("@Model", string.Format("%{0}%", cbCode.SelectedItem.ToString()));

                                }

                                if (cbResult.SelectedIndex > 0)
                                {
                                    strQuery += " and [Result] = ?";
                                    cmd.Parameters.AddWithValue("@Result", string.Format("{0}", cbResult.SelectedItem.ToString()));
                                }

                                strQuery += " order by date, time desc";
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

                                        nWidth = (int)((dg_List.Width - 20 - 100) / dg_List.ColumnCount - 1);

                                        for (int i = 0; i < dg_List.ColumnCount; i++)
                                        {
                                            dg_List.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

                                            if (dg_List.Columns[i].HeaderText == "Data" || dg_List.Columns[i].HeaderText == "Data(2D)")
                                            {
                                                dg_List.Columns[i].HeaderText = "Data(2D)";
                                                dg_List.Columns[i].Width = 350;
                                            }
                                            else
                                                dg_List.Columns[i].Width = nWidth;

                                            if (dg_List.Columns[i].HeaderText == "CameraNo")
                                                dg_List.Columns[i].Width = 100;
                                        }
                                    }
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
                    if (dg_List.Columns[i].Name == "TotalResult")
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
                    for (int j = 0; j < _frmMain._var._nScreenCnt; j++)
                    {
                        if (dg_List.Columns[i].Name == "Cam" + string.Format("{0:D2}", j + 1))
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
            }
            catch { }
        }

        private void dg_List_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int nSel = dg_List.CurrentRow.Index;
            int nSel2 = dg_List.CurrentCell.ColumnIndex;

            if (nSel == -1)
                return;

            try
            {
                string strModel = dg_List.Rows[nSel].Cells[0].FormattedValue.ToString();
                string strLotID = dg_List.Rows[nSel].Cells[1].FormattedValue.ToString();
                string strCode = "";
                string strCamNo = "";
                string strDate = "";
                string strTime = "";
                string strTotalResult = dg_List.Rows[nSel].Cells[3].FormattedValue.ToString();
                if (!chkold.Checked)
                {
                    strCamNo = dg_List.Columns[nSel2].Name.ToString();
                    strDate = Convert.ToString(Convert.ToDateTime(dg_List.Rows[nSel].Cells[2].Value.ToString()).ToString("yyyy-MM-dd HH:mm:ss"));

                    if (strCamNo.Length > 5)
                        return;
                    string strRes = dg_List.Rows[nSel].Cells[nSel2].FormattedValue.ToString(); ;
                    string strOriginFormat = _frmMain._var._nOriginImageFormat == 0 ? ".bmp" : ".jpg";
                    string strResultFormat = _frmMain._var._nOriginImageFormat == 0 ? ".bmp" : ".jpg";

                    int.TryParse(strCamNo.Substring(3, strCamNo.Length - 3), out int _icode);
                    strCode = ini.ReadIniFile("Code", "Value", _frmMain._var._strModelPath + "\\" + strModel, string.Format("CAM{0}.ini", _icode.ToString()));
                    _bmpMaster = null;
                    _bmpOrigin = null;
                    _bmpResult = null;
                    var strImgPath = string.Format("{0}\\Camera_{1:D2}\\{2}.bmp", _frmMain._var._strMasterImagePath, Convert.ToInt32(strCamNo.Substring(strCamNo.Length - 2, 2)), strCode);
                    FileInfo fi = new FileInfo(strImgPath);
                    if (fi.Exists)
                        _bmpMaster = new Bitmap(Bitmap.FromFile(strImgPath));

                    if (_bmpOrigin == null)
                    {
                        strImgPath = _frmMain._var._strSaveImagePath + "\\OriginImage\\" + strDate.Substring(0, 4) + "\\" + strDate.Substring(5, 2) + "\\" + strDate.Substring(8, 2) + "\\" + strModel + "_" + strLotID +
                            "\\" + strRes + "_Cam" + strCamNo.Substring(strCamNo.Length - 2, 2) + "_" + strModel + "_" + strLotID + "_" + strDate.Substring(11, 2) + strDate.Substring(14, 2) + strDate.Substring(17, 2) + ".bmp";

                        fi = new FileInfo(strImgPath);
                        if (fi.Exists)
                            _bmpOrigin = new Bitmap(strImgPath);
                    }
                    if (_bmpOrigin == null)
                    {
                        strImgPath = _frmMain._var._strSaveImagePath + "\\OriginImage\\" + strDate.Substring(0, 4) + "\\" + strDate.Substring(5, 2) + "\\" + strDate.Substring(8, 2) + "\\" + strTotalResult + "\\" + strModel + "_" + strLotID +
                            "\\" + strRes + "_Cam" + strCamNo.Substring(strCamNo.Length - 2, 2) + "_" + strModel + "_" + strLotID + "_" + strDate.Substring(11, 2) + strDate.Substring(14, 2) + strDate.Substring(17, 2) + ".bmp";

                        fi = new FileInfo(strImgPath);
                        if (fi.Exists)
                            _bmpOrigin = new Bitmap(strImgPath);
                    }
                    if (_bmpOrigin == null)
                    {
                        strImgPath = _frmMain._var._strSaveImagePath + "\\OriginImage\\" + strDate.Substring(0, 4) + "\\" + strDate.Substring(5, 2) + "\\" + strDate.Substring(8, 2) + "\\" + strModel + "_" + strLotID +
                            "\\" + strRes + "_Cam" + strCamNo.Substring(strCamNo.Length - 2, 2) + "_" + strModel + "_" + strLotID + "_" + strDate.Substring(11, 2) + strDate.Substring(14, 2) + strDate.Substring(17, 2) + ".jpg";

                        fi = new FileInfo(strImgPath);
                        if (fi.Exists)
                            _bmpOrigin = new Bitmap(strImgPath);
                    }
                    if (_bmpOrigin == null)
                    {
                        strImgPath = _frmMain._var._strSaveImagePath + "\\OriginImage\\" + strDate.Substring(0, 4) + "\\" + strDate.Substring(5, 2) + "\\" + strDate.Substring(8, 2) + "\\" + strTotalResult + "\\" + strModel + "_" + strLotID +
                            "\\" + strRes + "_Cam" + strCamNo.Substring(strCamNo.Length - 2, 2) + "_" + strModel + "_" + strLotID + "_" + strDate.Substring(11, 2) + strDate.Substring(14, 2) + strDate.Substring(17, 2) + ".jpg";

                        fi = new FileInfo(strImgPath);
                        if (fi.Exists)
                            _bmpOrigin = new Bitmap(strImgPath);
                    }
                    if (_bmpResult == null)
                    {
                        strImgPath = _frmMain._var._strSaveResultImagePath + "\\ResultImage\\" + strDate.Substring(0, 4) + "\\" + strDate.Substring(5, 2) + "\\" + strDate.Substring(8, 2) + "\\" + strModel + "_" + strLotID +
                            "\\" + strRes + "_Cam" + strCamNo.Substring(strCamNo.Length - 2, 2) + "_" + strModel + "_" + strLotID + "_" + strDate.Substring(11, 2) + strDate.Substring(14, 2) + strDate.Substring(17, 2) + ".jpg";
                        fi = new FileInfo(strImgPath);
                        if (fi.Exists)
                            _bmpResult = new Bitmap(strImgPath);
                    }
                    if (_bmpResult == null)
                    {
                        strImgPath = _frmMain._var._strSaveResultImagePath + "\\ResultImage\\" + strDate.Substring(0, 4) + "\\" + strDate.Substring(5, 2) + "\\" + strDate.Substring(8, 2) + "\\" + strTotalResult + "\\" + strModel + "_" + strLotID +
                           "\\" + strRes + "_Cam" + strCamNo.Substring(strCamNo.Length - 2, 2) + "_" + strModel + "_" + strLotID + "_" + strDate.Substring(11, 2) + strDate.Substring(14, 2) + strDate.Substring(17, 2) + ".jpg";
                        fi = new FileInfo(strImgPath);
                        if (fi.Exists)
                            _bmpResult = new Bitmap(strImgPath);
                    }
                    if (_bmpResult == null)
                    {
                        strImgPath = _frmMain._var._strSaveResultImagePath + "\\ResultImage\\" + strDate.Substring(0, 4) + "\\" + strDate.Substring(5, 2) + "\\" + strDate.Substring(8, 2) + "\\" + strModel + "_" + strLotID +
                            "\\" + strRes + "_Cam" + strCamNo.Substring(strCamNo.Length - 2, 2) + "_" + strModel + "_" + strLotID + "_" + strDate.Substring(11, 2) + strDate.Substring(14, 2) + strDate.Substring(17, 2) + ".bmp";
                        fi = new FileInfo(strImgPath);
                        if (fi.Exists)
                            _bmpResult = new Bitmap(strImgPath);
                    }
                    if (_bmpResult == null)
                    {
                        strImgPath = _frmMain._var._strSaveResultImagePath + "\\ResultImage\\" + strDate.Substring(0, 4) + "\\" + strDate.Substring(5, 2) + "\\" + strDate.Substring(8, 2) + "\\" + strTotalResult + "\\" + strModel + "_" + strLotID +
                           "\\" + strRes + "_Cam" + strCamNo.Substring(strCamNo.Length - 2, 2) + "_" + strModel + "_" + strLotID + "_" + strDate.Substring(11, 2) + strDate.Substring(14, 2) + strDate.Substring(17, 2) + ".bmp";
                        fi = new FileInfo(strImgPath);
                        if (fi.Exists)
                            _bmpResult = new Bitmap(strImgPath);
                    }

                    ShowImage("Master");

                    if (swGraphic.IsOn)
                        ShowImage("Result");
                    else
                        ShowImage("Origin");
                }
                else
                {
                    strCode = dg_List.Rows[nSel].Cells[2].FormattedValue.ToString();
                    strDate = Convert.ToString(Convert.ToDateTime(dg_List.Rows[nSel].Cells[3].FormattedValue.ToString()).ToString("yyyyMMdd"));
                    strTime = string.Format("{0}{1}{2}{3}", dg_List.Rows[nSel].Cells[4].FormattedValue.ToString().Substring(0, 2), dg_List.Rows[nSel].Cells[4].FormattedValue.ToString().Substring(2, 2), dg_List.Rows[nSel].Cells[4].FormattedValue.ToString().Substring(4, 2), dg_List.Rows[nSel].Cells[4].FormattedValue.ToString().Substring(6, 3));
                    strCamNo = dg_List.Rows[nSel].Cells[5].FormattedValue.ToString();

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
                        "\\" + strRes + "_Cam" + strCamNo + "_" + strCode + "_" + strModel + "_" + strLotID + "_" + strTime + strOriginFormat;

                    fi = new FileInfo(strImgPath);
                    if (fi.Exists)
                        _bmpOrigin = new Bitmap(strImgPath);

                    strImgPath = _frmMain._var._strSaveImagePath + "\\ResultImage\\" + strDate.Substring(0, 4) + "\\" + strDate.Substring(4, 2) + "\\" + strDate.Substring(6, 2) + "\\" + strModel + "_" + strLotID +
                        "\\" + strRes + "_Cam" + strCamNo + "_" + strCode + "_" + strModel + "_" + strLotID + "_" + strTime + strResultFormat;
                    fi = new FileInfo(strImgPath);
                    if (fi.Exists)
                        _bmpResult = new Bitmap(strImgPath);

                    ShowImage("Master");
                    ShowImage("Result");
                }
            }
            catch (Exception ex) { }
        }

        private void ShowImage(string strType)
        {
            if (strType == "Master")
            {
                if (_bmpMaster != null)
                {
                    cogMasterDisp.AutoFit = true;
                    cogMasterDisp.Image = new CogImage24PlanarColor(_bmpMaster);
                }
                else
                    MessageBox.Show("No Master Image", "No Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (strType == "Origin")
            {
                if (_bmpOrigin != null)
                {
                    cogResDisp.AutoFit = true;
                    cogResDisp.Image = new CogImage24PlanarColor(_bmpOrigin);
                }
                else
                {
                    cogResDisp.Image = null;
                    MessageBox.Show("No Original Image", "No Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                if (_bmpResult != null)
                {
                    cogResDisp.AutoFit = true;
                    cogResDisp.Image = new CogImage24PlanarColor(_bmpResult);
                }
                else
                {
                    cogResDisp.Image = null;
                    MessageBox.Show("No Result Image", "No Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void swGraphic_Toggled(object sender, EventArgs e)
        {
            if (swGraphic.IsOn)
                ShowImage("Result");
            else
                ShowImage("Origin");
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
                    {
                        string CSVSaveData = string.Format("{0}", i + 1);
                        for (int j = 0; j < dg_List.ColumnCount; j++)
                        {
                            CSVSaveData += ",";
                            CSVSaveData += string.Format("{0}", dg_List.Rows[i].Cells[j].FormattedValue.ToString());
                        }
                            System_Log.WriteLine(CSVSaveData);
                    }

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
    }
}