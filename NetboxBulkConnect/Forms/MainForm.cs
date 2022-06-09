using NetboxBulkConnect.Models;
using NetboxBulkConnect.Misc;
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

            RequestWrapper.InitializeWebClient();
            RefreshEverything();

            ChangeMetrics(Config.GetConfig().MetricsType);
            textBox1.Text = Config.GetConfig().numberOfPorts.ToString();
            textBox3.Text = Config.GetConfig().cableLength.ToString();

            foreach (Port.Type type in Enum.GetValues(typeof(Port.Type)))
            {
                comboBox4.Items.Add(type.ToString());
            }
            comboBox4.SelectedIndex = 0;
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

            int savedIndex = Config.GetConfig().cableType;
            if (savedIndex > (comboBox3.Items.Count - 1))
            {
                comboBox3.SelectedIndex = 0;

                Config.GetConfig().cableType = 0;
                Config.SaveConfig();
            }
            else
            {
                comboBox3.SelectedIndex = savedIndex;
            }
        }

        private void DisplayDeviceAPorts(int index)
        {
            comboBox5.Items.Clear();
            comboBox5.Text = string.Empty;

            var device = devices.ElementAt(index);
            foreach (var port in device.Value.ports)
            {
                if (port.type != (Port.Type)comboBox4.SelectedIndex)
                {
                    continue;
                }

                comboBox5.Items.Add(port.name);
            }
            
            if (comboBox5.Items.Count > 0)
            {
                comboBox5.SelectedIndex = 0;
            }
        }

        private void DisplayDeviceBPorts(int index)
        {
            comboBox6.Items.Clear();
            comboBox6.Text = string.Empty;

            var device = devices.ElementAt(index);
            foreach (var port in device.Value.ports)
            {
                if (port.type != (Port.Type)comboBox4.SelectedIndex)
                {
                    continue;
                }

                comboBox6.Items.Add(port.name);
            }

            if (comboBox6.Items.Count > 0)
            {
                comboBox6.SelectedIndex = 0;
            }
        }

        private void RefreshEverything()
        {
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

            devices.Clear();
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();

            ProgressForm progressBar = new ProgressForm();
            progressBar.SetMaxProgress(100);
            progressBar.Show();

            progressBar.SetText("Loading Rearports");
            RefreshPort(Port.Type.Rearport);

            progressBar.SetCurrentProgress(25);
            progressBar.SetText("Loading Frontports");
            RefreshPort(Port.Type.Frontport);

            progressBar.SetCurrentProgress(50);
            progressBar.SetText("Loading Interfaces");
            RefreshPort(Port.Type.Interface);

            progressBar.SetCurrentProgress(75);
            progressBar.SetText("Loading Cable Types");
            RefreshCableTypes();

            progressBar.Dispose();
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

        // ----- Cable Type ----- \\
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            Config.GetConfig().cableType = comboBox3.SelectedIndex;
            Config.SaveConfig();
        }

        // ----- Number Of Ports ----- \\
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox1.Text, out int ports) == false)
            {
                return;
            }

            Config.GetConfig().numberOfPorts = ports;
            Config.SaveConfig();
        }

        // ----- Cable Length ----- \\
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox3.Text, out int ports) == false)
            {
                return;
            }

            Config.GetConfig().cableLength = ports;
            Config.SaveConfig();
        }

        // ----- Connect Ports Button ----- \\
        private void button2_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox1.Text, out int portCount) == false)
            {
                MessageBox.Show("Port count is not a whole number", "Error");
                return;
            }

            if (int.TryParse(textBox3.Text, out int cableLength) == false)
            {
                MessageBox.Show("Cable length is not a whole number", "Error");
                return;
            }

            var deviceA = devices.ElementAt(comboBox1.SelectedIndex);
            var deviceB = devices.ElementAt(comboBox2.SelectedIndex);

            if (deviceA.Key == deviceB.Key)
            {
                MessageBox.Show("You can't connect the same device to itself", "Error");
                return;
            }

            int deviceAIndex = comboBox5.SelectedIndex;
            int deviceBIndex = comboBox6.SelectedIndex;

            if ((deviceAIndex + portCount) > deviceA.Value.ports.Count)
            {
                MessageBox.Show("Port count that you're trying to connect on Device A is going out of bounds", "Error");
                return;
            }

            if ((deviceBIndex + portCount) > deviceB.Value.ports.Count)
            {
                MessageBox.Show("Port count that you're trying to connect on Device B is going out of bounds", "Error");
                return;
            }

            StringBuilder output = new StringBuilder();
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

                if (deviceAPort.type != (Port.Type)comboBox4.SelectedIndex ||
                    deviceBPort.type != (Port.Type)comboBox4.SelectedIndex)
                {
                    MessageBox.Show($"Port: {i} is not of type: {(Port.Type)comboBox4.SelectedIndex}", "Error");
                    return;
                }

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

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayDeviceAPorts(comboBox1.SelectedIndex);
            DisplayDeviceBPorts(comboBox2.SelectedIndex);
        }
    }
}