using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cognex.VisionPro.Dimensioning;
using Cognex.VisionPro;
using DevExpress.XtraEditors;

namespace VisionSystem
{
    public partial class CreateControl : UserControl
    {
        CogCreateCircleTool _cogCreateCircle = null;
        CogCreateEllipseTool _cogCreateEllipseTool= null;
        CogCreateLineTool _cogCreateLineTool = null;

        ToolEdit _toolEdit = new ToolEdit();
        ICogRegion _cogRegion = null;
        CogLine _cogLIne = new CogLine();

        LabelControl[] lblTitle = new LabelControl[5];
        TextEdit[] txtValue = new TextEdit[5];

        private enum ToolType
        {
            Circle,
            Ellipse,
            Line
        }

        ToolType toolType = ToolType.Circle;

        public CreateControl()
        {
            InitializeComponent();
        }

        public void LoadSet(ICogTool cogTool)
        {
            for (var i = 0; i < 5; i++)
            {
                lblTitle[i] = new LabelControl();
                txtValue[i] = new TextEdit();

                lblTitle[i] = Controls.Find(string.Format("lblTitle{0}", i + 1), true).FirstOrDefault() as LabelControl;
                txtValue[i] = Controls.Find(string.Format("txtValue{0}", i + 1), true).FirstOrDefault() as TextEdit;
            }

            if (cogTool.GetType() == typeof(CogCreateCircleTool))
            {
                toolType = ToolType.Circle;
                _cogCreateCircle = cogTool as CogCreateCircleTool;
                _cogRegion = _cogCreateCircle.InputCircle;

                var list = _toolEdit.GetResionSize(_cogRegion);
                SetRegion(list);
                _toolEdit.SetDragMode(_cogRegion);
                
            }
            else if (cogTool.GetType() == typeof(CogCreateEllipseTool))
            {
                toolType = ToolType.Ellipse;
                _cogCreateEllipseTool = cogTool as CogCreateEllipseTool;
                _cogRegion = _cogCreateEllipseTool.Ellipse;

                var list = _toolEdit.GetResionSize(_cogRegion);
                SetRegion(list);
                _toolEdit.SetDragMode(_cogRegion);
            }
            else if (cogTool.GetType() == typeof(CogCreateLineTool))
            {
                toolType = ToolType.Line;
                _cogCreateLineTool = cogTool as CogCreateLineTool;
                _cogLIne = _cogCreateLineTool.Line;
                
                var list = new List<string>();
                list.Add(string.Format("X :,{0:F3}", _cogLIne.X));
                list.Add(string.Format("Y :,{0:F3}", _cogLIne.Y));
                list.Add(string.Format("회전 :,{0:F3}", _toolEdit.RadToAngle(_cogLIne.Rotation)));

                SetRegion(list);

                _toolEdit.SetToolDragMode(_cogCreateLineTool);
            }

            _toolEdit.OnDragSize = OnDragSize;
        }

        private void OnDragSize(List<string> list, ToolEdit.RegionType regionType)
        {
            SetRegion(list);
            ToolRun();
        }

        private void SetRegion(List<string> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (i < list.Count)
                {
                    var strTemp = list[i].Split(',');
                    lblTitle[i].Text = strTemp[0];
                    txtValue[i].Text = strTemp[1];

                    lblTitle[i].Visible = true;
                    txtValue[i].Visible = true;
                }
                else
                {
                    lblTitle[i].Visible = false;
                    txtValue[i].Visible = false;
                }
            }
        }

        private void txtValue1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var dValue = new double[5];

                for (int i = 0; i < 5; i++)
                    double.TryParse(txtValue[i].Text, out dValue[i]);

                ICogTool cogTool = null;

                if (toolType == ToolType.Circle)
                    cogTool = _cogCreateCircle;
                else if (toolType == ToolType.Ellipse)
                    cogTool = _cogCreateEllipseTool;
                else
                    cogTool = _cogCreateLineTool;

                _toolEdit.SetRegionSize(0, dValue, cogTool);
                ToolRun();
            }
        }

        private void ToolRun()
        {
            if (toolType == ToolType.Circle)
                _cogCreateCircle.Run();
            else if (toolType == ToolType.Circle)
                _cogCreateEllipseTool.Run();
            else
                _cogCreateLineTool.Run();
        }
    }
}
