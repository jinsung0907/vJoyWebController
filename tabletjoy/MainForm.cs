using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tabletjoy
{
    public partial class MainForm : Form
    {
        private vjoyHandler vjoy;
        private WsListener ws;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            vjoy = new vjoyHandler();
            vjoy.startJoy();
            ws = new WsListener();
            ws.vjoy = vjoy;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int port = int.Parse(portTextBox.Text);
            ws.startServer(port);

            portTextBox.ReadOnly = true;
            serverStatus_label.Text = "ON";
            serverStatus_label.ForeColor = System.Drawing.Color.Green;
            button1.Enabled = false;
        }

        private void portTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Verify that the pressed key isn't CTRL or any non-numeric digit
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // If you want, you can allow decimal (float) numbers
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
    }
}
