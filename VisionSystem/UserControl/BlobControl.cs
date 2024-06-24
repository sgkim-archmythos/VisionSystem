using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cognex.VisionPro;
using Cognex.VisionPro.Blob;
using DevExpress.XtraEditors;

namespace VisionSystem
{
    public partial class BlobControl : UserControl
    {
        CogBlobTool _cogBlobTool = null;
        ToolEdit _toolEdit = new ToolEdit();
        bool _bLoad = false;
        int _nMode = 0;
        int _nPolarity = 0;
        int _nHardFixed = 0;
        int _nHardRelative = 0;
        int _nSoftFixedHigh = 0;
        int _nSoftFixedLow = 0;
        int _nSoftness = 0;
        int _nSoftRelativeHigh = 0;
        int _nSoftRelativeLow = 0;
        int _nTailHigh = 0;
        int _nTailLow = 0;
        int _nMinPixel = 0;

        ICogRegion _cogRegion = null;
        LabelControl[] lblThresholdName = new LabelControl[5];
        TextEdit[] txtThreshold = new TextEdit[5];
        LabelControl[] lblPercentage = new LabelControl[5];

        LabelControl[] lblTitle = new LabelControl[8];
        TextEdit[] txtSize = new TextEdit[8];

        private enum BlobFilterMode
        {
            Mode,
            FilterMode,
            FilterRangeLow,
            FilterRangeHigh
        }

        public BlobControl()
        {
            InitializeComponent();
        }

        public void LoadSet(CogBlobTool cogBlobTool)
        {
            _cogBlobTool = cogBlobTool;

            if (InvokeRequired)
            {
                Invoke(new EventHandler(delegate
                {
                    LoadData();
                }));
            }
            else
            {
                LoadData();
            }

            _bLoad = true;
        }

        private void LoadData()
        {
            for (var i = 0; i < 5; i++)
            {
                lblThresholdName[i] = new LabelControl();
                txtThreshold[i] = new TextEdit();
                lblPercentage[i] = new LabelControl();

                lblThresholdName[i] = Controls.Find(string.Format("lblThresholdName{0}", i + 1), true).FirstOrDefault() as LabelControl;
                txtThreshold[i] = Controls.Find(string.Format("txtThreshold{0}", i + 1), true).FirstOrDefault() as TextEdit;
                lblPercentage[i] = Controls.Find(string.Format("lblPercentage{0}", i + 1), true).FirstOrDefault() as LabelControl;
            }

            for (var i = 0; i < 8; i++)
            {
                lblTitle[i] = new LabelControl();
                txtSize[i] = new TextEdit();

                lblTitle[i] = Controls.Find(string.Format("lblTitle{0}", i + 1), true).FirstOrDefault() as LabelControl;
                txtSize[i] = Controls.Find(string.Format("txtSize{0}", i + 1), true).FirstOrDefault() as TextEdit;
            }

            _nMode = (int)_cogBlobTool.RunParams.SegmentationParams.Mode;
            _nPolarity = (int)_cogBlobTool.RunParams.SegmentationParams.Polarity;
            _nHardFixed = _cogBlobTool.RunParams.SegmentationParams.HardFixedThreshold;
            _nHardRelative = _cogBlobTool.RunParams.SegmentationParams.HardRelativeThreshold;
            _nSoftFixedHigh = _cogBlobTool.RunParams.SegmentationParams.SoftFixedThresholdHigh;
            _nSoftFixedLow = _cogBlobTool.RunParams.SegmentationParams.SoftFixedThresholdLow;
            _nSoftRelativeHigh = _cogBlobTool.RunParams.SegmentationParams.SoftRelativeThresholdHigh;
            _nSoftRelativeLow = _cogBlobTool.RunParams.SegmentationParams.SoftRelativeThresholdLow;
            _nTailHigh = _cogBlobTool.RunParams.SegmentationParams.TailHigh;
            _nTailLow = _cogBlobTool.RunParams.SegmentationParams.TailLow;
            _nSoftness = _cogBlobTool.RunParams.SegmentationParams.Softness;

            cbBlobPolarity.SelectedIndex = _nPolarity;
            cbBlobMode.SelectedIndex = _nMode - 2;

            _nMinPixel = _cogBlobTool.RunParams.ConnectivityMinPixels;
            _cogRegion = _cogBlobTool.Region;

            var strValue = _toolEdit.GetBlobFilter(_cogBlobTool);

            cbBlobFilter.SelectedIndex = int.Parse(strValue[0]);
            cbBlobFilterStatus.SelectedIndex = int.Parse(strValue[1]);
            txtBlobFilterMin.Text = strValue[2];
            txtBlobFilterMax.Text = strValue[3];
            txtBlobPixel.Text = _nMinPixel.ToString();

            cbBlobRegion.SelectedIndex = _toolEdit.GetRegionIndex(_cogRegion);

            _toolEdit.OnDragSize = OnDragSize;
        }

        private void OnDragSize(List<string> list, ToolEdit.RegionType regionType)
        {
            SetRegionValue(regionType, list);
            _cogBlobTool.Run();
        }

        private void cbBlobMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            _nMode = cbBlobMode.SelectedIndex;
            SetThresholdParam();
            _cogBlobTool.Run();
        }

        private void SetThresholdParam()
        {
            if (_bLoad)
                _cogBlobTool.RunParams.SegmentationParams.Mode = (CogBlobSegmentationModeConstants)_nMode;

            if (_nMode == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i == 0)
                    {
                        lblThresholdName[i].Visible = true;
                        lblThresholdName[i].Text = "임계값 : ";
                        txtThreshold[i].Visible = true;
                        txtThreshold[i].Text = _nHardFixed.ToString();
                        lblPercentage[i].Visible = false;
                    }
                    else
                    {
                        lblThresholdName[i].Visible = false;
                        txtThreshold[i].Visible = false;
                        lblPercentage[i].Visible = false;
                    }
                }
            }
            else if (_nMode == 1)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i == 0 || i == 1 || i == 2)
                    {
                        lblThresholdName[i].Visible = true;
                        txtThreshold[i].Visible = true;
                        lblPercentage[i].Visible = true;

                        if (i == 0)
                        {
                            lblThresholdName[i].Text = "임계값 : ";
                            txtThreshold[i].Text = _nHardRelative.ToString();
                        }
                        else if (i == 1)
                        {
                            lblThresholdName[i].Text = "하위 테일 : ";
                            txtThreshold[i].Text = _nTailLow.ToString();
                        }
                        else if (i == 2)
                        {
                            lblThresholdName[i].Text = "상위 테일 : ";
                            txtThreshold[i].Text = _nTailHigh.ToString();
                        }
                    }
                    else
                    {
                        lblThresholdName[i].Visible = false;
                        txtThreshold[i].Visible = false;
                        lblPercentage[i].Visible = false;
                    }
                }
            }
            else if (_nMode == 2)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i == 0 || i == 1)
                    {
                        lblThresholdName[i].Visible = true;
                        txtThreshold[i].Visible = true;
                        lblPercentage[i].Visible = true;

                        if (i == 0)
                        {
                            lblThresholdName[i].Text = "하위 테일 : ";
                            txtThreshold[i].Text = _nTailLow.ToString();
                        }
                        else if (i == 1)
                        {
                            lblThresholdName[i].Text = "상위 테일 : ";
                            txtThreshold[i].Text = _nTailHigh.ToString();
                        }
                    }
                    else
                    {
                        lblThresholdName[i].Visible = false;
                        txtThreshold[i].Visible = false;
                        lblPercentage[i].Visible = false;
                    }
                }
            }
            else if (_nMode == 3)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i == 0 || i == 1 || i == 2)
                    {
                        lblThresholdName[i].Visible = true;
                        txtThreshold[i].Visible = true;
                        lblPercentage[i].Visible = false;

                        if (i == 0)
                        {
                            lblThresholdName[i].Text = "최저 임계값 : ";
                            txtThreshold[i].Text = _nSoftFixedLow.ToString();
                        }
                        else if (i == 1)
                        {
                            lblThresholdName[i].Text = "최고 임계값 : ";
                            txtThreshold[i].Text = _nSoftFixedHigh.ToString();
                        }
                        else if (i == 2)
                        {
                            lblThresholdName[i].Text = "유연도 : ";
                            txtThreshold[i].Text = _nSoftness.ToString();
                        }
                    }
                    else
                    {
                        lblThresholdName[i].Visible = false;
                        txtThreshold[i].Visible = false;
                        lblPercentage[i].Visible = false;
                    }
                    
                }
            }
            else if (_nMode == 4)
            {
                for (int i = 0; i < 5; i++)
                {
                    lblThresholdName[i].Visible = true;
                    txtThreshold[i].Visible = true;

                    if (i > 4)
                        lblPercentage[i].Visible = true;

                    if (i == 0)
                    {
                        lblThresholdName[i].Text = "최저 임계값 : ";
                        txtThreshold[i].Text = _nSoftRelativeLow.ToString();
                    }
                    else if (i == 1)
                    {
                        lblThresholdName[i].Text = "최고 임계값 : ";
                        txtThreshold[i].Text = _nSoftRelativeHigh.ToString();
                    }
                    else if (i == 2)
                    {
                        lblThresholdName[i].Text = "하위 테일 : ";
                        txtThreshold[i].Text = _nTailLow.ToString();
                    }
                    else if (i == 3)
                    {
                        lblThresholdName[i].Text = "상위 테일 : ";
                        txtThreshold[i].Text = _nTailHigh.ToString();
                    }
                    else if (i == 4)
                    {
                        lblThresholdName[i].Text = "유연도 : ";
                        txtThreshold[i].Text = _nSoftness.ToString();
                    }
                }
            }
        }

        private void txtThreshold1_KeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtThreshold1.Text))
                return;

            if (_cogBlobTool == null)
                return;

            if (e.KeyCode == Keys.Enter)
            {
                if (_nMode == 0)
                {
                    int.TryParse(txtThreshold1.Text, out _nHardFixed);
                    _cogBlobTool.RunParams.SegmentationParams.HardFixedThreshold = _nHardFixed;
                }
                else if (_nMode == 1)
                {
                    int.TryParse(txtThreshold1.Text, out _nHardRelative);
                    _cogBlobTool.RunParams.SegmentationParams.HardRelativeThreshold = _nHardRelative;
                }
                else if (_nMode == 2)
                {
                    int.TryParse(txtThreshold1.Text, out _nTailLow);
                    _cogBlobTool.RunParams.SegmentationParams.TailLow = _nTailLow;
                }
                else if (_nMode == 3)
                {
                    int.TryParse(txtThreshold1.Text, out _nSoftFixedLow);
                    _cogBlobTool.RunParams.SegmentationParams.SoftFixedThresholdLow = _nSoftFixedLow;
                }
                else if (_nMode == 4)
                {
                    int.TryParse(txtThreshold1.Text, out _nSoftRelativeLow);
                    _cogBlobTool.RunParams.SegmentationParams.SoftRelativeThresholdLow = _nSoftRelativeLow;
                }

                _cogBlobTool.Run();
            }
        }

        private void txtThreshold2_KeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtThreshold2.Text))
                return;

            if (_cogBlobTool == null)
                return;

            if (e.KeyCode == Keys.Enter)
            {
                if (_nMode == 1)
                {
                    int.TryParse(txtThreshold2.Text, out _nTailLow);
                    _cogBlobTool.RunParams.SegmentationParams.TailLow = _nTailLow;
                }
                else if (_nMode == 2)
                {
                    int.TryParse(txtThreshold2.Text, out _nTailHigh);
                    _cogBlobTool.RunParams.SegmentationParams.TailHigh = _nTailHigh;
                }
                else if (_nMode == 3)
                {
                    int.TryParse(txtThreshold2.Text, out _nSoftFixedHigh);
                    _cogBlobTool.RunParams.SegmentationParams.SoftFixedThresholdHigh = _nSoftFixedHigh;
                }
                else if (_nMode == 4)
                {
                    int.TryParse(txtThreshold2.Text, out _nSoftRelativeHigh);
                    _cogBlobTool.RunParams.SegmentationParams.SoftRelativeThresholdHigh = _nSoftRelativeHigh;
                }

                _cogBlobTool.Run();
            }
        }

        private void txtThreshold3_KeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtThreshold3.Text))
                return;

            if (_cogBlobTool == null)
                return;

            if (e.KeyCode == Keys.Enter)
            {
                if (_nMode == 1)
                {
                    int.TryParse(txtThreshold3.Text, out _nTailHigh);
                    _cogBlobTool.RunParams.SegmentationParams.TailHigh = _nTailHigh;
                }
                else if (_nMode == 3)
                {
                    int.TryParse(txtThreshold3.Text, out _nSoftness);
                    _cogBlobTool.RunParams.SegmentationParams.Softness = _nSoftness;
                }
                else if (_nMode == 4)
                {
                    int.TryParse(txtThreshold3.Text, out _nTailLow);
                    _cogBlobTool.RunParams.SegmentationParams.TailLow = _nTailLow;
                }

                _cogBlobTool.Run();
            }
        }

        private void txtThreshold4_KeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtThreshold4.Text))
                return;

            if (_cogBlobTool == null)
                return;

            if (e.KeyCode == Keys.Enter)
            {
                int.TryParse(txtThreshold4.Text, out _nTailHigh);
                _cogBlobTool.RunParams.SegmentationParams.TailHigh = _nTailHigh;

                _cogBlobTool.Run();
            }
        }

        private void txtThreshold5_KeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtThreshold5.Text))
                return;

            if (_cogBlobTool == null)
                return;

            if (e.KeyCode == Keys.Enter)
            {
                int.TryParse(txtThreshold5.Text, out _nSoftness);
                _cogBlobTool.RunParams.SegmentationParams.Softness = _nSoftness;

                _cogBlobTool.Run();
            }
        }

        private void txtBlobPixel_KeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBlobPixel.Text))
                return;

            if (_cogBlobTool == null)
                return;

            int.TryParse(txtBlobPixel.Text, out var nValue);
            _cogBlobTool.RunParams.ConnectivityMinPixels = nValue;

            _cogBlobTool.Run();
        }

        private void cbBlobRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_bLoad)
                _toolEdit.SetRegion((ToolEdit.RegionType)cbBlobRegion.SelectedIndex, _cogBlobTool);

            _cogRegion = _cogBlobTool.Region;
            _toolEdit.SetDragMode(_cogRegion);
            var list = _toolEdit.GetResionSize(_cogRegion);

            SetRegionValue((ToolEdit.RegionType)cbBlobRegion.SelectedIndex, list);

            _cogBlobTool.Run();
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

        private void cbBlobFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_bLoad)
            {
                SetBlobFilter(BlobFilterMode.Mode, cbBlobFilter.SelectedIndex);
                _cogBlobTool.Run();
            }

            if (cbBlobFilter.SelectedIndex == 1)
            {
                cbBlobFilterStatus.Enabled = false;
                txtBlobFilterMin.Enabled = false;
                txtBlobFilterMax.Enabled = false;
            }
            else
            {
                cbBlobFilterStatus.Enabled = true;
                txtBlobFilterMin.Enabled = true;
                txtBlobFilterMax.Enabled = true;
            }

        }

        private void SetBlobFilter(BlobFilterMode Mode, int nValue)
        {
            try
            {
                foreach (CogBlobMeasure Measure in _cogBlobTool.RunParams.RunTimeMeasures)
                {
                    if (Measure.Measure == CogBlobMeasureConstants.Area)
                    {
                        if (Mode == BlobFilterMode.Mode)
                            Measure.Mode = (CogBlobMeasureModeConstants)nValue;
                        else if (Mode == BlobFilterMode.FilterMode)
                            Measure.FilterMode = (CogBlobFilterModeConstants)nValue;
                        else if (Mode == BlobFilterMode.FilterRangeLow)
                            Measure.FilterRangeLow = nValue;
                        else
                            Measure.FilterRangeHigh = nValue;
                    }
                }
            }
            catch { }
        }

        private void cbBlobFilterStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_bLoad)
            {
                SetBlobFilter(BlobFilterMode.FilterMode, cbBlobFilterStatus.SelectedIndex);
                _cogBlobTool.Run();
            }
        }

        private void txtBlobFilterMin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            int.TryParse(txtBlobFilterMin.Text, out var nValue);
            SetBlobFilter(BlobFilterMode.FilterRangeLow, nValue);
            _cogBlobTool.Run();
        }

        private void txtBlobFilterMax_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            int.TryParse(txtBlobFilterMin.Text, out var nValue);
            SetBlobFilter(BlobFilterMode.FilterRangeLow, nValue);
            _cogBlobTool.Run();
        }

        private void txtSize1_KeyDown(object sender, KeyEventArgs e)
        {
            var dValue = new double[8];
            for (var i = 0; i < 8; i++)
                double.TryParse(txtSize[i].Text, out dValue[i]);

            _toolEdit.SetRegionSize(0, dValue, _cogBlobTool);
            _cogBlobTool.Run();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var nRow = dgPolygon.Rows.Count;

            if (nRow == 0)
            {
                _toolEdit.SetRegionSize(0, new double[2] { 100, 100 }, _cogBlobTool);
                _toolEdit.SetRegionSize(0, new double[2] { 200, 100 }, _cogBlobTool);
                _toolEdit.SetRegionSize(0, new double[2] { 200, 200 }, _cogBlobTool);
                _toolEdit.SetRegionSize(0, new double[2] { 100, 200 }, _cogBlobTool);
            }
            else
            {
                double.TryParse(dgPolygon.Rows[nRow - 1].Cells[1].Value.ToString(), out var dPosX);
                double.TryParse(dgPolygon.Rows[nRow - 1].Cells[2].Value.ToString(), out var dPosY);

                _toolEdit.SetRegionSize(nRow, new double[2] { dPosX-50, dPosY }, _cogBlobTool);
            }

            var list = _toolEdit.GetResionSize(_cogRegion);
            SetRegionValue((ToolEdit.RegionType)cbBlobRegion.SelectedIndex, list);
            _cogBlobTool.Run();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var nIdx = dgPolygon.CurrentRow.Index;

            if (nIdx == -1)
                return;

            _toolEdit.SetRegionSize(nIdx, null, _cogBlobTool);

            var list = _toolEdit.GetResionSize(_cogRegion);
            SetRegionValue((ToolEdit.RegionType)cbBlobRegion.SelectedIndex, list);
            _cogBlobTool.Run();
        }

        private void cbBlobPolarity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_bLoad)
            {
                _cogBlobTool.RunParams.SegmentationParams.Polarity = (CogBlobSegmentationPolarityConstants)cbBlobPolarity.SelectedIndex;
                _cogBlobTool.Run();
            }
        }
    }
}
