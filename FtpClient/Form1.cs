using MyFtp3;

namespace FtpClient
{
	public partial class Form1 : Form
	{
		FtpService service = new();

		public Form1()
		{
			InitializeComponent();
		}

		void Log(string message)
		{
			if (txtStatus.Text.Length != 0)
			{
				txtStatus.AppendText("\n");
			}
			txtStatus.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] {message.Trim()}");
			txtStatus.SelectionStart = txtStatus.Text.Length - 1;
			txtStatus.SelectionLength = 0;
			txtStatus.ScrollToCaret();
		}
		
		void LogStatus(string statMsg)
		{
			Log($"Status: {statMsg}");
		}

		void LogError(string errMsg)
		{
			Log($"Error: {errMsg}");
		}

		private void btnConnect_Click(object sender, EventArgs e)
		{
			try
			{
				service.Host = txtHost.Text;
				service.User = txtUser.Text;
				service.Pass = txtPass.Text;
				service.Connect();
			}
			catch (Exception ex)
			{
				LogError(ex.Message);
			}
		}
	}
}