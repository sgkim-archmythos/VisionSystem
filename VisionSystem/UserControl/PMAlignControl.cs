using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro;
using DevExpress.XtraEditors;
using System.Threading;

namespace VisionSystem
{
    public partial class PMAlignControl : UserControl
    {
        CogPMAlignTool _cogPMAlignTool = null;
        bool _bLoad = false;
        ToolEdit _toolEdit = new ToolEdit();
        ICogRegion _cogRegion = null;

        LabelControl[] lblTitle = new LabelControl[8];
        TextEdit[] txtSize = new TextEdit[8];

        private enum ParamType
        {
            Angle,
            Scale,
            ScaleX,
            ScaleY
        }
        private enum ParanRange
        {
            Min,
            Max
        }

        public PMAlignControl()
        {
            InitializeComponent();
        }

        public void LoadSet(CogPMAlignTool PMAlignTool)
        {
            _cogPMAlignTool = PMAlignTool;

            for (var i = 0; i < 8; i++)
            {
                lblTitle[i] = new LabelControl();
                txtSize[i] = new TextEdit();

                lblTitle[i] = Controls.Find(string.Format("lblTitle{0}", i + 1), true).FirstOrDefault() as LabelControl;
                txtSize[i] = Controls.Find(string.Format("txtSize{0}", i + 1), true).FirstOrDefault() as TextEdit;
            }

            _cogRegion = _cogPMAlignTool.Pattern.TrainRegion;

            cbPMalignAlgorithm.SelectedIndex = (int)_cogPMAlignTool.Pattern.TrainAlgorithm;
            cbPMalignRegion.SelectedIndex = _toolEdit.GetRegionIndex(_cogPMAlignTool.Pattern.TrainRegion);

            txtFindNum.Text = _cogPMAlignTool.RunParams.ApproximateNumberToFind.ToString();
            txtThresold.Text = _cogPMAlignTool.RunParams.AcceptThreshold.ToString();

            if (_cogPMAlignTool.RunParams.ZoneAngle.Configuration == CogPMAlignZoneConstants.LowHigh)
                chkPMalignAngle.Checked = true;
            else
                chkPMalignAngle.Checked = false;

            if (_cogPMAlignTool.RunParams.ZoneScale.Configuration == CogPMAlignZoneConstants.LowHigh)
                chkPMalignScale.Checked = true;
            else
                chkPMalignScale.Checked = false;

            if (_cogPMAlignTool.RunParams.ZoneScaleX.Configuration == CogPMAlignZoneConstants.LowHigh)
                chkPMalignScaleX.Checked = true;
            else
                chkPMalignScaleX.Checked = false;

            if (_cogPMAlignTool.RunParams.ZoneScaleY.Configuration == CogPMAlignZoneConstants.LowHigh)
                chkPMalignScaleY.Checked = true;
            else
                chkPMalignScaleY.Checked = false;

            txtAngleMin.Text = string.Format("{0:F2}", _toolEdit.RadToAngle(_cogPMAlignTool.RunParams.ZoneAngle.Low));
            txtAngleMax.Text = string.Format("{0:F2}", _toolEdit.RadToAngle(_cogPMAlignTool.RunParams.ZoneAngle.High));
            txtScalingMin.Text = string.Format("{0:F2}", _cogPMAlignTool.RunParams.ZoneScale.Low);
            txtScalingMax.Text = string.Format("{0:F2}", _cogPMAlignTool.RunParams.ZoneScale.High);
            txtScaleXMin.Text = string.Format("{0:F2}", _cogPMAlignTool.RunParams.ZoneScaleX.Low);
            txtScaleXMax.Text = string.Format("{0:F2}", _cogPMAlignTool.RunParams.ZoneScaleX.High);
            txtScaleYMin.Text = string.Format("{0:F2}", _cogPMAlignTool.RunParams.ZoneScaleY.Low);
            txtScaleYMax.Text = string.Format("{0:F2}", _cogPMAlignTool.RunParams.ZoneScaleY.High);

            ZoneChange(ParamType.Angle, chkPMalignAngle.Checked);
            ZoneChange(ParamType.Scale, chkPMalignScale.Checked);
            ZoneChange(ParamType.ScaleX, chkPMalignScaleX.Checked);
            ZoneChange(ParamType.ScaleY, chkPMalignScaleY.Checked);

            TrainStatus();

            _toolEdit.OnDragSize = OnDragSize;

            _bLoad = true;
        }

        private void OnDragSize(List<string> list, ToolEdit.RegionType regionType)
        {
            SetRegionValue(regionType, list);
            TrainStatus();

            _cogPMAlignTool.Run();
        }

        private void TrainStatus()
        {
            try
            {
                if (_cogPMAlignTool.Pattern.Trained)
                {
                    lblTrain.Text = "Trained";
                    lblTrain.ForeColor = Color.Lime;
                }
                else
                {
                    lblTrain.Text = "Not Trained";
                    lblTrain.ForeColor = Color.Red;
                }
            }
            catch { }

        }

        private void cbPMalignRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            _cogRegion = _cogPMAlignTool.Pattern.TrainRegion;
            _toolEdit.SetDragMode(_cogRegion);
            var list = _toolEdit.GetResionSize(_cogRegion);

            SetRegionValue((ToolEdit.RegionType)cbPMalignRegion.SelectedIndex, list);
            TrainStatus();

            if (_bLoad)
            {
                _toolEdit.SetRegion((ToolEdit.RegionType)cbPMalignRegion.SelectedIndex, _cogPMAlignTool);
                _cogPMAlignTool.Run();
            }
        }

        private void SetRegionValue(ToolEdit.RegionType regionType, List<string> list)
        {
            if (regionType == ToolEdit.RegionType.CogPolygon)
            {
                pnlRegion2.Dock = DockStyle.Fill;

                dgPolygon.Rows.Clear();

                for (var i = 0; i < list.Count; i++)
                {
                    var strValue = list[i].Split(',');
                    dgPolygon.Rows.Add((i + 1).ToString(), strValue[0], strValue[1]);
                }

                pnlRegion1.Visible = false;
                pnlRegion2.Visible = true;
            }
            else
            {
                pnlRegion1.Dock = DockStyle.Fill;

                for (var i = 0; i < 8; i++)
                {
                    if (list.Count > i)
                    {
                        var strValue = list[i].Split(',');
                        lblTitle[i].Text = strValue[0];
                        txtSize[i].Text = strValue[1];
                        lblTitle[i].Visible = true;
                        txtSize[i].Visible = true;
                    }
                    else
                    {
                        lblTitle[i].Visible = false;
                        txtSize[i].Visible = false;
                    }
                }

                pnlRegion1.Visible = true;
                pnlRegion2.Visible = false;
            }
        }

        private void txtSize1_KeyDown(object sender, KeyEventArgs e)
        {
            var dValue = new double[8];
            for (var i = 0; i < 8; i++)
                double.TryParse(txtSize[i].Text, out dValue[i]);

            _toolEdit.SetRegionSize(0, dValue, _cogPMAlignTool);
            _cogPMAlignTool.Run();
        }

        private void btnPMalignCenter_Click(object sender, EventArgs e)
        {
            _toolEdit.SetPMalingOrigin(_cogPMAlignTool);            
        }

        private void btnPMalignTrain_Click(object sender, EventArgs e)
        {
            try
            {
                if (_cogPMAlignTool == null)
                    return;

                splashScreenManager1.ShowWaitForm();
                Thread.Sleep(500);

                _cogPMAlignTool.Pattern.Train();
                _cogPMAlignTool.Run();

                splashScreenManager1.CloseWaitForm();
            }
            catch { }

            TrainStatus();
        }

        private void cbPMalignAlgorithm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbPMalignAlgorithm.SelectedIndex == -1)
                return;

            try
            {
                if (_bLoad)
                {
                    if (_cogPMAlignTool == null)
                        return;

                    _cogPMAlignTool.Pattern.TrainAlgorithm = (CogPMAlignTrainAlgorithmConstants)cbPMalignAlgorithm.SelectedIndex;
                    _cogPMAlignTool.Run();
                }
            }
            catch { }

            TrainStatus();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var nRow = dgPolygon.Rows.Count;

            if (nRow == 0)
            {
                _toolEdit.SetRegionSize(0, new double[2] { 100, 100 }, _cogPMAlignTool);
                _toolEdit.SetRegionSize(0, new double[2] { 200, 100 }, _cogPMAlignTool);
                _toolEdit.SetRegionSize(0, new double[2] { 200, 200 }, _cogPMAlignTool);
                _toolEdit.SetRegionSize(0, new double[2] { 100, 200 }, _cogPMAlignTool);
            }
            else
            {
                double.TryParse(dgPolygon.Rows[nRow - 1].Cells[1].Value.ToString(), out var dPosX);
                double.TryParse(dgPolygon.Rows[nRow - 1].Cells[2].Value.ToString(), out var dPosY);

                _toolEdit.SetRegionSize(nRow, new double[2] { dPosX - 50, dPosY }, _cogPMAlignTool);
            }

            var list = _toolEdit.GetResionSize(_cogRegion);
            SetRegionValue((ToolEdit.RegionType)cbPMalignRegion.SelectedIndex, list);

            _cogPMAlignTool.Run();
            TrainStatus();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var nIdx = dgPolygon.CurrentRow.Index;

            if (nIdx == -1)
                return;

            _toolEdit.SetRegionSize(nIdx, null, _cogPMAlignTool);

            var list = _toolEdit.GetResionSize(_cogRegion);
            SetRegionValue((ToolEdit.RegionType)cbPMalignRegion.SelectedIndex, list);

            _cogPMAlignTool.Run();
            TrainStatus();
        }

        private void txtFindNum_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int.TryParse(txtFindNum.Text, out var nValue);

                if (nValue == 0)
                    return;

                _cogPMAlignTool.RunParams.ApproximateNumberToFind = nValue;
                _cogPMAlignTool.Run();
            }
        }

        private void txtThresold_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                double.TryParse(txtThresold.Text, out var dValue);

                if (dValue == 0)
                    return;

                _cogPMAlignTool.RunParams.AcceptThreshold = dValue;
                _cogPMAlignTool.Run();
            }
        }

        private void chkPMalignAngle_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckEdit).Checked)
                (sender as CheckEdit).ForeColor = Color.Yellow;
            else
                (sender as CheckEdit).ForeColor = Color.White;

            int.TryParse((sender as CheckEdit).Tag.ToString(), out var nTag);
            ZoneChange((ParamType)nTag, (sender as CheckEdit).Checked);

            SetParam(ParamType.Angle, ParanRange.Min);
            SetParam(ParamType.Angle, ParanRange.Max);

            if (_bLoad)
                _cogPMAlignTool.Run();
        }

        private void ZoneChange(ParamType paramType, bool bEnable)
        {
            if (paramType == ParamType.Angle)
            {
                txtAngleMin.Enabled = bEnable;
                txtAngleMax.Enabled = bEnable;

                _cogPMAlignTool.RunParams.ZoneAngle.HasChanged = bEnable;
            }
            else if (paramType == ParamType.Scale)
            {
                txtScalingMin.Enabled = bEnable;
                txtScalingMax.Enabled = bEnable;

                _cogPMAlignTool.RunParams.ZoneScale.HasChanged = bEnable;
            }
            else if (paramType == ParamType.ScaleX)
            {
                txtScaleXMin.Enabled = bEnable;
                txtScaleXMax.Enabled = bEnable;

                _cogPMAlignTool.RunParams.ZoneScaleX.HasChanged = bEnable;
            }
            else
            {
                txtScaleYMin.Enabled = bEnable;
                txtScaleYMax.Enabled = bEnable;

                _cogPMAlignTool.RunParams.ZoneScaleY.HasChanged = bEnable;
            }
        }

        private void SetParam(ParamType paramType, ParanRange range)
        {
            if (paramType == ParamType.Angle)
            {
                if (range == ParanRange.Min)
                {
                    double.TryParse(txtAngleMin.Text, out var dMin);
                    _cogPMAlignTool.RunParams.ZoneAngle.Low = _toolEdit.AngleToRad(dMin);
                }
                else
                {
                    double.TryParse(txtAngleMax.Text, out var dMax);
                    _cogPMAlignTool.RunParams.ZoneAngle.High = _toolEdit.AngleToRad(dMax);
                }
            }
            else if (paramType == ParamType.Scale)
            {
                if (range == ParanRange.Min)
                {
                    double.TryParse(txtScalingMin.Text, out var dMin);
                    _cogPMAlignTool.RunParams.ZoneScale.Low = dMin;
                }
                else
                {
                    double.TryParse(txtScalingMin.Text, out var dMax);
                    _cogPMAlignTool.RunParams.ZoneAngle.High = dMax;
                }
            }
            else if (paramType == ParamType.ScaleX)
            {
                if (range == ParanRange.Min)
                {
                    double.TryParse(txtScaleXMin.Text, out var dMin);
                    _cogPMAlignTool.RunParams.ZoneAngle.Low = dMin;
                }
                else
                {
                    double.TryParse(txtScaleXMax.Text, out var dMax);
                    _cogPMAlignTool.RunParams.ZoneAngle.High = dMax;
                }
            }
            else
            {
                if (range == ParanRange.Min)
                {
                    double.TryParse(txtScaleYMin.Text, out var dMin);
                    _cogPMAlignTool.RunParams.ZoneAngle.Low = dMin;
                }
                else
                {
                    double.TryParse(txtScaleYMax.Text, out var dMax);
                    _cogPMAlignTool.RunParams.ZoneAngle.High = dMax;
                }
            }
            
        }
        private void txtAngleMin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ParamType type = ParamType.Angle;
                if ((sender as TextEdit).Name.Contains("Scaling"))
                    type = ParamType.Scale;
                else if ((sender as TextEdit).Name.Contains("ScaleX"))
                    type = ParamType.ScaleX;
                else if ((sender as TextEdit).Name.Contains("ScaleY"))
                    type = ParamType.ScaleY;

                int.TryParse((sender as TextEdit).Tag.ToString(), out var nTag);
                SetParam(type, (ParanRange)nTag);

                _cogPMAlignTool.Run();
            }
        }

        private void btnTraginGrab_Click(object sender, EventArgs e)
        {
            if (_cogPMAlignTool.InputImage != null)
            {
                _cogPMAlignTool.Pattern.TrainImage = _cogPMAlignTool.InputImage;
                TrainStatus();
            }
        }
    }
}
