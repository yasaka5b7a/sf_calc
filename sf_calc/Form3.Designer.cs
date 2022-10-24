namespace sf_calc
{
    partial class Form3
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.chartFFT = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.nudPhi0 = new System.Windows.Forms.NumericUpDown();
            this.nudSigma = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.button_up = new System.Windows.Forms.Button();
            this.button_down = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.chartFFT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPhi0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSigma)).BeginInit();
            this.SuspendLayout();
            // 
            // chartFFT
            // 
            chartArea1.AxisX.Crossing = -1D;
            chartArea1.AxisX.Interval = 0.2D;
            chartArea1.AxisX.Maximum = 1D;
            chartArea1.AxisX.Minimum = -1D;
            chartArea1.AxisY.Interval = 1D;
            chartArea1.AxisY.Maximum = 2D;
            chartArea1.AxisY.Minimum = -2D;
            chartArea1.Name = "ChartArea1";
            this.chartFFT.ChartAreas.Add(chartArea1);
            legend1.DockedToChartArea = "ChartArea1";
            legend1.Name = "Legend1";
            this.chartFFT.Legends.Add(legend1);
            this.chartFFT.Location = new System.Drawing.Point(12, 15);
            this.chartFFT.Name = "chartFFT";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series1.Legend = "Legend1";
            series1.Name = "M[Φ]";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series2.Legend = "Legend1";
            series2.Name = "M[Φ]・F[Φ]";
            this.chartFFT.Series.Add(series1);
            this.chartFFT.Series.Add(series2);
            this.chartFFT.Size = new System.Drawing.Size(852, 443);
            this.chartFFT.TabIndex = 0;
            this.chartFFT.Text = "chart1";
            // 
            // nudPhi0
            // 
            this.nudPhi0.DecimalPlaces = 3;
            this.nudPhi0.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudPhi0.Location = new System.Drawing.Point(870, 201);
            this.nudPhi0.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudPhi0.Name = "nudPhi0";
            this.nudPhi0.Size = new System.Drawing.Size(69, 19);
            this.nudPhi0.TabIndex = 1;
            this.nudPhi0.Value = new decimal(new int[] {
            45,
            0,
            0,
            131072});
            this.nudPhi0.ValueChanged += new System.EventHandler(this.nudPhi0_ValueChanged);
            // 
            // nudSigma
            // 
            this.nudSigma.DecimalPlaces = 3;
            this.nudSigma.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudSigma.Location = new System.Drawing.Point(872, 260);
            this.nudSigma.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudSigma.Name = "nudSigma";
            this.nudSigma.Size = new System.Drawing.Size(66, 19);
            this.nudSigma.TabIndex = 2;
            this.nudSigma.Value = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.nudSigma.ValueChanged += new System.EventHandler(this.nudSigma_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(871, 187);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "Phi0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(873, 247);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "Sigma";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(875, 373);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(63, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(876, 401);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(61, 22);
            this.btnApply.TabIndex = 6;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(876, 428);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(60, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // button_up
            // 
            this.button_up.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button_up.ForeColor = System.Drawing.Color.Red;
            this.button_up.Location = new System.Drawing.Point(876, 47);
            this.button_up.Name = "button_up";
            this.button_up.Size = new System.Drawing.Size(58, 21);
            this.button_up.TabIndex = 8;
            this.button_up.Text = "レンジ＋";
            this.button_up.UseVisualStyleBackColor = true;
            this.button_up.Click += new System.EventHandler(this.button_up_Click);
            // 
            // button_down
            // 
            this.button_down.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button_down.ForeColor = System.Drawing.Color.Blue;
            this.button_down.Location = new System.Drawing.Point(876, 85);
            this.button_down.Name = "button_down";
            this.button_down.Size = new System.Drawing.Size(58, 21);
            this.button_down.TabIndex = 9;
            this.button_down.Text = "レンジ－";
            this.button_down.UseVisualStyleBackColor = true;
            this.button_down.Click += new System.EventHandler(this.button_down_Click);
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(946, 480);
            this.Controls.Add(this.button_down);
            this.Controls.Add(this.button_up);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nudSigma);
            this.Controls.Add(this.nudPhi0);
            this.Controls.Add(this.chartFFT);
            this.Name = "Form3";
            this.Text = "畳み込みの設定";
            this.Load += new System.EventHandler(this.Form3_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chartFFT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPhi0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSigma)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chartFFT;
        private System.Windows.Forms.NumericUpDown nudPhi0;
        private System.Windows.Forms.NumericUpDown nudSigma;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button button_up;
        private System.Windows.Forms.Button button_down;
    }
}