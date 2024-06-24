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
using System.Data.OleDb;

namespace VisionSystem
{
    public delegate void PrintMessageHaldler(string strMsg, Color color, bool bShoPopup, bool bError);
    public delegate void MaunalPrintHalder(string[] strdata);
    public delegate void RePrintHanlder(string[] strData);

    public partial class PrintControl : DevExpress.XtraEditors.XtraUserControl
    {
        public GlovalVar.PrintParam _PrintParam;
        public OleDbConnection _PrintOleDB = null; //데이터 저장

        public PrintMessageHaldler _OnMessage;
        public MaunalPrintHalder _OnMaunalPrint;
        public GlovalVar _var;
        public RePrintHanlder _OnRePrint;

        DataTable _dt = new DataTable();
        public PrintControl()
        {
            InitializeComponent();
            tablePanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            LoadData();
            ini_print();
            //PrintSearchData();
        }
        private void LoadModel()
        {
            for (int i = 0; i < _var._listModel.Count; i++)
            {
                cbBModel.Items.Add(_var._listModel[i]);
            }
        }
        private void ini_print()
        {
            
            _dt.Columns.Add("Time", typeof(string));
            _dt.Columns.Add("Model", typeof(string));
            _dt.Columns.Add("Seiral", typeof(string));
            _dt.Columns.Add("Model Seiral", typeof(string));
            _dt.Columns.Add("Print Data", typeof(string));
            _dt.Columns.Add("print Type", typeof(string));
        }
        public void printMessage(string _strMsg, Color _color, bool _bShoPopup, bool _bError)
        {
            if (_OnMessage != null)
                _OnMessage(_strMsg, _color, _bShoPopup, _bError);
        }
        public void ViewData(string strData)
        {
            Invoke(new EventHandler(delegate
            {
                lblVIewData.Text = strData;
            }));
        }
        public void ViewMotorData(string strData)
        {
            Invoke(new EventHandler(delegate
            {
                lblMotorData.Text = strData;
            }));
        }
        public void ModelChange()
        {

        }
        public void LoadData()
        {
            try
            {
                string strConn = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Application.StartupPath + @"\Data\Print.accdb";
                _PrintOleDB = new OleDbConnection(strConn);
                _PrintOleDB.Open();

                if (_PrintOleDB.State == ConnectionState.Open)
                    printMessage(Str.dataOpen, Color.Green, false, false);
            }
            catch (Exception ex)
            {
                printMessage(Str.dataloaderr + ex.Message, Color.Red, false, false);
            }
        }
        private void labelControl11_Click(object sender, EventArgs e)
        {

        }

        public void lblLabel_status(bool _bstatus)
        {
            if (!_bstatus)
                lblLabelStatus.BackColor = Color.Red;
            else
                lblLabelStatus.BackColor = Color.Lime;
        }
        public void lblRibben_status(bool _bstatus)
        {
            if (!_bstatus)
                lblRibbenStatus.BackColor = Color.Red;
            else
                lblRibbenStatus.BackColor = Color.Lime;
        }
        private void btnControl_Click(object sender, EventArgs e)
        {
            if (flyManual.IsPopupOpen)
                flyManual.HideBeakForm();

            if (!fypnl.IsPopupOpen)
                fypnl.ShowPopup();
            else
                fypnl.HidePopup();
        }

        private void panelControl2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnChk_Click(object sender, EventArgs e)
        {
            if (!flyManual.IsPopupOpen)
                flyManual.ShowBeakForm();
            else
                flyManual.HideBeakForm();
        }

        private void lblMenu_Click(object sender, EventArgs e)
        {

        }

        private void btnChkSave_Click(object sender, EventArgs e)
        {
            if (cbBModel.Items[cbBModel.SelectedIndex].ToString() != null && txtSerial.Text != "" && txtCount.Text != "")
            {
                string[] strMaunalPrintData = new string[3];
                strMaunalPrintData[0] = txtCount.Text;
                strMaunalPrintData[1] = cbBModel.Items[cbBModel.SelectedIndex].ToString();
                strMaunalPrintData[2] = txtSerial.Text;
                if (_OnMaunalPrint != null)
                    _OnMaunalPrint(strMaunalPrintData);

                if (flyManual.IsPopupOpen)
                    flyManual.HideBeakForm();
            }
            else
            {
                MessageBox.Show("입력데이터를 확인해주세요");
            }


        }
        private void PrintSearchData()
        {
            if (_PrintOleDB.State == ConnectionState.Open)
            {
                try
                {
                    using (var cmd = new OleDbCommand())
                    {
                        dg_Print.DataSource = null;

                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = _PrintOleDB;

                        var strQuery = "Select Model, LotID, Date, PrintData, PrintType"; //from Data where [Date] >= ? and [Date] <= ?";

                        strQuery += " from Data ";

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

                                dg_Print.DataSource = ds.Tables[0];

                                int nWidth = 0;


                                for (int i = 0; i < dg_Print.ColumnCount; i++)
                                {
                                    dg_Print.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

                                    if (dg_Print.Columns[i].HeaderText == "Model")
                                        dg_Print.Columns[i].Width = 200;
                                    else if (dg_Print.Columns[i].HeaderText == "LotID")
                                        dg_Print.Columns[i].Width = 250;
                                    else if (dg_Print.Columns[i].HeaderText == "Date")
                                        dg_Print.Columns[i].Width = 250;
                                    else if (dg_Print.Columns[i].HeaderText == "PrintData")
                                        dg_Print.Columns[i].Width = 500;
                                    else if (dg_Print.Columns[i].HeaderText == "PrintType")
                                        dg_Print.Columns[i].Width = 200; ;
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

                //flySearch.HidePopup();
            }
        }
        public void PrintData_Write(string strModel, string strSerial,string strSerial2, string strPrintData, string strPrintType)
        {
            Invoke(new EventHandler(delegate
            {

                DateTime _NowTime = DateTime.Now;
                DataRow _dr = _dt.NewRow();
                _dr[0] = _NowTime.ToString();
                _dr[1] = strModel;
                _dr[2] = strSerial;
                _dr[3] = strSerial2;
                _dr[4] = strPrintData;
                _dr[5] = strPrintType;
                _dt.Rows.InsertAt(_dr, 0);

                if (_dt.Rows.Count > 300)
                {
                    _dt.Rows.RemoveAt(_dt.Rows.Count - 1);
                }

                dg_Print.DataSource = _dt;

                dg_Print.Columns[0].Width = 150;
                dg_Print.Columns[1].Width = 100;
                dg_Print.Columns[2].Width = 150;
                dg_Print.Columns[3].Width = 150;
                dg_Print.Columns[4].Width = 700;
                dg_Print.Columns[5].Width = 200;

                InsertData(_NowTime, strModel, strSerial, strSerial2, strPrintData, strPrintType);
            }));
        }
        private void InsertData(DateTime InspdateTime, string strModel, string strSerial,string strSerial2, string strPrintData, string strPrintType)
        {
            if (_PrintOleDB.State == ConnectionState.Open)
            {
                try
                {
                    using (OleDbCommand insertCommand = new OleDbCommand("INSERT INTO Data ([Model],[LotID],[MotorID],[Date],[PrintData],[PrintType]) VALUES (?,?,?,?,?,?)", _PrintOleDB))
                    {
                        insertCommand.Parameters.AddWithValue("@Model", strModel);
                        insertCommand.Parameters.AddWithValue("@LotID", strSerial);
                        insertCommand.Parameters.AddWithValue("@MotorID", strSerial2);
                        insertCommand.Parameters.AddWithValue("@Date", Convert.ToDateTime(InspdateTime.ToString("yyyy-MM-dd HH:mm:ss")));
                        insertCommand.Parameters.AddWithValue("@PrintData", strPrintData);
                        insertCommand.Parameters.AddWithValue("@PrintType", strPrintType);
                        insertCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void btnChkClose_Click(object sender, EventArgs e)
        {
            if (flyManual.IsPopupOpen)
                flyManual.HideBeakForm();
        }

        private void btnCamExpose_Click(object sender, EventArgs e)
        {
            string[] strListReprint = new string[3];
            if(dg_Print.SelectedRows.Count > 0)
            {
                strListReprint[0] = "1";
                strListReprint[1] = dg_Print.Rows[dg_Print.SelectedRows.Count-1].Cells[1].Value.ToString();
                strListReprint[2] = dg_Print.Rows[dg_Print.SelectedRows.Count-1].Cells[3].Value.ToString();
                if (_OnRePrint != null)
                    _OnRePrint(strListReprint);
            }
        }

        private void PrintControl_Load(object sender, EventArgs e)
        {
            LoadModel();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
