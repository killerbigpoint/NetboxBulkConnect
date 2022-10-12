using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetboxBulkConnect
{
    public partial class ProgressForm : MetroForm
    {
        public ProgressForm()
        {
            InitializeComponent();
        }

        public void SetText(string text)
        {
            label1.Text = text;
        }

        public void SetMaxProgress(int maxProgress)
        {
            progressBar1.Maximum = maxProgress;
        }

        public void SetCurrentProgress(int currentProgress)
        {
            progressBar1.Value = currentProgress;
        }

        public void OutputText(string text)
        {
            Invoke(new Action(() =>
            {
                textBox1.AppendText(text + "\r\n");
            }));
        }
    }
}
