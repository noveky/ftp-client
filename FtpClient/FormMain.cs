using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyFtp3;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace FtpClient
{
	public partial class FormMain : Form
	{
		readonly FtpService service = new();

		public FormMain()
		{
			InitializeComponent();

			service.GotResponse += LogResponse;
			service.UpdatedTransferTasks += RefreshTransferList;
		}

		void Log(string message)
		{
			if (txtStatus.Text.Length != 0)
			{
				txtStatus.AppendText("\n");
			}
			txtStatus.AppendText($"[{DateTime.Now:HH:mm:ss}] {message.Trim()}");
			txtStatus.SelectionStart = txtStatus.Text.Length - 1;
			txtStatus.SelectionLength = 0;
			txtStatus.ScrollToCaret();
		}

		void LogResponse(string? statMsg)
		{
			Log($"应答：{statMsg}");
		}

		void LogStatus(string? statMsg)
		{
			Log($"状态：{statMsg}");
		}

		void LogError(string? errMsg)
		{
			Log($"错误：{errMsg}");
		}

		// 更改工作路径
		void ChangeDir(string newDir)
		{
			string lstDir = service.WorkDir; // 更新前的工作路径
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
					service.WorkDir = newDir;
					service.ListWorkDir(); // 测试一下新路径是否有效，否则抛出异常，不更新

					LogStatus($"转到路径 \"{service.WorkDir}\"");
				}

				// 获取并显示文件列表
				DisplayDirList(throwException: true);
			}
			catch (Exception ex)
			{
				LogError(ex.Message);
				LogStatus($"路径 \"{newDir}\" 无效或无法访问");

				// 回滚路径
				service.WorkDir = txtWorkDir.Text = lstDir;
				DisplayDirList();
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

		// 为确保新文件名不会重复，若原文件名已存在，则在后面加一个(1)，如果仍然存在则括号内数字递增到不存在为止
		string GetUniqueNameLocalFile(string localFile)
		{
			string fileName = Path.GetFileNameWithoutExtension(localFile);
			string extension = Path.GetExtension(localFile);
			string newLocalFile = localFile;
			int count = 1;
			while (File.Exists(newLocalFile))
			{
				newLocalFile = Path.Combine(Path.GetDirectoryName(localFile) ?? "", $"{fileName} ({count}){extension}");
				++count;
			}
			return newLocalFile;
		}

		// 获取并显示文件列表
		void DisplayDirList(bool throwException = false)
		{
			try
			{
				txtWorkDir.Text = service.WorkDir;

				lstWorkDir.BeginUpdate();

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

					// 添加修改日期和大小信息
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
			}
			catch (Exception ex)
			{
				LogError(ex.Message);

				if (throwException)
				{
					throw;
				}
			}
			finally
			{
				lstWorkDir.EndUpdate();
			}
		}

		// 刷新文件列表，输出状态信息
		void RefreshDirList()
		{
			try
			{
				DisplayDirList(throwException: true);

				LogStatus($"刷新文件列表");
			}
			catch
			{
				LogStatus($"刷新文件列表失败");
			}
		}

		// 刷新传输列表
		void RefreshTransferList()
		{
			try
			{
				// 记录一下刷新选中了哪些项，滚动到了什么位置
				HashSet<int> selectedIndexes = new();
				foreach (int index in lstTransfer.SelectedIndices)
				{
					selectedIndexes.Add(index);
				}
				int scrollPos = lstTransfer.TopItem?.Index ?? -1;

				lstTransfer.BeginUpdate();

				lstTransfer.Items.Clear();

				foreach (var transferTask in service.transferTasks)
				{
					// 创建该任务对应的列表项
					ListViewItem item = new()
					{
						Text = transferTask.FileName,
						Name = transferTask.IsUpload ? "upload" : "download", // 用Name区分上传和下载
					};

					// 添加状态信息
					if (transferTask.IsUpload)
					{
						item.SubItems.Add(
							transferTask.Task?.IsCompletedSuccessfully ?? false
							? "上传完成"
							: transferTask.Task?.IsCompleted ?? false
							? "上传失败"
							: $"上传中 {transferTask.Progress:0.00%}"
						);
					}
					else
					{
						item.SubItems.Add(
							transferTask.Task?.IsCompletedSuccessfully ?? false
							? "下载完成"
							: transferTask.Task?.IsCompleted ?? false
							? "下载失败"
							: $"下载中 {transferTask.Progress:0.00%}"
						);
					}

					// 向列表视图中添加该项
					lstTransfer.Items.Add(item);

					// 因为刷新了所有列表项，所以要重新选中原来选中的项
					item.Selected = selectedIndexes.Contains(item.Index);
				}
				// 因为刷新了所有列表项，所以要重新设置滚动状态
				if (scrollPos >= 0 && scrollPos < lstTransfer.Items.Count)
				{
					lstTransfer.TopItem = lstTransfer.Items[scrollPos];
					lstTransfer.EnsureVisible(scrollPos);
					lstTransfer.TopItem = lstTransfer.Items[scrollPos];
					lstTransfer.TopItem = lstTransfer.Items[scrollPos];
					lstTransfer.TopItem = lstTransfer.Items[scrollPos];
					lstTransfer.TopItem = lstTransfer.Items[scrollPos];
					lstTransfer.TopItem = lstTransfer.Items[scrollPos];
					lstTransfer.TopItem = lstTransfer.Items[scrollPos];
					lstTransfer.TopItem = lstTransfer.Items[scrollPos];
					lstTransfer.TopItem = lstTransfer.Items[scrollPos];
					lstTransfer.TopItem = lstTransfer.Items[scrollPos];
					lstTransfer.TopItem = lstTransfer.Items[scrollPos];
					lstTransfer.TopItem = lstTransfer.Items[scrollPos];
					lstTransfer.TopItem = lstTransfer.Items[scrollPos];
					lstTransfer.TopItem = lstTransfer.Items[scrollPos];
					lstTransfer.TopItem = lstTransfer.Items[scrollPos];
					lstTransfer.TopItem = lstTransfer.Items[scrollPos];
					lstTransfer.TopItem = lstTransfer.Items[scrollPos];
					lstTransfer.TopItem = lstTransfer.Items[scrollPos];
				}
			}
			finally
			{
				lstTransfer.EndUpdate();
			}
		}

		// 递归删除目录及所有子项
		void DeleteDirectoryWithAll(string dir)
		{
			string? name = Path.GetFileName(dir);
			if (name == "." || name == "..")
			{
				return;
			}

			// 遍历当前目录的一级子项
			DirItemInfo[] infos = service.ListDir(dir);
			foreach (var info in infos)
			{
				string path = $"{dir}{info.Name}";
				if (info.IsDirectory)
				{
					// 为目录，递归删除
					path += "/";
					DeleteDirectoryWithAll(path);
				}
				else
				{
					// 为文件，直接删除
					service.DeleteFile(path);

					LogStatus($"删除文件 \"{path}\"");
				}
			}

			// 移除空目录
			service.RemoveDirectory(dir);

			LogStatus($"移除空目录 \"{dir}\"");
		}

		public async Task UploadFile(string localFile, string remoteFile)
		{
			string fileName = Path.GetFileName(localFile);

			// 在传输列表中记录传输任务
			TransferTask transferTask = new()
			{
				FileName = Path.GetFileName(remoteFile),
				IsUpload = true,
			};
			service.transferTasks.Add(transferTask);
			RefreshTransferList();

			LogStatus($"开始上传 {fileName}");

			Task task = service.UploadFile(localFile, remoteFile, transferTask);
			transferTask.Task = task;
			try
			{
				await task;

				LogStatus($"{fileName} 上传完成");

				RefreshDirList();
			}
			catch (Exception ex)
			{
				LogError(ex.Message);

				LogStatus($"{fileName} 上传失败");
			}

			RefreshTransferList();
		}

		public async Task DownloadFile(string remoteFile, string localFile)
		{
			string fileName = Path.GetFileName(remoteFile);

			// 在传输列表中记录传输任务
			TransferTask transferTask = new()
			{
				FileName = Path.GetFileName(localFile),
				IsUpload = false,
			};
			service.transferTasks.Add(transferTask);
			RefreshTransferList();

			LogStatus($"开始下载 {fileName}");

			Task task = service.DownloadFile($"{service.WorkDir}{fileName}", localFile, transferTask);
			transferTask.Task = task;
			try
			{
				await task;

				LogStatus($"{fileName} 下载完成");

				RefreshDirList();
			}
			catch (Exception ex)
			{
				LogError(ex.Message);

				LogStatus($"{fileName} 下载失败");
			}

			RefreshTransferList();
		}

		private void btnConnect_Click(object sender, EventArgs e)
		{
			try
			{
				service.Host = txtHost.Text;
				service.User = txtUser.Text;
				service.Pass = txtPass.Text;
				service.Connect();
				DisplayDirList();

				LogStatus($"已连接到 FTP 服务器：{service.Host}");
			}
			catch (Exception ex)
			{
				LogError(ex.Message);
				LogStatus($"连接或验证失败");

				// 重置服务器地址和验证信息
				service.Host = service.User = service.Pass = string.Empty;
			}
		}

		private void btnUp_Click(object sender, EventArgs e)
		{
			ChangeDir((Path.GetDirectoryName(service.WorkDir.TrimEnd('/')) ?? "/"));
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
						ChangeDir(service.WorkDir.TrimEnd('/') + "/" + item.Text + "/");
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
			RefreshDirList();
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
			if (e.Button == MouseButtons.Right)
			{
				if (lstWorkDir.SelectedItems.Count != 0)
				{
					bool selDir = false; // 选择的项目中是否包含目录
					bool selFile = false; // 选择的项目中是否包含文件
					foreach (ListViewItem item in lstWorkDir.SelectedItems) {
						// 判断是文件还是目录
						if (item.Name == "dir")
						{
							selDir |= true;
						}
						else
						{
							selFile |= true;
						}

						tsiDirList_Download.Enabled = selFile && !selDir; // 只能下载文件
						tsiDirList_Rename.Enabled = lstWorkDir.SelectedItems.Count == 1; // 只能重命名单个项目
						tsiDirList_Delete.Enabled = selFile || selDir;

					}

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

		private async void btnUpload_Click(object sender, EventArgs e)
		{
			try
			{
				OpenFileDialog dialog = new()
				{
					Multiselect = true,
					Title = "上传文件",
					InitialDirectory = txtLocalPath.Text,
				};
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					List<Task> tasks = new();

					string[] localFiles = dialog.FileNames;
					foreach (string localFile in localFiles)
					{
						string fileName = Path.GetFileName(localFile);
						string targetFile = $"{service.WorkDir}{fileName}";
						if (service.FileExists(targetFile))
						{
							if (MessageBox.Show($"\"{targetFile}\" 已存在。是否覆盖？", "确认上传", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
							{
								continue;
							}
						}

						// 添加异步任务
						tasks.Add(UploadFile(localFile, targetFile));
					}

					// 同时开始上传
					await Task.WhenAll(tasks);
				}
			}
			catch (Exception ex)
			{
				LogError(ex.Message);

				RefreshTransferList();
			}
		}

		private async void tsiDirList_Download_Click(object sender, EventArgs e)
		{
			try
			{
				List<Task> tasks = new();

				foreach (ListViewItem item in lstWorkDir.SelectedItems)
				{
					string fileName = item.Text;
					string targetLocalFile = Path.Combine(txtLocalPath.Text, fileName);
					if (File.Exists(targetLocalFile))
					{
						string newLocalFile = GetUniqueNameLocalFile(targetLocalFile);

						LogStatus($"\"{targetLocalFile}\" 已存在，下载到新文件名 {Path.GetFileName(newLocalFile)}");

						targetLocalFile = newLocalFile;
					}

					// 添加异步任务
					tasks.Add(DownloadFile($"{service.WorkDir}{fileName}", targetLocalFile));
				}

				// 同时开始下载
				await Task.WhenAll(tasks);
			}
			catch (Exception ex)
			{
				LogError(ex.Message);

				RefreshTransferList();
			}
		}

		private void tsiDirList_Delete_Click(object sender, EventArgs e)
		{
			try
			{
				int selDirCnt = 0; // 选择的项目中包含目录的个数
				int selFileCnt = 0; // 选择的项目中包含文件的个数
				foreach (ListViewItem item in lstWorkDir.SelectedItems)
				{
					if (item.Name == "dir")
					{
						++selDirCnt;
					}
					else
					{
						++selFileCnt;
					}
				}

				// 弹出确认对话框
				if (MessageBox.Show(
					selDirCnt != 0 && selFileCnt != 0
					? $"确认要永久删除这 {selFileCnt} 个文件、{selDirCnt} 个目录及目录中所有子项吗？" 
					: selDirCnt != 0
					? $"确认要永久删除这 {selDirCnt} 个目录及目录中所有子项吗？"
					: $"确认要永久删除这 {selFileCnt} 个文件吗？"
					, "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
				{
					return;
				}

				try
				{
					foreach (ListViewItem item in lstWorkDir.SelectedItems)
					{
						string name = item.Text;
						string path = $"{service.WorkDir}{name}";
						// 根据项目是文件还是目录，使用不同的删除方式
						if (item.Name == "dir")
						{
							path += "/";

							DeleteDirectoryWithAll(path);

							LogStatus($"目录 \"{path}\" 删除完成");
						}
						else
						{
							service.DeleteFile(path);

							LogStatus($"删除文件 \"{path}\"");
						}
					}
				}
				catch (Exception ex)
				{
					LogError(ex.Message);

					LogStatus("删除失败");
				}
				finally
				{
					RefreshDirList();
				}
			}
			catch (Exception ex)
			{
				LogError(ex.Message);
			}
		}

		private void tsiStatus_Clear_Click(object sender, EventArgs e)
		{
			txtStatus.Clear();
		}

		private void btnMkDir_Click(object sender, EventArgs e)
		{
			
		}

		private void tmrRefreshTransfer_Tick(object sender, EventArgs e)
		{
			RefreshTransferList();
		}
	}
}
