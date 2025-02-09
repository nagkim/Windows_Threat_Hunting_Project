using Interface.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Interface
{
    public partial class File_Scan : Form
    {
        private OpenFileDialog openFileDialog;
        private const string HybridAnalysisApiKey = "api_key_here"; 
        private const string HybridAnalysisApiUrl = "https://www.hybrid-analysis.com/api/v2/"; 
        string selectedFilePath = "";
        public File_Scan(string _selectedFilePath)
        {
            InitializeComponent();
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All Files|*.*"; /
            pictureBox2.Visible = false; 
            pictureBox3.Visible = false; 
            pictureBox4.Visible = false; 
            pictureBox5.Visible = false; 
            this.selectedFilePath = _selectedFilePath;
            if (_selectedFilePath != "")
            {
                passvalue();
            }
        }
        private async void passvalue()
        {
           
           ResetLabels();
           textBox1.Text = "File path: " + selectedFilePath;

            pictureBox4.Visible = true; 

            string scanResult = await GetHybridAnalysisReputation(selectedFilePath);
            textBox3.Text = scanResult;
            pictureBox4.Visible = false; 
            pictureBox2.Visible = false; 
        }
        private async void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog.FileName;
                ResetLabels();
                textBox1.Text = "File path: " + selectedFilePath;

                pictureBox4.Visible = true; 

                string scanResult = await GetHybridAnalysisReputation(selectedFilePath);
                textBox3.Text = scanResult;
                pictureBox4.Visible = false; 
                pictureBox2.Visible = false; 
            }
        }

        public void ResetLabels()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox2.ForeColor = System.Drawing.Color.Black; 
            textBox3.ForeColor = System.Drawing.Color.Black; 
            pictureBox2.Visible = false; 
            pictureBox3.Visible = false; 
            pictureBox5.Visible = false; 
        }

        public async Task<string> GetHybridAnalysisReputation(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                MessageBox.Show("File path is invalid or file does not exist.");
                return "unknown";
            }

            string jobId = await SubmitFileForScanning(filePath, "120");
            if (jobId != null)
            {
                pictureBox4.Visible = false;
                textBox2.Text = "Success";
                textBox2.ForeColor = System.Drawing.Color.Green; 
                pictureBox2.Visible = true; 
            }
            else
            {
                textBox2.Text = "Failed";
                textBox2.ForeColor = System.Drawing.Color.Red; 
                return "unknown";
            }

            int maxRetries = 40;
            int retryCount = 0;
            var status = await CheckSubmissionStatus(jobId);

            while (status != null && status.state != "SUCCESS" && status.state != "ERROR" && retryCount < maxRetries)
            {
                await Task.Delay(60000);
                status = await CheckSubmissionStatus(jobId);
                retryCount++;
            }

            if (status == null || status.state == "ERROR")
            {
                MessageBox.Show("Failed to get successful scan status.");
                return "unknown";
            }

            if (status.state == "SUCCESS")
            {
                var report = await GetAnalysisReport(jobId);
                if (report != null)
                {
                    if (InterpretReport(report))
                    {
                        pictureBox3.Visible = true; 
                        return "No Specific Threat";
                    }
                    else
                    {
                        pictureBox5.Visible = true; 
                        textBox3.ForeColor = System.Drawing.Color.Red; 
                        return "Malicious";
                    }
                }
                else
                {
                    MessageBox.Show("Failed to retrieve analysis report.");
                }
            }

            return "unknown";
        }

        private async Task<string> SubmitFileForScanning(string filePath, string environmentId)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Falcon Sandbox");
            client.DefaultRequestHeaders.Add("api-key", HybridAnalysisApiKey.Trim());

            try
            {
                using var content = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(filePath));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                content.Add(fileContent, "file", System.IO.Path.GetFileName(filePath));
                content.Add(new StringContent(environmentId), "environment_id");

                var response = await client.PostAsync($"{HybridAnalysisApiUrl}submit/file", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<SubmissionResponse>(await response.Content.ReadAsStringAsync());
                    return result?.job_id;
                }
                else
                {
                    MessageBox.Show("Failed to submit file: " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting file for scanning: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        private async Task<SubmissionStatus> CheckSubmissionStatus(string jobId)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Falcon Sandbox");
            client.DefaultRequestHeaders.Add("api-key", HybridAnalysisApiKey.Trim());

            try
            {
                var response = await client.GetAsync($"{HybridAnalysisApiUrl}report/{jobId}/state");
                if (response.IsSuccessStatusCode)
                {
                    var status = JsonSerializer.Deserialize<SubmissionStatus>(await response.Content.ReadAsStringAsync());
                    return status;
                }
                else
                {
                    MessageBox.Show("Failed to check submission status: " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking submission status: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        private async Task<AnalysisReport> GetAnalysisReport(string jobId)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Falcon Sandbox");
            client.DefaultRequestHeaders.Add("api-key", HybridAnalysisApiKey.Trim());

            try
            {
                var response = await client.GetAsync($"{HybridAnalysisApiUrl}report/{jobId}/summary");
                if (response.IsSuccessStatusCode)
                {
                    var report = JsonSerializer.Deserialize<AnalysisReport>(await response.Content.ReadAsStringAsync());
                    return report;
                }
                else
                {
                    MessageBox.Show("Failed to retrieve analysis report: " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving analysis report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        private bool InterpretReport(AnalysisReport report)
        {
            string verdict = report.verdict ?? "unknown";
            int? threatScore = report.threat_score;
            int? threatLevel = report.threat_level;

            return verdict == "no specific threat" && (threatScore == null || threatScore == 0) && (threatLevel == null || threatLevel == 0);
        }

        private class SubmissionResponse
        {
            public string job_id { get; set; }
        }

        private class SubmissionStatus
        {
            public string state { get; set; }
        }

        private class AnalysisReport
        {
            public string verdict { get; set; }
            public int? threat_score { get; set; }
            public int? threat_level { get; set; }
        }

        private void File_Scan_Load(object sender, EventArgs e)
        {
          
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}
