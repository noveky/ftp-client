using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using Microsoft.VisualBasic.Logging;
using System.Xml.Linq;
using System.Runtime.CompilerServices;

namespace MyFtp3
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
		public Task? Task = null;
		public string FileName = string.Empty;
		public bool IsUpload = true; // true：上传，false：下载
		public bool HasCompleted = false;
		public double Progress = 0;

		public TransferTask() { }
	}

	// FTP 服务
	public class FtpService
	{
		public string Host = ""; // 服务器IP地址
		public string User = ""; // 登录用户名，为空则匿名
		public string Pass = ""; // 登录密码

		public string WorkDir = "/"; // 当前工作路径

		const int bufferSize = 1048576;

		public readonly List<TransferTask> transferTasks = new();

		// 用事件的方式向用户界面输出应答、通知更新传输列表
		public event Action<string?> GotResponse;
		public event Action UpdatedTransferTasks;

		public FtpService()
		{
			GotResponse += _ => { };
			UpdatedTransferTasks += () => { };
		}

		public void Connect()
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{Host}");

			if (User != string.Empty)
			{
				request.Credentials = request.Credentials = new NetworkCredential(User, Pass);
			}
			request.Method = WebRequestMethods.Ftp.ListDirectory;

			using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
			{
				GotResponse(response.StatusDescription);
			}

			WorkDir = "/";
		}

		// 列出工作路径中所有项的详细信息
		public DirItemInfo[] ListWorkDir()
		{
			return ListDir(WorkDir);
		}

		// 列出指定路径中所有项的详细信息
		public DirItemInfo[] ListDir(string dir)
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

				// 获取修改日期和文件大小，如果抛出异常说明不是文件而是目录
				try
				{
					lastModified = GetFileLastModified($"{dir}{list[i]}");
					size = GetFileSize($"{dir}{list[i]}");
				}
				catch (Exception)
				{
					isDirectory = true;
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

		public DateTime GetFileLastModified(string file)
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

		public long GetFileSize(string file)
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

		public bool FileExists(string file)
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{Host}{file}");

			if (User != string.Empty)
			{
				request.Credentials = request.Credentials = new NetworkCredential(User, Pass);
			}
			request.Method = WebRequestMethods.Ftp.GetFileSize;

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
		public async Task UploadFile(string localFile, string remoteFile, TransferTask transferTask)
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{Host}{remoteFile}");

			if (User != string.Empty)
			{
				request.Credentials = request.Credentials = new NetworkCredential(User, Pass);
			}
			request.Method = WebRequestMethods.Ftp.UploadFile;

			// 用文件流将本地文件上传到服务器
			using (FileStream fileStream = File.Open(localFile, FileMode.Open, FileAccess.Read))
			{
				using Stream requestStream = request.GetRequestStream();
				//await fileStream.CopyToAsync(requestStream);

				long fileSize = fileStream.Length;
				byte[] buffer = new byte[bufferSize];
				long bytesSent = 0;
				int bytesToSend = await fileStream.ReadAsync(buffer, 0, bufferSize);
				while (bytesToSend != 0)
				{
					await requestStream.WriteAsync(buffer.AsMemory(0, bytesToSend));
					bytesSent += bytesToSend;
					transferTask.Progress = (double)bytesSent / fileSize;
					//UpdatedTransferTasks();
					bytesToSend = await fileStream.ReadAsync(buffer, 0, bufferSize);
				}
			}

			// 上传完成
			using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
			GotResponse(response.StatusDescription);
		}

		public async Task DownloadFile(string remoteFile, string localFile, TransferTask transferTask)
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{Host}{remoteFile}");

			if (User != string.Empty)
			{
				request.Credentials = request.Credentials = new NetworkCredential(User, Pass);
			}
			request.Method = WebRequestMethods.Ftp.DownloadFile;

			using FtpWebResponse response = (FtpWebResponse)request.GetResponse();

			// 用文件流将服务端文件下载到本地
			using Stream fileStream = File.Open(localFile, FileMode.OpenOrCreate, FileAccess.Write);
			using Stream responseStream = response.GetResponseStream();
			//await responseStream.CopyToAsync(fileStream);

			long fileSize = GetFileSize(remoteFile);
			byte[] buffer = new byte[bufferSize];
			long bytesRead = 0;
			int bytesToRead = await responseStream.ReadAsync(buffer, 0, bufferSize);
			while (bytesToRead != 0)
			{
				await fileStream.WriteAsync(buffer.AsMemory(0, bytesToRead));
				bytesRead += bytesToRead;
				transferTask.Progress = (double)bytesRead / fileSize;
				//UpdatedTransferTasks();
				bytesToRead = await responseStream.ReadAsync(buffer, 0, bufferSize);
			}

			// 下载完成
			GotResponse(response.StatusDescription);
		}

		public void DeleteFile(string file)
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{Host}{file}");

			if (User != string.Empty)
			{
				request.Credentials = request.Credentials = new NetworkCredential(User, Pass);
			}
			request.Method = WebRequestMethods.Ftp.DeleteFile;

			using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
			
			GotResponse(response.StatusDescription);
		}

		public void RemoveDirectory(string dir)
		{
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{Host}{dir}");

			if (User != string.Empty)
			{
				request.Credentials = request.Credentials = new NetworkCredential(User, Pass);
			}
			request.Method = WebRequestMethods.Ftp.RemoveDirectory;

			using FtpWebResponse response = (FtpWebResponse)request.GetResponse();

			GotResponse(response.StatusDescription);
		}
	}
}

namespace MyFtp2
{
	/// <summary>
	/// FTP Client(不允许匿名登录)
	/// 上传、下载已测试，其余未测试
	/// </summary>
	public class FtpService
	{
		#region 构造函数
		/// <summary>
		/// 缺省构造函数
		/// </summary>
		public FtpService()
		{
			this.ftpServerIP = "";
			this.remoteFilePath = "";
			this.ftpUserID = "";
			this.ftpPassword = "";
			this.ftpServerPort = 21;
			this.bConnected = false;
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="remoteHost">FTP服务器IP地址</param>
		/// <param name="remotePath">当前服务器目录</param>
		/// <param name="remoteUser">Ftp 服务器登录用户账号</param>
		/// <param name="remotePass">Ftp 服务器登录用户密码</param>
		/// <param name="remotePort">FTP服务器端口</param>
		public FtpService(string ftpServerIP, string remoteFilePath, string ftpUserID, string ftpPassword, int ftpServerPort, bool anonymousAccess = false)
		{
			this.ftpServerIP = ftpServerIP;
			this.remoteFilePath = remoteFilePath;
			this.ftpUserID = ftpUserID;
			this.ftpPassword = ftpPassword;
			this.ftpServerPort = ftpServerPort;
			this.Connect();
		}
		#endregion

		#region 登录字段、属性
		/// <summary>
		/// FTP服务器IP地址
		/// </summary>
		private string ftpServerIP;
		public string FtpServerIP
		{
			get
			{
				return ftpServerIP;
			}
			set
			{
				this.ftpServerIP = value;
			}
		}
		/// <summary>
		/// FTP服务器端口
		/// </summary>
		private int ftpServerPort;
		public int FtpServerPort
		{
			get
			{
				return ftpServerPort;
			}
			set
			{
				this.ftpServerPort = value;
			}
		}
		/// <summary>
		/// 当前服务器目录
		/// </summary>
		private string remoteFilePath;
		public string RemoteFilePath
		{
			get
			{
				return remoteFilePath;
			}
			set
			{
				this.remoteFilePath = value;
			}
		}
		/// <summary>
		/// Ftp 服务器登录用户账号
		/// </summary>
		private string ftpUserID;
		public string FtpUserID
		{
			set
			{
				this.ftpUserID = value;
			}
		}
		/// <summary>
		/// Ftp 服务器用户登录密码
		/// </summary>
		private string ftpPassword;
		public string FtpPassword
		{
			set
			{
				this.ftpPassword = value;
			}
		}

		/// <summary>
		/// 是否登录
		/// </summary>
		private bool bConnected;
		public bool Connected
		{
			get
			{
				return this.bConnected;
			}
		}
		#endregion

		#region 连接
		/// <summary>
		/// 建立连接 
		/// </summary>
		public void Connect()
		{
			socketControl = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPEndPoint ep = new IPEndPoint(IPAddress.Parse(FtpServerIP), ftpServerPort);
			// 连接
			try
			{
				socketControl.Connect(ep);
			}
			catch (Exception)
			{
				throw new IOException("Couldn't connect to remote server");
			}

			// 获取应答码
			ReadReply();
			if (iReplyCode != 220)
			{
				Disconnect();
				throw new IOException(strReply.Substring(4));
			}

			// 登录
			SendCommand("USER " + ftpUserID);
			if (!(iReplyCode == 331 || iReplyCode == 230))
			{
				CloseSocketConnect();//关闭连接
				throw new IOException(strReply.Substring(4));
			}
			if (iReplyCode != 230)
			{
				SendCommand("PASS " + ftpPassword);
				if (!(iReplyCode == 230 || iReplyCode == 202))
				{
					CloseSocketConnect();//关闭连接
					throw new IOException(strReply.Substring(4));
				}
			}

			bConnected = true;

			// 切换到初始目录
			if (!string.IsNullOrEmpty(remoteFilePath))
			{
				ChDir(remoteFilePath);
			}
		}


		/// <summary>
		/// 关闭连接
		/// </summary>
		public void Disconnect()
		{
			if (socketControl != null)
			{
				SendCommand("QUIT");
			}
			CloseSocketConnect();
		}

		#endregion

		#region 传输模式

		/// <summary>
		/// 传输模式:二进制类型、ASCII类型
		/// </summary>
		public enum TransferType
		{
			Binary,
			ASCII
		};

		/// <summary>
		/// 设置传输模式
		/// </summary>
		/// <param name="ttType">传输模式</param>
		public void SetTransferType(TransferType ttType)
		{
			if (ttType == TransferType.Binary)
			{
				SendCommand("TYPE I");//binary类型传输
			}
			else
			{
				SendCommand("TYPE A");//ASCII类型传输
			}
			if (iReplyCode != 200)
			{
				throw new IOException(strReply.Substring(4));
			}
			else
			{
				trType = ttType;
			}
		}


		/// <summary>
		/// 获得传输模式
		/// </summary>
		/// <returns>传输模式</returns>
		public TransferType GetTransferType()
		{
			return trType;
		}

		#endregion

		#region 文件操作
		/// <summary>
		/// 获得文件列表
		/// </summary>
		/// <param name="strMask">文件名的匹配字符串</param>
		/// <returns></returns>
		public string[] Dir(string strMask)
		{
			// 建立链接
			if (!bConnected)
			{
				Connect();
			}

			//建立进行数据连接的socket
			Socket socketData = CreateDataSocket();

			//传送命令
			SendCommand("LIST " + strMask);

			//分析应答代码
			if (!(iReplyCode == 150 || iReplyCode == 125 || iReplyCode == 226))
			{
				throw new IOException(strReply.Substring(4));
			}

			//获得结果
			strMsg = "";
			while (true)
			{
				int iBytes = socketData.Receive(buffer, buffer.Length, 0);
				strMsg += GB2312.GetString(buffer, 0, iBytes);
				if (iBytes < buffer.Length)
				{
					break;
				}
			}
			char[] seperator = { '\n' };
			string[] strsFileList = strMsg.Split(seperator);
			socketData.Close();//数据socket关闭时也会有返回码
			if (iReplyCode != 226)
			{
				ReadReply();
				if (iReplyCode != 226)
				{
					throw new IOException(strReply.Substring(4));
				}
			}
			return strsFileList;
		}


		/// <summary>
		/// 获取文件大小
		/// </summary>
		/// <param name="strFileName">文件名</param>
		/// <returns>文件大小</returns>
		public long GetFileSize(string strFileName)
		{
			if (!bConnected)
			{
				Connect();
			}
			SendCommand("SIZE " + Path.GetFileName(strFileName));
			long lSize = 0;
			if (iReplyCode == 213)
			{
				lSize = Int64.Parse(strReply.Substring(4));
			}
			else
			{
				throw new IOException(strReply.Substring(4));
			}
			return lSize;
		}


		/// <summary>
		/// 删除
		/// </summary>
		/// <param name="strFileName">待删除文件名</param>
		public void Delete(string strFileName)
		{
			if (!bConnected)
			{
				Connect();
			}
			SendCommand("DELE " + strFileName);
			if (iReplyCode != 250)
			{
				throw new IOException(strReply.Substring(4));
			}
		}


		/// <summary>
		/// 重命名(如果新文件名与已有文件重名,将覆盖已有文件)
		/// </summary>
		/// <param name="strOldFileName">旧文件名</param>
		/// <param name="strNewFileName">新文件名</param>
		public void Rename(string strOldFileName, string strNewFileName)
		{
			if (!bConnected)
			{
				Connect();
			}
			SendCommand("RNFR " + strOldFileName);
			if (iReplyCode != 350)
			{
				throw new IOException(strReply.Substring(4));
			}
			//  如果新文件名与原有文件重名,将覆盖原有文件
			SendCommand("RNTO " + strNewFileName);
			if (iReplyCode != 250)
			{
				throw new IOException(strReply.Substring(4));
			}
		}
		#endregion

		#region 上传和下载
		/// <summary>
		/// 下载一批文件
		/// </summary>
		/// <param name="strFileNameMask">文件名的匹配字符串</param>
		/// <param name="strFolder">本地目录(不得以\结束)</param>
		public void Download(string strFileNameMask, string strFolder)
		{
			if (!bConnected)
			{
				Connect();
			}
			string[] strFiles = Dir(strFileNameMask);
			foreach (string strFile in strFiles)
			{
				if (!strFile.Equals(""))//一般来说strFiles的最后一个元素可能是空字符串
				{
					if (strFile.LastIndexOf(".") > -1)
					{
						Download(strFile.Replace("\r", ""), strFolder, strFile.Replace("\r", ""));
					}
				}
			}
		}

		/// <summary>
		/// 下载目录
		/// </summary>
		/// <param name="strRemoteFileName">要下载的文件名</param>
		/// <param name="strFolder">本地目录(不得以\结束)</param>
		/// <param name="strLocalFileName">保存在本地时的文件名</param>
		public void Download(string strRemoteFileName, string strFolder, string strLocalFileName)
		{
			if (strLocalFileName.StartsWith("-r"))
			{
				string[] infos = strLocalFileName.Split(' ');
				strRemoteFileName = strLocalFileName = infos[infos.Length - 1];
				if (!this.bConnected)
				{
					this.Connect();
				}
				SetTransferType(TransferType.Binary);
				if (strLocalFileName.Equals(""))
				{
					strLocalFileName = strRemoteFileName;
				}
				if (!File.Exists(strLocalFileName))
				{
					Stream st = File.Create(strLocalFileName);
					st.Close();
				}
				FileStream output = new
					FileStream(strFolder + "\\" + strLocalFileName, FileMode.Create);
				Socket socketData = CreateDataSocket();
				SendCommand("RETR " + strRemoteFileName);
				if (!(iReplyCode == 150 || iReplyCode == 125
				|| iReplyCode == 226 || iReplyCode == 250))
				{
					throw new IOException(strReply.Substring(4));
				}
				int receiveBytes = 0;
				while (true)
				{
					int iBytes = socketData.Receive(buffer, buffer.Length, 0);
					receiveBytes = receiveBytes + iBytes;
					output.Write(buffer, 0, iBytes);
					if (iBytes <= 0)
					{
						break;
					}
				}
				output.Close();
				if (socketData.Connected)
				{
					socketData.Close();
				}
				if (!(iReplyCode == 226 || iReplyCode == 250))
				{
					ReadReply();
					if (!(iReplyCode == 226 || iReplyCode == 250))
					{
						throw new IOException(strReply.Substring(4));
					}
				}
			}
		}

		/// <summary>
		/// 下载一个文件
		/// </summary>
		/// <param name="strRemoteFileName">要下载的文件名</param>
		/// <param name="strFolder">本地目录(不得以\结束)</param>
		/// <param name="strLocalFileName">保存在本地时的文件名</param>
		public void DownloadFile(string strRemoteFileName, string strFolder, string strLocalFileName)
		{
			if (!bConnected)
			{
				Connect();
			}
			SetTransferType(TransferType.Binary);
			if (strLocalFileName.Equals(""))
			{
				strLocalFileName = strRemoteFileName;
			}
			if (!File.Exists(strLocalFileName))
			{
				Stream st = File.Create(strLocalFileName);
				st.Close();
			}

			FileStream output = new
				FileStream(strFolder + "\\" + strLocalFileName, FileMode.Create);
			Socket socketData = CreateDataSocket();
			SendCommand("RETR " + strRemoteFileName);
			if (!(iReplyCode == 150 || iReplyCode == 125
			|| iReplyCode == 226 || iReplyCode == 250))
			{
				throw new IOException(strReply.Substring(4));
			}
			while (true)
			{
				int iBytes = socketData.Receive(buffer, buffer.Length, 0);
				output.Write(buffer, 0, iBytes);
				if (iBytes <= 0)
				{
					break;
				}
			}
			output.Close();
			if (socketData.Connected)
			{
				socketData.Close();
			}
			if (!(iReplyCode == 226 || iReplyCode == 250))
			{
				ReadReply();
				if (!(iReplyCode == 226 || iReplyCode == 250))
				{
					throw new IOException(strReply.Substring(4));
				}
			}
		}

		/// <summary>
		/// 下载一个文件(断点续传)
		/// </summary>
		/// <param name="strRemoteFileName">要下载的文件名</param>
		/// <param name="strFolder">本地目录(不得以\结束)</param>
		/// <param name="strLocalFileName">保存在本地时的文件名</param>
		/// <param name="size">已下载文件流长度</param>
		public void DownloadBrokenFile(string strRemoteFileName, string strFolder, string strLocalFileName, long size)
		{
			if (!bConnected)
			{
				Connect();
			}
			SetTransferType(TransferType.Binary);
			FileStream output = new
				FileStream(strFolder + "\\" + strLocalFileName, FileMode.Append);
			Socket socketData = CreateDataSocket();
			SendCommand("REST " + size.ToString());
			SendCommand("RETR " + strRemoteFileName);
			if (!(iReplyCode == 150 || iReplyCode == 125
			|| iReplyCode == 226 || iReplyCode == 250))
			{
				throw new IOException(strReply.Substring(4));
			}
			while (true)
			{
				int iBytes = socketData.Receive(buffer, buffer.Length, 0);
				output.Write(buffer, 0, iBytes);
				if (iBytes <= 0)
				{
					break;
				}
			}
			output.Close();
			if (socketData.Connected)
			{
				socketData.Close();
			}
			if (!(iReplyCode == 226 || iReplyCode == 250))
			{
				ReadReply();
				if (!(iReplyCode == 226 || iReplyCode == 250))
				{
					throw new IOException(strReply.Substring(4));
				}
			}
		}



		/// <summary>
		/// 上传一批文件
		/// </summary>
		/// <param name="strFolder">本地目录(不得以\结束)</param>
		/// <param name="strFileNameMask">文件名匹配字符(可以包含*和?)</param>
		public void Upload(string strFolder, string strFileNameMask)
		{
			string[] strFiles = Directory.GetFiles(strFolder, strFileNameMask);
			foreach (string strFile in strFiles)
			{
				//strFile是完整的文件名(包含路径)
				Upload(strFile);
			}
		}


		/// <summary>
		/// 上传一个文件
		/// </summary>
		/// <param name="strFileName">本地文件名</param>
		public void Upload(string strFileName)
		{
			if (!bConnected)
			{
				Connect();
			}
			Socket socketData = CreateDataSocket();
			SendCommand("STOR " + Path.GetFileName(strFileName));
			if (!(iReplyCode == 125 || iReplyCode == 150))
			{
				throw new IOException(strReply.Substring(4));
			}
			FileStream input = new
			FileStream(strFileName, FileMode.Open);
			int iBytes = 0;
			while ((iBytes = input.Read(buffer, 0, buffer.Length)) > 0)
			{
				socketData.Send(buffer, iBytes, 0);
			}
			input.Close();
			if (socketData.Connected)
			{
				socketData.Close();
			}
			if (!(iReplyCode == 226 || iReplyCode == 250))
			{
				ReadReply();
				if (!(iReplyCode == 226 || iReplyCode == 250))
				{
					throw new IOException(strReply.Substring(4));
				}
			}
		}

		#endregion

		#region 目录操作
		/// <summary>
		/// 创建目录
		/// </summary>
		/// <param name="strDirName">目录名</param>
		public void MkDir(string strDirName)
		{
			if (!bConnected)
			{
				Connect();
			}
			SendCommand("MKD " + strDirName);
			if (iReplyCode != 257)
			{
				throw new IOException(strReply.Substring(4));
			}
		}


		/// <summary>
		/// 删除目录
		/// </summary>
		/// <param name="strDirName">目录名</param>
		public void RmDir(string strDirName)
		{
			if (!bConnected)
			{
				Connect();
			}
			SendCommand("RMD " + strDirName);
			if (iReplyCode != 250)
			{
				throw new IOException(strReply.Substring(4));
			}
		}


		/// <summary>
		/// 改变目录
		/// </summary>
		/// <param name="strDirName">新的工作目录名</param>
		public void ChDir(string strDirName)
		{
			if (strDirName.Equals(".") || strDirName.Equals(""))
			{
				return;
			}
			if (!bConnected)
			{
				Connect();
			}
			SendCommand("CWD " + strDirName);
			if (iReplyCode != 250)
			{
				throw new IOException(strReply.Substring(4));
			}
			this.remoteFilePath = strDirName;
		}

		#endregion

		#region 内部变量
		/// <summary>
		/// 服务器返回的应答信息(包含应答码)
		/// </summary>
		private string strMsg;
		/// <summary>
		/// 服务器返回的应答信息(包含应答码)
		/// </summary>
		private string strReply;
		/// <summary>
		/// 服务器返回的应答码
		/// </summary>
		private int iReplyCode;
		/// <summary>
		/// 进行控制连接的socket
		/// </summary>
		private Socket socketControl;
		/// <summary>
		/// 传输模式
		/// </summary>
		private TransferType trType;
		/// <summary>
		/// 接收和发送数据的缓冲区
		/// </summary>
		private static int BLOCK_SIZE = 512;
		Byte[] buffer = new Byte[BLOCK_SIZE];
		/// <summary>
		/// 编码方式(为防止出现中文乱码采用 GB2312编码方式)
		/// </summary>
		Encoding GB2312 = Encoding.Unicode;
		#endregion

		#region 内部函数
		/// <summary>
		/// 将一行应答字符串记录在strReply和strMsg
		/// 应答码记录在iReplyCode
		/// </summary>
		private void ReadReply()
		{
			strMsg = "";
			strReply = ReadLine();
			iReplyCode = Int32.Parse(strReply.Substring(0, 3));
		}

		/// <summary>
		/// 建立进行数据连接的socket
		/// </summary>
		/// <returns>数据连接socket</returns>
		private Socket CreateDataSocket()
		{
			SendCommand("PASV");
			if (iReplyCode != 227)
			{
				throw new IOException(strReply.Substring(4));
			}
			int index1 = strReply.IndexOf('(');
			int index2 = strReply.IndexOf(')');
			string ipData =
			strReply.Substring(index1 + 1, index2 - index1 - 1);
			int[] parts = new int[6];
			int len = ipData.Length;
			int partCount = 0;
			string buf = "";
			for (int i = 0; i < len && partCount <= 6; i++)
			{
				char ch = Char.Parse(ipData.Substring(i, 1));
				if (Char.IsDigit(ch))
					buf += ch;
				else if (ch != ',')
				{
					throw new IOException("Malformed PASV strReply: " +
					strReply);
				}
				if (ch == ',' || i + 1 == len)
				{
					try
					{
						parts[partCount++] = Int32.Parse(buf);
						buf = "";
					}
					catch (Exception)
					{
						throw new IOException("Malformed PASV strReply: " +
						 strReply);
					}
				}
			}
			string ipAddress = parts[0] + "." + parts[1] + "." +
			parts[2] + "." + parts[3];
			int port = (parts[4] << 8) + parts[5];
			Socket s = new
			Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPEndPoint ep = new
			IPEndPoint(IPAddress.Parse(ipAddress), port);
			try
			{
				s.Connect(ep);
			}
			catch (Exception)
			{
				throw new IOException("Can't connect to remote server");
			}
			return s;
		}


		/// <summary>
		/// 关闭socket连接(用于登录以前)
		/// </summary>
		private void CloseSocketConnect()
		{
			if (socketControl != null)
			{
				socketControl.Close();
				socketControl = null;
			}
			bConnected = false;
		}

		/// <summary>
		/// 读取Socket返回的所有字符串
		/// </summary>
		/// <returns>包含应答码的字符串行</returns>
		private string ReadLine()
		{
			while (true)
			{
				int iBytes = socketControl.Receive(buffer, buffer.Length, 0);
				strMsg += GB2312.GetString(buffer, 0, iBytes);
				if (iBytes < buffer.Length)
				{
					break;
				}
			}
			/*char[] seperator = { '\n' };
			string[] mess = strMsg.Split(seperator);
			if (strMsg.Length > 2)
			{
				strMsg = mess[mess.Length - 2];
				//seperator[0]是10,换行符是由13和0组成的,分隔后10后面虽没有字符串,
				//但也会分配为空字符串给后面(也是最后一个)字符串数组,
				//所以最后一个mess是没用的空字符串
				//但为什么不直接取mess[0],因为只有最后一行字符串应答码与信息之间有空格
			}
			else
			{
				strMsg = mess[0];
			}*/
			char[] separator = { '\r', '\n' }; // 修改分隔符为回车符和换行符的组合
			string[] mess = strMsg.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			if (mess.Length > 1)
			{
				strMsg = mess[mess.Length - 2];
			}
			else
			{
				strMsg = mess[0];
			}
			if (!strMsg.Substring(3, 1).Equals(" "))//返回字符串正确的是以应答码(如220开头,后面接一空格,再接问候字符串)
			{
				return ReadLine();
			}
			return strMsg;
		}


		/// <summary>
		/// 发送命令并获取应答码和最后一行应答字符串
		/// </summary>
		/// <param name="strCommand">命令</param>
		private void SendCommand(String strCommand)
		{
			Byte[] cmdBytes =
			GB2312.GetBytes((strCommand + "\r\n").ToCharArray());
			socketControl.Send(cmdBytes, cmdBytes.Length, 0);
			ReadReply();
		}

		#endregion

	}

}

namespace MyFtp1
{
	class FtpService
	{
		private string host = null;
		private string user = null;
		private string pass = null;
		private FtpWebRequest ftpRequest = null;
		private FtpWebResponse ftpResponse = null;
		private Stream ftpStream = null;
		private int bufferSize = 2048;

		/* Construct Object */
		public FtpService(string hostIP, string userName, string password) { host = hostIP; user = userName; pass = password; }

		/* Download File */
		public void Download(string remoteFile, string localFile)
		{
			try
			{
				/* Create an FTP Request */
				ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + remoteFile);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
				/* Establish Return Communication with the FTP Server */
				ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
				/* Get the FTP Server's Response Stream */
				ftpStream = ftpResponse.GetResponseStream();
				/* Open a File Stream to Write the Downloaded File */
				FileStream localFileStream = new FileStream(localFile, FileMode.Create);
				/* Buffer for the Downloaded Data */
				byte[] byteBuffer = new byte[bufferSize];
				int bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
				/* Download the File by Writing the Buffered Data Until the Transfer is Complete */
				try
				{
					while (bytesRead > 0)
					{
						localFileStream.Write(byteBuffer, 0, bytesRead);
						bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
					}
				}
				catch (Exception ex) { Console.WriteLine(ex.ToString()); }
				/* Resource Cleanup */
				localFileStream.Close();
				ftpStream.Close();
				ftpResponse.Close();
				ftpRequest = null;
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
			return;
		}

		/* Upload File */
		public void Upload(string remoteFile, string localFile)
		{
			try
			{
				/* Create an FTP Request */
				ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + remoteFile);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
				/* Establish Return Communication with the FTP Server */
				ftpStream = ftpRequest.GetRequestStream();
				/* Open a File Stream to Read the File for Upload */
				FileStream localFileStream = new FileStream(localFile, FileMode.Create);
				/* Buffer for the Downloaded Data */
				byte[] byteBuffer = new byte[bufferSize];
				int bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
				/* Upload the File by Sending the Buffered Data Until the Transfer is Complete */
				try
				{
					while (bytesSent != 0)
					{
						ftpStream.Write(byteBuffer, 0, bytesSent);
						bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
					}
				}
				catch (Exception ex) { Console.WriteLine(ex.ToString()); }
				/* Resource Cleanup */
				localFileStream.Close();
				ftpStream.Close();
				ftpRequest = null;
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
			return;
		}

		/* Delete File */
		public void Delete(string deleteFile)
		{
			try
			{
				/* Create an FTP Request */
				ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + deleteFile);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
				/* Establish Return Communication with the FTP Server */
				ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
				/* Resource Cleanup */
				ftpResponse.Close();
				ftpRequest = null;
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
			return;
		}

		/* Rename File */
		public void Rename(string currentFileNameAndPath, string newFileName)
		{
			try
			{
				/* Create an FTP Request */
				ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + currentFileNameAndPath);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.Rename;
				/* Rename the File */
				ftpRequest.RenameTo = newFileName;
				/* Establish Return Communication with the FTP Server */
				ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
				/* Resource Cleanup */
				ftpResponse.Close();
				ftpRequest = null;
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
			return;
		}

		/* Create a New Directory on the FTP Server */
		public void CreateDirectory(string newDirectory)
		{
			try
			{
				/* Create an FTP Request */
				ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + newDirectory);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
				/* Establish Return Communication with the FTP Server */
				ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
				/* Resource Cleanup */
				ftpResponse.Close();
				ftpRequest = null;
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
			return;
		}

		/* Get the Date/Time a File was Created */
		public string GetFileCreatedDateTime(string fileName)
		{
			try
			{
				/* Create an FTP Request */
				ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + fileName);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;
				/* Establish Return Communication with the FTP Server */
				ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
				/* Establish Return Communication with the FTP Server */
				ftpStream = ftpResponse.GetResponseStream();
				/* Get the FTP Server's Response Stream */
				StreamReader ftpReader = new StreamReader(ftpStream);
				/* Store the Raw Response */
				string fileInfo = null;
				/* Read the Full Response Stream */
				try { fileInfo = ftpReader.ReadToEnd(); }
				catch (Exception ex) { Console.WriteLine(ex.ToString()); }
				/* Resource Cleanup */
				ftpReader.Close();
				ftpStream.Close();
				ftpResponse.Close();
				ftpRequest = null;
				/* Return File Created Date Time */
				return fileInfo;
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
			/* Return an Empty string Array if an Exception Occurs */
			return "";
		}

		/* Get the Size of a File */
		public string GetFileSize(string fileName)
		{
			try
			{
				/* Create an FTP Request */
				ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + fileName);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;
				/* Establish Return Communication with the FTP Server */
				ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
				/* Establish Return Communication with the FTP Server */
				ftpStream = ftpResponse.GetResponseStream();
				/* Get the FTP Server's Response Stream */
				StreamReader ftpReader = new StreamReader(ftpStream);
				/* Store the Raw Response */
				string fileInfo = null;
				/* Read the Full Response Stream */
				try { while (ftpReader.Peek() != -1) { fileInfo = ftpReader.ReadToEnd(); } }
				catch (Exception ex) { Console.WriteLine(ex.ToString()); }
				/* Resource Cleanup */
				ftpReader.Close();
				ftpStream.Close();
				ftpResponse.Close();
				ftpRequest = null;
				/* Return File Size */
				return fileInfo;
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
			/* Return an Empty string Array if an Exception Occurs */
			return "";
		}

		/* List Directory Contents File/Folder Name Only */
		public string[] DirectoryListSimple(string directory)
		{
			try
			{
				/* Create an FTP Request */
				ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
				/* Establish Return Communication with the FTP Server */
				ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
				/* Establish Return Communication with the FTP Server */
				ftpStream = ftpResponse.GetResponseStream();
				/* Get the FTP Server's Response Stream */
				StreamReader ftpReader = new StreamReader(ftpStream);
				/* Store the Raw Response */
				string directoryRaw = null;
				/* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
				try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
				catch (Exception ex) { Console.WriteLine(ex.ToString()); }
				/* Resource Cleanup */
				ftpReader.Close();
				ftpStream.Close();
				ftpResponse.Close();
				ftpRequest = null;
				/* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
				try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
				catch (Exception ex) { Console.WriteLine(ex.ToString()); }
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
			/* Return an Empty string Array if an Exception Occurs */
			return new string[] { "" };
		}

		/* List Directory Contents in Detail (Name, Size, Created, etc.) */
		public string[] DirectoryListDetailed(string directory)
		{
			try
			{
				/* Create an FTP Request */
				ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
				/* Establish Return Communication with the FTP Server */
				ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
				/* Establish Return Communication with the FTP Server */
				ftpStream = ftpResponse.GetResponseStream();
				/* Get the FTP Server's Response Stream */
				StreamReader ftpReader = new StreamReader(ftpStream);
				/* Store the Raw Response */
				string directoryRaw = null;
				/* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
				try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
				catch (Exception ex) { Console.WriteLine(ex.ToString()); }
				/* Resource Cleanup */
				ftpReader.Close();
				ftpStream.Close();
				ftpResponse.Close();
				ftpRequest = null;
				/* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
				try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
				catch (Exception ex) { Console.WriteLine(ex.ToString()); }
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
			/* Return an Empty string Array if an Exception Occurs */
			return new string[] { "" };
		}
	}
}