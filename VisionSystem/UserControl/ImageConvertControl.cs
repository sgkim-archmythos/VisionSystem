using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cognex.VisionPro.ImageProcessing;

namespace VisionSystem
{
    public partial class ImageConvertControl : UserControl
    {
        CogImageConvertTool _cogImageConvertTool = null;
        ToolEdit _toolEdit = new ToolEdit();
        bool _bLoad = false;

        RadioButton[] radMode = new RadioButton[12];
        public ImageConvertControl()
        {
            InitializeComponent();
        }

        public void LoadSet(CogImageConvertTool ConvertTool)
        {
            Invoke(new EventHandler(delegate
            {
                _cogImageConvertTool = ConvertTool;

                for (var i = 0; i < 12; i++)
                {
                    radMode[i] = new RadioButton();
                    radMode[i] = Controls.Find(string.Format("rad{0}", i + 1), true).FirstOrDefault() as RadioButton;
                    radMode[i].Tag = i;
                }

                var nValue = _toolEdit.GetConvertRunMode(_cogImageConvertTool);
                radMode[nValue].Checked = true;

                _bLoad = true;
            }));
        }

        private void rad1_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
                (sender as RadioButton).ForeColor = Color.Yellow;
            else
                (sender as RadioButton).ForeColor = Color.White;

            if (_bLoad)
            {
                int.TryParse((sender as RadioButton).Tag.ToString(), out var nMode);
                _toolEdit.SetConvertRunMode(nMode, _cogImageConvertTool);

                _cogImageConvertTool.Run();
            }
        }
    }
}
