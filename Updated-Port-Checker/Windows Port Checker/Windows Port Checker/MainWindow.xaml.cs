using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Windows_Port_Checker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadRandomFreePorts();
            LoadRandomBusyPorts();
        }

        private void LoadRandomFreePorts()
        {
            var freePorts = GetFreePorts(20);
            FreePortsList.ItemsSource = freePorts;
        }

        private void LoadRandomBusyPorts()
        {
            var busyPorts = GetBusyPorts(20);
            BusyPortsList.ItemsSource = busyPorts;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadRandomFreePorts();
            LoadRandomBusyPorts();
        }

        private void SearchPort_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(PortSearchBox.Text, out int port))
            {
                string result = CheckPortStatus(port);
                PortStatusText.Text = result;
            }
            else
            {
                PortStatusText.Text = "Please enter a valid port number.";
            }
        }

        private List<int> GetFreePorts(int count)
        {
            var usedPorts = GetUsedPorts();
            var freePorts = new List<int>();
            var rnd = new Random();

            while (freePorts.Count < count)
            {
                int port = rnd.Next(1024, 65535);
                if (!usedPorts.Contains(port) && !freePorts.Contains(port))
                {
                    freePorts.Add(port);
                }
            }

            return freePorts;
        }

        private List<string> GetBusyPorts(int count)
        {
            var usedPorts = GetUsedPorts();
            var busyPortsWithDetails = new List<string>();
            var rnd = new Random();
            var usedPortsList = usedPorts.ToList();

            if (usedPortsList.Count == 0)
            {
                busyPortsWithDetails.Add("No busy ports found");
                return busyPortsWithDetails;
            }

            var selectedPorts = new HashSet<int>();
            int attempts = 0;

            while (busyPortsWithDetails.Count < count && attempts < count * 3)
            {
                attempts++;
                if (usedPortsList.Count == 0) break;

                int randomIndex = rnd.Next(usedPortsList.Count);
                int port = usedPortsList[randomIndex];

                if (!selectedPorts.Contains(port))
                {
                    selectedPorts.Add(port);
                    string processInfo = GetProcessUsingPort(port);
                    busyPortsWithDetails.Add($"Port {port} - {processInfo}");
                }
            }

            return busyPortsWithDetails;
        }

        private HashSet<int> GetUsedPorts()
        {
            var usedPorts = new HashSet<int>();

            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndpoints = ipProperties.GetActiveTcpListeners();
            IPEndPoint[] udpEndpoints = ipProperties.GetActiveUdpListeners();

            usedPorts.UnionWith(tcpEndpoints.Select(p => p.Port));
            usedPorts.UnionWith(udpEndpoints.Select(p => p.Port));

            return usedPorts;
        }

        private string CheckPortStatus(int port)
        {
            var usedPorts = GetUsedPorts();
            if (!usedPorts.Contains(port))
                return $"Port {port} is free.";

            string process = GetProcessUsingPort(port);
            return $"Port {port} is busy.\nUsed by: {process}";
        }

        private string GetProcessUsingPort(int port)
        {
            try
            {
                Process netstat = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c netstat -ano | findstr :{port}",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                netstat.Start();
                string output = netstat.StandardOutput.ReadToEnd();
                netstat.WaitForExit();

                if (string.IsNullOrWhiteSpace(output))
                    return "Unknown process (not detected via netstat).";

                // Extract PID from output
                string[] lines = output.Split('\n');
                foreach (var line in lines)
                {
                    var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 5 && int.TryParse(parts[4], out int pid))
                    {
                        var proc = Process.GetProcessById(pid);
                        return $"{proc.ProcessName} (PID: {pid})";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }

            return "Unknown process.";
        }
    }
}