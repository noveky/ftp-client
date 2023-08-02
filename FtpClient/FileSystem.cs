using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpClient
{
	public static class FileSystem
	{
		public static string GetSizeStr(long size)
		{
			if (size < 1024)
			{
				return $"{size} B";
			}
			else if (size < 1048576)
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
		public static string GetUniqueNameLocalFile(string localFile)
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
	}
}
