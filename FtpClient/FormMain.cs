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
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace FtpClient
{
	public partial class FormMain : Form
	{
		string LocalPath
		{
			get => txtLocalPath.Text;

			set
			{
				try
				{
					if (string.IsNullOrEmpty(value)) return;

					// 将路径的写法规范化
					value = value.Trim().Replace('/', '\\').TrimEnd('\\') + "\\";

					// 检查路径是否存在
					if (value == "\\" || !Path.Exists(value))
					{
						throw new DirectoryNotFoundException();
					}

					txtLocalPath.Text = value;
				}
				catch (Exception ex)
				{
					LogError(ex.Message);

					// 将文本框置为空
					txtLocalPath.Text = string.Empty;
				}
			}
		}

		public FormMain()
		{
			InitializeComponent();

			FtpService.GotResponse += LogResponse;
			FtpService.LogInfo += LogInfo;
			FtpService.UpdatedTransferTasks += RefreshTransferList;
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

		void LogInfo(string? statMsg)
		{
			Log($"信息：{statMsg}");
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
			string lstDir = FtpService.WorkDir; // 更新前的工作路径
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
					FtpService.WorkDir = newDir;
					FtpService.ListWorkDir(); // 测试一下新路径是否有效，否则抛出异常，不更新

					LogStatus($"转到路径 \"{FtpService.WorkDir}\"");
				}

				// 获取并显示文件列表
				DisplayDirList(throwException: true);
			}
			catch (Exception ex)
			{
				LogError(ex.Message);
				LogStatus($"路径 \"{newDir}\" 无效或无法访问");

				// 回滚路径
				FtpService.WorkDir = txtWorkDir.Text = lstDir;
				DisplayDirList();
			}
		}

		// 获取并显示文件列表
		void DisplayDirList(bool throwException = false)
		{
			try
			{
				txtWorkDir.Text = FtpService.WorkDir;

				lstWorkDir.BeginUpdate();

				lstWorkDir.Items.Clear();

				// 获得工作目录下所有项的详细信息
				DirItemInfo[] infos = FtpService.ListWorkDir();

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
						item.SubItems.Add(FileSystem.GetSizeStr(info.Size));
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
				int scrollTopIndex = lstTransfer.TopItem?.Index ?? -1;

				lstTransfer.BeginUpdate();

				lstTransfer.Items.Clear();

				foreach (var transferTask in FtpService.transferTasks)
				{
					// 创建该任务对应的列表项
					ListViewItem item = new()
					{
						Text = transferTask.FileName,
						Name = transferTask.Id,
					};

					// 添加状态信息
					string opName = transferTask.OperationName;
					item.SubItems.Add(
						transferTask.IsSucceeded
						? $"{opName}完成"
						: transferTask.IsFaulted
						? $"{opName}失败"
						: transferTask.IsRunning
						? $"{opName}中 {transferTask.Progress:0.00%}"
						: transferTask.IsPaused
						? $"暂停{opName}"
						: transferTask.IsCanceled
						? $"取消{opName}"
						: ""
					);

					// 向列表视图中添加该项
					lstTransfer.Items.Add(item);

					// 因为刷新了所有列表项，所以要重新选中原来选中的项
					item.Selected = selectedIndexes.Contains(item.Index);
				}
				// 因为刷新了所有列表项，所以要重新设置滚动状态
				if (scrollTopIndex >= 0 && scrollTopIndex < lstTransfer.Items.Count)
				{
					lstTransfer.TopItem = lstTransfer.Items[scrollTopIndex];
					int scrollBottomIndex = scrollTopIndex + lstTransfer.Height / lstTransfer.GetItemRect(scrollTopIndex).Height - 2;
					for (int i = 2; i-- > 0;)
					{
						lstTransfer.EnsureVisible(scrollTopIndex);
						if (scrollBottomIndex >= 0 && scrollBottomIndex < lstTransfer.Items.Count)
						{
							lstTransfer.EnsureVisible(scrollBottomIndex);
						}
					}
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
			if (name == "." || name == "..") return;

			// 遍历当前目录的一级子项
			DirItemInfo[] infos = FtpService.ListDir(dir);
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
					FtpService.DeleteFile(path);

					LogStatus($"删除文件 \"{path}\"");
				}
			}

			// 移除空目录
			FtpService.RemoveDirectory(dir);

			LogStatus($"移除空目录 \"{dir}\"");
		}
		
		async Task UploadFile(string localFile, string remoteFile, TransferTask? transferTask = null)
		{
			string fileName = Path.GetFileName(remoteFile);

			bool allowBreakpointResume = transferTask != null;

			// 在传输列表中记录传输任务
			if (transferTask == null)
			{
				transferTask = new()
				{
					FileName = fileName,
					IsUpload = true,
					LocalFile = localFile,
					RemoteFile = remoteFile,
				};
				FtpService.transferTasks.Add(transferTask);
			}

			LogStatus($"开始上传 {fileName}");

			Task task = FtpService.UploadFile(localFile, remoteFile, transferTask, allowBreakpointResume);
			transferTask.Task = task;
			RefreshTransferList();
			try
			{
				await task;

				if (transferTask.IsSucceeded)
				{
					LogStatus($"{fileName} 上传完成");
				}

				RefreshDirList();
			}
			catch (Exception ex)
			{
				LogError(ex.Message);

				LogStatus($"{fileName} 上传失败");
			}

			RefreshTransferList();
		}

		async Task DownloadFile(string remoteFile, string localFile, TransferTask? transferTask = null)
		{
			if (File.Exists(localFile))
			{
				string newLocalFile = FileSystem.GetUniqueNameLocalFile(localFile);

				LogStatus($"\"{localFile}\" 已存在，下载到新文件名 {Path.GetFileName(newLocalFile)}");

				localFile = newLocalFile;
			}

			string fileName = Path.GetFileName(localFile);

			bool allowBreakpointResume = transferTask != null;

			// 在传输列表中记录传输任务
			if (transferTask == null)
			{
				transferTask = new()
				{
					FileName = fileName,
					IsUpload = false,
					LocalFile = localFile,
					RemoteFile = remoteFile,
				};
				FtpService.transferTasks.Add(transferTask);
			}

			LogStatus($"开始下载 {fileName}");

			Task task = FtpService.DownloadFile(remoteFile, localFile, transferTask, allowBreakpointResume);
			transferTask.Task = task;
			RefreshTransferList();
			try
			{
				await task;

				if (transferTask.IsSucceeded)
				{
					LogStatus($"{fileName} 下载完成");
				}
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
				FtpService.Host = txtHost.Text;
				FtpService.User = txtUser.Text;
				FtpService.Pass = txtPass.Text;
				FtpService.Connect();
				DisplayDirList();

				LogStatus($"已连接到 FTP 服务器：{FtpService.Host}");
			}
			catch (Exception ex)
			{
				LogError(ex.Message);
				LogStatus($"连接或验证失败");

				// 重置服务器地址和验证信息
				FtpService.Host = FtpService.User = FtpService.Pass = string.Empty;
			}
		}

		private void btnUp_Click(object sender, EventArgs e)
		{
			ChangeDir((Path.GetDirectoryName(FtpService.WorkDir.TrimEnd('/')) ?? "/"));
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
					// 若该项是目录，则转到该目录下；若该项是文件，则下载该文件
					if (item.Name == "dir")
					{
						ChangeDir(FtpService.WorkDir.TrimEnd('/') + "/" + item.Text + "/");
					}
					else
					{
						DownloadDirItem(item);
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
			LocalPath = txtLocalPath.Text;
		}

		private void lstWorkDir_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				if (lstWorkDir.SelectedItems.Count != 0)
				{
					bool selDir = false; // 选择的项目中是否包含目录
					bool selFile = false; // 选择的项目中是否包含文件

					foreach (ListViewItem item in lstWorkDir.SelectedItems)
					{
						// 判断是文件还是目录
						if (item.Name == "dir")
						{
							selDir |= true;
						}
						else
						{
							selFile |= true;
						}
					}

					tsiDirList_Download.Enabled = selFile && !selDir; // 只能下载文件
					tsiDirList_Rename.Enabled = lstWorkDir.SelectedItems.Count == 1; // 只能重命名单个项目
					tsiDirList_Delete.Enabled = selFile || selDir;

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
					InitialDirectory = LocalPath,
				};
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					List<Task> tasks = new();

					string[] localFiles = dialog.FileNames;
					foreach (string localFile in localFiles)
					{
						string fileName = Path.GetFileName(localFile);
						string targetFile = $"{FtpService.WorkDir}{fileName}";
						if (FtpService.FileExists(targetFile))
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

		void DownloadDirItem(ListViewItem item)
		{
			string fileName = item.Text;
			string targetLocalFile = Path.Combine(LocalPath, fileName);
			Invoke(async () => await DownloadFile($"{FtpService.WorkDir}{fileName}", targetLocalFile));
		}

		private void tsiDirList_Download_Click(object sender, EventArgs e)
		{
			try
			{
				List<Task> tasks = new();

				foreach (ListViewItem item in lstWorkDir.SelectedItems)
				{
					DownloadDirItem(item);
				}
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
						string path = $"{FtpService.WorkDir}{name}";
						// 根据项目是文件还是目录，使用不同的删除方式
						if (item.Name == "dir")
						{
							path += "/";

							DeleteDirectoryWithAll(path);

							LogStatus($"目录 \"{path}\" 删除完成");
						}
						else
						{
							FtpService.DeleteFile(path);

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
			if (FtpService.transferTasks.Any(t => t.IsRunning))
			{
				RefreshTransferList();
			}
		}

		List<TransferTask> SelectedTransferTasks
		{
			get
			{
				List<TransferTask> sel = new();
				foreach (ListViewItem item in lstTransfer.SelectedItems)
				{
					foreach (var tTask in FtpService.transferTasks)
					{
						if (tTask.Id == item.Name)
						{
							sel.Add(tTask);
							break;
						}
					}
				}
				return sel;
			}
		}

		private void lstTransfer_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				bool canPause, canUnpause, canRetry, canCancel;
				canPause = canUnpause = canRetry = canCancel = SelectedTransferTasks.Count != 0;

				foreach (var tTask in SelectedTransferTasks)
				{
					canPause &= tTask.CanBePaused;
					canUnpause &= tTask.CanBeUnpaused;
					canRetry &= tTask.CanBeRetried;
					canCancel &= tTask.CanBeCanceled;
				}

				tsiTransferList_Pause.Visible = canPause;
				tsiTransferList_Unpause.Visible = canUnpause;
				tsiTransferList_Retry.Visible = canRetry;
				tsiTransferList_Cancel.Visible = canCancel;

				// 弹出右键菜单
				cmsTransferList.Show(lstTransfer.PointToScreen(e.Location));
			}
		}

		private void btnOpenLocalPath_Click(object sender, EventArgs e)
		{
			try
			{
				if (!Directory.Exists(LocalPath))
				{
					throw new DirectoryNotFoundException();
				}
				Process proc = new()
				{
					StartInfo = new("explorer.exe")
					{
						Arguments = $"/root, {LocalPath}",
					},
				};
				proc.Start();
			}
			catch (Exception ex)
			{
				LogError(ex.Message);
			}
		}

		private void txtLocalPath_DoubleClick(object sender, EventArgs e)
		{
			// 去掉控件焦点
			txtLocalPath.SelectionLength = 0;
			ActiveControl = null;

			// 弹出选择文件夹对话框
			FolderBrowserDialog dialog = new()
			{
				Description = "设置本地默认路径",
				UseDescriptionForTitle = true,
				InitialDirectory = LocalPath,
			};
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				LocalPath = dialog.SelectedPath;
			}
		}

		private void tsiTransferList_Pause_Click(object sender, EventArgs e)
		{
			SelectedTransferTasks.ForEach(tTask => tTask.Pause());
		}

		async Task ResumeTransferTask(TransferTask transferTask)
		{
			Task task = transferTask.IsUpload
				? UploadFile(transferTask.LocalFile, transferTask.RemoteFile, transferTask)
				: DownloadFile(transferTask.RemoteFile, transferTask.LocalFile, transferTask);
			await task;
		}

		private void tsiTransferList_Unpause_Click(object sender, EventArgs e)
		{
			SelectedTransferTasks.ForEach(async tTask =>
			{
				tTask.Unpause();
				await ResumeTransferTask(tTask);
			});
		}

		private void tsiTransferList_Retry_Click(object sender, EventArgs e)
		{
			SelectedTransferTasks.ForEach(async tTask =>
			{
				tTask.Retry();
				await ResumeTransferTask(tTask);
			});
		}

		private void tsiTransferList_Cancel_Click(object sender, EventArgs e)
		{
			SelectedTransferTasks.ForEach(tTask =>
			{
				tTask.Cancel();

				try
				{
					if (tTask.IsUpload)
					{
						FtpService.DeleteFile(tTask.RemoteFile);
					}
					else
					{
						File.Delete(tTask.LocalFile);
					}
				}
				catch (Exception ex)
				{
					LogError(ex.Message);
				}

				LogStatus($"取消{tTask.OperationName} {tTask.FileName}");

				if (tTask.IsUpload)
				{
					RefreshDirList();
				}
				RefreshTransferList();
			});
		}

		private void tsiTransferList_RemoveInactive_Click(object sender, EventArgs e)
		{
			List<TransferTask> tTasksToRemove = new();
			FtpService.transferTasks.ForEach(tTask =>
			{
				if (tTask.IsSucceeded || tTask.IsCanceled)
				{
					tTasksToRemove.Add(tTask);
				}
			});
			tTasksToRemove.ForEach(tTask => FtpService.transferTasks.Remove(tTask));

			RefreshTransferList();
		}
	}
}
