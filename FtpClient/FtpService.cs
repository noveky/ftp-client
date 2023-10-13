using System.Net;
using System;

namespace FtpClient
{
	// 文件或子目录的详细信息结构体
	public struct DirItemInfo
	{
		public string Name = string.Empty;
		public bool IsDirectory = false;
		public DateTime LastModified = DateTime.MinValue;
		public long Size = 0; // 以字节为单位

		public DirItemInfo() { }
	}

	// 文件传输任务类
	public class TransferTask
	{
		public readonly string Id;
		public Task? Task = null; // 传输线程
		public string FileName = string.Empty;
		public string LocalFile = string.Empty;
		public string RemoteFile = string.Empty;
		public bool IsUpload = true; // true：上传，false：下载
		public long BytesTransferred = 0;
		public double Progress = 0;

		private bool paused = false;
		private bool canceled = false;

		// 方便使用的get属性
		public string OperationName => IsUpload ? "上传" : "下载";
		public bool IsRunning => !Task?.IsCompleted ?? false;
		public bool IsPaused => !canceled && paused;
		public bool IsCanceled => canceled;
		public bool IsSucceeded => Task?.IsCompletedSuccessfully ?? false;
		public bool IsFaulted => Task?.IsFaulted ?? false;
		public bool CanBePaused => IsRunning;
		public bool CanBeUnpaused => IsPaused;
		public bool CanBeRetried => IsFaulted;
		public bool CanBeCanceled => !IsSucceeded && !IsCanceled;

		public TransferTask()
		{
			Id = Guid.NewGuid().ToString();
		}

		public void Pause()
		{
			if (!CanBePaused) return;

			paused = true;
			Task = null;
		}

		public void Unpause()
		{
			if (!CanBeUnpaused) return;

			paused = false;
		}

		public void Retry()
		{
			if (!CanBeRetried) return;

			paused = false;
		}

		public void Cancel()
		{
			if (!CanBeCanceled) return;

			canceled = true;
			Task = null;
		}
	}

	// FTP 服务
	public static class FtpService
	{
		public static string Host = ""; // 服务器IP地址
		public static string User = ""; // 登录用户名，为空则匿名
		public static string Pass = ""; // 登录密码

		public static string WorkDir = "/"; // 当前工作路径

		const int bufferSize = 1048576;

		public static readonly List<TransferTask> transferTasks = new();

		// 用事件的方式向用户界面输出应答、通知更新文件列表
		public static event Action<string?>? LogResponse;
		public static event Action<string?>? LogMessage;
		public static event Action? RefreshDirList;

		public static void Connect()
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{Host}");

			if (User != string.Empty)
			{
				request.Credentials = request.Credentials = new NetworkCredential(User, Pass);
			}
			request.Method = WebRequestMethods.Ftp.ListDirectory;

			using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
			{
				LogResponse?.Invoke(response.StatusDescription);
			}

			WorkDir = "/";
		}

		// 列出工作目录中所有项的详细信息
		public static DirItemInfo[] ListWorkDir()
		{
			return ListDir(WorkDir);
		}

		// 列出指定目录中所有项的详细信息
		public static DirItemInfo[] ListDir(string dir)
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{Host}{dir}");

			if (User != string.Empty)
			{
				request.Credentials = request.Credentials = new NetworkCredential(User, Pass);
			}
			request.Method = WebRequestMethods.Ftp.ListDirectory;

			// 获取子项列表字符串
			string listStr;

			using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
			{
				using StreamReader reader = new(response.GetResponseStream());
				listStr = reader.ReadToEnd();
			}

			// 将字符串按行存入数组，并去掉空项和每项两端的'\r'等无效字符
			string[] list = listStr.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			DirItemInfo[] infos = new DirItemInfo[list.Length];
			for (int i = 0; i < list.Length; ++i)
			{
				bool isDirectory = false;
				DateTime lastModified = DateTime.MinValue;
				long size = 0;

				string path = $"{dir}{list[i]}";

				// 获取修改日期和文件大小，并确定是否为目录
				try
				{
					lastModified = GetFileLastModified(path);
					size = GetFileSize(path);
				}
				catch (Exception)
				{
					if (DirectoryExists($"{path}/"))
					{
						isDirectory = true;
					}
				}

				infos[i] = new()
				{
					Name = list[i],
					IsDirectory = isDirectory,
					LastModified = lastModified,
					Size = size,
				};
			}

			return infos;
		}

		public static DateTime GetFileLastModified(string file)
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{Host}{file}");

			if (User != string.Empty)
			{
				request.Credentials = request.Credentials = new NetworkCredential(User, Pass);
			}
			request.Method = WebRequestMethods.Ftp.GetDateTimestamp;

			DateTime lastModified;

			using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
			{
				lastModified = response.LastModified;
			}

			return lastModified;
		}

		public static long GetFileSize(string file)
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{Host}{file}");

			if (User != string.Empty)
			{
				request.Credentials = request.Credentials = new NetworkCredential(User, Pass);
			}
			request.Method = WebRequestMethods.Ftp.GetFileSize;

			long fileSize;

			using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
			{
				fileSize = response.ContentLength;
			}

			return fileSize;
		}

		public static bool FileExists(string file)
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{Host}{file}");

			if (User != string.Empty)
			{
				request.Credentials = request.Credentials = new NetworkCredential(User, Pass);
			}
			request.Method = WebRequestMethods.Ftp.GetDateTimestamp;

			try
			{
				request.GetResponse();
			}
			catch
			{
				return false;
			}

			return true;
		}

		public static bool DirectoryExists(string dir)
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{Host}{dir}");

			if (User != string.Empty)
			{
				request.Credentials = request.Credentials = new NetworkCredential(User, Pass);
			}
			request.Method = WebRequestMethods.Ftp.ListDirectory;

			try
			{
				request.GetResponse();
			}
			catch
			{
				return false;
			}

			return true;
		}

		// 异步上传文件
		public static async Task UploadFile(string localFile, string remoteFile, TransferTask transferTask, bool allowBreakpointResume)
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{Host}{remoteFile}");

			if (User != string.Empty)
			{
				request.Credentials = request.Credentials = new NetworkCredential(User, Pass);
			}
			request.Method = WebRequestMethods.Ftp.UploadFile;

			using (FileStream fileStream = File.Open(localFile, FileMode.Open, FileAccess.Read))
			{
				long fileSize = fileStream.Length;
				long startPosition = 0;
				if (allowBreakpointResume)
				{
					//startPosition = FileExists(remoteFile) ? GetFileSize(remoteFile) : 0;
					startPosition = transferTask.BytesTransferred;
					if (startPosition > 0)
					{
						// 断点续传
						request.ContentOffset = startPosition;
						fileStream.Seek(startPosition, SeekOrigin.Begin);

						LogMessage?.Invoke($"{Path.GetFileName(remoteFile)} 开始断点续传 {FileUtility.GetSizeStr(startPosition)} / {FileUtility.GetSizeStr(fileSize)}");
					}
				}

				using Stream requestStream = request.GetRequestStream();
				//await fileStream.CopyToAsync(requestStream);

				// 分块上传
				byte[] buffer = new byte[bufferSize];
				long bytesSent = startPosition; // 断点续传按断点之前都已传输完成算
				int bytesToSend = await fileStream.ReadAsync(buffer.AsMemory(0, bufferSize));
				while (bytesToSend != 0)
				{
					await requestStream.WriteAsync(buffer.AsMemory(0, bytesToSend));

					bytesSent += bytesToSend;
					transferTask.BytesTransferred = bytesSent;
					transferTask.Progress = (double)bytesSent / fileSize;
					if (transferTask.IsPaused || transferTask.IsCanceled)
					{
						request.Abort();

						if (transferTask.IsCanceled)
						{
							DeleteFile(remoteFile);
							RefreshDirList?.Invoke();
						}

						return;
					}

					bytesToSend = await fileStream.ReadAsync(buffer.AsMemory(0, bufferSize));
				}
			}

			// 上传完成
			using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
			LogResponse?.Invoke(response.StatusDescription);
		}

		public static async Task DownloadFile(string remoteFile, string localFile, TransferTask transferTask, bool allowBreakpointResume)
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{Host}{remoteFile}");

			if (User != string.Empty)
			{
				request.Credentials = request.Credentials = new NetworkCredential(User, Pass);
			}
			request.Method = WebRequestMethods.Ftp.DownloadFile;

			using Stream fileStream = File.Open(localFile, FileMode.OpenOrCreate, FileAccess.Write);

			long fileSize = GetFileSize(remoteFile);
			long startPosition = 0;
			if (allowBreakpointResume)
			{
				//startPosition = File.Exists(localFile) ? new FileInfo(localFile).Length : 0;
				startPosition = transferTask.BytesTransferred;
				if (startPosition > 0)
				{
					// 断点续传
					request.ContentOffset = startPosition;
					fileStream.Seek(startPosition, SeekOrigin.Begin);

					LogMessage?.Invoke($"{Path.GetFileName(remoteFile)} 开始断点续传 {FileUtility.GetSizeStr(startPosition)} / {FileUtility.GetSizeStr(fileSize)}");
				}
			}

			using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
			using Stream responseStream = response.GetResponseStream();
			//await responseStream.CopyToAsync(fileStream);

			// 分块下载
			byte[] buffer = new byte[bufferSize];
			long bytesRead = startPosition; // 断点续传按断点之前都已传输完成算
			int bytesToRead = await responseStream.ReadAsync(buffer.AsMemory(0, bufferSize));
			while (bytesToRead != 0)
			{
				await fileStream.WriteAsync(buffer.AsMemory(0, bytesToRead));

				bytesRead += bytesToRead;
				transferTask.BytesTransferred = bytesRead;
				transferTask.Progress = (double)bytesRead / fileSize;
				if (transferTask.IsPaused || transferTask.IsCanceled)
				{
					request.Abort();

					if (transferTask.IsCanceled)
					{
						File.Delete(localFile);
					}

					return;
				}

				bytesToRead = await responseStream.ReadAsync(buffer.AsMemory(0, bufferSize));
			}

			// 下载完成
			LogResponse?.Invoke(response.StatusDescription);
		}

		public static void DeleteFile(string file)
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{Host}{file}");

			if (User != string.Empty)
			{
				request.Credentials = request.Credentials = new NetworkCredential(User, Pass);
			}
			request.Method = WebRequestMethods.Ftp.DeleteFile;

			using FtpWebResponse response = (FtpWebResponse)request.GetResponse();

			LogResponse?.Invoke(response.StatusDescription);
		}

		public static void RemoveDirectory(string dir)
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{Host}{dir}");

			if (User != string.Empty)
			{
				request.Credentials = request.Credentials = new NetworkCredential(User, Pass);
			}
			request.Method = WebRequestMethods.Ftp.RemoveDirectory;

			using FtpWebResponse response = (FtpWebResponse)request.GetResponse();

			LogResponse?.Invoke(response.StatusDescription);
		}
	}
}
