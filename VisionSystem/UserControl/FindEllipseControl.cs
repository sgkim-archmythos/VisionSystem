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
    public partial class FindEllipseControl : UserControl
    {
        CogFindEllipseTool _cogFindEllipse = null;
        TextEdit[] txtValue = new TextEdit[7];

        ToolEdit _toolEdit = new ToolEdit();

        bool _bLoad = false;
        public FindEllipseControl()
        {
            InitializeComponent();
        }

        public void LoadSet(CogFindEllipseTool FindEllipseTool)
        {
            _cogFindEllipse = FindEllipseTool;

            for (int i = 0; i < 7; i++)
                txtValue[i] = Controls.Find(string.Format("txtValue{0}", i + 1), true).FirstOrDefault() as TextEdit;

            _toolEdit.GetFindEllipseParam(out int nCaliperCnt, out double dSearchLen, out double dProjectionLen, out int nSearchDirection, out int nEdgeMode, out int nPolarity1, out int nPolarity2, out double dThreshold, out int nFilterPixel, out double dEdgeLen, out bool bTolgnore, out int nTolognreCnt,  _cogFindEllipse);

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

                _toolEdit.SetFindEllipseParam((ToolEdit.FindEllipseParam)nTag, strValue, _cogFindEllipse);

                _cogFindEllipse.Run();
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
                _toolEdit.SetFindEllipseParam(ToolEdit.FindEllipseParam.SearchDirection, strTag, _cogFindEllipse);
                _cogFindEllipse.Run();
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
                _toolEdit.SetFindEllipseParam(ToolEdit.FindEllipseParam.EdgeMode, strTag, _cogFindEllipse);
                _cogFindEllipse.Run();
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
                _toolEdit.SetFindEllipseParam(ToolEdit.FindEllipseParam.Polarity1, strTag, _cogFindEllipse);
                _cogFindEllipse.Run();
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
                _toolEdit.SetFindEllipseParam(ToolEdit.FindEllipseParam.Polarity2, strTag, _cogFindEllipse);
                _cogFindEllipse.Run();
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
                _toolEdit.SetFindEllipseParam(ToolEdit.FindEllipseParam.Tolgnore, chkFindCircleTolgnore.Checked.ToString(), _cogFindEllipse);

                int.TryParse(txtValue4.Text, out var nCnt);
                _toolEdit.SetFindEllipseParam(ToolEdit.FindEllipseParam.TolonoreCnt, nCnt.ToString(), _cogFindEllipse);
                _cogFindEllipse.Run();
            }
        }
    }
}
