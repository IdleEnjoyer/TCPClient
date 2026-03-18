using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Configuration;
using System.Net;
using System.Windows.Markup;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Threading;
using System.Globalization;

namespace TCPDevice
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private TcpClient Client;
		private NetworkStream Stream;
		private float[] OPU1_Limits = { -90.0f, 90.0f };
		private bool OPU1_Enabled = false;
		private double OPU1_Position = 0.0;
		private double OPU1_TargetPos = 0.0;
		
		private bool OPU1_Stopped = false;
		private bool OPU1_InError = false;
		private DispatcherTimer Status_Timer = new DispatcherTimer();
		private int Status = 0;

		private byte[] WriteByteData;
		private NumberFormatInfo DoubleFormat = new NumberFormatInfo();

		private bool isDragging = false;
		public MainWindow()
        {
            InitializeComponent();
			DoubleFormat.NumberDecimalSeparator = ".";
			Status_Timer.Interval = TimeSpan.FromMilliseconds(125);
			Status_Timer.Tick += Status_Timer_Tick;

		}

		private void Status_Timer_Tick(object? sender, EventArgs e)
		{
			switch (Status)
			{
				case 0:
					SendCommand("EN?");
					Status = 1;
					break;
				case 1:
					SendCommand("POS?");
					Status = 0;
					break;
			}
		}

		public void SendCommand(string Com)
		{
			if (Client.Connected)
			{
				WriteByteData = Encoding.UTF8.GetBytes("^" + Com + "\r\n");
				Stream.Write(WriteByteData, 0, WriteByteData.Length);
			}
		}



		private async void ConnectOPU1()
		{
			try
			{
				IPAddress Address = IPAddress.Parse("192.168.0.101");
				int Port = 2000;
				Client = new TcpClient(Address.ToString(), Port);

				if (Client.Connected)
				{
					Stream = Client.GetStream();
					Status_Timer.Start();
					await StartReadingDataAsync();
					
				}
				else
				{
					throw new Exception("No connection");
				}
			}
			catch(Exception ex) { 
				MessageBox.Show(ex.Message);
			}
		}

		private async Task StartReadingDataAsync()
		{
			try
			{
				byte[] Buffer = new byte[1024];
				while (Client.Connected)
				{
					try
					{
						int BytesRead = await Stream.ReadAsync(Buffer);
						if (BytesRead == 0)
						{
							Client.Close();
							break;
						}
						string Data = Encoding.UTF8.GetString(Buffer, 0, BytesRead);

						switch (Data)
						{
							case "^EN?:1":
								OPU1_Enabled = true;
								PowerSwitch_Figure.Stroke = Brushes.Green;
								break;
							default:
								if (Data.Contains("EN?"))
								{
									OPU1_Enabled = false;
									PowerSwitch_Figure.Stroke = Brushes.Red;
								}
								if (Data.Contains("POS?"))
								{
									double.TryParse(Data.Split(':')[1], DoubleFormat, out OPU1_Position);
									//OPU1_Position[0] = double.Parse(Data.Split(':')[1].Replace('.',','));
									//OPU1_Position[1] = double.Parse(Data.Split(':')[2].Replace('~', ' ').Replace('.', ','));
									XPos_Label.Content = "Позиция: " + OPU1_Position.ToString("0.0°");
									RotateTransform RT = new RotateTransform(OPU1_Position);
									PosLine.RenderTransform = RT;
								}
								break;
						}
					}
					catch (IOException)
					{
						MessageBox.Show("Connection error");
						break;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Connection Error: {ex.Message}");
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (Client != null)
			{
				if (Client.Connected)
				{
					SendCommand("DIS");
				}
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				ConnectOPU1();
			}
			catch
			{
				MessageBox.Show("NO Connection");
			}
		}

		private void PowerSwitch_Btn_Click(object sender, RoutedEventArgs e)
		{
			if (Client != null)
			{
				if (!OPU1_Enabled)
				{
					SendCommand("EN");
				}
				else
				{
					SendCommand("DIS");
				}
			}
		}

		private void Stop_Btn_Click(object sender, RoutedEventArgs e)
		{
			if (Client != null)
			{
				SendCommand("STOP");
			}
		}

		private void MoveBtn_Click(object sender, RoutedEventArgs e)
		{
			
			if (Client != null)
			{
				if (OPU1_Enabled)
				{
					double Pos;
					double Spd;
					if (!double.TryParse(MovePos.Text, DoubleFormat, out Pos)) return;
					if (!double.TryParse(MoveSpd.Text, DoubleFormat, out Spd)) return;
					SendCommand("MOVEA " + Pos + " " + Spd);
				}
			}
		}

		private void StepBtn_Click(object sender, RoutedEventArgs e)
		{
			if (Client != null)
			{
				if (OPU1_Enabled)
				{
					double Step;
					double Spd;
					if (!double.TryParse(StepSize.Text, DoubleFormat, out Step)) return;
					if (!double.TryParse(StepSpd.Text, DoubleFormat, out Spd)) return;
					SendCommand("STEP " + Step + " " + Spd);
				}
			}
		}
	}
}
