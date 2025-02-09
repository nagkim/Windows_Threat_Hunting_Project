namespace Interface
{
    partial class Services
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView services_grid;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataVisualization.Charting.Chart cpuUsageChart;
        private System.Windows.Forms.DataVisualization.Charting.Chart memoryUsageChart;
        private System.Windows.Forms.Label runningCountLabel;
        private System.Windows.Forms.Label stoppedCountLabel;
        private System.Windows.Forms.Label totalCountLabel;
        private System.Windows.Forms.Panel countPanel;
        private System.Windows.Forms.Label cpuUsageUnitLabel;
        private System.Windows.Forms.Label memoryUsageUnitLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.services_grid = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.cpuUsageChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.memoryUsageChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.runningCountLabel = new System.Windows.Forms.Label();
            this.stoppedCountLabel = new System.Windows.Forms.Label();
            this.totalCountLabel = new System.Windows.Forms.Label();
            this.countPanel = new System.Windows.Forms.Panel();
            this.cpuUsageUnitLabel = new System.Windows.Forms.Label();
            this.memoryUsageUnitLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.services_grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cpuUsageChart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.memoryUsageChart)).BeginInit();
            this.countPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // services_grid
            // 
            this.services_grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.services_grid.BackgroundColor = System.Drawing.Color.White;
            this.services_grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.services_grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.services_grid.Location = new System.Drawing.Point(18, 118);
            this.services_grid.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.services_grid.Name = "services_grid";
            this.services_grid.Size = new System.Drawing.Size(866, 515);
            this.services_grid.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.label1.Location = new System.Drawing.Point(442, 14);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 25);
            this.label1.TabIndex = 5;
            this.label1.Text = "Services";
            // 
            // cpuUsageChart
            // 
            this.cpuUsageChart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cpuUsageChart.Location = new System.Drawing.Point(900, 118);
            this.cpuUsageChart.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cpuUsageChart.Name = "cpuUsageChart";
            this.cpuUsageChart.Size = new System.Drawing.Size(450, 308);
            this.cpuUsageChart.TabIndex = 6;
            // 
            // memoryUsageChart
            // 
            this.memoryUsageChart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.memoryUsageChart.Location = new System.Drawing.Point(900, 435);
            this.memoryUsageChart.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.memoryUsageChart.Name = "memoryUsageChart";
            this.memoryUsageChart.Size = new System.Drawing.Size(450, 308);
            this.memoryUsageChart.TabIndex = 7;
            this.memoryUsageChart.Click += new System.EventHandler(this.memoryUsageChart_Click);
            // 
            // runningCountLabel
            // 
            this.runningCountLabel.AutoSize = true;
            this.runningCountLabel.BackColor = System.Drawing.Color.LimeGreen;
            this.runningCountLabel.ForeColor = System.Drawing.Color.White;
            this.runningCountLabel.Location = new System.Drawing.Point(4, 8);
            this.runningCountLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.runningCountLabel.Name = "runningCountLabel";
            this.runningCountLabel.Size = new System.Drawing.Size(86, 20);
            this.runningCountLabel.TabIndex = 7;
            this.runningCountLabel.Text = "Running: 0";
            // 
            // stoppedCountLabel
            // 
            this.stoppedCountLabel.AutoSize = true;
            this.stoppedCountLabel.BackColor = System.Drawing.Color.Red;
            this.stoppedCountLabel.ForeColor = System.Drawing.Color.White;
            this.stoppedCountLabel.Location = new System.Drawing.Point(225, 8);
            this.stoppedCountLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.stoppedCountLabel.Name = "stoppedCountLabel";
            this.stoppedCountLabel.Size = new System.Drawing.Size(87, 20);
            this.stoppedCountLabel.TabIndex = 8;
            this.stoppedCountLabel.Text = "Stopped: 0";
            // 
            // totalCountLabel
            // 
            this.totalCountLabel.AutoSize = true;
            this.totalCountLabel.Location = new System.Drawing.Point(450, 8);
            this.totalCountLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.totalCountLabel.Name = "totalCountLabel";
            this.totalCountLabel.Size = new System.Drawing.Size(61, 20);
            this.totalCountLabel.TabIndex = 9;
            this.totalCountLabel.Text = "Total: 0";
            // 
            // countPanel
            // 
            this.countPanel.Controls.Add(this.runningCountLabel);
            this.countPanel.Controls.Add(this.stoppedCountLabel);
            this.countPanel.Controls.Add(this.totalCountLabel);
            this.countPanel.Location = new System.Drawing.Point(18, 72);
            this.countPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.countPanel.Name = "countPanel";
            this.countPanel.Size = new System.Drawing.Size(866, 37);
            this.countPanel.TabIndex = 10;
            // 
            // cpuUsageUnitLabel
            // 
            this.cpuUsageUnitLabel.AutoSize = true;
            this.cpuUsageUnitLabel.Location = new System.Drawing.Point(900, 431);
            this.cpuUsageUnitLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.cpuUsageUnitLabel.Name = "cpuUsageUnitLabel";
            this.cpuUsageUnitLabel.Size = new System.Drawing.Size(121, 20);
            this.cpuUsageUnitLabel.TabIndex = 11;
            this.cpuUsageUnitLabel.Text = "CPU Usage (%)";
            // 
            // memoryUsageUnitLabel
            // 
            this.memoryUsageUnitLabel.AutoSize = true;
            this.memoryUsageUnitLabel.Location = new System.Drawing.Point(900, 754);
            this.memoryUsageUnitLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.memoryUsageUnitLabel.Name = "memoryUsageUnitLabel";
            this.memoryUsageUnitLabel.Size = new System.Drawing.Size(154, 20);
            this.memoryUsageUnitLabel.TabIndex = 12;
            this.memoryUsageUnitLabel.Text = "Memory Usage (MB)";
            // 
            // Services
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1476, 769);
            this.Controls.Add(this.countPanel);
            this.Controls.Add(this.memoryUsageUnitLabel);
            this.Controls.Add(this.cpuUsageUnitLabel);
            this.Controls.Add(this.memoryUsageChart);
            this.Controls.Add(this.cpuUsageChart);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.services_grid);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Services";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Services";
            this.Load += new System.EventHandler(this.Services_Load);
            ((System.ComponentModel.ISupportInitialize)(this.services_grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cpuUsageChart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.memoryUsageChart)).EndInit();
            this.countPanel.ResumeLayout(false);
            this.countPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }

}