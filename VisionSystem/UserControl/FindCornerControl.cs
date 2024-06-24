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
using DevExpress.XtraEditors;

namespace VisionSystem
{
    public partial class FindCornerControl : UserControl
    {
        CogFindCornerTool _cogFindCorner = null;
        TextEdit[] txtValue = new TextEdit[8];
        ToolEdit _toolEdit = new ToolEdit();
        bool _bLoad = false;

        public FindCornerControl()
        {
            InitializeComponent();
        }

        public void LoadSet(CogFindCornerTool FindCornerTool)
        {
            _cogFindCorner = FindCornerTool;

            for (int i = 0; i < 8; i++)
                txtValue[i] = Controls.Find(string.Format("txtValue{0}", i + 1), true).FirstOrDefault() as TextEdit;

            _toolEdit.GetFindCornerParam(out int nCaliperCnt, out double dSearchLen, out double dProjectionLen, out double nSearchDirection, out int nEdgeMode, out int nPolarity1, out int nPolarity2, out double dThreshold, out int nFilterPixel, out double dEdgeLen, out bool blgnore, out int nlgnoreCnt, _cogFindCorner);

            txtValue[0].Text = nCaliperCnt.ToString();
            txtValue[1].Text = dSearchLen.ToString();
            txtValue[2].Text = dProjectionLen.ToString();
            txtValue[3].Text = nSearchDirection.ToString();
            txtValue[4].Text = dThreshold.ToString();
            txtValue[5].Text = nFilterPixel.ToString();
            txtValue[6].Text = dEdgeLen.ToString();
            txtValue[7].Text = nlgnoreCnt.ToString();

            if (nEdgeMode == 1)
                radFindCircleEdge1.Checked = true;
            else
                radFindCircleEdge2.Checked = true;

            if (nPolarity1 == 1)
                radFindCircleEdge1_1.Checked = true;
            else if (nPolarity1 == 2)
                radFindCircleEdge1_2.Checked = true;
            else
                radFindCircleEdge1_3.Checked = true;

            if (nPolarity1 == 2)
                radFindCircleEdge2_1.Checked = true;
            else if (nPolarity1 == 2)
                radFindCircleEdge2_2.Checked = true;
            else
                radFindCircleEdge2_3.Checked = true;

            chklgnore.Checked = blgnore;

            _bLoad = true;
        }

        private void txtValue1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int.TryParse((sender as TextEdit).Tag.ToString(), out var nTag);
                var strValue = (sender as TextEdit).Text;

                _toolEdit.SetFineCornerParam((ToolEdit.FindCornerParam)nTag, strValue, _cogFindCorner);
                _cogFindCorner.Run();
            }
        }

        private void radFindCircleEdge1_CheckedChanged(object sender, EventArgs e)
        {
            var strTag = (sender as RadioButton).Tag.ToString();

            if ((sender as RadioButton).Checked)
            {
                (sender as RadioButton).ForeColor = Color.Yellow;

                if (strTag == "1")
                {
                    gpEdge1.Enabled = true;
                    gpEdge2.Enabled = false;
                }
                else
                {
                    gpEdge1.Enabled = true;
                    gpEdge2.Enabled = true;
                }
            }
            else
                (sender as RadioButton).ForeColor = Color.White;

            if (_bLoad)
            {
                _toolEdit.SetFineCornerParam(ToolEdit.FindCornerParam.EdgeMode, strTag, _cogFindCorner);
                _cogFindCorner.Run();
            }
        }

        private void radFindCircleEdge1_1_CheckedChanged(object sender, EventArgs e)
        {
            var strTag = (sender as RadioButton).Tag.ToString();

            if ((sender as RadioButton).Checked)
                (sender as RadioButton).ForeColor = Color.Yellow;
            else
                (sender as RadioButton).ForeColor = Color.White;

            if (_bLoad)
            {
                _toolEdit.SetFineCornerParam(ToolEdit.FindCornerParam.Polarity1, strTag, _cogFindCorner);
                _cogFindCorner.Run();
            }
        }

        private void radFindCircleEdge2_1_CheckedChanged(object sender, EventArgs e)
        {
            var strTag = (sender as RadioButton).Tag.ToString();

            if ((sender as RadioButton).Checked)
                (sender as RadioButton).ForeColor = Color.Yellow;
            else
                (sender as RadioButton).ForeColor = Color.White;

            if (_bLoad)
            {
                _toolEdit.SetFineCornerParam(ToolEdit.FindCornerParam.Polarity2, strTag, _cogFindCorner);
                _cogFindCorner.Run();
            }
        }

        private void btnFindCornerDirect_Click(object sender, EventArgs e)
        {
            int.TryParse(txtValue[3].Text, out int nValue);

            nValue *= -1;

            _toolEdit.SetFineCornerParam(ToolEdit.FindCornerParam.SearchDirection, nValue.ToString(), _cogFindCorner);
            _cogFindCorner.Run();
        }

        private void chklgnore_CheckedChanged(object sender, EventArgs e)
        {
            if (chklgnore.Checked)
            {
                chklgnore.ForeColor = Color.Yellow;
                txtValue[7].Enabled = true;
            }
            else
            {
                chklgnore.ForeColor = Color.White;
                txtValue[7].Enabled = false;
            }

            if (_bLoad)
            {
                _toolEdit.SetFineCornerParam(ToolEdit.FindCornerParam.lonore, chklgnore.Checked.ToString(), _cogFindCorner);

                int.TryParse(txtValue[7].Text, out int nValue);
                _toolEdit.SetFineCornerParam(ToolEdit.FindCornerParam.lgonreCnt, nValue.ToString(), _cogFindCorner);

                _cogFindCorner.Run();
            }
        }
    }
}
