using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

public class GlovalVar
{
    public string _strConfigPath = Application.StartupPath + "\\Config";
    public string _strLogPath = Application.StartupPath + "\\Log";
    public string _strCameraInfoPath = Application.StartupPath + "\\CameraInfo";
    public string _strCommInfoPath = Application.StartupPath + "\\Communication";
    public string _strMasterImagePath = Application.StartupPath + "\\MasterImage";
    public string _strModelPath = Application.StartupPath + "\\Model";

    public string _strAdminPW = "0000";
    public string _strOPPW = "9999";
    public string _tsReset = "";
    public string _AutoDeleteTime1 = "";
    public string _AutoDeleteTime2 = "";

    public struct CamPram  //카메라 파라미터
    {
        public string strCamType;
        public string strCamSerial;
        public string strCamFormat;
        public string strIP;
        public double dExpose;
        public double dBright;
        public double dContract;
        public int nTimeout;
        public bool bTiime;
        public string strCopy;
    }

    public struct ModelParam  //카메라 파라미터
    {
        public int nInspCnt;
        public int nGrabdelay;
        public double dExpose;
        public string strCode;
        public int nDefectCnt;
        public string strExposeInc;
        public int nAlign;
        public int nFontSize;
        public int nPosX;
        public int nPosY;
        public bool bDefectInsp;
        public bool bBCRInsp;
        public bool bBCRInspSwap;
        public bool bAlingInsp;
        public string strBCRData;
        public string strBCRLen;
        public bool bPinChange;
        public int nPinCnt;
        public double dAlignMasterX;
        public double dAlignMasterY;
        public double dAlignMasterR;
        public double dAlignOffsetX;
        public double dAlignOffsetY;
        public double dAlignOffsetR;
        public double dResoluton;
        public string strPinMaster;
        public string strPinProhibit;
        public bool bXreversal;
        public bool bYreversal;
        public bool bZreversal;
        public bool bXYreversal;
    }

    public struct PLCPram  //통신 파라미터
    {
        public int nCommType;
        public string strPLCIP;
        public string strPLCPort;
        public short shRack;
        public short shSlot;
        public string strHeartBeatAddr;
        public int nHearBeatInterval;
        public string strStartSignal;
        public string strOKSignal;
        public string strNGSignal;
        public int nReadInterval;
        public string strReadStartAddr;
        public string strReadLength;
        public string strReadStartTrigger;
        public string strReadEndTrigger;
        public string strReadModelStart;
        public string strReadModelEnd;
        public string strReadLotStart;
        public string strReadLotEnd;
        public string strReadCamPassStart;
        public string strReadCamPassEnd;
        public string strWriteStartAddr;
        public string strWriteTriggerStart;
        public string strWriteModelStart;
        public string strWriteLotStart;
        public string strWriteResStart;
        public string strWriteTotalRes;
        public string strWrite2DRes;
        public string strWrite2DData;
        public string strWrite2DLen;
        public string strWriteAlignX;
        public string strWriteAlignY;
        public string strWriteAlignZ;
        public string strWritePinChange;
        public string strWritePinProhibit;
        public bool bIndividualTrigger;
        public string strWriteCamPointRes;
        public string strIndividualTrigger;
        public string[] strIndividualData;
        public bool bReadSwap;
        public bool bWriteSwap;
        public bool bUseSpecialFunction;
        public bool bCommunicationType;                  //ASCII & Binary
    }

    public struct GraphicParam
    {
        public bool bUse;
        public int nColor;
        public int nColor2;
        public int nLineThick;
        public int nPosX;
        public int nPosY;
    }

    public struct GraphicResParam
    {
        public int nAlign;
        public int nFontSize;
        public int nWidth;
        public int nHeight;
        public bool bCamUse;
        public int nFont;
        public int nPosY;
        public int n2DWidth;
        public int n2DHeight;
    }

    public enum GraphicAlign
    {
        Left,
        Center,
        Right
    }

    public enum GraphicColor
    {
        Green,
        Yellow,
        Blue,
        Cyan,
        Magenta,
        Red
    }

    public enum PLCType
    {
        MX,
        Simens,
        IO,
        LS
    }

    public enum PositionType
    {
        Previous,
        Next
    }

    public enum AdminMode
    {
        Model,
        Communication,
        Camera
    }

    public enum PasswordType
    {
        Admin,
        Operator,
        All
    }

    public enum CameraInterface
    {
        Cognex,
        Basler,
        Hik
    }

    public enum CamParamType
    {
        Exposure,
        Brightness,
        Constrast
    }

    public List<string> _strlistLocation = new List<string>();
    //1920,1080 1936, 1056
    //public int _nWidth = 1922;
    //public int _nHeight = 939;

    //1280 1024, 1296, 1000,
    public int _nWidth = 1282;
    public int _nHeight = 883;

    public string _strWidth = "";
    public string _strHeight = "";

    public int _nScreenCnt = 0;

    public string _strModelNo = "";
    public string _strLotNo = "";
    public string _strVppNo = "";
    public string _strPassData = "";

    public int _nTotalCnt = 0;
    public int _nOKCnt = 0;
    public int _nNGCnt = 0;

    public List<string> _listModel = new List<string>();
    public List<string> _listCode = new List<string>();
    public List<string> _listCodeComment = new List<string>();

    public string _strModelName = "";

    public bool _bOriginImageSave;
    public int _nOriginImageFormat;
    public bool _bResultImageSave;
    public int _nResultImageFormat;
    public bool _bAutoImageDelete;
    public int _nDiskOriginalDelete;
    public int _nDiskResultDelete;
    public int _nDiskAutoDelete;
    public string _strSaveImagePath;
    public string _strSaveResultImagePath;
    public string _strAddfunction;

    public bool _bUsePrint = false;
    public bool _bUseSensor = false;
    public bool _bUsePinchange = false;

    public static bool _License = false;
    

    public struct RobotParam
    {
        public string strIP;
        public string strPort;
        public string strTrigger1;
        public string strTrigger2;
    }

    public struct LightParam
    {
        public string strPortName;
        public string strBaudrate;
        public string strChannel1;
        public string strChannel2;
        public string strLightValue1;
        public string strLightValue2;
    }


    public enum Language
    {
        English,
        Korean
    }
    public struct PrintParam
    {
        public string strIP;
        public string strPort;
        public string strOriginalX;
        public string strOriginalY;
        public string str1DX;
        public string str1DY;
        public string str1DWidth;
        public string str1DHeight;
        public string strSerialX;
        public string strSerialY;
        public string strSerialWidth;
        public string strSerialHeight;
        public string str2DX;
        public string str2DY;
        public string str2DSize;
        public string strMIPX;
        public string strMIPY;
        public string strMIPWidth;
        public string strMIPHeight;
        public int iCount;
        public string strHMCData;
    }

    public struct PrintModelParam
    {
        public string strMIPData;
        public string strCount;
    }

    public struct SensorParam
    {
        public string strIP;
        public string strPort;
        public double dLimitLow;
        public double dLimitHigh;
        public int nTriggerCycleTime;
        public int nModelNo;
    }
    public void LogData(string Data)
    {

    }

    public Language _Language = Language.Korean;
}
