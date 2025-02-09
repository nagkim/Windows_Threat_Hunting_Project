using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskScheduler;

namespace Interface
{
    public partial class Tasks : Form
    {
        private Dictionary<string, List<int>> taskProcesses = new Dictionary<string, List<int>>();
        private static readonly string ibmApiKey = "api_key_here"; 

        private int totalTasks = 0;
        private int suspiciousPositiveTasks = 0;
        private int suspiciousNegativeTasks = 0;
        private Panel barPanel;
        private Label totalCountLabel;

        public Tasks()
        {
            InitializeComponent();
            deleteButton.Click += new EventHandler(deleteButton_Click);
        }

        private async void Tasks_Load(object sender, EventArgs e)
        {
            await LoadScheduledTasksAsync();
            MonitorNetworkActivity();
            InitializeTaskCountsBar();
            UpdateTaskCountsBar();
        }

        private async Task LoadScheduledTasksAsync()
        {
            try
            {
               
                TaskScheduler.TaskScheduler taskService = new TaskScheduler.TaskScheduler();
                taskService.Connect();

               
                ITaskFolder rootFolder = taskService.GetFolder("\\");

                
                var tasks = GetAllTasks(rootFolder);

                
                DataTable dt = new DataTable();
                dt.Columns.Add("Name");
                dt.Columns.Add("Next Run Time");
                dt.Columns.Add("Last Run Time");
                dt.Columns.Add("State");
                dt.Columns.Add("Is Suspicious");

                totalTasks = tasks.Count;
                suspiciousPositiveTasks = 0;
                suspiciousNegativeTasks = 0;

                
                foreach (var task in tasks)
                {
                    DataRow row = dt.NewRow();
                    row["Name"] = task.Name;
                    row["Next Run Time"] = task.NextRunTime.ToString("g");
                    row["Last Run Time"] = task.LastRunTime.ToString("g");
                    row["State"] = task.State.ToString();
                    row["Is Suspicious"] = await IsTaskSuspiciousAsync(task) ? "Yes" : "No";

                    dt.Rows.Add(row);

                    if (row["Is Suspicious"].ToString() == "Yes")
                    {
                        suspiciousPositiveTasks++;
                    }
                    else
                    {
                        suspiciousNegativeTasks++;
                    }

                    
                    TrackTaskProcesses(task);
                }

                
                tasks_grid.DataSource = dt;
                tasks_grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                
                foreach (DataGridViewRow row in tasks_grid.Rows)
                {
                    if (row.Cells["Is Suspicious"].Value != null && row.Cells["Is Suspicious"].Value.ToString() == "Yes")
                    {
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.LightCoral;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.LightGreen;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private List<IRegisteredTask> GetAllTasks(ITaskFolder folder)
        {
            List<IRegisteredTask> taskList = new List<IRegisteredTask>();
            var tasks = folder.GetTasks((int)_TASK_ENUM_FLAGS.TASK_ENUM_HIDDEN);
            foreach (IRegisteredTask task in tasks)
            {
                taskList.Add(task);
            }

            var subFolders = folder.GetFolders(0);
            foreach (ITaskFolder subFolder in subFolders)
            {
                taskList.AddRange(GetAllTasks(subFolder));
            }

            return taskList;
        }

        private void TrackTaskProcesses(IRegisteredTask task)
        {
            foreach (IAction action in task.Definition.Actions)
            {
                if (action is IExecAction execAction)
                {
                    string processPath = execAction.Path;
                    try
                    {
                        if (!string.IsNullOrEmpty(processPath) && System.IO.File.Exists(processPath))
                        {
                            Process[] processes = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(processPath));
                            if (processes.Length > 0)
                            {
                                taskProcesses[task.Name] = processes.Select(p => p.Id).ToList();
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Invalid path for task '{task.Name}': {processPath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error tracking processes for task '{task.Name}': {ex.Message}");
                    }
                }
            }
        }

        private void MonitorNetworkActivity()
        {
            try
            {
                foreach (var task in taskProcesses)
                {
                    foreach (var processId in task.Value)
                    {
                        try
                        {
                            var tcpConnections = GetAllTcpConnections();
                            var taskConnections = tcpConnections.Where(c => c.OwningPid == processId).ToList();

                            foreach (var connection in taskConnections)
                            {
                                
                                Console.WriteLine($"Task: {task.Key}, Process ID: {processId}, Local Address: {connection.LocalEndPoint}, Remote Address: {connection.RemoteEndPoint}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error monitoring network activity for Process ID: {processId}. {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MonitorNetworkActivity: {ex.Message}");
            }
        }

        private async Task<bool> IsTaskSuspiciousAsync(IRegisteredTask task)
        {
            string name = task.Name.ToLower();
            string[] safeKeywords = { "update", "system", "windows", "microsoft" };
            if (safeKeywords.Any(keyword => name.Contains(keyword)))
            {
                return false;
            }

            foreach (IAction action in task.Definition.Actions)
            {
                if (action is IExecAction execAction)
                {
                    string path = execAction.Path.ToLower();
                    if (!path.StartsWith(@"c:\windows") && !path.StartsWith(@"c:\program files"))
                    {
                        return true;
                    }

                    
                    string[] suspiciousPaths = { @"appdata\roaming", @"appdata\local" };
                    if (suspiciousPaths.Any(suspiciousPath => path.Contains(suspiciousPath)))
                    {
                        return true;
                    }

                   
                    if (string.IsNullOrEmpty(execAction.Arguments))
                    {
                        return true;
                    }

                    
                    string[] suspiciousExecutables = { "cmd.exe", "powershell.exe", "regsvr32.exe", "rundll32.exe", "mshta.exe" };
                    if (suspiciousExecutables.Any(exe => path.EndsWith(exe)))
                    {
                        return true;
                    }

                    // IBM X-Force Exchange check
                    string fileHash = GetFileHash(path); 
                    bool isKnownMalicious = await XForceExchangeService.IsKnownMalicious(fileHash);
                    if (isKnownMalicious)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private string GetFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    var hashBytes = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        private List<TcpConnectionInformationEx> GetAllTcpConnections()
        {
            List<TcpConnectionInformationEx> table = new List<TcpConnectionInformationEx>();
            int afInet = AF_INET;
            int tcpTableLength = 0;
            IntPtr tcpTable = IntPtr.Zero;

            GetExtendedTcpTable(IntPtr.Zero, ref tcpTableLength, true, afInet, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
            try
            {
                tcpTable = Marshal.AllocHGlobal(tcpTableLength);
                int result = GetExtendedTcpTable(tcpTable, ref tcpTableLength, true, afInet, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
                if (result != 0)
                    throw new Exception("Unable to retrieve TCP table.");

                MIB_TCPTABLE_OWNER_PID tableStruct = (MIB_TCPTABLE_OWNER_PID)Marshal.PtrToStructure(tcpTable, typeof(MIB_TCPTABLE_OWNER_PID));
                IntPtr rowPtr = (IntPtr)((long)tcpTable + Marshal.SizeOf(tableStruct.dwNumEntries));
                for (int i = 0; i < tableStruct.dwNumEntries; i++)
                {
                    MIB_TCPROW_OWNER_PID row = (MIB_TCPROW_OWNER_PID)Marshal.PtrToStructure(rowPtr, typeof(MIB_TCPROW_OWNER_PID));
                    table.Add(new TcpConnectionInformationEx(row));
                    rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(row));
                }
            }
            finally
            {
                Marshal.FreeHGlobal(tcpTable);
            }

            return table;
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            try
            {
                
                if (tasks_grid.SelectedRows.Count > 0)
                {
                    DialogResult result = MessageBox.Show("Are you sure you want to stop the selected tasks?", "Confirm Stop", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        foreach (DataGridViewRow row in tasks_grid.SelectedRows)
                        {
                            string taskName = row.Cells["Name"].Value.ToString();
                            bool isSuspicious = row.Cells["Is Suspicious"].Value.ToString() == "Yes";
                            StopTask(taskName);
                            tasks_grid.Rows.Remove(row);

                            
                            totalTasks--;
                            if (isSuspicious)
                            {
                                suspiciousPositiveTasks--;
                            }
                            else
                            {
                                suspiciousNegativeTasks--;
                            }

                           
                            UpdateTaskCountsBar();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select at least one task to stop.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error stopping task: " + ex.Message);
            }
        }

        private void StopTask(string taskName)
        {
            try
            {
                TaskScheduler.TaskScheduler taskService = new TaskScheduler.TaskScheduler();
                taskService.Connect();
                ITaskFolder rootFolder = taskService.GetFolder("\\");
                IRegisteredTask task = rootFolder.GetTask(taskName);
                if (task != null)
                {
                    task.Stop(0); // Stop the task
                    Console.WriteLine($"Stopped task: {taskName}");
                }
                else
                {
                    Console.WriteLine($"Task not found: {taskName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping task '{taskName}': {ex.Message}");
            }
        }

        private void InitializeTaskCountsBar()
        {
            var totalWidth = this.ClientSize.Width - 40; 
            var totalHeight = 50; 

            
            barPanel = new Panel
            {
                Width = totalWidth,
                Height = totalHeight,
                Location = new System.Drawing.Point(20, 50), 
                BorderStyle = BorderStyle.FixedSingle
            };

            this.Controls.Add(barPanel);

            
            totalCountLabel = new Label
            {
                Location = new System.Drawing.Point(20, barPanel.Bottom + 10),
                AutoSize = true
            };

            this.Controls.Add(totalCountLabel);
        }

        private void UpdateTaskCountsBar()
        {
            barPanel.Controls.Clear();

            var totalWidth = barPanel.Width;
            var totalHeight = barPanel.Height;

            
            var positiveWidth = (int)(totalWidth * ((double)suspiciousPositiveTasks / totalTasks));
            var negativeWidth = totalWidth - positiveWidth;

           
            var positiveLabel = new Label
            {
                Width = positiveWidth,
                Height = totalHeight,
                BackColor = System.Drawing.Color.LightCoral,
                Text = $"Suspicious: {suspiciousPositiveTasks}",
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };

            var negativeLabel = new Label
            {
                Width = negativeWidth,
                Height = totalHeight,
                BackColor = System.Drawing.Color.LightGreen,
                Text = $"Not Suspicious: {suspiciousNegativeTasks}",
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Location = new System.Drawing.Point(positiveWidth, 0)
            };

            
            barPanel.Controls.Add(positiveLabel);
            barPanel.Controls.Add(negativeLabel);

            
            totalCountLabel.Text = $"Total: {totalTasks}";
        }

        
        public class TcpConnectionInformationEx
        {
            public IPEndPoint LocalEndPoint { get; }
            public IPEndPoint RemoteEndPoint { get; }
            public int OwningPid { get; }

            public TcpConnectionInformationEx(MIB_TCPROW_OWNER_PID row)
            {
                LocalEndPoint = new IPEndPoint(new IPAddress(row.dwLocalAddr), BitConverter.ToUInt16(new byte[2] { (byte)row.dwLocalPort, (byte)(row.dwLocalPort >> 8) }, 0));
                RemoteEndPoint = new IPEndPoint(new IPAddress(row.dwRemoteAddr), BitConverter.ToUInt16(new byte[2] { (byte)row.dwRemotePort, (byte)(row.dwRemotePort >> 8) }, 0));
                OwningPid = (int)row.dwOwningPid; 
            }
        }

        
        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern int GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, TCP_TABLE_CLASS tblClass, int reserved);

        private const int AF_INET = 2;

        public enum TCP_TABLE_CLASS
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
        public struct MIB_TCPROW_OWNER_PID
        {
            public uint dwState;
            public uint dwLocalAddr;
            public uint dwLocalPort;
            public uint dwRemoteAddr;
            public uint dwRemotePort;
            public uint dwOwningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPTABLE_OWNER_PID
        {
            public uint dwNumEntries;
            public MIB_TCPROW_OWNER_PID table;
        }
    }

    // Add the XForceExchangeService class here
    public static class XForceExchangeService
    {
        private static readonly string apiUrl = "https://api.xforce.ibmcloud.com/";
        private static readonly string apiKey = "api_key_here"; // Replace with your IBM X-Force API key

        public static async Task<bool> IsKnownMalicious(string hash)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes(apiKey))}");

                var response = await client.GetAsync($"{apiUrl}api/malware/{hash}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                    return json.malware && json.malware.family;
                }
            }

            return false;
        }
    }
}
