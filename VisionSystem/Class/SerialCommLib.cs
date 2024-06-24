using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class SerialComm
{
    public delegate void DataReceivedHandlerFunc(byte[] receiveData);
    public DataReceivedHandlerFunc DataReceivedHandler;

    public delegate void DisconnectedHandlerFunc();
    public DisconnectedHandlerFunc DisconnectedHandler;

    public delegate void MessageFunc(string strMsg);
    public MessageFunc MessageHandler;

    private SerialPort serialPort;
    private int m_nIdx = -1;

    public bool IsOpen
    {
        get
        {
            if (serialPort != null) return serialPort.IsOpen;
            return false;
        }
    }

    // serial port check
    private Thread threadCheckSerialOpen;
    private bool isThreadCheckSerialOpen = false;

    public SerialComm(int nIdx)
    {
        m_nIdx = nIdx;
    }

    public bool OpenComm(string portName, int baudrate, int databits, string stopbits, string parity, string handshake)
    {
        try
        {
            serialPort = new SerialPort();

            serialPort.PortName = portName;
            serialPort.BaudRate = baudrate;
            serialPort.DataBits = databits;
            serialPort.StopBits = (StopBits)System.Enum.Parse(typeof(StopBits), stopbits);
            serialPort.Parity = (Parity)System.Enum.Parse(typeof(Parity), parity); ;
            serialPort.Handshake = (Handshake)System.Enum.Parse(typeof(Handshake), handshake); ;

            //serialPort.Encoding = new System.Text.ASCIIEncoding();
            serialPort.NewLine = "\r\n";
            serialPort.ErrorReceived += serialPort_ErrorReceived;
            serialPort.DataReceived += serialPort_DataReceived;

            serialPort.Open();

            StartCheckSerialOpenThread();
            return true;
        }
        catch (Exception ex)
        {
            //Debug.WriteLine(ex.ToString());
            MessageHandler(ex.ToString());
            return false;
        }
    }

    public void CloseComm()
    {
        try
        {
            if (serialPort != null)
            {
                StopCheckSerialOpenThread();
                serialPort.Close();
                serialPort = null;
            }
        }
        catch (Exception ex)
        {
            //Debug.WriteLine(ex.ToString());
            MessageHandler(ex.ToString());
        }
    }

    public bool Send(string sendData)
    {
        try
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Write(sendData);
                return true;
            }
        }
        catch (Exception ex)
        {
            //Debug.WriteLine(ex.ToString());
            MessageHandler(ex.ToString());
        }
        return false;
    }

    public bool Send(byte[] sendData)
    {
        try
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Write(sendData, 0, sendData.Length);
                return true;
            }
        }
        catch (Exception ex)
        {
            //Debug.WriteLine(ex.ToString());
            MessageHandler(ex.ToString());
        }
        return false;
    }

    public bool Send(byte[] sendData, int offset, int count)
    {
        try
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Write(sendData, offset, count);
                return true;
            }
        }
        catch (Exception ex)
        {
            //Debug.WriteLine(ex.ToString());
            MessageHandler(ex.ToString());
        }
        return false;
    }

    private byte[] ReadSerialByteData()
    {
        serialPort.ReadTimeout = 100;
        byte[] bytesBuffer = new byte[serialPort.BytesToRead];
        int bufferOffset = 0;
        int bytesToRead = serialPort.BytesToRead;

        while (bytesToRead > 0)
        {
            try
            {
                int readBytes = serialPort.Read(bytesBuffer, bufferOffset, bytesToRead - bufferOffset);
                bytesToRead -= readBytes;
                bufferOffset += readBytes;
            }
            catch (TimeoutException ex)
            {
                //Debug.WriteLine(ex.ToString());
                MessageHandler(ex.ToString());
            }
        }

        return bytesBuffer;
    }

    private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            byte[] bytesBuffer = ReadSerialByteData();
            string strBuffer = Encoding.ASCII.GetString(bytesBuffer);

            if (DataReceivedHandler != null)
                DataReceivedHandler(bytesBuffer);

            //Debug.WriteLine("received(" + strBuffer.Length + ") : " + strBuffer);
        }
        catch (Exception ex)
        {
            //Debug.WriteLine(ex.ToString());
            MessageHandler(ex.ToString());
        }
    }

    private void serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
    {
        //Debug.WriteLine(e.ToString());
        MessageHandler(e.ToString());
    }

    private void StartCheckSerialOpenThread()
    {
        isThreadCheckSerialOpen = true;
        threadCheckSerialOpen = new Thread(new ThreadStart(ThreadCheckSerialOpen));
        threadCheckSerialOpen.Start();
    }

    private void StopCheckSerialOpenThread()
    {
        if (isThreadCheckSerialOpen)
        {
            isThreadCheckSerialOpen = false;
            if (Thread.CurrentThread != threadCheckSerialOpen)
                threadCheckSerialOpen.Join();
        }
    }

    private void ThreadCheckSerialOpen()
    {
        while (isThreadCheckSerialOpen)
        {
            Thread.Sleep(100);

            try
            {
                if (serialPort == null || !serialPort.IsOpen)
                {
                    //Debug.WriteLine("seriaport disconnected");
                    if (DisconnectedHandler != null)
                        DisconnectedHandler();
                    break;
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.ToString());
                MessageHandler(ex.ToString());
            }
        }
    }
}
