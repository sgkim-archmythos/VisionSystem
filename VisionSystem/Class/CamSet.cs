using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cognex.VisionPro;
using Basler.Pylon;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;
using System.Drawing;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;
using System.IO;

using Cognex.VisionPro.FGGigE;

public delegate void GrabHandler(int nIdx, Bitmap bmpImg,  ICogImage cogGrab);
public delegate void MessageHanlder(string strMsg, Color color, bool bShowMsg, bool bError, int msgType);
public class CamSet
{
    [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
    public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

    public GrabHandler _GrabFunc;
    public MessageHanlder _MessageFunc;

    public ICogAcqFifo _AcqFifo = null;
    public Camera _cams = null;
    public PylonCam _cam = new PylonCam();
    public MyCamera m_MyCamera = null;

    MyCamera.MV_FRAME_OUT_INFO_EX m_stFrameInfo;

    UInt32 m_nBufSizeForDriver = 0;
    IntPtr m_BufForDriver = IntPtr.Zero;
    private static Object BufForDriverLock = new Object();

    MyCamera.MV_CC_DEVICE_INFO device;

    //public int _nCamCnt = 0;
    public bool _bConnect = false;

    public bool _bGrabThread = false;
    bool _bGrab = false;
    public int _nCamType = -1;
    public int _nIdx = -1;
    public bool _bCamCopy = false;
    bool _bLive = false;
    double _dExpose = 0;
    //public int _nColorType = 0;
    //GlovalVar.CamPram[] _camParam;
    
    public string CamConnect(int nSel,  GlovalVar.CamPram[] camParam, CogFrameGrabbers Grabber, List<Camera> camera, MyCamera.MV_CC_DEVICE_INFO_LIST m_stDeviceList, GlovalVar.ModelParam modelParam)
    {
        string strRes = "";
        int nidx = 0;
        try
        {
            if (camParam == null)
                strRes = "No camera connection information";
            else
            {
                //_camParam = camParam;
                if (camParam[nSel].strCamType == "")
                    strRes = "No camera connection information";
                else
                {
                    if (int.Parse(camParam[nSel].strCamType) == (int)GlovalVar.CameraInterface.Cognex)
                    {
                        if(string.IsNullOrEmpty(camParam[nSel].strCopy) || camParam[nSel].strCopy == "-1")
                        {
                            int nFormat = int.Parse(camParam[nSel].strCamFormat);

                            if (_bConnect)                                      //연결시 연결 해제 
                            {
                                if (_AcqFifo != null)
                                    _AcqFifo.FrameGrabber.Disconnect(true);

                                //camParam[nSel].bConnect = false;
                                _bConnect = false;
                            }
                            else
                            {
                                for (int i = 0; i < Grabber.Count; i++)
                                {
                                    if (Grabber[i].SerialNumber == camParam[nSel].strCamSerial)
                                    {
                                        _AcqFifo = Grabber[i].CreateAcqFifo(Grabber[i].AvailableVideoFormats[nFormat], (CogAcqFifoPixelFormatConstants)0, 0, true);
                                        _AcqFifo.Timeout = 1000;
                                        _AcqFifo.TimeoutEnabled = true;
                                        break;
                                    }
                                    else
                                        nidx++;
                                }
                            }
                            try
                            {
                                if (_cams != null)
                                {
                                    if (_cams.IsConnected)
                                    {
                                        if (_cams.CameraInfo["Serialnumber"] == camParam[nSel].strCamSerial)
                                        {
                                            camera[_nIdx].Close();
                                            _cams.Close();
                                        }
                                    }
                                }
                                
                            }
                            catch (Exception ex) { }

                            try
                            {
                                if (m_MyCamera != null)
                                {
                                    if (m_MyCamera.MV_CC_IsDeviceConnected_NET())
                                    {
                                        m_MyCamera.MV_CC_CloseDevice_NET();
                                        m_MyCamera = null;
                                    }
                                }
                            }
                            catch (Exception ex) { }
                           
                           
                            if (!Grabber[nidx].AvailableVideoFormats[nFormat].ToString().ToLower().ToString().Contains("mono"))
                            {
                                _AcqFifo.OwnedCustomPropertiesParams.CustomPropsAsString = "Write\tBalanceWhiteAuto\tOnce\n\r";
                                _AcqFifo.OwnedCustomPropertiesParams.CustomPropsAsString = "Write\tUserSetSelector\tUserSet1\n\rWrite\tUserSetDefaultSelector\tUserSet1\n\rCommand\tUserSetSave\n\r";
                            }                           
                        }

                        
                        _bConnect = true;

                        //double dExpose = (camParam[nSel].dExpose == 0) ? camParam[nSel].dExpose : modelParam.dExpose;
                        double dExpose = 10;

                        _AcqFifo.OwnedExposureParams.Exposure = dExpose;
                        _AcqFifo.Prepare();

                        _AcqFifo.OwnedCustomPropertiesParams.CustomPropsAsString = "";

                        _nCamType = int.Parse(camParam[nSel].strCamType);
                        _bGrabThread = false;
                        Thread.Sleep(50);
                        _bGrabThread = true;
                        Thread threadGrab = new Thread(GrabThread);
                        threadGrab.Start();
                    }
                    else if (int.Parse(camParam[nSel].strCamType) == (int)GlovalVar.CameraInterface.Basler)
                    {
                        if (string.IsNullOrEmpty(camParam[nSel].strCopy) || camParam[nSel].strCopy == "-1")
                        {
                            if (_bConnect)
                            {
                                _cams.StreamGrabber.Stop();
                                _cams = null;
                            }

                            _cams = null;
                            nidx = int.Parse(camParam[nSel].strCamSerial);

                            try
                            {
                                if (Grabber.Count > 0)
                                {
                                    if (Grabber[nidx].SerialNumber.Contains(camParam[nSel].strCamSerial))
                                        Grabber[nidx].Disconnect(true);
                                }
                            }
                            catch { }

                            try
                            {
                                if (m_MyCamera != null)
                                {
                                    if (m_MyCamera.MV_CC_IsDeviceConnected_NET())
                                    {
                                        m_MyCamera.MV_CC_CloseDevice_NET();
                                        m_MyCamera = null;
                                    }
                                }
                            }
                            catch (Exception ex) { }

                            _cams = camera[nidx];
                            _cams.Open();

                            _cam.GetAOI(_cams, out long lWidth, out long lHeight);
                            _cam.SetAOI(_cams, lWidth, lHeight);
                            _cam.SetTrigger(_cams, true, 0);

                            if (camParam[nSel].nTimeout == 0)
                                _cam.SetTimeout(_cams, 1000);
                            else
                                _cam.SetTimeout(_cams, camParam[nSel].nTimeout);

                            double dExpose = (camParam[nSel].dExpose == 0) ? camParam[nSel].dExpose : modelParam.dExpose;

                            string strPixelformat = _cam.GetPixelFormat(_cams).ToLower();
                            if (strPixelformat.Contains("color"))
                                _cam.SetWhiteBalnce(_cams, 1); // 0 : off 1: once 2: continous  blance white

                            _cam.SetExposure(_cams, dExpose);
                            _cams.StreamGrabber.Start();
                            _cams.StreamGrabber.GrabResultWaitHandle.WaitOne(0);
                        }

                        _bConnect = true;
                        _nCamType = int.Parse(camParam[nSel].strCamType);
                        _bGrabThread = false;
                        Thread.Sleep(50);
                        _bGrabThread = true;
                        Thread threadGrab = new Thread(GrabThread);
                        threadGrab.Start();
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(camParam[nSel].strCopy) || camParam[nSel].strCopy == "-1")
                        {
                            try
                            {
                                if (Grabber.Count > 0)
                                {
                                    if (Grabber[nSel].SerialNumber.Contains(camParam[nSel].strCamSerial))
                                        Grabber[nSel].Disconnect(true);
                                }
                            }
                            catch { }

                            try
                            {
                                if (Grabber[nSel].SerialNumber.Contains(_cams.CameraInfo["SerialNumber"]))
                                {
                                    if (_cams != null)
                                    {
                                        if (_cams.IsConnected)
                                        {
                                            camera[nSel].Close();
                                            _cams.Close();
                                        }
                                    }
                                }
                            }
                            catch (Exception ex) { }

                            device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[nSel], typeof(MyCamera.MV_CC_DEVICE_INFO));

                            if (null == m_MyCamera)
                            {
                                m_MyCamera = new MyCamera();
                                if (null == m_MyCamera)
                                {
                                    return "Hik Cam Error";
                                }
                            }

                            int nRet = m_MyCamera.MV_CC_CreateDevice_NET(ref device);
                            if (MyCamera.MV_OK != nRet)
                            {
                                return "Device Create fail!";
                            }

                            nRet = m_MyCamera.MV_CC_OpenDevice_NET();
                            if (MyCamera.MV_OK != nRet)
                            {
                                m_MyCamera.MV_CC_DestroyDevice_NET();
                                //ShowErrorMsg("Device open fail!", nRet);
                                return "Device open fail!";
                            }

                            m_MyCamera.MV_CC_SetHeartBeatTimeout_NET(3000);

                            if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                            {
                                int nPacketSize = m_MyCamera.MV_CC_GetOptimalPacketSize_NET();
                                if (nPacketSize > 0)
                                {
                                    nRet = m_MyCamera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                                    if (nRet != MyCamera.MV_OK)
                                    {
                                        return "Set Packet Size failed!";
                                        //ShowErrorMsg("Set Packet Size failed!", nRet);
                                    }
                                }
                                else
                                {
                                    return "Get Packet Size failed!";
                                    //ShowErrorMsg("Get Packet Size failed!", nPacketSize);
                                }
                            }

                            m_MyCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", (uint)MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
                            m_MyCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
                            m_MyCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
                            
                            double dExpose = (camParam[nSel].dExpose == 0) ? camParam[nSel].dExpose : modelParam.dExpose;

                            m_MyCamera.MV_CC_SetHeartBeatTimeout_NET(1000);
                            m_MyCamera.MV_CC_SetEnumValue_NET("ExposureAuto", 0);
                            nRet = m_MyCamera.MV_CC_SetFloatValue_NET("ExposureTime", (float)dExpose);
                            if (nRet != MyCamera.MV_OK)
                            {
                                return "Set Exposure Time Fail!";
                                //ShowErrorMsg("Set Exposure Time Fail!", nRet);
                            }
                            

                            nRet = m_MyCamera.MV_CC_StartGrabbing_NET();
                            //m_MyCamera.MV_CC_GetBalanceWhiteAuto_NET();MyCamera.MV_CAM_BALANCEWHITE_AUTO.MV_BALANCEWHITE_AUTO_ONCE

                            if (nRet != MyCamera.MV_OK)
                            {
                                return "Start Grabbing Fail!";
                                //ShowErrorMsg("Set Gain Fail!", nRet);
                            }
                        }

                        _bConnect = true;
                        _nCamType = int.Parse(camParam[nSel].strCamType);
                        _bGrabThread = false;
                        Thread.Sleep(50);
                        _bGrabThread = true;
                        Thread threadGrab = new Thread(GrabThread);
                        threadGrab.Start();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            strRes = "Camera Connect Error : " + ex.Message;
        }

        return strRes;
    }

    private bool IsMonoPixelFormat(MyCamera.MvGvspPixelType enType)
    {
        switch (enType)
        {
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                return true;
            default:
                return false;
        }
    }

    private bool IsColorPixelFormat(MyCamera.MvGvspPixelType enType)
    {
        switch (enType)
        {
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BGR8_Packed:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_Packed:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_YUYV_Packed:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR8:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG8:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB8:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG8:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10_Packed:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10_Packed:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10_Packed:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10_Packed:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12_Packed:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12_Packed:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12_Packed:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12:
            case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12_Packed:
                return true;
            default:
                return false;
        }
    }

    public double[] GetCamParam()
    {
        double[] dValue = new double[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        try
        {
            if (_nCamType == (int)GlovalVar.CameraInterface.Cognex)
            {
                dValue[0] = _AcqFifo.OwnedExposureParams.Exposure;
                dValue[1] = _AcqFifo.OwnedBrightnessParams.Brightness;
                dValue[2] = _AcqFifo.OwnedContrastParams.Contrast;
            }
            else if (_nCamType == (int)GlovalVar.CameraInterface.Basler)
            {
                dValue[0] = _cam.GetExpose(_cams);
                dValue[1] = _cam.GetBright(_cams);
                dValue[2] = _cam.GetGain(_cams);

                _cam.GetExposeLimit(_cams, out dValue[3], out dValue[4]);
                _cam.GetGammaLimit(_cams, out dValue[5], out dValue[6]);
                _cam.GetGainLimit(_cams, out dValue[7], out dValue[8]);
            }
            else
            {
                MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
                int nRet = m_MyCamera.MV_CC_GetFloatValue_NET("ExposureTime", ref stParam);
                if (MyCamera.MV_OK == nRet)
                {
                    dValue[0] = stParam.fCurValue;
                    //tbExposure.Text = stParam.fCurValue.ToString("F1");
                }

                nRet = m_MyCamera.MV_CC_GetFloatValue_NET("Gain", ref stParam);
                if (MyCamera.MV_OK == nRet)
                {
                    dValue[1] = stParam.fCurValue;
                }
            }
        }
        catch { }

        return dValue;
    }


    public void SetCamParam(int nType, double dValue)
    {
        try
        {
            if (_nCamType == (int)GlovalVar.CameraInterface.Cognex)
            {
                if (nType == (int)GlovalVar.CamParamType.Exposure)
                {
                    if (dValue == 0)
                        _AcqFifo.OwnedExposureParams.Exposure = 10.0;
                    else
                        _AcqFifo.OwnedExposureParams.Exposure = dValue;
                }
                else if (nType == (int)GlovalVar.CamParamType.Brightness)
                    _AcqFifo.OwnedBrightnessParams.Brightness = dValue;
                else
                    _AcqFifo.OwnedContrastParams.Contrast = dValue;

                _AcqFifo.Prepare();
            }
            else if (_nCamType == (int)GlovalVar.CameraInterface.Basler)
            {
                if (nType == (int)GlovalVar.CamParamType.Exposure)
                {
                    if (dValue == 0)
                        _cam.SetExposure(_cams, 10000);
                    else
                        _cam.SetExposure(_cams, dValue);
                }
                else if (nType == (int)GlovalVar.CamParamType.Exposure)
                    _cam.SetBright(_cams, dValue);
                else
                    _cam.SetGain(_cams, (long)dValue);
            }
            else
            {
                m_MyCamera.MV_CC_SetEnumValue_NET("ExposureAuto", 0);
                int nRet = -1;

                if (dValue == 0)
                    nRet = m_MyCamera.MV_CC_SetFloatValue_NET("ExposureTime", 10000);
                else
                    nRet = m_MyCamera.MV_CC_SetFloatValue_NET("ExposureTime", (float)dValue);
            }
        }
        catch { }
    }

    public void CamDisconnect()
    {
        _bGrabThread = false;

        Thread.Sleep(100);

        try
        {
            if (_nCamType == (int)GlovalVar.CameraInterface.Basler)
            {
                if (_cams != null)
                {
                    _cams.StreamGrabber.Stop();
                    _cams.Close();
                }
            }
            else if (_nCamType == (int)GlovalVar.CameraInterface.Cognex)
            {
                if (!_bCamCopy)
                {
                    if (_AcqFifo != null)
                    {
                        _AcqFifo.FrameGrabber.Disconnect(false);
                        _AcqFifo = null;
                    }
                }
            }
            else
            {
                if (m_MyCamera != null)
                {
                    m_MyCamera.MV_CC_CloseDevice_NET();
                    m_MyCamera.MV_CC_DestroyDevice_NET();
                }
            }
        }
        catch { }

        _bConnect = false;
    }

    private void GrabThread()
    {
        int ntrignum = 0;
        ICogImage cogGrab = null;
        Bitmap bmpImg = null;
        int nWidth = 0, nHeight = 0;
        int nType = 0;
        int nRet = 0;
        var nCnt = 0;

        PixelDataConverter converter = null;
        //MyCamera.MV_FRAME_OUT stFrameInfo;

        while (true)
        {
            nType = _nCamType;

            if (nType == (int)GlovalVar.CameraInterface.Cognex)
            {
                if (!_bGrabThread)
                    return;

                if (_bGrab)
                {
                    _bGrab = false;
                    ntrignum = 0;
                    try
                    {
                        cogGrab = _AcqFifo.Acquire(out ntrignum);
                        _GrabFunc(_nIdx, null, cogGrab);

                        nCnt++;

                        if (nCnt > 4)
                        {
                            _AcqFifo.Flush();
                            nCnt = 0;
                        }
                        //_GrabFunc(_nIdx, cogGrab);
                    }
                    catch (Exception ex) {
                    }
                }

                Thread.Sleep(1);
            }
            else if (nType == (int)GlovalVar.CameraInterface.Basler)
            {
                if (converter == null)
                    converter = new PixelDataConverter();

                try
                {
                    IGrabResult grabResult = _cams.StreamGrabber.RetrieveResult(System.Threading.Timeout.Infinite, TimeoutHandling.ThrowException);

                    if (!_bGrabThread)
                        return;

                    using (grabResult)
                    {
                        if (grabResult == null)
                            continue;

                        if (grabResult.GrabSucceeded)
                        {
                            nWidth = grabResult.Width;
                            nHeight = grabResult.Height;

                            using (Bitmap bitmap = new Bitmap(nWidth, nHeight, PixelFormat.Format32bppRgb))
                            {
                                System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);
                                converter.OutputPixelFormat = PixelType.BGRA8packed;

                                IntPtr ptrBmp = bmpData.Scan0;
                                converter.Convert(ptrBmp, bmpData.Stride * bitmap.Height, grabResult);
                                bitmap.UnlockBits(bmpData);

                                bmpImg = (Bitmap)bitmap.Clone();
                            }

                            if (_GrabFunc != null)
                                _GrabFunc(_nIdx, bmpImg, null);
                        }
                    }
                }
                catch { }

                if (_bLive)
                    _cam.SoftwareTrigger(_cams);

                Thread.Sleep(1);
            }
            else
            {
                var stFrameInfo = new MyCamera.MV_FRAME_OUT();
                nRet = m_MyCamera.MV_CC_GetImageBuffer_NET(ref stFrameInfo, 1000);

                if (!_bGrabThread)
                    return;

                if (nRet != MyCamera.MV_OK)
                    continue;

                lock (BufForDriverLock)
                {
                    if (m_BufForDriver == IntPtr.Zero || stFrameInfo.stFrameInfo.nFrameLen > m_nBufSizeForDriver)
                    {
                        if (m_BufForDriver != IntPtr.Zero)
                        {
                            Marshal.Release(m_BufForDriver);
                            m_BufForDriver = IntPtr.Zero;
                        }

                        m_BufForDriver = Marshal.AllocHGlobal((Int32)stFrameInfo.stFrameInfo.nFrameLen);
                        if (m_BufForDriver == IntPtr.Zero)
                        {
                            return;
                        }
                        m_nBufSizeForDriver = stFrameInfo.stFrameInfo.nFrameLen;
                    }

                    m_stFrameInfo = stFrameInfo.stFrameInfo;
                    CopyMemory(m_BufForDriver, stFrameInfo.pBufAddr, stFrameInfo.stFrameInfo.nFrameLen);                    
                }

                if (RemoveCustomPixelFormats(stFrameInfo.stFrameInfo.enPixelType))
                {
                    m_MyCamera.MV_CC_FreeImageBuffer_NET(ref stFrameInfo);
                    continue;
                }

                if (stFrameInfo.stFrameInfo.enPixelType.ToString().ToLower().Contains("mono"))
                {
                    using (Bitmap bitmap = new Bitmap(stFrameInfo.stFrameInfo.nWidth, stFrameInfo.stFrameInfo.nHeight, stFrameInfo.stFrameInfo.nWidth, PixelFormat.Format8bppIndexed, stFrameInfo.pBufAddr))
                    {
                        ColorPalette cp = bitmap.Palette;

                        for (int i = 0; i < 256; i++)
                        {
                            cp.Entries[i] = Color.FromArgb(i, i, i);
                        }

                        bitmap.Palette = cp;
                        bmpImg = (Bitmap)bitmap.Clone();
                    }
                }
                else
                {
                    using (Bitmap bitmap = new Bitmap(stFrameInfo.stFrameInfo.nWidth, stFrameInfo.stFrameInfo.nHeight, stFrameInfo.stFrameInfo.nWidth * 3, PixelFormat.Format24bppRgb, stFrameInfo.pBufAddr))
                    {
                        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, stFrameInfo.stFrameInfo.nWidth, stFrameInfo.stFrameInfo.nHeight), ImageLockMode.WriteOnly, bitmap.PixelFormat);
                        int nLen = Math.Abs(data.Stride) * bitmap.Height;

                        unsafe
                        {
                            byte* value = (byte*)data.Scan0.ToPointer();

                            try
                            {
                                for (int px = 0; px < nLen; px += 3)
                                {
                                    byte temp = value[px];
                                    value[px] = value[px + 2];
                                    value[px + 2] = temp;
                                }
                            }
                            catch { }
                        }

                        bitmap.UnlockBits(data);
                        bmpImg = (Bitmap)bitmap.Clone();
                    }
                }

                if (_GrabFunc != null)
                    _GrabFunc(_nIdx, bmpImg, null);

                m_MyCamera.MV_CC_FreeImageBuffer_NET(ref stFrameInfo);

                Thread.Sleep(1);
            }
        }
    }

    private bool RemoveCustomPixelFormats(MyCamera.MvGvspPixelType enPixelFormat)
    {
        Int32 nResult = ((int)enPixelFormat) & (unchecked((Int32)0x80000000));
        if (0x80000000 == nResult)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public void Grab(int nIdx, double dExpo)
    {
        if (_bConnect)
        {
            _nIdx = nIdx;
            var dExpose = dExpo;
            try
            {
                SetCamParam((int)GlovalVar.CamParamType.Exposure, dExpose);

                if (_nCamType == (int)GlovalVar.CameraInterface.Cognex)
                    _bGrab = true;
                else if (_nCamType == (int)GlovalVar.CameraInterface.Basler)
                    _cam.SoftwareTrigger(_cams);
                else
                    m_MyCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
            }
            catch (Exception ex) {
                _MessageFunc(string.Format("{0} camera Grab Err> :" + ex.ToString(), _nIdx + 1), Color.Red,false, false, 0);
            }
        }
    }

    public void LiveView(int nIdx, CogRecordDisplay cogDisp, bool bLive)
    {
        if (_bConnect)
        {
            _nIdx = nIdx;
            if (_nCamType == (int)GlovalVar.CameraInterface.Cognex)
            {
                if (bLive)
                {
                    cogDisp.StartLiveDisplay(_AcqFifo);
                    cogDisp.AutoFit = true;
                }
                else
                    cogDisp.StopLiveDisplay();
            }
            else if (_nCamType == (int)GlovalVar.CameraInterface.Basler)
            {
                if (bLive)
                {
                    _bLive = true;
                    _cam.SoftwareTrigger(_cams);
                }
                else
                {
                    _bLive = false;
                }
            }
            else
            {
                if (bLive)
                {
                    m_MyCamera.MV_CC_StopGrabbing_NET();
                    m_MyCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
                    m_MyCamera.MV_CC_StartGrabbing_NET();
                }
                else
                {
                    m_MyCamera.MV_CC_StopGrabbing_NET();
                    m_MyCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
                    m_MyCamera.MV_CC_StartGrabbing_NET();
                }
            }
        }
    }
}
