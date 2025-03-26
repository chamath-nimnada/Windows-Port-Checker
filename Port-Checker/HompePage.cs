using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;


namespace Port_Checker
{
    public partial class PortCheck : Form
    {
        public PortCheck()
        {
            InitializeComponent();
            LoaddAllPorts();
        }

        private bool IsPortInUse(int port)
        {
            var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnections = ipProperties.GetActiveTcpListeners();
            var udpConnections = ipProperties.GetActiveUdpListeners();

            return tcpConnections.Any(t => t.Port == port) || udpConnections.Any(u => u.Port == port);
        }


        private void PortCheck_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtPort.Text, out int port))
            {
                bool inUse = IsPortInUse(port);
                MessageBox.Show($"Port {port} is {(inUse ? "In Use" : "Free")}.", "Port Status");
                txtPort.Clear();
            }
            else
            {
                MessageBox.Show("Please enter a valid port number.", "Error");
                txtPort.Clear();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoaddAllPorts();
        }

        private void LoaddAllPorts() 
        {
            lstBusyPorts.Items.Clear();
            lstFreePorts.Items.Clear();

            var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnections = ipProperties.GetActiveTcpListeners().Select(t => t.Port).ToList();
            var udpConnections = ipProperties.GetActiveUdpListeners().Select(u => u.Port).ToList();

            var busyPorts = tcpConnections.Concat(udpConnections).Distinct().ToList();
            var allPorts = Enumerable.Range(1, 65535);
            var freePorts = allPorts.Except(busyPorts).OrderBy(_ => Guid.NewGuid()).Take(100); // Random 100 free ports

            foreach (var port in busyPorts)
            {
                lstBusyPorts.Items.Add($"Port {port} - In Use");
            }

            foreach (var port in freePorts)
            {
                lstFreePorts.Items.Add($"Port {port} - Free");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoaddAllPorts();
        }
    }
}
