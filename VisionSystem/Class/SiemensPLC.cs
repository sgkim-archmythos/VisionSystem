using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using S7.Net;


public delegate void SiemensConnectHandler(string strMsg);
public delegate void SiemensDisconnectHandler(string strMsg);
public delegate void SiemensTriggerHandler(int nIdx);
public delegate void SiemensModelChangeHandler(string strModel);
public delegate void SiemensLotIDHandler(string strLotID);
public delegate void SiemensMessagehandler(string strMsg, Color color, string strMsgType);

public class SiemensPLC
{

    public SiemensConnectHandler _OnConnect;
    public SiemensDisconnectHandler _OnDisconnect;
    public SiemensTriggerHandler _OnTrigger;
    public SiemensModelChangeHandler _OnModelChange;
    public SiemensLotIDHandler _OnLotIDReceive;
    public SiemensMessagehandler _OnMessage;

    public List<string> _listWriteData = new List<string>();

    bool _bRead = false;
    byte[] _bytes = null;
    string _strRectData = "";
    bool _bConnect = false;

    public GlovalVar.PLCPram _plcParam;
    Plc _SiemensPLC = null;

    public bool IsConnected
    {
        get
        {
            return _SiemensPLC.IsConnected;
        }
    }
    public bool Connect(GlovalVar.PLCPram plcParam)
    {
        try
        {
            _plcParam = plcParam;
            CpuType cpu = (CpuType)Enum.Parse(typeof(CpuType), _plcParam.strPLCPort);
            _SiemensPLC = new Plc(cpu, _plcParam.strPLCIP, _plcParam.shRack, _plcParam.shSlot);
            _SiemensPLC.Open();

            if (_SiemensPLC.IsConnected)
            {
                if (_listWriteData.Count > 0)
                    _listWriteData.Clear();

                for (int i = 0; i < 50; i++)
                    _listWriteData.Add("0");

                if (_OnConnect != null)
                    _OnConnect("SiemensPLC Connected");

                _bRead = true;
                Thread threadRead = new Thread(PLCRead);
                threadRead.Start();
            }
            else
            {
                if (_OnDisconnect != null)
                    _OnDisconnect("SiemensPLC Disconnected");
            }
        }
        catch(Exception ex)
        {
            if (_OnMessage != null)
                _OnMessage("SiemensPlC Connect Error : " + ex.Message, Color.Red, "Alarm");
        }

        return _SiemensPLC.IsConnected;
    }

    private void PLCRead()
    {
        int nIntervar = 100;
        int nType = 0;
        Stopwatch sw = new Stopwatch();

        string strHeartbeartAddr = "";
        int nHeartbeatInterval = 1000;

        string strRead = "";
        byte[] bytes = null;
        int nLen = 0;
        string strReadStartAddr = "";
        string strWriteStartAddr = "";
        int nReadTriggerStart = -1;
        int nReadTriggerEnd = -1;
        int nReadModelStart = -1;
        int nReadModelEnd = -1;
        int nReadLotStart = -1;
        int nReadLotEnd = -1;

        string strStarSignal = "";
        string strOkSignal = "";
        string strNGSignal = "";

        string strTemp = "";

        List<string> listWrite = new List<string>();

        for (int i = 0; i < 30; i++)
            listWrite.Add("0");

        int nStart = 0;

        sw.Start();

        while (true)
        {
            if (!_bRead)
                return;

            try
            {
                if (_SiemensPLC.IsConnected)
                {
                    strHeartbeartAddr = _plcParam.strHeartBeatAddr;
                    nHeartbeatInterval = _plcParam.nHearBeatInterval;

                    if (nHeartbeatInterval == 0)
                        nHeartbeatInterval = 1000;

                    nType = _plcParam.nCommType;
                    nIntervar = _plcParam.nReadInterval;

                    if (nIntervar == 0)
                        nIntervar = 100;

                    if (strHeartbeartAddr != "")
                    {
                        if (sw.ElapsedMilliseconds >= nHeartbeatInterval)
                        {
                            sw.Reset();
                            sw.Start();

                            _SiemensPLC.Write(string.Format("{0}.{1}", "DB3200", "DBW0"), (short)1);  // DB3200.DBW0;
                        }
                    }

                    //    strReadStartAddr = _plcParam.strReadStartAddr;
                    //    bytes = null;
                    //    SendBinary("D100", 12);
                    //    bytes = _bytes;
                    //    if (bytes != null)
                    //    {
                    //        int.TryParse(_plcParam.strReadLength, out nLen);

                    //        if (nLen == 0)
                    //            nLen = 50;

                    //        if (bytes != null)
                    //        {
                    //            strRead = PLCDataDecode(bytes, nLen);

                    //            if (_strRectData != strRead)
                    //            {
                    //                _strRectData = strRead;

                    //                if (string.IsNullOrEmpty(_plcParam.strReadStartTrigger))
                    //                    int.TryParse(_plcParam.strReadStartTrigger, out nReadTriggerStart);

                    //                if (string.IsNullOrEmpty(_plcParam.strReadEndTrigger))
                    //                    int.TryParse(_plcParam.strReadStartTrigger, out nReadTriggerEnd);

                    //                if (string.IsNullOrEmpty(_plcParam.strReadModelStart))
                    //                    int.TryParse(_plcParam.strReadStartTrigger, out nReadModelStart);

                    //                if (string.IsNullOrEmpty(_plcParam.strReadModelEnd))
                    //                    int.TryParse(_plcParam.strReadStartTrigger, out nReadModelEnd);

                    //                if (string.IsNullOrEmpty(_plcParam.strReadLotStart))
                    //                    int.TryParse(_plcParam.strReadStartTrigger, out nReadLotStart);

                    //                if (string.IsNullOrEmpty(_plcParam.strReadLotStart))
                    //                    int.TryParse(_plcParam.strReadStartTrigger, out nReadLotEnd);

                    //                strStarSignal = _plcParam.strStartSignal;

                    //                if (nReadTriggerStart != -1 && nReadTriggerEnd != -1)
                    //                {
                    //                    nStart = 0;
                    //                    for (int i = nReadTriggerStart; i < nReadTriggerEnd * 2; i++)
                    //                    {
                    //                        strTemp = Regex.Replace(strRead.Substring(1 * 2, 2), @"\d", "");
                    //                        if (strStarSignal == strTemp)
                    //                        {
                    //                            //if (_CAM[nStart] != null)
                    //                            //    _CAM[nStart].Grab(false);
                    //                        }

                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }

                    //    strWriteStartAddr = _plcParam.strWriteStartAddr;
                    //    strOkSignal = _plcParam.strOKSignal;
                    //    strNGSignal = _plcParam.strNGSignal;
                    //    for (int i = 0; i < listWrite.Count; i++)
                    //    {
                    //        if (listWrite[i] != _listWriteData[i])
                    //        {
                    //            listWrite[i] = _listWriteData[i];

                    //            bytes = null;
                    //            bytes = WriteResultDataBinary(strWriteStartAddr, i, listWrite[i]);

                    //            if (bytes != null)
                    //                _client.SendBytes(bytes);
                    //        }
                    //    }
                    //}
                }
            }
            catch { }


            Thread.Sleep(10);
        }
    }


    public void Disconnect()
    {
        _bRead = false;

        Thread.Sleep(100);
        if (_SiemensPLC.IsConnected)
            _SiemensPLC.Close();
    }
}
