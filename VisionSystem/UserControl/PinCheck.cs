using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using DevExpress.XtraEditors;

using System.IO;
using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using System.Drawing.Imaging;

namespace VisionSystem
{

    public partial class PinCheck : DevExpress.XtraEditors.XtraUserControl
    {
        public GlovalVar _var;
        IniFiles ini = new IniFiles();
        LBSoft.IndustrialCtrls.Leds.LBLed[] _lbled = new LBSoft.IndustrialCtrls.Leds.LBLed[19];
        public PinCheck()
        {
            InitializeComponent();
            ledSet();
        }
        private void ledSet()
        {
            _lbled[0] = this.Controls.Find("lbLedP11", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;
            _lbled[1] = this.Controls.Find("lbLedP12", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;
            _lbled[2] = this.Controls.Find("lbLedP13", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;
            _lbled[3] = this.Controls.Find("lbLedP21", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;
            _lbled[4] = this.Controls.Find("lbLedP22", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;
            _lbled[5] = this.Controls.Find("lbLedP23", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;
            _lbled[6] = this.Controls.Find("lbLedP31", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;
            _lbled[7] = this.Controls.Find("lbLedP32", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;
            _lbled[8] = this.Controls.Find("lbLedP33", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;
            _lbled[9] = this.Controls.Find("lbLedP41", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;
            _lbled[10] = this.Controls.Find("lbLedP42", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;
            _lbled[11] = this.Controls.Find("lbLedP43", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;
            _lbled[12] = this.Controls.Find("lbLedP44", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;
            _lbled[13] = this.Controls.Find("lbLedP51", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;
            _lbled[14] = this.Controls.Find("lbLedP52", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;
            _lbled[15] = this.Controls.Find("lbLedP53", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;
            _lbled[16] = this.Controls.Find("lbLedP54", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;
            _lbled[17] = this.Controls.Find("lbLedP61", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;
            _lbled[18] = this.Controls.Find("lbLedP62", true).FirstOrDefault() as LBSoft.IndustrialCtrls.Leds.LBLed;

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

        public void ledonoff(string ledData)
        {
            int ipinCount = 18;

            foreach (char c in ledData)
            {
                if (c == '1')
                    _lbled[ipinCount].State = LBSoft.IndustrialCtrls.Leds.LBLed.LedState.On;
                else
                    _lbled[ipinCount].State = LBSoft.IndustrialCtrls.Leds.LBLed.LedState.Off;
                ipinCount--;

            }



        }

        private void groupControl1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void groupControl1_DoubleClick(object sender, EventArgs e)
        {
            ledonoff("1111111111111111111");
        }
    }
}
