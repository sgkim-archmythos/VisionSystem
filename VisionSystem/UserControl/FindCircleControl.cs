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
    public partial class FindCircleControl : UserControl
    {
        CogFindCircleTool _cogFindCircle = null;
        TextEdit[] txtValue = new TextEdit[7];

        ToolEdit _toolEdit = new ToolEdit();

        bool _bLoad = false;

        public FindCircleControl()
        {
            InitializeComponent();
        }

        public void LoadSet(CogFindCircleTool cogFindCircle)
        {
            _cogFindCircle = cogFindCircle;

            for (int i=0; i<7; i++)
                txtValue[i] = Controls.Find(string.Format("txtValue{0}", i + 1), true).FirstOrDefault() as TextEdit;

            _toolEdit.GetFindCircleParam(out int nCaliperCnt, out double dSearchLen, out double dProjectionLen, out int nSearchDirection, out int nEdgeMode, out int nPolarity1, out int nPolarity2, out double dThreshold, out int nFilterPixel, out bool bTolgnore, out int nTolognreCnt, out double dEdgeLen, _cogFindCircle);

            txtValue[0].Text = nCaliperCnt.ToString();
            txtValue[1].Text = dSearchLen.ToString();
            txtValue[2].Text = dProjectionLen.ToString();

            if (nSearchDirection == 0)
                radFindCircle1.Checked = true;
            else
                radFindCircle2.Checked = true;

            chkFindCircleTolgnore.Checked = bTolgnore;
            txtValue[3].Text = nTolognreCnt.ToString();

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

            txtValue[4].Text = dEdgeLen.ToString();
            txtValue[5].Text = dThreshold.ToString();
            txtValue[6].Text = nFilterPixel.ToString();

            _bLoad = true;
        }

        private void txtValue1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int.TryParse((sender as TextEdit).Tag.ToString(), out var nTag);
                var strValue = (sender as TextEdit).Text;

                _toolEdit.SetFindCircleParam((ToolEdit.FindCircleParam)nTag, strValue, _cogFindCircle);

                _cogFindCircle.Run();
            }
        }

        private void radFindCircle1_CheckedChanged(object sender, EventArgs e)
        {
            var strTag = (sender as RadioButton).Tag.ToString();

            if ((sender as RadioButton).Checked)
                (sender as RadioButton).ForeColor = Color.Yellow;
            else
                (sender as RadioButton).ForeColor = Color.White;

            if (_bLoad)
            {
                _toolEdit.SetFindCircleParam(ToolEdit.FindCircleParam.SearchDirection, strTag, _cogFindCircle);
                _cogFindCircle.Run();
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
                _toolEdit.SetFindCircleParam(ToolEdit.FindCircleParam.EdgeMode, strTag, _cogFindCircle);
                _cogFindCircle.Run();
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
                _toolEdit.SetFindCircleParam(ToolEdit.FindCircleParam.Polarity1, strTag, _cogFindCircle);
                _cogFindCircle.Run();
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
                _toolEdit.SetFindCircleParam(ToolEdit.FindCircleParam.Polarity2, strTag, _cogFindCircle);
                _cogFindCircle.Run();
            }
        }

        private void chkFindCircleTolgnore_CheckedChanged(object sender, EventArgs e)
        {
            if (chkFindCircleTolgnore.Checked)
            {
                chkFindCircleTolgnore.ForeColor = Color.Yellow;
                txtValue4.Enabled = true;
            }
            else
            {
                chkFindCircleTolgnore.ForeColor = Color.White;
                txtValue4.Enabled = false;
            }

            if (_bLoad)
            {
                _toolEdit.SetFindCircleParam(ToolEdit.FindCircleParam.Tolgnore, chkFindCircleTolgnore.Checked.ToString(), _cogFindCircle);
                int.TryParse(txtValue4.Text, out var nCnt);
                _toolEdit.SetFindCircleParam(ToolEdit.FindCircleParam.TolgnoreCnt, nCnt.ToString(), _cogFindCircle);

                _cogFindCircle.Run();
            }
        }
    }
}
