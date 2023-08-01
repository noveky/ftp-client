using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceProcess;
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

		void LogReply(string statMsg)
		{
			Log($"应答：{statMsg}");
		}

		void LogStatus(string statMsg)
		{
			Log($"状态：{statMsg}");
		}

		void LogError(string errMsg)
		{
			Log($"错误：{errMsg}");
		}

		// 更改工作路径
		void ChangeDir(string newDir)
		{
			string lstDir = service.workDir; // 更新前的工作路径
			try
			{
				// 将路径的写法规范化
				newDir = newDir.Replace('\\', '/').TrimEnd('/') + "/";
				while (newDir.Contains("//"))
				{
					newDir = newDir.Replace("//", "/");
				}

				if (lstDir != newDir)
				{
					service.workDir = newDir;
					service.ListWorkDir(); // 测试一下新路径是否有效，否则抛出异常，不更新

					LogStatus($"转到路径 {service.workDir}");
				}

				// 获取并显示文件列表
				Exception? dispDirEx = DisplayDir();
				if (dispDirEx != null)
				{
					throw dispDirEx;
				}
			}
			catch (Exception ex)
			{
				LogError(ex.Message);
				LogStatus($"路径 {newDir} 无效或无法访问");

				// 回滚路径
				service.workDir = txtWorkDir.Text = lstDir;
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

		// 获取并显示文件列表，返回抛出的异常
		Exception? DisplayDir()
		{
			try
			{
				txtWorkDir.Text = service.workDir;
				lstWorkDir.Items.Clear();

				// 获得工作目录下所有项的详细信息
				DirItemInfo[] infos = service.ListWorkDir();

				// 将工作目录下的项先按目录在先，后按字典序排列
				infos = infos.OrderBy(info => info.IsDirectory ? 0 : 1)
					.ThenBy(info => info.Name)
					.ToArray();

				foreach (var info in infos)
				{
					// 创建该文件对应的列表项
					ListViewItem item = new()
					{
						Text = info.Name,
						Name = info.IsDirectory ? "dir" : "file", // 用Name区分文件和目录
					};

					// 两个SubItems为同一行的右边两栏，修改日期和大小
					if (info.IsDirectory)
					{
						item.SubItems.Add("<目录>");
						item.SubItems.Add("<目录>");
					}
					else
					{
						item.SubItems.Add(info.LastModified.ToString());
						item.SubItems.Add(GetSizeStr(info.Size));
					}

					// 向列表视图中添加该项
					lstWorkDir.Items.Add(item);
				}

				return null;
			}
			catch (Exception ex)
			{
				LogError(ex.Message);

				return ex;
			}
		}

		private void btnConnect_Click(object sender, EventArgs e)
		{
			try
			{
				service.host = txtHost.Text;
				service.user = txtUser.Text;
				service.pass = txtPass.Text;
				LogReply(service.Connect());
				DisplayDir();

				LogStatus($"已连接到 FTP 服务器：{service.host}");
			}
			catch (Exception ex)
			{
				LogError(ex.Message);
				LogStatus($"连接或验证失败");

				// 重置服务器地址和验证信息
				service.host = service.user = service.pass = string.Empty;
			}
		}

		private void btnUp_Click(object sender, EventArgs e)
		{
			ChangeDir((Path.GetDirectoryName(service.workDir.TrimEnd('/')) ?? "/"));
		}

		private void lstWorkDir_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			// 获得鼠标双击的项
			ListViewHitTestInfo hitInfo = lstWorkDir.HitTest(e.X, e.Y);
			ListViewItem? item = hitInfo.Item;

			if (e.Button == MouseButtons.Left)
			{
				if (item != null)
				{
					// 若该项是目录，则转到该目录下
					if (item.Name == "dir")
					{
						ChangeDir(service.workDir.TrimEnd('/') + "/" + item.Text + "/");
					}
				}
			}
		}

		// 工作路径输入框失去焦点事件
		private void txtWorkDir_Leave(object sender, EventArgs e)
		{
			ChangeDir(txtWorkDir.Text);
		}

		private void btnRefresh_Click(object sender, EventArgs e)
		{
			Exception? dispDirEx = DisplayDir();
			if (dispDirEx == null)
			{
				LogStatus($"刷新文件列表");
			}
			else
			{
				LogStatus($"刷新文件列表失败");
			}
		}

		// 本地路径输入框失去焦点事件
		private void txtLocalPath_Leave(object sender, EventArgs e)
		{
			try
			{
				if (string.IsNullOrEmpty(txtLocalPath.Text))
				{
					return;
				}

				// 将路径的写法规范化
				txtLocalPath.Text = txtLocalPath.Text.Trim().Replace('/', '\\').TrimEnd('\\') + "\\";
				
				// 检查路径是否存在
				if (txtLocalPath.Text == "\\" || !Path.Exists(txtLocalPath.Text))
				{
					throw new DirectoryNotFoundException();
				}
			}
			catch (Exception ex)
			{
				LogError(ex.Message);

				// 将文本框置为空
				txtLocalPath.Text = string.Empty;
			}
		}

		private void lstWorkDir_MouseClick(object sender, MouseEventArgs e)
		{
			// 获得鼠标单击的项
			ListViewHitTestInfo hitInfo = lstWorkDir.HitTest(e.X, e.Y);
			ListViewItem? item = hitInfo.Item;

			if (e.Button == MouseButtons.Right)
			{
				if (item != null)
				{
					// 判断是文件还是目录
					tsiDirList_Download.Visible = item.Name != "dir"; // 只能下载文件

					// 弹出右键菜单
					cmsDirList.Show(lstWorkDir.PointToScreen(e.Location));
				}
			}
		}

		private void txtWorkDir_KeyDown(object sender, KeyEventArgs e)
		{
			// 按下回车时，将控件焦点去掉，触发Leave事件
			if (e.KeyCode == Keys.Return)
			{
				ActiveControl = null;
			}
		}

		private void txtLocalPath_KeyDown(object sender, KeyEventArgs e)
		{
			// 按下回车时，将控件焦点去掉，触发Leave事件
			if (e.KeyCode == Keys.Return)
			{
				ActiveControl = null;
			}
		}
	}
}
