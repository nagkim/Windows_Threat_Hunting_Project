using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using IWshRuntimeLibrary;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Interface
{
    public partial class ResultForm : Form
    {
        private Form1 Form1;
        private const string HybridAnalysisApiKey = "api_key_here";
        private const string HybridAnalysisApiUrl = "https://www.hybrid-analysis.com/api/v2/";
        private int programCounter = 1;

        public ResultForm(Form1 form1)
        {
            InitializeComponent();
            Form1 = form1;
            InitializeStartupProgramsGrid();
        }

        private async void ResultForm_Load(object sender, EventArgs e)
        {
            Properties.Settings.Default.interfacepositioning = "1";
            Properties.Settings.Default.Save();
            await RescanStartupFiles();
        }

        private void InitializeStartupProgramsGrid()
        {
            dataGridView1.ColumnCount = 11;
            dataGridView1.Columns[0].Name = "#";
            dataGridView1.Columns[0].Width = 20;
            dataGridView1.Columns[1].Name = "Name";
            dataGridView1.Columns[1].Width = 100;
            dataGridView1.Columns[2].Name = "Publisher";
            dataGridView1.Columns[2].Width = 70;
            dataGridView1.Columns[3].Name = "Path";
            dataGridView1.Columns[3].Width = 100;
            dataGridView1.Columns[4].Name = "Status";
            dataGridView1.Columns[4].Width = 50;
            dataGridView1.Columns[5].Name = "Status Impact";
            dataGridView1.Columns[5].Width = 50;
            dataGridView1.Columns[6].Name = "Type";
            dataGridView1.Columns[6].Width = 30;
            dataGridView1.Columns[7].Name = "File Version";
            dataGridView1.Columns[7].Width = 50;
            dataGridView1.Columns[8].Name = "Copyright";
            dataGridView1.Columns[8].Width = 60;
            dataGridView1.Columns[9].Name = "Size";
            dataGridView1.Columns[9].Width = 60;
            dataGridView1.Columns[10].Name = "Date Modified";
            dataGridView1.Columns[10].Width = 70;

            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.DefaultCellStyle.BackColor = Color.White;
            dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.GridColor = Color.LightGray;
        }





        private async void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.interfacepositioning = "0";
            Properties.Settings.Default.Save();
            foreach (Form a in MdiChildren)
            {
                if (a is QuickScan)
                {
                    //the form is open, set focus to it
                    a.BringToFront();
                    a.Activate();
                    break;
                }
            }
            if (ActiveMdiChild == null || ActiveMdiChild.Name != "QuickScan")
            {
                //no forms are open
                //open one
                QuickScan Accounts = new QuickScan(Form1);
                Accounts.MdiParent = Form1;
                Accounts.Dock = DockStyle.Fill;
                Accounts.Show();
            }
            // Form1.panelform(new QuickScan(Form1));
        }

        private async Task RescanStartupFiles()
        {
            dataGridView1.Rows.Clear();
            programCounter = 1;

            var registryPaths = new List<(RegistryKey, string)>
            {
                (Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run"),
                (Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Run"),
                (Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\RunOnce"),
                (Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\RunOnce"),
                (Registry.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Run"),
                (Registry.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\RunOnce")

            };

            foreach (var (baseKey, subKeyPath) in registryPaths)
            {
                programCounter = await AddStartupProgramsToGrid(baseKey, subKeyPath, programCounter);
            }

            programCounter = await AddStartupProgramsFromStartupFolders(programCounter);
        }

        private async Task<int> AddStartupProgramsToGrid(RegistryKey baseKey, string subKeyPath, int startCounter)
        {
            try
            {
                using (RegistryKey key = baseKey.OpenSubKey(subKeyPath))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            string rawPath = key.GetValue(valueName)?.ToString() ?? "";
                            string path = ExtractExecutablePath(rawPath);
                            var fileInfo = GetFileInfo(path);
                            int rowIndex = dataGridView1.Rows.Add(startCounter++, fileInfo.Name, fileInfo.Publisher, fileInfo.Path, fileInfo.Status, fileInfo.StatusImpact, fileInfo.FileType, fileInfo.FileVersion, fileInfo.Copyright, fileInfo.Size, fileInfo.DateModified);

                            string reputation = await GetHybridAnalysisReputation(fileInfo.Path);
                            Color rowColor = GetRowColorFromReputation(reputation);
                            dataGridView1.Rows[rowIndex].DefaultCellStyle.BackColor = rowColor;
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Registry key not found: {baseKey.Name}\\{subKeyPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Access denied to registry key: {baseKey.Name}\\{subKeyPath}\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error accessing registry key: {baseKey.Name}\\{subKeyPath}\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return startCounter;
        }

        private async Task<int> AddStartupProgramsFromStartupFolders(int startCounter)
        {
            var startupFolders = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup)
            };

            foreach (var folder in startupFolders)
            {
                if (Directory.Exists(folder))
                {
                    var files = Directory.GetFiles(folder, "*.lnk")
                        .Concat(Directory.GetFiles(folder, "*.exe"));
                    foreach (var file in files)
                    {
                        var path = ResolveShortcut(file) ?? file;
                        var fileInfo = GetFileInfo(path);
                        int rowIndex = dataGridView1.Rows.Add(startCounter++, fileInfo.Name, fileInfo.Publisher, fileInfo.Path, fileInfo.Status, fileInfo.StatusImpact, fileInfo.FileType, fileInfo.FileVersion, fileInfo.Copyright, fileInfo.Size, fileInfo.DateModified);

                        string reputation = await GetHybridAnalysisReputation(fileInfo.Path);
                        Color rowColor = GetRowColorFromReputation(reputation);
                        dataGridView1.Rows[rowIndex].DefaultCellStyle.BackColor = rowColor;
                    }
                }
            }

            return startCounter;
        }

        private Color GetRowColorFromReputation(string reputation)
        {
            switch (reputation)
            {
                case "malicious":
                    return Color.Red;
                case "no specific threat":
                    return Color.LightGreen;
                default:
                    return Color.Orange;
            }
        }

        private FileInfo GetFileInfo(string path)
        {
            var fileInfo = new FileInfo
            {
                Path = path,
                Name = "Unknown",
                Publisher = "Unknown",
                Status = System.IO.File.Exists(path) ? "Enabled" : "Disabled",
                StatusImpact = "Unknown",
                FileType = "Unknown",
                FileVersion = "Unknown",
                Copyright = "Unknown",
                Size = "Unknown",
                DateModified = "Unknown"
            };

            try
            {
                if (System.IO.File.Exists(path))
                {
                    var info = FileVersionInfo.GetVersionInfo(path);
                    fileInfo.Name = string.IsNullOrEmpty(info.FileDescription) ? Path.GetFileNameWithoutExtension(path) : info.FileDescription;
                    fileInfo.Publisher = string.IsNullOrEmpty(info.CompanyName) ? "Unknown" : info.CompanyName;
                    fileInfo.FileType = Path.GetExtension(path);
                    fileInfo.FileVersion = string.IsNullOrEmpty(info.FileVersion) ? "Unknown" : info.FileVersion;
                    fileInfo.Copyright = string.IsNullOrEmpty(info.LegalCopyright) ? "Unknown" : info.LegalCopyright;
                    fileInfo.Size = FormatFileSize(new System.IO.FileInfo(path).Length);
                    fileInfo.DateModified = System.IO.File.GetLastWriteTime(path).ToString();

                    if (fileInfo.Publisher.Contains("Microsoft") || fileInfo.Publisher.Contains("Adobe"))
                    {
                        fileInfo.StatusImpact = "Low";
                    }
                    else
                    {
                        fileInfo.StatusImpact = "High";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting file info for path {path}: {ex.Message}");
            }

            return fileInfo;
        }

        private string ExtractExecutablePath(string rawPath)
        {
            string pattern = @"""([^""]+\.exe)""|(\S+\.exe)";
            Match match = Regex.Match(rawPath, pattern);

            if (match.Success)
            {
                return match.Groups[1].Value != "" ? match.Groups[1].Value : match.Groups[2].Value;
            }

            return rawPath;
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes >= 1073741824) // GB
                return (bytes / 1073741824.0).ToString("F2") + " GB";
            if (bytes >= 1048576) // MB
                return (bytes / 1048576.0).ToString("F2") + " MB";
            if (bytes >= 1024) // KB
                return (bytes / 1024.0).ToString("F2") + " KB";
            return bytes + " bytes";
        }

        private async Task<string> GetHybridAnalysisReputation(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return "unknown";

            string jobId = await SubmitFileForScanning(filePath, "120");

            if (jobId != null)
            {
                int maxRetries = 40;
                int retryCount = 0;
                var status = await CheckSubmissionStatus(jobId);

                while (status != null && status.state != "SUCCESS" && status.state != "ERROR" && retryCount < maxRetries)
                {
                    await Task.Delay(60000);
                    status = await CheckSubmissionStatus(jobId);
                    retryCount++;
                }

                if (status != null && status.state == "SUCCESS")
                {
                    var report = await GetAnalysisReport(jobId);
                    if (report != null)
                    {
                        return InterpretReport(report) ? "no specific threat" : "malicious";
                    }
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
                content.Add(fileContent, "file", Path.GetFileName(filePath));
                content.Add(new StringContent(environmentId), "environment_id");

                var response = await client.PostAsync($"{HybridAnalysisApiUrl}submit/file", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<SubmissionResponse>(await response.Content.ReadAsStringAsync());
                    return result?.job_id;
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
                    return JsonSerializer.Deserialize<SubmissionStatus>(await response.Content.ReadAsStringAsync());
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
                    return JsonSerializer.Deserialize<AnalysisReport>(await response.Content.ReadAsStringAsync());
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

        private string ResolveShortcut(string file)
        {
            try
            {
                if (Path.GetExtension(file).ToLower() != ".lnk")
                    return null;

                var shell = new WshShell();
                var shortcut = (IWshShortcut)shell.CreateShortcut(file);
                return shortcut.TargetPath;
            }
            catch
            {
                return null;
            }
        }

        private class FileInfo
        {
            public string Path { get; set; }
            public string Name { get; set; }
            public string Publisher { get; set; }
            public string Status { get; set; }
            public string StatusImpact { get; set; }
            public string FileType { get; set; }
            public string FileVersion { get; set; }
            public string Copyright { get; set; }
            public string Size { get; set; }
            public string DateModified { get; set; }
        }
    }
}
