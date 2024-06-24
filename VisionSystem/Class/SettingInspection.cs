using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ToolGroup;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.Dimensioning;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.Blob;
using Cognex.VisionPro.ColorMatch;
using Cognex.VisionPro.ColorExtractor;
using Cognex.VisionPro.ColorSegmenter;
using Cognex.VisionPro.OCRMax;
using Cognex.VisionPro.CNLSearch;
using Cognex.VisionPro.ResultsAnalysis;
using System.Windows.Forms;
using System.Diagnostics;
using Cognex.VisionPro.ID;
using Cognex.VisionPro.OCRMax;
using Cognex.VisionPro.OCVMax;



public delegate void InspectionSettingGrahphicHandler(List<CogPointMarker> cogPoint, List<CogLine> cogLine, List<CogCircle> cogCircle, List<CogCompositeShape> cogShape, List<CogEllipse> cogEllipse, List<ICogRegion> cogRegion, List<CogCompositeShape> cogBlobShape, string[] strData);
public delegate void JobSettingLoadHandler(string strRes);
public delegate void SettingMessageHandler(string strMsg, Color color, bool bShoPopup, bool bError, string Type);
public delegate void InspectionSettingRegionGraphicHandler(ICogGraphic[] _Region);
public class SettingInspection
{
    public InspectionSettingGrahphicHandler _OnSettingInspGraphic;
    public InspectionSettingRegionGraphicHandler _OnSettingInspRegionGraphic;
    public JobSettingLoadHandler _OnSettingJobLoad;
    public SettingMessageHandler _OnSettingMessage;

    private CogToolGroup _job = new CogToolGroup();
    private CogToolGroup _toolGroup = new CogToolGroup();
    private CogImageConvertTool[] _cogImgConvertTool = new CogImageConvertTool[2];
    private CogIPOneImageTool _cogIPOimgTool = null;
    private CogDataAnalysisTool _DataAnalysisTool = null;
    private CogIDTool _cogIDTool = null;
    private CogOCRMaxTool _cogOCRMaxTool = null;
    private CogOCVMaxTool _cogOCVMaxTool = null;

    public string _strPath = "";
    public string _strJobName = "";

    // int i = 0;
    GlovalVar.GraphicParam[] _graphic;
    GlovalVar.ModelParam _Model;


    public int GetJobCount
    {
        get { return _job.Tools.Count; }
    }

    public bool VJoblode(string strJobPath, string strJobName)
    {
        bool bRes = true;
        try
        {
            _strPath = strJobPath;
            _strJobName = strJobName;
            _job = CogSerializer.LoadObjectFromFile(strJobPath) as CogToolGroup;

            if (_job != null)
            {
                _job.AbortRunOnToolFailure = false;
                _job.GarbageCollectionEnabled = true;
                _job.GarbageCollectionFrequency = 15;
                int iCount = 0;
                for (int i = 0; i < _job.Tools.Count; i++)
                {
                    if (_job.Tools[i].GetType() == typeof(CogImageConvertTool))
                    {
                        _cogImgConvertTool[iCount] = _job.Tools[i] as CogImageConvertTool;
                        iCount++;
                    }


                    if (_job.Tools[i].GetType() == typeof(CogDataAnalysisTool))
                        _DataAnalysisTool = _job.Tools[i] as CogDataAnalysisTool;
                }
            }

            if (_OnSettingJobLoad != null)
                _OnSettingJobLoad("");

        }
        catch (Exception ex)
        {
            bRes = false;

            if (_OnSettingJobLoad != null)
                _OnSettingJobLoad(ex.Message);
        }

        return bRes;

    }

    public void FileLoad(string strFile)
    {
        _job = CogSerializer.LoadObjectFromFile(strFile) as CogToolGroup;

        if (_job != null)
        {
            _job.AbortRunOnToolFailure = false;
            _job.GarbageCollectionEnabled = true;
            _job.GarbageCollectionFrequency = 15;
            int iCount = 0;
            for (int i = 0; i < _job.Tools.Count; i++)
            {
                if (_job.Tools[i].GetType() == typeof(CogImageConvertTool))
                {
                    _cogImgConvertTool[iCount] = _job.Tools[i] as CogImageConvertTool;
                    iCount++;
                }
                if (_job.Tools[i].GetType() == typeof(CogDataAnalysisTool))
                    _DataAnalysisTool = _job.Tools[i] as CogDataAnalysisTool;
            }
        }
    }

    public CogToolGroup GetJob
    {
        get { return _job; }
        set { _job = value; }
    }

    public void VJobSave()
    {
        CogSerializer.SaveObjectToFile(_job, _strPath, typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter), CogSerializationOptionsConstants.Minimum);
    }

    public void VJobrun(int nSel, ICogImage cogGrab, GlovalVar.GraphicParam[] graphicParam, GlovalVar.ModelParam ModelParam)
    {
        _graphic = graphicParam;
        _Model = ModelParam;
        try
        {
            ICogImage cogImg = null;
            List<CogPointMarker> cogPoint = new List<CogPointMarker>();
            List<CogLine> cogLine = new List<CogLine>();
            List<CogCircle> cogCircle = new List<CogCircle>();
            List<CogRectangleAffine> cogRectAffine = new List<CogRectangleAffine>();
            List<CogCompositeShape> cogShape = new List<CogCompositeShape>();
            List<CogRectangle> cogRect = new List<CogRectangle>();
            List<CogEllipse> cogEllipse = new List<CogEllipse>();
            List<ICogRegion> cogRegion = new List<ICogRegion>();
            List<CogCompositeShape> cogBlobShape = new List<CogCompositeShape>();
            List<CogCompositeShape> cogPMaling = new List<CogCompositeShape>();

            _cogIDTool = new CogIDTool();
            _cogOCRMaxTool = new CogOCRMaxTool();
            _cogOCVMaxTool = new CogOCVMaxTool();

            bool bRes = true;

            bool[] bGraphicRes = new bool[_job.Tools.Count];
            cogImg = cogGrab;

            if (_job != null && cogImg != null)
            {
                if (_cogImgConvertTool[0] != null)
                {
                    if (_cogImgConvertTool[0] != null)
                    {
                        _cogImgConvertTool[0].InputImage = null;
                        _cogImgConvertTool[0].InputImage = cogImg;
                        if (_cogImgConvertTool[1] != null)
                            _cogImgConvertTool[1].InputImage = cogImg;
                    }
                }
                else
                {
                    return;
                }

                _job.Run();

                GetResultGraphic(ref cogPoint, ref cogLine, ref cogCircle, ref cogRectAffine, ref cogShape, ref cogRect, ref cogEllipse, ref cogRegion, ref cogBlobShape, ref bGraphicRes);
                Graphic_Result();
            }
            else
            {
                if (_job == null)
                    _OnSettingMessage("The working file was not loaded.", Color.Red, false, false, "alarm");

                if (cogImg == null)
                    _OnSettingMessage("No Image", Color.Red, false, false, "alarm");
            }
        }
        catch (System.Exception ex)
        {
            double[] dData = new double[1];
            dData[0] = 0;
        }
    }

    #region Draw Graphic
    private void GetResultGraphic(ref List<CogPointMarker> cogPoint, ref List<CogLine> cogLine, ref List<CogCircle> cogCircle, ref List<CogRectangleAffine> cogRectAffine, ref List<CogCompositeShape> cogShape, ref List<CogRectangle> cogRect, ref List<CogEllipse> cogEllipse, ref List<ICogRegion> cogRegion, ref List<CogCompositeShape> cogBlobShape, ref bool[] bGraphicRes)
    {
        try
        {
            int nCnt = 0;
            CogCompositeShape[] shape = null;
            CogCircle[] Circle = null;
            CogLine[] Line = null;
            CogEllipse[] Ellipse = null;
            CogPointMarker[] point = null;
            CogCreateSegmentTool Segment = null;
            CogCreateGraphicLabelTool _GrahpicLabel = null;
            //ICogRegion region = null;

            CogColorConstants color = new CogColorConstants();
            CogColorConstants color2 = new CogColorConstants();
            for (int i = 0; i < _job.Tools.Count; i++)
            {
                if (_graphic[i].bUse)
                {
                    if (_graphic[i].nColor == (int)GlovalVar.GraphicColor.Green)
                        color = CogColorConstants.Green;
                    else if (_graphic[i].nColor == (int)GlovalVar.GraphicColor.Blue)
                        color = CogColorConstants.Blue;
                    else if (_graphic[i].nColor == (int)GlovalVar.GraphicColor.Cyan)
                        color = CogColorConstants.Cyan;
                    else if (_graphic[i].nColor == (int)GlovalVar.GraphicColor.Magenta)
                        color = CogColorConstants.Magenta;
                    else if (_graphic[i].nColor == (int)GlovalVar.GraphicColor.Red)
                        color = CogColorConstants.Red;
                    else if (_graphic[i].nColor == (int)GlovalVar.GraphicColor.Yellow)
                        color = CogColorConstants.Yellow;

                    if (_graphic[i].nColor2 == (int)GlovalVar.GraphicColor.Green)
                        color2 = CogColorConstants.Green;
                    else if (_graphic[i].nColor2 == (int)GlovalVar.GraphicColor.Blue)
                        color2 = CogColorConstants.Blue;
                    else if (_graphic[i].nColor2 == (int)GlovalVar.GraphicColor.Cyan)
                        color2 = CogColorConstants.Cyan;
                    else if (_graphic[i].nColor2 == (int)GlovalVar.GraphicColor.Magenta)
                        color2 = CogColorConstants.Magenta;
                    else if (_graphic[i].nColor2 == (int)GlovalVar.GraphicColor.Red)
                        color2 = CogColorConstants.Red;
                    else if (_graphic[i].nColor2 == (int)GlovalVar.GraphicColor.Yellow)
                        color2 = CogColorConstants.Yellow;

                    if (_job.Tools[i].GetType() == typeof(CogPMAlignTool))
                    {
                        nCnt = (_job.Tools[i] as CogPMAlignTool).Results.Count;
                        shape = null;
                        shape = new CogCompositeShape[nCnt];

                        for (int j = 0; j < nCnt; j++)
                        {
                            shape[j] = new CogCompositeShape();
                            shape[j] = (_job.Tools[i] as CogPMAlignTool).Results[j].CreateResultGraphics(CogPMAlignResultGraphicConstants.MatchRegion);
                            shape[j].CompositionMode = CogCompositeShapeCompositionModeConstants.Uniform;
                            shape[j].Color = CogColorConstants.Blue;
                            shape[j].LineWidthInScreenPixels = 1;
                            shape[j].TipText = string.Format("{0}, Score : {1:F3}", (_job.Tools[i] as CogPMAlignTool).Name, (_job.Tools[i] as CogPMAlignTool).Results[j].Score);
                            cogShape.Add(shape[j]);
                        }

                    }

                    if (_job.Tools[i].GetType() == typeof(CogBlobTool))
                    {
                        CogBlobResultCollection blob = (_job.Tools[i] as CogBlobTool).Results.GetTopLevelBlobs(false);
                        var shapes = new CogCompositeShape();
                        for (int j = 0; j < blob.Count; j++)
                        {
                            if ((_job.Tools[i] as CogBlobTool).RunParams.RunTimeMeasures[0].FilterRangeLow <= blob[j].Area)
                            {
                                shapes = blob[j].CreateResultGraphics(CogBlobResultGraphicConstants.Boundary);
                                shapes.CompositionMode = CogCompositeShapeCompositionModeConstants.Uniform;
                                shapes.Color = CogColorConstants.Blue;
                                //if ((_job.Tools[i] as CogBlobTool).Results.GetBlobs().Count > 0)
                                //    shapes.Color = color;
                                //else
                                //    shapes.Color = CogColorConstants.Red;
                                shapes.LineWidthInScreenPixels = 1;

                                cogBlobShape.Add(shapes);
                            }
                        }

                        if ((_job.Tools[i] as CogBlobTool).Region.GetType() == typeof(CogCircle))
                        {
                            CogCircle cogBlobCircle = new CogCircle();
                            cogBlobCircle = (_job.Tools[i] as CogBlobTool).Region as CogCircle;
                            cogBlobCircle.Interactive = false;
                            cogBlobCircle.Color = color;
                            cogBlobCircle.LineWidthInScreenPixels = _graphic[i].nLineThick + 1;

                            cogRegion.Add(cogBlobCircle);
                        }
                        else if ((_job.Tools[i] as CogBlobTool).Region.GetType() == typeof(CogEllipse))
                        {
                            CogEllipse cogBlobEllipse = new CogEllipse();
                            cogBlobEllipse = new CogEllipse();
                            cogBlobEllipse = (_job.Tools[i] as CogBlobTool).Region as CogEllipse;
                            cogBlobEllipse.Interactive = false;
                            cogBlobEllipse.Color = color;
                            cogBlobEllipse.LineWidthInScreenPixels = _graphic[i].nLineThick + 1;

                            cogRegion.Add(cogBlobEllipse);
                        }
                        else if ((_job.Tools[i] as CogBlobTool).Region.GetType() == typeof(CogPolygon))
                        {
                            CogPolygon CogBlobPolygon = new CogPolygon();
                            CogBlobPolygon = (_job.Tools[i] as CogBlobTool).Region as CogPolygon;
                            CogBlobPolygon.Interactive = false;
                            CogBlobPolygon.Color = color;
                            CogBlobPolygon.LineWidthInScreenPixels = _graphic[i].nLineThick + 1;

                            cogRegion.Add(CogBlobPolygon);
                        }
                        else if ((_job.Tools[i] as CogBlobTool).Region.GetType() == typeof(CogRectangleAffine))
                        {
                            CogRectangleAffine cogBlobRectAff = new CogRectangleAffine();
                            cogBlobRectAff = (_job.Tools[i] as CogBlobTool).Region as CogRectangleAffine;
                            cogBlobRectAff.Interactive = false;
                            cogBlobRectAff.Color = color;
                            cogBlobRectAff.LineWidthInScreenPixels = _graphic[i].nLineThick + 1;

                            cogRegion.Add(cogBlobRectAff);
                        }
                        else if ((_job.Tools[i] as CogBlobTool).Region.GetType() == typeof(CogRectangle))
                        {
                            CogRectangle cogBlobRect = new CogRectangle();
                            cogBlobRect = (_job.Tools[i] as CogBlobTool).Region as CogRectangle;
                            cogBlobRect.Interactive = false;
                            cogBlobRect.Color = color;
                            cogBlobRect.LineWidthInScreenPixels = _graphic[i].nLineThick + 1;

                            cogRegion.Add(cogBlobRect);
                        }
                        else if ((_job.Tools[i] as CogBlobTool).Region.GetType() == typeof(CogCircularAnnulusSection))
                        {
                            CogCircularAnnulusSection cogBlobCircular = new CogCircularAnnulusSection();
                            //Circle = new CogCircle[1];
                            cogBlobCircular = (_job.Tools[i] as CogBlobTool).Region as CogCircularAnnulusSection;
                            cogBlobCircular.Interactive = false;
                            cogBlobCircular.Color = color;
                            cogBlobCircular.LineWidthInScreenPixels = _graphic[i].nLineThick + 1;

                            cogRegion.Add(cogBlobCircular);
                        }
                        else if ((_job.Tools[i] as CogBlobTool).Region.GetType() == typeof(CogEllipticalAnnulusSection))
                        {
                            CogEllipticalAnnulusSection cogBlobElliptical = new CogEllipticalAnnulusSection();
                            cogBlobElliptical = (_job.Tools[i] as CogBlobTool).Region as CogEllipticalAnnulusSection;
                            cogBlobElliptical.Color = color;
                            cogBlobElliptical.Interactive = false;
                            cogBlobElliptical.LineWidthInScreenPixels = _graphic[i].nLineThick + 1;

                            cogRegion.Add(cogBlobElliptical);
                        }
                    }

                    if (_job.Tools[i].GetType() == typeof(CogCaliperTool))
                    {
                        nCnt = (_job.Tools[i] as CogCaliperTool).Results.Count;
                        point = null;
                        point = new CogPointMarker[nCnt];
                        for (int j = 0; j < nCnt; j++)
                        {
                            point[j] = new CogPointMarker();
                            point[j].X = (_job.Tools[i] as CogCaliperTool).Results[j].PositionX;
                            point[j].Y = (_job.Tools[i] as CogCaliperTool).Results[j].PositionY;
                            point[j].Color = CogColorConstants.Blue;
                            point[j].LineWidthInScreenPixels = 1;
                            point[j].TipText = string.Format("{0}, CenterX : {0:F3}, CenterY : {1:F3}", (_job.Tools[i] as CogCaliperTool).Name, (_job.Tools[i] as CogCaliperTool).Results[j].PositionX, (_job.Tools[i] as CogCaliperTool).Results[j].PositionX);

                            cogPoint.Add(point[j]);
                        }
                    }

                    if (_job.Tools[i].GetType() == typeof(CogCreateCircleTool))
                    {
                        Circle = null;
                        Circle = new CogCircle[1];
                        Circle[0] = new CogCircle();
                        Circle[0] = (_job.Tools[i] as CogCreateCircleTool).GetOutputCircle();
                        Circle[0].Color = CogColorConstants.Blue;
                        Circle[0].LineWidthInScreenPixels = 1;
                        Circle[0].GraphicDOFEnable = CogCircleDOFConstants.None;
                        Circle[0].Interactive = true;
                        Circle[0].SelectedSpaceName = (_job.Tools[i] as CogCreateCircleTool).InputImage.SelectedSpaceName;
                        Circle[0].TipText = string.Format("{0}, CenterX : {1:F3}, CenterY : {2:F3}, Area : {3:F3}", (_job.Tools[i] as CogPMAlignTool).Name, Circle[0].CenterX, Circle[0].CenterY, Circle[0].Area);
                        cogCircle.Add(Circle[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogCreateEllipseTool))
                    {
                        Ellipse = null;
                        Ellipse = new CogEllipse[1];
                        Ellipse[0] = new CogEllipse();
                        Ellipse[0] = (_job.Tools[i] as CogCreateEllipseTool).GetOutputEllipse();
                        Ellipse[0].Color = CogColorConstants.Blue;
                        Ellipse[0].LineWidthInScreenPixels = 1;
                        Ellipse[0].GraphicDOFEnable = CogEllipseDOFConstants.None;
                        Ellipse[0].Interactive = true;
                        Ellipse[0].SelectedSpaceName = (_job.Tools[i] as CogCreateCircleTool).InputImage.SelectedSpaceName;
                        Ellipse[0].TipText = string.Format("{0}, CenterX : {1:F3}, CenterY : {2:F3}, Area : {3:F3}", (_job.Tools[i] as CogCreateEllipseTool).Name, Ellipse[0].CenterX, Ellipse[0].CenterY, Ellipse[0].Area);
                        cogEllipse.Add(Ellipse[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogCreateLineTool))
                    {
                        Line = null;
                        Line = new CogLine[1];
                        Line[0] = new CogLine();
                        Line[0] = (_job.Tools[i] as CogCreateLineTool).GetOutputLine();
                        Line[0].Color = color;
                        Line[0].LineWidthInScreenPixels = 3;
                        Line[0].GraphicDOFEnable = CogLineDOFConstants.None;
                        Line[0].Interactive = true;
                        Line[0].SelectedSpaceName = (_job.Tools[i] as CogCreateLineTool).InputImage.SelectedSpaceName;
                        Line[0].TipText = string.Format("{0}, CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogCreateLineTool).Name, Line[0].X, Line[0].Y);
                        cogLine.Add(Line[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogCreateSegmentAvgSegsTool))
                    {
                        Line = null;
                        Line = new CogLine[2];
                        Line[0] = new CogLine();
                        Line[1] = new CogLine();
                        Line[0] = (_job.Tools[i] as CogCreateSegmentAvgSegsTool).SegmentA.CreateLine();
                        Line[0].Color = color;
                        Line[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Line[0].GraphicDOFEnable = CogLineDOFConstants.None;
                        Line[0].Interactive = true;
                        Line[0].SelectedSpaceName = (_job.Tools[i] as CogCreateSegmentAvgSegsTool).InputImage.SelectedSpaceName;
                        Line[0].TipText = string.Format("{0}, CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogCreateSegmentAvgSegsTool).Name, Line[0].X, Line[0].Y);
                        cogLine.Add(Line[0]);

                        Line[1] = (_job.Tools[i] as CogCreateSegmentAvgSegsTool).SegmentB.CreateLine();
                        Line[1].Color = color;
                        Line[1].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Line[1].GraphicDOFEnable = CogLineDOFConstants.None;
                        Line[1].Interactive = true;
                        Line[1].SelectedSpaceName = (_job.Tools[i] as CogCreateSegmentAvgSegsTool).InputImage.SelectedSpaceName;
                        Line[1].TipText = string.Format("{0}, X : {1:F3}, rY : {2:F3}", (_job.Tools[i] as CogCreateSegmentAvgSegsTool).Name, Line[1].X, Line[1].Y);
                        cogLine.Add(Line[1]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogCreateSegmentTool))
                    {
                        Line = null;
                        Line = new CogLine[1];
                        Line[0] = new CogLine();
                        Line[0] = (_job.Tools[i] as CogCreateSegmentTool).Segment.CreateLine();
                        Line[0].Color = color;
                        Line[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Line[0].GraphicDOFEnable = CogLineDOFConstants.None;
                        Line[0].Interactive = true;
                        Line[0].SelectedSpaceName = (_job.Tools[i] as CogCreateSegmentTool).InputImage.SelectedSpaceName;
                        Line[0].TipText = string.Format("{0}, CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogCreateSegmentTool).Name, Line[0].X, Line[0].Y);
                        cogLine.Add(Line[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogFindCircleTool))
                    {
                        nCnt = (_job.Tools[i] as CogFindCircleTool).Results.Count;
                        if (nCnt > 0)
                        {
                            Circle = null;
                            Circle = new CogCircle[1];

                            Circle[0] = new CogCircle();
                            Circle[0] = (_job.Tools[i] as CogFindCircleTool).Results.GetCircle();
                            Circle[0].Color = color;
                            Circle[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                            Circle[0].GraphicDOFEnable = CogCircleDOFConstants.None;
                            Circle[0].Interactive = true;
                            Circle[0].SelectedSpaceName = (_job.Tools[i] as CogFindCircleTool).InputImage.SelectedSpaceName;
                            Circle[0].TipText = string.Format("{0}, CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogFindCircleTool).Name, Circle[0].CenterX, Circle[0].CenterX);
                            cogCircle.Add(Circle[0]);

                        }
                        //Circle = null;
                        //Circle = new CogCircle[nCnt];

                        //for (int j = 0; j < nCnt; j++)
                        //{
                        //    Circle[j] = new CogCircle();
                        //    Circle[j] = (_job.Tools[i] as CogFindCircleTool).Results.GetCircle();
                        //    Circle[j].Color = color;
                        //    Circle[j].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        //    Circle[j].GraphicDOFEnable = CogCircleDOFConstants.None;
                        //    Circle[j].Interactive = true;
                        //    Circle[j].SelectedSpaceName = (_job.Tools[i] as CogCreateCircleTool).InputImage.SelectedSpaceName;
                        //    Circle[j].TipText = string.Format("{0}, CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogFindCircleTool).Name, Circle[j].CenterX, Circle[j].CenterX);
                        //    cogCircle.Add(Circle[j]);
                        //}
                    }

                    if (_job.Tools[i].GetType() == typeof(CogFindCornerTool))
                    {
                        point = null;
                        point = new CogPointMarker[1];
                        point[0] = new CogPointMarker();
                        point[0].X = (_job.Tools[i] as CogFindCornerTool).Result.CornerX;
                        point[0].X = (_job.Tools[i] as CogFindCornerTool).Result.CornerY;
                        point[0].Color = color;
                        point[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        point[0].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                        point[0].Interactive = true;
                        point[0].SelectedSpaceName = (_job.Tools[i] as CogFindCornerTool).InputImage.SelectedSpaceName;
                        point[0].TipText = string.Format("{0}, CornerX : {1:F3}, CornerY : {2:F3}", (_job.Tools[i] as CogFindCornerTool).Name, point[0].X, point[0].Y);
                        cogPoint.Add(point[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogFindEllipseTool))
                    {
                        nCnt = (_job.Tools[i] as CogFindEllipseTool).Results.Count;
                        Ellipse = null;
                        Ellipse = new CogEllipse[nCnt];

                        for (int j = 0; j < nCnt; j++)
                        {
                            Ellipse[j] = new CogEllipse();
                            Ellipse[j] = (_job.Tools[i] as CogFindEllipseTool).Results.GetEllipse();
                            Ellipse[j].Color = color;
                            Ellipse[j].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                            Ellipse[j].GraphicDOFEnable = CogEllipseDOFConstants.None;
                            Ellipse[j].Interactive = true;
                            Ellipse[j].SelectedSpaceName = (_job.Tools[i] as CogFindEllipseTool).InputImage.SelectedSpaceName;
                            Ellipse[j].TipText = string.Format("{0}, X : {1:F3}, Y : {1:F3}, Area : {2:F3}", (_job.Tools[i] as CogFindEllipseTool).Name, Ellipse[j].CenterX, Ellipse[j].CenterY, Ellipse[j].Area);
                            cogEllipse.Add(Ellipse[j]);
                        }
                    }

                    if (_job.Tools[i].GetType() == typeof(CogFindLineTool))
                    {
                        nCnt = (_job.Tools[i] as CogFindLineTool).Results.Count;
                        Line = null;
                        Line = new CogLine[nCnt];
                        for (int j = 0; j < (_job.Tools[i] as CogFindLineTool).Results.Count; j++)
                        {
                            Line[j] = new CogLine();
                            Line[j] = (_job.Tools[i] as CogFindLineTool).Results.GetLine();
                            Line[j].Color = color;
                            Line[j].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                            Line[j].GraphicDOFEnable = CogLineDOFConstants.None;
                            Line[j].Interactive = true;
                            Line[j].SelectedSpaceName = (_job.Tools[i] as CogFindLineTool).InputImage.SelectedSpaceName;
                            Line[j].TipText = string.Format("{0}, X : {1:F3}, Y : {2:F3}", (_job.Tools[i] as CogFindLineTool).Name, Line[j].X, Line[j].Y);
                            cogLine.Add(Line[j]);
                        }
                    }

                    if (_job.Tools[i].GetType() == typeof(CogFindCircleTool))
                    {
                        nCnt = (_job.Tools[i] as CogFindCircleTool).Results.Count;
                        Circle = null;
                        Circle = new CogCircle[nCnt];
                        for (int j = 0; j < nCnt; j++)
                        {
                            Circle[j] = new CogCircle();
                            Circle[j] = (_job.Tools[i] as CogFindCircleTool).Results.GetCircle();
                            Circle[j].Color = color;
                            Circle[j].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                            Circle[j].GraphicDOFEnable = CogCircleDOFConstants.None;
                            Circle[j].Interactive = true;
                            Circle[j].SelectedSpaceName = (_job.Tools[i] as CogFindCircleTool).InputImage.SelectedSpaceName;
                            Circle[j].TipText = string.Format("{0}, CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogFindCircleTool).Name, Circle[j].CenterX, Circle[j].Area);
                            cogCircle.Add(Circle[j]);
                        }
                    }

                    if (_job.Tools[i].GetType() == typeof(CogFitCircleTool))
                    {
                        Circle = null;
                        Circle = new CogCircle[1];
                        Circle[0] = new CogCircle();
                        Circle[0] = (_job.Tools[i] as CogFitCircleTool).Result.GetCircle();
                        Circle[0].Color = color;
                        Circle[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Circle[0].GraphicDOFEnable = CogCircleDOFConstants.None;
                        Circle[0].Interactive = true;
                        Circle[0].SelectedSpaceName = (_job.Tools[i] as CogFitCircleTool).InputImage.SelectedSpaceName;
                        Circle[0].TipText = string.Format("{0}, CenterX : {1:F3}, CenterY : {2:F3}, , Area : {3:F3}", (_job.Tools[i] as CogFitCircleTool).Name, Circle[0].CenterX, Circle[0].CenterY, Circle[0].Area);
                        cogCircle.Add(Circle[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogFitEllipseTool))
                    {
                        Ellipse = null;
                        Ellipse = new CogEllipse[1];
                        Ellipse[0] = new CogEllipse();
                        Ellipse[0] = (_job.Tools[i] as CogFitEllipseTool).Result.GetEllipse();
                        Ellipse[0].Color = color;
                        Ellipse[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Ellipse[0].GraphicDOFEnable = CogEllipseDOFConstants.None;
                        Ellipse[0].Interactive = true;
                        Ellipse[0].SelectedSpaceName = (_job.Tools[i] as CogFitEllipseTool).InputImage.SelectedSpaceName;
                        Ellipse[0].TipText = string.Format("{0}, X : {1:F3}, Y : {2:F3}, Area : {3:F3}", (_job.Tools[i] as CogFitEllipseTool).Name, Ellipse[0].CenterX, Ellipse[0].CenterX, Ellipse[0].Area);
                        cogEllipse.Add(Ellipse[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogFitLineTool))
                    {
                        Line = null;
                        Line = new CogLine[1];
                        Line[0] = new CogLine();
                        Line[0] = (_job.Tools[i] as CogFitLineTool).Result.GetLine();
                        Line[0].Color = color;
                        Line[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Line[0].GraphicDOFEnable = CogLineDOFConstants.None;
                        Line[0].Interactive = true;
                        Line[0].SelectedSpaceName = (_job.Tools[i] as CogFitLineTool).InputImage.SelectedSpaceName;
                        Line[0].TipText = string.Format("{0}, X : {1:F3}, Y : {2:F3}", (_job.Tools[i] as CogFitLineTool).Name, Line[0].X, Line[0].Y);
                        cogLine.Add(Line[0]);
                    }

                    //if (_job.Tools[i].GetType() == typeof(CogAngleLineLineTool))
                    //{
                    //    Segment = null;
                    //    Segment = new CogCreateSegmentTool();

                    //    Segment.Segment.StartX = (_job.Tools[i] as CogAngleLineLineTool).LineA.X;
                    //    Segment.Segment.StartX = (_job.Tools[i] as CogAngleLineLineTool).LineA.Y;

                    //    Segment.Segment.EndX = (_job.Tools[i] as CogAngleLineLineTool).LineB.X;
                    //    Segment.Segment.EndY = (_job.Tools[i] as CogAngleLineLineTool).LineB.Y;

                    //    point = null;
                    //    point = new CogPointMarker[2];
                    //    point[0] = new CogPointMarker();
                    //    point[0].X = Segment.Segment.StartX;
                    //    point[0].X = Segment.Segment.StartY;
                    //    point[0].Color = CogColorConstants.Green;
                    //    point[0].LineWidthInScreenPixels = 2;
                    //    point[0].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                    //    point[0].Interactive = true;
                    //    point[0].SelectedSpaceName = (_job.Tools[i] as CogAngleLineLineTool).InputImage.SelectedSpaceName;
                    //    point[0].TipText = string.Format("{0}(LineA), CornerX : {1:F3}, CornerY : {2:F3}", (_job.Tools[i] as CogAngleLineLineTool).Name, point[0].X, point[0].Y);
                    //    cogPoint.Add(point[0]);

                    //    point[1] = new CogPointMarker();
                    //    point[1].X = Segment.Segment.EndX;
                    //    point[1].X = Segment.Segment.EndY;
                    //    point[1].Color = CogColorConstants.Green;
                    //    point[1].LineWidthInScreenPixels = 2;
                    //    point[1].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                    //    point[1].Interactive = true;
                    //    point[1].SelectedSpaceName = (_job.Tools[i] as CogAngleLineLineTool).InputImage.SelectedSpaceName;
                    //    point[1].TipText = string.Format("{0}(LineA), CornerX : {1:F3}, CornerY : {2:F3}", (_job.Tools[i] as CogAngleLineLineTool).Name, point[1].X, point[1].Y);
                    //    cogPoint.Add(point[1]);

                    //    Line = null;
                    //    Line = new CogLine[1];
                    //    Line[0] = new CogLine();
                    //    Line[0] = Segment.Segment.CreateLine();
                    //    Line[0].Color = CogColorConstants.Green;
                    //    Line[0].LineWidthInScreenPixels = 2;
                    //    Line[0].GraphicDOFEnable = CogLineDOFConstants.None;
                    //    Line[0].Interactive = true;
                    //    Line[0].SelectedSpaceName = (_job.Tools[i] as CogAngleLineLineTool).InputImage.SelectedSpaceName;
                    //    Line[0].TipText = string.Format("{0}, X : {1:F3}, Y : {2:F3}", (_job.Tools[i] as CogAngleLineLineTool).Name, Line[0].X, Line[0].Y);
                    //    cogLine.Add(Line[0]);
                    //}

                    if (_job.Tools[i].GetType() == typeof(CogAnglePointPointTool))
                    {
                        Segment = null;
                        Segment = new CogCreateSegmentTool();

                        Segment.Segment.StartX = (_job.Tools[i] as CogAnglePointPointTool).StartX;
                        Segment.Segment.StartY = (_job.Tools[i] as CogAnglePointPointTool).StartY;

                        Segment.Segment.EndX = (_job.Tools[i] as CogAnglePointPointTool).EndX;
                        Segment.Segment.EndY = (_job.Tools[i] as CogAnglePointPointTool).EndY;

                        point = null;
                        point = new CogPointMarker[2];
                        point[0] = new CogPointMarker();
                        point[0].X = Segment.Segment.StartX;
                        point[0].X = Segment.Segment.StartY;
                        point[0].Color = color;
                        point[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        point[0].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                        point[0].Interactive = true;
                        point[0].SelectedSpaceName = (_job.Tools[i] as CogAnglePointPointTool).InputImage.SelectedSpaceName;
                        point[0].TipText = string.Format("{0}, CenterX : {1:F3}, CenterY: {2:F3}", (_job.Tools[i] as CogAnglePointPointTool).Name, point[0].X, point[0].Y);
                        cogPoint.Add(point[0]);

                        point[1] = new CogPointMarker();
                        point[1].X = Segment.Segment.EndX;
                        point[1].X = Segment.Segment.EndY;
                        point[1].Color = color;
                        point[1].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        point[1].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                        point[1].Interactive = true;
                        point[1].SelectedSpaceName = (_job.Tools[i] as CogAnglePointPointTool).InputImage.SelectedSpaceName;
                        point[1].TipText = string.Format("{0}, CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogAnglePointPointTool).Name, point[1].X, point[1].Y);
                        cogPoint.Add(point[1]);

                        Line = null;
                        Line = new CogLine[1];
                        Line[0] = new CogLine();
                        Line[0] = Segment.Segment.CreateLine();
                        Line[0].Color = color;
                        Line[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Line[0].GraphicDOFEnable = CogLineDOFConstants.None;
                        Line[0].Interactive = true;
                        Line[0].SelectedSpaceName = (_job.Tools[i] as CogAnglePointPointTool).InputImage.SelectedSpaceName;
                        Line[0].TipText = string.Format("{0}, X : {1:F3}, Y : {2:F3}", (_job.Tools[i] as CogAnglePointPointTool).Name, Line[0].X, Line[0].Y);
                        cogLine.Add(Line[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogDistanceCircleCircleTool))
                    {
                        Segment = null;
                        Segment = new CogCreateSegmentTool();

                        Segment.Segment.StartX = (_job.Tools[i] as CogDistanceCircleCircleTool).CircleA.CenterX;
                        Segment.Segment.StartY = (_job.Tools[i] as CogDistanceCircleCircleTool).CircleA.CenterY;

                        Segment.Segment.EndX = (_job.Tools[i] as CogDistanceCircleCircleTool).CircleB.CenterX;
                        Segment.Segment.EndY = (_job.Tools[i] as CogDistanceCircleCircleTool).CircleB.CenterY;

                        point = null;
                        point = new CogPointMarker[2];
                        point[0] = new CogPointMarker();
                        point[0].X = Segment.Segment.StartX;
                        point[0].X = Segment.Segment.StartY;
                        point[0].Color = color;
                        point[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        point[0].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                        point[0].Interactive = true;
                        point[0].SelectedSpaceName = (_job.Tools[i] as CogDistanceCircleCircleTool).InputImage.SelectedSpaceName;
                        point[0].TipText = string.Format("{0}, CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogDistanceCircleCircleTool).Name, point[0].X, point[0].Y);
                        cogPoint.Add(point[0]);

                        point[1] = new CogPointMarker();
                        point[1].X = Segment.Segment.EndX;
                        point[1].X = Segment.Segment.EndY;
                        point[1].Color = color;
                        point[1].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        point[1].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                        point[1].Interactive = true;
                        point[1].SelectedSpaceName = (_job.Tools[i] as CogDistanceCircleCircleTool).InputImage.SelectedSpaceName;
                        point[1].TipText = string.Format("{0}(LineA), CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogDistanceCircleCircleTool).Name, point[1].X, point[1].Y);
                        cogPoint.Add(point[1]);

                        Line = null;
                        Line = new CogLine[1];
                        Line[0] = new CogLine();
                        Line[0] = Segment.Segment.CreateLine();
                        Line[0].Color = color;
                        Line[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Line[0].GraphicDOFEnable = CogLineDOFConstants.None;
                        Line[0].Interactive = true;
                        Line[0].SelectedSpaceName = (_job.Tools[i] as CogDistanceCircleCircleTool).InputImage.SelectedSpaceName;
                        Line[0].TipText = string.Format("{0}, X : {1:F3}, Y : {2:F3}", (_job.Tools[i] as CogDistanceCircleCircleTool).Name, Line[0].X, Line[0].Y);
                        cogLine.Add(Line[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogDistanceLineCircleTool))
                    {
                        Segment = null;
                        Segment = new CogCreateSegmentTool();

                        Segment.Segment.StartX = (_job.Tools[i] as CogDistanceLineCircleTool).InputCircle.CenterX;
                        Segment.Segment.StartY = (_job.Tools[i] as CogDistanceLineCircleTool).InputCircle.CenterY;

                        Segment.Segment.EndX = (_job.Tools[i] as CogDistanceLineCircleTool).Line.X;
                        Segment.Segment.EndY = (_job.Tools[i] as CogDistanceLineCircleTool).Line.Y;

                        point = null;
                        point = new CogPointMarker[1];
                        point[0] = new CogPointMarker();
                        point[0].X = Segment.Segment.StartX;
                        point[0].X = Segment.Segment.StartY;
                        point[0].Color = color;
                        point[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        point[0].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                        point[0].Interactive = true;
                        point[0].SelectedSpaceName = (_job.Tools[i] as CogDistanceLineCircleTool).InputImage.SelectedSpaceName;
                        point[0].TipText = string.Format("{0}, CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogDistanceLineCircleTool).Name, point[0].X, point[0].Y);
                        cogPoint.Add(point[0]);

                        Line = null;
                        Line = new CogLine[1];
                        Line[0] = new CogLine();
                        Line[0] = Segment.Segment.CreateLine();
                        Line[0].Color = color;
                        Line[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Line[0].GraphicDOFEnable = CogLineDOFConstants.None;
                        Line[0].Interactive = true;
                        Line[0].SelectedSpaceName = (_job.Tools[i] as CogDistanceLineCircleTool).InputImage.SelectedSpaceName;
                        Line[0].TipText = string.Format("{0}, X : {1:F3}, Y : {2:F3}", (_job.Tools[i] as CogDistanceLineCircleTool).Name, Line[0].X, Line[0].Y);
                        cogLine.Add(Line[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogDistanceLineEllipseTool))
                    {
                        Segment = null;
                        Segment = new CogCreateSegmentTool();

                        Segment.Segment.StartX = (_job.Tools[i] as CogDistanceLineEllipseTool).EllipseX;
                        Segment.Segment.StartY = (_job.Tools[i] as CogDistanceLineEllipseTool).EllipseY;

                        Segment.Segment.EndX = (_job.Tools[i] as CogDistanceLineEllipseTool).LineX;
                        Segment.Segment.EndY = (_job.Tools[i] as CogDistanceLineEllipseTool).LineY;

                        point = null;
                        point = new CogPointMarker[1];
                        point[0] = new CogPointMarker();
                        point[0].X = Segment.Segment.StartX;
                        point[0].X = Segment.Segment.StartY;
                        point[0].Color = color;
                        point[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        point[0].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                        point[0].Interactive = true;
                        point[0].SelectedSpaceName = (_job.Tools[i] as CogDistanceLineEllipseTool).InputImage.SelectedSpaceName;
                        point[0].TipText = string.Format("{0}(LineA), CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogDistanceLineEllipseTool).Name, point[0].X, point[0].Y);
                        cogPoint.Add(point[0]);

                        Line = null;
                        Line = new CogLine[1];
                        Line[0] = new CogLine();
                        Line[0] = Segment.Segment.CreateLine();
                        Line[0].Color = color;
                        Line[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Line[0].GraphicDOFEnable = CogLineDOFConstants.None;
                        Line[0].Interactive = true;
                        Line[0].SelectedSpaceName = (_job.Tools[i] as CogDistanceLineEllipseTool).InputImage.SelectedSpaceName;
                        Line[0].TipText = string.Format("{0}, X : {1:F3}, Y : {2:F3}", (_job.Tools[i] as CogDistanceLineEllipseTool).Name, Line[0].X, Line[0].Y);
                        cogLine.Add(Line[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogDistancePointCircleTool))
                    {
                        Segment = null;
                        Segment = new CogCreateSegmentTool();

                        Segment.Segment.StartX = (_job.Tools[i] as CogDistancePointCircleTool).X;
                        Segment.Segment.StartY = (_job.Tools[i] as CogDistancePointCircleTool).Y;

                        Segment.Segment.EndX = (_job.Tools[i] as CogDistancePointCircleTool).InputCircle.CenterX;
                        Segment.Segment.EndY = (_job.Tools[i] as CogDistancePointCircleTool).InputCircle.CenterY;

                        point = null;
                        point = new CogPointMarker[1];
                        point[0] = new CogPointMarker();
                        point[0].X = Segment.Segment.StartX;
                        point[0].X = Segment.Segment.StartY;
                        point[0].Color = color;
                        point[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        point[0].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                        point[0].Interactive = true;
                        point[0].SelectedSpaceName = (_job.Tools[i] as CogDistancePointCircleTool).InputImage.SelectedSpaceName;
                        point[0].TipText = string.Format("{0}(LineA), CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogDistancePointCircleTool).Name, point[0].X, point[0].Y);
                        cogPoint.Add(point[0]);

                        Line = null;
                        Line = new CogLine[1];
                        Line[0] = new CogLine();
                        Line[0] = Segment.Segment.CreateLine();
                        Line[0].Color = color;
                        Line[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Line[0].GraphicDOFEnable = CogLineDOFConstants.None;
                        Line[0].Interactive = true;
                        Line[0].SelectedSpaceName = (_job.Tools[i] as CogDistancePointCircleTool).InputImage.SelectedSpaceName;
                        Line[0].TipText = string.Format("{0}, X : {1:F3}, Y : {2:F3}", (_job.Tools[i] as CogDistancePointCircleTool).Name, Line[0].X, Line[0].Y);
                        cogLine.Add(Line[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogDistancePointEllipseTool))
                    {
                        Segment = null;
                        Segment = new CogCreateSegmentTool();

                        Segment.Segment.StartX = (_job.Tools[i] as CogDistancePointEllipseTool).X;
                        Segment.Segment.StartY = (_job.Tools[i] as CogDistancePointEllipseTool).Y;

                        Segment.Segment.EndX = (_job.Tools[i] as CogDistancePointEllipseTool).EllipseX;
                        Segment.Segment.EndY = (_job.Tools[i] as CogDistancePointEllipseTool).EllipseY;

                        point = null;
                        point = new CogPointMarker[1];
                        point[0] = new CogPointMarker();
                        point[0].X = Segment.Segment.StartX;
                        point[0].X = Segment.Segment.StartY;
                        point[0].Color = color;
                        point[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        point[0].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                        point[0].Interactive = true;
                        point[0].SelectedSpaceName = (_job.Tools[i] as CogDistancePointEllipseTool).InputImage.SelectedSpaceName;
                        point[0].TipText = string.Format("{0}(LineA), CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogDistancePointEllipseTool).Name, point[0].X, point[0].Y);
                        cogPoint.Add(point[0]);

                        Line = null;
                        Line = new CogLine[1];
                        Line[0] = new CogLine();
                        Line[0] = Segment.Segment.CreateLine();
                        Line[0].Color = color;
                        Line[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Line[0].GraphicDOFEnable = CogLineDOFConstants.None;
                        Line[0].Interactive = true;
                        Line[0].SelectedSpaceName = (_job.Tools[i] as CogDistancePointEllipseTool).InputImage.SelectedSpaceName;
                        Line[0].TipText = string.Format("{0}, X : {1:F3}, Y : {2:F3}", (_job.Tools[i] as CogDistancePointEllipseTool).Name, Line[0].X, Line[0].Y);
                        cogLine.Add(Line[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogDistancePointLineTool))
                    {
                        Segment = null;
                        Segment = new CogCreateSegmentTool();

                        Segment.Segment.StartX = (_job.Tools[i] as CogDistancePointLineTool).X;
                        Segment.Segment.StartY = (_job.Tools[i] as CogDistancePointLineTool).Y;

                        Segment.Segment.EndX = (_job.Tools[i] as CogDistancePointLineTool).LineX;
                        Segment.Segment.EndY = (_job.Tools[i] as CogDistancePointLineTool).LineY;

                        point = null;
                        point = new CogPointMarker[1];
                        point[0] = new CogPointMarker();
                        point[0].X = (_job.Tools[i] as CogDistancePointLineTool).X;
                        point[0].X = (_job.Tools[i] as CogDistancePointLineTool).Y;
                        point[0].Color = color;
                        point[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        point[0].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                        point[0].Interactive = true;
                        point[0].SelectedSpaceName = (_job.Tools[i] as CogDistancePointLineTool).InputImage.SelectedSpaceName;
                        point[0].TipText = string.Format("{0}(LineA), CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogDistancePointLineTool).Name, point[0].X, point[0].Y);
                        cogPoint.Add(point[0]);

                        Line = null;
                        Line = new CogLine[1];
                        Line[0] = new CogLine();
                        Line[0] = Segment.Segment.CreateLine();
                        Line[0].Color = color;
                        Line[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Line[0].GraphicDOFEnable = CogLineDOFConstants.None;
                        Line[0].Interactive = true;
                        Line[0].SelectedSpaceName = (_job.Tools[i] as CogDistancePointLineTool).InputImage.SelectedSpaceName;
                        Line[0].TipText = string.Format("{0}, X : {1:F3}, Y : {2:F3}", (_job.Tools[i] as CogDistancePointLineTool).Name, Line[0].X, Line[0].Y);
                        cogLine.Add(Line[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogDistancePointPointTool))
                    {
                        Segment = null;
                        Segment = new CogCreateSegmentTool();

                        Segment.Segment.StartX = (_job.Tools[i] as CogDistancePointPointTool).StartX;
                        Segment.Segment.StartY = (_job.Tools[i] as CogDistancePointPointTool).StartY;

                        Segment.Segment.EndX = (_job.Tools[i] as CogDistancePointPointTool).EndX;
                        Segment.Segment.EndY = (_job.Tools[i] as CogDistancePointPointTool).EndY;

                        point = null;
                        point = new CogPointMarker[2];
                        point[0] = new CogPointMarker();
                        point[0].X = Segment.Segment.StartX;
                        point[0].X = Segment.Segment.StartY;
                        point[0].Color = color;
                        point[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        point[0].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                        point[0].Interactive = true;
                        point[0].SelectedSpaceName = (_job.Tools[i] as CogDistancePointPointTool).InputImage.SelectedSpaceName;
                        point[0].TipText = string.Format("{0}(LineA), CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogDistancePointPointTool).Name, point[0].X, point[0].Y);
                        cogPoint.Add(point[0]);

                        point[1] = new CogPointMarker();
                        point[1].X = Segment.Segment.EndX;
                        point[1].X = Segment.Segment.EndY;
                        point[1].Color = color;
                        point[1].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        point[1].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                        point[1].Interactive = true;
                        point[1].SelectedSpaceName = (_job.Tools[i] as CogDistancePointPointTool).InputImage.SelectedSpaceName;
                        point[1].TipText = string.Format("{0}(LineA), CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogDistancePointPointTool).Name, point[1].X, point[1].Y);
                        cogPoint.Add(point[1]);

                        Line = null;
                        Line = new CogLine[1];
                        Line[0] = new CogLine();
                        Line[0] = Segment.Segment.CreateLine();
                        Line[0].Color = color;
                        Line[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Line[0].GraphicDOFEnable = CogLineDOFConstants.None;
                        Line[0].Interactive = true;
                        Line[0].SelectedSpaceName = (_job.Tools[i] as CogDistancePointPointTool).InputImage.SelectedSpaceName;
                        Line[0].TipText = string.Format("{0}, X : {1:F3}, Y : {2:F3}", (_job.Tools[i] as CogDistancePointPointTool).Name, Line[0].X, Line[0].Y);
                        cogLine.Add(Line[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogDistancePointSegmentTool))
                    {
                        Segment = null;
                        Segment = new CogCreateSegmentTool();

                        Segment.Segment.StartX = (_job.Tools[i] as CogDistancePointSegmentTool).X;
                        Segment.Segment.StartY = (_job.Tools[i] as CogDistancePointSegmentTool).X;

                        Segment.Segment.EndX = (_job.Tools[i] as CogDistancePointSegmentTool).SegmentX;
                        Segment.Segment.EndY = (_job.Tools[i] as CogDistancePointSegmentTool).SegmentY;

                        point = null;
                        point = new CogPointMarker[1];
                        point[0] = new CogPointMarker();
                        point[0].X = (_job.Tools[i] as CogDistancePointSegmentTool).X;
                        point[0].X = (_job.Tools[i] as CogDistancePointSegmentTool).Y;
                        point[0].Color = color;
                        point[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        point[0].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                        point[0].Interactive = true;
                        point[0].SelectedSpaceName = (_job.Tools[i] as CogDistancePointSegmentTool).InputImage.SelectedSpaceName;
                        point[0].TipText = string.Format("{0}(LineA), CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogDistancePointSegmentTool).Name, point[0].X, point[0].Y);
                        cogPoint.Add(point[0]);

                        Line = null;
                        Line = new CogLine[1];
                        Line[0] = new CogLine();
                        Line[0] = Segment.Segment.CreateLine();
                        Line[0].Color = color;
                        Line[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Line[0].GraphicDOFEnable = CogLineDOFConstants.None;
                        Line[0].Interactive = true;
                        Line[0].SelectedSpaceName = (_job.Tools[i] as CogDistancePointSegmentTool).InputImage.SelectedSpaceName;
                        Line[0].TipText = string.Format("{0}, X : {1:F3}, Y : {2:F3}", (_job.Tools[i] as CogDistancePointSegmentTool).Name, Line[0].X, Line[0].Y);
                        cogLine.Add(Line[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogDistanceSegmentCircleTool))
                    {
                        Segment = null;
                        Segment = new CogCreateSegmentTool();

                        Segment.Segment.StartX = (_job.Tools[i] as CogDistanceSegmentCircleTool).InputCircle.CenterX;
                        Segment.Segment.StartY = (_job.Tools[i] as CogDistanceSegmentCircleTool).InputCircle.CenterY;

                        Segment.Segment.EndX = (_job.Tools[i] as CogDistanceSegmentCircleTool).SegmentX;
                        Segment.Segment.EndY = (_job.Tools[i] as CogDistanceSegmentCircleTool).SegmentY;

                        point = null;
                        point = new CogPointMarker[1];
                        point[0] = new CogPointMarker();
                        point[0].X = Segment.Segment.StartX;
                        point[0].X = Segment.Segment.StartY;
                        point[0].Color = color;
                        point[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        point[0].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                        point[0].Interactive = true;
                        point[0].SelectedSpaceName = (_job.Tools[i] as CogDistanceSegmentCircleTool).InputImage.SelectedSpaceName;
                        point[0].TipText = string.Format("{0}(LineA), CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogDistanceSegmentCircleTool).Name, point[0].X, point[0].Y);
                        cogPoint.Add(point[0]);

                        Line = null;
                        Line = new CogLine[1];
                        Line[0] = new CogLine();
                        Line[0] = Segment.Segment.CreateLine();
                        Line[0].Color = color;
                        Line[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Line[0].GraphicDOFEnable = CogLineDOFConstants.None;
                        Line[0].Interactive = true;
                        Line[0].SelectedSpaceName = (_job.Tools[i] as CogDistanceSegmentCircleTool).InputImage.SelectedSpaceName;
                        Line[0].TipText = string.Format("{0}, X : {1:F3}, Y : {2:F3}", (_job.Tools[i] as CogDistanceSegmentCircleTool).Name, Line[0].X, Line[0].Y);
                        cogLine.Add(Line[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogDistanceSegmentEllipseTool))
                    {
                        Segment = null;
                        Segment = new CogCreateSegmentTool();

                        Segment.Segment.StartX = (_job.Tools[i] as CogDistanceSegmentEllipseTool).EllipseX;
                        Segment.Segment.StartY = (_job.Tools[i] as CogDistanceSegmentEllipseTool).EllipseX;

                        Segment.Segment.EndX = (_job.Tools[i] as CogDistanceSegmentEllipseTool).SegmentX;
                        Segment.Segment.EndY = (_job.Tools[i] as CogDistanceSegmentEllipseTool).SegmentY;

                        point = null;
                        point = new CogPointMarker[1];
                        point[0] = new CogPointMarker();
                        point[0].X = Segment.Segment.StartX;
                        point[0].X = Segment.Segment.StartY;
                        point[0].Color = color;
                        point[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        point[0].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                        point[0].Interactive = true;
                        point[0].SelectedSpaceName = (_job.Tools[i] as CogDistanceSegmentEllipseTool).InputImage.SelectedSpaceName;
                        point[0].TipText = string.Format("{0}(LineA), CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogDistanceSegmentEllipseTool).Name, point[0].X, point[0].Y);
                        cogPoint.Add(point[0]);

                        Line = null;
                        Line = new CogLine[1];
                        Line[0] = new CogLine();
                        Line[0] = Segment.Segment.CreateLine();
                        Line[0].Color = color;
                        Line[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Line[0].GraphicDOFEnable = CogLineDOFConstants.None;
                        Line[0].Interactive = true;
                        Line[0].SelectedSpaceName = (_job.Tools[i] as CogDistanceSegmentEllipseTool).InputImage.SelectedSpaceName;
                        Line[0].TipText = string.Format("{0}, X : {1:F3}, Y : {2:F3}", (_job.Tools[i] as CogDistanceSegmentEllipseTool).Name, Line[0].X, Line[0].Y);
                        cogLine.Add(Line[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogDistanceSegmentLineTool))
                    {
                        Segment = null;
                        Segment = new CogCreateSegmentTool();

                        Segment.Segment.StartX = (_job.Tools[i] as CogDistanceSegmentLineTool).LineX;
                        Segment.Segment.StartY = (_job.Tools[i] as CogDistanceSegmentLineTool).LineY;

                        Segment.Segment.EndX = (_job.Tools[i] as CogDistanceSegmentLineTool).SegmentX;
                        Segment.Segment.EndY = (_job.Tools[i] as CogDistanceSegmentLineTool).SegmentY;

                        point = null;
                        point = new CogPointMarker[1];
                        point[0] = new CogPointMarker();
                        point[0].X = Segment.Segment.StartX;
                        point[0].X = Segment.Segment.StartY;
                        point[0].Color = color;
                        point[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        point[0].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                        point[0].Interactive = true;
                        point[0].SelectedSpaceName = (_job.Tools[i] as CogDistanceSegmentLineTool).InputImage.SelectedSpaceName;
                        point[0].TipText = string.Format("{0}(LineA), CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogDistanceSegmentLineTool).Name, point[0].X, point[0].Y);
                        cogPoint.Add(point[0]);

                        Line = null;
                        Line = new CogLine[1];
                        Line[0] = new CogLine();
                        Line[0] = Segment.Segment.CreateLine();
                        Line[0].Color = color;
                        Line[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Line[0].GraphicDOFEnable = CogLineDOFConstants.None;
                        Line[0].Interactive = true;
                        Line[0].SelectedSpaceName = (_job.Tools[i] as CogDistanceSegmentLineTool).InputImage.SelectedSpaceName;
                        Line[0].TipText = string.Format("{0}, X : {1:F3}, Y : {2:F3}", (_job.Tools[i] as CogDistanceSegmentLineTool).Name, Line[0].X, Line[0].Y);
                        cogLine.Add(Line[0]);
                    }

                    if (_job.Tools[i].GetType() == typeof(CogDistanceSegmentSegmentTool))
                    {
                        Segment = null;
                        Segment = new CogCreateSegmentTool();

                        Segment.Segment.StartX = (_job.Tools[i] as CogDistanceSegmentSegmentTool).SegmentAX;
                        Segment.Segment.StartY = (_job.Tools[i] as CogDistanceSegmentSegmentTool).SegmentAY;

                        Segment.Segment.EndX = (_job.Tools[i] as CogDistanceSegmentSegmentTool).SegmentBX;
                        Segment.Segment.EndY = (_job.Tools[i] as CogDistanceSegmentSegmentTool).SegmentBY;

                        point = null;
                        point = new CogPointMarker[1];
                        point[0] = new CogPointMarker();
                        point[0].X = Segment.Segment.StartX;
                        point[0].X = Segment.Segment.StartY;
                        point[0].Color = color;
                        point[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        point[0].GraphicDOFEnable = CogPointMarkerDOFConstants.None;
                        point[0].Interactive = true;
                        point[0].SelectedSpaceName = (_job.Tools[i] as CogDistanceSegmentSegmentTool).InputImage.SelectedSpaceName;
                        point[0].TipText = string.Format("{0}(LineA), CenterX : {1:F3}, CenterY : {2:F3}", (_job.Tools[i] as CogDistanceSegmentSegmentTool).Name, point[0].X, point[0].Y);
                        cogPoint.Add(point[0]);

                        Line = null;
                        Line = new CogLine[1];
                        Line[0] = new CogLine();
                        Line[0] = Segment.Segment.CreateLine();
                        Line[0].Color = color;
                        Line[0].LineWidthInScreenPixels = _graphic[i].nLineThick + 1;
                        Line[0].GraphicDOFEnable = CogLineDOFConstants.None;
                        Line[0].Interactive = true;
                        Line[0].SelectedSpaceName = (_job.Tools[i] as CogDistanceSegmentSegmentTool).InputImage.SelectedSpaceName;
                        Line[0].TipText = string.Format("{0}, X : {1:F3}, Y : {2:F3}", (_job.Tools[i] as CogDistanceSegmentSegmentTool).Name, Line[0].X, Line[0].Y);
                        cogLine.Add(Line[0]);
                    }
                    if (_job.Tools[i].GetType() == typeof(CogCreateGraphicLabelTool))
                    {
                        _GrahpicLabel = null;
                        _GrahpicLabel = new CogCreateGraphicLabelTool();

                        _GrahpicLabel.GetOutputGraphicLabel();

                    }
                }
                else
                    bGraphicRes[i] = true;
            }
        }
        catch (Exception ex) { }
    }
    public void Graphic_Result()
    {
        ICogRegion[] toolsregion = new ICogRegion[_job.Tools.Count];
        string[] regionName = new string[_job.Tools.Count];
        ICogGraphic[] toolgraphic = new ICogGraphic[_job.Tools.Count];
        double[] ToolResultsValue;
        CogPointMarker _marker = new CogPointMarker();

        CogColorConstants[] color = new CogColorConstants[_job.Tools.Count];
        CogColorConstants[] color2 = new CogColorConstants[_job.Tools.Count];
        try
        {
            ToolResultsValue = new double[_DataAnalysisTool.Results.Count];
        }
        catch
        {
            ToolResultsValue = new double[0];
        }
        for (int i = 1; i < _job.Tools.Count; i++)
        {
            if (_graphic[i].bUse)
            {
                if (_graphic[i].nColor == (int)GlovalVar.GraphicColor.Green)
                    color[i - 1] = CogColorConstants.Green;
                else if (_graphic[i].nColor == (int)GlovalVar.GraphicColor.Blue)
                    color[i - 1] = CogColorConstants.Blue;
                else if (_graphic[i].nColor == (int)GlovalVar.GraphicColor.Cyan)
                    color[i - 1] = CogColorConstants.Cyan;
                else if (_graphic[i].nColor == (int)GlovalVar.GraphicColor.Magenta)
                    color[i - 1] = CogColorConstants.Magenta;
                else if (_graphic[i].nColor == (int)GlovalVar.GraphicColor.Red)
                    color[i - 1] = CogColorConstants.Red;
                else if (_graphic[i].nColor == (int)GlovalVar.GraphicColor.Yellow)
                    color[i - 1] = CogColorConstants.Yellow;

                if (_graphic[i].nColor2 == (int)GlovalVar.GraphicColor.Green)
                    color2[i - 1] = CogColorConstants.Green;
                else if (_graphic[i].nColor2 == (int)GlovalVar.GraphicColor.Blue)
                    color2[i - 1] = CogColorConstants.Blue;
                else if (_graphic[i].nColor2 == (int)GlovalVar.GraphicColor.Cyan)
                    color2[i - 1] = CogColorConstants.Cyan;
                else if (_graphic[i].nColor2 == (int)GlovalVar.GraphicColor.Magenta)
                    color2[i - 1] = CogColorConstants.Magenta;
                else if (_graphic[i].nColor2 == (int)GlovalVar.GraphicColor.Red)
                    color2[i - 1] = CogColorConstants.Red;
                else if (_graphic[i].nColor2 == (int)GlovalVar.GraphicColor.Yellow)
                    color2[i - 1] = CogColorConstants.Yellow;
            }
        }
        GetToolRegion(ref toolsregion, ref regionName);      // 각 툴의 검사 영역을 가지고 온다음~

        toolgraphic = GetToolGraphic(toolsregion, regionName, color, color2);     // 각 채널의 결과에 따라 색을 입히고 던지자

        for (int i = 0; i < toolgraphic.Length; i++)
        {
            if (toolgraphic[i] == null)
                toolgraphic[i] = _marker as ICogGraphic;
        }
        if (toolgraphic != null)
        {
            //if (_OnInspRegionGraphic != null)
            //    _OnInspRegionGraphic(toolgraphic);
        }

    }
    private ICogRegion[] GetToolRegion(ref ICogRegion[] _toolregion, ref string[] regionName)
    {
        _toolregion = new ICogRegion[_job.Tools.Count - 1];
        regionName = new string[_job.Tools.Count - 1];

        CogImageConvertTool _Image = new CogImageConvertTool();
        CogIPOneImageTool _IPImage = new CogIPOneImageTool();
        CogBlobTool _blob = new CogBlobTool();
        CogPMAlignTool _pmalign = new CogPMAlignTool();
        CogCaliperTool _caliper = new CogCaliperTool();
        CogFindCircleTool _findcircle = new CogFindCircleTool();
        CogFindLineTool _findline = new CogFindLineTool();
        CogIDTool _ID = new CogIDTool();
        CogOCRMaxTool _OCR = new CogOCRMaxTool();
        CogHistogramTool _jistogram = new CogHistogramTool();
        CogCNLSearchTool _CNLserch = new CogCNLSearchTool();

        try
        {
            for (int i = 1; i < _job.Tools.Count - 1; i++)
            {
                if (_job.Tools[i].GetType() == _Image.GetType())   // 타입 비교~ 1
                {
                    _Image = _job.Tools[i] as CogImageConvertTool;

                    if (_Image.Region != null)
                    {
                        _toolregion[i - 1] = _Image.Region;
                        regionName[i - 1] = _Image.Name;
                    }
                    else
                    {
                        _toolregion[i - 1] = null;
                        regionName[i - 1] = _Image.Name;
                    }
                }
                if (_job.Tools[i].GetType() == _IPImage.GetType())   // 타입 비교~ 1
                {
                    _IPImage = _job.Tools[i] as CogIPOneImageTool;

                    if (_IPImage.Region != null)
                    {
                        _toolregion[i - 1] = _IPImage.Region;
                        regionName[i - 1] = _IPImage.Name;
                    }
                    else
                    {
                        _toolregion[i - 1] = null;
                        regionName[i - 1] = _IPImage.Name;
                    }
                }


                if (_job.Tools[i].GetType() == _blob.GetType())   // 타입 비교~ 1
                {
                    _blob = _job.Tools[i] as CogBlobTool;

                    if (_blob.Region != null)
                    {
                        _toolregion[i - 1] = _blob.Region;
                        regionName[i - 1] = _blob.Name;
                    }
                }

                else if (_job.Tools[i].GetType() == _jistogram.GetType())   // 타입 비교~ 2
                {
                    _jistogram = _job.Tools[i] as CogHistogramTool;

                    if (_jistogram.Region != null)
                    {
                        _toolregion[i - 1] = _jistogram.Region;
                        regionName[i - 1] = _jistogram.Name;
                    }
                }

                else if (_job.Tools[i].GetType() == _findcircle.GetType())   // 타입 비교~ 2
                {
                    _findcircle = _job.Tools[i] as CogFindCircleTool;

                    if (_findcircle.Results != null)
                    {
                        _toolregion[i - 1] = _findcircle.Results.GetCircle() as ICogRegion;
                        regionName[i - 1] = _findcircle.Name;
                    }
                }

                else if (_job.Tools[i].GetType() == _pmalign.GetType())   // 타입 비교~ 3
                {
                    _pmalign = _job.Tools[i] as CogPMAlignTool;

                    if (_pmalign.SearchRegion != null)
                    {
                        _toolregion[i - 1] = _pmalign.SearchRegion;
                        regionName[i - 1] = _pmalign.Name;
                    }
                }

                else if (_job.Tools[i].GetType() == _caliper.GetType())   // 타입 비교~ 4
                {
                    _caliper = _job.Tools[i] as CogCaliperTool;

                    if (_caliper.Region != null)
                    {
                        _toolregion[i - 1] = _caliper.Region;
                        regionName[i - 1] = _caliper.Name;
                    }
                }

                else if (_job.Tools[i].GetType() == _findline.GetType())   // 타입 비교~ 7
                {
                    _findline = _job.Tools[i] as CogFindLineTool;

                    if (_findline.Results != null)
                    {
                        if (_findline.Results.GetLine() != null)
                        {
                            _toolregion[i - 1] = _findline.Results.GetLine() as ICogRegion;
                            regionName[i - 1] = _findline.Name;
                        }

                        else if (_findline.Results.GetLineSegment() != null)
                        {
                            _toolregion[i - 1] = _findline.Results.GetLineSegment() as ICogRegion;
                            regionName[i - 1] = _findline.Name;
                        }
                    }
                }

                else if (_job.Tools[i].GetType() == _ID.GetType())   // 타입 비교~ 10
                {
                    _ID = _job.Tools[i] as CogIDTool;

                    if (_ID.Region != null)
                    {
                        _toolregion[i - 1] = _ID.Region;
                        regionName[i - 1] = _ID.Name;
                    }
                }

                else if (_job.Tools[i].GetType() == _OCR.GetType())   // 타입 비교~ 10
                {
                    _OCR = _job.Tools[i] as CogOCRMaxTool;

                    if (_OCR.Region != null)
                    {
                        _toolregion[i - 1] = _OCR.Region;
                        regionName[i - 1] = _OCR.Name;
                    }
                }

                else if (_job.Tools[i].GetType() == _CNLserch.GetType())   // 타입 비교~ 10
                {
                    _CNLserch = _job.Tools[i] as CogCNLSearchTool;

                    if (_CNLserch.SearchRegion != null)
                    {
                        _toolregion[i - 1] = _CNLserch.SearchRegion;
                        regionName[i - 1] = _CNLserch.Name;
                    }
                }
            }
        }
        catch
        { }

        return _toolregion;
    }
    private ICogGraphic[] GetToolGraphic(ICogRegion[] regions, string[] regionName, CogColorConstants[] _OKColor, CogColorConstants[] _NGColor)
    {
        ICogGraphic[] Toolgraphic = new ICogGraphic[regions.Length];

        int toolcount = _DataAnalysisTool.RunParams.Count;

        try
        {
            for (int i = 0; i < toolcount; i++)     // 데이타 결과 툴의 채널 갯수~
            {
                for (int n = 0; n < regions.Length; n++)   // 영역을 그릴수 있는 툴의 갯수~
                {
                    if (regions[n] != null)
                    {
                        if (_DataAnalysisTool.RunParams[i].Name == regionName[n])    // 채널과 툴의 이름 비교~
                        {
                            Toolgraphic[n] = regions[n] as ICogGraphic;      // 드디어 그림 그릴놈을 만들어주고
                            Toolgraphic[n].LineWidthInScreenPixels = _graphic[n + 1].nLineThick + 1;     // 영역 두께 

                            if (_DataAnalysisTool.Results != null)
                            {
                                if (_DataAnalysisTool.Results[i] != null)
                                {
                                    if (_DataAnalysisTool.Results[i].Pass)      // 결과에 따라 색도 칠하고
                                        Toolgraphic[n].Color = _OKColor[n];
                                    else
                                        Toolgraphic[n].Color = _NGColor[n];
                                }
                            }
                        }
                    }
                }
            }
        }
        catch
        { }
        return Toolgraphic;
    }
    #endregion 
}
