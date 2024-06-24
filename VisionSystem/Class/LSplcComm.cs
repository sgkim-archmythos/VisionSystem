using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Threading;
using System.Diagnostics;


public delegate void LSplcConnectHandler(string strMsg);
public delegate void LSplcDisconnectHandler(string strMsg);
public delegate void LSplcTriggerHandler(int nIdx);
public delegate void LSplcModelChangeHandler(string strModel);
public delegate void LSplcLotIDHandler(string strLotID);
public delegate void LSplcMessagehandler(string strMsg, Color color);
public delegate void LSplcRecvData(string strMsg, byte[] bytes);
public class LSplcComm
{
    TCPComponents.TCPSimpleClient _client = new TCPComponents.TCPSimpleClient();
    //bool _bConnect = false;

    public LSplcConnectHandler _OnConnect;
    public LSplcDisconnectHandler _OnDisconnect;
    public LSplcTriggerHandler _OnTrigger;
    public LSplcModelChangeHandler _OnModelChange;
    public LSplcLotIDHandler _OnLotIDReceive;
    public LSplcMessagehandler _OnMessage;
    public LSplcRecvData _OnRecvData;

    public List<string> _listWriteData = new List<string>();

    bool _bRead = false;
    byte[] _bytes = null;
    string _strRectData = "";

    public bool _bCamStatus = true;

    public GlovalVar.PLCPram _plcParam;
    bool _bConnect = false;
    public string _strBCRData = "";

    public void LoadSet()
    {
        this._client.OnConnect += new TCPComponents.TCPSimpleClient.OnConnectDelegate(this.OnConnect);
        this._client.OnDisconnect += new TCPComponents.TCPSimpleClient.OnDisconnectDelegate(this.OnDisconnect);
        this._client.OnDataAvailable += new TCPComponents.TCPSimpleClient.OnDataAvailableDelegate(this.OnDataAvailable);
    }

    private void OnConnect(object sender)
    {
        _bConnect = true;
        if (_OnConnect != null)
            _OnConnect("LS PLC Connected");
    }
    private void OnDisconnect(object sender)
    {
        _bConnect = false;
        if (_OnDisconnect != null)
            _OnDisconnect("LS PLC Disconnected");
    }

    private void OnDataAvailable(object sender, string data, byte[] _data)
    {
        _bytes = _data;
    }

    public bool isConnected
    {
        get
        {
            return _client.Connected;
        }
    }

    public void Disconnect()
    {
        _bRead = false;
        Thread.Sleep(100);

        if (_client.Connected)
            _client.Disconnect();
    }
    public bool Connect(GlovalVar.PLCPram plcParam)
    {
        _plcParam = plcParam;
        _client.Host = _plcParam.strPLCIP;
        int.TryParse(_plcParam.strPLCPort, out int nPort);
        _client.Port = nPort;

        try
        {
            _client.Connect();
            if (_client.Connected)
            {
                if (_listWriteData.Count > 0)
                    _listWriteData.Clear();

                int.TryParse(_plcParam.strReadLength, out int nLen);
                for (int i = 0; i < nLen; i++)
                    _listWriteData.Add("0");

                _bConnect = true;
                _bRead = false;
                Thread.Sleep(50);
                _bRead = true;
                Thread threadRead = new Thread(PLCRead);
                threadRead.Start();
            }
        }
        catch (Exception ex)
        {
            if (_OnMessage != null)
                _OnMessage(ex.Message, Color.Red);
        }

        return _client.Connected;
    }

    private void PLCRead()///Comm Read ...PLC, Simens, IO
    {
        int nIntervar = 100;
        int nType = 0;
        Stopwatch sw = new Stopwatch();

        string strHeartbeartAddr = "";
        int nHeartbeatInterval = 1000;

        bool[] bTrigger = new bool[30];
        string strRead = "";
        byte[] bytes = null;
        int nLen = 0;
        string strModel = "";
        string strSerial = "";
        string strReadStartAddr = "";
        string strWriteStartAddr = "";
        int nReadTriggerStart = -1;
        int nReadTriggerEnd = -1;
        int nReadModelStart = -1;
        int nReadModelEnd = -1;
        int nReadLotStart = -1;
        int nReadLotEnd = -1;
        int nWriteResStart = -1;
        int nWriteBCR = -1;

        string strWriteData = "";
        string strStarSignal = "";
        string strTemp = "";

        List<string> listWrite = new List<string>();

        for (int i = 0; i < _listWriteData.Count; i++)
            listWrite.Add("0");

        long[] lValue = null;
        string strAddr = "";
        int nAddr;
        string[] strValue = null;

        sw.Start();
        string strCamStatusAddr = "";
        int nStatus = -1;

        int nDiv1, nDiv2;

        while (true)
        {
            if (!_bRead)
                return;

            try
            {
                if (_bConnect)
                {
                    lValue = null;
                    lValue = new long[1];

                    if (nStatus == -1)
                    {
                        if (_bCamStatus)
                            lValue[0] = 49;
                        else
                            lValue[0] = 50;

                        strCamStatusAddr = string.Format("%{0}W{1}", "M", 80002);
                        RegisterWriteL(strCamStatusAddr, lValue);
                    }
                    else
                    {
                        if (_bCamStatus)
                        {
                            if (nStatus == 2)
                            {
                                nStatus = 1;
                                lValue[0] = 49;

                                strCamStatusAddr = string.Format("%{0}W{1}", "M", 80002);
                                RegisterWriteL(strCamStatusAddr, lValue);
                            }
                        }
                        else
                        {
                            if (nStatus == 1)
                            {
                                nStatus = 2;
                                lValue[0] = 50;

                                strCamStatusAddr = string.Format("%{0}W{1}", "M", 80002);
                                RegisterWriteL(strCamStatusAddr, lValue);
                            }
                        }
                    }

                    strHeartbeartAddr = _plcParam.strHeartBeatAddr;
                    nHeartbeatInterval = _plcParam.nHearBeatInterval;

                    if (nHeartbeatInterval == 0)
                        nHeartbeatInterval = 1000;

                    nIntervar = _plcParam.nReadInterval;

                    if (nIntervar == 0)
                        nIntervar = 100;

                    strWriteStartAddr = _plcParam.strWriteStartAddr;
                    if (strWriteStartAddr != "" && strHeartbeartAddr != "")
                    {
                        if (sw.ElapsedMilliseconds >= nHeartbeatInterval)
                        {
                            sw.Reset();
                            sw.Start();

                            lValue[0] = 49;
                            int.TryParse(Regex.Replace(strWriteStartAddr, @"\D", ""), out nAddr);
                            strAddr = Regex.Replace(strWriteStartAddr, @"\d", "");
                            strCamStatusAddr = string.Format("%{0}W{1}", strAddr, nAddr);
                            RegisterWriteL(strCamStatusAddr, lValue);

                            //RegisterRead(string.Format("%{0}B{1}", strAddr, nAddr), nLen);
                        }
                    }

                    if (!string.IsNullOrEmpty(_plcParam.strReadStartAddr) && !string.IsNullOrEmpty(_plcParam.strReadLength))
                    {
                        strReadStartAddr = _plcParam.strReadStartAddr;
                        int.TryParse(_plcParam.strReadLength, out nLen);

                        if (nLen == 0)
                            nLen = 50;

                        int.TryParse(Regex.Replace(strReadStartAddr, @"\D", ""), out nAddr);
                        strAddr = Regex.Replace(strReadStartAddr, @"\d", "");
                        bytes = null;
                        RegisterRead(string.Format("%{0}B{1}", strAddr, nAddr * 2), nLen);

                        Thread.Sleep(30);
                        bytes = _bytes;
                        if (bytes != null)
                        {
                            strRead = DataRead(bytes);

                            if (_strRectData != strRead)
                            {
                                _strRectData = strRead;

                                if (_OnRecvData != null)
                                    _OnRecvData(_strRectData, bytes);

                                strValue = null;
                                strValue = _strRectData.Split(';');

                                if (!string.IsNullOrEmpty(_plcParam.strReadStartTrigger))
                                    int.TryParse(_plcParam.strReadStartTrigger, out nReadTriggerStart);

                                if (!string.IsNullOrEmpty(_plcParam.strReadEndTrigger))
                                    int.TryParse(_plcParam.strReadEndTrigger, out nReadTriggerEnd);

                                if (!string.IsNullOrEmpty(_plcParam.strReadModelStart))
                                    int.TryParse(_plcParam.strReadModelStart, out nReadModelStart);

                                if (!string.IsNullOrEmpty(_plcParam.strReadModelEnd))
                                    int.TryParse(_plcParam.strReadModelEnd, out nReadModelEnd);

                                if (!string.IsNullOrEmpty(_plcParam.strReadLotStart))
                                    int.TryParse(_plcParam.strReadLotStart, out nReadLotStart);

                                if (!string.IsNullOrEmpty(_plcParam.strReadLotEnd))
                                    int.TryParse(_plcParam.strReadLotEnd, out nReadLotEnd);

                                strStarSignal = _plcParam.strStartSignal;

                                if (nReadTriggerStart != -1 && nReadTriggerEnd != -1)
                                {
                                    for (int i = 0; i < nReadTriggerEnd; i++)
                                    {
                                        if (strValue[nReadTriggerStart + i].TrimEnd('\0') == strStarSignal)
                                        {
                                            if (!bTrigger[i])
                                            {
                                                if (_OnTrigger != null)
                                                    _OnTrigger(i);

                                                bTrigger[i] = true;
                                            }
                                        }
                                        else
                                            bTrigger[i] = false;
                                    }
                                }

                                if (nReadModelStart != -1 && nReadModelEnd != -1)
                                {
                                    strTemp = "";
                                    for (int i = 0; i < nReadModelEnd; i++)
                                        strTemp += strValue[nReadModelStart + i].TrimEnd('\0');

                                    if (strTemp != "0" || strTemp != "")
                                    {
                                        if (strTemp != strModel)
                                        {
                                            if (_OnModelChange != null)
                                                _OnModelChange(strTemp);

                                            strModel = strTemp;
                                        }
                                    }
                                }

                                if (nReadLotStart != -1 && nReadLotEnd != -1)
                                {
                                    strTemp = "";
                                    for (int i = 0; i < nReadLotEnd; i++)
                                        strTemp += strValue[nReadLotStart + i].TrimEnd('\0');

                                    if (strTemp != strSerial)
                                    {
                                        if (_OnLotIDReceive != null)
                                            _OnLotIDReceive(strTemp);

                                        strSerial = strTemp;
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(strWriteStartAddr))
                        {
                            int.TryParse(strWriteStartAddr, out nWriteResStart);
                            for (int i = 0; i < listWrite.Count; i++)
                            {
                                if (listWrite[i] != _listWriteData[i])
                                {
                                    if (_listWriteData[i] != "")
                                    {
                                        listWrite[i] = _listWriteData[i];
                                        _listWriteData[i] = "";

                                        bytes = null;
                                        bytes = Encoding.UTF8.GetBytes(listWrite[i]);
                                        strWriteData = BitConverter.ToString(bytes).Replace("-", "").PadRight(4, '0');

                                        lValue = null;
                                        lValue = new long[strWriteData.Length / 4];

                                        for (int j = 0; j < lValue.Length; j++)
                                            lValue[i] = Convert.ToInt32(strWriteData.Substring(i * 4, 4), 16);

                                        int.TryParse(Regex.Replace(strWriteStartAddr, @"\D", ""), out nAddr);
                                        strAddr = Regex.Replace(strWriteStartAddr, @"\d", "");
                                        strCamStatusAddr = string.Format("%{0}W{1}", strAddr, nAddr + i);

                                        RegisterWriteL(strCamStatusAddr, lValue);
                                    }
                                    else
                                        listWrite[i] = _listWriteData[i];
                                }
                            }

                            if (!string.IsNullOrEmpty(_strBCRData))
                            {
                                if (!string.IsNullOrEmpty(_plcParam.strWrite2DData))
                                {
                                    bytes = null;
                                    bytes = Encoding.UTF8.GetBytes(_strBCRData);

                                    nDiv1 = _strBCRData.Length / 4;
                                    nDiv2 = _strBCRData.Length % 4;

                                    nLen = (nDiv2 == 0) ? (4 * nDiv1) : (4 * (nDiv1 + 1));

                                    strWriteData = BitConverter.ToString(bytes).Replace("-", "").PadRight(nLen, '0');

                                    lValue = null;
                                    lValue = new long[nLen / 4];

                                    for (int i = 0; i < lValue.Length; i++)
                                        lValue[i] = Convert.ToInt32(strWriteData.Substring(i * 4, 4), 16);

                                    int.TryParse(_plcParam.strWrite2DData, out nWriteBCR);
                                    int.TryParse(Regex.Replace(strWriteStartAddr, @"\D", ""), out nAddr);
                                    strAddr = Regex.Replace(strWriteStartAddr, @"\d", "");

                                    strCamStatusAddr = string.Format("%{0}W{1}", strAddr, nAddr + nWriteBCR);
                                    RegisterWriteL(strCamStatusAddr, lValue);
                                }
                            }
                        }
                    }
                }
                else
                    Connect(_plcParam);
            }
            catch { }

            Thread.Sleep(nIntervar);
        }
    }

    private long MAKE_WORD(int _a, int _b)
    {
        return ((int)_a << 8) | _b;
    }

    private string DataRead(byte[] bytes)
    {
        //object GetVal = 0;
        string strRead = "";
        int nEnd = 0;
        try
        {
            char[] buffer = Encoding.Default.GetChars(bytes);

            if (bytes[26] == 0x00 && bytes[27] == 0x00)
            {
                if (bytes[20] == 0x55)
                {
                    int nBock = (ushort)MAKE_WORD(bytes[29], bytes[28]);
                    int nData = (ushort)MAKE_WORD(bytes[31], bytes[30]);

                    // 읽기 응답 : 0x59
                    //[ h0000 비트 ][ h0100 바이트 ][ h0200 워드 ][ h0300 더블워드 ][ h0400 롱워드][ h1400 연속]
                    if (bytes[22] == 0x14)
                    {
                        string strData = "";
                        for (int i = 0; i < nData / 2; i++)
                        {
                            ushort uData = (ushort)MAKE_WORD(bytes[33 + (i * 2)], bytes[32 + (i * 2)]);
                            strData = uData.ToString("X4");

                            int nCnt = strData.Length / 2;
                            for (int j = 0; j < nCnt; j++)
                                strRead += Convert.ToChar(Convert.ToByte(strData.Substring((nCnt - j - 1) * 2, 2), 16));

                            nEnd++;
                            if (nEnd == 1)
                            {
                                strRead += ";";
                                nEnd = 0;
                            }

                            strData = "";
                        }
                    }

                    #region

                    //[ h0200 워드 ]
                    if (bytes[22] == 0x02)
                    {
                        for (int i = 0; i < nData / 2; i++)
                        {
                            ushort uData = (ushort)MAKE_WORD(bytes[33 + (i * 2)], bytes[32 + (i * 2)]);

                            //if (uData != m_DataBuff[i])
                            //{
                            //    m_DataBuff[i] = uData;
                            //}
                        }
                        // 27= 블록 개수 , 29 = 데이터 갯수
                    }
                    #endregion
                }
                else if (bytes[20] == 0x59)
                {
                    // 쓰기 응답 : 0x59
                    //DisplayMsg("쓰기 응답(0x59)--> 정상");
                }
                else if (bytes[20] == 0x89)
                {
                    int nErr = bytes[28];
                    //ErrorMessage(nErr);
                }
                //m_bSendingTx = false;
                //GetVal = m_DataBuff[0];
            }
            else
            {
                // 에러상태
                // 에러코드(Hex 1Byte)

            }

        }
        catch (System.Exception ex)
        {

        }

        return strRead;
    }

    private void RegisterGetBRead(string devicetype, int iAddress)
    {
        string device = string.Format("%{0}B{1}", devicetype, iAddress);

        this.RegisterGetWord(device);
    }
    private void RegisterWriteBit(string szDevice, bool pData)
    {
        RegisterWrite(szDevice, pData ? 0x01 : 0x00);
    }
    private void RegisterWriteByte(string szDevice, params byte[] pData)
    {
        if (pData.Length == 1)
        {
            RegisterWrite(szDevice, pData[0]);
        }
        else
        {
            RegisterWriteArray(szDevice, pData);
        }
    }
    private void RegisterWriteW(string szDevice, params ushort[] pData)
    {
        if (pData.Length == 1)
        {
            RegisterWrite(szDevice, pData[0]);
        }
        else
        {
            byte[] bytes = new byte[pData.Length * 2];

            for (int i = 0; i < pData.Length; i++)
            {
                int nData = pData[i];
                bytes[i * 2] = (byte)(nData & 0xFF);
                bytes[(i * 2) + 1] = (byte)(nData >> 8);
            }

            RegisterWriteArray(szDevice, bytes);
        }
    }
    private void RegisterWriteL(string szDevice, long[] pData)
    {
        if (pData.Length == 1)
        {
            RegisterWrite(szDevice, pData[0]);
        }
        else
        {
            byte[] bytes = new byte[pData.Length * 2];

            for (int i = 0; i < pData.Length; i++)
            {
                long nData = pData[i];

                bytes[i * 2] = (byte)(nData & 0xFF);
                bytes[(i * 2) + 1] = (byte)(nData >> 8);

                bytes[(i * 2) + 2] = (byte)(nData & 0xFFFF);
                bytes[(i * 2) + 3] = (byte)(nData >> 16);
            }

            RegisterWriteArray(szDevice, bytes);
        }
    }

    private int ByteCheckSum(char[] buff, int iStart, int iEnd)
    {
        int CheckSum = 0;

        for (int i = iStart; i < iEnd; i++)
        {
            CheckSum = CheckSum + buff[i];

            if (CheckSum > 255)
            {
                CheckSum = CheckSum - 256;
            }
        }

        return CheckSum;
    }

    private void RegisterRead(string szDevice, int nLen)
    {
        int nLength = 0, nCnt = 0;
        char[] buf = new char[255];

        Array.Clear(buf, 0, buf.Length);

        // LSIS 고유번호(10)

        buf[nCnt++] = 'L';
        buf[nCnt++] = 'S';
        buf[nCnt++] = 'I';
        buf[nCnt++] = 'S';
        buf[nCnt++] = '-';
        buf[nCnt++] = 'X';
        buf[nCnt++] = 'G';
        buf[nCnt++] = 'T';

        buf[nCnt++] = (char)0x00;
        buf[nCnt++] = (char)0x00;

        // PLC 정보(2)
        buf[nCnt++] = (char)0x00;
        buf[nCnt++] = (char)0x00;

        // CPU 정보(2)
        //buf[nCnt++] = (char)0xA0;
        buf[nCnt++] = (char)0xA4;
        //buf[nCnt++] = (char)0xA8;
        //buf[nCnt++] = (char)0xB0;
        //buf[nCnt++] = (char)0xB4;

        //프레임 방향(1)
        buf[nCnt++] = (char)0x33;

        //프레임 순서번호(2)
        buf[nCnt++] = (char)0x00;
        buf[nCnt++] = (char)0x01;

        //길이(2)
        nLength = szDevice.Length - 4;

        if (nLength < 0)
        {
            nLength = 0;
        }

        nLength += 0x10;

        buf[nCnt++] = (char)nLength;
        buf[nCnt++] = (char)0x00;


        //위치 정보(1)
        buf[nCnt++] = (char)0x00;

        //체크섬(1)
        buf[nCnt++] = (char)ByteCheckSum(buf, 0, nCnt - 1);

        //명령어(2)
        //[ h5400 읽기][ h5800 쓰기 ]

        buf[nCnt++] = (char)0x54;
        buf[nCnt++] = (char)0x00;

        //데이터 타입(2)
        //[ h00 비트 ][ h01 바이트 ][ h02 워드 ][ h03 더블워드 ][ h04 롱워드][ h14 연속]

        buf[nCnt++] = (char)0x14;
        buf[nCnt++] = (char)0x00;

        //예약 영역(2)
        //0x0000 : Don’t Care.
        buf[nCnt++] = (char)0x00;
        buf[nCnt++] = (char)0x00;

        //블록수(2)
        //읽고자 하는 블록의 개수. 0x0001
        buf[nCnt++] = (char)0x01;
        buf[nCnt++] = (char)0x00;

        //변수명 길이(2)
        //변수 명의 길이. 최대 16자.
        buf[nCnt++] = (char)szDevice.Length;
        buf[nCnt++] = (char)0x00;

        //데이터 주소
        //쓰고자 하는 Data, 최대 1400byte

        char[] ch = szDevice.ToCharArray();

        for (int i = 0; i < ch.Length; i++)
        {
            buf[nCnt++] = ch[i];
        }

        //데이터 개수(길이)(2)

        switch (szDevice.Substring(1, 1))
        {
            case "P":
            case "Z":
            case "S":
                {
                    nLength = 128 * 2;
                    break;
                }
            default:
                {
                    nLength = 256 * 2;
                    break;
                }
        }

        buf[nCnt++] = (char)(nLength & 0xFF); //연속읽기 일 경우 Data length(L)
        buf[nCnt++] = (char)(nLength >> 8);   //연속읽기 일 경우 Data length(H)   

        Array.Resize(ref buf, nCnt);
        byte[] bytes = new byte[nCnt];

        for (int i = 0; i < nCnt; i++)
            bytes[i] = Convert.ToByte(buf[i]);

        if (_client.Connected)
            _client.SendBytes(bytes);
    }
    private void RegisterWrite(string szDevice, long pData)
    {
        int nLength = 0, nCnt = 0;
        char[] buf = new char[1024];

        Array.Clear(buf, 0, buf.Length);

        // LSIS 고유번호(10)

        buf[nCnt++] = 'L';
        buf[nCnt++] = 'S';
        buf[nCnt++] = 'I';
        buf[nCnt++] = 'S';
        buf[nCnt++] = '-';
        buf[nCnt++] = 'X';
        buf[nCnt++] = 'G';
        buf[nCnt++] = 'T';

        buf[nCnt++] = (char)0x00;
        buf[nCnt++] = (char)0x00;

        // PLC 정보(2)
        buf[nCnt++] = (char)0x00;
        buf[nCnt++] = (char)0x00;

        // CPU 정보(2)
        //buf[nCnt++] = (char)0xA0;
        buf[nCnt++] = (char)0xA4;
        //buf[nCnt++] = (char)0xA8;
        //buf[nCnt++] = (char)0xB0;
        //buf[nCnt++] = (char)0xB4;

        //프레임 방향(1)
        buf[nCnt++] = (char)0x33;

        //프레임 순서번호(2)
        buf[nCnt++] = (char)0x00;
        buf[nCnt++] = (char)0x01;

        //길이(2)

        nLength = szDevice.Length - 4;

        if (nLength < 0)
        {
            nLength = 0;
        }

        nLength += 0x11;

        switch (szDevice.Substring(2, 1))
        {
            case "B":
            case "W":
                {
                    nLength += 1;
                    break;
                }
            case "D":
                {
                    nLength += 2;
                    break;
                }
            case "L":
                {
                    nLength += 4;
                    break;
                }
        }

        //길이(2)
        buf[nCnt++] = (char)(nLength & 0xFF);           // Data length(L)
        buf[nCnt++] = (char)(nLength >> 8);             // Data length(H)   

        //위치 정보(1)
        buf[nCnt++] = (char)0x00;

        //체크섬(1)
        buf[nCnt++] = (char)ByteCheckSum(buf, 0, nCnt - 1);

        //명령어(2)
        //[ h5400 읽기][ h5800 쓰기 ]

        buf[nCnt++] = (char)0x58;
        buf[nCnt++] = (char)0x00;

        //데이터 타입(2)
        //[ h00 비트 ][ h01 바이트 ][ h02 워드 ][ h03 더블워드 ][ h04 롱워드][ h14 연속]

        switch (szDevice.Substring(2, 1))
        {
            case "X":
                {
                    nLength = 1;
                    buf[nCnt++] = (char)0x00;
                    buf[nCnt++] = (char)0x00;
                    break;
                }
            case "B":
                {
                    nLength = 1;

                    buf[nCnt++] = (char)0x01;
                    buf[nCnt++] = (char)0x00;
                    break;
                }
            case "W":
                {
                    nLength = 2;

                    buf[nCnt++] = (char)0x02;
                    buf[nCnt++] = (char)0x00;
                    break;
                }
            case "D":
                {
                    nLength = 4;

                    buf[nCnt++] = (char)0x03;
                    buf[nCnt++] = (char)0x00;
                    break;
                }
            case "L":
                {
                    nLength = 8;

                    buf[nCnt++] = (char)0x04;
                    buf[nCnt++] = (char)0x00;
                    break;
                }
        }

        //예약 영역(2)
        //0x0000 : Don’t Care.
        buf[nCnt++] = (char)0x00;
        buf[nCnt++] = (char)0x00;

        //블록수(2)
        //읽고자 하는 블록의 개수. 0x0001
        buf[nCnt++] = (char)0x01;
        buf[nCnt++] = (char)0x00;

        //변수명 길이(2)
        //변수 명의 길이. 최대 16자.
        buf[nCnt++] = (char)szDevice.Length;
        buf[nCnt++] = (char)0x00;

        //데이터 주소
        //쓰고자 하는 Data, 최대 1400byte

        char[] ch = szDevice.ToCharArray();

        for (int i = 0; i < ch.Length; i++)
        {
            buf[nCnt++] = ch[i];
        }

        //데이터 개수(2)
        buf[nCnt++] = (char)(nLength & 0xFF);           // Data length(L)
        buf[nCnt++] = (char)(nLength >> 8);             // Data length(H)   


        switch (nLength)
        {
            case 1:
                {
                    buf[nCnt++] = (char)pData;
                    break;
                }
            case 2:
                {
                    buf[nCnt++] = (char)(pData & 0xFF);
                    buf[nCnt++] = (char)(pData >> 8);

                    break;
                }
            case 4:
                {
                    buf[nCnt++] = (char)(pData & 0xFF);
                    buf[nCnt++] = (char)(pData >> 8);

                    buf[nCnt++] = (char)(pData & 0xFFFF);
                    buf[nCnt++] = (char)(pData >> 16);

                    break;
                }
            case 8:
                {
                    buf[nCnt++] = (char)(pData & 0xFF);
                    buf[nCnt++] = (char)(pData >> 8);

                    buf[nCnt++] = (char)(pData & 0xFFFF);
                    buf[nCnt++] = (char)(pData >> 16);

                    buf[nCnt++] = (char)(pData & 0xFFFF);
                    buf[nCnt++] = (char)(pData >> 32);

                    buf[nCnt++] = (char)(pData & 0xFFFFFFFF);
                    buf[nCnt++] = (char)(pData >> 64);

                    break;
                }
        }


        Array.Resize(ref buf, nCnt);
        byte[] bytes = new byte[nCnt];
        for (int i = 0; i < nCnt; i++)
            bytes[i] = Convert.ToByte(buf[i]);

        if (_client.Connected)
            _client.SendBytes(bytes);
        //string strData = BytetoHex(bytes);
        //m_CmdList.Enqueue(buf);
    }

    private string BytetoHex(byte[] byteData)
    {
        StringBuilder sb = new StringBuilder(byteData.Length * 2);
        foreach (byte b in byteData)
            sb.AppendFormat("{0:X2}", b);

        return sb.ToString();
        //return strData;
    }
    private void RegisterWriteArray(string szDevice, params byte[] pData)
    {
        string[] array = Array.ConvertAll(pData, element => element.ToString());
        string txt = string.Format("{0}-->{1}", szDevice, string.Join(",", array));
        //this.DisplayMsg(txt);

        int nLength = 0, nCnt = 0;
        char[] buf = new char[1024];

        Array.Clear(buf, 0, buf.Length);

        // LSIS 고유번호(10)

        buf[nCnt++] = 'L';
        buf[nCnt++] = 'S';
        buf[nCnt++] = 'I';
        buf[nCnt++] = 'S';
        buf[nCnt++] = '-';
        buf[nCnt++] = 'X';
        buf[nCnt++] = 'G';
        buf[nCnt++] = 'T';

        buf[nCnt++] = (char)0x00;
        buf[nCnt++] = (char)0x00;

        // PLC 정보(2)
        buf[nCnt++] = (char)0x00;
        buf[nCnt++] = (char)0x00;

        // CPU 정보(2)
        buf[nCnt++] = (char)0xA0;

        //프레임 방향(1)
        buf[nCnt++] = (char)0x33;

        //프레임 순서번호(2)
        buf[nCnt++] = (char)0x00;
        buf[nCnt++] = (char)0x01;

        //길이(2)

        nLength = szDevice.Length - 4;

        if (nLength < 0)
        {
            nLength = 0;
        }

        nLength += 0x10;
        nLength += pData.Length;

        buf[nCnt++] = (char)(nLength & 0xFF);           // Data length(L)
        buf[nCnt++] = (char)(nLength >> 8);             // Data length(H)   

        //위치 정보(1)
        buf[nCnt++] = (char)0x00;

        //체크섬(1)
        buf[nCnt++] = (char)ByteCheckSum(buf, 0, nCnt - 1);

        //명령어(2)
        //[ h5400 읽기][ h5800 쓰기 ]

        buf[nCnt++] = (char)0x58;
        buf[nCnt++] = (char)0x00;

        //데이터 타입(2)
        //[ h00 비트 ][ h01 바이트 ][ h02 워드 ][ h03 더블워드 ][ h04 롱워드][ h14 연속]

        buf[nCnt++] = (char)0x14;
        buf[nCnt++] = (char)0x00;

        //예약 영역(2)
        //0x0000 : Don’t Care.
        buf[nCnt++] = (char)0x00;
        buf[nCnt++] = (char)0x00;

        //블록수(2)
        //읽고자 하는 블록의 개수. 0x0001
        buf[nCnt++] = (char)0x01;
        buf[nCnt++] = (char)0x00;

        //변수명 길이(2)
        //변수 명의 길이. 최대 16자.
        buf[nCnt++] = (char)szDevice.Length;
        buf[nCnt++] = (char)0x00;

        //데이터 주소
        //쓰고자 하는 Data, 최대 1400byte

        char[] ch = szDevice.ToCharArray();

        for (int i = 0; i < ch.Length; i++)
        {
            buf[nCnt++] = ch[i];
        }

        //데이터 개수(2)
        nLength = pData.Length;
        buf[nCnt++] = (char)(nLength & 0xFF);           // Data length(L)
        buf[nCnt++] = (char)(nLength >> 8);             // Data length(H)   

        for (int i = 0; i < pData.Length; i++)
        {
            int nData = pData[i];
            buf[nCnt++] = (char)pData[i];
        }

        Array.Resize(ref buf, nCnt);

        byte[] bytes = new byte[nCnt];

        for (int i = 0; i < nCnt; i++)
            bytes[i] = Convert.ToByte(buf[i]);
        //m_CmdList.Enqueue(buf);
    }

    private void RegisterGetWord(string szDevice)
    {

        int HCnt = 0, ICnt = 0;
        char[] buf = new char[255];
        char[] format_Header = new char[255];
        char[] format_Instruct = new char[255];

        Array.Clear(buf, 0, buf.Length);
        Array.Clear(format_Header, 0, format_Header.Length);
        Array.Clear(format_Instruct, 0, format_Instruct.Length);

        //명령어(1) Application Header Format
        // LSIS 고유번호(10)

        format_Header[HCnt++] = 'L';
        format_Header[HCnt++] = 'S';
        format_Header[HCnt++] = 'I';
        format_Header[HCnt++] = 'S';
        format_Header[HCnt++] = '-';
        format_Header[HCnt++] = 'X';
        format_Header[HCnt++] = 'G';
        format_Header[HCnt++] = 'T';

        format_Header[HCnt++] = (char)0x00;
        format_Header[HCnt++] = (char)0x00;

        // PLC 정보(2)
        format_Header[HCnt++] = (char)0x00;
        format_Header[HCnt++] = (char)0x00;

        // CPU 정보(2)
        format_Header[HCnt++] = (char)0xA0;

        //프레임 방향(1)
        format_Header[HCnt++] = (char)0x33;

        //프레임 순서번호(2)
        format_Header[HCnt++] = (char)0x00;
        format_Header[HCnt++] = (char)0x01;

        //길이(2)
        int nbyteLength = szDevice.Length - 4;

        if (nbyteLength < 0)
        {
            nbyteLength = 0;
        }

        nbyteLength += 0x10;

        format_Header[HCnt++] = (char)nbyteLength; //format_Header[16]
        format_Header[HCnt++] = (char)0x00;    //format_Header[17]

        //위치 정보(1)
        format_Header[HCnt++] = (char)0x00;

        //체크섬(1)
        format_Header[HCnt++] = (char)ByteCheckSum(format_Header, 0, HCnt - 1);

        /////////////////////////////////////////////////////////////////////////////

        //명령어(2)  //Application Instruction Format
        //[ h5400 읽기][ h5800 쓰기 ]

        format_Instruct[ICnt++] = (char)0x54;
        format_Instruct[ICnt++] = (char)0x00;

        //데이터 타입(2)
        //[ h00 비트 ][ h01 바이트 ][ h02 워드 ][ h03 더블워드 ][ h04 롱워드][ h14 연속]

        format_Instruct[ICnt++] = (char)0x14;
        format_Instruct[ICnt++] = (char)0x00;

        //예약 영역(2)
        //0x0000 : Don’t Care.
        format_Instruct[ICnt++] = (char)0x00;
        format_Instruct[ICnt++] = (char)0x00;

        //블록수(2)
        //읽고자 하는 블록의 개수. 0x0100
        format_Instruct[ICnt++] = (char)0x01;
        format_Instruct[ICnt++] = (char)0x00;

        //변수명 길이(2)
        //변수 명의 길이. 최대 16자.
        format_Instruct[ICnt++] = (char)szDevice.Length;
        format_Instruct[ICnt++] = (char)0x00;

        //데이터 주소
        //쓰고자 하는 Data, 최대 1400byte

        char[] ch = szDevice.ToCharArray();

        for (int i = 0; i < ch.Length; i++)
        {
            format_Instruct[ICnt++] = ch[i];
        }


        //데이터 개수(길이)(2)
        switch (szDevice.Substring(1, 1))
        {
            case "P":
            case "Z":
            case "S":
                {
                    //nbyteLength = 128*2;
                    nbyteLength = 256 * 2;
                    break;
                }
            default:
                {
                    nbyteLength = 256 * 2;
                    break;
                }
        }


        format_Instruct[ICnt++] = (char)(nbyteLength & 0xFF); //연속읽기 일 경우 Data length(L)
        format_Instruct[ICnt++] = (char)(nbyteLength >> 8);   //연속읽기 일 경우 Data length(H)   


        Array.Resize(ref format_Header, HCnt);
        Array.Resize(ref format_Instruct, ICnt);

        //Instruct 가변길이 적용
        format_Header[16] = (char)format_Instruct.Length; //format_Header[16]
        format_Header[17] = (char)0x00;                   //format_Header[17]

        for (int i = 0; i < HCnt; i++)
        {
            if (i < HCnt)
            {
                buf[i] = format_Header[i];
            }
        }
        for (int i = 0; i < ICnt; i++)
        {
            if (i < ICnt)
            {
                buf[20 + i] = format_Instruct[i];
            }
        }
        Array.Resize(ref buf, HCnt + ICnt);

        //m_CmdList.Enqueue(buf);
    }

    public void GrabComplete(int nSel)
    {
        int nIdx = nSel;

        try
        {
            if (!string.IsNullOrEmpty(_plcParam.strWriteTriggerStart))
            {
                //int nWriteTriggerStart = 0;
                int.TryParse(_plcParam.strWriteTriggerStart, out int nWriteTriggerStart);
                _listWriteData[nWriteTriggerStart + nIdx] = "1";
            }
        }
        catch { }
    }

    public void InspCompete(int nSel, bool bResult)
    {
        try
        {
            int nIdx = nSel;
            bool bRes = bResult;

            if (!string.IsNullOrEmpty(_plcParam.strWriteResStart) && !string.IsNullOrEmpty(_plcParam.strOKSignal) && !string.IsNullOrEmpty(_plcParam.strNGSignal))
            {
                string strRes = bRes ? _plcParam.strOKSignal : _plcParam.strNGSignal;

                //int nWriteResult = 0;
                int.TryParse(_plcParam.strWriteResStart, out int nWriteResult);
                _listWriteData[nWriteResult + nIdx] = strRes;
            }
        }
        catch { }
    }

    public void CamStatus(int nStatus)
    {
        int.TryParse(_plcParam.strWriteResStart, out int nWriteResult);
        _listWriteData[nWriteResult + 2] = nStatus.ToString();
    }

    private string SetPLCAddressToHex(string data)  // 데이터 변환  
    {
        string tmp = null;
        string _data = null;
        tmp = Convert.ToString(Convert.ToInt32(data), 16);

        if ((tmp.Length % 2) > 0)
            tmp = "0" + tmp;

        for (int i = tmp.Length; i > 0; i = i - 2)
            _data += tmp.Substring(i - 2, 2);

        return _data.PadRight(6, '0').ToUpper();
    }

    private byte[] HexStringToByteArray(string Hex)  // 데이터 변환  
    {
        byte[] Bytes = new byte[Hex.Length / 2];
        int[] HexValue = new int[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
                                 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x0B, 0x0C, 0x0D,
                                 0x0E, 0x0F };
        for (int x = 0, i = 0; i < Hex.Length; i += 2, x += 1)
        {
            Bytes[x] = (byte)(HexValue[Char.ToUpper(Hex[i + 0]) - '0'] << 4 |
                              HexValue[Char.ToUpper(Hex[i + 1]) - '0']);
        }
        return Bytes;
    }

    private static string ByteArrayToHexString(byte[] Bytes, int nStart, int nEnd)  // 데이터 변환  
    {
        string strData = "";

        for (int i = nStart; i < nEnd; i++)
        {
            if (Bytes[i] == 0)
                strData += "0";
            else
                strData += (char)Bytes[i];
        }
        return strData;
    }
}
