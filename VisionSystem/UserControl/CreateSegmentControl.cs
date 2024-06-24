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

namespace VisionSystem
{
    public partial class CreateSegmentControl : UserControl
    {
        CogCreateSegmentAvgSegsTool _cogCreateSegmentAvg = null;
        CogCreateSegmentTool _cogCreateSegment = null;

        ToolEdit _toolEdit = new ToolEdit();

        private enum ToolType
        {
            CogCreateSegmentTool,
            CogCreateSegmentAvgSegsTool
        }

        ToolType _toolType = ToolType.CogCreateSegmentTool;

        public CreateSegmentControl()
        {
            InitializeComponent();
        }

        public void LoadSet(ICogTool cogTool)
        {
            
            if (cogTool.GetType() == typeof(CogCreateSegmentAvgSegsTool))
            {
                _toolType = ToolType.CogCreateSegmentAvgSegsTool;
                _cogCreateSegmentAvg = cogTool as CogCreateSegmentAvgSegsTool;

                txtValueA1.Text = string.Format("{0:F3}", _cogCreateSegmentAvg.SegmentA.StartX);
                txtValueA2.Text = string.Format("{0:F3}", _cogCreateSegmentAvg.SegmentA.StartY);
                txtValueA3.Text = string.Format("{0:F3}", _cogCreateSegmentAvg.SegmentA.EndX);
                txtValueA4.Text = string.Format("{0:F3}", _cogCreateSegmentAvg.SegmentA.EndY);

                txtValueB1.Text = string.Format("{0:F3}", _cogCreateSegmentAvg.SegmentB.StartX);
                txtValueB2.Text = string.Format("{0:F3}", _cogCreateSegmentAvg.SegmentB.StartY);
                txtValueB3.Text = string.Format("{0:F3}", _cogCreateSegmentAvg.SegmentB.EndX);
                txtValueB4.Text = string.Format("{0:F3}", _cogCreateSegmentAvg.SegmentB.EndY);

                _cogCreateSegmentAvg.SegmentA.DraggingStopped += new CogDraggingStoppedEventHandler(OnDraggingAStop);
                _cogCreateSegmentAvg.SegmentA.DraggingStopped += new CogDraggingStoppedEventHandler(OnDraggingBStop);

                gpSegmentA.Visible = true;
                gpSenmentB.Visible = true;
            }
            else if (cogTool.GetType() == typeof(CogCreateSegmentTool))
            {
                _toolType = ToolType.CogCreateSegmentTool;
                _cogCreateSegment = cogTool as CogCreateSegmentTool;

                txtValueA1.Text = string.Format("{0:F3}", _cogCreateSegment.Segment.StartX);
                txtValueA2.Text = string.Format("{0:F3}", _cogCreateSegment.Segment.StartY);
                txtValueA3.Text = string.Format("{0:F3}", _cogCreateSegment.Segment.EndX);
                txtValueA4.Text = string.Format("{0:F3}", _cogCreateSegment.Segment.EndY);

                _cogCreateSegment.Segment.DraggingStopped += new CogDraggingStoppedEventHandler(OnDraggingAStop);

                gpSegmentA.Visible = true;
                gpSenmentB.Visible = false;
            }
        }

        private void ToolRun()
        {
            if (_toolType == ToolType.CogCreateSegmentAvgSegsTool)
                _cogCreateSegmentAvg.Run();
            else
                _cogCreateSegment.Run();
        }

        private void OnDraggingAStop(object sender, CogDraggingEventArgs e)
        {
            var cogLine = e.DragGraphic as CogLineSegment;

            txtValueA1.Text = string.Format("{0:F3}", cogLine.StartX);
            txtValueA2.Text = string.Format("{0:F3}", cogLine.StartY);
            txtValueA3.Text = string.Format("{0:F3}", cogLine.EndX);
            txtValueA4.Text = string.Format("{0:F3}", cogLine.EndY);
        }



        private void OnDraggingBStop(object sender, CogDraggingEventArgs e)
        {
            var cogLine = e.DragGraphic as CogLineSegment;

            txtValueB1.Text = string.Format("{0:F3}", cogLine.StartX);
            txtValueB2.Text = string.Format("{0:F3}", cogLine.StartY);
            txtValueB3.Text = string.Format("{0:F3}", cogLine.EndX);
            txtValueB4.Text = string.Format("{0:F3}", cogLine.EndY);
        }

        private void txtValueA1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var dValue = new double[4];

                double.TryParse(txtValueA1.Text, out dValue[0]);
                double.TryParse(txtValueA2.Text, out dValue[1]);
                double.TryParse(txtValueA3.Text, out dValue[2]);
                double.TryParse(txtValueA4.Text, out dValue[3]);

                if (_toolType == ToolType.CogCreateSegmentAvgSegsTool)
                {
                    _cogCreateSegmentAvg.SegmentA.StartX = dValue[0];
                    _cogCreateSegmentAvg.SegmentA.StartY = dValue[1];
                    _cogCreateSegmentAvg.SegmentA.EndX = dValue[2];
                    _cogCreateSegmentAvg.SegmentA.EndY = dValue[3];
                }
                else
                {
                    _cogCreateSegment.Segment.StartX = dValue[0];
                    _cogCreateSegment.Segment.StartY = dValue[1];
                    _cogCreateSegment.Segment.EndX = dValue[2];
                    _cogCreateSegment.Segment.EndY = dValue[3];
                }

                ToolRun();
            }
        }

        private void txtValueB1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var dValue = new double[4];

                double.TryParse(txtValueB1.Text, out dValue[0]);
                double.TryParse(txtValueB2.Text, out dValue[1]);
                double.TryParse(txtValueB3.Text, out dValue[2]);
                double.TryParse(txtValueB4.Text, out dValue[3]);

                if (_toolType == ToolType.CogCreateSegmentAvgSegsTool)
                {
                    _cogCreateSegmentAvg.SegmentB.StartX = dValue[0];
                    _cogCreateSegmentAvg.SegmentB.StartY = dValue[1];
                    _cogCreateSegmentAvg.SegmentB.EndX = dValue[2];
                    _cogCreateSegmentAvg.SegmentB.EndY = dValue[3];
                }

                ToolRun();
            }
        }
    }
}
