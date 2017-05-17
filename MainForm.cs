using System;
using System.Drawing;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkResourceChecker
{
	public partial class MainForm : Form
	{
		const int CHECK_PERIOD_SECONDS = 30;

		readonly KnownPort[] knownPorts =
		{
			new KnownPort("HTTP / 80", 80), 
            new KnownPort("HTTPS / 81", 81), 
            new KnownPort("FTP / 21", 21),
            new KnownPort("RDP / 3389", 3389)
		};

		bool running;

		public MainForm()
		{
			InitializeComponent();

			timer.Interval = CHECK_PERIOD_SECONDS * 1000;

			cbPort.DataSource = knownPorts;
			cbPort.DisplayMember = "name";
			cbPort.ValueMember = "port";

			if (!string.IsNullOrEmpty(getProp<string>("host"))) tbHost.Text = getProp<string>("host");
			cbPort.Text = getProp<int>("port").ToString();
		}

		private void MainForm_Resize(object sender, EventArgs e)
		{
			if (WindowState == FormWindowState.Minimized)
			{
				notifyIcon.Icon = SystemIcons.Application;
				notifyIcon.Visible = true;
				Hide();
			}
			else if (WindowState == FormWindowState.Normal)
			{
				notifyIcon.Visible = false;
			}
		}

		async void timer_Tick(object sender, EventArgs e)
		{
			if (!running) return;

			if (tbHost.Text != "" &&  getSelectedPort() != 0)
			{
				if (await isResourceAvailable(tbHost.Text, getSelectedPort()))
				{
					notifyIcon.Icon = SystemIcons.Application;
					notifyIcon.ShowBalloonTip(30*1000, "Resource avalilable", tbHost.Text + " : " + getSelectedPort(), ToolTipIcon.Info);
					btStop_Click(null, null);
				}
			}
		}

		private void notifyIcon_Click(object sender, EventArgs e)
		{
			notifyIcon.Visible = false;
			Show();
			WindowState = FormWindowState.Normal;
		}

		async Task<bool> isResourceAvailable(string host, int port)
		{
			using (var tcpClient = new TcpClient())
			{
				try
				{
					await tcpClient.ConnectAsync(host, port);
					return true;
				}
				catch
				{
					return false;
				}
			}
		}

		private void btStart_Click(object sender, EventArgs e)
		{
			if (tbHost.Text != "") setProp("host", tbHost.Text);
			if (cbPort.Text != "") setProp("port", getSelectedPort());
			Properties.Settings.Default.Save();

			running = true;
			btStart.Enabled = false;
			btStop.Enabled = true;
			tbHost.ReadOnly = true;
			cbPort.Enabled = false;
		}

		private void btStop_Click(object sender, EventArgs e)
		{
			running = false;
			btStart.Enabled = true;
			btStop.Enabled = false;
			tbHost.ReadOnly = false;
			cbPort.Enabled = true;
		}

		T getProp<T>(string name)
		{
			return (T)Properties.Settings.Default[name];
		}

		void setProp<T>(string name, T v)
		{
			Properties.Settings.Default[name] = v;
		}

		int getSelectedPort()
		{
			try
			{
				return (int?)cbPort.SelectedValue ?? int.Parse(cbPort.Text);
			}
			catch
			{
				return 0;
			}
		}
	}
}
