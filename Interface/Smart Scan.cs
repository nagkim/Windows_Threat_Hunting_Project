using System;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.Net.NetworkInformation;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;
using Newtonsoft.Json.Linq;
using Interface.Models;
using System.Runtime.InteropServices.ComTypes;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;
using System.Timers;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json;
using System.Net.Http;

namespace Interface
{
    public partial class Smart_Scan : Form
    {
        Form1 f;
     
        public Smart_Scan(Form1 _f)
        {
            InitializeComponent();
            this.f = _f;
        }
       

        private void Smart_Scan_Load(object sender, EventArgs e)
        {
          
            InitializeDataGridView();

            
            Thread monitoringThread = new Thread(MonitorProcesses);
            monitoringThread.IsBackground = true;
            monitoringThread.Start();
          
            InitializeChart();

        }

        private void InitializeDataGridView()
        {
            dataGridView1.Columns.Add("ProcessName", "Process Name");
            dataGridView1.Columns.Add("ProcessID", "Process ID");
            dataGridView1.Columns.Add("ParentProcessID", "Parent Process ID");
            dataGridView1.Columns.Add("CommandLine", "Command Line");
            dataGridView1.Columns.Add("CPUUsage", "CPU Usage (%)");
            dataGridView1.Columns.Add("MemoryUsage", "Memory Usage (MB)");
            dataGridView1.Columns.Add("NetworkConnections", "Network Connections");
            dataGridView1.Columns.Add("ExecutablePath", "Executable Path");
            dataGridView1.Columns.Add("SHA256", "SHA-256");
            dataGridView1.Columns.Add("MD5", "MD5");
        }
        private  NetworkUsageMonitor _monitor;
        private  System.Timers.Timer _timer;
        private void InitializeChart()
        {
           
            var chartArea = new ChartArea("NetworkUsage")
            {
                AxisX = { Title = "Time (s)" },
                AxisY = { Title = "KB/s" }
            };
            networkChart.ChartAreas.Add(chartArea);

            
            var sentSeries = new Series("Sent (KB/s)")
            {
                ChartType = SeriesChartType.Line,
                Color = System.Drawing.Color.Blue,
                BorderWidth = 2
            };

            
            var receivedSeries = new Series("Received (KB/s)")
            {
                ChartType = SeriesChartType.Line,
                Color = System.Drawing.Color.Red,
                BorderWidth = 2
            };

            networkChart.Series.Add(sentSeries);
            networkChart.Series.Add(receivedSeries);
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            
            var (sentBytesPerSec, receivedBytesPerSec) = _monitor.GetNetworkUsage();

           
            var sentKbPerSec = sentBytesPerSec / 1024;
            var receivedKbPerSec = receivedBytesPerSec / 1024;

            networkChart.Series["Sent (KB/s)"].Points.AddY(sentKbPerSec);
            networkChart.Series["Received (KB/s)"].Points.AddY(receivedKbPerSec);

           
            if (networkChart.Series["Sent (KB/s)"].Points.Count > 60)
                networkChart.Series["Sent (KB/s)"].Points.RemoveAt(0);

            if (networkChart.Series["Received (KB/s)"].Points.Count > 60)
                networkChart.Series["Received (KB/s)"].Points.RemoveAt(0);

            
            networkChart.ChartAreas[0].RecalculateAxesScale();
        }
        
        public static T ByteArrayToStructure<T>(byte[] byteArray)
        {
            GCHandle handle = GCHandle.Alloc(byteArray, GCHandleType.Pinned);
            T structure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return structure;
        }

        // Method to send data to FastAPI for prediction
        static async Task<int> MakePrediction(List<double> features)
        {
            string apiUrl = "http://127.0.0.1:8000/predict";

            var inputData = new
            {
                features = features.ToArray() 
            };

            string jsonData = JsonConvert.SerializeObject(inputData);

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseString = await response.Content.ReadAsStringAsync();
                        var responseJson = JsonConvert.DeserializeObject<Dictionary<string, int>>(responseString);

                        if (responseJson != null && responseJson.ContainsKey("prediction"))
                        {
                            return responseJson["prediction"]; 
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error Code: {response.StatusCode}");
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error Details: {errorResponse}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }

            return -1; 
        }


        const uint IMAGE_DOS_SIGNATURE = 0x5A4D;  
    const uint IMAGE_NT_SIGNATURE = 0x00004550; 

    
    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_DOS_HEADER
    {
        public ushort e_magic;  
        public ushort e_lfanew; 
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_FILE_HEADER
    {
        public ushort Machine;
        public ushort NumberOfSections;
        public uint TimeDateStamp;
        public uint PointerToSymbolTable;
        public uint NumberOfSymbols;
        public ushort SizeOfOptionalHeader;
        public ushort Characteristics;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_OPTIONAL_HEADER
    {
        public ushort Magic;
        public byte MajorLinkerVersion;
        public byte MinorLinkerVersion;
        public uint SizeOfCode;
        public uint SizeOfInitializedData;
        public uint SizeOfUninitializedData;
        public uint AddressOfEntryPoint;
        public uint BaseOfCode;
        public uint BaseOfData;
        public uint ImageBase;
        public uint SectionAlignment;
        public uint FileAlignment;
        public ushort MajorOperatingSystemVersion;
        public ushort MinorOperatingSystemVersion;
        public ushort MajorImageVersion;
        public ushort MinorImageVersion;
        public ushort MajorSubsystemVersion;
        public ushort MinorSubsystemVersion;
        public uint SizeOfImage;
        public uint SizeOfHeaders;
        public uint CheckSum;
        public ushort Subsystem;
        public ushort DllCharacteristics;
        public uint SizeOfStackReserve;
        public uint SizeOfStackCommit;
        public uint SizeOfHeapReserve;
        public uint SizeOfHeapCommit;
        public uint LoaderFlags;
        public uint NumberOfRvaAndSizes;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_SECTION_HEADER
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Name;
        public uint VirtualSize;
        public uint VirtualAddress;
        public uint SizeOfRawData;
        public uint PointerToRawData;
        public uint PointerToRelocations;
        public uint PointerToLinenumbers;
        public ushort NumberOfRelocations;
        public ushort NumberOfLinenumbers;
        public uint Characteristics;
    }

    
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr CreateFile(
        string lpFileName, uint dwDesiredAccess, uint dwShareMode,
        IntPtr lpSecurityAttributes, uint dwCreationDisposition,
        uint dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToRead,
        ref uint lpNumberOfBytesRead, IntPtr lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr hObject);

        private async void  MonitorProcesses()
        {
            while (true)
            {
                try
                {
                    var allProcesses = Process.GetProcesses().OrderBy(p => p.ProcessName).ToList();

                    foreach (var process in allProcesses)
                    {
                        try
                        {
                            string processName = process.ProcessName;
                            int processId = process.Id;
                            int parentProcessId = GetParentProcessId(process.Id);
                            string commandLineArgs = GetCommandLineArgs(process.Id);
                            double cpuUsage = GetCpuUsageFallback(process);
                            long memoryUsage = process.WorkingSet64 / (1024 * 1024); 
                            string networkConnections = GetNetworkActivity(process.Id);
                            string filePath = process.MainModule?.FileName ?? "N/A";
                            string sha256Hash = filePath != "N/A" ? ComputeFileHash(filePath, SHA256.Create()) : "N/A";
                            string md5Hash = filePath != "N/A" ? ComputeFileHash(filePath, MD5.Create()) : "N/A";
                            string filename= Path.GetFileName(filePath);
                            
                            byte[] optionalHeaderBuffer = new byte[Marshal.SizeOf(typeof(IMAGE_OPTIONAL_HEADER))];
                            byte[] fileHeaderBuffer = new byte[Marshal.SizeOf(typeof(IMAGE_FILE_HEADER))];
                            byte[] sectionHeaderBuffer = new byte[Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER))];

                           
                            var features = new List<double>();

                           
                            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                            {
                                // Read the IMAGE_OPTIONAL_HEADER structure
                                fileStream.Seek(0xF0, SeekOrigin.Begin); 
                                fileStream.Read(optionalHeaderBuffer, 0, optionalHeaderBuffer.Length);
                                IMAGE_OPTIONAL_HEADER optionalHeader = ByteArrayToStructure<IMAGE_OPTIONAL_HEADER>(optionalHeaderBuffer);

                                // Read the IMAGE_FILE_HEADER structure
                                fileStream.Seek(0x04, SeekOrigin.Begin); 
                                fileStream.Read(fileHeaderBuffer, 0, fileHeaderBuffer.Length);
                                IMAGE_FILE_HEADER fileHeader = ByteArrayToStructure<IMAGE_FILE_HEADER>(fileHeaderBuffer);

                                // Read the IMAGE_SECTION_HEADER structure for SectionMaxChar
                                fileStream.Seek(0xF8, SeekOrigin.Begin); 
                                fileStream.Read(sectionHeaderBuffer, 0, sectionHeaderBuffer.Length);
                                IMAGE_SECTION_HEADER sectionHeader = ByteArrayToStructure<IMAGE_SECTION_HEADER>(sectionHeaderBuffer);

                                // Add features to the list
                                features.Add(optionalHeader.MajorSubsystemVersion);  // Feature 1
                                features.Add(optionalHeader.MajorLinkerVersion);     // Feature 2
                                features.Add(optionalHeader.SizeOfCode);            // Feature 3
                                features.Add(optionalHeader.SizeOfImage);           // Feature 4
                                features.Add(optionalHeader.SizeOfHeaders);         // Feature 5
                                features.Add(optionalHeader.SizeOfInitializedData); // Feature 6
                                features.Add(optionalHeader.SizeOfUninitializedData); // Feature 7
                                features.Add(optionalHeader.SizeOfStackReserve);    // Feature 8
                                features.Add(optionalHeader.SizeOfHeapReserve);     // Feature 9
                                features.Add(fileHeader.NumberOfSymbols);           // Feature 10

                               
                                int maxSectionNameLength = 0;
                                foreach (byte b in sectionHeader.Name)
                                {
                                    if (b != 0) 
                                    {
                                        maxSectionNameLength++;
                                    }
                                }
                                features.Add(maxSectionNameLength);  // Feature 11: SectionMaxChar

                               
                                int prediction = await MakePrediction(features);

                                
                                int rowIndex =AddToDataGridView(processName, processId, parentProcessId, commandLineArgs, cpuUsage, memoryUsage, networkConnections, filePath, sha256Hash, md5Hash);

                                if (rowIndex >= 0)
                                {
                                    dataGridView1.Invoke(new Action(() =>
                                    {
                                        if (prediction == 0)
                                            dataGridView1.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                                        else if (prediction == 1)
                                            dataGridView1.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                                        else
                                            dataGridView1.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Gray; 
                                    }));
                                }

                            }
   
                    


                            _monitor = new NetworkUsageMonitor(processId);

                            

                          
                            var (sentBytesPerSec, receivedBytesPerSec) = _monitor.GetNetworkUsage();

                            
                            var sentKbPerSec = sentBytesPerSec / 1024;
                            var receivedKbPerSec = receivedBytesPerSec / 1024;

                            
                            networkChart.Series["Sent (KB/s)"].Points.AddY(sentKbPerSec);
                            networkChart.Series["Received (KB/s)"].Points.AddY(receivedKbPerSec);

                            
                            if (networkChart.Series["Sent (KB/s)"].Points.Count > 60)
                                networkChart.Series["Sent (KB/s)"].Points.RemoveAt(0);

                            if (networkChart.Series["Received (KB/s)"].Points.Count > 60)
                                networkChart.Series["Received (KB/s)"].Points.RemoveAt(0);

                            
                            networkChart.ChartAreas[0].RecalculateAxesScale();
                            UpdateTotalProcessesAndNetworkUsage();
                            var services = new ServicesNamerepo().FirstOrDefault(x => x.service_name.Equals(filename));
                            if (services != null)
                            {
                                
                                int rowIndex = dataGridView1.Rows.Count - 1; // Assuming the row is added at the end

                               
                                var pathCell = dataGridView1.Rows[rowIndex].Cells["ExecutablePath"]; 

                                
                                pathCell.Style.BackColor = Color.Red;
                            }
                            var md = new MDrepo().FirstOrDefault(x => x.MD5.Equals(md5Hash));
                            if (md != null)
                            {
                               
                                int rowIndex = dataGridView1.Rows.Count - 1; 

                                
                                var pathCell = dataGridView1.Rows[rowIndex].Cells["MD5"]; 

                                
                                pathCell.Style.BackColor = Color.Red;
                            }
                            var sha = new SHArepo().FirstOrDefault(x => x.SHA256.Equals(sha256Hash));
                            if (sha != null)
                            {
                               
                                int rowIndex = dataGridView1.Rows.Count - 1; 

                                
                                var pathCell = dataGridView1.Rows[rowIndex].Cells["SHA256"]; 

                                
                                pathCell.Style.BackColor = Color.Red;
                            }
                            var IP = new IPrepo().FirstOrDefault(x => x.IP.Equals(networkConnections));
                            if (IP != null)
                            {
                               
                                 int rowIndex = dataGridView1.Rows.Count - 1; 

                                
                                var pathCell = dataGridView1.Rows[rowIndex].Cells["NetworkConnections"]; 

                               
                                pathCell.Style.BackColor = Color.Red;
                            }

                        }
                        catch (Exception innerEx)
                        {
                            
                            if (innerEx.Message.Contains("Access is denied"))
                                continue;

                            
                        }
                    }
                }
                catch (Exception ex)
                {
                   // AddToDataGridView("Error", 0, 0, $"Error: {ex.Message}", 0, 0, "N/A", "N/A", "N/A", "N/A");
                }

                // Pause before next iteration
                Thread.Sleep(300000);
            }
        }


        private int AddToDataGridView(string processName, int processId, int parentProcessId, string commandLineArgs, double cpuUsage, long memoryUsage, string networkConnections, string filePath, string sha256Hash, string md5Hash)
        {
            int rowIndecx=-1;
            if (dataGridView1.InvokeRequired)
            {
                dataGridView1.Invoke(new Action(() =>
                {
                    rowIndecx= dataGridView1.Rows.Add(processName, processId, parentProcessId, commandLineArgs, cpuUsage, memoryUsage, networkConnections, filePath, sha256Hash, md5Hash);
                  
                }));
              
            }
            else
            {
                rowIndecx= dataGridView1.Rows.Add(processName, processId, parentProcessId, commandLineArgs, cpuUsage, memoryUsage, networkConnections, filePath, sha256Hash, md5Hash);
           
            }
            return rowIndecx;
        }

        static int GetParentProcessId(int processId)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {processId}"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return Convert.ToInt32(obj["ParentProcessId"]);
                    }
                }
            }
            catch
            {
                // Handle errors gracefully
            }
            return -1;
        }

        static string GetCommandLineArgs(int processId)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processId}"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["CommandLine"]?.ToString() ?? "N/A";
                    }
                }
            }
            catch
            {
                
            }
            return "N/A";
        }

        static double GetCpuUsageFallback(Process process)
        {
            try
            {
                var startTime = process.TotalProcessorTime;
                var startCpuTime = DateTime.UtcNow;
                Thread.Sleep(500); 
                var endTime = process.TotalProcessorTime;
                var endCpuTime = DateTime.UtcNow;

                double cpuUsedMs = (endTime - startTime).TotalMilliseconds;
                double totalTimeMs = (endCpuTime - startCpuTime).TotalMilliseconds;
                return (cpuUsedMs / totalTimeMs) * 100 / Environment.ProcessorCount;
            }
            catch
            {
                return 0;
            }
        }

        static string GetNetworkActivity(int processId)
        {
            StringBuilder result = new StringBuilder();

            try
            {
                uint bufferSize = 0;
                GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);

                IntPtr tcpTableBuffer = Marshal.AllocHGlobal((int)bufferSize);

                try
                {
                    uint res = GetExtendedTcpTable(tcpTableBuffer, ref bufferSize, true, AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
                    if (res != 0)
                        throw new Exception($"GetExtendedTcpTable failed with error {res}");

                    MIB_TCPTABLE_OWNER_PID table = Marshal.PtrToStructure<MIB_TCPTABLE_OWNER_PID>(tcpTableBuffer);
                    IntPtr rowPtr = (IntPtr)((long)tcpTableBuffer + Marshal.SizeOf(table.dwNumEntries));

                    for (int i = 0; i < table.dwNumEntries; i++)
                    {
                        MIB_TCPROW_OWNER_PID row = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);

                        if (row.owningPid == processId && row.state == MIB_TCP_STATE.MIB_TCP_STATE_ESTAB)
                        {
                            result.AppendLine($"Local: {row.localAddr}:{row.localPort} -> Remote: {row.remoteAddr}:{row.remotePort}");
                        }

                        rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(row));
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(tcpTableBuffer);
                }
            }
            catch (Exception ex)
            {
                return $"Error capturing network activity: {ex.Message}";
            }

            return result.Length > 0 ? result.ToString() : "No active connections.";
        }

        static string ComputeFileHash(string filePath, HashAlgorithm algorithm)
        {
            try
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    byte[] hash = algorithm.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                }
            }
            catch
            {
                return "Error calculating hash.";
            }
        }

        #region PInvoke Structures and Enums
        private const int AF_INET = 2;

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref uint dwOutBufLen, bool sort, int ipVersion, TCP_TABLE_CLASS tblClass, uint reserved);

        private enum TCP_TABLE_CLASS
        {
            TCP_TABLE_BASIC_LISTENER,
            TCP_TABLE_BASIC_CONNECTIONS,
            TCP_TABLE_BASIC_ALL,
            TCP_TABLE_OWNER_PID_LISTENER,
            TCP_TABLE_OWNER_PID_CONNECTIONS,
            TCP_TABLE_OWNER_PID_ALL,
            TCP_TABLE_OWNER_MODULE_LISTENER,
            TCP_TABLE_OWNER_MODULE_CONNECTIONS,
            TCP_TABLE_OWNER_MODULE_ALL
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MIB_TCPTABLE_OWNER_PID
        {
            public uint dwNumEntries;
            private MIB_TCPROW_OWNER_PID table;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MIB_TCPROW_OWNER_PID
        {
            public MIB_TCP_STATE state;
            public uint localAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] localPortBytes;
            public uint remoteAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] remotePortBytes;
            public int owningPid;

            public ushort localPort => BitConverter.ToUInt16(new[] { localPortBytes[1], localPortBytes[0] }, 0);
            public ushort remotePort => BitConverter.ToUInt16(new[] { remotePortBytes[1], remotePortBytes[0] }, 0);
        }

        private enum MIB_TCP_STATE
        {
            MIB_TCP_STATE_CLOSED = 1,
            MIB_TCP_STATE_LISTEN,
            MIB_TCP_STATE_SYN_SENT,
            MIB_TCP_STATE_SYN_RCVD,
            MIB_TCP_STATE_ESTAB,
            MIB_TCP_STATE_FIN_WAIT1,
            MIB_TCP_STATE_FIN_WAIT2,
            MIB_TCP_STATE_CLOSE_WAIT,
            MIB_TCP_STATE_CLOSING,
            MIB_TCP_STATE_LAST_ACK,
            MIB_TCP_STATE_TIME_WAIT,
            MIB_TCP_STATE_DELETE_TCB
        }
        #endregion
        public string path { get; set; }
        private  void button2_ClickAsync(object sender, EventArgs e)
        {
           
            f.panelform(new File_Scan(dataGridView1.CurrentRow.Cells[7].Value.ToString()));
            foreach (Control ctrl in f.tableLayoutPanel1.Controls)
            {
                if (ctrl is Button)
                {
                    Button textBox = (Button)ctrl;
                    textBox.BackColor = Color.FromArgb(235, 235, 235);
                }
            }
           f. button3.BackColor = Color.White;
        }


        private void UpdateTotalProcessesAndNetworkUsage()
        {
           
            int totalProcesses = Process.GetProcesses().Length;

           
            float totalSentBytes = 0;
            float totalReceivedBytes = 0;

            var category = new PerformanceCounterCategory("Process");
            string[] instances = category.GetInstanceNames();

            foreach (var instance in instances)
            {
                try
                {
                    using (var sentCounter = new PerformanceCounter("Process", "IO Write Bytes/sec", instance, true))
                    using (var receivedCounter = new PerformanceCounter("Process", "IO Read Bytes/sec", instance, true))
                    {
                        totalSentBytes += sentCounter.NextValue();
                        totalReceivedBytes += receivedCounter.NextValue();
                    }
                }
                catch
                {
                    
                }
            }

           
            float totalSentKb = totalSentBytes / 1024;
            float totalReceivedKb = totalReceivedBytes / 1024;

            
            lblTotalProcesses.Text = $"Total Processes: {totalProcesses}";
            lblTotalNetworkUsage.Text = $"Total Network Usage: Sent: {totalSentKb:F2} KB/s, Received: {totalReceivedKb:F2} KB/s";
        }






        private void button1_Click(object sender, EventArgs e)
        {
            f.panelform(new Ask_Gpt(dataGridView1.CurrentRow.Cells[0].Value.ToString(),
                dataGridView1.CurrentRow.Cells[1].Value.ToString(),
                dataGridView1.CurrentRow.Cells[2].Value.ToString(),
                dataGridView1.CurrentRow.Cells[3].Value.ToString(),
                dataGridView1.CurrentRow.Cells[4].Value.ToString(),
                dataGridView1.CurrentRow.Cells[5].Value.ToString(),
                dataGridView1.CurrentRow.Cells[6].Value.ToString(),
                dataGridView1.CurrentRow.Cells[7].Value.ToString(),
                dataGridView1.CurrentRow.Cells[8].Value.ToString(),
                dataGridView1.CurrentRow.Cells[9].Value.ToString()));
            foreach (Control ctrl in f.tableLayoutPanel1.Controls)
            {
                if (ctrl is Button)
                {
                    Button textBox = (Button)ctrl;
                    textBox.BackColor = Color.FromArgb(235, 235, 235);
                }
            }
            f.button6.BackColor = Color.White;

        }
    }
  

public class NetworkUsageMonitor
    {
        private PerformanceCounter bytesSentCounter;
        private PerformanceCounter bytesReceivedCounter;

        public NetworkUsageMonitor(int processId)
        {
            string instanceName = GetProcessInstanceName(processId);

            if (instanceName != null)
            {
                bytesSentCounter = new PerformanceCounter("Process", "IO Write Bytes/sec", instanceName);
                bytesReceivedCounter = new PerformanceCounter("Process", "IO Read Bytes/sec", instanceName);
            }
        }

        public (float sentBytesPerSec, float receivedBytesPerSec) GetNetworkUsage()
        {
            if (bytesSentCounter == null || bytesReceivedCounter == null)
                return (0, 0);

            float sentBytes = bytesSentCounter.NextValue();
            float receivedBytes = bytesReceivedCounter.NextValue();

            return (sentBytes, receivedBytes);
        }

        private string GetProcessInstanceName(int processId)
        {
            var category = new PerformanceCounterCategory("Process");
            string[] instances = category.GetInstanceNames();

            foreach (string instance in instances)
            {
                using (PerformanceCounter counter = new PerformanceCounter("Process", "ID Process", instance, true))
                {
                    if (counter.RawValue == processId)
                    {
                        return instance;
                    }
                }
            }

            return null;
        }
    }
}
