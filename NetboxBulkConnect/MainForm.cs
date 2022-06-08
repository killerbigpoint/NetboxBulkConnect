using NetboxBulkConnect.Models;
using MetroFramework.Forms;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Net;
using System;

namespace NetboxBulkConnect
{
    public partial class MainForm : MetroForm
    {
        // ----- API Information ----- \\

        private readonly Dictionary<string, DeviceData> devices = new Dictionary<string, DeviceData>();
        private readonly List<CableTypeChoices> cablesTypes = new List<CableTypeChoices>();

        private SettingsForm currentSettingsMenu = null;

        public MainForm()
        {
            InitializeComponent();

            Shown += MainForm_Shown;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            Config.LoadConfig();

            if (string.IsNullOrEmpty(Config.GetConfig().Server) == true)
            {
                MessageBox.Show("Server is missing", "Notice");
                new SettingsForm(this).Show();
                return;
            }

            if (string.IsNullOrEmpty(Config.GetConfig().ApiToken) == true)
            {
                MessageBox.Show("ApiToken is missing", "Notice");
                new SettingsForm(this).Show();
                return;
            }

            RequestWrapper.InitializeWebClient();
            RefreshEverything();
            ChangeMetrics(Config.GetConfig().MetricsType);
        }

        private void RefreshPort(Port.Type portType)
        {
            string endpoint = $"dcim/{Port.TypeToEndpoint(portType)}/";
            while (string.IsNullOrEmpty(endpoint) == false)
            {
                RequestWrapper.RequestResponse request = RequestWrapper.RetrieveRequest(endpoint, RequestWrapper.RetrieveType.GET);
                if (request.statusCode != HttpStatusCode.OK)
                {
                    MessageBox.Show($"Server responded with error: {request.data}", "Error");
                    return;
                }

                PortResponse response = JsonConvert.DeserializeObject<PortResponse>(request.data);
                if (response.next != null)
                {
                    int lengthToRemove = RequestWrapper.GetServer().Length + 5;
                    endpoint = response.next.Substring(lengthToRemove, response.next.Length - lengthToRemove);
                }
                else
                {
                    endpoint = string.Empty;
                }

                if (response.results == null)
                {
                    MessageBox.Show("Failed getting unconnected ports", "Error");
                    return;
                }

                foreach (ActualPort port in response.results)
                {
                    string deviceName = port.device.name;
                    bool isConnected = port.cable != null;

                    if (isConnected == false)
                    {
                        if (devices.ContainsKey(deviceName) == false)
                        {   
                            comboBox1.Items.Add(deviceName);
                            comboBox2.Items.Add(deviceName);

                            devices[deviceName] = new DeviceData()
                            {
                                id = port.device.id,
                                ports = new List<Port>()
                            };
                        }

                        devices[deviceName].ports.Add(new Port(port.id, port.name, portType));
                    }
                }
            }

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        private void RefreshCableTypes()
        {
            RequestWrapper.RequestResponse request = RequestWrapper.RetrieveRequest("dcim/cables/?limit=0&?brief=1", RequestWrapper.RetrieveType.OPTIONS);
            if (request.statusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"Server responded with error: {request.data}", "Error");
                return;
            }

            CableTypeResponse response = JsonConvert.DeserializeObject<CableTypeResponse>(request.data);
            if (response.actions == null)
            {
                MessageBox.Show("Failed getting cable types", "Error");
                return;
            }

            cablesTypes.Clear();
            comboBox3.Items.Clear();

            foreach (CableTypeChoices type in response.actions.POST.type.choices)
            {
                cablesTypes.Add(type);
                comboBox3.Items.Add(type.display_name);
            }
            comboBox3.SelectedIndex = 0;
        }

        private void DisplayDeviceAPorts(int index)
        {
            comboBox5.Items.Clear();

            var device = devices.ElementAt(index);
            foreach (var port in device.Value.ports)
            {
                comboBox5.Items.Add(port.name);
            }
            comboBox5.SelectedIndex = 0;
        }

        private void DisplayDeviceBPorts(int index)
        {
            comboBox6.Items.Clear();

            var device = devices.ElementAt(index);
            foreach (var port in device.Value.ports)
            {
                comboBox6.Items.Add(port.name);
            }
            comboBox6.SelectedIndex = 0;
        }

        private void RefreshEverything()
        {
            RefreshCableTypes();

            devices.Clear();
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();

            RefreshPort(Port.Type.Rearport);
            RefreshPort(Port.Type.Frontport);
            RefreshPort(Port.Type.Interface);
        }

        public void ChangeMetrics(Metrics.Type type)
        {
            label5.Text = $"Cable Length ({type}):";
        }

        // ---------- Combobox Callbacks ---------- \\

        // ----- Device A ----- \\
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayDeviceAPorts(comboBox1.SelectedIndex);
        }

        // ----- Device B ----- \\
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayDeviceBPorts(comboBox2.SelectedIndex);
        }

        // ---------- Button Callbacks ---------- \\

        // ----- Refresh Button ----- \\
        private void button4_Click(object sender, EventArgs e)
        {
            RefreshEverything();
        }

        // ----- Print CSV Format ----- \\
        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Coming soon", "Beta");
        }

        // ----- Settings Button ----- \\
        private void button3_Click(object sender, EventArgs e)
        {
            if (currentSettingsMenu != null && currentSettingsMenu.IsDisposed == false)
            {
                currentSettingsMenu.Dispose();
            }

            currentSettingsMenu = new SettingsForm(this);
            currentSettingsMenu.Show();
        }

        // ----- Import CSV Format ----- \\
        private void button5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Coming soon", "Beta");
        }

        // ----- Connect Ports Button ----- \\
        private void button2_Click(object sender, EventArgs e)
        {
            StringBuilder output = new StringBuilder();

            if (int.TryParse(textBox1.Text, out int portCount) == false)
            {
                output.AppendLine("Port count is not a whole number");
                textBox2.Text = output.ToString();
                return;
            }

            if (int.TryParse(textBox3.Text, out int cableLength) == false)
            {
                output.AppendLine("Cable length is not a whole number");
                textBox2.Text = output.ToString();
                return;
            }

            var deviceA = devices.ElementAt(comboBox1.SelectedIndex);
            var deviceB = devices.ElementAt(comboBox2.SelectedIndex);

            if (deviceA.Key == deviceB.Key)
            {
                output.AppendLine("You can't connect the same device to itself");
                textBox2.Text = output.ToString();
                return;
            }

            int deviceAIndex = comboBox5.SelectedIndex;
            int deviceBIndex = comboBox6.SelectedIndex;

            if ((deviceAIndex + portCount) > deviceA.Value.ports.Count)
            {
                output.AppendLine("Port count that you're trying to connect on Device A is going out of bounds");
                textBox2.Text = output.ToString();
                return;
            }

            if ((deviceBIndex + portCount) > deviceB.Value.ports.Count)
            {
                output.AppendLine("Port count that you're trying to connect on Device B is going out of bounds");
                textBox2.Text = output.ToString();
                return;
            }

            output.AppendLine($"Trying to connect {portCount} ports from {deviceA.Key} to {deviceB.Key}...");

            StringBuilder apiRequest = new StringBuilder();
            apiRequest.Append("[");

            List<Port> deviceAPortsToRemove = new List<Port>();
            List<Port> deviceBPortsToRemove = new List<Port>();

            string metricsType = Metrics.TypeToApiType(Config.GetConfig().MetricsType);

            for (int i = 0; i < portCount; i++)
            {
                if (i > 0)
                {
                    apiRequest.Append(",");
                }

                Port deviceAPort = deviceA.Value.ports[deviceAIndex];
                Port deviceBPort = deviceB.Value.ports[deviceBIndex];

                deviceAPortsToRemove.Add(deviceAPort);
                deviceBPortsToRemove.Add(deviceBPort);

                apiRequest.Append("{\"termination_a_type\": \"" + deviceAPort.GetApiName() + "\", \"termination_a_id\": " + deviceAPort.id + ", \"termination_b_type\": \"" + deviceBPort.GetApiName() + "\", \"termination_b_id\": " + deviceBPort.id + ", \"type\": \"" + cablesTypes[comboBox3.SelectedIndex].value.ToString() + "\", \"length_unit\": \"" + metricsType + "\", \"length\": " + cableLength + "}");
                output.AppendLine($"{deviceA.Key}:{deviceAPort.name} -> {deviceB.Key}:{deviceBPort.name}");

                deviceAIndex++;
                deviceBIndex++;
            }

            apiRequest.Append("]");
            HttpStatusCode responseCode = RequestWrapper.PostRequest($"dcim/cables/", apiRequest.ToString());

            if (responseCode == HttpStatusCode.OK ||
                responseCode == HttpStatusCode.Created)
            {
                foreach (Port port in deviceAPortsToRemove)
                {
                    deviceA.Value.ports.Remove(port);
                }

                foreach (Port port in deviceBPortsToRemove)
                {
                    deviceB.Value.ports.Remove(port);
                }

                output.AppendLine($"All {portCount} ports connected succesfully!");
            }
            else
            {
                output.AppendLine($"Connecting ports failed with error code: {responseCode}");
            }

            textBox2.Text = output.ToString();

            DisplayDeviceAPorts(comboBox1.SelectedIndex);
            DisplayDeviceBPorts(comboBox2.SelectedIndex);
        }
    }
}