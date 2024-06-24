using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cognex.VisionPro;
using Cognex.VisionPro.ToolGroup;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.Dimensioning;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.Blob;
using Cognex.VisionPro.ColorMatch;
using Cognex.VisionPro.ColorExtractor;
using Cognex.VisionPro.ColorSegmenter;
using Cognex.VisionPro.CompositeColorMatch;
using Cognex.VisionPro.OCRMax;
using Cognex.VisionPro.OCVMax;
using Cognex.VisionPro.LineMax;
using Cognex.VisionPro.ResultsAnalysis;
using Cognex.VisionPro.ID;

using DevExpress.XtraEditors;

public class ToolEdit
{
    public delegate void DragSizeHandler(List<string> list, RegionType regionType);
    public DragSizeHandler OnDragSize;

    public enum ToolList
    {
        CogImageConvertTool,
        CogBlobTool,
        CogCaliperTool,
        CogPMAlignTool,
        CogCreateCircleTool,
        CogCreateEllipseTool,
        CogCreateLineTool,
        CogCreateSegmentAvgSegsTool,
        CogCreateSegmentTool,
        CogFindCircleTool,
        CogFindCornerTool,
        CogFindEllipseTool,
        CogFindLineTool,
        CogIDTool,
        CogOCRMaxTool
    }

    public enum BlobValaueType
    {
        HardFixedThreshold,
        HardRelativeThreshold,
        TailLow,
        TailHigh,
        SoftFixedThresholdLow,
        SoftFixedThresholdHigh,
        Softness
    }

    public enum BlobRunMode
    {
        HardFixedThreshold,
        HardRelativeThreshold,
        HardDynamicThreshold,
        SoftFixedThreshold,
        SoftRelativeThreshold
    }


    public enum ConvertImageRunmode
    {
        Intensity,
        HSI,
        IntensityFromBayer,
        RGBFromBayer,
        HSIFromBayer,
        Plane0,
        Plane1,
        Plane2,
        RGB,
        IntensityFromWeightedRGB,
        PixelFromRange,
        MaskFromRange
    }

    public enum CaliperParam
    {
        Position,
        ContrastThreshold,
        FilterHalfSizeinPixels,
        MasResults
    }

    public enum RegionType
    {
        CogCircle,
        CogEllipse,
        CogPolygon,
        CogRectangle,
        CogRectangleAffine,
        CogCircularAnnulusSection,
        CogEllipticalAnnulusSection,
        None,
        CogLine,
        SegmentA,
        SegmentB
    }

    public void SetConvertRunMode(int nRunMode, CogImageConvertTool cogImageConvertTool)
    {
        if (cogImageConvertTool == null)
            return;

        try
        {
            cogImageConvertTool.RunParams.RunMode = (CogImageConvertRunModeConstants)nRunMode+1;
        }
        catch { }
        
    }

    public int GetConvertRunMode(CogImageConvertTool cogImageConvertTool)
    {
        if (cogImageConvertTool == null)
            return -1;

        int nValue = 0;
        try
        {
            nValue = (int)cogImageConvertTool.RunParams.RunMode-1;
        }
        catch { }

        return nValue;
    }


    public void SetRegion(RegionType regionType, ICogTool cogTool)
    {
        try
        {
            ICogRegion cogRegion = null;
            if (regionType == RegionType.CogCircle)
            {
                var cogCircle = new CogCircle();
                cogCircle.GraphicDOFEnable = CogCircleDOFConstants.All;
                cogCircle.Interactive = true;

                cogRegion = cogCircle;                
            }
            else if (regionType == RegionType.CogEllipse)
            {
                var cogEllipse = new CogEllipse();
                cogEllipse.GraphicDOFEnable = CogEllipseDOFConstants.All;
                cogEllipse.Interactive = true;

                cogRegion = cogEllipse;
            }
            else if (regionType == RegionType.CogPolygon)
            {
                var cogPolygon = new CogPolygon();
                cogPolygon.GraphicDOFEnable = CogPolygonDOFConstants.All;
                cogPolygon.Interactive = true;

                cogPolygon.AddVertex(100, 100, 0);
                cogPolygon.AddVertex(200, 100, 1);
                cogPolygon.AddVertex(200, 200, 2);
                cogPolygon.AddVertex(100, 200, 3);

                cogRegion = cogPolygon;
            }
            else if (regionType == RegionType.CogRectangle)
            {
                var cogRectangle = new CogRectangle();
                cogRectangle.GraphicDOFEnable = CogRectangleDOFConstants.All;
                cogRectangle.Interactive = true;

                cogRegion = cogRectangle;
            }
            else if (regionType == RegionType.CogRectangleAffine)
            {
                var cogRectangleAffine = new CogRectangleAffine();
                cogRectangleAffine.GraphicDOFEnable = CogRectangleAffineDOFConstants.All;
                cogRectangleAffine.Interactive = true;

                cogRegion = cogRectangleAffine;
            }
            else if (regionType == RegionType.CogCircularAnnulusSection)
            {
                var cogCircular = new CogCircularAnnulusSection();
                cogCircular.GraphicDOFEnable = CogCircularAnnulusSectionDOFConstants.All;
                cogCircular.Interactive = true;

                cogRegion = cogCircular;
            }
            else if (regionType == RegionType.CogEllipticalAnnulusSection)
            {
                var cogElliptical = new CogEllipticalAnnulusSection();
                cogElliptical.GraphicDOFEnable = CogEllipticalAnnulusSectionDOFConstants.All;
                cogElliptical.Interactive = true;

                cogRegion = cogElliptical;
            }
            else
                cogRegion = null;


            if (cogTool.GetType() == typeof(CogBlobTool))
                (cogTool as CogBlobTool).Region = cogRegion;
            else if (cogTool.GetType() == typeof(CogPMAlignTool))
                (cogTool as CogPMAlignTool).Pattern.TrainRegion = cogRegion;
            else if (cogTool.GetType() == typeof(CogIDTool))
                (cogTool as CogIDTool).Region = cogRegion;
            else if (cogTool.GetType() == typeof(CogCaliperTool))
            {
                if (cogRegion == null)
                    (cogTool as CogCaliperTool).Region = null;
                else
                    (cogTool as CogCaliperTool).Region = cogRegion as CogRectangleAffine;
            }
        }
        catch { }
    }


    public int GetRegionIndex(ICogRegion cogRegion)
    {
        if (cogRegion == null)
            return 7;

        var nIdx = 7;

        if (cogRegion.GetType() == typeof(CogCircle))
        {
            (cogRegion as CogCircle).Interactive = true;
            (cogRegion as CogCircle).GraphicDOFEnable = CogCircleDOFConstants.All;
            nIdx = (int)RegionType.CogCircle;
        }
        else if (cogRegion.GetType() == typeof(CogEllipse))
        {
            (cogRegion as CogEllipse).Interactive = true;
            (cogRegion as CogEllipse).GraphicDOFEnable = CogEllipseDOFConstants.All;
            nIdx = (int)RegionType.CogEllipse;
        }
        else if (cogRegion.GetType() == typeof(CogPolygon))
        {
            (cogRegion as CogPolygon).Interactive = true;
            (cogRegion as CogPolygon).GraphicDOFEnable = CogPolygonDOFConstants.All;
            nIdx = (int)RegionType.CogPolygon;
        }
        else if (cogRegion.GetType() == typeof(CogRectangleAffine))
        {
            (cogRegion as CogRectangleAffine).Interactive = true;
            (cogRegion as CogRectangleAffine).GraphicDOFEnable = CogRectangleAffineDOFConstants.All;
            nIdx = (int)RegionType.CogRectangleAffine;
        }
        else if (cogRegion.GetType() == typeof(CogRectangle))
        {
            (cogRegion as CogRectangle).Interactive = true;
            (cogRegion as CogRectangle).GraphicDOFEnable = CogRectangleDOFConstants.All;
            nIdx = (int)RegionType.CogRectangle;
        }
        else if (cogRegion.GetType() == typeof(CogCircularAnnulusSection))
        {
            (cogRegion as CogCircularAnnulusSection).Interactive = true;
            (cogRegion as CogCircularAnnulusSection).GraphicDOFEnable = CogCircularAnnulusSectionDOFConstants.All;
            nIdx = (int)RegionType.CogCircularAnnulusSection;
        }
        else if (cogRegion.GetType() == typeof(CogEllipticalAnnulusSection))
        {
            (cogRegion as CogEllipticalAnnulusSection).Interactive = true;
            (cogRegion as CogEllipticalAnnulusSection).GraphicDOFEnable = CogEllipticalAnnulusSectionDOFConstants.All;

            nIdx = (int)RegionType.CogEllipticalAnnulusSection;
        }
            

        return nIdx;
    }

    public List<string> GetResionSize(ICogRegion region)
    {
        if (region == null)
            return null;

        var listValue = new List<string>();

        try
        {
            if (region.GetType() == typeof(CogCircle))
            {
                var circle = region as CogCircle;
                listValue.Add(string.Format("중심X :,{0:F3}", circle.CenterX));
                listValue.Add(string.Format("중심Y :,{0:F3}", circle.CenterY));
                listValue.Add(string.Format("반지름 :,{0:F3}", RadToAngle(circle.Radius)));
                //circle

            }
            else if (region.GetType() == typeof(CogEllipse))
            {
                var ellipse = region as CogEllipse;
                listValue.Add(string.Format("중심X :,{0:F3}", ellipse.CenterX));
                listValue.Add(string.Format("중심Y :,{0:F3}", ellipse.CenterY));
                listValue.Add(string.Format("반지름X :,{0:F3}", ellipse.RadiusX));
                listValue.Add(string.Format("반지름Y :,{0:F3}", ellipse.RadiusY));
                listValue.Add(string.Format("회전 :,{0:F3}", RadToAngle(ellipse.Rotation)));
            }
            else if (region.GetType() == typeof(CogPolygon))
            {
                var polygon = region as CogPolygon;
                var nCnt = polygon.NumVertices;

                for (int i = 0; i < nCnt; i++)
                {
                    polygon.GetVertex(i, out var dPosX, out var dPosY);
                    listValue.Add(string.Format("{0:F3},{1:F3}", dPosX, dPosY));
                }
            }
            else if (region.GetType() == typeof(CogRectangle))
            {
                var Rect = region as CogRectangle;
                listValue.Add(string.Format("원점X :,{0:F3}", Rect.X));
                listValue.Add(string.Format("원점Y :,{0:F3}", Rect.Y));
                listValue.Add(string.Format("너비 :,{0:F3}", Rect.Width));
                listValue.Add(string.Format("너비 :,{0:F3}", Rect.Height));
            }
            else if (region.GetType() == typeof(CogRectangleAffine))
            {
                var RectangleAffine = region as CogRectangleAffine;
                listValue.Add(string.Format("중심X :,{0:F3}", RectangleAffine.CornerOriginX));
                listValue.Add(string.Format("중심Y :,{0:F3}", RectangleAffine.CornerOriginY));
                listValue.Add(string.Format("길이X :,{0:F3}", RectangleAffine.SideXLength));
                listValue.Add(string.Format("길이Y :,{0:F3}", RectangleAffine.SideYLength));
                listValue.Add(string.Format("회전 :,{0:F3}", RadToAngle(RectangleAffine.Rotation)));
                listValue.Add(string.Format("기울이기 :,{0:F3}", RadToAngle(RectangleAffine.Skew)));
            }
            else if (region.GetType() == typeof(CogCircularAnnulusSection))
            {
                var Circular = region as CogCircularAnnulusSection;
                listValue.Add(string.Format("중심X :,{0:F3}", Circular.CenterX));
                listValue.Add(string.Format("중심Y :,{0:F3}", Circular.CenterY));
                listValue.Add(string.Format("반지름 :,{0:F3}", Circular.Radius));
                listValue.Add(string.Format("방시 비율 :,{0:F3}", Circular.RadialScale));
                listValue.Add(string.Format("각도 시작 :,{0:F3}", RadToAngle(Circular.AngleStart)));
                listValue.Add(string.Format("각도 확장 :,{0:F3}", RadToAngle(Circular.AngleSpan)));

            }
            else if (region.GetType() == typeof(CogEllipticalAnnulusSection))
            {
                var Elliptical = region as CogEllipticalAnnulusSection;
                listValue.Add(string.Format("중심X :,{0:F3}", Elliptical.CenterX));
                listValue.Add(string.Format("중심Y :,{0:F3}", Elliptical.CenterY));
                listValue.Add(string.Format("반지름X :,{0:F3}", Elliptical.RadiusX));
                listValue.Add(string.Format("반지름Y :,{0:F3}", Elliptical.RadiusY));
                listValue.Add(string.Format("방사 비율 :,{0:F3}", Elliptical.RadialScale));
                listValue.Add(string.Format("회전 :,{0:F3}", RadToAngle(Elliptical.Rotation)));
                listValue.Add(string.Format("각도 시작 :,{0:F3}",  RadToAngle(Elliptical.AngleStart)));
                listValue.Add(string.Format("각도 확장 :,{0:F3}", RadToAngle(Elliptical.AngleSpan))) ;
            }
        }
        catch (Exception ex) 
        {
            return null;
        }
        
        return listValue;
    }



    public string[] GetBlobFilter(CogBlobTool cogBlobTool)
    {
        if (cogBlobTool == null)
            return null;

        var strValue = new string[4];
        var nCnt = cogBlobTool.RunParams.RunTimeMeasures.Count;

        foreach(CogBlobMeasure Measure in cogBlobTool.RunParams.RunTimeMeasures)
        {
            if (Measure.Measure == CogBlobMeasureConstants.Area)
            {
                strValue[0] = Convert.ToString((int)Measure.Mode);
                strValue[1] = Convert.ToString((int)Measure.FilterMode);
                strValue[2] = string.Format("{0}" ,Measure.FilterRangeLow);
                strValue[3] = string.Format("{0}", Measure.FilterRangeHigh);
            }
        }

        return strValue;
    }


    public void SetRegionSize(int nIdx, double[] dValue, ICogTool cogTool)
    {
        ICogRegion Region = null;

        if (cogTool.GetType() == typeof(CogBlobTool))
            Region = (cogTool as CogBlobTool).Region;
        else if (cogTool.GetType() == typeof(CogPMAlignTool))
            Region = (cogTool as CogPMAlignTool).Pattern.TrainRegion;
        else if (cogTool.GetType() == typeof(CogCreateCircleTool))
            Region = (cogTool as CogCreateCircleTool).InputCircle;
        else if (cogTool.GetType() == typeof(CogCreateEllipseTool))
            Region = (cogTool as CogCreateEllipseTool).Ellipse;
        else if (cogTool.GetType() == typeof(CogCreateLineTool))
        {
            (cogTool as CogCreateLineTool).Line.X = dValue[0];
            (cogTool as CogCreateLineTool).Line.Y = dValue[1];
            (cogTool as CogCreateLineTool).Line.Rotation = AngleToRad(dValue[2]);

            return;
        }
        else if (cogTool.GetType() == typeof(CogIDTool))
            Region = (cogTool as CogIDTool).Region;

        if (Region.GetType() == typeof(CogCircle))
        {
            (Region as CogCircle).SetCenterRadius(dValue[0], dValue[1], dValue[2]);
        }
        else if (Region.GetType() == typeof(CogEllipse))
        {
            (Region as CogEllipse).SetCenterXYRadiusXYRotation(dValue[0], dValue[1], dValue[2], dValue[3], AngleToRad(dValue[4]));
        }
        else if (Region.GetType() == typeof(CogPolygon))
        {
            if (dValue == null)
            {
                if ((Region as CogPolygon).NumVertices >= nIdx)
                    (Region as CogPolygon).RemoveVertex(nIdx);
            }
            else
                (Region as CogPolygon).AddVertex(dValue[0], dValue[1], nIdx);
        }
        else if (Region.GetType() == typeof(CogRectangle))
        {
            (Region as CogRectangle).SetCenterWidthHeight(dValue[0], dValue[1], dValue[2], dValue[3]);
        }
        else if (Region.GetType() == typeof(CogRectangleAffine))
        {
            (Region as CogRectangleAffine).SetCenterLengthsRotationSkew(dValue[0], dValue[1], dValue[2], dValue[3], AngleToRad(dValue[4]), AngleToRad(dValue[5]));
        }
        else if (Region.GetType() == typeof(CogCircularAnnulusSection))
        {
            (Region as CogCircularAnnulusSection).SetCenterRadiusAngleStartAngleSpanRadialScale(dValue[0], dValue[1], dValue[2], dValue[3], AngleToRad(dValue[4]), AngleToRad(dValue[5]));
        }
        else if (Region.GetType() == typeof(CogEllipticalAnnulusSection))
        {
            (Region as CogEllipticalAnnulusSection).SetCenterRadiusXYRotationAngleStartAngleSpanRadialScale(dValue[0], dValue[1], dValue[2], dValue[3], dValue[4], dValue[5], dValue[6], dValue[7]);
        }
    }


    public void SetCaliperParam(double dValue, CogCaliperTool cogCaliperTool, CaliperParam caliperParam)
    {
        if (cogCaliperTool == null)
            return;

        try
        {
            if (caliperParam == CaliperParam.Position)
            {
                cogCaliperTool.RunParams.Edge0Position = (dValue / 2.0) * -1.0;
                cogCaliperTool.RunParams.Edge0Position = dValue / 2.0;
            }
            else if (caliperParam == CaliperParam.ContrastThreshold)
            {
                cogCaliperTool.RunParams.ContrastThreshold = dValue;
            }
            else if (caliperParam == CaliperParam.FilterHalfSizeinPixels)
            {
                cogCaliperTool.RunParams.FilterHalfSizeInPixels = (int)dValue;
            }
            else if (caliperParam == CaliperParam.MasResults)
            {
                cogCaliperTool.RunParams.MaxResults = (int)dValue;
            }
        }
        catch { }
        
    }


    public void SetPMalingOrigin(CogPMAlignTool cogPMAlignTool)
    {
        if (cogPMAlignTool == null)
            return;

        try
        {
            ICogRegion cogRegion = cogPMAlignTool.Pattern.TrainRegion;

            if (cogPMAlignTool.Pattern.TrainRegion.GetType() == typeof(CogCircle))
            {
                CogCircle cogCircle = cogRegion as CogCircle;
                cogPMAlignTool.Pattern.Origin.TranslationX = cogCircle.CenterX;
                cogPMAlignTool.Pattern.Origin.TranslationY = cogCircle.CenterY;
            }
            else if (cogPMAlignTool.Pattern.TrainRegion.GetType() == typeof(CogEllipse))
            {
                CogEllipse cogEllipse = cogRegion as CogEllipse;
                cogPMAlignTool.Pattern.Origin.TranslationX = cogEllipse.CenterX;
                cogPMAlignTool.Pattern.Origin.TranslationY = cogEllipse.CenterY;
            }
            else if (cogPMAlignTool.Pattern.TrainRegion.GetType() == typeof(CogPolygon))
            {

                CogPolygon cogPolygon = cogRegion as CogPolygon;
                cogPolygon.Interactive = true;
                cogPolygon.GraphicDOFEnable = CogPolygonDOFConstants.All;
                cogPolygon.GraphicDOFEnableBase = CogGraphicDOFConstants.All;

                double dX, dY;
                cogPolygon.AreaCenter(out dX, out dY);
                cogPMAlignTool.Pattern.Origin.TranslationX = dX;
                cogPMAlignTool.Pattern.Origin.TranslationY = dY;
            }
            else if (cogPMAlignTool.Pattern.TrainRegion.GetType() == typeof(CogRectangleAffine))
            {
                CogRectangleAffine cogRectangleAffine = cogRegion as CogRectangleAffine;
                cogPMAlignTool.Pattern.Origin.TranslationX = cogRectangleAffine.CenterX;
                cogPMAlignTool.Pattern.Origin.TranslationY = cogRectangleAffine.CenterY;
            }

            else if (cogPMAlignTool.Pattern.TrainRegion.GetType() == typeof(CogRectangle))
            {
                CogRectangle cogRectangle = cogRegion as CogRectangle;
                cogPMAlignTool.Pattern.Origin.TranslationX = cogRectangle.CenterX;
                cogPMAlignTool.Pattern.Origin.TranslationY = cogRectangle.CenterY;
            }

            else if (cogPMAlignTool.Pattern.TrainRegion.GetType() == typeof(CogCircularAnnulusSection))
            {
                CogCircularAnnulusSection cogCircularAnnulusSection = cogRegion as CogCircularAnnulusSection;
                cogCircularAnnulusSection.Interactive = true;
                cogCircularAnnulusSection.GraphicDOFEnable = CogCircularAnnulusSectionDOFConstants.All;
                cogCircularAnnulusSection.GraphicDOFEnableBase = CogGraphicDOFConstants.All;
                cogPMAlignTool.Pattern.Origin.TranslationX = cogCircularAnnulusSection.CenterX;
                cogPMAlignTool.Pattern.Origin.TranslationY = cogCircularAnnulusSection.CenterY;
            }

            else if (cogPMAlignTool.Pattern.TrainRegion.GetType() == typeof(CogEllipticalAnnulusSection))
            {
                CogEllipticalAnnulusSection cogEllipticalAnnulusSection = cogRegion as CogEllipticalAnnulusSection;
                cogEllipticalAnnulusSection.Interactive = true;
                cogEllipticalAnnulusSection.GraphicDOFEnable = CogEllipticalAnnulusSectionDOFConstants.All;
                cogEllipticalAnnulusSection.GraphicDOFEnableBase = CogGraphicDOFConstants.All;
                cogPMAlignTool.Pattern.Origin.TranslationX = cogEllipticalAnnulusSection.CenterX;
                cogPMAlignTool.Pattern.Origin.TranslationY = cogEllipticalAnnulusSection.CenterY;
            }
        }
        catch { }
    }


    public void GetFindCircleParam(out int nCaliperCnt, out double dSearchLen, out double dProjectionLen, out int nSearchDirection, out int nEdgeMode, out int nPolarity1, out int nPolarity2, out double dThreshold, out int nFilterPixer, out bool bTolgnore, out int nTolgnoreCnt,  out double dEdgeLen, CogFindCircleTool cogFindCircleTool)
    {
        nCaliperCnt = 0;
        dSearchLen = 0;
        dProjectionLen = 0;
        nSearchDirection = 0;
        nEdgeMode = 1;
        nPolarity1 = 0;
        nPolarity2 = 0;
        dThreshold = 0;
        nFilterPixer = 0;
        bTolgnore = false;
        nTolgnoreCnt = 0;
        dEdgeLen = 0;
        
        try
        {
            nCaliperCnt = cogFindCircleTool.RunParams.NumCalipers;
            dSearchLen = cogFindCircleTool.RunParams.CaliperSearchLength;
            dProjectionLen = cogFindCircleTool.RunParams.CaliperProjectionLength;
            nSearchDirection = (int)cogFindCircleTool.RunParams.CaliperSearchDirection;
            nEdgeMode = (int)cogFindCircleTool.RunParams.CaliperRunParams.EdgeMode;
            nPolarity1 = (int)cogFindCircleTool.RunParams.CaliperRunParams.Edge0Polarity;
            nPolarity2 = (int)cogFindCircleTool.RunParams.CaliperRunParams.Edge1Polarity;
            dThreshold = cogFindCircleTool.RunParams.CaliperRunParams.ContrastThreshold;
            nFilterPixer = cogFindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels;
            bTolgnore = cogFindCircleTool.RunParams.DecrementNumToIgnore;
            nTolgnoreCnt = cogFindCircleTool.RunParams.NumToIgnore;
            dEdgeLen = cogFindCircleTool.RunParams.CaliperRunParams.Edge1Position - cogFindCircleTool.RunParams.CaliperRunParams.Edge0Position;
        }
        catch { }
    }

    public enum FindCircleParam
    {
        CaliperCnt,
        SearchLen,
        ProjectionLen,
        SearchDirection,
        EdgeMode,
        Polarity1,
        Polarity2,
        Threshold,
        FilterPixel,
        Tolgnore,
        TolgnoreCnt,
        EdgeLen
    }
    public void SetFindCircleParam(FindCircleParam nType, string strValue, CogFindCircleTool cogFindCircleTool)
    {
        try
        {
            if (nType == FindCircleParam.CaliperCnt)
                cogFindCircleTool.RunParams.NumCalipers = Convert.ToInt32(strValue);
            else if (nType == FindCircleParam.SearchLen)
                cogFindCircleTool.RunParams.CaliperSearchLength = Convert.ToDouble(strValue);
            else if (nType == FindCircleParam.ProjectionLen)
                cogFindCircleTool.RunParams.CaliperProjectionLength = Convert.ToDouble(strValue);
            else if (nType == FindCircleParam.SearchDirection)
                cogFindCircleTool.RunParams.CaliperSearchDirection = (CogFindCircleSearchDirectionConstants)Convert.ToInt32(strValue);
            else if (nType == FindCircleParam.EdgeMode)
                cogFindCircleTool.RunParams.CaliperRunParams.EdgeMode = (CogCaliperEdgeModeConstants)(int)Convert.ToInt32(strValue);
            else if (nType == FindCircleParam.Polarity1)
                cogFindCircleTool.RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)(int)Convert.ToInt32(strValue);
            else if (nType == FindCircleParam.Polarity2)
                cogFindCircleTool.RunParams.CaliperRunParams.Edge1Polarity = (CogCaliperPolarityConstants)(int)Convert.ToInt32(strValue);
            else if (nType == FindCircleParam.Threshold)
                cogFindCircleTool.RunParams.CaliperRunParams.ContrastThreshold = (double)Convert.ToDouble(strValue);
            else if (nType == FindCircleParam.FilterPixel)
                cogFindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = (int)Convert.ToInt32(strValue);
            else if (nType == FindCircleParam.Tolgnore)
                cogFindCircleTool.RunParams.DecrementNumToIgnore = (bool)Convert.ToBoolean(strValue);
            else if (nType == FindCircleParam.TolgnoreCnt)
                cogFindCircleTool.RunParams.NumToIgnore = (int)Convert.ToInt32(strValue);
            else if (nType == FindCircleParam.EdgeLen)
            {
                cogFindCircleTool.RunParams.CaliperRunParams.Edge1Position = (double)Convert.ToDouble(strValue) / 2.0;
                cogFindCircleTool.RunParams.CaliperRunParams.Edge0Position = (double)Convert.ToDouble(strValue) / 2.0 * -1.0;
            }
        }
        catch { }
    }

    public enum FindCornerParam
    {
        CaliperCnt,
        SearchLen,
        ProjectionLen,
        SearchDirection,
        EdgeMode,
        Polarity1,
        Polarity2,
        Threshold,
        FilterPixel,
        EdgeLen,
        lonore,
        lgonreCnt
    }
    public void GetFindCornerParam(out int nCaliperCnt, out double dSearchLen, out double dProjectionLen, out double dSearchDirection, out int nEdgeMode, out int nPolarity1, out int nPolarity2, out double dThreshold, out int nFilterPixer, out double dEdgeLen, out bool blgnore, out int nlgnoreCnt, CogFindCornerTool cogFindCorner)
    {
        nCaliperCnt = 0;
        dSearchLen = 0;
        dProjectionLen = 0;
        dSearchDirection = 0;
        nEdgeMode = 1;
        nPolarity1 = 0;
        nPolarity2 = 0;
        dThreshold = 0;
        nFilterPixer = 0;
        dEdgeLen = 0;
        blgnore = false;
        nlgnoreCnt = 0;

        try
        {
            nCaliperCnt = cogFindCorner.RunParams.NumCalipers;
            dSearchLen = cogFindCorner.RunParams.CaliperSearchLength;
            dProjectionLen = cogFindCorner.RunParams.CaliperProjectionLength;
            dSearchDirection = RadToAngle(cogFindCorner.RunParams.CaliperSearchDirection);
            nEdgeMode = (int)cogFindCorner.RunParams.CaliperRunParams.EdgeMode;
            nPolarity1 = (int)cogFindCorner.RunParams.CaliperRunParams.Edge0Polarity;
            nPolarity2 = (int)cogFindCorner.RunParams.CaliperRunParams.Edge1Polarity;
            dThreshold = cogFindCorner.RunParams.CaliperRunParams.ContrastThreshold;
            nFilterPixer = cogFindCorner.RunParams.CaliperRunParams.FilterHalfSizeInPixels;
            dEdgeLen = cogFindCorner.RunParams.CaliperRunParams.Edge1Position - cogFindCorner.RunParams.CaliperRunParams.Edge0Position;
            blgnore = cogFindCorner.RunParams.DecrementNumToIgnore;
            nlgnoreCnt = cogFindCorner.RunParams.NumToIgnore;
        }
        catch { }
    }

    public void SetFineCornerParam(FindCornerParam nType, string strValue, CogFindCornerTool cogFindCornerTool)
    {
        try
        {
            if (nType == FindCornerParam.CaliperCnt)
                cogFindCornerTool.RunParams.NumCalipers = int.Parse(strValue);
            else if (nType == FindCornerParam.SearchLen)
                cogFindCornerTool.RunParams.CaliperSearchLength = double.Parse(strValue);
            else if (nType == FindCornerParam.ProjectionLen)
                cogFindCornerTool.RunParams.CaliperProjectionLength = double.Parse(strValue);
            else if (nType == FindCornerParam.SearchDirection)
                cogFindCornerTool.RunParams.CaliperSearchDirection = double.Parse(strValue);
            else if (nType == FindCornerParam.EdgeMode)
                cogFindCornerTool.RunParams.CaliperRunParams.EdgeMode = (CogCaliperEdgeModeConstants)int.Parse(strValue);
            else if (nType == FindCornerParam.Polarity1)
                cogFindCornerTool.RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)int.Parse(strValue);
            else if (nType == FindCornerParam.Polarity2)
                cogFindCornerTool.RunParams.CaliperRunParams.Edge1Polarity = (CogCaliperPolarityConstants)(int)int.Parse(strValue);
            else if (nType == FindCornerParam.Threshold)
                cogFindCornerTool.RunParams.CaliperRunParams.ContrastThreshold = double.Parse(strValue);
            else if (nType == FindCornerParam.FilterPixel)
                cogFindCornerTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = int.Parse(strValue);
            else if (nType == FindCornerParam.EdgeLen)
            {
                cogFindCornerTool.RunParams.CaliperRunParams.Edge1Position = double.Parse(strValue) / 2.0;
                cogFindCornerTool.RunParams.CaliperRunParams.Edge0Position = double.Parse(strValue) / 2.0 * -1.0;
            }
            else if (nType == FindCornerParam.lonore)
                cogFindCornerTool.RunParams.DecrementNumToIgnore = bool.Parse(strValue);
            else if (nType == FindCornerParam.lgonreCnt)
                cogFindCornerTool.RunParams.NumToIgnore = int.Parse(strValue);
        }
        catch { }
    }

    public enum FindEllipseParam
    {
        CaliperCnt,
        SearchLen,
        ProjectionLen,
        SearchDirection,
        Tolgnore,
        TolonoreCnt,
        EdgeMode,
        Polarity1,
        Polarity2,
        EdgeLen,
        Threshold,
        FilterPixel,
    }

    public void GetFindEllipseParam(out int nCaliperCnt, out double dSearchLen, out double dProjectionLen, out int nSearchDirection, out int nEdgeMode, out int nPolarity1, out int nPolarity2, out double dThreshold, out int nFilterPixer, out double dEdgeLen, out bool bTolgnore, out int nTolgnoreCnt, CogFindEllipseTool cogFindEllipseTool)
    {
        nCaliperCnt = 0;
        dSearchLen = 0;
        dProjectionLen = 0;
        nSearchDirection = 0;
        nEdgeMode = 1;
        nPolarity1 = 0;
        nPolarity2 = 0;
        dThreshold = 0;
        nFilterPixer = 0;
        dEdgeLen = 0;
        bTolgnore = false;
        nTolgnoreCnt = 0;

        try
        {
            nCaliperCnt = cogFindEllipseTool.RunParams.NumCalipers;
            dSearchLen = cogFindEllipseTool.RunParams.CaliperSearchLength;
            dProjectionLen = cogFindEllipseTool.RunParams.CaliperProjectionLength;
            nSearchDirection = (int)cogFindEllipseTool.RunParams.CaliperSearchDirection;
            nEdgeMode = (int)cogFindEllipseTool.RunParams.CaliperRunParams.EdgeMode;
            nPolarity1 = (int)cogFindEllipseTool.RunParams.CaliperRunParams.Edge0Polarity;
            nPolarity2 = (int)cogFindEllipseTool.RunParams.CaliperRunParams.Edge1Polarity;
            dThreshold = cogFindEllipseTool.RunParams.CaliperRunParams.ContrastThreshold;
            nFilterPixer = cogFindEllipseTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels;
            dEdgeLen = cogFindEllipseTool.RunParams.CaliperRunParams.Edge1Position - cogFindEllipseTool.RunParams.CaliperRunParams.Edge0Position;
            bTolgnore = cogFindEllipseTool.RunParams.DecrementNumToIgnore;
            nTolgnoreCnt = cogFindEllipseTool.RunParams.NumToIgnore;
        }
        catch { }
    }

    public void SetFindEllipseParam(FindEllipseParam nType, string strValue, CogFindEllipseTool cogFindEllipseTool)
    {
        try
        {
            if (nType == FindEllipseParam.CaliperCnt)
                cogFindEllipseTool.RunParams.NumCalipers = int.Parse(strValue);
            else if (nType == FindEllipseParam.SearchLen)
                cogFindEllipseTool.RunParams.CaliperSearchLength = double.Parse(strValue);
            else if (nType == FindEllipseParam.ProjectionLen)
                cogFindEllipseTool.RunParams.CaliperProjectionLength = double.Parse(strValue);
            else if (nType == FindEllipseParam.SearchDirection)
                cogFindEllipseTool.RunParams.CaliperSearchDirection = (CogFindEllipseSearchDirectionConstants)int.Parse(strValue);
            else if (nType == FindEllipseParam.EdgeMode)
                cogFindEllipseTool.RunParams.CaliperRunParams.EdgeMode = (CogCaliperEdgeModeConstants)int.Parse(strValue);
            else if (nType == FindEllipseParam.Polarity1)
                cogFindEllipseTool.RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)int.Parse(strValue);
            else if (nType == FindEllipseParam.Polarity2)
                cogFindEllipseTool.RunParams.CaliperRunParams.Edge1Polarity = (CogCaliperPolarityConstants)int.Parse(strValue);
            else if (nType == FindEllipseParam.Threshold)
                cogFindEllipseTool.RunParams.CaliperRunParams.ContrastThreshold = double.Parse(strValue);
            else if (nType == FindEllipseParam.FilterPixel)
                cogFindEllipseTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = int.Parse(strValue);
            else if (nType == FindEllipseParam.Tolgnore)
                cogFindEllipseTool.RunParams.DecrementNumToIgnore = bool.Parse(strValue);
            else if (nType == FindEllipseParam.TolonoreCnt)
                cogFindEllipseTool.RunParams.NumToIgnore = int.Parse(strValue);
            else if (nType == FindEllipseParam.EdgeLen)
            {
                cogFindEllipseTool.RunParams.CaliperRunParams.Edge1Position = double.Parse(strValue) / 2.0;
                cogFindEllipseTool.RunParams.CaliperRunParams.Edge0Position = double.Parse(strValue) / 2.0 * -1.0;
            }
        }
        catch { }
    }

    public enum FindLineParam
    {
        CaliperCnt,
        SearchLen,
        ProjectionLen,
        SearchDirection,
        Tolgnore,
        TolonoreCnt,
        EdgeMode,
        Polarity1,
        Polarity2,
        EdgeLen,
        Threshold,
        FilterPixel,
    }

    public void GetFindLineParam(out int nCaliperCnt, out double dSearchLen, out double dProjectionLen, out double dSearchDirection, out int nEdgeMode, out int nPolarity1, out int nPolarity2, out double dThreshold, out int nFilterPixer, out double dEdgeLen, out bool bTolgnore, out int nTolgnoreCnt, CogFindLineTool cogFindLineTool)
    {
        nCaliperCnt = 0;
        dSearchLen = 0;
        dProjectionLen = 0;
        dSearchDirection = 0;
        nEdgeMode = 1;
        nPolarity1 = 0;
        nPolarity2 = 0;
        dThreshold = 0;
        nFilterPixer = 0;
        dEdgeLen = 0;
        bTolgnore = false;
        nTolgnoreCnt = 0;

        try
        {
            nCaliperCnt = cogFindLineTool.RunParams.NumCalipers;
            dSearchLen = cogFindLineTool.RunParams.CaliperSearchLength;
            dProjectionLen = cogFindLineTool.RunParams.CaliperProjectionLength;
            dSearchDirection = RadToAngle(cogFindLineTool.RunParams.CaliperSearchDirection);
            nEdgeMode = (int)cogFindLineTool.RunParams.CaliperRunParams.EdgeMode;
            nPolarity1 = (int)cogFindLineTool.RunParams.CaliperRunParams.Edge0Polarity;
            nPolarity2 = (int)cogFindLineTool.RunParams.CaliperRunParams.Edge1Polarity;
            dThreshold = cogFindLineTool.RunParams.CaliperRunParams.ContrastThreshold;
            nFilterPixer = cogFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels;
            dEdgeLen = cogFindLineTool.RunParams.CaliperRunParams.Edge1Position - cogFindLineTool.RunParams.CaliperRunParams.Edge0Position;
            bTolgnore = cogFindLineTool.RunParams.DecrementNumToIgnore;
            nTolgnoreCnt = cogFindLineTool.RunParams.NumToIgnore;
        }
        catch { }
    }

    public void SetFindLineParam(FindLineParam nType, string strValue, CogFindLineTool cogFindLineTool)
    {
        try
        {
            if (nType == FindLineParam.CaliperCnt)
                cogFindLineTool.RunParams.NumCalipers = int.Parse(strValue);
            else if (nType == FindLineParam.SearchLen)
                cogFindLineTool.RunParams.CaliperSearchLength = double.Parse(strValue);
            else if (nType == FindLineParam.ProjectionLen)
                cogFindLineTool.RunParams.CaliperProjectionLength = double.Parse(strValue);
            else if (nType == FindLineParam.SearchDirection)
                cogFindLineTool.RunParams.CaliperSearchLength = double.Parse(strValue);
            else if (nType == FindLineParam.EdgeMode)
                cogFindLineTool.RunParams.CaliperRunParams.EdgeMode = (CogCaliperEdgeModeConstants)double.Parse(strValue);
            else if (nType == FindLineParam.Polarity1)
                cogFindLineTool.RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)double.Parse(strValue);
            else if (nType == FindLineParam.Polarity2)
                cogFindLineTool.RunParams.CaliperRunParams.Edge1Polarity = (CogCaliperPolarityConstants)double.Parse(strValue);
            else if (nType == FindLineParam.Threshold)
                cogFindLineTool.RunParams.CaliperRunParams.ContrastThreshold = double.Parse(strValue);
            else if (nType == FindLineParam.FilterPixel)
                cogFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = int.Parse(strValue);
            else if (nType == FindLineParam.Tolgnore)
                cogFindLineTool.RunParams.DecrementNumToIgnore = bool.Parse(strValue);
            else if (nType == FindLineParam.TolonoreCnt)
                cogFindLineTool.RunParams.NumToIgnore = int.Parse(strValue);
            else if (nType == FindLineParam.EdgeLen)
            {
                cogFindLineTool.RunParams.CaliperRunParams.Edge1Position = double.Parse(strValue) / 2.0;
                cogFindLineTool.RunParams.CaliperRunParams.Edge0Position = double.Parse(strValue) / 2.0 * -1.0;
            }
        }
        catch { }
    }
    public double RadToAngle(double Rad)
    {
        return (Rad * 180.0) / Math.PI;
    }

    public double AngleToRad(double Angle)
    {
        return (Angle * Math.PI) / 180;
    }

    public void SetDragMode(ICogRegion cogRegion)
    {
        try
        {
            if (cogRegion == null)
                return;

            if (cogRegion.GetType() == typeof(CogCircle))
            {
                (cogRegion as CogCircle).DraggingStopped -= new CogDraggingStoppedEventHandler(OnDraggingStop);
                (cogRegion as CogCircle).DraggingStopped += new CogDraggingStoppedEventHandler(OnDraggingStop);
            }
            else if (cogRegion.GetType() == typeof(CogEllipse))
            {
                (cogRegion as CogEllipse).DraggingStopped -= new CogDraggingStoppedEventHandler(OnDraggingStop);
                (cogRegion as CogEllipse).DraggingStopped += new CogDraggingStoppedEventHandler(OnDraggingStop);
            }

            else if (cogRegion.GetType() == typeof(CogPolygon))
            {
                (cogRegion as CogPolygon).DraggingStopped -= new CogDraggingStoppedEventHandler(OnDraggingStop);
                (cogRegion as CogPolygon).DraggingStopped += new CogDraggingStoppedEventHandler(OnDraggingStop);
            }

            else if (cogRegion.GetType() == typeof(CogRectangle))
            {
                (cogRegion as CogRectangle).DraggingStopped -= new CogDraggingStoppedEventHandler(OnDraggingStop);
                (cogRegion as CogRectangle).DraggingStopped += new CogDraggingStoppedEventHandler(OnDraggingStop);
            }

            else if (cogRegion.GetType() == typeof(CogRectangleAffine))
            {
                (cogRegion as CogRectangleAffine).DraggingStopped -= new CogDraggingStoppedEventHandler(OnDraggingStop);
                (cogRegion as CogRectangleAffine).DraggingStopped += new CogDraggingStoppedEventHandler(OnDraggingStop);
            }

            else if (cogRegion.GetType() == typeof(CogCircularAnnulusSection))
            {
                (cogRegion as CogCircularAnnulusSection).DraggingStopped -= new CogDraggingStoppedEventHandler(OnDraggingStop);
                (cogRegion as CogCircularAnnulusSection).DraggingStopped += new CogDraggingStoppedEventHandler(OnDraggingStop);
            }

            else if (cogRegion.GetType() == typeof(CogEllipticalAnnulusSection))
            {
                (cogRegion as CogEllipticalAnnulusSection).DraggingStopped -= new CogDraggingStoppedEventHandler(OnDraggingStop);
                (cogRegion as CogEllipticalAnnulusSection).DraggingStopped += new CogDraggingStoppedEventHandler(OnDraggingStop);
            }
        }
        catch { }
    }

    public void SetToolDragMode(ICogTool cogTool)
    {
        try
        {
            if (cogTool.GetType() == typeof(CogCreateLineTool))
            {
                (cogTool as CogCreateLineTool).Line.DraggingStopped -= new CogDraggingStoppedEventHandler(OnToolDraggingStop);
                (cogTool as CogCreateLineTool).Line.DraggingStopped += new CogDraggingStoppedEventHandler(OnToolDraggingStop);
            }
        }
        catch { }
    }

    private void OnToolDraggingStop(object sender, CogDraggingEventArgs e)
    {
        try
        {
            if (e.DragGraphic.GetType() == typeof(CogLine))
            {
                var cogLine = e.DragGraphic as CogLine;

                var list = new List<string>();
                list.Add(string.Format("X : ,{0:F3}", cogLine.X));
                list.Add(string.Format("Y : ,{0:F3}", cogLine.Y));
                list.Add(string.Format("회전 : ,{0:F3}", RadToAngle(cogLine.Rotation)));

                if (OnDragSize != null)
                    OnDragSize(list, RegionType.CogLine);
            }
        }
        catch { }

    }

    private void OnDraggingStop(object sender, CogDraggingEventArgs e)
    {
        try
        {
            var listValue = new List<string>();
            RegionType regionType = RegionType.None;

            if (e.DragGraphic.GetType() == typeof(CogCircle))
            {
                var cogCircle = e.DragGraphic as CogCircle;
                listValue = GetResionSize(cogCircle);
                regionType = RegionType.CogCircle;
            }
            else if (e.DragGraphic.GetType() == typeof(CogEllipse))
            {
                var cogEllipse = e.DragGraphic as CogEllipse;
                listValue = GetResionSize(cogEllipse);
                regionType = RegionType.CogEllipse;
            }
            else if (e.DragGraphic.GetType() == typeof(CogPolygon))
            {
                var cogPolygon = e.DragGraphic as CogPolygon;
                listValue = GetResionSize(cogPolygon);
                regionType = RegionType.CogPolygon;
            }
            else if (e.DragGraphic.GetType() == typeof(CogRectangle))
            {
                var cogRectangle = e.DragGraphic as CogRectangle;
                listValue = GetResionSize(cogRectangle);
                regionType = RegionType.CogRectangle;
            }
            else if (e.DragGraphic.GetType() == typeof(CogRectangleAffine))
            {
                var cogRectangleAffine = e.DragGraphic as CogRectangleAffine;
                listValue = GetResionSize(cogRectangleAffine);
                regionType = RegionType.CogRectangleAffine;
            }
            else if (e.DragGraphic.GetType() == typeof(CogCircularAnnulusSection))
            {
                var cogCircular = e.DragGraphic as CogCircularAnnulusSection;
                listValue = GetResionSize(cogCircular);
                regionType = RegionType.CogCircularAnnulusSection;
            }
            else if (e.DragGraphic.GetType() == typeof(CogEllipticalAnnulusSection))
            {
                var cogElliptical = e.DragGraphic as CogEllipticalAnnulusSection;
                listValue = GetResionSize(cogElliptical);
                regionType = RegionType.CogEllipticalAnnulusSection;
            }

            if (OnDragSize != null)
                OnDragSize(listValue, regionType);
        }
        catch { }

    }
}
