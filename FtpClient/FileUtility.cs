using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpClient
{
	public static class FileUtility
	{
		public static string FormatFileSize(long size)
		{
			string[] units = { "B", "KB", "MB", "GB", "TB" };
			int unitIndex = 0;
			double fileSize = size;

			while (fileSize >= 1024 && unitIndex < units.Length - 1)
			{
				fileSize /= 1024;
				unitIndex++;
			}

			// 在整数部分少于3位时，只保留3位有效数字
			if (fileSize >= 100)
			{
				return $"{fileSize:0} {units[unitIndex]}";
			}
			else
			{
				return $"{fileSize:G3} {units[unitIndex]}";
			}
		}

		// 为确保新文件名不会重复，若原文件名已存在，则在后面加一个(1)，如果仍然存在则括号内数字递增到不存在为止
		public static string GenerateUniqueNamedFile(string file)
		{
			string fileName = Path.GetFileNameWithoutExtension(file);
			string extension = Path.GetExtension(file);
			string newFile = file;
			int count = 1;
			while (File.Exists(newFile))
			{
				newFile = Path.Combine(Path.GetDirectoryName(file) ?? "", $"{fileName} ({count}){extension}");
				++count;
			}
			return newFile;
		}
	}
}
