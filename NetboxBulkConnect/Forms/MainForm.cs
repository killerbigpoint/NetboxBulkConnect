using NetboxBulkConnect.Models;
using NetboxBulkConnect.Misc;
using MetroFramework.Forms;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
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
            RefreshEverything(OnRefreshDone);
        }

        private void OnRefreshDone(bool success)
        {
            if (success == true)
            {
                ChangeMetrics(Config.GetConfig().MetricsType);
                textBox1.Text = Config.GetConfig().NumberOfPorts.ToString();
                textBox3.Text = Config.GetConfig().CableLength.ToString();
                textBox4.Text = Config.GetConfig().DeviceAPortSkips.ToString();
                textBox5.Text = Config.GetConfig().DeviceBPortSkips.ToString();

                foreach (Port.Type type in Enum.GetValues(typeof(Port.Type)))
                {
                    comboBox4.Items.Add(type.ToString());
                    comboBox7.Items.Add(type.ToString());
                }
                comboBox4.SelectedIndex = 0;
                comboBox7.SelectedIndex = 0;
            }

            UnlockUI(success);
        }

        private void LockUI()
        {
            comboBox1.Enabled = false;
            comboBox2.Enabled = false;
            comboBox3.Enabled = false;
            comboBox4.Enabled = false;
            comboBox5.Enabled = false;
            comboBox6.Enabled = false;
            comboBox7.Enabled = false;

            textBox1.Enabled = false;
            textBox3.Enabled = false;
            textBox4.Enabled = false;
            textBox5.Enabled = false;

            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
        }

        private void UnlockUI(bool success)
        {
            comboBox1.Enabled = true;
            comboBox2.Enabled = true;
            comboBox3.Enabled = true;
            comboBox4.Enabled = true;
            comboBox5.Enabled = true;
            comboBox6.Enabled = true;
            comboBox7.Enabled = true;

            textBox1.Enabled = true;
            textBox3.Enabled = true;
            textBox4.Enabled = true;
            textBox5.Enabled = true;

            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
        }

        private void RefreshPort(Port.Type portType, ProgressForm progressBar)
        {
            string endpoint = $"dcim/{Port.TypeToEndpoint(portType)}/";
            while (string.IsNullOrEmpty(endpoint) == false)
            {
                progressBar?.OutputText($"Sending request to: {endpoint}");

                RequestWrapper.RequestResponse request = RequestWrapper.RetrieveRequest(endpoint, RequestWrapper.RetrieveType.GET);
                if (request.statusCode != HttpStatusCode.OK)
                {
                    string error = $"Server responded with error: {request.data}";

                    MessageBox.Show(error, "Error");
                    progressBar?.OutputText(error);

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

                    progressBar?.OutputText("Failed getting unconnected ports");
                    return;
                }

                foreach (ActualPort port in response.results)
                {
                    string deviceName = port.device.name;
                    bool isConnected = port.cable != null;

                    if (isConnected == true)
                    {
                        continue;
                    }

                    if (devices.ContainsKey(deviceName) == false)
                    {
                        Invoke(new Action(() =>
                        {
                            comboBox1.Items.Add(deviceName);
                            comboBox2.Items.Add(deviceName);
                        }));

                        devices[deviceName] = new DeviceData()
                        {
                            id = port.device.id,
                            ports = new List<Port>()
                        };
                    }

                    devices[deviceName].ports.Add(new Port(port.id, port.name, portType));
                }
            }

            Invoke(new Action(() =>
            {
                comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = 0;
            }));
        }

        private void RefreshCableTypes(ProgressForm progressBar)
        {
            string endpoint = $"dcim/cables/?limit=0&?brief=1";
            progressBar?.OutputText($"Sending request to: {endpoint}");

            RequestWrapper.RequestResponse request = RequestWrapper.RetrieveRequest(endpoint, RequestWrapper.RetrieveType.OPTIONS);
            if (request.statusCode != HttpStatusCode.OK)
            {
                string error = $"Server responded with error: {request.data}";

                MessageBox.Show(error, "Error");
                progressBar?.OutputText(error);
                return;
            }

            CableTypeResponse response = JsonConvert.DeserializeObject<CableTypeResponse>(request.data);
            if (response.actions == null)
            {
                MessageBox.Show("Failed getting cable types", "Error");
                progressBar?.OutputText("Failed getting cable types");
                return;
            }

            cablesTypes.Clear();
            Invoke(new Action(() =>
            {
                comboBox3.Items.Clear();

                foreach (CableTypeChoices type in response.actions.POST.type.choices)
                {
                    cablesTypes.Add(type);
                    comboBox3.Items.Add(type.display_name);
                }

                int savedIndex = Config.GetConfig().CableType;
                if (savedIndex > (comboBox3.Items.Count - 1))
                {
                    comboBox3.SelectedIndex = 0;

                    Config.GetConfig().CableType = 0;
                    Config.SaveConfig();
                }
                else
                {
                    comboBox3.SelectedIndex = savedIndex;
                }
            }));
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
                if (port.type != (Port.Type)comboBox7.SelectedIndex)
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

        private void RefreshEverything(Action<bool> onComplete)
        {
            if (string.IsNullOrEmpty(Config.GetConfig().Server) == true)
            {
                MessageBox.Show("Server is missing", "Notice");
                new SettingsForm(this).Show();

                onComplete?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(Config.GetConfig().ApiToken) == true)
            {
                MessageBox.Show("ApiToken is missing", "Notice");
                new SettingsForm(this).Show();

                onComplete?.Invoke(false);
                return;
            }

            LockUI();

            devices.Clear();
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();

            ProgressForm progressBar = new ProgressForm();
            progressBar.SetMaxProgress(100);
            progressBar.Show();

            Thread fetchThread = new Thread(() =>
            {
                RefreshEverythingInternal(onComplete, progressBar);
            });
            fetchThread.Start();
        }

        private void RefreshEverythingInternal(Action<bool> onComplete, ProgressForm progressBar)
        {
            //Refresh Rearports
            Invoke(new Action(() =>
            {
                progressBar.SetCurrentProgress(0);
                progressBar.SetText("Loading Rearports");
            }));
            RefreshPort(Port.Type.Rearport, progressBar);

            //Refresh Frontports
            Invoke(new Action(() =>
            {
                progressBar.SetCurrentProgress(25);
                progressBar.SetText("Loading Frontports");
            }));
            RefreshPort(Port.Type.Frontport, progressBar);

            //Refresh Interfaces
            Invoke(new Action(() =>
            {
                progressBar.SetCurrentProgress(50);
                progressBar.SetText("Loading Interfaces");
            }));
            RefreshPort(Port.Type.Interface, progressBar);

            //Refresh Cable Types
            Invoke(new Action(() =>
            {
                progressBar.SetCurrentProgress(75);
                progressBar.SetText("Loading Cable Types");
            }));
            RefreshCableTypes(progressBar);

            //Mark as complete
            Invoke(new Action(() =>
            {
                progressBar.Dispose();
                onComplete?.Invoke(true);
            }));
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
            LockUI();
            RefreshEverything(UnlockUI);
        }

        // ----- Print CSV Format ----- \\
        private void button1_Click(object sender, EventArgs e)
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

            string cableType = cablesTypes[comboBox3.SelectedIndex].display_name;
            string metricsType = Metrics.TypeToCSV(Config.GetConfig().MetricsType);
            Port.Type deviceAPortType = (Port.Type)comboBox4.SelectedIndex;
            Port.Type deviceBPortType = (Port.Type)comboBox7.SelectedIndex;

            List<Port> portsOfTypeA = new List<Port>();
            List<Port> portsOfTypeB = new List<Port>();

            foreach (Port port in deviceA.Value.ports)
            {
                if (port.type != deviceAPortType)
                {
                    continue;
                }

                portsOfTypeA.Add(port);
            }

            foreach (Port port in deviceB.Value.ports)
            {
                if (port.type != deviceBPortType)
                {
                    continue;
                }

                portsOfTypeB.Add(port);
            }

            StringBuilder output = new StringBuilder();
            output.AppendLine("side_a_device,side_a_type,side_a_name,side_b_device,side_b_type,side_b_name,type,length,length_unit");

            List<Port> deviceAPortsToRemove = new List<Port>();
            List<Port> deviceBPortsToRemove = new List<Port>();

            int deviceAPortSkips = Config.GetConfig().DeviceAPortSkips;
            int deviceBPortSkips = Config.GetConfig().DeviceBPortSkips;

            //Start building API Request
            for (int i = 0; i < portCount; i++)
            {
                if (deviceAIndex == -1 || deviceAIndex >= portsOfTypeA.Count)
                {
                    MessageBox.Show("Port count that you're trying to connect on Device A is going out of bounds", "Error");
                    return;
                }

                if (deviceBIndex == -1 || deviceBIndex >= portsOfTypeB.Count)
                {
                    MessageBox.Show("Port count that you're trying to connect on Device B is going out of bounds", "Error");
                    return;
                }

                Port deviceAPort = portsOfTypeA[deviceAIndex];
                Port deviceBPort = portsOfTypeB[deviceBIndex];

                deviceAPortsToRemove.Add(deviceAPort);
                deviceBPortsToRemove.Add(deviceBPort);

                output.AppendLine($"{deviceA.Key},{deviceAPort.GetCSVName()},{deviceAPort.name},{deviceB.Key},{deviceBPort.GetCSVName()},{deviceBPort.name},{cableType},{cableLength},{metricsType}");

                deviceAIndex += deviceAPortSkips + 1;
                deviceBIndex += deviceBPortSkips + 1;
            }

            textBox2.Text = output.ToString();
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

        // ----- Cable Type ----- \\
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            Config.GetConfig().CableType = comboBox3.SelectedIndex;
            Config.SaveConfig();
        }

        // ----- Number Of Ports ----- \\
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox1.Text, out int ports) == false)
            {
                return;
            }

            Config.GetConfig().NumberOfPorts = ports;
            Config.SaveConfig();
        }

        // ----- Cable Length ----- \\
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox3.Text, out int ports) == false)
            {
                return;
            }

            Config.GetConfig().CableLength = ports;
            Config.SaveConfig();
        }

        // ----- Device A Port Skips ----- \\
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox4.Text, out int skips) == false)
            {
                return;
            }

            Config.GetConfig().DeviceAPortSkips = skips;
            Config.SaveConfig();
        }

        // ----- Device B Port Skips ----- \\
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox5.Text, out int skips) == false)
            {
                return;
            }

            Config.GetConfig().DeviceBPortSkips = skips;
            Config.SaveConfig();
        }

        // ----- Connect Ports Button ----- \\
        private void button2_Click(object sender, EventArgs e)
        {
            LockUI();
            ConnectPorts(UnlockUI);
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayDeviceAPorts(comboBox1.SelectedIndex);
        }

        private void comboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayDeviceBPorts(comboBox2.SelectedIndex);
        }

        private void ConnectPorts(Action<bool> onComplete)
        {
            if (int.TryParse(textBox1.Text, out int portCount) == false)
            {
                MessageBox.Show("Port count is not a whole number", "Error");

                onComplete?.Invoke(false);
                return;
            }

            if (int.TryParse(textBox3.Text, out int cableLength) == false)
            {
                MessageBox.Show("Cable length is not a whole number", "Error");

                onComplete?.Invoke(false);
                return;
            }

            var deviceA = devices.ElementAt(comboBox1.SelectedIndex);
            var deviceB = devices.ElementAt(comboBox2.SelectedIndex);

            if (deviceA.Key == deviceB.Key)
            {
                MessageBox.Show("You can't connect the same device to itself", "Error");

                onComplete?.Invoke(false);
                return;
            }

            int deviceAIndex = comboBox5.SelectedIndex;
            int deviceBIndex = comboBox6.SelectedIndex;

            string cableType = cablesTypes[comboBox3.SelectedIndex].value;
            string metricsType = Metrics.TypeToApi(Config.GetConfig().MetricsType);
            Port.Type deviceAPortType = (Port.Type)comboBox4.SelectedIndex;
            Port.Type deviceBPortType = (Port.Type)comboBox7.SelectedIndex;

            List<Port> portsOfTypeA = new List<Port>();
            List<Port> portsOfTypeB = new List<Port>();

            foreach (Port port in deviceA.Value.ports)
            {
                if (port.type != deviceAPortType)
                {
                    continue;
                }

                portsOfTypeA.Add(port);
            }

            foreach (Port port in deviceB.Value.ports)
            {
                if (port.type != deviceBPortType)
                {
                    continue;
                }

                portsOfTypeB.Add(port);
            }

            StringBuilder output = new StringBuilder();
            output.AppendLine($"Trying to connect {portCount} ports from {deviceA.Key} to {deviceB.Key}...");

            StringBuilder apiRequest = new StringBuilder();
            apiRequest.Append("[");

            List<Port> deviceAPortsToRemove = new List<Port>();
            List<Port> deviceBPortsToRemove = new List<Port>();

            int deviceAPortSkips = Config.GetConfig().DeviceAPortSkips;
            int deviceBPortSkips = Config.GetConfig().DeviceBPortSkips;

            //Start building API Request
            for (int i = 0; i < portCount; i++)
            {
                if (i > 0)
                {
                    apiRequest.Append(",");
                }

                if (deviceAIndex == -1 || deviceAIndex >= portsOfTypeA.Count)
                {
                    MessageBox.Show("Port count that you're trying to connect on Device A is going out of bounds", "Error");

                    onComplete?.Invoke(false);
                    return;
                }

                if (deviceBIndex == -1 || deviceBIndex >= portsOfTypeB.Count)
                {
                    MessageBox.Show("Port count that you're trying to connect on Device B is going out of bounds", "Error");

                    onComplete?.Invoke(false);
                    return;
                }

                Port deviceAPort = portsOfTypeA[deviceAIndex];
                Port deviceBPort = portsOfTypeB[deviceBIndex];

                deviceAPortsToRemove.Add(deviceAPort);
                deviceBPortsToRemove.Add(deviceBPort);

                apiRequest.Append("{\"termination_a_type\": \"" + deviceAPort.GetApiName() + "\", \"termination_a_id\": " + deviceAPort.id + ", \"termination_b_type\": \"" + deviceBPort.GetApiName() + "\", \"termination_b_id\": " + deviceBPort.id + ", \"type\": \"" + cableType + "\", \"length_unit\": \"" + metricsType + "\", \"length\": " + cableLength + "}");
                output.AppendLine($"{deviceA.Key}:{deviceAPort.name} --> {deviceB.Key}:{deviceBPort.name}");

                deviceAIndex += deviceAPortSkips + 1;
                deviceBIndex += deviceBPortSkips + 1;
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

            onComplete?.Invoke(true);
        }
    }
}