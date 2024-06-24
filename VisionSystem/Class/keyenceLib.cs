using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Cognex.VisionPro;

using Keyence.IX.Sdk;
using Keyence.IX.Sdk.Tools;
using Keyence.IX.Sdk.Types;

public class keyenceLib
{
    public delegate void SensorInfoHandler(string strDeviceName, string strHeadModelName, string strHeadVersion, string strHeadSerial, string strAmpModelName, string strAmpVersion, string strAmpSerial);
    public SensorInfoHandler OnSensorInfo;

    public delegate void ProgramSetHandler(string strProgramNo, string strProgramName, string strMode, string strExternalTrigger, string strExternalTriggerTime, Bitmap MasterImg);
    public ProgramSetHandler OnProgramSet;

    public delegate void ProgramListHandler(List<string> listProgram);
    public ProgramListHandler OnProgramList;

    public delegate void ThresholdVauleHandler(double dHigh, double dLow);
    public ThresholdVauleHandler OnThresholdValue;

    public delegate void DataHandler(string strRes, string strValue, ICogImage bmpImg);
    public DataHandler OnData;

    private Sensor _sensor = null;
    private IDisposable _dataChangeEventHandler;

    //Bitmap _bmpMasterImg = null;
    public bool Connect(string strIP, string strPort)
    {
        try
        {
            _sensor = SensorFactory.ConnectSensor(new IPEndPoint(IPAddress.Parse(strIP), Convert.ToInt32(strPort)));
            _dataChangeEventHandler = _sensor.Subscribe(DataChangeEventHandler);

            GetSensonInfo();
            GetProgramList();
            GetProgramSet();

            return true;
        }
        catch 
        {
            return false;
        }
    }

    public double[] GetThresholdValue(int nIdx)
    {
        if (!_sensor.Connected) return null;
        if (_sensor.ProgramDescription.Tools.Length == 0) return null;

        ThresholdValue thresholdValue = _sensor.ProgramDescription.Tools[nIdx].Threshold;

        double[] dValue = new double[2];
        dValue[0] = (double)thresholdValue.Height.HighThreshold;
        dValue[1] = (double)thresholdValue.Height.LowThreshold;

        return dValue;
    }

    public void SetThresholdValue(int nIdx, double dHigh, double dLow)
    {
        if (!_sensor.Connected) return;
        if (_sensor.ProgramDescription.Tools.Length == 0) return;

        ThresholdValue thresholdValue = _sensor.ProgramDescription.Tools[nIdx].Threshold;

        _sensor.ProgramDescription.Tools[nIdx].Threshold.Height.HighThreshold = (decimal)dHigh;
        _sensor.ProgramDescription.Tools[nIdx].Threshold.Height.HighThreshold = (decimal)dLow;
    }

    public string[] GetToolList()
    {
        if (!_sensor.Connected) return null;
        if (_sensor.ProgramDescription.Tools.Length == 0) return null;

        var strToolList = new string[_sensor.ProgramDescription.Tools.Length];

        for (var i = 0; i < _sensor.ProgramDescription.Tools.Length; i++)
            strToolList[i] = _sensor.ProgramDescription.Tools[i].ToolName;

        return strToolList;
    }

    private void GetProgramList()
    {
        if (!_sensor.Connected) return;

        var listProgram = new List<string>();

        foreach (ProgramHeader program in _sensor.ProgramHeaders)
            listProgram.Add(program.ToString());

        if (OnProgramList != null)
            OnProgramList(listProgram);
    }

    private void GetSensonInfo()
    {
        if (!_sensor.Connected) return;

        if (OnSensorInfo != null)
            OnSensorInfo(_sensor.DeviceName, _sensor.HeadModelName, _sensor.HeadVersion, _sensor.HeadSerialNo, _sensor.AmpModelName, _sensor.AmpVersion, _sensor.AmpSerialNo);
    }

    private void GetProgramSet()
    {
        if (!_sensor.Connected) return;

        ProgramHeader programHeader = _sensor.ProgramHeaders[_sensor.ActiveProgramNo];
        ScanDetectionSetting setting = _sensor.ProgramDescription.ScanDetectionSetting;

        string strProgramNo = _sensor.ActiveProgramNo.ToString();
        string strProgramName = programHeader.ProgramName;
        string strMode = (programHeader.DetectionMode == DetectionMode.Scan) ? "Scan Mode" : "Line Mode";
        string strExternalTrigger = setting.ExternalTrigger.ToString();
        string strExternalTriggerTime = setting.InternalTriggerInterval.ToString();
        Bitmap bmpMasterImg = (Bitmap)programHeader.CreateMasterThumbnailImage().Clone();

        if (OnProgramSet != null)
            OnProgramSet(strProgramNo, strProgramName, strMode, strExternalTrigger, strExternalTriggerTime, bmpMasterImg);

    }

    public void TriggerOn()
    {
        if (!_sensor.Connected) return;

        _sensor.InputScanTrigger();
    }

    public void Disconnect()
    {
        if (_sensor == null) return;

        if (!_sensor.Connected) return;

        if (_sensor != null)
        {
            _sensor.Dispose();
            _sensor = null;
        }
    }

    public bool isConnected
    {
        get { return _sensor.Connected; }
    }

    public void ZeroOffset(string strType)
    {
        if (!_sensor.Connected) return;

        if (strType == "Zero")
            _sensor.ExecuteZeroOffset(_sensor.RunningInfoSetting.ToolIndex);
        else
            _sensor.ClearZeroOffset(_sensor.RunningInfoSetting.ToolIndex);
    }

    public void ChangeProdgram(int nIdx)
    {
        try
        {
            if (_sensor != null)
            {
                if (!_sensor.Connected) return;

                _sensor.SwitchProgramNo(nIdx);
                GetProgramSet();
            }
        }
        catch { }
    }

    private void DataChangeEventHandler(object sender, DataChangedEventArgs dataChangedEventArgs)
    {
        try
        {
            if (dataChangedEventArgs.ChangedDataTypes.HasFlag(ChangedDataTypes.ProgramDescription))
            {
                GetProgramSet();
            }

            if (dataChangedEventArgs.ChangedDataTypes.HasFlag(ChangedDataTypes.RunningInfo))
            {
                GetData(dataChangedEventArgs);
            }

            if (dataChangedEventArgs.ChangedDataTypes.HasFlag(ChangedDataTypes.SensorErrors))
            {
                //errorStatusDisplayControl.SensorErrorStatusUpdated(dataChangedEventArgs);
            }
        }
        catch (Exception ex)
        {
            
            //MessageBox.Show(ex.ToString());
        }
    }


    private void GetData(DataChangedEventArgs dataChangedEventArgs)
    {
        if (!_sensor.Connected) return;

        if (dataChangedEventArgs.RunningInfo.Tools.Length == 0) return;

        var strRes = "";
        var strValue = "";

        for (var i = 0; i < dataChangedEventArgs.RunningInfo.Tools.Length; i++)
        {
            ToolResult toolResult = dataChangedEventArgs.RunningInfo.Tools[i];

            if (toolResult.ToolType == ToolType.Step)
            {
                if (toolResult.Result == ToolJudgeResult.Ok)
                    strRes += "OK" + ",";
                else
                    strRes += "NG" + ",";

                strValue += toolResult.ResultValue.Height.Value.ToString() + ",";
            }
        }

        ICogImage cogImg = new CogImage24PlanarColor(dataChangedEventArgs.RunningInfo.CreateImage());
        //Bitmap bmpImg = dataChangedEventArgs.RunningInfo.CreateImage();

        if (OnData != null)
            OnData(strRes, strValue, cogImg);
    }
}
