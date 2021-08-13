using System;
using System.Windows.Forms;

namespace sf_calc
{
    public partial class Form3 : Form
    {
        #region メンバ変数

        Form1 mainForm = (Form1)sf_calc.Form1.ActiveForm;
        public double phi0;
        public double sigma;
        private double mphi0;
        private double msigma;

        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>

        public Form3()
        {

            InitializeComponent();
            phi0 = mainForm.Phi0[mainForm.nowChannel];
            sigma = mainForm.Sigma[mainForm.nowChannel];
            mphi0 = phi0;
            msigma = sigma;
            nudPhi0.Value = (decimal)phi0;
            nudSigma.Value = (decimal)sigma;
        }

        #endregion

        #region publicメソッド

        #region グラフ描画
        /// <summary>
        /// データをチャートに描画
        /// </summary>
        public void ShowChartFFT()
        {

            // グラフのデータクリア
            chartFFT.Series[0].Points.Clear();
            chartFFT.Series[1].Points.Clear();
            mainForm.Phi0[mainForm.nowChannel] = this.phi0;
            mainForm.Sigma[mainForm.nowChannel] = this.sigma;
            double dphi = 1D / 4096D / mainForm.dz[mainForm.nowChannel];

            for (int i = 0; i < 2048; i++)
            {
                double phi = (double)i * dphi;
                double re = mainForm.re_M[i];
                double im = mainForm.im_M[i];
                double m = Math.Sqrt(re * re + im * im);
                double f = m * mainForm.ReF(phi);
                if (m > 0D) chartFFT.Series[0].Points.AddXY(phi, Math.Log10(m));
                if (f > 0D) chartFFT.Series[1].Points.AddXY(phi, Math.Log10(f));
            }
            for (int i = 2048; i < 4096; i++)
            {
                double phi = (double)(i - 4096) * dphi;
                double re = mainForm.re_M[i];
                double im = mainForm.im_M[i];
                double m = Math.Sqrt(re * re + im * im);
                double f = m * mainForm.ReF(phi);
                if (m > 0D) chartFFT.Series[0].Points.AddXY(phi, Math.Log10(m));
                if (f > 0D) chartFFT.Series[1].Points.AddXY(phi, Math.Log10(f));
            }


            this.Refresh();
        }
        #endregion

        #endregion

        #region privateメソッド

        #region Form3_Load
        private void Form3_Load(object sender, EventArgs e)
        {

            this.ShowChartFFT();
        }

        #endregion

        #region Φ0変更
        private void nudPhi0_ValueChanged(object sender, EventArgs e)
        {
            this.phi0 = (double)nudPhi0.Value;
            this.ShowChartFFT();
        }


        #endregion


        #region Σ変更
        private void nudSigma_ValueChanged(object sender, EventArgs e)
        {
            this.sigma = (double)nudSigma.Value;
            this.ShowChartFFT();
        }
        #endregion

        #region OKボタン
        private void btnOK_Click(object sender, EventArgs e)
        {
            this.phi0 = (double)nudPhi0.Value;
            this.sigma = (double)nudSigma.Value;
            this.ShowChartFFT();
            this.Close();
        }
        #endregion

        #region Applyボタン
        private void btnApply_Click(object sender, EventArgs e)
        {
            this.phi0 = (double)nudPhi0.Value;
            this.sigma = (double)nudSigma.Value;
            this.ShowChartFFT();
        }
        #endregion

        #region Cancelボタン
        private void btnCancel_Click(object sender, EventArgs e)
        {
            phi0 = mphi0;
            sigma = msigma;
            this.ShowChartFFT();
            this.Close();
        }
        #endregion

        #region レンジ＋ボタン
        private void button_up_Click(object sender, EventArgs e)
        {
            chartFFT.ChartAreas[0].AxisY.Maximum += 1D;
            chartFFT.ChartAreas[0].AxisY.Minimum += 1D;
        }
        #endregion


        #region レンジ－ボタン
        private void button_down_Click(object sender, EventArgs e)
        {
            chartFFT.ChartAreas[0].AxisY.Maximum -= 1D;
            chartFFT.ChartAreas[0].AxisY.Minimum -= 1D;
        }
        #endregion



        #endregion
    }
}
