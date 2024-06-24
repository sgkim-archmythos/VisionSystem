using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.Threading;

public delegate void IOConnectHandler(string strMsg);
public delegate void IODisconnectHandler(string strMsg);
public delegate void IOTriggerHandler(int nIdx);
public delegate void IOModelChangeHandler(string strModel);
public delegate void IOLotIDHandler(string strLotID);
public delegate void IOMessagehandler(string strMsg, Color color, string strMsgType);

public class ioBoard
{
    public IOConnectHandler _OnConnect;
    public IODisconnectHandler _OnDisconnect;
    public IOTriggerHandler _OnTrigger;
    public IOModelChangeHandler _OnModelChange;
    public IOLotIDHandler _OnLotIDReceive;
    public IOMessagehandler _OnMessage;
    short _shDev = -1;

    public List<string> _listWriteData = new List<string>();

    bool _bRead = false;
    bool _bWrite = false;

    public GlovalVar.PLCPram _plcParam;


    public bool Connect(GlovalVar.PLCPram plcParam)
    {
        try
        {
            _plcParam = plcParam;
            _shDev = DASK.Register_Card(DASK.PCI_7230, 0);

            if (_shDev == 0)
            {
                if (_listWriteData.Count > 0)
                    _listWriteData.Clear();

                for (int i = 0; i < 50; i++)
                    _listWriteData.Add("0");

                _bRead = true;
                Thread threadRead = new Thread(PLCRead);
                threadRead.Start();

                _bWrite = true;
                Thread threadWrite = new Thread(PLCWrite);
                threadRead.Start();

                if (_OnConnect != null)
                    _OnConnect("I/O Connected");
            }
            else
            {
                if (_OnDisconnect != null)
                    _OnDisconnect("I/O Disconnected");
            }
        }
        catch (Exception ex)
        {
            if (_OnMessage != null)
                _OnMessage("I/O Connect Error : " + ex.Message, Color.Red, "Alarm");
        }

        return (_shDev == 0) ? true : false;
    }

    private char[] GetReadData(uint nData)
    {
        char[] chars = Convert.ToString(3, 2).PadLeft(16, '0').PadLeft(16, '0').ToCharArray();
        Array.Reverse(chars);

        return chars;
    }

    private void PLCRead()///Comm Read ...PLC, Simens, IO
    {
        int nIntervar = 100;
        int nType = 0;
        Stopwatch sw = new Stopwatch();
        Stopwatch swHeart = new Stopwatch();

        string strHeartbeartAddr = "";
        int nHeartbeatInterval = 1000;

        bool[] bTrigger = null;
        string strRead = "";
        byte[] bytes = null;
        int nLen = 0;
        string strModel = "";
        string strSerial = "";
        string strReadStartAddr = "";
        //string strWriteStartAddr = "";
        int nReadTriggerStart = -1;
        int nReadTriggerEnd = -1;
        int nReadModelStart = -1;
        int nReadModelEnd = -1;

        string strTemp = "";

        

        

        int nStart = 0;
        uint nReadData = 0;
        char[] chars = null;

        sw.Start();
        while (true)
        {
            if (!_bRead)
                return;

            try
            {
                if (_shDev == 0)
                {
                    strHeartbeartAddr = _plcParam.strHeartBeatAddr;
                    nHeartbeatInterval = _plcParam.nHearBeatInterval;

                    if (nHeartbeatInterval == 0)
                        nHeartbeatInterval = 1000;

                    nIntervar = _plcParam.nReadInterval;

                    if (nIntervar == 0)
                        nIntervar = 100;

                    if (strHeartbeartAddr != "")
                    {
                        if (sw.ElapsedMilliseconds >= nHeartbeatInterval)
                        {
                            sw.Reset();
                            sw.Start();

                            swHeart.Reset();
                            swHeart.Start();

                            short.TryParse(strHeartbeartAddr, out short shHeartbeatAddr);
                            DASK.DO_WriteLine((ushort)_shDev, 0, (ushort)shHeartbeatAddr, 1);
                            //WriteResultDataBinary(strHeartbeartAddr, 0, "1");
                        }

                        if (swHeart.ElapsedMilliseconds>= 30)
                        {
                            short.TryParse(strHeartbeartAddr, out short shHeartbeatAddr);
                            DASK.DO_WriteLine((ushort)_shDev, 0, (ushort)shHeartbeatAddr, 0);
                        }
                    }

                    DASK.DI_ReadPort((ushort)_shDev, 0, out nReadData);

                    if (!string.IsNullOrEmpty(_plcParam.strReadStartTrigger))
                        int.TryParse(_plcParam.strReadStartTrigger, out nReadTriggerStart);

                    if (!string.IsNullOrEmpty(_plcParam.strReadEndTrigger))
                        int.TryParse(_plcParam.strReadEndTrigger, out nReadTriggerEnd);

                    if (!string.IsNullOrEmpty(_plcParam.strReadModelStart))
                        int.TryParse(_plcParam.strReadModelStart, out nReadModelStart);

                    if (!string.IsNullOrEmpty(_plcParam.strReadModelEnd))
                        int.TryParse(_plcParam.strReadModelEnd, out nReadModelEnd);

                    chars = GetReadData(nReadData);
                    nStart = 0;
                    for (int i = nReadTriggerStart; i < nReadTriggerEnd; i++)
                    {
                        if (chars[i] == '1')
                        {
                            if (!bTrigger[nStart])
                            {
                                if (_OnTrigger != null)
                                    _OnTrigger(i);

                                bTrigger[nStart] = true;
                            }
                        }
                    }

                    nStart = 0;
                    for (int i = nReadModelStart; i < nReadModelEnd; i++)
                    {
                        if (chars[i] == '1')
                        {
                            if (strModel != nStart.ToString())
                            {
                                if (_OnModelChange != null)
                                    _OnModelChange(strModel);
                            }
                        }
                        nStart++;
                    }
                }
                else
                    Connect(_plcParam);
            }
            catch { }

            Thread.Sleep(nIntervar);
        }
    }

    private void PLCWrite()
    {
        List<string> listWrite = new List<string>();

        for (int i = 0; i < _listWriteData.Count; i++)
            listWrite.Add("0");

        while (true)
        {
            if (!_bWrite)
                return;

            if (_shDev == 0)
            {
                for (int i = 0; i < listWrite.Count; i++)
                {
                    if (listWrite[i] != _listWriteData[i])
                    {
                        if (_listWriteData[i] != "")
                        {
                            listWrite[i] = _listWriteData[i];
                            _listWriteData[i] = "";

                            DASK.DO_WriteLine((ushort)_shDev, 0, (ushort)i, 1);
                            Thread.Sleep(50);
                            DASK.DO_WriteLine((ushort)_shDev, 0, (ushort)i, 0);
                        }
                        else
                            listWrite[i] = _listWriteData[i];
                    }
                }
            }

            Thread.Sleep(5);
        }
    }

    public void Disconnect()
    {
        _bRead = false;
        _bWrite = false;

        Thread.Sleep(100);

        if (_shDev == 0)
            DASK.Release_Card((ushort)_shDev);
    }

}
