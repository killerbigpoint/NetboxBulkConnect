using NetboxBulkConnect.Models;
using NetboxBulkConnect.Misc;
using MetroFramework.Forms;
using System;

namespace NetboxBulkConnect
{
    public partial class SettingsForm : MetroForm
    {
        private readonly MainForm mainForm;

        public SettingsForm(MainForm mainForm)
        {
            InitializeComponent();

            this.mainForm = mainForm;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            foreach (Metrics.Type type in Enum.GetValues(typeof(Metrics.Type)))
            {
                comboBox1.Items.Add(type.ToString());
            }

            textBox1.Text = Config.GetConfig().Server;
            textBox2.Text = Config.GetConfig().ApiToken;
            checkBox1.Checked = Config.GetConfig().UseHttpEncryption;

            checkBox2.Checked = Config.GetConfig().UseTooltips;
            toolTip1.Active = Config.GetConfig().UseTooltips;
            comboBox1.SelectedIndex = (int)Config.GetConfig().MetricsType;
        }

        // ----- Server IP ----- \\
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Config.GetConfig().Server = textBox1.Text;
        }

        // ----- API Token ----- \\
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Config.GetConfig().ApiToken = textBox2.Text;
        }

        // ----- Use HTTP Encryption ----- \\
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Config.GetConfig().UseHttpEncryption = checkBox1.Checked;
        }

        // ----- Use Tooltips ----- \\
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Config.GetConfig().UseTooltips = checkBox2.Checked;
            toolTip1.Active = Config.GetConfig().UseTooltips;
            mainForm.ChangeTooltipState(Config.GetConfig().UseTooltips);
        }

        // ----- Change Metrics ----- \\
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Config.GetConfig().MetricsType = (Metrics.Type)comboBox1.SelectedIndex;
            mainForm.ChangeMetrics(Config.GetConfig().MetricsType);
        }

        // ----- Save Config ----- \\
        private void button1_Click(object sender, EventArgs e)
        {
            Config.SaveConfig();
            Dispose();
        }
    }
}
