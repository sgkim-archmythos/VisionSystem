using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Cognex.VisionPro.ToolGroup;
using Cognex.VisionPro;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.ImageFile;


namespace VisionSystem
{
    public partial class frmToolSetup : DevExpress.XtraEditors.XtraForm
    {
        frmMain _frmMain;
        frmModel _frmModel;
        CogToolGroup _toolGrp = new CogToolGroup();
        CogImageConvertTool _cogImageConvertTool = new CogImageConvertTool();
        ICogImage _cogImg = null;
        IniFiles ini = new IniFiles();

        public frmToolSetup(frmModel ModelFrm, frmMain MainFrm)
        {
            InitializeComponent();
            _frmModel = ModelFrm;
            _frmMain = MainFrm;

            this.Width = _frmMain.Width;
            this.Height = _frmMain.Height;

            cbModel.Properties.Items.Clear();
            cbModel.Properties.Items.Add("Select Model");

            foreach (string str in _frmMain._var._listModel)
                cbModel.Properties.Items.Add(str);

            cbModel.SelectedIndex = 0;

            cbCamera.Properties.Items.Clear();
            cbCamera.Properties.Items.Add("Select Camera");

            for (int i = 0; i < _frmMain._var._nScreenCnt; i++)
                cbCamera.Properties.Items.Add(string.Format("Camera{0}", i + 1));

            cbCamera.SelectedIndex = 0;

            _toolGrp = CogSerializer.LoadObjectFromFile(Application.StartupPath + "\\vpp\\Default.vpp") as CogToolGroup;
            _cogImageConvertTool = _toolGrp.Tools["CogImageConvertTool1"] as CogImageConvertTool;

            cogToolGroupEdit.Subject = _toolGrp;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void frmToolSetup_Load(object sender, EventArgs e)
        {
            ActiveControl = cogToolGroupEdit;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_cogImg == null)
            {
                MessageBox.Show("Create a scan file after loading the image.", "No Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cbModel.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a Model.", "Select Model", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cbCamera.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a Camera.", "Select Camera", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Do you want to save the working file?", "Save File", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            try
            {
                string strPath = string.Format("{0}\\vpp\\Camera_{1:D2}", Application.StartupPath, cbCamera.SelectedIndex);
                DirectoryInfo dr = new DirectoryInfo(strPath);
                var strFileName = "";

                if (!dr.Exists)
                {
                    dr.Create();
                    strFileName = "01";
                }
                else
                {
                    var strFiles = dr.GetFiles("*.vpp");
                    strFileName = string.Format("{0:D2}", strFiles.Length+1);
                }
                    
                _toolGrp = cogToolGroupEdit.Subject;
                CogSerializer.SaveObjectToFile(_toolGrp, strPath + "\\" + strFileName + ".vpp", typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter), CogSerializationOptionsConstants.Minimum);

                var strImgPath = string.Format("{0}\\Camera_{1:D2}", _frmMain._var._strMasterImagePath, cbCamera.SelectedIndex);
                dr = new DirectoryInfo(strImgPath);
                if (!dr.Exists)
                    dr.Create();

                FileInfo fi = new FileInfo(string.Format("{0}\\{1}.bmp", strImgPath, strFileName));
                if (!fi.Exists)
                {
                    using (CogImageFileBMP cogBMP = new CogImageFileBMP())
                    {
                        cogBMP.Open(string.Format("{0}\\{1}.bmp", strImgPath, strFileName), CogImageFileModeConstants.Write);
                        cogBMP.Append(_cogImageConvertTool.OutputImage);
                        cogBMP.Close();
                    }
                }

                ini.WriteIniFile(strFileName, "Value", txtComment.Text, strPath, "Comment.ini");

                _frmMain.AddMsg("New Working file saved", Color.GreenYellow, true, false, frmMain.MsgType.Alarm);
            }
            catch { }
        }

        private DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);
            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }
            return DateTime.Now;
        }

        private void frmToolSetup_FormClosing(object sender, FormClosingEventArgs e)
        {
            //_frmModel.Show();
            //_frmModel.TopMost = true;
        }

        private void frmToolSetup_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
            else e.Effect = DragDropEffects.None;
        }

        private void frmToolSetup_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] strFilePath = (string[])e.Data.GetData(DataFormats.FileDrop);
                
                foreach (string str in strFilePath)
                {
                    using (Bitmap bmp = new Bitmap(Bitmap.FromFile(str)))
                    {
                        _cogImageConvertTool.InputImage = new CogImage24PlanarColor(bmp);
                        _cogImageConvertTool.Run();

                        _cogImg = _cogImageConvertTool.InputImage;
                    }
                }
            }
            catch { }
        }

        private void btnOpenImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Image Open";
            ofd.InitialDirectory = Application.StartupPath + "\\MasterImage";
            ofd.Filter = "Image File (*.bmp,*.jpg,*.png) | *.bmp;*.jpg;*.png; | All Files (*.*) | *.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (Bitmap bmp = new Bitmap(Bitmap.FromFile(ofd.FileName)))
                {
                    try
                    {
                        _cogImageConvertTool.InputImage = new CogImage24PlanarColor(bmp);
                        _cogImageConvertTool.Run();
                        _cogImg = _cogImageConvertTool.InputImage;
                    }
                    catch { }
                }
            }
        }
    }
}