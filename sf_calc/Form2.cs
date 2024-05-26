using System;
using System.Windows.Forms;

namespace sf_calc
{
    public partial class Form2 : Form
    {
        #region メンバ変数

        public double[] RefTime = new double[60000];
        public double[] RefVf = new double[60000];
        public int avc = 9;

        /// <summary>
        /// Timeデータ
        /// </summary>
        public double[] dataArrayTime = new double[60000];

        /// <summary>
        /// Vfデータ
        /// </summary>
        public double[] dataArrayVf = new double[60000];

        public double[] sTime = new double[512];
        public double[] Zthja = new double[512];
        public double[] dZthja = new double[512];
        public int[] sCount = new int[512];
        public double powerstep;
        public double heatsinktemp;
        public double sensitivity;
        public int maxTimeCount = 0;
        public int visibleTimeCount = 1000;
        public double Tjmax = 0;
        public bool flag = false;
        public bool Rstat = false;
        public double triggertime = 0;
        public double cutofftime = 0;

        private int selectedrb = 1;
        public int triggernum = 0;

        public int cutoffnum = 100;
        private double Vf0 = 0;
        private double rTdt = 1.0;
        private double cutoffTj = 0;

        public double ActiveArea = 0;
        public double ktherm = 0;

        public int AverageCount = 300;

        Properties.Settings Settings1 = new Properties.Settings();

        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>

        public Form2()
        {
            InitializeComponent();

        }
        #endregion

        #region publicメソッド

        #region グラフ描画
        /// <summary>
        /// データをチャートに描画
        /// </summary>
        public void ShowChartTj()
        {
            // グラフのデータクリア
            chartOffset.Series[0].Points.Clear();
            chartOffset.Series[1].Points.Clear();
            chartOffset.Series[2].Points.Clear();

            triggertime = this.dataArrayTime[triggernum];
            cutofftime = this.dataArrayTime[cutoffnum + triggernum] - triggertime;

            // 近似直線、温度データをチャートに追加
            for (int i = 1; i < visibleTimeCount; i++)
            {
                // 小数点に変換
                double tempTime = this.dataArrayTime[i + triggernum] - triggertime;
                double tempVf = (this.dataArrayVf[i + triggernum] - this.dataArrayVf[maxTimeCount - 1]) / this.sensitivity + this.heatsinktemp;
                double tempVf0 = (Vf0 + rTdt * Math.Sqrt(this.dataArrayTime[i + triggernum] - triggertime) - this.dataArrayVf[maxTimeCount - 1]) / this.sensitivity + this.heatsinktemp;
                if (selectedrb == 2)
                {
                    if (tempTime <= 0.0) break;
                    tempTime = Math.Log(tempTime);
                }
                if (selectedrb == 3)
                {
                    tempTime = Math.Sqrt(tempTime);
                }

                // グラフにデータ追加
                chartOffset.Series[0].Points.AddXY(tempTime, tempVf);
                chartOffset.Series[1].Points.AddXY(tempTime, tempVf0);

            }
            Tjmax = (Vf0 - this.dataArrayVf[maxTimeCount - 1]) / this.sensitivity + this.heatsinktemp;
            cutoffTj = (Vf0 + rTdt * Math.Sqrt(cutofftime) - this.dataArrayVf[maxTimeCount - 1]) / this.sensitivity + this.heatsinktemp;
            ActiveArea = powerstep / rTdt * ktherm * -sensitivity;
            double time = 0;
            double t0 = 0;
            time = cutofftime;
            if (selectedrb == 2)
            {
                if (time > 0.0)
                {
                    time = Math.Log(time);
                    t0 = Math.Log(this.dataArrayTime[triggernum]);
                }
            }
            if (selectedrb == 3)
            {
                time = Math.Sqrt(time);
            }

            if (selectedrb == 1)
            {
                chartOffset.ChartAreas[0].AxisX.Title = "Time [sec]";
                chartOffset.ChartAreas[0].AxisX.Minimum = 0.0;
                chartOffset.Series[2].Points.AddXY(time, cutoffTj);
                chartOffset.Series[2].Points.AddXY(t0, Tjmax);
            }
            if (selectedrb == 2)
            {
                chartOffset.ChartAreas[0].AxisX.Title = "Log(Time)";
                chartOffset.ChartAreas[0].AxisX.Minimum = Double.NaN;
            }
            if (selectedrb == 3)
            {
                chartOffset.ChartAreas[0].AxisX.Title = "Sqrt(Time)";
                chartOffset.ChartAreas[0].AxisX.Minimum = 0.0;
                chartOffset.Series[2].Points.AddXY(time, cutoffTj);
                chartOffset.Series[2].Points.AddXY(t0, Tjmax);
            }
            if (checkBox1.Checked)
            {
                chartOffset.ChartAreas[0].AxisY.Maximum = Double.NaN;
                chartOffset.ChartAreas[0].AxisY.Minimum = Double.NaN;
            }
            else
            {
                chartOffset.ChartAreas[0].AxisY.Maximum = (double)numericUpDown2.Value;
                chartOffset.ChartAreas[0].AxisY.Minimum = (double)numericUpDown3.Value;
            }

            textBox4.Text = String.Format("{0:F1}", Tjmax);
            textBox6.Text = String.Format("{0:F3}", ActiveArea);

        }
        #endregion

        #endregion

        #region privateメソッド

        #region オフセット調整
        private void Form2_Load(object sender, EventArgs e)
        {
            checkBoxR.Enabled = Rstat;
            numericUpDown1.Value = Settings1.VisibleTimeCount;
            numericUpDown4.Value = Settings1.CutOffNumber;
            numericUpDown5.Value = Settings1.TriggerNumber;
            numericUpDown6.Value = Settings1.AverageCount;
            setOffset();
            ShowChartTj();
        }
        #endregion

        #region ラジオボタン変更
        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb == null)
            {
                selectedrb = 1;
                return;
            }

            // Ensure that the RadioButton.Checked property
            // changed to true.
            if (rb.Checked)
            {
                // Keep track of the selected RadioButton by saving a reference
                // to it.
                selectedrb = rb.TabIndex;
                setOffset();
                ShowChartTj();
            }
        }
        #endregion

        #region 表示データ数変更
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown nud = sender as NumericUpDown;
            if (nud == null) return;
            visibleTimeCount = (int)nud.Value;
            if (visibleTimeCount > maxTimeCount) visibleTimeCount = maxTimeCount;
            setOffset();
            ShowChartTj();
        }
        #endregion

        #region チェックボックス変更
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            setOffset();
            ShowChartTj();
        }
        #endregion

        #region MaxTj変更
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown2.Value <= numericUpDown3.Value)
            {
                numericUpDown2.Value = numericUpDown3.Value + 1;
            }
            setOffset();
            ShowChartTj();
        }
        #endregion

        #region MinTj変更
        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown2.Value <= numericUpDown3.Value)
            {
                numericUpDown3.Value = numericUpDown2.Value - 1;
            }
            setOffset();
            ShowChartTj();
        }
        #endregion

        #region Cut-Off data №変更
        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            setOffset();
            ShowChartTj();
        }
        #endregion

        #region Trigger data №変更
        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            setOffset();
            ShowChartTj();
        }
        #endregion

        #region オフセットの計算
        private void setOffset()
        {
            AverageCount = (int)numericUpDown6.Value;
            triggernum = (int)numericUpDown5.Value;
            triggertime = this.dataArrayTime[triggernum];
            cutoffnum = (int)numericUpDown4.Value;
            cutofftime = this.dataArrayTime[cutoffnum + triggernum] - triggertime;
            textBox1.Text = String.Format("{0}", cutofftime * 1.0e6);
            textBox2.Text = String.Format("{0:F1}", heatsinktemp);
            textBox3.Text = String.Format("{0:F2}", powerstep);
            textBox4.Text = String.Format("{0:F1}", Tjmax);
            textBox5.Text = String.Format("{0}", triggertime * 1.0e6);
            textBox6.Text = String.Format("{0:F3}", ActiveArea);

            double sumx = 0.0;
            double sumy = 0.0;
            double sumxy = 0.0;
            double sumxx = 0.0;
            for (int i = 1; i < AverageCount + 1; i++)
            {
                double yk = this.dataArrayVf[cutoffnum + triggernum + i];
                double xk = Math.Sqrt(this.dataArrayTime[cutoffnum + triggernum + i] - triggertime);
                sumx += xk;
                sumy += yk;
                sumxy += xk * yk;
                sumxx += xk * xk;

            }
            double ac = (double)AverageCount;
            rTdt = (ac * sumxy - sumx * sumy) / (ac * sumxx - sumx * sumx);
            Vf0 = (sumxx * sumy - sumxy * sumx) / (ac * sumxx - sumx * sumx);

        }
        #endregion

        #region OKボタン
        private void button1_Click(object sender, EventArgs e)
        {

            double step = Math.Pow(10.0, 12.0 / 512.0);     //1e-6から1e+6まで512分割
            double Vf;

            for(int i = 0; i < 512; i++)
            {
                sTime[i] = Math.Pow(step, (double)(i - 256));
                int j = triggernum;
                while (this.dataArrayTime[j] < (sTime[i] * Math.Sqrt(step) + triggertime))
                {
                    j++;
                    if (j > maxTimeCount - 1)
                    {
                        j = maxTimeCount;
                        break;
                    }
                }
                sCount[i] = j;
            }


            for (int i = 0; i < 512; i++)
            {
                

                if (sTime[i] < cutofftime)
                {
                    Vf = Vf0 + rTdt * Math.Sqrt(sTime[i]);
                    dZthja[i] = -rTdt / 2.0 * Math.Sqrt(sTime[i]) / powerstep / sensitivity;
                }
                else
                {

                    if (sCount[i] == maxTimeCount)
                    {
                        double sum = 0.0;
                        for (int kk = 1; kk < 11; kk++) sum += dataArrayVf[maxTimeCount - kk];
                        Vf = sum /= 10.0;
                        dZthja[i] = 0.0;
                    }
                    else
                    {
                        double sumx = 0.0;
                        double sumy = 0.0;
                        double sumxx = 0.0;
                        double sumxy = 0.0;
                        double nn = 0.0;
                        int c1 = sCount[i] - avc;
                        int c2 = sCount[i];
                        int c3 = sCount[i] + avc;
                        if (i > 1)
                        {
                            c1 = (sCount[i - 1] + c2) / 2;
                            c3 = (sCount[i + 1] + c2) / 2;
                        }
                        for (int kk = c1; kk < c3; kk++)
                        {
                            double sx = Math.Log(dataArrayTime[kk]);
                            double sy = dataArrayVf[kk];
                            sumx += sx;
                            sumy += sy;
                            sumxy += sx * sy;
                            sumxx += sx * sx;
                            nn += 1.0;
                        }
                        double aa = (nn * sumxy - sumx * sumy) / (nn * sumxx - sumx * sumx);
                        double bb = (sumxx * sumy - sumxy * sumx) / (nn * sumxx - sumx * sumx);
                        if (!double.IsNaN(aa) && !double.IsNaN(bb))
                        {

                            Vf = aa * Math.Log(dataArrayTime[c2]) + bb;
                            dZthja[i] = -aa / powerstep / sensitivity;
                        }
                        else
                        {
                            double sum = 0.0;
                            for (int kk = 1; kk < 11; kk++) sum += dataArrayVf[maxTimeCount - kk];
                            Vf = sum /= 10.0;
                            dZthja[i] = 0.0;
                        }

                    }
                }
                Zthja[i] = (Tjmax - (Vf - dataArrayVf[maxTimeCount - 1]) / sensitivity - heatsinktemp) / powerstep;
            }
            Settings1.CutOffNumber = cutoffnum;
            Settings1.TriggerNumber = triggernum;
            Settings1.VisibleTimeCount = visibleTimeCount;
            Settings1.AverageCount = AverageCount;
            Settings1.Save();
            flag = true;

            this.Close();
        }
        #endregion

        #region キャンセルボタン
        private void button2_Click(object sender, EventArgs e)
        {
            flag = false;
            this.Close();
        }
        #endregion

        #region 平均化データ数変更
        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            setOffset();
            ShowChartTj();
        }
        #endregion

        #region リファレンス
        private void checkBoxR_CheckedChanged(object sender, EventArgs e)
        {

            chartOffset.Series[3].Enabled = checkBoxR.Checked;
            chartOffset.Series[3].Points.Clear();
            for (int i = 1; i < visibleTimeCount; i++)
            {
                double tempTime = RefTime[i];
                if (selectedrb == 2)
                {
                    tempTime = Math.Log(tempTime);
                }
                if (selectedrb == 3)
                {
                    tempTime = Math.Sqrt(tempTime);
                }
                chartOffset.Series[3].Points.AddXY(tempTime, RefVf[i]);
            }
            ShowChartTj();
        }
        #endregion

        #region リファレンス取込ボタン
        private void buttonR_Click(object sender, EventArgs e)
        {

            triggertime = this.dataArrayTime[triggernum];
            cutofftime = this.dataArrayTime[cutoffnum + triggernum] - triggertime;

            // 近似直線、温度データをチャートに追加
            for (int i = 1; i < visibleTimeCount; i++)
            {
                // 小数点に変換
                double tempTime = this.dataArrayTime[i + triggernum] - triggertime;
                double tempVf = (this.dataArrayVf[i + triggernum] - this.dataArrayVf[maxTimeCount - 1]) / this.sensitivity + this.heatsinktemp;

                // リファレンスにデータ追加

                RefTime[i] = tempTime;
                RefVf[i] = tempVf;

            }
            checkBoxR.Enabled = true;
            Rstat = true;
        }
        #endregion

        #region da/dz平均化データ数の変更
        private void numericUpDown_daAv_ValueChanged(object sender, EventArgs e)
        {
            avc = (int)numericUpDown_daAv.Value;
            if (avc > cutoffnum) avc = cutoffnum;
            numericUpDown_daAv.Value = (decimal)avc;
        }
        #endregion

        #endregion

    }
}
