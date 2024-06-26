using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using S7.Net.Types;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using S7.Net;
using DevExpress.Pdf.Native;
using Cognex.VisionPro.Exceptions;
using static GlovalVar;
using System.Diagnostics;

using MCProtocol;
using VagabondK.Protocols.Channels;
using VagabondK.Protocols.LSElectric;
using VagabondK.Protocols.LSElectric.FEnet;
using VagabondK.Protocols.Logging;


public class PLCComm
{
    public delegate void OnPLCConnect();
    public delegate void OnPLCDisconnect();
    public delegate void OnTrigger(int nCamNo, int nTriggerCnt);
    public delegate void OnModelChange(string strModelNo);
    public delegate void OnLotNo(string strLotNo);
    public delegate void Onmessage(string strMsg);

    public OnPLCConnect _OnPLCConnect;
    public OnPLCDisconnect _OnPLCDisconnect;
    public OnTrigger _OnTrigger;
    public OnModelChange _OnModelChange;
    public OnLotNo _OnLotNo;
    public Onmessage _OnMessage;

    bool[] _bGrabComplete = new bool[30];

    //IniFiles _ini = new IniFiles();

    MCProtocol.Mitsubishi.Plc _mxPLC = null; //미쯔비시
    FEnetClient _lsPLC = null;
    Plc _SiemensPLC = null; //sigmens 통신용

    bool _bConnect = false;
    //int[] _nData = null;
    //ushort[] _ushData = null;
    byte[] _bytes;
    bool _bReadPLC = false;
    
    Queue<string> _qdata = new Queue<string>();
    //Queue<string> _qGrabEnd = new Queue<string>();

    string[] _strPinData = new string[30];

    public bool _bGetWriteData = false;
    int[] nWriteValue = null;
    byte[] _byteWrite = null;

    public bool Connect()
    {
        try
        {
            LoadPLCParam();

            if (string.IsNullOrEmpty(_plcParam.strPLCIP) || string.IsNullOrEmpty(_plcParam.strPLCPort))
            {
                if (_OnPLCDisconnect != null)
                    _OnPLCDisconnect();

                return _bConnect;
            }
                
            int.TryParse(_plcParam.strPLCPort, out var nPort);

            if (_plcParam.plcType == PLCType.MX) // 미쯔비시 or LS or simens
            {
                _mxPLC = new Mitsubishi.McProtocolTcp(_plcParam.strPLCIP, nPort, Mitsubishi.McFrame.MC3E);

                var Result = _mxPLC.Open();

                if ((bool)Result.IsFaulted == false)
                    _bConnect = true;
                else
                    _bConnect = false;
            }
            else if (_plcParam.plcType == PLCType.LS)
            {
                _lsPLC = new FEnetClient(new TcpChannel(_plcParam.strPLCIP, nPort));
                
                var Logger = new ChannelOpenEventLog(_lsPLC.Channel);

                if (Logger.ToString() == "Opened Channel")
                    _bConnect = true;
            }
            else if (_plcParam.plcType == PLCType.Simens)
            {
                CpuType cpu = (CpuType)Enum.Parse(typeof(CpuType), _plcParam.strPLCPort);
                _SiemensPLC = new Plc(cpu, _plcParam.strPLCIP, _plcParam.shRack, _plcParam.shSlot);
                _SiemensPLC.Open();

                if (_SiemensPLC.IsConnected)
                    _bConnect = true;
            }

            _plcParam.bConnect = _bConnect;
            SQL sql = new SQL();
            sql.SavePLCInfo(_strProcName, _dbInfo, _plcParam);
            sql.Dispose();

            if (_bConnect)
            {
                if (_OnPLCConnect != null)
                    _OnPLCConnect();
            }
            else
            {
                if (_OnPLCDisconnect != null)
                    _OnPLCDisconnect();
            }

            if (!_bReadPLC)
            {
                _bReadPLC = true;
                Thread threadReadPLC = new Thread(ReadPLC);
                threadReadPLC.Start();
            }
        }
        catch { }

        return _bConnect;
    }


    public bool isConnected
    {
        get { return _bConnect; }
    }

    public byte[] GetData
    {
        get { return _bytes; }
    }

    public string SetData
    {
        set {_qdata.Enqueue(value); }
    }


    public GlovalVar.PLCPram plcParams
    {
        get
        {
            return _plcParam;
        }

        set
        {
            _plcParam = value;
        }
    }

    public void GrabComplete(int nCamera, int nCameraCount)
    {
        var nCamNo = nCamera;
        var nCamCnt = nCameraCount;

        _bGrabComplete[nCamNo] = true;

        try
        {
            if (_plcParam.strWriteDev != "" && _plcParam.strWriteGrabComplete != "")
            {
                var nWriteData = 0;

                if (nCamCnt.ToString() == _plcParam.strReadTriggerCnt)
                {
                    if (_plcParam.SignalFormat == SignalFormat.DEC)
                        nWriteData = 1;
                    else
                        nWriteData = '1';

                    int.TryParse(_plcParam.strWriteGrabComplete, out var nStartAddr);
                    _qdata.Enqueue(string.Format("{0},{1},{2}", _plcParam.strWriteDev, nStartAddr + nCamNo, nWriteData));
                }
                else
                {
                    if (_plcParam.strReadTriggerCnt == "1")
                    {
                        if (_plcParam.SignalFormat == SignalFormat.DEC)
                            nWriteData = nCamNo + 1;
                        else
                        {
                            byte[] bytes = BitConverter.GetBytes((Int16)(nCamNo + 1));
                            nWriteData = BitConverter.ToInt16(bytes, 0);
                        }

                        _qdata.Enqueue(string.Format("{0},{1},{2}", _plcParam.strWriteDev, _plcParam.strWriteGrabComplete, nWriteData));
                    }
                }
            }
        }
        catch { }
    }

    public void ModelChange()
    {
        try
        {
            if (_plcParam.strWriteDev != "" && _plcParam.strWriteModelChange != "")
            {
                //int.TryParse(_plcParam.strCommType, out var nCommType);
                var nWriteData = 0;
                if (_plcParam.SignalFormat == SignalFormat.DEC)
                    nWriteData = 1;
                else
                    nWriteData = (int)'1';

                _qdata.Enqueue(string.Format("{0},{1},{2}", _plcParam.strWriteDev, _plcParam.strWriteModelChange, nWriteData));
            }
        }
        catch { }                   
    }

    public void LotNoReceive()
    {
        try
        {
            if (_plcParam.strWriteDev != "" && _plcParam.strWriteLotComplete != "")
            {
                //int.TryParse(_plcParam.strCommType, out var nCommType);
                var nWriteData = 0;
                if (_plcParam.SignalFormat == SignalFormat.DEC)
                    nWriteData = 1;
                else
                    nWriteData = (int)'1';

                _qdata.Enqueue(string.Format("{0},{1},{2}", _plcParam.strWriteDev, _plcParam.strWriteLotComplete, nWriteData));
            }
        }
        catch
        { }
    }

    public void SetPointResult(int nCamera, bool bResult, string[] strResData)
    {
        var nCamNo = nCamera;
        var bRes = bResult;
        var strData = strResData;
        var nWriteData = 0;

        try
        {
            if (nCamNo == 0 || nCamNo == 1)
                _strPinData[nCamNo] = strData[7];

            if (_plcParam.strWriteDev != "" && _plcParam.strWrite2DData != "")
            {
                if (strData[1] != "") // 2D
                    _qdata.Enqueue(string.Format("{0},{1},{2}", _plcParam.strWriteDev, _plcParam.strWrite2DData, GetStToIntStr(strData[1])));
            }

            if (_plcParam.strWriteDev != "" && _plcParam.strWriteAlignX != "")
            {
                if (strData[2] != "") //AlignX
                {
                    int.TryParse(strData[2], out nWriteData);
                    _qdata.Enqueue(string.Format("{0},{1},{2}", _plcParam.strWriteDev, _plcParam.strWriteAlignX, nWriteData));
                }
            }

            if (_plcParam.strWriteDev != "" && _plcParam.strWriteAlignY != "")
            {
                if (strData[3] != "") //AlignY
                {
                    int.TryParse(strData[3], out nWriteData);
                    _qdata.Enqueue(string.Format("{0},{1},{2}", _plcParam.strWriteDev, _plcParam.strWriteAlignY, nWriteData));
                }
            }

            if (_plcParam.strWriteDev != "" && _plcParam.strWriteAlignR != "")
            {
                if (strData[4] != "") //AlignR
                {
                    int.TryParse(strData[4], out nWriteData);
                    _qdata.Enqueue(string.Format("{0},{1},{2}", _plcParam.strWriteDev, _plcParam.strWriteAlignR, nWriteData));
                }
            }

            if (_plcParam.strWriteDev != "" && _plcParam.strWriteWidth != "")
            {
                if (strData[5] != "") //Width
                {
                    var strWidth = strData[5].Split(',');
                    var strWriteData = "";

                    if (strWidth.Length == 1)
                        strWriteData = strWidth[0];
                    else
                    {
                        for (var i =0; i<strWidth.Length; i++)
                        {
                            if (strWriteData == "")
                                strWriteData += strWidth[i];
                            else
                                strWriteData += "," + strWidth[i];
                        }
                    }

                    _qdata.Enqueue(string.Format("{0},{1},{2}", _plcParam.strWriteDev, _plcParam.strWriteWidth, strWriteData));
                }
            }

            if (_plcParam.strWriteDev != "" && _plcParam.strWriteHeight != "")
            {
                if (strData[6] != "") //Height
                {
                    var strHeight = strData[6].Split(',');
                    var strWriteData = "";

                    if (strHeight.Length == 1)
                        strWriteData = strHeight[0];
                    else
                    {
                        for (var i = 0; i < strHeight.Length; i++)
                        {
                            if (strWriteData == "")
                                strWriteData += strHeight[i];
                            else
                                strWriteData += "," + strHeight[i];
                        }
                    }
                    _qdata.Enqueue(string.Format("{0},{1},{2}", _plcParam.strWriteDev, _plcParam.strWriteHeight, GetStToIntStr(strWriteData)));
                }
            }

            if (_plcParam.strWriteDev != "" && _plcParam.strWritePinChange != "")
            {
                if (nCamNo == 0 || nCamNo == 1)
                {
                    if (!string.IsNullOrEmpty(_strPinData[0]) && !string.IsNullOrEmpty(_strPinData[1]))
                    {
                        nWriteData = Convert.ToInt32(_strPinData[0] + _strPinData[1], 2);
                        _qdata.Enqueue(string.Format("{0},{1},{2}", _plcParam.strWriteDev, _plcParam.strWritePinChange, nWriteData));

                        _strPinData[0] = "";
                        _strPinData[1] = "";
                    }
                }
            }

            if (_plcParam.strWriteDev != "" && _plcParam.strWritePointResult != "")
            {
                if (_plcParam.SignalFormat == SignalFormat.DEC)
                {
                    if (bRes)
                        int.TryParse(_plcParam.strOKSignal, out nWriteData);
                    else
                        int.TryParse(_plcParam.strNGSignal, out nWriteData);
                }
                else
                {
                    if (bRes)
                        nWriteData = _plcParam.strOKSignal[0];
                    else
                        nWriteData = _plcParam.strNGSignal[0];
                }

                int.TryParse(_plcParam.strWritePointResult, out var nAddr);
                int.TryParse(_plcParam.strReadTriggerCnt, out var nTriggerCnt);

                if (nTriggerCnt == 1)
                  _qdata.Enqueue(string.Format("{0},{1},{2}", _plcParam.strWriteDev, nAddr, nWriteData));
                else if (nTriggerCnt >= 2)
                 _qdata.Enqueue(string.Format("{0},{1},{2}", _plcParam.strWriteDev, nAddr + nCamNo, nWriteData));
            }
        }
        catch { }
        
    }

    private string GetStToIntStr(string strValue)
    {
        string strData = "";

        try
        {
            if (strValue.Length % 2 == 1)
                strValue += " ";

            byte[] bytes = Encoding.UTF8.GetBytes(strValue);

            var usData = 0;
            for (var i = 0; i < bytes.Length / 2; i++)
            {
                usData = MAKEWORD(bytes[(i * 2) + 1], bytes[i * 2]);

                if (strValue == "")
                    strData += usData.ToString();
                else
                    strData += "," + usData.ToString();
            }
        }
        catch { }

        return strData;
    }

    public void SetTotalResult(int nResult)
    {
        var nRes = nResult;

        try
        {
            if (_plcParam.strWriteDev != "" && _plcParam.strWriteTotalResult != "")
            {
                var nWriteData = 0;

                if (_plcParam.SignalFormat == SignalFormat.DEC)
                {
                    if (nRes == 1)
                        int.TryParse(_plcParam.strOKSignal, out nWriteData);
                    else if (nRes == 0)
                        int.TryParse(_plcParam.strNGSignal, out nWriteData);
                    else
                        nWriteData = nRes;
                }
                else
                {
                    if (nRes == 1)
                        nWriteData = _plcParam.strOKSignal[0];
                    else if (nRes == 0)
                        nWriteData = _plcParam.strNGSignal[0];
                    else
                        nWriteData = nRes.ToString()[0];
                }

                _qdata.Enqueue(string.Format("{0},{1},{2}", _plcParam.strWriteDev, _plcParam.strWriteTotalResult, nWriteData));
            }
        }
        catch { }
    }

    public void LoadPLCParam()
    {
        SQL sql = new SQL();
        PLCPram plcParam = new PLCPram();

        try
        {
            sql.GetPLCParam(_strProcName, _dbInfo, ref plcParam);
            _plcParam = plcParam;
        }
        catch { }

        sql.Dispose();
    }

    public bool SavePLCParam()
    {
        SQL sql = new SQL();
        PLCPram plcParam = new PLCPram();

        try
        {
            plcParam = _plcParam;
            sql.SavePLCInfo(_strProcName, _dbInfo, plcParam);
        }
        catch { return false; }

        sql.Dispose();

        return true;
    }

    public void Disconnect()
    {
        _bReadPLC = false;

        if (_bConnect)
        {
            try
            {
                if (_plcParam.plcType == PLCType.MX)
                    _mxPLC.Close();
                else if (_plcParam.plcType == PLCType.LS)
                    _lsPLC.Dispose();
                else if (_plcParam.plcType == PLCType.Simens)
                    _SiemensPLC.Close();
            }
            catch { }
        }

        Thread.Sleep(100);
    }

    private void ReadPLC()
    {
        var nReadInterval = 0;
        var nReadCnt = 0;

        int[] nReadData = null;
        int[] nWriteData = new int[50];
        var nWriteAddr = 0;

        ushort[] usReadData = null;
        string strTemp = "";
        byte[] bytesTemp = null;

        var nReadAddr = 0;
        var nReadTrigger = 0;
        var nReadTriggerCnt = 0;
        var nTriggerSignal = 0;
        var nReadModel = 0;
        var nReadModelCnt = 0;
        var nReadModelDetailNo = 0;
        var nReadModelDetailNoCnt = 0;
        var nReadLot = 0;
        var nReadLotCnt = 0;

        var nCommType = 0;
        Stopwatch sw = new Stopwatch();
        bool bHeartbeat = false;

        string[] strWriteData = null;

        byte[] byteSend = null;
        byte[] byteRead = null;

        string strData = "";
        string strModelNo = "";
        string strLotNo = "";

        bool[] bTrigger = new bool[50];

        string[] strTriggerSignal = null;
        byte[] byteTemp = null;
        int nDetailNo = 0;

        while (_bReadPLC)
        {
            try
            {
                if (_bConnect)
                {
                    int.TryParse(_plcParam.strReadInterval, out nReadInterval);

                    if (_plcParam.strReadCnt == "")
                        nReadCnt = 100;
                    else
                        int.TryParse(_plcParam.strReadCnt, out nReadCnt);

                    if (_bytes == null)
                        _bytes = new byte[nReadCnt * 2];
                    else
                    {
                        if (_bytes.Length  != nReadCnt * 2)
                            _bytes = new byte[nReadCnt * 2];
                    }

                    if (_plcParam.plcType == PLCType.MX)
                    {
                        int.TryParse(_plcParam.strRead, out nReadAddr);

                        //MX 하트비트
                        if (_plcParam.strWriteHeartbeat != "" && _plcParam.strWriteDev != "")
                        {
                            if (!sw.IsRunning)
                            {
                                sw.Start();
                            }
                            else
                            {
                                if (sw.ElapsedMilliseconds >= int.Parse(_plcParam.strWriteHeartbeatInterval))
                                {
                                    sw.Restart();

                                    if (!bHeartbeat)
                                    {
                                        bHeartbeat = true;
                                        if (_plcParam.SignalFormat == SignalFormat.DEC)
                                            nWriteData[0] = 1;
                                        else
                                            nWriteData[0] = (int)'1';
                                    }
                                    else
                                    {
                                        bHeartbeat = false;

                                        if (_plcParam.SignalFormat == SignalFormat.DEC)
                                            nWriteData[0] = 0;
                                        else
                                            nWriteData[0] = (int)'0';
                                    }

                                    _mxPLC.SetDevice(string.Format("{0}{1}", _plcParam.strWriteDev, _plcParam.strWriteHeartbeat), nWriteData[0]);
                                }
                            }
                        }

                        if ((nReadData == null) || (nReadData.Length != nReadCnt))
                            nReadData = new int[nReadCnt];

                        //MX Read
                        _mxPLC.ReadDeviceBlock(string.Format("{0}{1}", _plcParam.strReadDev, nReadAddr), nReadCnt, nReadData);

                        for (var i = 0; i < nReadData.Length; i++)
                        {
                            byteTemp = BitConverter.GetBytes((Int16)nReadData[i]);
                            Array.Copy(byteTemp, 0, _bytes, i * 2, byteTemp.Length);
                        }

                        //Model
                        if (_plcParam.strReadModel != "" && _plcParam.strReadModelCnt != "")
                        {
                            int.TryParse(_plcParam.strReadModel, out nReadModel);
                            int.TryParse(_plcParam.strReadModelCnt, out nReadModelCnt);

                            strData = IntToStr(nReadAddr, nReadModel, nReadModelCnt, nReadData, null, _plcParam.SignalFormat, PLCType.MX);


                            if (_plcParam.strReadModelDetailNo != "" && _plcParam.strReadModelDetailNoCnt != "")
                            {
                                int.TryParse(_plcParam.strReadModel, out nReadModelDetailNo);

                                if (strData != "")
                                {
                                    if (strData != strModelNo || (nDetailNo != nReadData[nReadModelDetailNo - nReadAddr]))
                                    {
                                        strModelNo = strData;
                                        nDetailNo = nReadData[nReadModelDetailNo - nReadAddr];
                                        if (_OnModelChange != null)
                                            _OnModelChange(string.Format("{0}_{1}", strModelNo, nDetailNo));
                                    }
                                }
                            }
                            else
                            {
                                if (strData != "")
                                {
                                    if (strData != strModelNo)
                                    {
                                        strModelNo = strData;
                                        nDetailNo = nReadData[3];
                                        if (_OnModelChange != null)
                                            _OnModelChange(strModelNo);
                                    }
                                }
                            }
                        }

                        //Lot
                        if (_plcParam.strReadLot != "" && _plcParam.strReadLotCnt != "")
                        {
                            int.TryParse(_plcParam.strReadLot, out nReadLot);
                            int.TryParse(_plcParam.strReadLotCnt, out nReadLotCnt);

                            strData = IntToStr(nReadAddr, nReadLot, nReadLotCnt, nReadData, null, SignalFormat.HEX, PLCType.MX);

                            if (strData != "")
                            {
                                if (strData != strLotNo)
                                {
                                    strLotNo = strData;
                                    if (_OnLotNo != null)
                                        _OnLotNo(strLotNo);
                                }
                            }
                        }

                        //Trigger
                        if (_plcParam.strReadTrigger != "" && _plcParam.strTriggerSignal != "")
                        {
                            int.TryParse(_plcParam.strReadTrigger, out nReadTrigger);
                            int.TryParse(_plcParam.strReadTriggerCnt, out nReadTriggerCnt);

                            strTriggerSignal = _plcParam.strTriggerSignal.Split(',');

                            if (strTriggerSignal.Length > 0)
                            {
                                if (nReadTriggerCnt == 1)
                                {
                                    for (var i = 0; i < strTriggerSignal.Length; i++)
                                    {
                                        if (_plcParam.SignalFormat == SignalFormat.DEC)
                                            int.TryParse(strTriggerSignal[i], out nTriggerSignal);
                                        else
                                        {
                                            byteTemp = Encoding.UTF8.GetBytes(strTriggerSignal[i]);
                                            if (byteTemp.Length == 1)
                                                nTriggerSignal = MAKEWORD(byteTemp[0], 0);
                                            else
                                                nTriggerSignal = MAKEWORD(byteTemp[0], byteTemp[1]);
                                        }

                                        if (nReadData[nReadTrigger - nReadAddr] == nTriggerSignal)
                                        {
                                            if (_nScreenCnt > nReadTriggerCnt)
                                            {
                                                for (var j = 0; j<_nScreenCnt; j++)
                                                {
                                                    if (!bTrigger[j])
                                                    {
                                                        bTrigger[j] = true;

                                                        if (_OnTrigger != null)
                                                            _OnTrigger(j, nReadTriggerCnt);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (!bTrigger[i])
                                                {
                                                    bTrigger[i] = true;

                                                    if (_OnTrigger != null)
                                                        _OnTrigger(i, nReadTriggerCnt);
                                                }
                                            }
                                            
                                        }
                                        else if (nReadData[nReadTrigger - nReadAddr] == 0)
                                        {
                                            if (_nScreenCnt > nReadTriggerCnt)
                                            {
                                                for (var j = 0; j < _nScreenCnt; j++)
                                                    bTrigger[j] = false;
                                            }
                                            else
                                            {
                                                bTrigger[i] = false;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    int.TryParse(strTriggerSignal[0], out nTriggerSignal);

                                    if (_plcParam.SignalFormat == SignalFormat.DEC)
                                    {
                                        if (nTriggerSignal > 10)
                                            int.TryParse(_plcParam.strTriggerSignal, out nTriggerSignal);
                                    }
                                    else
                                    {
                                        nTriggerSignal = (int)_plcParam.strTriggerSignal[0];
                                    }

                                    for (var i = 0; i < nReadTriggerCnt; i++)
                                    {
                                        if (nReadData[nReadTrigger - nReadAddr + i] == nTriggerSignal)
                                        {
                                            if (!bTrigger[i])
                                            {
                                                bTrigger[i] = true;
                                                if (_OnTrigger != null)
                                                    _OnTrigger(i, nReadTriggerCnt);
                                            }
                                        }
                                        else if (nReadData[nReadTrigger - nReadAddr + 1] == 0)
                                            bTrigger[i] = false;
                                    }
                                }
                            }
                        }

                        //_nData = nReadData;

                        //MX Write
                        for (var i =0; i<_qdata.Count; i++)
                        {
                            strWriteData = _qdata.Dequeue().Split(',');

                            if (strWriteData.Length > 3)
                            {
                                nWriteData = new int[strWriteData.Length - 2];
                                for (var j = 0; j < strWriteData.Length - 2; j++)
                                    int.TryParse(strWriteData[j + 2], out nWriteData[j]);

                                _mxPLC.WriteDeviceBlock(string.Format("{0}{1}", strWriteData[0]), strWriteData.Length - 2, nWriteData);

                                nWriteData = new int[50];
                            }
                            else
                            {
                                int.TryParse(strWriteData[2], out nWriteData[0]);
                                _mxPLC.SetDevice(string.Format("{0}{1}", strWriteData[0], strWriteData[1]), nWriteData[0]);
                            }
                        }
                    }
                    else if (_plcParam.plcType == PLCType.LS)
                    {
                        //하트비트
                        if (_plcParam.strWriteHeartbeat != "" && _plcParam.strWriteDev != "")
                        {
                            if (!sw.IsRunning)
                            {
                                sw.Start();
                            }
                            else
                            {
                                if (sw.ElapsedMilliseconds >= int.Parse(_plcParam.strWriteHeartbeatInterval))
                                {
                                    sw.Restart();

                                    if (!bHeartbeat)
                                    {
                                        bHeartbeat = true;
                                        if (_plcParam.SignalFormat == SignalFormat.DEC)
                                            nWriteData[2] = 1;
                                        else
                                            nWriteData[2] = '1';
                                    }
                                    else
                                    {
                                        bHeartbeat = false;

                                        if (_plcParam.SignalFormat == SignalFormat.DEC)
                                            nWriteData[2] = 0;
                                        else
                                            nWriteData[2] = '0';
                                    }

                                    byteSend = BitConverter.GetBytes((ushort)nWriteData[2]);

                                    nWriteData[0] = _plcParam.strWriteDev[0];
                                    int.TryParse(_plcParam.strWriteHeartbeat, out nWriteData[1]);
                                    _lsPLC.Write((DeviceType)nWriteData[0], (uint)nWriteData[1] * 2, byteSend);
                                }
                            }
                        }

                        //LS Read
                        nWriteData[0] = _plcParam.strReadDev[0];
                        int.TryParse(_plcParam.strRead, out nReadAddr);
                        byteRead = _lsPLC.Read((DeviceType)nWriteData[0], (uint)nReadAddr * 2, nReadCnt * 2).ToArray();
                        _bytes = byteRead;

                        if (usReadData == null || (usReadData.Length != byteRead.Length / 2))
                            usReadData = new ushort[byteRead.Length / 2];

                        for (var i = 0; i < byteRead.Length / 2; i++)
                            usReadData[i] = BitConverter.ToUInt16(byteRead, i * 2);

                        //데이터 처리
                        //Model
                        if (_plcParam.strReadModel != "" && _plcParam.strReadModelCnt != "")
                        {
                            int.TryParse(_plcParam.strReadModel, out nReadModel);
                            int.TryParse(_plcParam.strReadModelCnt, out nReadModelCnt);

                            strData = IntToStr(nReadAddr, nReadModel, nReadModelCnt, null, usReadData, _plcParam.SignalFormat, PLCType.LS);

                            if (_plcParam.strReadModelDetailNo != "" && _plcParam.strReadModelDetailNoCnt != "")
                            {
                                int.TryParse(_plcParam.strReadModelDetailNo, out nReadModelDetailNo);

                                if (strData != "")
                                {
                                    if (strData != strModelNo || (nDetailNo != usReadData[nReadModelDetailNo - nReadAddr]))
                                    {
                                        strModelNo = strData;
                                        nDetailNo = usReadData[nReadModelDetailNo - nReadAddr];
                                        if (_OnModelChange != null)
                                            _OnModelChange(string.Format("{0}_{1}", strModelNo, nDetailNo));
                                    }
                                }
                            }
                            else
                            {
                                if (strData != "")
                                {
                                    if (strData != strModelNo)
                                    {
                                        strModelNo = strData;
                                        //nDetailNo = nReadData[3];
                                        if (_OnModelChange != null)
                                            _OnModelChange(strModelNo);
                                    }
                                }
                            }

                            //int.TryParse(_plcParam.strReadModel, out nReadModel);
                            //int.TryParse(_plcParam.strReadModelCnt, out nReadModelCnt);

                            //strData = IntToStr(nReadAddr, nReadModel, nReadModelCnt, null, usReadData, _plcParam.SignalFormat, PLCType.LS);

                            //if (strData != "")
                            //{
                            //    if (strData != strModelNo)
                            //    {
                            //        strModelNo = strData;
                            //        if (_OnModelChange != null)
                            //            _OnModelChange(strModelNo);
                            //    }
                            //}
                        }

                        //Lot
                        if (_plcParam.strReadLot != "" && _plcParam.strReadLotCnt != "")
                        {
                            int.TryParse(_plcParam.strReadLot, out nReadLot);
                            int.TryParse(_plcParam.strReadLotCnt, out nReadLotCnt);

                            strTemp = "";
                            strData = "";
                            for (var i = 0; i < nReadLotCnt; i++)
                            {
                                if (usReadData[nReadLot - nReadAddr + i] > 0)
                                {
                                    bytesTemp = BitConverter.GetBytes(usReadData[nReadLot - nReadAddr + i]);
                                    strTemp = Encoding.UTF8.GetString(bytesTemp);
                                    strTemp = strTemp.TrimEnd('\0');
                                    strData += strTemp.Trim();
                                }
                            }

                            //strData = IntToStr(nReadAddr, nReadLot, nReadLotCnt, null, usReadData, SignalFormat.HEX, PLCType.LS);

                            if (strData != "")
                            {
                                if (strData != strLotNo)
                                {
                                    strLotNo = strData;
                                    if (_OnLotNo != null)
                                        _OnLotNo(strLotNo);
                                }
                            }
                        }
                        _bStartSignal = false;
                        if (usReadData[12] == 1)
                            _bStartSignal = usReadData[12] == 1 ? true : false;

                        //Trigger
                        if (_plcParam.strReadTrigger != "" && _plcParam.strTriggerSignal != "")
                        {
                            int.TryParse(_plcParam.strReadTrigger, out nReadTrigger);
                            int.TryParse(_plcParam.strReadTriggerCnt, out nReadTriggerCnt);

                            strTriggerSignal = _plcParam.strTriggerSignal.Split(',');

                            if (strTriggerSignal.Length > 0)
                            {
                                if (nReadTriggerCnt == 1)
                                {
                                    if (strTriggerSignal.Length > 1)
                                    {
                                        for (var i = 0; i < strTriggerSignal.Length; i++)
                                        {
                                            if (_plcParam.SignalFormat == SignalFormat.DEC)
                                                int.TryParse(strTriggerSignal[i], out nTriggerSignal);
                                            else
                                            {
                                                byteTemp = Encoding.UTF8.GetBytes(strTriggerSignal[i]);
                                                if (byteTemp.Length == 1)
                                                    nTriggerSignal = MAKEWORD(byteTemp[0], 0);
                                                else
                                                    nTriggerSignal = MAKEWORD(byteTemp[0], byteTemp[1]);
                                            }

                                            if (usReadData[nReadTrigger - nReadAddr] == nTriggerSignal)
                                            {
                                                if (!bTrigger[i])
                                                {
                                                    bTrigger[i] = true;

                                                    if (_OnTrigger != null)
                                                        _OnTrigger(i, nReadTriggerCnt);
                                                }
                                            }
                                            else if (usReadData[nReadTrigger - nReadAddr] == 0)
                                            {
                                                bTrigger[i] = false;
                                                //Array.Clear(bTrigger, 0, bTrigger.Length);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (_plcParam.SignalFormat == SignalFormat.DEC)
                                            int.TryParse(strTriggerSignal[0], out nTriggerSignal);
                                        else
                                        {
                                            byteTemp = Encoding.UTF8.GetBytes(strTriggerSignal[0]);
                                            if (byteTemp.Length == 1)
                                                nTriggerSignal = MAKEWORD(byteTemp[0], 0);
                                            else
                                                nTriggerSignal = MAKEWORD(byteTemp[0], byteTemp[1]);
                                        }

                                        if (usReadData[nReadTrigger - nReadAddr] == nTriggerSignal)
                                        {
                                            for (var i = 0; i < _nScreenCnt; i++)
                                            {
                                                if (!bTrigger[i])
                                                {
                                                    bTrigger[i] = true;

                                                    if (_OnTrigger != null)
                                                        _OnTrigger(i, nReadTriggerCnt);
                                                }
                                            }
                                        }
                                        else if (usReadData[nReadTrigger - nReadAddr] == 0)
                                        {
                                            for (var i = 0; i < _nScreenCnt; i++)
                                                bTrigger[i] = false;
                                        }
                                    }
                                }
                                else
                                {
                                    int.TryParse(strTriggerSignal[0], out nTriggerSignal);

                                    if (_plcParam.SignalFormat == SignalFormat.DEC)
                                    {
                                        if (nTriggerSignal > 10)
                                            int.TryParse(_plcParam.strTriggerSignal, out nTriggerSignal);
                                    }
                                    else
                                    {
                                        nTriggerSignal = (int)_plcParam.strTriggerSignal[0];
                                    }

                                    for (var i = 0; i < nReadTriggerCnt; i++)
                                    {
                                        if (usReadData[nReadTrigger - nReadAddr + i] == nTriggerSignal)
                                        {
                                            if (!bTrigger[i])
                                            {
                                                bTrigger[i] = true;
                                                if (_OnTrigger != null)
                                                    _OnTrigger(i, nReadTriggerCnt);
                                            }
                                        }
                                        else if (usReadData[nReadTrigger - nReadAddr + i] == 0)
                                            bTrigger[i] = false;
                                    }
                                }
                            }
                        }

                        for (var i = 0; i < _qdata.Count; i++)
                        {
                            strWriteData = _qdata.Dequeue().Split(',');

                            nWriteData[0] = strWriteData[0][0];
                            int.TryParse(strWriteData[1], out nWriteData[1]);

                            byteSend = new byte[(strWriteData.Length - 2) * 2];
                            for (var j = 0; j<strWriteData.Length - 2; j++)
                            {
                                int.TryParse(strWriteData[2 + j], out nWriteData[2]);
                                byteTemp = BitConverter.GetBytes((ushort)nWriteData[2]);
                                Array.Copy(byteTemp, 0, byteSend, j * 2, byteTemp.Length);
                            }

                            _lsPLC.Write((DeviceType)nWriteData[0], (uint)(nWriteData[1] * 2), byteSend);
                           // _OnMessage(strWriteData[0] + "," + strWriteData[1] + "," + strWriteData[2]);

                        }
                    }
                    else if (_plcParam.plcType == PLCType.Simens)  //simens
                    {
                        int.TryParse(Regex.Replace(_plcParam.strRead, @"\D", ""), out nReadAddr);

                        if ((usReadData == null) || usReadData.Length != nReadCnt)
                            usReadData = new ushort[nReadCnt];

                        //하트비트
                        if (_plcParam.strWriteHeartbeat != "" && _plcParam.strWriteDev != "")
                        {
                            if (!sw.IsRunning)
                            {
                                sw.Start();
                            }
                            else
                            {
                                if (sw.ElapsedMilliseconds >= int.Parse(_plcParam.strWriteHeartbeatInterval))
                                {
                                    sw.Restart();

                                    if (!bHeartbeat)
                                    {
                                        bHeartbeat = true;
                                        if (_plcParam.SignalFormat == SignalFormat.DEC)
                                            nWriteData[0] = 1;
                                        else
                                            nWriteData[0] = (int)'1';
                                    }
                                    else
                                    {
                                        bHeartbeat = false;

                                        if (_plcParam.SignalFormat == SignalFormat.DEC)
                                            nWriteData[0] = 0;
                                        else
                                            nWriteData[0] = (int)'0';
                                    }

                                    _SiemensPLC.Write(string.Format("{0}{1}", _plcParam.strWriteDev, _plcParam.strWriteHeartbeat), (short)nWriteData[0]);
                                }
                            }
                        }

                        int.TryParse(Regex.Replace(_plcParam.strReadDev, @"\D", ""), out nWriteData[0]);
                        int.TryParse(Regex.Replace(_plcParam.strRead, @"\D", ""), out nWriteData[1]);

                        byteRead = _SiemensPLC.ReadBytes(S7.Net.DataType.DataBlock, nWriteData[0], nWriteData[1], nReadCnt);
                        usReadData = byteToushort(byteRead, PLCType.Simens);
                        _bytes = byteRead;

                        if (_plcParam.strReadModel != "" && _plcParam.strReadModelCnt != "")
                        {
                            int.TryParse(Regex.Replace(_plcParam.strReadModel, @"\D", ""), out nReadModel);
                            int.TryParse(_plcParam.strReadModelCnt, out nReadModelCnt);

                            strData = IntToStr(nReadAddr, nReadModel, nReadModelCnt, null, usReadData, _plcParam.SignalFormat, PLCType.Simens);

                            if (strData != "")
                            {
                                if (strData != strModelNo)
                                {
                                    strModelNo = strData;
                                    if (_OnModelChange != null)
                                        _OnModelChange(strModelNo);
                                }
                            }
                        }

                        //Lot
                        if (_plcParam.strReadLot != "" && _plcParam.strReadLotCnt != "")
                        {

                            int.TryParse(_plcParam.strReadLot, out nReadLot);
                            int.TryParse(_plcParam.strReadLotCnt, out nReadLotCnt);

                            strData = IntToStr(nReadAddr, nReadLot, nReadLotCnt, null, usReadData, SignalFormat.HEX, PLCType.Simens);

                            if (strData != "")
                            {
                                if (strData != strLotNo)
                                {
                                    strLotNo = strData;
                                    if (_OnLotNo != null)
                                        _OnLotNo(strLotNo);
                                }
                            }
                        }

                        //Trigger
                        if (_plcParam.strReadTrigger != "" && _plcParam.strTriggerSignal != "")
                        {
                            int.TryParse(_plcParam.strReadTrigger, out nReadTrigger);
                            int.TryParse(_plcParam.strReadTriggerCnt, out nReadTriggerCnt);

                            strTriggerSignal = _plcParam.strTriggerSignal.Split(',');

                            if (strTriggerSignal.Length > 0)
                            {
                                if (nReadTriggerCnt == 1)
                                {
                                    for (var i = 0; i < strTriggerSignal.Length; i++)
                                    {
                                        if (_plcParam.SignalFormat == SignalFormat.DEC)
                                            int.TryParse(strTriggerSignal[i], out nTriggerSignal);
                                        else
                                        {
                                            byteTemp = Encoding.UTF8.GetBytes(strTriggerSignal[i]);
                                            if (byteTemp.Length == 1)
                                                nTriggerSignal = MAKEWORD(byteTemp[0], 0);
                                            else
                                                nTriggerSignal = MAKEWORD(byteTemp[0], byteTemp[1]);
                                        }

                                        if (usReadData[nReadTrigger - nReadAddr] == nTriggerSignal)
                                        {
                                            if (!bTrigger[i])
                                            {
                                                bTrigger[i] = true;

                                                if (_OnTrigger != null)
                                                    _OnTrigger(i, nReadTriggerCnt);
                                            }
                                        }
                                        else if (usReadData[nReadTrigger - nReadAddr] == 0)
                                        {
                                            Array.Clear(bTrigger, 0, bTrigger.Length);
                                        }
                                    }
                                }
                                else
                                {
                                    int.TryParse(strTriggerSignal[0], out nTriggerSignal);

                                    if (_plcParam.SignalFormat == SignalFormat.DEC)
                                    {
                                        if (nTriggerSignal > 10)
                                            int.TryParse(_plcParam.strTriggerSignal, out nTriggerSignal);
                                    }
                                    else
                                    {
                                        nTriggerSignal = (int)_plcParam.strTriggerSignal[0];
                                    }

                                    for (var i = 0; i < nReadTriggerCnt; i++)
                                    {
                                        if (usReadData[nReadTrigger - nReadAddr + i] == nTriggerSignal)
                                        {
                                            if (!bTrigger[i])
                                            {
                                                bTrigger[i] = true;
                                                if (_OnTrigger != null)
                                                    _OnTrigger(i, nReadTriggerCnt);
                                            }
                                        }
                                        else if (usReadData[nReadTrigger - nReadAddr + i] == 0)
                                            bTrigger[i] = false;
                                    }
                                }
                            }
                        }

                        //_ushData = usReadData;

                        //Siemens Write
                        for (var i = 0; i < _qdata.Count; i++)
                        {
                            strWriteData = _qdata.Dequeue().Split(',');

                            int.TryParse(Regex.Replace(strWriteData[0], @"\D", ""), out nWriteData[0]);
                            int.TryParse(strWriteData[1], out nWriteData[1]);
                            //int.TryParse(strWriteData[2], out nWriteData[2]);

                            byteSend = null;
                            byteSend = new byte[(strWriteData.Length - 2) * 2];
                            Array.Clear(byteSend, 0, byteSend.Length);

                            HexToBytes(strWriteData, ref byteSend);
                            _SiemensPLC.WriteBytes(S7.Net.DataType.DataBlock, nWriteData[0], nWriteData[1], byteSend);
                        }
                    }
                }
                else
                {
                    Connect();
                    Delay(5000);
                }
            }
            catch(Exception ex) 
            {
                if (_OnMessage != null)
                    _OnMessage(ex.Message);
            }

            Thread.Sleep(nReadInterval);
        }
    }

    private ushort[] byteToushort(byte[] bytes, GlovalVar.PLCType pLCType)
    {
        ushort[] usData = new ushort[bytes.Length/2];

        try
        {
            for (var i = 0; i < bytes.Length / 2; i++)
            {
                if (pLCType == GlovalVar.PLCType.LS)
                    usData[i] = MAKEWORD(bytes[i * 2], bytes[(i * 2) + 1]);
                else
                    usData[i] = MAKEWORD(bytes[(i * 2) + 1], bytes[i * 2]);
            }
        }
        catch { }

        return usData;
    }

    private string IntToStr(int nStartAdd, int nReadStart, int nLen, int[] nData, ushort[] usData, GlovalVar.SignalFormat format, GlovalVar.PLCType pLCType)
    {
        string strData = "";
        var nReadAddr = 0;

        try
        {
            if (nData != null)
            {
                for (var i = 0; i < nLen; i++)
                {
                    if (nData[nReadStart - nStartAdd + i] > 0)
                    {
                        if (format == GlovalVar.SignalFormat.DEC)
                            strData += nData[nReadStart - nStartAdd + i].ToString();
                        else
                            strData += Convert.ToChar(nData[nReadStart - nStartAdd + i]);
                    }
                }
            }
            else if (usData != null)
            {
                for (var i = 0; i < nLen; i++)
                {
                    if (pLCType == GlovalVar.PLCType.LS)
                        nReadAddr = nReadStart - nStartAdd + i;
                    else if (pLCType == GlovalVar.PLCType.Simens)
                        nReadAddr = ((nReadStart - nStartAdd) /2) + i;

                    if (usData[nReadAddr] > 0)
                    {
                        if (format == GlovalVar.SignalFormat.DEC)
                            strData += usData[nReadAddr].ToString();
                        else
                            strData += Convert.ToChar(usData[nReadAddr]);
                    }
                }
            }
        }
        catch { }
        
        return strData;
    }

    private static UInt16 MAKEWORD(byte low, byte high)
    {
        return (UInt16)((high << 8) | low);
    }

    private void HexToBytes(string[]strData, ref byte[] bytes)
    {
        try
        {
            if (strData.Length == 3)
            {
                int.TryParse(strData[2], out var nValue);
                bytes[0] = Convert.ToByte(nValue.ToString("X4").Substring(2, 2), 16);
                bytes[1] = Convert.ToByte(nValue.ToString("X4").Substring(0, 2), 16);
            }
            else
            {
                for (var i = 0; i<strData.Length - 2; i++)
                {
                    int.TryParse(strData[2], out var nValue);
                    bytes[i * 2] = Convert.ToByte(nValue.ToString("X4").Substring(2, 2), 16);
                    bytes[(i * 2) +1] = Convert.ToByte(nValue.ToString("X4").Substring(0, 2), 16);
                }
            }
        }
        catch { }
    }
}
