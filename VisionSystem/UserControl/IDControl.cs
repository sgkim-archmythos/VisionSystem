using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cognex.VisionPro.ID;
using DevExpress.XtraEditors;
using Cognex.VisionPro;



namespace VisionSystem
{
    public partial class IDControl : UserControl
    {
        CogIDTool _cogID = new CogIDTool();
        CheckEdit[] chkValue = new CheckEdit[15];
        ToolEdit _toolEdit = new ToolEdit();

        LabelControl[] lblTitle = new LabelControl[8];
        TextEdit[] txtSize = new TextEdit[8];

        bool _bLoad = false;

        string[] _strSymbol = new string[15] { "데이터 매트릭스", "QR 코드", "코드 128", "UPC/EAN", "코드 39", "코드 93", "인터리브된 2/5", "Codabar", "Pharmacode", "GS1 DataBar", "PDF417", "EAN.UCC 합성", "POSTNET", "PLANET", "4상 우편용" };

        ICogRegion _cogRegion = null;
        bool[] _bSymbol = new bool[15] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
        
        public IDControl()
        {
            InitializeComponent();
        }

        public void LoadSet(CogIDTool IDTool)
        {
            _cogID = IDTool;

            _cogRegion = _cogID.Region;

            for (var i = 0; i < 8; i++)
            {
                lblTitle[i] = new LabelControl();
                txtSize[i] = new TextEdit();

                lblTitle[i] = Controls.Find(string.Format("lblTitle{0}", i + 1), true).FirstOrDefault() as LabelControl;
                txtSize[i] = Controls.Find(string.Format("txtSize{0}", i + 1), true).FirstOrDefault() as TextEdit;
            }

            for (int i=0; i<_strSymbol.Length; i++)
            {
                tabConfig.TabPages[i].Text = _strSymbol[i];
                tabConfig.TabPages[i].PageVisible = false;

                chkValue[i] = new CheckEdit();
                chkValue[i] = Controls.Find(string.Format("chkValue{0}", i + 1), true).FirstOrDefault() as CheckEdit;
                chkValue[i].Tag = i;
            }

            cbMode.SelectedIndex = (int)_cogID.RunParams.ProcessingMode;
            txtNumtoFind.Text = _cogID.RunParams.NumToFind.ToString();
            chkAllowSymbol.Checked = _cogID.RunParams.AllowIdenticalSymbols;
            chkAllowParitaResult.Checked = _cogID.RunParams.AllowPartialResults;
            chkTimeout.Checked = _cogID.RunParams.TimeoutEnabled;
            txtTimeout.Text = _cogID.RunParams.Timeout.ToString();

            if (_cogID.RunParams.DecodedStringCodePage == CogIDCodePageConstants.ANSILatin1)
                cbDecodeCode.SelectedIndex = 0;
            else if (_cogID.RunParams.DecodedStringCodePage == CogIDCodePageConstants.Japanese)
                cbDecodeCode.SelectedIndex = 1;
            else if (_cogID.RunParams.DecodedStringCodePage == CogIDCodePageConstants.Korean)
                cbDecodeCode.SelectedIndex = 2;
            else if (_cogID.RunParams.DecodedStringCodePage == CogIDCodePageConstants.SimplifiedChinese)
                cbDecodeCode.SelectedIndex = 3;
            else if (_cogID.RunParams.DecodedStringCodePage == CogIDCodePageConstants.UTF8)
                cbDecodeCode.SelectedIndex = 4;
            else if (_cogID.RunParams.DecodedStringCodePage == CogIDCodePageConstants.UTF16LE)
                cbDecodeCode.SelectedIndex = 5;
            else if (_cogID.RunParams.DecodedStringCodePage == CogIDCodePageConstants.UTF16BE)
                cbDecodeCode.SelectedIndex = 6;
            else if (_cogID.RunParams.DecodedStringCodePage == CogIDCodePageConstants.UTF32LE)
                cbDecodeCode.SelectedIndex = 7;
            else if (_cogID.RunParams.DecodedStringCodePage == CogIDCodePageConstants.UTF32BE)
                cbDecodeCode.SelectedIndex = 8;

            chkValue[0].Checked = _cogID.RunParams.DataMatrix.Enabled;
            chkValue[1].Checked = _cogID.RunParams.QRCode.Enabled;
            chkValue[2].Checked = _cogID.RunParams.Code128.Enabled;
            chkValue[3].Checked = _cogID.RunParams.UpcEan.Enabled;
            chkValue[4].Checked = _cogID.RunParams.Code39.Enabled;
            chkValue[5].Checked = _cogID.RunParams.Code93.Enabled;
            chkValue[6].Checked = _cogID.RunParams.I2Of5.Enabled;
            chkValue[7].Checked = _cogID.RunParams.Codabar.Enabled;
            chkValue[8].Checked = _cogID.RunParams.Pharmacode.Enabled;
            chkValue[9].Checked = _cogID.RunParams.DataBar.Enabled;
            chkValue[10].Checked = _cogID.RunParams.PDF417.Enabled;
            chkValue[11].Checked = _cogID.RunParams.EANUCCComposite.Enabled;
            chkValue[11].Checked = _cogID.RunParams.Postnet.Enabled;
            chkValue[13].Checked = _cogID.RunParams.Planet.Enabled;
            chkValue[14].Checked = _cogID.RunParams.FourState.Enabled;

            for (int i = 0; i < 15; i++)
                _bSymbol[i] = chkValue[i].Checked;

            TrainStatus();

            cbBlobRegion.SelectedIndex = _toolEdit.GetRegionIndex(_cogRegion);

            _toolEdit.OnDragSize = OnDragSize;
            _bLoad = true;
        }

        private void OnDragSize(List<string> list, ToolEdit.RegionType regionType)
        {
            SetRegionValue(regionType, list);
            _cogID.Run();
        }

        private void SetRegionValue(ToolEdit.RegionType regionType, List<string> list)
        {
            if (regionType == ToolEdit.RegionType.CogPolygon)
            {
                pnlRegion2.Dock = DockStyle.Fill;

                dgPolygon.Rows.Clear();

                for (var i = 0; i < list.Count; i++)
                {
                    var strValue = list[i].Split(',');
                    dgPolygon.Rows.Add((i + 1).ToString(), strValue[0], strValue[1]);
                }

                pnlRegion1.Visible = false;
                pnlRegion2.Visible = true;
            }
            else
            {
                pnlRegion1.Dock = DockStyle.Fill;

                for (var i = 0; i < 8; i++)
                {
                    if (list.Count > i)
                    {
                        var strValue = list[i].Split(',');
                        lblTitle[i].Text = strValue[0];
                        txtSize[i].Text = strValue[1];
                        lblTitle[i].Visible = true;
                        txtSize[i].Visible = true;
                    }
                    else
                    {
                        lblTitle[i].Visible = false;
                        txtSize[i].Visible = false;
                    }
                }

                pnlRegion1.Visible = true;
                pnlRegion2.Visible = false;
            }
        }


        private void cbDecodeCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_bLoad)
                return;

            if (cbDecodeCode.SelectedIndex == 0)
                _cogID.RunParams.DecodedStringCodePage = CogIDCodePageConstants.ANSILatin1;
            else if (cbDecodeCode.SelectedIndex == 1)
                _cogID.RunParams.DecodedStringCodePage = CogIDCodePageConstants.Japanese;
            else if (cbDecodeCode.SelectedIndex == 2)
                _cogID.RunParams.DecodedStringCodePage = CogIDCodePageConstants.Korean;
            else if (cbDecodeCode.SelectedIndex == 3)
                _cogID.RunParams.DecodedStringCodePage = CogIDCodePageConstants.SimplifiedChinese;
            else if (cbDecodeCode.SelectedIndex == 4)
                _cogID.RunParams.DecodedStringCodePage = CogIDCodePageConstants.UTF8;
            else if (cbDecodeCode.SelectedIndex == 5)
                _cogID.RunParams.DecodedStringCodePage = CogIDCodePageConstants.UTF16LE;
            else if (cbDecodeCode.SelectedIndex == 6)
                _cogID.RunParams.DecodedStringCodePage = CogIDCodePageConstants.UTF16BE;
            else if (cbDecodeCode.SelectedIndex == 7)
                _cogID.RunParams.DecodedStringCodePage = CogIDCodePageConstants.UTF32LE;
            else if (cbDecodeCode.SelectedIndex == 8)
                _cogID.RunParams.DecodedStringCodePage = CogIDCodePageConstants.UTF32BE;
        }

        private void cbMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_bLoad)
                _cogID.RunParams.ProcessingMode = (CogIDProcessingModeConstants)cbMode.SelectedIndex;
        }

        private void txtNumtoFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_bLoad)
                return;

            int.TryParse(txtNumtoFind.Text, out var nCnt);
            _cogID.RunParams.NumToFind = nCnt;
        }

        private void chkAllowSymbol_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAllowSymbol.Checked)
                chkAllowSymbol.ForeColor = Color.Yellow;
            else
                chkAllowSymbol.ForeColor = Color.White;

            if (_bLoad)
            {
                _cogID.RunParams.AllowIdenticalSymbols = chkAllowSymbol.Checked;
                _cogID.Run();
            }
        }

        private void chkAllowParitaResult_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAllowParitaResult.Checked)
                chkAllowParitaResult.ForeColor = Color.Yellow;
            else
                chkAllowParitaResult.ForeColor = Color.White;

            if (_bLoad)
            {
                _cogID.RunParams.AllowPartialResults = chkAllowParitaResult.Checked;
                _cogID.Run();
            }
        }

        private void chkTimeout_CheckedChanged(object sender, EventArgs e)
        {
            if (chkTimeout.Checked)
                chkTimeout.ForeColor = Color.Yellow;
            else
                chkTimeout.ForeColor = Color.White;

            if (_bLoad)
            {
                _cogID.RunParams.TimeoutEnabled = chkTimeout.Checked;

                int.TryParse(txtTimeout.Text, out var dValue);
                _cogID.RunParams.Timeout= dValue;

                _cogID.Run();
            }
        }

        private void txtTimeout_KeyDown(object sender, KeyEventArgs e)
        {
            int.TryParse(txtTimeout.Text, out var dValue);

            if (dValue > 0)
                _cogID.RunParams.Timeout = dValue;
        }

        private void btnTrain_Click(object sender, EventArgs e)
        {
            if (btnTrain.Text == "학습")
                _cogID.RunParams.Train(_cogID.InputImage, _cogRegion);
            else
                _cogID.RunParams.Untrain();

            TrainStatus();
        }

        private void TrainStatus()
        {
            if (_cogID.RunParams.Trained)
                btnTrain.Text = "학습 취소";
            else
                btnTrain.Text = "학습";
        }

        private void chkValue1_CheckedChanged(object sender, EventArgs e)
        {
            if (!_bLoad)
                return;

            int.TryParse((sender as CheckEdit).Tag.ToString(), out var nTag);

            if (nTag == 0 || nTag == 1)
            {
                if (nTag == 0)
                {
                    if (chkValue[0].Checked)
                    {
                        for (int i = 1; i < 15; i++)
                            chkValue[i].Checked = false;

                        chkValue[0].ForeColor = Color.Yellow;
                        tabConfig.TabPages[0].PageVisible = true;
                    }
                    else
                    {
                        chkValue[0].ForeColor = Color.White;
                        tabConfig.TabPages[0].PageVisible = false;
                    }

                    _bSymbol[0] = chkValue[0].Checked;
                }
                else
                {
                    if (chkValue[1].Checked)
                    {
                        chkValue[1].Checked = false;
                        for (int i = 2; i < 15; i++)
                            chkValue[i].Checked = false;

                        chkValue[1].ForeColor = Color.Yellow;
                        tabConfig.TabPages[1].PageVisible = true;
                    }
                    else
                    {
                        chkValue[1].ForeColor = Color.White;
                        tabConfig.TabPages[1].PageVisible = false;
                    }

                    _bSymbol[1] = chkValue[1].Checked;
                }
            }
            else
            {
                if ((sender as CheckEdit).Checked)
                {
                    (sender as CheckEdit).ForeColor = Color.Yellow;
                    tabConfig.TabPages[1].PageVisible = true;
                }
                else
                {
                    (sender as CheckEdit).ForeColor = Color.White;
                    tabConfig.TabPages[1].PageVisible = false;
                }

                _bSymbol[nTag] = (sender as CheckEdit).Checked;
            }

            SetSymbol();
        }

        private void SetSymbol()
        {
            for (int i=0; i<15; i++)
            {
                if (i == 0)
                    _cogID.RunParams.DataMatrix.Enabled = _bSymbol[i];
                else if (i == 1)
                    _cogID.RunParams.QRCode.Enabled = _bSymbol[i];
                else if (i == 2)
                    _cogID.RunParams.Code128.Enabled = _bSymbol[i];
                else if (i == 3)
                    _cogID.RunParams.UpcEan.Enabled = _bSymbol[i];
                else if (i == 4)
                    _cogID.RunParams.Code39.Enabled = _bSymbol[i];
                else if (i == 5)
                    _cogID.RunParams.Code93.Enabled = _bSymbol[i];
                else if (i == 6)
                    _cogID.RunParams.I2Of5.Enabled = _bSymbol[i];
                else if (i == 7)
                    _cogID.RunParams.Codabar.Enabled = _bSymbol[i];
                else if (i == 8)
                    _cogID.RunParams.Pharmacode.Enabled = _bSymbol[i];
                else if (i == 9)
                    _cogID.RunParams.DataBar.Enabled = _bSymbol[i];
                else if (i == 10)
                    _cogID.RunParams.PDF417.Enabled = _bSymbol[i];
                else if (i == 11)
                    _cogID.RunParams.EANUCCComposite.Enabled = _bSymbol[i];
                else if (i == 12)
                    _cogID.RunParams.Postnet.Enabled = _bSymbol[i];
                else if (i == 13)
                    _cogID.RunParams.Planet.Enabled = _bSymbol[i];
                else if (i == 114)
                    _cogID.RunParams.FourState.Enabled = _bSymbol[i];
            }
        }

        private void cbBlobRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_bLoad)
                _toolEdit.SetRegion((ToolEdit.RegionType)cbBlobRegion.SelectedIndex, _cogID);

            _cogRegion = _cogID.Region;
            _toolEdit.SetDragMode(_cogRegion);
            var list = _toolEdit.GetResionSize(_cogRegion);

            SetRegionValue((ToolEdit.RegionType)cbBlobRegion.SelectedIndex, list);

            _cogID.Run();
        }

        private void txtSize1_KeyDown(object sender, KeyEventArgs e)
        {
            var dValue = new double[8];
            for (var i = 0; i < 8; i++)
                double.TryParse(txtSize[i].Text, out dValue[i]);

            _toolEdit.SetRegionSize(0, dValue, _cogID);
            _cogID.Run();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var nRow = dgPolygon.Rows.Count;

            if (nRow == 0)
            {
                _toolEdit.SetRegionSize(0, new double[2] { 100, 100 }, _cogID);
                _toolEdit.SetRegionSize(0, new double[2] { 200, 100 }, _cogID);
                _toolEdit.SetRegionSize(0, new double[2] { 200, 200 }, _cogID);
                _toolEdit.SetRegionSize(0, new double[2] { 100, 200 }, _cogID);
            }
            else
            {
                double.TryParse(dgPolygon.Rows[nRow - 1].Cells[1].Value.ToString(), out var dPosX);
                double.TryParse(dgPolygon.Rows[nRow - 1].Cells[2].Value.ToString(), out var dPosY);

                _toolEdit.SetRegionSize(nRow, new double[2] { dPosX - 50, dPosY }, _cogID);
            }

            var list = _toolEdit.GetResionSize(_cogRegion);
            SetRegionValue((ToolEdit.RegionType)cbBlobRegion.SelectedIndex, list);
            _cogID.Run();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var nIdx = dgPolygon.CurrentRow.Index;

            if (nIdx == -1)
                return;

            _toolEdit.SetRegionSize(nIdx, null, _cogID);

            var list = _toolEdit.GetResionSize(_cogRegion);
            SetRegionValue((ToolEdit.RegionType)cbBlobRegion.SelectedIndex, list);
            _cogID.Run();
        }
    }
}
