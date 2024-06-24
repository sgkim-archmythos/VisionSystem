using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Threading;
using System.Diagnostics;


public delegate void MXConnectHandler(string strMsg);
public delegate void MXDisconnectHandler(string strMsg);
public delegate void MXTriggerHandler(int nIdx);
public delegate void MXModelChangeHandler(string strModel);
public delegate void MXLotIDHandler(string strLotID);
public delegate void MXMessagehandler(string strMsg, Color color, string strMsgType);
public delegate void MXRecvData(string strMsg, byte[] bytes, ushort[] usData);
public class MXplc
{
    TCPComponents.TCPSimpleClient _client = new TCPComponents.TCPSimpleClient();
    //bool _bConnect = false;

    public MXConnectHandler _OnConnect;
    public MXDisconnectHandler _OnDisconnect;
    public MXTriggerHandler _OnTrigger;
    public MXModelChangeHandler _OnModelChange;
    public MXLotIDHandler _OnLotIDReceive;
    public MXMessagehandler _OnMessage;
    public MXRecvData _OnRecvData;

    public List<string> _listWriteData = new List<string>();

    bool _bRead = false;
    byte[] _bytes = null;
    string _strRectData = "";

    public GlovalVar.PLCPram _plcParam;

    public void LoadSet()
    {
        this._client.OnConnect += new TCPComponents.TCPSimpleClient.OnConnectDelegate(this.OnConnect);
        this._client.OnDisconnect += new TCPComponents.TCPSimpleClient.OnDisconnectDelegate(this.OnDisconnect);
        this._client.OnDataAvailable += new TCPComponents.TCPSimpleClient.OnDataAvailableDelegate(this.OnDataAvailable);
    }

    private void OnConnect(object sender)
    {
        if (_OnConnect != null)
            _OnConnect("MX PLC Connected");
    }
    private void OnDisconnect(object sender)
    {
        if (_OnDisconnect != null)
            _OnDisconnect("MX PLC Disconnected");
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

                for (int i = 0; i < 50; i++)
                    _listWriteData.Add("0");

                _bRead = true;
                Thread threadRead = new Thread(PLCRead);
                threadRead.Start();
            }
        }
        catch (Exception ex)
        {
            if (_OnMessage != null)
                _OnMessage(ex.Message, Color.Red, "Alarm");
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
        bool bheartBitStatus = false;
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

        string strStarSignal = "";
        string strTemp = "";

        List<string> listWrite = new List<string>();

        for (int i = 0; i < _listWriteData.Count; i++)
            listWrite.Add("0");

        // int nStart = 0;
        ushort[] usData = null;
        int lByteOffset = 0;

        sw.Start();
        while (true)
        {
            if (!_bRead)
                return;

            try
            {
                if (_client.Connected)
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

                            if (bheartBitStatus)
                            {
                                bheartBitStatus = false;
                                WriteResultDataBinary(strHeartbeartAddr, 0, "1");
                            }
                            else
                            {
                                bheartBitStatus = true;
                                WriteResultDataBinary(strHeartbeartAddr, 0, "2");
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(_plcParam.strReadStartAddr) && !string.IsNullOrEmpty(_plcParam.strReadLength))
                    {
                        strReadStartAddr = _plcParam.strReadStartAddr;
                        int.TryParse(_plcParam.strReadLength, out nLen);

                        if (nLen == 0)
                            nLen = 50;

                        bytes = null;
                        //tpye Select
                        SendBinary(strReadStartAddr, 12, nLen);
                        Thread.Sleep(30);
                        bytes = _bytes;
                        if (bytes != null)
                        {
                            strRead = "";
                            usData = null;
                            usData = new ushort[nLen / 2];
                            for (int i=0; i< nLen/2; i++)
                            {
                                lByteOffset = i * 2;
                                if(_plcParam.bReadSwap)
                                    usData[i] = MAKEWORD(bytes[lByteOffset + 11], bytes[lByteOffset+12]);
                                else
                                    usData[i] = MAKEWORD(bytes[lByteOffset + 12], bytes[lByteOffset + 11]);

                                if (bytes[11 + (i * 2)] == 0)
                                    strRead += " ";
                                else
                                    strRead += Convert.ToChar(bytes[11 + (i * 2)]);

                                if (bytes[11 + (i * 2) + 1] == 0)
                                    strRead += " ";
                                else
                                    strRead += Convert.ToChar(bytes[11 + (i * 2) + 1]);
                            }

                            if (_strRectData != strRead)
                            {
                                _strRectData = strRead;

                                if (_OnRecvData != null)
                                    _OnRecvData(_strRectData, bytes, usData);

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
                                        if (strRead.Substring(nReadTriggerStart * 2 + (i * 2), 2).Trim() == strStarSignal)
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
                                        strTemp += strRead.Substring(nReadModelStart * 2 + (i * 2), 2).Trim();

                                    if (strTemp != strModel)
                                    {
                                        if (_OnModelChange != null)
                                            _OnModelChange(strTemp);

                                        strModel = strTemp;
                                    }
                                }

                                if (nReadLotStart != -1 && nReadLotEnd != -1)
                                {
                                    strTemp = "";
                                    for (int i = 0; i < nReadLotEnd; i++)
                                        strTemp += strRead.Substring(nReadLotStart * 2 + (i * 2), 2).Trim();

                                    if (strTemp != strSerial)
                                    {
                                        if (_OnLotIDReceive != null)
                                            _OnLotIDReceive(strTemp);

                                        strSerial = strTemp;
                                    }
                                }
                            }
                        }

                        strWriteStartAddr = _plcParam.strWriteStartAddr;
                        for (int i = 0; i < listWrite.Count; i++)
                        {
                            if (listWrite[i] != _listWriteData[i])
                            {
                                if (_listWriteData[i] != "")
                                {
                                    listWrite[i] = _listWriteData[i];
                                    _listWriteData[i] = "";

                                    bytes = null;
                                    //tpye Select
                                    bytes = WriteResultDataBinary(strWriteStartAddr, i, listWrite[i]);

                                    if (bytes != null)
                                        _client.SendBytes(bytes);
                                }
                                else
                                    listWrite[i] = _listWriteData[i];
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

    private static UInt16 MAKEWORD(byte low, byte high)
    {
        return (UInt16)((high << 8) | low);
    }

    public void GrabComplete(int nSel)
    {
        int nIdx = nSel;

        try
        {
            if (!string.IsNullOrEmpty(_plcParam.strWriteTriggerStart))
            {
                int nWriteTriggerStart = 0;
                int.TryParse(_plcParam.strWriteTriggerStart, out nWriteTriggerStart);
                _listWriteData[nWriteTriggerStart + nIdx] = "1";
            }
        }
        catch { }
    }

    public void InspCompete(int nSel, string[] strData, bool bResult, GlovalVar.ModelParam modelParam,GlovalVar Var)
    {
        try
        {
            int nIdx = nSel;
            string[] strValue = strData;
            bool bRes = true;

            if (!string.IsNullOrEmpty(_plcParam.strWriteResStart) && !string.IsNullOrEmpty(_plcParam.strOKSignal) && !string.IsNullOrEmpty(_plcParam.strNGSignal))
            {
                string strRes = bRes ? _plcParam.strOKSignal : _plcParam.strNGSignal;

                int nWriteResult = 0;
                int.TryParse(_plcParam.strWriteResStart, out nWriteResult);
                _listWriteData[nWriteResult + nIdx] = strRes;
            }
        }
        catch { }
    }

    public void TotalResult(bool bResult)
    {
        bool bRes = bResult;
    }


    private void SendBinary(string requireAddress, int nCnt, int nLen)  // 데이터 요구 신호  
    {
        string strDataCNT = "";
        int nCN = nCnt;
        string strSendData = "";
        string strAddress = "";
        string strTempMemType = "";
        string strDataCount = "";
        byte[] byteSendData = null;

        try
        {
            if (requireAddress == "")
                return;
            //요구 데이타 길이 - 감시타이머 부터 맨 끝단의 비트수
            //nCN = 12;
            //요구 데이타 길이 형식 맞춤
            strDataCNT = (Convert.ToString(nCN, 16).PadLeft(2, '0').ToUpper()).PadRight(4, '0');
            // PLC 메모리 Type
            strTempMemType = Regex.Replace(requireAddress, @"\d", "");    // 읽기 레지스트 : D
            switch (strTempMemType)
            {
                //특수 레지스터
                case "SD":
                    strTempMemType = "A9";
                    break;

                //데이터 레지스터
                case "D":
                    strTempMemType = "A8";
                    break;

                //링크 레지스터
                case "W":
                    strTempMemType = "B4";
                    break;

                //인덱스 레지스터
                case "Z":
                    strTempMemType = "CC";
                    break;

                //파일 레지스터
                case "R":
                    strTempMemType = "AF";
                    break;

                //파일 레지스터
                case "ZR":
                    strTempMemType = "B0";
                    break;
            }
            // PLC 메모리 Type이 한자리인 경우 * 추가
            if (strTempMemType.Length == 1)
                strTempMemType += "*";
            // PLC 영역 주소
            strAddress = Regex.Replace(requireAddress, @"\D", "");   // 읽기 레지스트 : D
            strAddress = SetPLCAddressToHex(strAddress);
            // Data Count - 수신 데이타 크기 16진수로 변환
            //strDataCount = "5000";  //iReadBufferLength.ToString().PadRight(4, '0'); 
            strDataCount = string.Format("{0:X2}", nLen).PadRight(4,'0');  //iReadBufferLength.ToString().PadRight(4, '0'); 
                                    //받는 데이터길이 기존 2000

            // Send Data 조합 50 0 0 FF FF 3 0 길이 0 0A 0 1 14 0 0 0 1 0 A8 
            strSendData = "5000" +                          //서브헤더
                                 "00" +                              //네트워크 번호
                                 "FF" +                              //PLC 번호
                                 "FF03" +                          //요구 상대 모듈
                                 "00" +                              //요구 상대 국번
                                 strDataCNT +                //요구 데이터 길이
                                 "0A00" +                         //감시 타이머
                                 "0104" +                          // 커맨드
                                 "0000" +                          // 서브 커맨드
                                 strAddress +                 // 선두 디바이스
                                 strTempMemType +       // 메모리 영역 타입
                                 strDataCount;               // 읽을 데이타 갯수 - HEX


            byteSendData = HexStringToByteArray(strSendData);
            //byte[] bytes = Encoding.UTF8.GetBytes(strSendData);
            _client.SendBytes(byteSendData);
        }
        catch
        { }
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

    private string PLCDataDecode(byte[] bytes, int nLen)    // PLC 수신 : 미쯔비시 MC 프로토콜  
    {
        string strData = "";
        byte[] data = bytes;                                                                       // PLC 데이터 수신
        if (data == null)
            return strData;

        strData = ByteArrayToHexString(data, 11, nLen);
        return strData;
    }

    private static string ByteArrayToHexString(byte[] Bytes, int nStart, int nEnd)  // 데이터 변환  
    {
        string strData = "";

        for (int i=nStart; i<nEnd; i++)
        {
            if (Bytes[i] == 0)
                strData += "0";
            else
                strData += (char)Bytes[i];
        }
        return strData;
    }

    private byte[] WriteResultDataBinary(string plcaddress, int nIdx, string strData)   // 결과 전송  
    {
        string strDataCNT = "";
        int nCN = 0;
        string strSendData = "";
        string strAddress = "";
        string strTempMemType = "";
        string strDataCount = "";
        byte[] byteSendData = null;

        try
        {
            strSendData = GetFormatCode(strData);

            //요구 데이타 길이 - 감시타이머 부터 맨 끝단의 비트수
            nCN = 12 + (strSendData.ToString().Length / 2);
            //요구 데이타 길이 형식 맞춤
            strDataCNT = (Convert.ToString(nCN, 16).PadLeft(2, '0').ToUpper()).PadRight(4, '0');

            // PLC 메모리 Type
            strTempMemType = Regex.Replace(plcaddress, @"\d", "");
            switch (strTempMemType)
            {
                //특수 레지스터
                case "SD":
                    strTempMemType = "A9";
                    break;

                //데이터 레지스터
                case "D":
                    strTempMemType = "A8";
                    break;

                //링크 레지스터
                case "W":
                    strTempMemType = "B4";
                    break;

                //인덱스 레지스터
                case "Z":
                    strTempMemType = "CC";
                    break;

                //파일 레지스터
                case "R":
                    strTempMemType = "AF";
                    break;

                //파일 레지스터
                case "ZR":
                    strTempMemType = "B0";
                    break;
            }
            // PLC 메모리 Type이 한자리인 경우 * 추가
            if (strTempMemType.Length == 1)
                strTempMemType += "*";
            // PLC 영역 주소
            strAddress = Regex.Replace(plcaddress, @"\D", "");   // 읽기 레지스트 : D
            int.TryParse(strAddress, out int nValue);
            nValue += nIdx;
            // PLC 영역 주소 변환
            strAddress = SetPLCAddressToHex(nValue.ToString());

            //형식에 따라 전송량 전환
            int nCount = strSendData.ToString().Length / 4;
            int nQ = nCount / 0xFF;
            int nR = nCount % 0xFF;
            nCount = (nR << 8) + nQ;
            strDataCount = Convert.ToString(nCount, 16);
            for (int i = strDataCount.Length; i < 4; i++)
                strDataCount = strDataCount.PadLeft(4, '0');

            // Send Data 조합
            strSendData = "5000" +                            // 서브헤더
                                  "00" +                               // 네트워크 번호
                                  "FF" +                               // PLC 번호
                                  "FF03" +                           // 요구 상대 모듈
                                  "00" +                              // 요구 상대 국번
                                  strDataCNT +                    // 요구 데이터 길이
                                  "0A00" +                          // 감시 타이머
                                  "0114" +                           // 쓰기 커맨드
                                  "0000" +                           // 서브 커맨드
                                  strAddress +                     // 선두 디바이스
                                  strTempMemType +           // 메모리 영역 타입
                                  strDataCount +                  // 써넣을 데이터 갯수 - HEX
                                  strSendData;                      // 전송 Data;

            byteSendData = HexStringToByteArray(strSendData);
            _client.SendBytes(byteSendData);
            //_PLC.SendMessage(byteSendData);
        }
        catch
        { }

        return byteSendData;
    }

    private string GetFormatCode(string strData)  // 문자열 변환  
    {
        try
        {
            string strTempData = null;

            if (strData.Length % 2 != 0)
                strData += " ";

            int length = strData.Length;

            // 현재 문자열을 16진수로 전환 "1" -> dec "8241" / hex "2031"
            for (int i = 0; i < length; i = i + 2)
            {
                strTempData += TransHex(strData.Substring(i, 2));

            }
            if (_plcParam.bWriteSwap)
                strTempData = strTempData.Substring(2, 2) + strTempData.Substring(0, 2);
            
            return strTempData;
        }
        catch
        {
            return null;
        }
    }

    private string TransHex(string strData)  // 문자열 변환  
    {
        string strResultData = null;
        try
        {
            byte[] curVal = Encoding.ASCII.GetBytes(strData);

            foreach (byte TrnVal in curVal)
                strResultData += String.Format("{0:0x}", Convert.ToString(TrnVal, 16));

            return strResultData;
        }
        catch
        {
            return null;
        }
    }
}
