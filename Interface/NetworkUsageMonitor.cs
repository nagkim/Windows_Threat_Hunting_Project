using System.Diagnostics;

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