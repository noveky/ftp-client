using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyFtp3;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FtpClient
{
	public partial class Form2 : Form
	{
		FtpService service = new();

		public Form2()
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
			Log($"状态：{statMsg}");
		}

		void LogError(string errMsg)
		{
			Log($"错误：{errMsg}");
		}

		void UpdateDir(string newDir)
		{
			string lstWorkDir = service.workDir;
			try
			{
				newDir = newDir.Replace('\\', '/').TrimEnd('/') + "/";
				while (newDir.Contains("//"))
				{
					newDir = newDir.Replace("//", "/");
				}
				service.workDir = newDir;
				service.ListWorkDir(); // 测试一下是否有效，否则抛出异常
				DisplayDir();
			}
			catch (Exception ex)
			{
				LogError(ex.Message);

				// 回滚
				service.workDir = txtWorkDir.Text = lstWorkDir;
				DisplayDir();
			}
		}

		string GetSizeStr(long size)
		{
			if (size < 1024)
			{
				return $"{size} B";
			}
			else if(size < 1048576)
			{
				return $"{(long)Math.Round((double)size / 1024)} KB";
			}
			else if (size < 1073741824)
			{
				return $"{(long)Math.Round((double)size / 1048576)} MB";
			}
			else
			{
				return $"{(long)Math.Round((double)size / 1073741824)} GB";
			}
		}

		void DisplayDir()
		{
			try
			{
				txtWorkDir.Text = service.workDir;
				lstWorkDir.Items.Clear();
				FtpFileInfo[] infos = service.ListWorkDir();
				foreach (var info in infos)
				{
					ListViewItem item = new()
					{
						Text = info.Name,
						Name = info.IsDirectory ? "dir" : "file",
					};
					if (info.IsDirectory)
					{
						item.SubItems.Add("<DIR>");
						item.SubItems.Add("<DIR>");
					}
					else
					{
						item.SubItems.Add(info.LastModified.ToString());
						item.SubItems.Add(GetSizeStr(info.Size));
					}
					lstWorkDir.Items.Add(item);
				}
			}
			catch (Exception ex)
			{
				LogError(ex.Message);
			}
		}

		private void btnConnect_Click(object sender, EventArgs e)
		{
			try
			{
				service.host = txtHost.Text;
				service.user = txtUser.Text;
				service.pass = txtPass.Text;
				LogStatus(service.Connect());
				DisplayDir();
			}
			catch (Exception ex)
			{
				LogError(ex.Message);
			}
		}

		private void btnUp_Click(object sender, EventArgs e)
		{
			UpdateDir((Path.GetDirectoryName(service.workDir.TrimEnd('/')) ?? "/"));
		}

		private void txtWorkDir_TextChanged(object sender, EventArgs e)
		{
			try
			{
				if (txtWorkDir.Text.Contains('\n') || txtWorkDir.Text.Contains('\r'))
				{
					txtWorkDir.Text = txtWorkDir.Text.Trim().Replace("\n", "").Replace("\r", "");
					ActiveControl = null;
				}
			}
			catch (Exception ex)
			{
				LogError(ex.Message);
			}
		}

		private void lstWorkDir_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			ListViewHitTestInfo hitInfo = lstWorkDir.HitTest(e.X, e.Y);
			ListViewItem? item = hitInfo.Item;

			if (item != null)
			{
				// 若是目录，则访问目录
				if (item.Name == "dir")
				{
					UpdateDir(service.workDir.TrimEnd('/') + "/" + item.Text + "/");
				}
			}
		}

		private void txtWorkDir_Leave(object sender, EventArgs e)
		{
			UpdateDir(txtWorkDir.Text);
		}

		private void btnRefresh_Click(object sender, EventArgs e)
		{
			DisplayDir();
		}
	}
}
