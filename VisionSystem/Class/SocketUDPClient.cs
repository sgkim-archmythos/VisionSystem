﻿using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Data;
using System.Collections.Generic;
using System.ComponentModel;

public class SocketUDPClient
{
    public class AsyncObject
    {
        public Byte[] Buffer;
        public Socket WorkingSocket;
        public AsyncObject(Int32 bufferSize)
        {
            this.Buffer = new Byte[bufferSize];
        }
    }

    const string m_strDis = "Disconnected";

    public delegate void DataReceivedHandlerFunc(int nIdx, byte[] byteRecive);
    public DataReceivedHandlerFunc OnReceiveData;

    public delegate void DisconnectFunc();
    public DisconnectFunc DisconnectHandler;

    public delegate void MessageFunc(string strMsg);
    public MessageFunc MessageHandler;

    public Boolean g_Connected = false;
    private Socket m_ClientSocket = null;
    private AsyncCallback m_fnReceiveHandler;
    private AsyncCallback m_fnSendHandler;
    public string m_strRead = string.Empty;
    public int m_nIdx = -1;


    public SocketUDPClient(int nIdx)
    {
        // 비동기 작업에 사용될 대리자를 초기화합니다.
        m_fnReceiveHandler = new AsyncCallback(handleDataReceive);
        m_fnSendHandler = new AsyncCallback(handleDataSend);

        m_nIdx = nIdx;
    }

    public Boolean Connected
    {
        get
        {
            return g_Connected;
        }
    }

    public int ConnectToServer(string hostName, int hostPort)
    {
        int nResult = 0;
        // TCP 통신을 위한 소켓을 생성합니다.
        m_ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        Boolean isConnected = false;
        try
        {
            //m_ClientSocket.Connect(hostName, hostPort);
            //// 연결 성공
            //isConnected = true;

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(hostName), hostPort);
            var result = m_ClientSocket.BeginConnect(ep, null, null);

            isConnected = true;
            // 연결 시도
            //m_ClientSocket.Connect(hostName, hostPort);
            //bool success = result.AsyncWaitHandle.WaitOne(Timeout, true);
            //if (success) { m_ClientSocket.EndConnect(result); }

            //if (success)
            //    // 연결 성공
            //    isConnected = true;
            //else
            //    isConnected = false;
        }
        catch
        {
            // 연결 실패 (연결 도중 오류가 발생함)
            isConnected = false;
            return -1;
        }
        g_Connected = isConnected;

        if (isConnected)
        {

            // 4096 바이트의 크기를 갖는 바이트 배열을 가진 AsyncObject 클래스 생성
            AsyncObject ao = new AsyncObject(4096);

            // 작업 중인 소켓을 저장하기 위해 sockClient 할당
            ao.WorkingSocket = m_ClientSocket;

            // 비동기적으로 들어오는 자료를 수신하기 위해 BeginReceive 메서드 사용!
            m_ClientSocket.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnReceiveHandler, ao);

            //Console.WriteLine("연결 성공!");

        }
        else
        {
            return -1;
        }
        return nResult;
    }

    public void StopClient()
    {
        // 가차없이 클라이언트 소켓을 닫습니다.
        if (m_ClientSocket != null)
            m_ClientSocket.Close();

        g_Connected = false;
    }

    public int SendMessage(byte[] strMsg, int nCount)
    {
        int nResult = 0;
        // 추가 정보를 넘기기 위한 변수 선언
        // 크기를 설정하는게 의미가 없습니다.
        // 왜냐하면 바로 밑의 코드에서 문자열을 유니코드 형으로 변환한 바이트 배열을 반환하기 때문에
        // 최소한의 크기르 배열을 초기화합니다.
        AsyncObject ao = new AsyncObject(1);

        // 문자열을 바이트 배열으로 변환
        //ao.Buffer = Encoding.Unicode.GetBytes(strMsg);

        ao.Buffer = strMsg;
        ao.WorkingSocket = m_ClientSocket;

        // 전송 시작!
        try
        {
            m_ClientSocket.BeginSend(ao.Buffer, 0, nCount, SocketFlags.None, m_fnSendHandler, ao);
        }
        catch
        {
            if (DisconnectHandler != null)
                DisconnectHandler();

            g_Connected = false;
            return -1;
        }
        return nResult;
    }

    private void handleDataReceive(IAsyncResult ar)
    {

        // 넘겨진 추가 정보를 가져옵니다.
        // AsyncState 속성의 자료형은 Object 형식이기 때문에 형 변환이 필요합니다~!
        AsyncObject ao = (AsyncObject)ar.AsyncState;

        // 받은 바이트 수 저장할 변수 선언
        Int32 recvBytes = 0;

        try
        {
            // 자료를 수신하고, 수신받은 바이트를 가져옵니다.
            recvBytes = ao.WorkingSocket.EndReceive(ar);
        }
        catch
        {

            if (DisconnectHandler != null)
                DisconnectHandler();

            g_Connected = false;

            return;
        }

        // 수신받은 자료의 크기가 1 이상일 때에만 자료 처리
        if (recvBytes > 0)
        {
            // 공백 문자들이 많이 발생할 수 있으므로, 받은 바이트 수 만큼 배열을 선언하고 복사한다.
            Byte[] msgByte = new Byte[recvBytes];
            Array.Copy(ao.Buffer, msgByte, recvBytes);

            //m_strRead = BytetoHex(msgByte);
            //m_strRead = Encoding.UTF8.GetString(msgByte);

            if (OnReceiveData != null)
                OnReceiveData(m_nIdx, msgByte);

            // 받은 메세지를 출력
            //Console.WriteLine("메세지 받음: {0}", Encoding.Unicode.GetString(msgByte));
        }

        try
        {
            // 자료 처리가 끝났으면~
            // 이제 다시 데이터를 수신받기 위해서 수신 대기를 해야 합니다.
            // Begin~~ 메서드를 이용해 비동기적으로 작업을 대기했다면
            // 반드시 대리자 함수에서 End~~ 메서드를 이용해 비동기 작업이 끝났다고 알려줘야 합니다!
            ao.WorkingSocket.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnReceiveHandler, ao);
        }
        catch
        {
            // 예외가 발생하면 예외 정보 출력 후 함수를 종료한다.
            //Console.WriteLine("자료 수신 대기 도중 오류 발생! 메세지: {0}", ex.Message);
            return;
        }
    }

    private string BytetoHex(byte[] byteData)
    {
        StringBuilder sb = new StringBuilder(byteData.Length * 2);
        foreach (byte b in byteData)
            sb.AppendFormat("{0:X2}", b);

        return sb.ToString();
        //return strData;
    }
    private void handleDataSend(IAsyncResult ar)
    {

        // 넘겨진 추가 정보를 가져옵니다.
        AsyncObject ao = (AsyncObject)ar.AsyncState;

        // 보낸 바이트 수를 저장할 변수 선언
        Int32 sentBytes;

        try
        {
            // 자료를 전송하고, 전송한 바이트를 가져옵니다.
            sentBytes = ao.WorkingSocket.EndSend(ar);
        }
        catch
        {
            // 예외가 발생하면 예외 정보 출력 후 함수를 종료한다.
            //Console.WriteLine("자료 송신 도중 오류 발생! 메세지: {0}", ex.Message);

            return;
        }

        if (sentBytes > 0)
        {
            // 여기도 마찬가지로 보낸 바이트 수 만큼 배열 선언 후 복사한다.
            Byte[] msgByte = new Byte[sentBytes];
            Array.Copy(ao.Buffer, msgByte, sentBytes);

            //Console.WriteLine("메세지 보냄: {0}", Encoding.Unicode.GetString(msgByte));
        }
    }

    public int SendMsg(string strMsg)
    {
        if (!g_Connected)
            return -1;

        int nResult = 0;
        // 추가 정보를 넘기기 위한 변수 선언
        // 크기를 설정하는게 의미가 없습니다.
        // 왜냐하면 바로 밑의 코드에서 문자열을 유니코드 형으로 변환한 바이트 배열을 반환하기 때문에
        // 최소한의 크기르 배열을 초기화합니다.
        AsyncObject ao = new AsyncObject(1);

        // 문자열을 바이트 배열으로 변환
        ao.Buffer = Encoding.UTF8.GetBytes(strMsg);
        //ao.Buffer = strMsg;

        ao.WorkingSocket = m_ClientSocket;

        // 전송 시작!
        try
        {
            m_ClientSocket.BeginSend(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnSendHandler, ao);

            if (MessageHandler != null)
                MessageHandler("Robot Send Data : " + strMsg.TrimEnd('\0'));
        }
        catch
        {
            return -1;
        }
        return nResult;
    }
}
