using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Basler.Pylon;
using System.Threading;
using System.Windows.Forms;


public class PylonCam
{
    private List<Camera> cameras = new List<Camera>();
    private List<ICameraInfo> camInfos = new List<ICameraInfo>();

    public int Init()
    {
        int num = 0;
        try
        {
            List<ICameraInfo> cameraInfoList = CameraFinder.Enumerate();
            this.camInfos = cameraInfoList;
            foreach (ICameraInfo cameraInfo in cameraInfoList)
            {
                ++num;
                string deviceIpAddress = CameraInfoKey.DeviceIpAddress;
                cameraInfo[deviceIpAddress].Split('.');
            }
        }
        catch (Exception ex)
        {
        }
        return num;
    }

    public void DeInit()
    {
        try
        {
            if (this.cameras != null)
            {
                foreach (Camera camera in this.cameras)
                {
                    camera.Close();
                    camera.Dispose();
                }
                this.cameras.Clear();
            }
            if (this.camInfos == null)
                return;
            this.camInfos.Clear();
        }
        catch (Exception ex)
        {
        }
    }

    public List<ICameraInfo> GetAllCamInfo() => CameraFinder.Enumerate();

    public List<Camera> Connect()
    {
        if (this.cameras != null)
        {
            foreach (Camera camera in this.cameras)
            {
                camera.Close();
                camera.Dispose();
            }
            this.cameras.Clear();
        }
        try
        {
            foreach (ICameraInfo camInfo in this.camInfos)
            {
                Camera camera = new Camera(camInfo);
                camera.CameraOpened += new EventHandler<EventArgs>(Configuration.AcquireContinuous);                
                camera.ConnectionLost += new EventHandler<EventArgs>(this.OnConnectionLost);
                this.cameras.Add(camera);
            }
        }
        catch (Exception ex)
        {
        }
        return this.cameras;
    }


    public List<Camera> Connect(ICameraInfo camInfo)
    {
        if (this.cameras != null)
        {
            foreach (Camera camera in this.cameras)
            {
                camera.Close();
                camera.Dispose();
            }
            this.cameras.Clear();
        }
        ICameraInfo cameraInfo = camInfo;
        try
        {
            Camera camera = new Camera(cameraInfo);
            camera.CameraOpened += new EventHandler<EventArgs>(Configuration.AcquireContinuous);
            camera.ConnectionLost += new EventHandler<EventArgs>(this.OnConnectionLost);
            this.cameras.Add(camera);
            camera.Open();
        }
        catch (Exception ex)
        {
        }
        return this.cameras;
    }

    public event PylonCam.ConnectionLostEventHandler ConnectionLostEvent;

    private void OnConnectionLost(object sender, EventArgs e)
    {
        if (this.ConnectionLostEvent == null)
            return;
        this.ConnectionLostEvent();
    }

    public void SetTrigger(Camera camera, bool bModeOn, int nTriggerSource)
    {
        if (bModeOn)
            camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);
        else
            camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.Off);

        if (nTriggerSource == 0)
            camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Software);
        else
            camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Line1);
    }

    public void SetWhiteBalnce(Camera camera, int nType)
    {
        if (nType == 0)
            camera.Parameters[PLCamera.BalanceWhiteAuto].SetValue(PLCamera.BalanceWhiteAuto.Off);
        else if (nType == 1)
            camera.Parameters[PLCamera.BalanceWhiteAuto].SetValue(PLCamera.BalanceWhiteAuto.Once);
        else
            camera.Parameters[PLCamera.BalanceWhiteAuto].SetValue(PLCamera.BalanceWhiteAuto.Continuous);
    }

    public string GetPixelFormat(Camera camera)
    {
        string strFormat = camera.Parameters[PLCamera.PixelFormat].GetValue();
        return strFormat;
    }

    public void SoftwareTrigger(Camera camera) => camera.Parameters[PLCamera.TriggerSoftware].Execute();

    public void SetAOI(Camera camera, long lWidth, long lHeight)
    {
        camera.Parameters[PLCamera.Width].SetValue(lWidth);
        //Thread.Sleep(100);
        camera.Parameters[PLCamera.Height].SetValue(lHeight);
    }

    public void GetAOI(Camera camera, out long lWidth, out long lHeight)
    {
        lWidth = camera.Parameters[PLCamera.Width].GetValue();
        Thread.Sleep(100);
        lHeight = camera.Parameters[PLCamera.Height].GetValue();
    }


    public void TestImgOff(Camera camera)
    {
        camera.Parameters[PLCamera.TestImageSelector].SetValue("Test Image Off");
    }

    public void SetFrameRate(Camera camera, double dFps)
    {
        camera.Parameters[PLCamera.AcquisitionFrameRateEnable].SetValue(true);
        //Thread.Sleep(100);
        camera.Parameters[PLCamera.AcquisitionFrameRateAbs].SetValue(dFps);
    }

    public void SetTimeout(Camera camera, long lTimeout)
    {
        camera.Parameters[PLTransportLayer.HeartbeatTimeout].SetValue(lTimeout);
        //Thread.Sleep(100);
    }

    public long GetTimeout(Camera camera)
    {
        long lTimeout = 0;
        lTimeout =  camera.Parameters[PLTransportLayer.HeartbeatTimeout].GetValue();
        Thread.Sleep(100);

        return lTimeout;
    }

    public void SetGain(Camera camera, long lGain)
    {
        camera.Parameters[PLCamera.GainRaw].SetValue(lGain);
        //Thread.Sleep(100);
    }

    public double GetGain(Camera camera)
    {
        double dGain = 0;
        dGain = camera.Parameters[PLCamera.GainRaw].GetValue();
        //dGain = camera.Parameters[PLCamera.GainAbs].GetValue();
        Thread.Sleep(100);

        return dGain;
    }

    public void GetGainLimit(Camera camera, out double dMin, out double dMax)
    {
        dMin = camera.Parameters[PLCamera.AutoGainRawLowerLimit].GetValue();
        dMax = camera.Parameters[PLCamera.AutoGainRawUpperLimit].GetValue();
    }

    public void SetBright(Camera camera, double dBright)
    {
        camera.Parameters[PLCamera.Gamma].SetValue(dBright);
       // Thread.Sleep(100);
    }

    public double GetBright(Camera camera)
    {
        double dBright = 0;
        dBright = camera.Parameters[PLCamera.Gamma].GetValue();
        Thread.Sleep(100);
        return dBright;
    }

    public void GetGammaLimit(Camera camera, out double dMin, out double dMax)
    {
        dMin = camera.Parameters[PLCamera.Gamma].GetMinimum();
        dMax = camera.Parameters[PLCamera.Gamma].GetMaximum();
    }

    public string GetAddress(Camera camera)
    {
        string strAddress = camera.CameraInfo["Address"];
        Thread.Sleep(100);
        return strAddress;
    }

    public string GetSerial(Camera camera)
    {
        string strSerialNo = camera.CameraInfo["SerialNumber"];
        Thread.Sleep(100);
        return strSerialNo;
    }

    public void SetExposure(Camera camera, double dExp)
    {
        camera.Parameters[PLCamera.ExposureTimeAbs].SetValue(dExp);
    }

    public double GetExpose(Camera camera)
    {
        double dExpose = 0;
        dExpose = camera.Parameters[PLCamera.ExposureTimeAbs].GetValue();
        Thread.Sleep(100);
        return dExpose;
    }

    public void GetExposeLimit(Camera camera, out double dMin, out double dMax)
    {
        dMin = camera.Parameters[PLCamera.AutoExposureTimeAbsLowerLimit].GetValue();
        dMax = camera.Parameters[PLCamera.AutoExposureTimeAbsUpperLimit].GetValue();
    }

    public delegate void ConnectionLostEventHandler();
}
