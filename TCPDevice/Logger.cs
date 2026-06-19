using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace TCPDevice
{
	internal class Logger : IDisposable
	{
		public enum LogType
		{
			SEND_LOG,
			GET_LOG,
			ERR_LOG,
			INFO_LOG
		}

		private StreamWriter FileStream;

		string DefaultLogPath = AppDomain.CurrentDomain.BaseDirectory + $"\\Logs\\{DateTime.Now.ToString().Replace(':','_')} Log.txt";

		public Logger()
		{
			FileStream = File.CreateText(DefaultLogPath);
			FileStream.WriteLine($"{DateTime.Now}: ИНФО     : НАЧАЛО ЛОГИРОВАНИЯ");
		}

		public Logger(string FilePath)
		{
			FileStream = File.CreateText(FilePath + $"\\{DateTime.Now.ToString().Replace(':', '_')} Log.txt");
			FileStream.WriteLine($"{DateTime.Now}: ИНФО     : НАЧАЛО ЛОГИРОВАНИЯ");
		}

		public void Dispose()
		{
			FileStream.WriteLine($"{DateTime.Now}: ИНФО     : КОНЕЦ ЛОГИРОВАНИЯ");
			FileStream.Close();
		}

		public bool WriteLog(LogType Type, string Message)
		{
			switch (Type)
			{
				case LogType.SEND_LOG:
					try
					{
						FileStream.WriteLine($"{DateTime.Now}: ОТПРАВКА : {Message}");
						return true;
					}
					catch
					{
						return false;
					}
				case LogType.GET_LOG:
					try
					{
						FileStream.WriteLine($"{DateTime.Now}: ПОЛУЧЕНИЕ: {Message}");
						return true;
					}
					catch
					{
						return false;
					}
				case LogType.ERR_LOG:
					try
					{
						FileStream.WriteLine($"{DateTime.Now}: ОШИБКА   : {Message}");
						return true;
					}
					catch
					{
						return false;
					}
				case LogType.INFO_LOG:
					try
					{
						FileStream.WriteLine($"{DateTime.Now}: ИНФО     : {Message}");
						return true;
					}
					catch
					{
						return false;
					}
				default:
					return false;
			}
		}
	}
}
