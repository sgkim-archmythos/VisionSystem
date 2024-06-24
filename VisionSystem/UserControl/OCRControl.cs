using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cognex.VisionPro.OCRMax;

namespace VisionSystem
{
    public partial class OCRControl : UserControl
    {
        CogOCRMaxTool _cogOCRMax = new CogOCRMaxTool();
        public OCRControl()
        {
            InitializeComponent();
        }

        public void LoadSet(CogOCRMaxTool OCRMaxTool)
        {
            _cogOCRMax = OCRMaxTool;
        }
    }
}
