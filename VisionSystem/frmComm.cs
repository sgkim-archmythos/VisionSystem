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
using System.Text.RegularExpressions;
using System.Threading;

namespace VisionSystem
{
    public partial class frmComm : DevExpress.XtraEditors.XtraForm
    {
        frmMain _frmMain;
        IniFiles ini = new IniFiles();
        string[] strCpuType = new string[7] { "S7300", "S7400", "S71200", "S71500", "S7200", "S7200Smart", "Logo0BA8" };

        bool _bRead = false;
        bool _bWrite = false;

        int _nDataType = (int)DataType.DEC;

        private enum DataType
        {
            DEC,
            BINARY,
            HEX,
            ASCII
        }

        GlovalVar.PLCPram _plcParam = new GlovalVar.PLCPram();
        public frmComm(frmMain MainFrm)
        {
            InitializeComponent();
            _frmMain = MainFrm;
        }

        private void frmComm_Load(object sender, EventArgs e)
        {
            LoadSet();

            if (_frmMain._bCommConnect)
            {
                btnRead.Enabled = true;
                btnWrite.Enabled = true;
            }
        }

        private void InitControl(int nType, string strType)
        {
            string strAddr = "";
            string strHeadAddr = "";
            int nAddr = 0, nLen = 0;
            if (nType == (int)GlovalVar.PLCType.MX)
            {
                if (strType == "Read")
                {
                    if (string.IsNullOrEmpty(txtMXReadStartAddr.Text) && string.IsNullOrEmpty(txtMXReadLength.Text))
                        return;

                    strAddr = Regex.Replace(txtMXReadStartAddr.Text, @"\D", "");
                    strHeadAddr = Regex.Replace(txtMXReadStartAddr.Text, @"\d", "");
                    int.TryParse(strAddr, out nAddr);
                    int.TryParse(txtMXReadLength.Text, out nLen);
                }
                else
                {
                    if (string.IsNullOrEmpty(txtMXWriteAddr.Text))
                        return;

                    strAddr = Regex.Replace(txtMXWriteAddr.Text, @"\D", "");
                    strHeadAddr = Regex.Replace(txtMXWriteAddr.Text, @"\d", "");
                    int.TryParse(strAddr, out nAddr);
                    nLen = 30;
                }
            }
            else if (nType == (int)GlovalVar.PLCType.Simens)
            {

            }
            else if (nType == (int)GlovalVar.PLCType.IO)
            {

            }
            else
            {
                if (strType == "Read")
                {
                    if (string.IsNullOrEmpty(txtLSReadStartAddr.Text) && string.IsNullOrEmpty(txtLSReadLength.Text))
                        return;

                    strAddr = Regex.Replace(txtLSReadStartAddr.Text, @"\D", "");
                    strHeadAddr = Regex.Replace(txtLSReadStartAddr.Text, @"\d", "");
                    int.TryParse(strAddr, out nAddr);
                    int.TryParse(txtLSReadLength.Text, out nLen);
                }
                else
                {
                    if (string.IsNullOrEmpty(txtLSWriteAddr.Text))
                        return;

                    strAddr = Regex.Replace(txtLSWriteAddr.Text, @"\D", "");
                    strHeadAddr = Regex.Replace(txtLSWriteAddr.Text, @"\d", "");
                    int.TryParse(strAddr, out nAddr);
                    nLen = 30;
                }
            }

            if (strType == "Read")
            {
                dgRead.Rows.Clear();

                for (int i = 0; i < nLen; i++)
                    dgRead.Rows.Add(string.Format("{0}{1}", strHeadAddr, nAddr + i), "");
            }
            else
            {
                dgWrite.Rows.Clear();

                for (int i = 0; i < 50; i++)
                    dgWrite.Rows.Add(string.Format("{0}{1}", strHeadAddr, nAddr + i), "");
            }
        }

        private void LoadSet()
        {
            if (_frmMain._bCommConnect)
            {
                lblStatus.BackColor = Color.Lime;
                lblStatus.ForeColor = Color.Black;
                lblStatus.Text = "Connect";
            }

            radDec.Checked = true;

            dgRead.Columns.Clear();
            dgRead.Columns.Add("Addr", "Addr");
            dgRead.Columns.Add("Data", "Data");

            dgWrite.Columns.Clear();
            dgWrite.Columns.Add("Addr", "Addr");
            dgWrite.Columns.Add("Data", "Data");

            dgRead.Columns[0].Width = 73;
            dgRead.Columns[1].Width = 149;

            dgWrite.Columns[0].Width = 73;
            dgWrite.Columns[1].Width = 149;

            dgRead.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgWrite.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            for (int i = 0; i < 2; i++)
            {
                dgRead.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgRead.Columns[i].ReadOnly = true;
                dgRead.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

                dgWrite.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                dgWrite.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                if (i == 0)
                    dgWrite.Columns[i].ReadOnly = true;
                else
                    dgWrite.Columns[i].ReadOnly = false;
            }

            _plcParam = _frmMain._plcParam;
            cbSiemensType.Properties.Items.AddRange(strCpuType);
            var nType = 0;
            if (_plcParam.nCommType == (int)GlovalVar.PLCType.MX)
            {
                swMX.IsOn = true;

                txtMXIP.Text = _plcParam.strPLCIP;
                txtMXPort.Text = _plcParam.strPLCPort;
                txtMXHeartbeatAddr.Text = _plcParam.strHeartBeatAddr;
                txtMXHeartbeatInterval.Text = _plcParam.nHearBeatInterval.ToString();
                txtMXReadInterval.Text = _plcParam.nReadInterval.ToString();
                txtMXSignal.Text = _plcParam.strStartSignal;
                txtMXOK.Text = _plcParam.strOKSignal;
                txtMXNG.Text = _plcParam.strNGSignal;
                txtMXReadStartAddr.Text = _plcParam.strReadStartAddr;
                txtMXReadLength.Text = _plcParam.strReadLength;
                txtMXReadTriggerStart.Text = _plcParam.strReadStartTrigger;
                txtMXReadTriggerEnd.Text = _plcParam.strReadEndTrigger;
                txtMXReadModelStart.Text = _plcParam.strReadModelStart;
                txtMXReadModelEnd.Text = _plcParam.strReadModelEnd;
                txtMXReadLotStart.Text = _plcParam.strReadLotStart;
                txtMXReadLotEnd.Text = _plcParam.strReadLotEnd;
                txtMXReadCamPassStart.Text = _plcParam.strReadCamPassStart;
                txtMXReadCamPassEnd.Text = _plcParam.strReadCamPassEnd;
                txtMXWriteAddr.Text = _plcParam.strWriteStartAddr;
                txtMXWriteResStart.Text = _plcParam.strWriteResStart;
                txtMXWriteTotalRes.Text = _plcParam.strWriteTotalRes;
                txtMXWrite2DRes.Text = _plcParam.strWrite2DRes;
                txtMXWrite2DData.Text = _plcParam.strWrite2DData;
                txtMXWriteAlignX.Text = _plcParam.strWriteAlignX;
                txtMXWriteAlignY.Text = _plcParam.strWriteAlignY;
                txtMXWriteAlignZ.Text = _plcParam.strWriteAlignZ;
                txtMXWritePinRes.Text = _plcParam.strWritePinChange;
                chkMXndividualTrigger.Checked = _plcParam.bIndividualTrigger;
                txtMXCamPointRes.Text = _plcParam.strWriteCamPointRes;
                tgsMXReadSwap.IsOn = _plcParam.bReadSwap;
                tgsMXWriteSwap.IsOn = _plcParam.bWriteSwap;
                tgsMXSpecialSwap.IsOn = _plcParam.bUseSpecialFunction;

                if (_plcParam.bCommunicationType == false)
                    rdGCommunicationType.SelectedIndex = 0;
                else
                    rdGCommunicationType.SelectedIndex = 1;

                if (_plcParam.nHearBeatInterval == 0)
                {
                    txtLSHeartbeatInterval.Text = "1000";
                    _plcParam.nHearBeatInterval = 1000;
                }

                if (_plcParam.nReadInterval == 0)
                {
                    txtLSReadInterval.Text = "1000";
                    _plcParam.nReadInterval = 1000;
                }

                nType = (int)GlovalVar.PLCType.MX;
            }
            else if (_plcParam.nCommType == (int)GlovalVar.PLCType.Simens)
            {
                swSiemens.IsOn = true;

                txtSiemensIP.Text = _plcParam.strPLCIP;
                int nIdx = 0;
                foreach (string str in cbSiemensType.Properties.Items)
                {
                    if (str == _plcParam.strPLCPort)
                    {
                        cbSiemensType.SelectedIndex = nIdx;
                        break;
                    }
                    nIdx++;
                }

                txtSiemensRack.Text = _plcParam.shRack.ToString();
                txtSiemensSlot.Text = _plcParam.shSlot.ToString();
                txtSiemensHeartbeatAddr.Text = _plcParam.strHeartBeatAddr;
                txtSiemensHeartbeatInterval.Text = _plcParam.nHearBeatInterval.ToString();
                txtSiemensSignal.Text = _plcParam.strStartSignal;
                txtSiemensOK.Text = _plcParam.strOKSignal;
                txtSiemensNG.Text = _plcParam.strNGSignal;
                txtSiemensReadInterval.Text = _plcParam.nReadInterval.ToString();

                txtSiemensReadStartAddr.Text = _plcParam.strReadStartAddr;
                txtSiemensReadLength.Text = _plcParam.strReadLength;
                txtSiemensReadTriggerStart.Text = _plcParam.strReadStartTrigger;
                txtSiemensReadTriggerEnd.Text = _plcParam.strReadEndTrigger;
                txtSiemensReadModelStart.Text = _plcParam.strReadModelStart;
                txtSiemensReadModelEnd.Text = _plcParam.strReadModelEnd;
                txtSiemensReadLotStart.Text = _plcParam.strReadLotStart;
                txtSiemensReadLotEnd.Text = _plcParam.strReadLotEnd;
                txtSiemensReadCamPassStart.Text = _plcParam.strReadCamPassStart;
                txtSiemensReadCamPassEnd.Text = _plcParam.strReadCamPassEnd;
                txtSiemensLWriteStartAddr.Text = _plcParam.strWriteStartAddr;
                txtSiemensLWriteTriggerStart.Text = _plcParam.strWriteTriggerStart;
                txtSiemensLWriteModelStart.Text = _plcParam.strWriteModelStart;
                txtSiemensLWriteLotlStart.Text = _plcParam.strWriteLotStart;
                txtSiemensWriteResStart.Text = _plcParam.strWriteResStart;
                txtSiemensWriteTotalRes.Text = _plcParam.strWriteTotalRes;
                txtSiemensWrite2DRes.Text = _plcParam.strWrite2DRes;
                txtSiemensWrite2DData.Text = _plcParam.strWrite2DData;
                txtSiemensWriteAlignX.Text = _plcParam.strWriteAlignX;
                txtSiemensWriteAlignY.Text = _plcParam.strWriteAlignY;
                txtSiemensWriteAlignZ.Text = _plcParam.strWriteAlignZ;
                txtSiemensWritePinRes.Text = _plcParam.strWritePinChange;
                chkSiemensndividualTrigger.Checked = _plcParam.bIndividualTrigger;
                txtSiemensCamPointRes.Text = _plcParam.strWriteCamPointRes;

                nType = (int)GlovalVar.PLCType.Simens;
            }
            else if (_plcParam.nCommType == (int)GlovalVar.PLCType.IO)
            {
                sw_IO.IsOn = true;

                txtIOHeartbeatNo.Text = _plcParam.strHeartBeatAddr;
                txtIOHeartbeatInterval.Text = _plcParam.nHearBeatInterval.ToString();
                txtIOReadInterval.Text = _plcParam.nReadInterval.ToString();
                txtIOTriggerStart.Text = _plcParam.strReadStartTrigger;
                txtIOTriggerEnd.Text = _plcParam.strReadEndTrigger;
                txtIOModelStart.Text = _plcParam.strReadModelStart;
                txtIOModelEnd.Text = _plcParam.strReadModelEnd;
                txtIOWriteTriggerStart.Text = _plcParam.strWriteTriggerStart;
                txtIOWriteModelStart.Text = _plcParam.strWriteModelStart;
                txtIOWriteResStart.Text = _plcParam.strWriteResStart;
                txtIOWriteTotalRes.Text = _plcParam.strWriteTotalRes;
                chkIOindividualTrigger.Checked = _plcParam.bIndividualTrigger;

                nType = (int)GlovalVar.PLCType.IO;
            }
            else
            {
                swLS.IsOn = true;

                txtLSIP.Text = _plcParam.strPLCIP;
                txtLSPort.Text = _plcParam.strPLCPort;
                txtLSHeartbeatAddr.Text = _plcParam.strHeartBeatAddr;
                txtLSHeartbeatInterval.Text = _plcParam.nHearBeatInterval.ToString();
                txtLSSignal.Text = _plcParam.strStartSignal;
                txtLSOK.Text = _plcParam.strOKSignal;
                txtLSNG.Text = _plcParam.strNGSignal;
                txtLSReadInterval.Text = _plcParam.nReadInterval.ToString();
                txtLSReadStartAddr.Text = _plcParam.strReadStartAddr;
                txtLSReadLength.Text = _plcParam.strReadLength;
                txtLSReadTriggerStart.Text = _plcParam.strReadStartTrigger;
                txtLSReadTriggerEnd.Text = _plcParam.strReadEndTrigger;
                txtLSReadModelStart.Text = _plcParam.strReadModelStart;
                txtLSReadModelEnd.Text = _plcParam.strReadModelEnd;
                txtLSReadLotStart.Text = _plcParam.strReadLotStart;
                txtLSReadLotEnd.Text = _plcParam.strReadLotEnd;
                txtLSReadCamPassStart.Text = _plcParam.strReadCamPassStart;
                txtLSReadCamPassEnd.Text = _plcParam.strReadCamPassEnd;
                txtLSWriteAddr.Text = _plcParam.strWriteStartAddr;
                txtLSWriteTriggerStart.Text = _plcParam.strWriteTriggerStart;
                txtLSWriteModelStart.Text = _plcParam.strWriteModelStart;
                txtLSWriteLotStart.Text = _plcParam.strWriteLotStart;
                txtLSWriteResStart.Text = _plcParam.strWriteResStart;
                txtLSWriteTotalRes.Text = _plcParam.strWriteTotalRes;
                txtLSWrite2DRes.Text = _plcParam.strWrite2DRes;
                txtLSWrite2DData.Text = _plcParam.strWrite2DData;
                txtLSWrite2DLen.Text = _plcParam.strWrite2DLen;
                txtLSWriteAlignX.Text = _plcParam.strWriteAlignX;
                txtLSWriteAlignY.Text = _plcParam.strWriteAlignY;
                txtLSWriteAlignZ.Text = _plcParam.strWriteAlignZ;
                txtLSWritePinRes.Text = _plcParam.strWritePinChange;
                chkLSndividualTrigger.Checked = _plcParam.bIndividualTrigger;
                txtLSCamPointRes.Text = _plcParam.strWriteCamPointRes;
                txtLSindividualTrigger.Text = _plcParam.strIndividualTrigger;
                txtLSWritePinProhibit.Text = _plcParam.strWritePinProhibit;
                sw_swap.IsOn = _plcParam.bReadSwap;

                nType = (int)GlovalVar.PLCType.LS;
            }

            InitControl(nType, "Read");
            InitControl(nType, "Write");
        }

        private void swMX_Toggled(object sender, EventArgs e)
        {
            if (swMX.IsOn)
            {
                swSiemens.IsOn = false;
                sw_IO.IsOn = false;
                swLS.IsOn = false;

                tabComm.TabPages[0].PageVisible = true;
                tabComm.TabPages[1].PageVisible = false;
                tabComm.TabPages[2].PageVisible = false;
                tabComm.TabPages[3].PageVisible = false;

                using (Font font = new Font("Tahoma", 12, FontStyle.Bold))
                    lblMX.Font = font;

                lblMX.ForeColor = Color.Yellow;
                btnConnect.Enabled = true;
            }
            else
            {
                using (Font font = new Font("Tahoma", 12, FontStyle.Regular))
                    lblMX.Font = font;

                lblMX.ForeColor = Color.White;

                if (!swSiemens.IsOn && !sw_IO.IsOn && !swLS.IsOn)
                    swMX.IsOn = true;
            }
        }

        private void swSiemens_Toggled(object sender, EventArgs e)
        {
            if (swSiemens.IsOn)
            {
                swMX.IsOn = false;
                sw_IO.IsOn = false;
                swLS.IsOn = false;

                tabComm.TabPages[0].PageVisible = false;
                tabComm.TabPages[1].PageVisible = true;
                tabComm.TabPages[2].PageVisible = false;
                tabComm.TabPages[3].PageVisible = false;

                using (Font font = new Font("Tahoma", 12, FontStyle.Bold))
                    lblSiemens.Font = font;

                lblSiemens.ForeColor = Color.Yellow;
                btnConnect.Enabled = true;
            }
            else
            {
                using (Font font = new Font("Tahoma", 12, FontStyle.Regular))
                    lblSiemens.Font = font;

                lblSiemens.ForeColor = Color.White;

                if (!swMX.IsOn && !sw_IO.IsOn && !swLS.IsOn)
                    swSiemens.IsOn = true;
            }
        }

        private void sw_IO_Toggled(object sender, EventArgs e)
        {
            if (sw_IO.IsOn)
            {
                swMX.IsOn = false;
                swSiemens.IsOn = false;
                swLS.IsOn = false;

                tabComm.TabPages[0].PageVisible = false;
                tabComm.TabPages[1].PageVisible = false;
                tabComm.TabPages[2].PageVisible = true;
                tabComm.TabPages[3].PageVisible = false;

                using (Font font = new Font("Tahoma", 12, FontStyle.Bold))
                    lblIO.Font = font;

                lblIO.ForeColor = Color.Yellow;
                btnConnect.Enabled = true;
            }
            else
            {
                using (Font font = new Font("Tahoma", 12, FontStyle.Regular))
                    lblIO.Font = font;

                lblIO.ForeColor = Color.White;

                if (!swMX.IsOn && !swSiemens.IsOn && !swLS.IsOn)
                    sw_IO.IsOn = true;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            _bRead = false;
            _bWrite = false;
            Close();
        }

        public struct PLCPram  //카메라 파라미터
        {
            public string strWriteLotStart;
            public string strWriteLotEnd;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!swMX.IsOn && !swSiemens.IsOn && !sw_IO.IsOn && !swLS.IsOn)
                {
                    MessageBox.Show("Please select the equipment to communicate with", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (swMX.IsOn)
                {
                    if (txtMXIP.Text.Trim() == "" || txtMXPort.Text.Trim() == "")
                    {
                        MessageBox.Show("Please enter plc connection information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    _plcParam.nCommType = (int)GlovalVar.PLCType.MX;
                    _plcParam.strPLCIP = txtMXIP.Text.Trim();
                    _plcParam.strPLCPort = txtMXPort.Text.Trim();
                    _plcParam.shRack = 0;
                    _plcParam.shSlot = 0;
                    _plcParam.strHeartBeatAddr = txtMXHeartbeatAddr.Text;

                    int.TryParse(txtMXHeartbeatInterval.Text, out _plcParam.nHearBeatInterval);
                    if (_plcParam.nHearBeatInterval == 0)
                    {
                        txtMXHeartbeatInterval.Text = "1000";
                        _plcParam.nHearBeatInterval = 1000;
                    }

                    _plcParam.strStartSignal = txtMXSignal.Text;
                    _plcParam.strOKSignal = txtMXOK.Text;
                    _plcParam.strNGSignal = txtMXNG.Text;
                    int.TryParse(txtMXReadInterval.Text, out _plcParam.nReadInterval);

                    if (_plcParam.nReadInterval == 0)
                    {
                        txtMXReadInterval.Text = "100";
                        _plcParam.nReadInterval = 100;
                    }

                    _plcParam.strReadStartAddr = txtMXReadStartAddr.Text;
                    _plcParam.strReadLength = txtMXReadLength.Text;
                    _plcParam.strReadStartTrigger = txtMXReadTriggerStart.Text;
                    _plcParam.strReadEndTrigger = txtMXReadTriggerEnd.Text;
                    _plcParam.strReadModelStart = txtMXReadModelStart.Text;
                    _plcParam.strReadModelEnd = txtMXReadModelEnd.Text;
                    _plcParam.strReadLotStart = txtMXReadLotStart.Text;
                    _plcParam.strReadLotEnd = txtMXReadLotEnd.Text;
                    _plcParam.strReadCamPassStart = txtMXReadCamPassStart.Text;
                    _plcParam.strReadCamPassEnd = txtMXReadCamPassEnd.Text;
                    _plcParam.strWriteStartAddr = txtMXWriteAddr.Text;
                    _plcParam.strWriteResStart = txtMXWriteResStart.Text;
                    _plcParam.strWriteTotalRes = txtMXWriteTotalRes.Text;
                    _plcParam.strWrite2DRes = txtMXWrite2DRes.Text;
                    _plcParam.strWrite2DData = txtMXWrite2DData.Text;
                    _plcParam.strWriteAlignX = txtMXWriteAlignX.Text;
                    _plcParam.strWriteAlignY = txtMXWriteAlignY.Text;
                    _plcParam.strWriteAlignZ = txtMXWriteAlignZ.Text;
                    _plcParam.strWritePinChange = txtMXWritePinRes.Text;
                    _plcParam.bIndividualTrigger = chkMXndividualTrigger.Checked;
                    _plcParam.strWriteCamPointRes = txtMXCamPointRes.Text;
                    _plcParam.bReadSwap = tgsMXReadSwap.IsOn;
                    _plcParam.bWriteSwap = tgsMXWriteSwap.IsOn;
                    _frmMain._plcParam = _plcParam;

                    if (_frmMain._mxPlc == null)
                        _frmMain._mxPlc = new MXplc();

                    _frmMain._mxPlc._plcParam = _plcParam;

                    //_frmMain._mxPlc._plcParam = _plcParam;
                    _frmMain.AddMsg("Done saving MX plc parameters", Color.GreenYellow, true, false, frmMain.MsgType.Alarm);

                }
                else if (swSiemens.IsOn)
                {
                    if (txtSiemensIP.Text.Trim() == "" || cbSiemensType.SelectedIndex == -1 || txtSiemensRack.Text == "" || txtSiemensSlot.Text == "")
                    {
                        MessageBox.Show("Please enter PLC IP and port.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    _plcParam.nCommType = (int)GlovalVar.PLCType.Simens;
                    _plcParam.strPLCIP = txtSiemensIP.Text.Trim();
                    _plcParam.strPLCPort = cbSiemensType.SelectedItem.ToString();
                    short.TryParse(txtSiemensRack.Text, out _plcParam.shRack);
                    short.TryParse(txtSiemensSlot.Text, out _plcParam.shSlot);
                    _plcParam.strHeartBeatAddr = txtSiemensHeartbeatAddr.Text;
                    int.TryParse(txtSiemensHeartbeatInterval.Text, out _plcParam.nHearBeatInterval);

                    if (_plcParam.nHearBeatInterval == 0)
                    {
                        _plcParam.nHearBeatInterval = 1000;
                        txtSiemensHeartbeatInterval.Text = "1000";
                    }

                    _plcParam.strStartSignal = txtSiemensSignal.Text;
                    _plcParam.strOKSignal = txtSiemensOK.Text;
                    _plcParam.strNGSignal = txtSiemensNG.Text;

                    int.TryParse(txtSiemensReadInterval.Text, out _plcParam.nReadInterval);
                    if (_plcParam.nReadInterval == 0)
                    {
                        _plcParam.nReadInterval = 100;
                        txtSiemensReadInterval.Text = "100";
                    }

                    _plcParam.strReadStartAddr = txtSiemensReadStartAddr.Text;
                    _plcParam.strReadLength = txtSiemensReadLength.Text;
                    _plcParam.strReadStartTrigger = txtSiemensReadTriggerStart.Text;
                    _plcParam.strReadEndTrigger = txtSiemensReadTriggerEnd.Text;
                    _plcParam.strReadModelStart = txtSiemensReadModelStart.Text;
                    _plcParam.strReadModelEnd = txtSiemensReadModelEnd.Text;
                    _plcParam.strReadLotStart = txtSiemensReadLotStart.Text;
                    _plcParam.strReadLotEnd = txtSiemensReadLotEnd.Text;
                    _plcParam.strReadCamPassStart = txtSiemensReadCamPassStart.Text;
                    _plcParam.strReadCamPassEnd = txtSiemensReadCamPassEnd.Text;
                    _plcParam.strWriteStartAddr = txtSiemensLWriteStartAddr.Text;
                    _plcParam.strWriteTriggerStart = txtSiemensLWriteTriggerStart.Text;
                    _plcParam.strWriteModelStart = txtSiemensLWriteModelStart.Text;
                    _plcParam.strWriteLotStart = txtSiemensLWriteLotlStart.Text;
                    _plcParam.strWriteResStart = txtSiemensWriteResStart.Text;
                    _plcParam.strWriteTotalRes = txtSiemensWriteTotalRes.Text;
                    _plcParam.strWrite2DRes = txtSiemensWrite2DRes.Text;
                    _plcParam.strWrite2DData = txtSiemensWrite2DData.Text;
                    _plcParam.strWriteAlignX = txtSiemensWriteAlignX.Text;
                    _plcParam.strWriteAlignY = txtSiemensWriteAlignY.Text;
                    _plcParam.strWriteAlignZ = txtSiemensWriteAlignZ.Text;
                    _plcParam.strWritePinChange = txtSiemensWritePinRes.Text;
                    _plcParam.bIndividualTrigger = chkSiemensndividualTrigger.Checked;
                    _plcParam.strWriteCamPointRes = txtSiemensCamPointRes.Text;

                    _frmMain._plcParam = _plcParam;

                    if (_frmMain._siemensPLC == null)
                        _frmMain._siemensPLC = new SiemensPLC();

                    _frmMain._siemensPLC._plcParam = _plcParam;

                    //_frmMain._siemensPLC._plcParam = _frmMain._plcParam;

                    _frmMain.AddMsg("Done saving Siemens plc parameters", Color.GreenYellow, true, false, frmMain.MsgType.Alarm);
                }
                else if (sw_IO.IsOn)
                {
                    _plcParam.nCommType = (int)GlovalVar.PLCType.IO;
                    _plcParam.strPLCIP = "";
                    _plcParam.strPLCPort = "";
                    _plcParam.shRack = 0;
                    _plcParam.shSlot = 0;
                    _plcParam.strHeartBeatAddr = txtIOHeartbeatNo.Text;

                    int.TryParse(txtIOHeartbeatInterval.Text, out _plcParam.nHearBeatInterval);
                    if (_plcParam.nHearBeatInterval == 0)
                    {
                        txtIOHeartbeatInterval.Text = "1000";
                        _plcParam.nHearBeatInterval = 1000;
                    }

                    _plcParam.strStartSignal = "";
                    _plcParam.strOKSignal = "";
                    _plcParam.strNGSignal = "";
                    int.TryParse(txtIOReadInterval.Text, out _plcParam.nReadInterval);
                    _plcParam.strReadStartAddr = "";
                    _plcParam.strReadLength = "";
                    _plcParam.strReadStartTrigger = txtIOTriggerStart.Text;
                    _plcParam.strReadEndTrigger = txtIOTriggerEnd.Text;
                    _plcParam.strReadModelStart = txtIOModelStart.Text;
                    _plcParam.strReadModelEnd = txtIOModelEnd.Text;
                    _plcParam.strReadLotStart = "";
                    _plcParam.strReadLotEnd = "";
                    _plcParam.strWriteTriggerStart = txtIOWriteTriggerStart.Text;
                    _plcParam.strWriteModelStart = txtIOWriteModelStart.Text;
                    _plcParam.strWriteLotStart = "";
                    _plcParam.strWriteResStart = txtIOWriteResStart.Text;
                    _plcParam.strWriteTotalRes = txtIOWriteTotalRes.Text;
                    _plcParam.bIndividualTrigger = chkIOindividualTrigger.Checked;

                    _frmMain._plcParam = _plcParam;

                    if (_frmMain._ioBoard == null)
                        _frmMain._ioBoard = new ioBoard();

                    _frmMain._ioBoard._plcParam = _plcParam;
                    _frmMain.AddMsg("Done saving I/O parameters", Color.GreenYellow, true, false, frmMain.MsgType.Alarm);
                }
                else
                {
                    if (txtLSIP.Text.Trim() == "" || txtLSPort.Text.Trim() == "")
                    {
                        MessageBox.Show("Please enter plc connection information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    _plcParam.nCommType = (int)GlovalVar.PLCType.LS;
                    _plcParam.strPLCIP = txtLSIP.Text.Trim();
                    _plcParam.strPLCPort = txtLSPort.Text.Trim();
                    _plcParam.shRack = 0;
                    _plcParam.shSlot = 0;
                    _plcParam.strHeartBeatAddr = txtLSHeartbeatAddr.Text;
                    _plcParam.bReadSwap = sw_swap.IsOn;

                    _plcParam.bUseSpecialFunction = tgsMXSpecialSwap.IsOn;

                    int.TryParse(txtLSHeartbeatInterval.Text, out _plcParam.nHearBeatInterval);

                    _plcParam.strStartSignal = txtLSSignal.Text;
                    _plcParam.strOKSignal = txtLSOK.Text;
                    _plcParam.strNGSignal = txtLSNG.Text;
                    int.TryParse(txtLSReadInterval.Text, out _plcParam.nReadInterval);

                    _plcParam.strReadStartAddr = txtLSReadStartAddr.Text;
                    _plcParam.strReadLength = txtLSReadLength.Text;
                    _plcParam.strReadStartTrigger = txtLSReadTriggerStart.Text;
                    _plcParam.strReadEndTrigger = txtLSReadTriggerEnd.Text;
                    _plcParam.strReadModelStart = txtLSReadModelStart.Text;
                    _plcParam.strReadModelEnd = txtLSReadModelEnd.Text;
                    _plcParam.strReadLotStart = txtLSReadLotStart.Text;
                    _plcParam.strReadLotEnd = txtLSReadLotEnd.Text;
                    _plcParam.strReadCamPassStart = txtLSReadCamPassStart.Text;
                    _plcParam.strReadCamPassEnd = txtLSReadCamPassEnd.Text;
                    _plcParam.strWriteStartAddr = txtLSWriteAddr.Text;
                    _plcParam.strWriteTriggerStart = txtLSWriteTriggerStart.Text;
                    _plcParam.strWriteModelStart = txtLSWriteModelStart.Text;
                    _plcParam.strWriteLotStart = txtLSWriteLotStart.Text;
                    _plcParam.strWriteResStart = txtLSWriteResStart.Text;
                    _plcParam.strWriteTotalRes = txtLSWriteTotalRes.Text;
                    _plcParam.strWrite2DRes = txtLSWrite2DRes.Text;
                    _plcParam.strWrite2DData = txtLSWrite2DData.Text;
                    _plcParam.strWrite2DLen = txtLSWrite2DLen.Text;
                    _plcParam.strWriteAlignX = txtLSWriteAlignX.Text;
                    _plcParam.strWriteAlignY = txtLSWriteAlignY.Text;
                    _plcParam.strWriteAlignZ = txtLSWriteAlignZ.Text;
                    _plcParam.strWritePinChange = txtLSWritePinRes.Text;
                    _plcParam.bIndividualTrigger = chkLSndividualTrigger.Checked;
                    _plcParam.strWriteCamPointRes = txtLSCamPointRes.Text;
                    _plcParam.strIndividualTrigger = txtLSindividualTrigger.Text;
                    _frmMain._plcParam = _plcParam;

                    if (_frmMain._LSplc == null)
                        _frmMain._LSplc = new XGCommSocket();

                    _frmMain._LSplc._plcParam = _plcParam;
                    _frmMain.AddMsg("Done saving LS plc parameters", Color.GreenYellow, true, false, frmMain.MsgType.Alarm);
                }

                ini.WriteIniFile("CommType", "Value", _plcParam.nCommType.ToString(), Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("IP", "Value", _plcParam.strPLCIP, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("Port", "Value", _plcParam.strPLCPort, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("Rack", "Value", _plcParam.shRack.ToString(), Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("Slot", "Value", _plcParam.shSlot.ToString(), Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("HeartBeatAddr", "Value", _plcParam.strHeartBeatAddr, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("HeartBeatInterval", "Value", _plcParam.nHearBeatInterval.ToString(), Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("StartSignal", "Value", _plcParam.strStartSignal, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("OKSignal", "Value", _plcParam.strOKSignal, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("NGSignal", "Value", _plcParam.strNGSignal, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("ReadInterval", "Value", _plcParam.nReadInterval.ToString(), Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("ReadSwap", "Value", _plcParam.bReadSwap.ToString(), Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("WriteSwap", "Value", _plcParam.bWriteSwap.ToString(), Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("FunctionSwap", "Value", _plcParam.bUseSpecialFunction.ToString(), Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("CommunciationType", "Value", _plcParam.bCommunicationType.ToString(), Application.StartupPath + "\\Communication", "Communication.ini");


                ini.WriteIniFile("ReadStartAddr", "Value", _plcParam.strReadStartAddr, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("ReadLength", "Value", _plcParam.strReadLength, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("ReadTriggerStar", "Value", _plcParam.strReadStartTrigger, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("ReadTriggerEnd", "Value", _plcParam.strReadEndTrigger, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("ReadModelStart", "Value", _plcParam.strReadModelStart, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("ReadModelEnd", "Value", _plcParam.strReadModelEnd, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("ReadLotStart", "Value", _plcParam.strReadLotStart, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("ReadLotEnd", "Value", _plcParam.strReadLotEnd, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("ReadCamPassStart", "Value", _plcParam.strReadCamPassStart, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("ReadCamPassEnd", "Value", _plcParam.strReadCamPassEnd, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("WriteStartAddr", "Value", _plcParam.strWriteStartAddr, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("WriteTriggerStart", "Value", _plcParam.strWriteTriggerStart, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("WriteModelStart", "Value", _plcParam.strWriteModelStart, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("WriteLotStart", "Value", _plcParam.strWriteLotStart, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("WriteResStart", "Value", _plcParam.strWriteResStart, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("WriteTotalRes", "Value", _plcParam.strWriteTotalRes, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("Write2DRes", "Value", _plcParam.strWrite2DRes, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("Write2DData", "Value", _plcParam.strWrite2DData, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("WriteAlignX", "Value", _plcParam.strWriteAlignX, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("WriteAlignY", "Value", _plcParam.strWriteAlignY, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("WriteAlignZ", "Value", _plcParam.strWriteAlignZ, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("WritePinChange", "Value", _plcParam.strWritePinChange, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("WritePinProhibit", "Value", _plcParam.strWritePinProhibit, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("TriggerMode", "Value", _plcParam.bIndividualTrigger.ToString(), Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("WriteCamPointRes", "Value", _plcParam.strWriteCamPointRes, Application.StartupPath + "\\Communication", "Communication.ini");
                ini.WriteIniFile("TriggerModeData", "Value", _plcParam.strIndividualTrigger, Application.StartupPath + "\\Communication", "Communication.ini");
            }
            catch (Exception ex)
            {
                _frmMain.AddMsg("plc parameters save Error : " + ex.Message, Color.Red, true, true, frmMain.MsgType.Alarm);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!swMX.IsOn && !swSiemens.IsOn && !sw_IO.IsOn && !swLS.IsOn)
            {
                MessageBox.Show("Please select the equipment to communicate with", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (swMX.IsOn)
            {
                if (txtMXIP.Text.Trim() == "" || txtMXPort.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter plc connection information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _plcParam.nCommType = (int)GlovalVar.PLCType.MX;
                _plcParam.strPLCIP = txtMXIP.Text.Trim();
                _plcParam.strPLCPort = txtMXPort.Text.Trim();
            }
            else if (swSiemens.IsOn)
            {
                if (txtSiemensIP.Text.Trim() == "" || cbSiemensType.SelectedIndex == -1 || txtSiemensRack.Text == "" || txtSiemensSlot.Text == "")
                {
                    MessageBox.Show("Please enter PLC IP and port.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _plcParam.nCommType = (int)GlovalVar.PLCType.Simens;
                _plcParam.strPLCIP = txtSiemensIP.Text.Trim();
                _plcParam.strPLCPort = cbSiemensType.SelectedItem.ToString();
                short.TryParse(txtSiemensRack.Text, out _plcParam.shRack);
                short.TryParse(txtSiemensSlot.Text, out _plcParam.shSlot);
            }
            else if (sw_IO.IsOn)
                _plcParam.nCommType = (int)GlovalVar.PLCType.IO;
            else
            {
                if (txtLSIP.Text.Trim() == "" || txtLSPort.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter plc connection information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _plcParam.nCommType = (int)GlovalVar.PLCType.LS;
                _plcParam.strPLCIP = txtLSIP.Text.Trim();
                _plcParam.strPLCPort = txtLSPort.Text.Trim();
            }

            _frmMain.CommConnect();

            if (_frmMain._bCommConnect)
                MessageBox.Show("PLC Connected", "Connect", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("PLC Disconnected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (_frmMain._bCommConnect)
            {
                lblStatus.BackColor = Color.Lime;
                lblStatus.ForeColor = Color.Black;
                lblStatus.Text = "Connect";

                btnRead.Enabled = true;
                btnWrite.Enabled = true;
            }
            else
            {
                lblStatus.BackColor = Color.Red;
                lblStatus.ForeColor = Color.White;

                lblStatus.Text = "Disconnect";
            }
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            _bRead = !_bRead;
            if (swMX.IsOn)
            {
                if (_bRead)
                {
                    InitControl((int)GlovalVar.PLCType.MX, "Read");
                    btnRead.ImageOptions.ImageIndex = 0;

                    Thread threadRead = new Thread(ReadData);
                    threadRead.Start();

                    lblReadRun.BackColor = Color.Lime;
                    lblReadRun.ForeColor = Color.Black;
                    lblReadRun.Text = "Run";
                }
                else
                {
                    btnRead.ImageOptions.ImageIndex = 1;

                    lblReadRun.BackColor = Color.Red;
                    lblReadRun.ForeColor = Color.White;
                    lblReadRun.Text = "Stop";


                    for (int i = 0; i < dgRead.Rows.Count; i++)
                        dgRead.Rows[i].Cells[1].Value = ""
;
                }
            }
            else if (swSiemens.IsOn)
            {

            }
            else if (sw_IO.IsOn)
            {

            }
            else if (swLS.IsOn)
            {
                if (_bRead)
                {
                    InitControl((int)GlovalVar.PLCType.LS, "Read");
                    btnRead.ImageOptions.ImageIndex = 0;

                    Thread threadRead = new Thread(ReadData);
                    threadRead.Start();

                    lblReadRun.BackColor = Color.Lime;
                    lblReadRun.ForeColor = Color.Black;
                    lblReadRun.Text = "Run";
                }
                else
                {
                    btnRead.ImageOptions.ImageIndex = 1;

                    lblReadRun.BackColor = Color.Red;
                    lblReadRun.ForeColor = Color.White;
                    lblReadRun.Text = "Stop";


                    for (int i = 0; i < dgRead.Rows.Count; i++)
                        dgRead.Rows[i].Cells[1].Value = "";
                }
            }
            else
            {
                MessageBox.Show("Please select the equipment to connect.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReadData()
        {
            string strValue = "";
            string strTemp = "";
            ushort[] usData = null;
            byte[] bytes = null;
            int nLen = 0;
            if (swMX.IsOn)
                int.TryParse(txtMXReadLength.Text, out nLen);
            else if (swSiemens.IsOn)
                int.TryParse(txtSiemensReadLength.Text, out nLen);
            else if (sw_IO.IsOn)
                nLen = 16;
            else if (swLS.IsOn)
                int.TryParse(txtLSReadLength.Text, out nLen);

            string strData = "";
            string[] strRead = null;

            int nDataType = 0;

            while (true)
            {
                if (!_bRead)
                    return;

                try
                {
                    if (usData == null)
                        usData = new ushort[_frmMain._ReadShort.Length];

                    if (nDataType != _nDataType)
                    {
                        nDataType = _nDataType;

                        Invoke(new EventHandler(delegate
                        {
                            for (var i = 0; i < usData.Length; i++)
                            {
                                if (nDataType == (int)DataType.DEC)
                                    dgRead.Rows[i].Cells[1].Value = usData[i].ToString();
                                else if (nDataType == (int)DataType.BINARY)
                                    dgRead.Rows[i].Cells[1].Value = Convert.ToString(usData[i], 2);
                                else if (nDataType == (int)DataType.HEX)
                                {
                                    if (_plcParam.bReadSwap)
                                        dgRead.Rows[i].Cells[1].Value = usData[i].ToString("X4");
                                    else
                                        dgRead.Rows[i].Cells[1].Value = usData[i].ToString("X4").Substring(2, 2) + usData[i].ToString("X4").Substring(0, 2);
                                }
                                else
                                {
                                    strTemp = Convert.ToInt32(usData[i]).ToString("X4");
                                    if (_plcParam.bReadSwap)
                                        dgRead.Rows[i].Cells[1].Value = string.Format("{0}{1}", Convert.ToChar(Convert.ToByte(strTemp.Substring(0, 2), 16)), Convert.ToChar(Convert.ToByte(strTemp.Substring(2, 2), 16)));
                                    else
                                        dgRead.Rows[i].Cells[1].Value = string.Format("{0}{1}", Convert.ToChar(Convert.ToByte(strTemp.Substring(2, 2), 16)), Convert.ToChar(Convert.ToByte(strTemp.Substring(0, 2), 16)));
                                }
                            }
                        }));
                    }
                    else
                    {
                        Invoke(new EventHandler(delegate
                        {
                            for (var i = 0; i < usData.Length; i++)
                            {
                                
                                    usData[i] = _frmMain._ReadShort[i];

                                    if (nDataType == (int)DataType.DEC)
                                        dgRead.Rows[i].Cells[1].Value = usData[i].ToString();
                                    else if (nDataType == (int)DataType.BINARY)
                                        dgRead.Rows[i].Cells[1].Value = Convert.ToString(usData[i], 2);
                                    else if (nDataType == (int)DataType.HEX)
                                    {
                                        if (_plcParam.bReadSwap)
                                            dgRead.Rows[i].Cells[1].Value = usData[i].ToString("X4");
                                        else
                                            dgRead.Rows[i].Cells[1].Value = usData[i].ToString("X4").Substring(2, 2) + usData[i].ToString("X4").Substring(0, 2);
                                    }
                                    else
                                    {
                                        strTemp = usData[i].ToString("X4");
                                        if (_plcParam.bReadSwap)
                                            dgRead.Rows[i].Cells[1].Value = string.Format("{0}{1}", Convert.ToChar(Convert.ToByte(strTemp.Substring(0, 2), 16)), Convert.ToChar(Convert.ToByte(strTemp.Substring(2, 2), 16)));
                                        else
                                            dgRead.Rows[i].Cells[1].Value = string.Format("{0}{1}", Convert.ToChar(Convert.ToByte(strTemp.Substring(2, 2), 16)), Convert.ToChar(Convert.ToByte(strTemp.Substring(0, 2), 16)));

                                    }
                            }
                        }));
                    }
                }
                catch { }


                Thread.Sleep(50);
            }
        }

        private void txtMXPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))    //숫자와 백스페이스를 제외한 나머지를 바로 처리
            {
                e.Handled = true;
            }
        }

        private void btnWrite_Click(object sender, EventArgs e)
        {
            _bWrite = !_bWrite;
            if (swMX.IsOn)
            {
                if (_bWrite)
                {
                    InitControl((int)GlovalVar.PLCType.MX, "Write");
                    btnWrite.ImageOptions.ImageIndex = 0;

                    Thread threadWrite = new Thread(WriteData);
                    threadWrite.Start();

                    lblWriteRun.BackColor = Color.Lime;
                    lblWriteRun.ForeColor = Color.Black;
                    lblWriteRun.Text = "Run";
                }
                else
                {
                    btnWrite.ImageOptions.ImageIndex = 1;

                    lblWriteRun.BackColor = Color.Red;
                    lblWriteRun.ForeColor = Color.White;
                    lblWriteRun.Text = "Stop";

                    for (int i = 0; i < dgWrite.Rows.Count; i++)
                    {
                        dgWrite.Rows[i].Cells[1].Value = "";
                        //_frmMain._listWriteData[i] = "";
                    }
                }
            }
            else if (swSiemens.IsOn)
            {

            }
            else if (sw_IO.IsOn)
            {

            }
            else if (swLS.IsOn)
            {
                if (_bWrite)
                {
                    InitControl((int)GlovalVar.PLCType.LS, "Write");
                    btnWrite.ImageOptions.ImageIndex = 0;

                    Thread threadWrite = new Thread(WriteData);
                    threadWrite.Start();

                    lblWriteRun.BackColor = Color.Lime;
                    lblWriteRun.ForeColor = Color.Black;
                    lblWriteRun.Text = "Run";
                }
                else
                {
                    btnWrite.ImageOptions.ImageIndex = 1;

                    lblWriteRun.BackColor = Color.Red;
                    lblWriteRun.ForeColor = Color.White;
                    lblWriteRun.Text = "Stop";

                    for (int i = 0; i < dgWrite.Rows.Count; i++)
                        dgWrite.Rows[i].Cells[1].Value = "";
                }

            }
            else
                MessageBox.Show("Please select the equipment to connect.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private void WriteData()
        {
            var strTemp = "";
            ushort usData = 0;
            byte[] bytes = null;
            int nDataTyp = 0;
            while (true)
            {
                if (!_bWrite)
                    return;

                try
                {
                    Invoke(new EventHandler(delegate
                    {
                        if (nDataTyp != _nDataType)
                        {
                            nDataTyp = _nDataType;
                            if (swMX.IsOn)
                            {
                            }
                            else if (swSiemens.IsOn)
                            {

                            }
                            else if (sw_IO.IsOn)
                            {

                            }
                            else
                            {
                                for (int i = 0; i < dgWrite.RowCount; i++)
                                {
                                    if (nDataTyp == (int)DataType.DEC)
                                    {
                                        dgWrite.Rows[i].Cells[1].Value = _frmMain._LSplc._listWriteData[i];
                                    }
                                    else if (nDataTyp == (int)DataType.BINARY)
                                    {
                                        if (!string.IsNullOrEmpty(_frmMain._LSplc._listWriteData[i]))
                                            dgWrite.Rows[i].Cells[1].Value = Convert.ToString(Convert.ToInt32(_frmMain._LSplc._listWriteData[i]), 2);
                                    }
                                    else if (nDataTyp == (int)DataType.HEX)
                                    {
                                        if (!string.IsNullOrEmpty(_frmMain._LSplc._listWriteData[i]))
                                            dgWrite.Rows[i].Cells[1].Value = Convert.ToInt32(_frmMain._LSplc._listWriteData[i]).ToString("X4");
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(_frmMain._LSplc._listWriteData[i]))
                                        {
                                            strTemp = Convert.ToInt32(_frmMain._LSplc._listWriteData[i]).ToString("X4");
                                            dgWrite.Rows[i].Cells[1].Value = string.Format("{0}{1}", Convert.ToChar(Convert.ToByte(strTemp.Substring(0, 2), 16)), Convert.ToChar(Convert.ToByte(strTemp.Substring(2, 2), 16)));
                                            //dgWrite.Rows[i].Cells[1].Value = string.Format("{0}{1}", Convert.ToChar(Convert.ToByte(strTemp.Substring(2, 2), 16)), Convert.ToChar(Convert.ToByte(strTemp.Substring(0, 2), 16)));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < dgWrite.RowCount; i++)
                            {
                                if (swMX.IsOn)
                                {
                                    if (string.IsNullOrEmpty(dgWrite.Rows[i].Cells[1].Value.ToString()))
                                        _frmMain._mxPlc._listWriteData[i] = "";
                                    else
                                        _frmMain._mxPlc._listWriteData[i] = dgWrite.Rows[i].Cells[1].Value.ToString();
                                }
                                else if (swSiemens.IsOn)
                                {

                                }
                                else if (sw_IO.IsOn)
                                {

                                }
                                else
                                {
                                    if (_nDataType == (int)DataType.DEC)
                                    {
                                        if (string.IsNullOrEmpty(dgWrite.Rows[i].Cells[1].Value.ToString()))
                                            strTemp = "";
                                        else
                                            strTemp = dgWrite.Rows[i].Cells[1].Value.ToString();
                                    }
                                    else if (_nDataType == (int)DataType.HEX)
                                    {
                                        if (string.IsNullOrEmpty(dgWrite.Rows[i].Cells[1].Value.ToString()))
                                            strTemp = "";
                                        else
                                        {
                                            usData = MAKEWORD(Convert.ToByte(dgWrite.Rows[i].Cells[1].Value.ToString().Substring(2, 2), 16), Convert.ToByte(dgWrite.Rows[i].Cells[1].Value.ToString().Substring(0, 2), 16));
                                            strTemp = usData.ToString();
                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(dgWrite.Rows[i].Cells[1].Value.ToString()))
                                            strTemp = "";
                                        else
                                        {
                                            bytes = null;
                                            bytes = Encoding.UTF8.GetBytes(dgWrite.Rows[i].Cells[1].Value.ToString());

                                            if (bytes.Length != 0)
                                            {
                                                //if (bytes.Length ==2)
                                                //usData = MAKEWORD(bytes[1], bytes[0]);
                                                //else
                                                usData = MAKEWORD(bytes[1], bytes[0]);
                                                strTemp = usData.ToString();
                                            }
                                            else
                                                strTemp = "";
                                            //usData = MAKEWORD(bytes[1], bytes[0]);
                                            //strTemp = usData.ToString();
                                        }
                                    }

                                    if (_frmMain._LSplc._listWriteData[i] != strTemp)
                                        _frmMain._LSplc._listWriteData[i] = strTemp;
                                }

                            }
                        }

                    }));
                }
                catch
                { }


                Thread.Sleep(50);
            }
        }

        private static UInt16 MAKEWORD(byte low, byte high)
        {
            return (UInt16)((high << 8) | low);
        }

        private void swLS_Toggled(object sender, EventArgs e)
        {
            if (swLS.IsOn)
            {
                swMX.IsOn = false;
                swSiemens.IsOn = false;
                sw_IO.IsOn = false;

                tabComm.TabPages[0].PageVisible = false;
                tabComm.TabPages[1].PageVisible = false;
                tabComm.TabPages[2].PageVisible = false;
                tabComm.TabPages[3].PageVisible = true;

                using (Font font = new Font("Tahoma", 12, FontStyle.Bold))
                    lblLS.Font = font;

                lblLS.ForeColor = Color.Yellow;
                btnConnect.Enabled = true;
            }
            else
            {
                using (Font font = new Font("Tahoma", 12, FontStyle.Regular))
                    lblLS.Font = font;

                lblLS.ForeColor = Color.White;

                if (!swSiemens.IsOn && !sw_IO.IsOn && !swMX.IsOn)
                    swMX.IsOn = true;
            }
        }

        private void radDec_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
            {
                (sender as RadioButton).ForeColor = Color.Yellow;
                int.TryParse((sender as RadioButton).Tag.ToString(), out _nDataType);
            }
            else
            {
                (sender as RadioButton).ForeColor = Color.White;
            }

        }

        private void chkMXndividualTrigger_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckEdit).Checked)
            {
                (sender as CheckEdit).ForeColor = Color.Yellow;
                txtMXindividualTrigger.Visible = true;
            }
            else
            {
                (sender as CheckEdit).ForeColor = Color.White;
                txtMXindividualTrigger.Visible = false;
            }
        }

        private void sw_swap_Toggled(object sender, EventArgs e)
        {
            if (sw_swap.IsOn == true)
                _plcParam.bReadSwap = true;
            else
                _plcParam.bReadSwap = false;
        }

        private void tgsMXReadSwap_Toggled(object sender, EventArgs e)
        {
            if (tgsMXReadSwap.IsOn)
                _plcParam.bReadSwap = true;
            else
                _plcParam.bReadSwap = false;
        }

        private void tgsMXWriteSwap_Toggled(object sender, EventArgs e)
        {
            if (tgsMXWriteSwap.IsOn)
                _plcParam.bWriteSwap = true;
            else
                _plcParam.bWriteSwap = false;
        }

        private void tgsMXSpecialSwap_Toggled(object sender, EventArgs e)
        {
            if (tgsMXSpecialSwap.IsOn)
            {
                gBMXSpecialWrite.Visible = true;
            }
            else
            {
                gBMXSpecialWrite.Visible = false;
            }


        }

        private void chkLSndividualTrigger_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckEdit).Checked)
            {
                (sender as CheckEdit).ForeColor = Color.Yellow;
                txtLSindividualTrigger.Visible = true;
            }
            else
            {
                (sender as CheckEdit).ForeColor = Color.White;
                txtLSindividualTrigger.Visible = false;
            }
        }

        private void rdGCommunicationType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rdGCommunicationType.SelectedIndex == 0)
                _plcParam.bCommunicationType = false;             //ASCII
            else
                _plcParam.bCommunicationType = true;             //Binary 
        }
    }
}
