using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Cognex.VisionPro;

namespace VisionSystem
{
    public partial class frmNGPopup : DevExpress.XtraEditors.XtraForm
    {
        public frmNGPopup()
        {
            InitializeComponent();
        }
        public void NGPopImage(string strMasterImage, string strResultImage)
        {
            try
            {
                Invoke(new EventHandler(delegate
                {
                    if (System.IO.File.Exists(strMasterImage))
                        picMaster.Image = Bitmap.FromFile(strMasterImage);
                    if (System.IO.File.Exists(strResultImage))
                    {
                        picResult.Image = Bitmap.FromFile(strResultImage);
                        picResult.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
                    }

                }));
            }
            catch (Exception ex)
            {

            }
        }
        private void pictureEdit1_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void frmNGPopup_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Hide();
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            Invoke(new EventHandler(delegate
            {
                picMaster.Image = null;
                picResult.Image = null;
            }));
            this.Hide();
        }
    }
}