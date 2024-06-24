using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cognex.VisionPro.ColorSegmenter;

namespace VisionSystem
{
    public partial class ColorSegmentControl : UserControl
    {
        CogColorSegmenterTool _cogColorSegmenter = new CogColorSegmenterTool();
        public ColorSegmentControl()
        {
            InitializeComponent();
        }

        public void LoadSet(CogColorSegmenterTool cogColorSegmenterTool)
        {
            _cogColorSegmenter = cogColorSegmenterTool;

            pngGrid.Size = new Size(357, 187);
            pngGrid.Location = new Point(5, 52);
            pnlColor.Size = pngGrid.Size;
            pnlColor.Location = pngGrid.Location;
        }
    }
}
