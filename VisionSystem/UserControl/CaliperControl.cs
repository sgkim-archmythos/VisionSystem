using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro;
using DevExpress.XtraEditors;

namespace VisionSystem
{
    public partial class CaliperControl : UserControl
    {
        CogCaliperTool _cogCaliper = null;
        bool _bLoad = false;
        ICogRegion _cogRegion;

        LabelControl[] lblTitle = new LabelControl[8];
        TextEdit[] txtSize = new TextEdit[8];

        ToolEdit _toolEdit = new ToolEdit();

        public CaliperControl()
        {
            InitializeComponent();
        }

        public void LoadSet(CogCaliperTool cogCaliper)
        {
            _cogCaliper = cogCaliper;
            _cogRegion = _cogCaliper.Region;

            for (var i = 0; i < 8; i++)
            {
                lblTitle[i] = new LabelControl();
                txtSize[i] = new TextEdit();

                lblTitle[i] = Controls.Find(string.Format("lblTitle{0}", i + 1), true).FirstOrDefault() as LabelControl;
                txtSize[i] = Controls.Find(string.Format("txtSize{0}", i + 1), true).FirstOrDefault() as TextEdit;
            }

            if (_cogCaliper.RunParams.EdgeMode == CogCaliperEdgeModeConstants.SingleEdge)
                radCaliperMode1.Checked = true;
            else
                radCaliperMode2.Checked = true;

            if (_cogCaliper.RunParams.Edge0Polarity == CogCaliperPolarityConstants.DarkToLight)
                radThreas1_1.Checked = true;
            else if (_cogCaliper.RunParams.Edge0Polarity == CogCaliperPolarityConstants.LightToDark)
                radThreas1_2.Checked = true;
            else
                radThreas1_3.Checked = true;

            if (_cogCaliper.RunParams.Edge1Polarity == CogCaliperPolarityConstants.DarkToLight)
                radThreas2_1.Checked = true;
            else if (_cogCaliper.RunParams.Edge1Polarity == CogCaliperPolarityConstants.LightToDark)
                radThreas2_2.Checked = true;
            else
                radThreas2_3.Checked = true;

            var dValue = new double[2];
            dValue[0] = _cogCaliper.RunParams.Edge0Position;
            dValue[1] = _cogCaliper.RunParams.Edge1Position;

            txtEdgiWidth.Text = Math.Abs(dValue[1] - dValue[0]).ToString();
            txtThresold.Text = _cogCaliper.RunParams.ContrastThreshold.ToString();
            txtFilterPixel.Text = _cogCaliper.RunParams.FilterHalfSizeInPixels.ToString();
            txtFindNum.Text = _cogCaliper.RunParams.MaxResults.ToString();

            if (_cogRegion == null)
                cbRegion.SelectedIndex = 1;
            else
                cbRegion.SelectedIndex = 0;

            _toolEdit.OnDragSize = OnDragSize;

            _bLoad = true;
        }

        private void OnDragSize(List<string> list, ToolEdit.RegionType regionType)
        {
            SetRegionValue(regionType, list);
            _cogCaliper.Run();
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

        private void radCaliperMode1_CheckedChanged(object sender, EventArgs e)
        {
            int.TryParse((sender as RadioButton).Tag.ToString(), out var nTag);
            
            if ((sender as RadioButton).Checked)
            {
                if (nTag == 1)
                {
                    pnlEdge1.Enabled = true;
                    pnlEdge2.Enabled = false;
                }
                else
                {
                    pnlEdge1.Enabled = true;
                    pnlEdge2.Enabled = true;
                }

                if (_bLoad)
                {
                    _cogCaliper.RunParams.EdgeMode = (CogCaliperEdgeModeConstants)nTag;
                    _cogCaliper.Run();
                }
            }
        }


        private void radThreas1_1_CheckedChanged(object sender, EventArgs e)
        {
            int.TryParse((sender as RadioButton).Tag.ToString(), out var nTag);

            if ((sender as RadioButton).Checked)
            {
                if (_bLoad)
                {
                    _cogCaliper.RunParams.Edge0Polarity = (CogCaliperPolarityConstants)nTag;
                    _cogCaliper.Run();
                }
            }
        }

        private void radThreas2_1_CheckedChanged(object sender, EventArgs e)
        {
            int.TryParse((sender as RadioButton).Tag.ToString(), out var nTag);

            if ((sender as RadioButton).Checked)
            {
                if (_bLoad)
                {
                    _cogCaliper.RunParams.Edge1Polarity = (CogCaliperPolarityConstants)nTag;
                    _cogCaliper.Run();
                }
            }
        }

        private void txtSize1_KeyDown(object sender, KeyEventArgs e)
        {
            var dValue = new double[8];
            for (var i = 0; i < 8; i++)
                double.TryParse(txtSize[i].Text, out dValue[i]);

            _toolEdit.SetRegionSize(0, dValue, _cogCaliper);
            _cogCaliper.Run();
        }

        private void cbRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_bLoad)
            {
                if (cbRegion.SelectedIndex == 0)
                    _toolEdit.SetRegion(ToolEdit.RegionType.CogRectangleAffine, _cogCaliper);
                else
                    _toolEdit.SetRegion(ToolEdit.RegionType.None, _cogCaliper);
            }

            _cogRegion = _cogCaliper.Region;
            _toolEdit.SetDragMode(_cogRegion);
            var list = _toolEdit.GetResionSize(_cogRegion);

            if (cbRegion.SelectedIndex == 0)
                SetRegionValue(ToolEdit.RegionType.CogRectangleAffine, list);
            else
            {
                pnlRegion1.Visible = false;
                pnlRegion2.Visible = false;
            }
            _cogCaliper.Run();
        }

        private void txtEdgiWidth_KeyDown(object sender, KeyEventArgs e)
        {
            double.TryParse((sender as TextEdit).Text, out var dValue);
            int.TryParse((sender as TextEdit).Tag.ToString(), out var nTag);

            _toolEdit.SetCaliperParam(dValue, _cogCaliper, (ToolEdit.CaliperParam)nTag);
            _cogCaliper.Run();
        }
    }
}
