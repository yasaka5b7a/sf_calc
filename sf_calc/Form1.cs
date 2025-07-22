using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Windows.Forms;


namespace sf_calc
{
    public partial class Form1 : Form
    {
        #region メンバクラス
        public class Test
        {
            public string Name { get; set; }
            public decimal Lamda { get; set; }
            public decimal Cap { get; set; }
            public decimal Roh { get; set; }
        }

        public class TestList : BindingList<Test> { }

        #endregion

        #region メンバ変数

        public double[,] Zth = new double[5, 512];
        public double[,] ZthDFT = new double[5, 512];
        public double[,] ZthDFTi = new double[5, 512];
        public double[,] RthF = new double[5, 512];
        public double[,] RthFi = new double[5, 512];
        public double[,] RthC = new double[5, 512];
        public double[,] CthC = new double[5, 512];
        public double[,] devZth = new double[5, 512];
        public double[,] devZthDFT = new double[5, 512];
        public double[,] stime = new double[5, 512];
        public double[] Rthcx = new double[5];
        public double[,] z = new double[5, 512];
        public double[] dz = new double[5];
        public double[] re_M = new double[4096];
        public double[] im_M = new double[4096];
        public double[] re_W = new double[4096];
        public double[] im_W = new double[4096];

        public double[] Tjmax = new double[5];   //Tjmax
        public double[] Powerstep = new double[5];
        public double[] HeatsinkTemp = new double[5];
        public double[] Sensitivity = new double[5];
        public double[] CutOffTime = new double[5];   //CutOffTime
        public double[] TriggerTime = new double[5];   //TriggerTime
        public int[] averageCount = new int[5];   //平均化データ数
        public double[] area = new double[5];   //発熱面積
        public double[] Phi0 = { 0.45D, 0.45D, 0.45D, 0.45D, 0.45D };
        public double[] Sigma = { 0.05D, 0.05D, 0.05D, 0.05D, 0.05D };

        // default Silicon
        public double c = 0.7046;           // specific heat [J/gK]
        public double roh = 0.002329;       // density [g/mm3]
        public double lamda = 0.149;        // thermal conductivity [W/mmK]

        public String RApath;
        public int nowChannel = 0;

        public double[] RefTime = new double[60000];
        public double[] RefVf = new double[60000];
        public bool Rstat = false;


        private bool[] dataRead = new bool[5] { false, false, false, false, false };
        private bool[] dataCalc = new bool[5] { false, false, false, false, false };

        private Char[] SepString = new Char[] { ' ', '\t', '=', '#', ',' };

        private Color[] linecolor = new Color[5] { Color.Blue, Color.Gold, Color.Red, Color.Green, Color.Purple };

        public string[] DataFileName = new string[5];
        public string[] ListName = new string[5];


        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>

        public Form1()
        {
            InitializeComponent();
        }

        #endregion

        #region publicメソッド

        #region グラフ 描画Zth
        /// <summary>
        /// Zthデータをチャートに描画
        /// </summary>
        public void ShowChartZth()
        {
            // グラフのデータクリア
            chartZth.Series[nowChannel].Points.Clear();
            chartdZ.Series[nowChannel].Points.Clear();

            for (int i = 0; i < 400; i++)
            {
                // グラフにデータ追加
                chartZth.Series[nowChannel].Points.AddXY(Math.Log10(stime[nowChannel, i]), Zth[nowChannel, i]);
                chartdZ.Series[nowChannel].Points.AddXY(Math.Log10(stime[nowChannel, i]), devZth[nowChannel, i]);
            }

        }

        #endregion

        #region グラフ描画 RthFoster
        /// <summary>
        ///Foster型のRthデータをチャートに描画
        /// </summary>
        public void ShowChartRthF()
        {
            double RthFmax = 0;

            // グラフのデータクリア
            chartRthF.Series[nowChannel].Points.Clear();
            chartZth.Series[nowChannel + 5].Points.Clear();
            chartdZ.Series[nowChannel + 5].Points.Clear();

            for (int i = 0; i < 400; i++)
            {
                double rr = RthF[nowChannel, i];

                // グラフにデータ追加
                chartRthF.Series[nowChannel].Points.AddXY(Math.Log10(stime[nowChannel, i]), rr);
                chartZth.Series[nowChannel + 5].Points.AddXY(Math.Log10(stime[nowChannel, i]), ZthDFT[nowChannel, i]);
                chartdZ.Series[nowChannel + 5].Points.AddXY(Math.Log10(stime[nowChannel, i]), devZthDFT[nowChannel, i]);
                if (rr > 0)
                {
                    RthFmax += rr;
                }
            }

            switch (nowChannel)
            {
                case 0:
                    tB_Ch1_Foster.Text = String.Format("{0:F4}", RthFmax * dz[0]);
                    break;
                case 1:
                    tB_Ch2_Foster.Text = String.Format("{0:F4}", RthFmax * dz[1]);
                    break;
                case 2:
                    tB_Ch3_Foster.Text = String.Format("{0:F4}", RthFmax * dz[2]);
                    break;
                case 3:
                    tB_Ch4_Foster.Text = String.Format("{0:F4}", RthFmax * dz[3]);
                    break;
                case 4:
                    tB_Ch5_Foster.Text = String.Format("{0:F4}", RthFmax * dz[4]);
                    break;
                default:
                    break;
            }
            this.sfc();
        }

        #endregion

        #region グラフ描画 構造関数
        /// <summary>
        ///構造関数をチャートに描画
        /// </summary>
        public void ShowChartRnCn()
        {
            try
            {
                if (!dataRead[nowChannel])
                {
                    this.label_cth.Text = String.Format("*****");
                    this.label_tc.Text = String.Format("*****");
                    if (chartRnCn.ChartAreas[0].CursorX.Position >= 0.0)
                    {
                        numericUpDownCurXd.Value = (decimal)chartRnCn.ChartAreas[0].CursorX.Position;
                    }
                    return;
                }

                // グラフのデータクリア
                chartRnCn.Series[nowChannel].Points.Clear();

                // X軸の最大最小を設定
                chartRnCn.ChartAreas[0].AxisX.Maximum = (double)numericUpDown_xmax.Value;
                chartRnCn.ChartAreas[0].AxisX.Minimum = (double)numericUpDown_xmin.Value;

                chartRnCn.ChartAreas[0].CursorX.Position = (double)numericUpDownCurX.Value;
                chartRnCn.ChartAreas[0].CursorY.Position = (double)numericUpDownCurY.Value;
                double cux = (double)numericUpDownCurX.Value;
                double curC = (double)numericUpDownCurY.Value;
                double cuy = 0.0;
                double rx = 0.0;
                double ry = 0.0;
                for (int i = 0; i < 400; i++)
                {
                    double xx = RthC[nowChannel, i];
                    double yy = CthC[nowChannel, i];


                    if (yy > 0.0)
                    {
                        // グラフにデータ追加
                        chartRnCn.Series[nowChannel].Points.AddXY(xx, Math.Log10(yy));
                        if (xx >= cux && rx <= cux)
                        {
                            cuy = ry + (cux - rx) / (xx - rx) * (yy - ry);
                        }
                        rx = xx;
                        ry = yy;
                    }
                }
                this.label_cth.Text = String.Format("{0:E3}", cuy);
                this.label_tc.Text = String.Format("{0:E3}", cuy * cux);
                numericUpDownCurXd.Value = (decimal)chartRnCn.ChartAreas[0].CursorX.Position;
                tB_curCth.Text = String.Format("{0:E3}", Math.Pow(10D, curC));
                for (int chn = 0; chn < 5; chn++)
                {
                    if (dataRead[chn])
                    {
                        double cx1 = 0D, cx2 = 0D;
                        double cy1 = 0D, cy2 = 0D;

                        for (int i = 0; i < chartRnCn.Series[chn].Points.Count; i++)
                        {

                            if (chartRnCn.Series[chn].Points[i].YValues[0] > curC)
                            {
                                cx2 = chartRnCn.Series[chn].Points[i].XValue;
                                cy2 = chartRnCn.Series[chn].Points[i].YValues[0];
                                if (i > 0)
                                {
                                    cx1 = chartRnCn.Series[chn].Points[i - 1].XValue;
                                    cy1 = chartRnCn.Series[chn].Points[i - 1].YValues[0];
                                }
                                break;
                            }

                        }
                        double cx = cx1 + (cx2 - cx1) / (cy2 - cy1) * (curC - cy1);
                        Rthcx[chn] = cx;
                        switch (chn)
                        {
                            case 0:
                                label_Rth_ch1.Text = String.Format("{0:E3}", cx);
                                break;
                            case 1:
                                label_Rth_ch2.Text = String.Format("{0:E3}", cx);
                                break;
                            case 2:
                                label_Rth_ch3.Text = String.Format("{0:E3}", cx);
                                break;
                            case 3:
                                label_Rth_ch4.Text = String.Format("{0:E3}", cx);
                                break;
                            case 4:
                                label_Rth_ch5.Text = String.Format("{0:E3}", cx);
                                break;
                            default:
                                break;

                        }

                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}", ex.ToString()));
            }
        }

        #endregion

        #region グラフ描画 構造関数(微分)
        /// <summary>
        ///構造関数(微分)をチャートに描画
        /// </summary>
        public void ShowChartdCdR()
        {
            try
            {
                if (!dataRead[nowChannel])
                {
                    this.label_dcdr.Text = String.Format("*****");
                    if (chartdRdC.ChartAreas[0].CursorX.Position >= 0.0)
                    {
                        numericUpDownCurX.Value = (decimal)chartdRdC.ChartAreas[0].CursorX.Position;
                    }
                    return;
                }


                // グラフのデータクリア
                chartdRdC.Series[nowChannel].Points.Clear();

                // X軸の最大最小を設定
                chartdRdC.ChartAreas[0].AxisX.Maximum = (double)numericUpDown_xmax.Value;
                chartdRdC.ChartAreas[0].AxisX.Minimum = (double)numericUpDown_xmin.Value;

                chartdRdC.ChartAreas[0].CursorX.Position = (double)numericUpDownCurXd.Value;
                double cux = (double)numericUpDownCurXd.Value;
                double cuy = 0.0;
                double rx = 0.0;
                double ry = 0.0;
                for (int i = 1; i < 400; i++)
                {
                    double dc = CthC[nowChannel, i] - CthC[nowChannel, i - 1];
                    double dr = RthC[nowChannel, i] - RthC[nowChannel, i - 1];
                    double xx = RthC[nowChannel, i];
                    double yy = dc / dr;
                    // グラフにデータ追加
                    if (dr > 0 && dc > 0) chartdRdC.Series[nowChannel].Points.AddXY(xx, Math.Log10(yy));
                    if (xx >= cux && rx <= cux)
                    {
                        cuy = ry + (cux - rx) / (xx - rx) * (yy - ry);
                    }
                    rx = xx;
                    ry = yy;
                }
                this.label_dcdr.Text = String.Format("{0:E3}", cuy);
                numericUpDownCurX.Value = (decimal)chartdRdC.ChartAreas[0].CursorX.Position;

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}", ex.ToString()));
            }
        }

        #endregion

        #region F(Φ)実数部
        /// <summary>
        /// F(Φ)実数部
        /// </summary>
        public double ReF(double phi)
        {
            double f = 1D / (Math.Exp((Math.Abs(phi) - Phi0[nowChannel]) / Sigma[nowChannel]) + 1D);
            return f;
        }
        #endregion

        #region 構造関数の算出
        /// <summary>
        /// 構造関数の算出
        /// </summary>
        private void sfc()
        {

            this.active_label.Visible = true;
            this.Refresh();
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            progressBar1.Refresh();

            RApath = Application.StartupPath;
            using (StreamWriter sw = new StreamWriter(RApath + "\\RC.txt"))
            {
                sw.WriteLine(" R \t C ");
                for (int i = 0; i < 400; i++)
                {
                    double Rx = RthF[nowChannel, i] * dz[nowChannel];
                    double Cx = stime[nowChannel, i] / Rx;
                    sw.WriteLine(String.Format("{0}\t{1}", Rx, Cx));
                }
            }

            using (StreamWriter sw = new StreamWriter(RApath + "\\Zth_Rth.csv"))
            {
                sw.WriteLine("time,Zth,da/dz,ZthDFT_R,ZthDFT_I,RthF_R,RthF_I");
                for (int i = 0; i < 400; i++)
                {
                    sw.WriteLine(String.Format("{0},{1},{2},{3},{4},{5},{6}", stime[nowChannel, i], Zth[nowChannel, i], devZth[nowChannel, i], ZthDFT[nowChannel, i], ZthDFTi[nowChannel, i], RthF[nowChannel, i], RthFi[nowChannel, i]));
                }
            }

            try
            {
                this.cmd();
            }
            catch (Exception ex)
            {
                MessageBox.Show("TDIM Error: " + ex.Message);
                this.active_label.Visible = false;
                this.progressBar1.Visible = false;
                return;
            }

            using (StreamReader sr = new StreamReader(RApath + "\\RnCn.csv"))
            {
                String line;
                for (int i = 0; i < 400; i++)
                {
                    line = sr.ReadLine();
                    if (line == null) break;
                    String[] TextArr1 = line.Split(SepString, StringSplitOptions.RemoveEmptyEntries);
                    RthC[nowChannel, i] = System.Convert.ToDouble(TextArr1[0]);
                    CthC[nowChannel, i] = System.Convert.ToDouble(TextArr1[1]);

                }
            }

            ShowChartRnCn();
            ShowChartdCdR();
            this.active_label.Visible = false;
            this.progressBar1.Visible = false;
            this.btnSave.Visible = true;
            dataCalc[nowChannel] = true;
        }
        #endregion

        #region FFTの計算
        /// <summary>
        ///FFTの計算を行う　変数の受け渡しはdouble型　x：実数部　y：虚数部
        ///データの数は2^m個
        ///dirで方向を設定　trueならばFFT　falseならばIFFT
        /// </summary>

        public static void FFT(bool dir, int m, double[] x, double[] y)
        {
            int n, i, i1, j, k, i2, l, l1, l2;
            double c1, c2, tx, ty, t1, t2, u1, u2, z;

            //  nはデータ数

            n = 1;

            for (i = 0; i < m; i++) n *= 2;

            // データ並べ替え

            i2 = n >> 1;
            j = 0;
            for (i = 0; i < n - 1; i++)
            {
                if (i < j)
                {
                    tx = x[i];
                    ty = y[i];
                    x[i] = x[j];
                    y[i] = y[j];
                    x[j] = tx;
                    y[j] = ty;
                }
                k = i2;

                while (k <= j)
                {
                    j -= k;
                    k >>= 1;
                }

                j += k;
            }

            // FFTの計算

            c1 = -1.0;
            c2 = 0.0;
            l2 = 1;

            for (l = 0; l < m; l++)
            {
                l1 = l2;
                l2 <<= 1;
                u1 = 1.0;
                u2 = 0.0;

                for (j = 0; j < l1; j++)
                {
                    for (i = j; i < n; i += l2)
                    {
                        i1 = i + l1;
                        t1 = u1 * x[i1] - u2 * y[i1];
                        t2 = u1 * y[i1] + u2 * x[i1];
                        x[i1] = x[i] - t1;
                        y[i1] = y[i] - t2;
                        x[i] += t1;
                        y[i] += t2;
                    }

                    z = u1 * c1 - u2 * c2;
                    u2 = u1 * c2 + u2 * c1;
                    u1 = z;
                }

                c2 = Math.Sqrt((1.0 - c1) / 2.0);

                if (dir)
                    c2 = -c2;

                c1 = Math.Sqrt((1.0 + c1) / 2.0);
            }

            // ユニタリー作用素

            double sn = Math.Sqrt(n);
            for (i = 0; i < n; i++)
            {
                x[i] /= sn;
                y[i] /= sn;
            }
        }

        #endregion

        #endregion

        #region privateメソッド

        #region 測定ファイルを開くのOkボタンが押された
        /// <summary>
        /// openFileDialog1のOkボタンが押された
        /// </summary>
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            Form2 frm = new Form2();
            frm.Rstat = this.Rstat;
            for (int i = 0; i < 60000; i++)
            {
                frm.dataArrayTime[i] = 0.0;
                frm.dataArrayVf[i] = 0.0;
                frm.RefTime[i] = this.RefTime[i];
                frm.RefTemp[i] = this.RefVf[i];
            }

            try
            {
                using (StreamReader myStream = new StreamReader(openFileDialog1.OpenFile()))
                {
                    // Insert code to read the stream here.

                    double powerstep;
                    double heatsinktemp;
                    double sensitivity;
                    double x, y, x1 = 0, y1 = 0, x2 = -1.0, y2 = -1.0;
                    int aflag = 0;
                    int cflag = 0;
                    int dflag = 0;
                    int i = 0;
                    int ll;

                    dataRead[nowChannel] = false;
                    dataCalc[nowChannel] = false;
                    String line;
                    string[] TextArr1;
                    while (!myStream.EndOfStream)
                    {
                        line = myStream.ReadLine();
                        ll = line.Length;
                        if (ll == 0) break;
                        if (line[0] != '#' && line[0] > 0x1f)
                        {
                            if (i > 55700)
                            {
                                ll = 0;
                            }
                            TextArr1 = line.Split(SepString, StringSplitOptions.RemoveEmptyEntries);
                            if (TextArr1.Length == 0) break;
                            aflag = 0;
                            if (String.Compare(TextArr1[0], "POWERSTEP", true) == 0)
                            {
                                powerstep = System.Convert.ToDouble(TextArr1[1]);
                                frm.powerstep = powerstep;
                                this.Powerstep[nowChannel] = powerstep;
                                aflag = 1;

                            }
                            if (String.Compare(TextArr1[0], "HEATSINKTEMP", true) == 0)
                            {
                                heatsinktemp = System.Convert.ToDouble(TextArr1[1]);
                                frm.heatsinktemp = heatsinktemp;
                                this.HeatsinkTemp[nowChannel] = heatsinktemp;
                                aflag = 1;

                            }
                            if (String.Compare(TextArr1[0], "SENSITIVITY", true) == 0)
                            {
                                sensitivity = System.Convert.ToDouble(TextArr1[1]);
                                frm.sensitivity = sensitivity;
                                this.Sensitivity[nowChannel] = sensitivity;
                                aflag = 1;

                            }
                            if (String.Compare(TextArr1[0], "CALIBRATION", true) == 0)
                            {
                                aflag = 1;
                                cflag = 2;

                            }
                            if (String.Compare(TextArr1[0], "DATA", true) == 0)
                            {
                                aflag = 1;
                                dflag = 1;

                            }
                            if (aflag == 0)
                            {
                                
                                try { 
                                    string sx = TextArr1[0];
                                    string sy = TextArr1[1];
                                    if (double.TryParse(sx, out x) & double.TryParse(sy, out y))
                                    {
                                        if (cflag == 2)
                                        {
                                            x1 = x; y1 = y;
                                            cflag--;

                                        }
                                        else if (cflag == 1)
                                        {
                                            sensitivity = (y - y1) / (x - x1);
                                            frm.sensitivity = sensitivity;
                                            cflag--;

                                        }
                                        if (dflag == 1)
                                        {
                                            if (x2 < x)
                                            {
                                                x2 = x; y2 = y;
                                                frm.dataArrayTime[i] = x2;
                                                frm.dataArrayVf[i] = y2;
                                                i++;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("SF_Calc Error: Could not convert data. \n Original error: " + ex.Message + "\n" + line);
                                }

                                
                            }

                        }

                    }

                    frm.maxTimeCount = i;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("SF_Calc Error: Could not read file from disk. \n Original error: " + ex.Message);
                return;
            }


            frm.ktherm = 2.0 / Math.Sqrt(Math.PI * c * roh * lamda);

            frm.ShowDialog(this);
            this.Rstat = frm.Rstat;
            for (int i = 0; i < 60000; i++)
            {
                this.RefTime[i] = frm.RefTime[i];
                this.RefVf[i] = frm.RefTemp[i];
            }
            dataRead[nowChannel] = frm.flag;
            dz[nowChannel] = (Math.Log(frm.sTime[400]) - Math.Log(frm.sTime[0])) / 400D;
            for (int i = 0; i < 512; i++)
            {
                stime[nowChannel, i] = frm.sTime[i];
                z[nowChannel, i] = Math.Log(stime[nowChannel, i]);

                Zth[nowChannel, i] = frm.Zthja[i];

            }

            //   Zthの微分（アルゴリズム1）

            devZth[nowChannel, 399] = (Zth[nowChannel, 399] - Zth[nowChannel, 398]) / dz[nowChannel];
            devZth[nowChannel, 398] = (Zth[nowChannel, 398] - Zth[nowChannel, 397]) / dz[nowChannel];
            devZth[nowChannel, 0] = (Zth[nowChannel, 1] - Zth[nowChannel, 0]) / dz[nowChannel];
            devZth[nowChannel, 1] = (Zth[nowChannel, 2] - Zth[nowChannel, 1]) / dz[nowChannel];
            for (int i = 2; i < 398; i++)
            {
                devZth[nowChannel, i] = (-Zth[nowChannel, i + 2] + 8.0 * Zth[nowChannel, i + 1] - 8.0 * Zth[nowChannel, i - 1] + Zth[nowChannel, i - 2]) / 12.0 / dz[nowChannel];
            }
            for (int i = 400; i < 512; i++) devZth[nowChannel, i] = 0D;

            //

            /*   Zthの微分（アルゴリズム2）
            for (int i = 0; i < 512; i++)
            {
                devZth[nowChannel, i] = frm.dZthja[i];
            }
            */

            ListName[nowChannel] = listBox1.SelectedItem.ToString();

            area[nowChannel] = frm.ActiveArea;
            Tjmax[nowChannel] = frm.Tjmax;
            CutOffTime[nowChannel] = frm.cutofftime;
            TriggerTime[nowChannel] = frm.triggertime;
            averageCount[nowChannel] = frm.AverageCount;
            DataFileName[nowChannel] = openFileDialog1.SafeFileName;

            switch (nowChannel)
            {
                case 0:
                    labelCh1.Text = "Ch1: " + openFileDialog1.SafeFileName;
                    labelCh1.Visible = true;
                    tB_Ch1_Zth.Text = String.Format("{0:F4}", frm.Zthja[399]);
                    tB_Ch1_Area.Text = String.Format("{0:F3}", frm.ActiveArea);
                    toolTip1.SetToolTip(this.labelCh1, String.Format("Tjmax={0:F3}", frm.Tjmax));
                    break;
                case 1:
                    labelCh2.Text = "Ch2: " + openFileDialog1.SafeFileName;
                    labelCh2.Visible = true;
                    tB_Ch2_Zth.Text = String.Format("{0:F4}", frm.Zthja[399]);
                    tB_Ch2_Area.Text = String.Format("{0:F3}", frm.ActiveArea);
                    toolTip1.SetToolTip(this.labelCh2, String.Format("Tjmax={0:F3}", frm.Tjmax));
                    break;
                case 2:
                    labelCh3.Text = "Ch3: " + openFileDialog1.SafeFileName;
                    labelCh3.Visible = true;
                    tB_Ch3_Zth.Text = String.Format("{0:F4}", frm.Zthja[399]);
                    tB_Ch3_Area.Text = String.Format("{0:F3}", frm.ActiveArea);
                    toolTip1.SetToolTip(this.labelCh3, String.Format("Tjmax={0:F3}", frm.Tjmax));
                    break;
                case 3:
                    labelCh4.Text = "Ch4: " + openFileDialog1.SafeFileName;
                    labelCh4.Visible = true;
                    tB_Ch4_Zth.Text = String.Format("{0:F4}", frm.Zthja[399]);
                    tB_Ch4_Area.Text = String.Format("{0:F3}", frm.ActiveArea);
                    toolTip1.SetToolTip(this.labelCh4, String.Format("Tjmax={0:F3}", frm.Tjmax));
                    break;
                case 4:
                    labelCh5.Text = "Ch5: " + openFileDialog1.SafeFileName;
                    labelCh5.Visible = true;
                    tB_Ch5_Zth.Text = String.Format("{0:F4}", frm.Zthja[399]);
                    tB_Ch5_Area.Text = String.Format("{0:F3}", frm.ActiveArea);
                    toolTip1.SetToolTip(this.labelCh5, String.Format("Tjmax={0:F3}", frm.Tjmax));
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 構造関数の保存のOkボタンが押された
        /// <summary>
        /// saveFileDialog1のOkボタンが押された
        /// </summary>
        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                using (StreamWriter myStream = new StreamWriter(saveFileDialog1.OpenFile(), System.Text.Encoding.GetEncoding("Shift_JIS")))
                {
                    myStream.WriteLine("sf_calc Version {0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    myStream.WriteLine("Tjmax [℃],{0}", Tjmax[nowChannel]);
                    myStream.WriteLine("Power step [W],{0}", Powerstep[nowChannel]);
                    myStream.WriteLine("Heatsink Temp [℃],{0}", HeatsinkTemp[nowChannel]);
                    myStream.WriteLine("発熱面積 [mm2],{0}", area[nowChannel]);
                    myStream.WriteLine("チップの材質,{0}", ListName[nowChannel]);
                    myStream.WriteLine("参照熱抵抗 [K/W],{1},参照熱容量 [J/K],{0}", Math.Pow(10.0, (double)numericUpDownCurY.Value), Rthcx[nowChannel]);
                    myStream.WriteLine("Trigger time [μsec],{0}", TriggerTime[nowChannel] * 1.0e6);
                    myStream.WriteLine("Cut-Off time [μsec],{0}", CutOffTime[nowChannel] * 1.0e6);
                    myStream.WriteLine("平均化データ数,{0}", averageCount[nowChannel]);
                    myStream.WriteLine("窓関数のΦ0,{0}", Phi0[nowChannel]);
                    myStream.WriteLine("窓関数のσ,{0}", Sigma[nowChannel]);
                    myStream.WriteLine("ΣRth [K/W],ΣCth [J/K],τ [sec],dCth/dRth [W2･sec/K2]");
                    double c = CthC[nowChannel, 0];
                    double r = RthC[nowChannel, 0];

                    if (r == 0.0)
                    {
                        myStream.WriteLine("{0},{1},{2}", r, c, r * c);
                    }
                    else
                    {
                        myStream.WriteLine("{0},{1},{2},{3}", r, c, r * c, c / r);
                    }

                    for (int i = 1; i < 400; i++)
                    {
                        c = CthC[nowChannel, i];
                        r = RthC[nowChannel, i];
                        double dc = c - CthC[nowChannel, i - 1];
                        double dr = r - RthC[nowChannel, i - 1];
                        if (dr == 0.0)
                        {
                            myStream.WriteLine("{0},{1},{2}", r, c, r * c);
                            break;
                        }
                        else
                        {
                            myStream.WriteLine("{0},{1},{2},{3}", r, c, r * c, dc / dr);
                        }
                    }
                    myStream.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("TDIM Error: Could not save file from disk. Original error: " + ex.Message);
                return;
            }
        }
        #endregion

        #region 測定ファイルを開く
        /// <summary>
        /// ファイルを開くボタンが押された
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {

            openFileDialog1.ShowDialog();
            if (dataRead[nowChannel] == true)
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                ShowChartZth();
                cal_RFoster();
                ShowChartRthF();
                sw.Stop();
                label_mes.Text = string.Format("計算時間 {0}", sw.Elapsed);
            }

        }
        #endregion

        #region RCの計算Foster
        /// <summary>
        /// Foster型RC回路の計算
        /// </summary>
        private void cal_RFoster()
        {

            for (int i = 0; i < 4096; i++)
            {
                if (i < 512)
                {
                    re_M[i] = devZth[nowChannel, i];
                    im_M[i] = 0D;
                    re_W[i] = Math.Exp(z[nowChannel, i] - Math.Exp(z[nowChannel, i])) * 64D * dz[nowChannel];
                    im_W[i] = 0D;
                }
                else
                {
                    re_M[i] = 0D;
                    im_M[i] = 0D;
                    re_W[i] = 0D;
                    im_W[i] = 0D;
                }
            }

            FFT(true, 12, re_M, im_M);
            FFT(true, 12, re_W, im_W);

            Form3 frm3 = new Form3();
            frm3.ShowDialog();

            double[] re_V = new double[4096];
            double[] im_V = new double[4096];
            double dphi = 1D / 4096D / dz[nowChannel];
            for (int i = 0; i < 4096; i++)
            {
                double ww = re_W[i] * re_W[i] + im_W[i] * im_W[i];
                double phi = (double)i * dphi;
                if (i > 2047) phi = (double)(i - 4096) * dphi;
                re_M[i] *= ReF(phi);
                im_M[i] *= ReF(phi);
                if (ww > 0D)
                {
                    re_V[i] = (re_M[i] * re_W[i] + im_M[i] * im_W[i]) / ww;
                    im_V[i] = (im_M[i] * re_W[i] - re_M[i] * im_W[i]) / ww;
                }
                else
                {
                    re_V[i] = 0D;
                    im_V[i] = 0D;
                }
            }

            FFT(false, 12, re_V, im_V);
            FFT(false, 12, re_M, im_M);
            double gsum = 0D;
            double gsumi = 0D;
            for (int i = 0; i < 512; i++)
            {
                devZthDFT[nowChannel, i] = re_M[i];
                gsum += re_M[i];
                gsumi += im_M[i];
                ZthDFT[nowChannel, i] = gsum * dz[nowChannel];
                ZthDFTi[nowChannel, i] = gsumi * dz[nowChannel];
                if (i < 256)
                {
                    RthF[nowChannel, i + 256] = re_V[i];
                    RthFi[nowChannel, i + 256] = im_V[i];
                }
                else
                {
                    RthF[nowChannel, i - 256] = re_V[i + 3584];
                    RthFi[nowChannel, i - 256] = im_V[i + 3584];
                }

            }

            btnReCal.Visible = dataRead[nowChannel];
            btnSave.Visible = dataCalc[nowChannel];
            return;
        }
        #endregion

        #region btnSav_Click
        /// <summary>
        /// 構造関数の保存ボタンが押された
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = DataFileName[nowChannel].Replace(".txt", "_sf.csv");
            saveFileDialog1.ShowDialog();
        }

        #endregion

        #region チャンネル変更
        /// <summary>
        /// 選択中のチャンネルを変更
        /// </summary>
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

            nowChannel = (int)numericUpDown1.Value - 1;
            btnReCal.Visible = dataRead[nowChannel];
            btnSave.Visible = dataCalc[nowChannel];
            numericUpDown1.ForeColor = linecolor[nowChannel];

            ShowChartRnCn();
            ShowChartdCdR();
            chartRnCn.ChartAreas[0].CursorX.LineColor = linecolor[nowChannel];
            chartdRdC.ChartAreas[0].CursorX.LineColor = linecolor[nowChannel];

        }
        #endregion

        #region btnReCal_Click
        /// <summary>
        /// 再計算ボタンが押された
        /// </summary>
        private void btnReCal_Click(object sender, EventArgs e)
        {
            if (dataRead[nowChannel] == true)
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                cal_RFoster();
                ShowChartRthF();
                sw.Stop();
                label_mes.Text = string.Format("計算時間 {0}", sw.Elapsed);
            }
        }
        #endregion

        #region X軸の最大値変更
        private void numericUpDown_xmax_ValueChanged(object sender, EventArgs e)
        {
            ShowChartRnCn();
            ShowChartdCdR();
        }
        #endregion

        #region X軸の最小値変更
        private void numericUpDown_xmin_ValueChanged(object sender, EventArgs e)
        {
            ShowChartRnCn();
            ShowChartdCdR();
        }
        #endregion

        #region 構造関数の算出　Foster to Cauel
        private void cmd()
        {

            Bigdecimal[] ns = new Bigdecimal[402];
            Bigdecimal[] ds = new Bigdecimal[402];

            for (int i = 0; i < 401; i++)
            {
                ns[i] = Bigdecimal.Zero;
                ds[i] = Bigdecimal.Zero;
            }


            StreamReader infs = new StreamReader("RC.txt");

            String line = infs.ReadLine();
            string[] TextArr1;
            int n = 0;

            while ((line = infs.ReadLine()) != null)
            {
                try
                {
                    TextArr1 = line.Split(SepString, StringSplitOptions.RemoveEmptyEntries);
                    double r = System.Convert.ToDouble(TextArr1[0]);
                    double cd = System.Convert.ToDouble(TextArr1[1]);

                    Bigdecimal RR = new Bigdecimal(r);
                    Bigdecimal CC = new Bigdecimal(cd);
                    Bigdecimal RC = RR * CC;
                    Bigdecimal n0 = Bigdecimal.Zero;

                    //r = Math.Abs(r);
                    //cd = Math.Abs(cd);

                    if (n == 0)
                    {
                        ns[0] = RR;
                        ds[0] = Bigdecimal.One;
                        ds[1] = RC;
                    }

                    if (r > 0 && cd > 0)       // r>0 && cd>0
                    {
                        n++;
                        ds[n + 1] = ds[n] * RC;
                        n0 = ns[0] + ds[0] * RR;
                        for (int i = n; i > 0; i--)
                        {
                            ns[i] = ns[i] + ns[i - 1] * RC + ds[i] * RR;
                            ds[i] = ds[i] + ds[i - 1] * RC;
                        }
                        ns[0] = n0;

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                //Console.WriteLine(n);
                progressBar1.PerformStep();
            }
            n++;
            int Nmax = n;
            infs.Close();


            Bigdecimal[] rn = new Bigdecimal[402];
            Bigdecimal[] cn = new Bigdecimal[402];
            rn[0] = Bigdecimal.Zero;
            progressBar1.Value = 0;
            progressBar1.Refresh();
            for (int i = 0; i < Nmax; i++)
            {
                cn[i] = ds[Nmax - i] / ns[Nmax - i - 1];
                ds[Nmax - i] = Bigdecimal.Zero;
                for (int j = 1; j < n; j++)
                {
                    ds[j] -= ns[j - 1] * cn[i];
                }
                rn[i + 1] = ns[Nmax - i - 1] / ds[Nmax - i - 1];

                ns[Nmax - i - 1] = 0;
                for (int j = 0; j < n; j++)
                {
                    ns[j] -= ds[j] * rn[i + 1];
                }

                n--;
                progressBar1.PerformStep();
            }


            double rx, cx, r_LT = 0D, c_LT = 0D;
            int k = 0;
            bool flag = true;
            Bigdecimal r1 = Bigdecimal.Zero;
            Bigdecimal c1 = Bigdecimal.Zero;


            StreamWriter outf = new StreamWriter("RnCn.CSV");
            StreamWriter outf2 = new StreamWriter("RC_LT.asc");


            outf2.WriteLine("Version 4");
            outf2.WriteLine("SHEET 1 880 680");

            for (int i = 0; i < Nmax; i++)
            {
                c1 += cn[i];
                r1 += rn[i];
                rx = (double)rn[i];
                cx = (double)cn[i];
                r_LT += rx;
                c_LT += cx;

                if (rn[i + 1] > 0 && cn[i + 1] > 0 && rx > 0.0 && cx > 0.0)
                {
                    outf.WriteLine("{0},{1},{2},{3}", (double)r1, (double)c1, rx, cx);

                    if (true)   //(i%10 == 0)
                    {
                        outf2.WriteLine("");
                        outf2.WriteLine("SYMBOL cap {0} 48 R0", k * 208 + 80);
                        outf2.WriteLine("SYMATTR InstName C{0}", k + 1);
                        outf2.WriteLine("SYMATTR Value {0:E3}", c_LT);
                        outf2.WriteLine("");
                        outf2.WriteLine("SYMBOL res {0} 16 R270", k * 208 + 144);
                        outf2.WriteLine("SYMATTR InstName R{0}", k + 1);
                        outf2.WriteLine("SYMATTR Value {0:E3}", r_LT);
                        outf2.WriteLine("");
                        outf2.WriteLine("WIRE {0} 0 {1} 0", k * 208 + 32, k * 208 + 160);
                        outf2.WriteLine("WIRE {0} 0 {1} 48", k * 208 + 96, k * 208 + 96);
                        outf2.WriteLine("WIRE {0} 112 {1} 160", k * 208 + 96, k * 208 + 96);
                        outf2.WriteLine("");
                        outf2.WriteLine("FLAG {0} 160 0", k * 208 + 96);
                        outf2.WriteLine("");
                        r_LT = 0D;
                        c_LT = 0D;
                        if (r1 > Rthcx[nowChannel] && flag)
                        {
                            flag = false;
                            outf2.WriteLine("WIRE {0} 0 {1} -300", k * 144 + 32, k * 144 + 32);
                            outf2.WriteLine("FLAG {0} -300 TP", k * 144 + 32);
                            outf2.WriteLine("");
                        }
                        k++;
                    }
                }

            }
            outf.Close();
            outf2.Close();

        }

        #endregion

        #region 設定を保存　ボタン
        /// <summary>
        /// 設定を保存　ボタンが押された
        /// </summary>

        private void button1_Click_1(object sender, EventArgs e)
        {
            saveFileDialog2.ShowDialog();
        }

        private void saveFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            Form2 frm = new Form2();
            try
            {
                using (StreamWriter myStream = new StreamWriter(saveFileDialog2.OpenFile(), System.Text.Encoding.GetEncoding("Shift_JIS")))
                {

                    myStream.WriteLine("[表示データ数]={0}", frm.visibleTimeCount);
                    myStream.WriteLine("[Trigger_data_№]={0}", frm.triggernum);
                    myStream.WriteLine("[Cut-Off_data_№]={0}", frm.cutoffnum);
                    myStream.WriteLine("[平均化データ数]={0}", frm.AverageCount);
                    myStream.WriteLine("[Phi0]={0}", Phi0);
                    myStream.WriteLine("[Sigma]={0}", Sigma);
                    myStream.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("TDIM Error: Could not save file. Original error: " + ex.Message);
                return;
            }
        }

        #endregion

        #region 設定を読込　ボタン
        /// <summary>
        /// 設定を読込　ボタンが押された
        /// </summary>

        private void button2_Click_1(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            Form2 frm = new Form2();
            try
            {
                using (StreamReader myStream = new StreamReader(openFileDialog2.OpenFile()))
                {
                    String line;
                    string[] TextArr1;
                    while (!myStream.EndOfStream)
                    {
                        line = myStream.ReadLine();
                        TextArr1 = line.Split(SepString, StringSplitOptions.RemoveEmptyEntries);
                        if (String.Compare(TextArr1[0], "[表示データ数]", true) == 0)
                        {
                            frm.visibleTimeCount = System.Convert.ToInt32(TextArr1[1]);
                        }
                        if (String.Compare(TextArr1[0], "[Trigger_data_№]", true) == 0)
                        {
                            frm.triggernum = System.Convert.ToInt32(TextArr1[1]);
                        }
                        if (String.Compare(TextArr1[0], "[Cut-Off_data_№]", true) == 0)
                        {
                            frm.cutoffnum = System.Convert.ToInt32(TextArr1[1]);
                        }
                        if (String.Compare(TextArr1[0], "[平均化データ数]", true) == 0)
                        {
                            frm.AverageCount = System.Convert.ToInt32(TextArr1[1]);
                        }
                        if (String.Compare(TextArr1[0], "[Phi0]", true) == 0)
                        {
                            Phi0[nowChannel] = System.Convert.ToDouble(TextArr1[1]);
                        }
                        if (String.Compare(TextArr1[0], "[Sigma]", true) == 0)
                        {
                            Sigma[nowChannel] = System.Convert.ToDouble(TextArr1[1]);
                        }
                    }
                    myStream.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("TDIM Error: Could not read file. Original error: " + ex.Message);
                return;
            }
        }

        #endregion

        #region チップの材質を変更
        /// <summary>
        /// チップの材質が変更されたされた
        /// </summary>
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            /* string ss = this.listBox1.SelectedItem.ToString();
            switch (ss)
            {
                case "":
                    break;

                case "Silicon":     //シリコン
                    c = 0.7046;       // specific heat [J/gK]
                    roh = 0.002329;   // density [g/mm3]
                    lamda = 0.149;    // thermal conductivity [W/mmK]
                    break;
                case "Silicon Carbide":
                    c = 0.714;
                    roh = 0.0032;
                    lamda = 0.37;
                    break;
                case "Germanium":
                    c = 0.3197;
                    roh = 0.005323;
                    lamda = 0.0602;
                    break;
                case "Gallium Arsenide":
                    c = 0.325;
                    roh = 0.005316;
                    lamda = 0.055;
                    break;
                case "Gallium Nitride":
                    c = 0.49;
                    roh = 0.0061;
                    lamda = 0.13;
                    break;
                default:
                    break;
            }
            */

            int rr=bindingSource1.Position;
            TestList testlist = (TestList)bindingSource1.DataSource;
            if (testlist != null)
            {
                c = (double)testlist[rr].Cap / 1000.0;
                roh = (double)testlist[rr].Roh / 1000000.0;
                lamda = (double)testlist[rr].Lamda / 1000.0;
            }
            
            
        }
        #endregion

        #region Ch1表示
        /// <summary>
        /// Ch1表示/非表示が変更されたされた
        /// </summary>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            chartZth.Series[0].Enabled = this.checkBox1.Checked;
            chartZth.Series[5].Enabled = this.checkBox1.Checked;
            chartdZ.Series[0].Enabled = this.checkBox1.Checked;
            chartdZ.Series[5].Enabled = this.checkBox1.Checked;
            chartRthF.Series[0].Enabled = this.checkBox1.Checked;
            chartRnCn.Series[0].Enabled = this.checkBox1.Checked;
            chartdRdC.Series[0].Enabled = this.checkBox1.Checked;
        }
        #endregion

        #region Ch2表示
        /// <summary>
        /// Ch2表示/非表示が変更されたされた
        /// </summary>
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            chartZth.Series[1].Enabled = this.checkBox2.Checked;
            chartZth.Series[6].Enabled = this.checkBox2.Checked;
            chartdZ.Series[1].Enabled = this.checkBox2.Checked;
            chartdZ.Series[6].Enabled = this.checkBox2.Checked;
            chartRthF.Series[1].Enabled = this.checkBox2.Checked;
            chartRnCn.Series[1].Enabled = this.checkBox2.Checked;
            chartdRdC.Series[1].Enabled = this.checkBox2.Checked;
        }
        #endregion

        #region Ch3表示
        /// <summary>
        /// Ch3表示/非表示が変更されたされた
        /// </summary>
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            chartZth.Series[2].Enabled = this.checkBox3.Checked;
            chartZth.Series[7].Enabled = this.checkBox3.Checked;
            chartdZ.Series[2].Enabled = this.checkBox3.Checked;
            chartdZ.Series[7].Enabled = this.checkBox3.Checked;
            chartRthF.Series[2].Enabled = this.checkBox3.Checked;
            chartRnCn.Series[2].Enabled = this.checkBox3.Checked;
            chartdRdC.Series[2].Enabled = this.checkBox3.Checked;
        }
        #endregion

        #region Ch4表示
        /// <summary>
        /// Ch4表示/非表示が変更されたされた
        /// </summary>
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            chartZth.Series[3].Enabled = this.checkBox4.Checked;
            chartZth.Series[8].Enabled = this.checkBox4.Checked;
            chartdZ.Series[3].Enabled = this.checkBox4.Checked;
            chartdZ.Series[8].Enabled = this.checkBox4.Checked;
            chartRthF.Series[3].Enabled = this.checkBox4.Checked;
            chartRnCn.Series[3].Enabled = this.checkBox4.Checked;
            chartdRdC.Series[3].Enabled = this.checkBox4.Checked;
        }
        #endregion

        #region Ch5表示
        /// <summary>
        /// Ch5表示/非表示が変更されたされた
        /// </summary>
        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            chartZth.Series[4].Enabled = this.checkBox5.Checked;
            chartZth.Series[9].Enabled = this.checkBox5.Checked;
            chartdZ.Series[4].Enabled = this.checkBox5.Checked;
            chartdZ.Series[9].Enabled = this.checkBox5.Checked;
            chartRthF.Series[4].Enabled = this.checkBox5.Checked;
            chartRnCn.Series[4].Enabled = this.checkBox5.Checked;
            chartdRdC.Series[4].Enabled = this.checkBox5.Checked;
        }
        #endregion

        #region このソフトについて　ボタン
        /// <summary>
        /// このソフトについて　ボタンが押された
        /// </summary>

        private void button3_Click_1(object sender, EventArgs e)
        {
            AboutBox1 dlg = new AboutBox1();
            dlg.ShowDialog();
        }

        #endregion

        #region RnCnカーソルX変更
        private void numericUpDown_CurX_ValueChanged(object sender, EventArgs e)
        {
            ShowChartRnCn();
        }
        #endregion

        #region RnCnグラフクリック
        private void chartRnCn_Click(object sender, EventArgs e)
        {
            numericUpDownCurX.Value = (decimal)chartRnCn.ChartAreas[0].CursorX.Position;
            numericUpDownCurXd.Value = (decimal)chartRnCn.ChartAreas[0].CursorX.Position;
        }
        #endregion

        #region dRdCカーソルX変更
        private void numericUpDownCurXd_ValueChanged(object sender, EventArgs e)
        {
            ShowChartdCdR();
        }
        #endregion

        #region dRdCグラフクリック
        private void chartdRdC_Click(object sender, EventArgs e)
        {
            numericUpDownCurX.Value = (decimal)chartdRdC.ChartAreas[0].CursorX.Position;
            numericUpDownCurXd.Value = (decimal)chartdRdC.ChartAreas[0].CursorX.Position;
        }
        #endregion

        #region 起動時の処理
        private void Form1_Load(object sender, EventArgs e)
        {
            ClientSize = new Size(1300, 820);
            this.Text += "   バージョン ";
            this.Text += Assembly.GetExecutingAssembly().GetName().Version.ToString();

            toolTip1.SetToolTip(this.labelCh1, "Ch1");
            toolTip1.SetToolTip(this.labelCh2, "Ch2");
            toolTip1.SetToolTip(this.labelCh3, "Ch3");
            toolTip1.SetToolTip(this.labelCh4, "Ch4");
            toolTip1.SetToolTip(this.labelCh5, "Ch5");

            /* 初期物質データを設定
             * Lamda: thermal conductivity [W/(m・K)]
             * Cap: specific heat [J/(kg・K)]
             * Roh: density [kg/m3]
             */

            TestList testlist = new TestList();
            testlist.Add(new Test { Name = "Silicon", Lamda = 149, Cap = 704.6m, Roh = 2329 });
            testlist.Add(new Test { Name = "Silicon Carbide", Lamda = 370, Cap = 714, Roh = 3210 });
            testlist.Add(new Test { Name = "Germanium", Lamda = 60.2m, Cap = 320, Roh = 5323 });
            testlist.Add(new Test { Name = "Gallium Arsenide", Lamda = 46, Cap = 350, Roh = 5316 });
            testlist.Add(new Test { Name = "Gallium Nitride", Lamda = 130, Cap = 490, Roh = 6100 });

            bindingSource1.DataSource = testlist;
            listBox1.DataSource = bindingSource1;
            listBox1.DisplayMember = "Name";

            listBox1.SelectedIndex = 1;
        }
        #endregion

        #region RnCnカーソルX変更
        private void numericUpDownCurY_ValueChanged(object sender, EventArgs e)
        {

            ShowChartRnCn();
        }

        private void tB_curCth_ValueChanged(object sender, EventArgs e)
        {
            double cth = 1D;
            if (double.TryParse(tB_curCth.Text, out cth))
            {
                numericUpDownCurY.Value = (decimal)Math.Log10(cth);
            }

            ShowChartRnCn();
        }
        #endregion

        #region チップの材質編集
        private void label75_Click(object sender, EventArgs e)
        {
            Form4 frm = new Form4();
            frm.ShowDialog(this);
        }
        #endregion



        #endregion

    }
}
