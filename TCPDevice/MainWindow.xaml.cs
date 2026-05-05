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
using System.Threading;
using System.Text.RegularExpressions;
using System.Drawing;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace TCPDevice
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private TcpClient Client_OPU1;
		private NetworkStream Stream_OPU1;
		private double OPU1_LowerLimits;
		private double OPU1_UpperLimits;
		private double OPU1_LowerVelLimits;
		private double OPU1_UpperVelLimits;
		private bool OPU1_Enabled = false;
		private double OPU1_Position;
		private bool OPU1_Stopped = false;
		private bool OPU1_InError = false;

		private DispatcherTimer OPU1_StatusTimer = new DispatcherTimer();
		private DispatcherTimer OPU_PosTimer = new DispatcherTimer();
		private int OPU1_Status = 0;
		private byte[] WriteByteData;
		public string Axis_OPU1 = "";
		public bool AllowNegative = false;

		private int OPU1_IngoreLimit_Counter = 0;
		private Regex ValidateRealNumber = new Regex(@"^-?[0-9]+(\\.[0-9]+)?$");

		private RadialGradientBrush GreenBrush = new(Color.FromRgb(255, 255, 255), Color.FromRgb(0, 255, 0));
		private RadialGradientBrush RedBrush = new(Color.FromRgb(255, 255, 255), Color.FromRgb(255, 0, 0));
		private NumberFormatInfo DoubleFormat = new NumberFormatInfo();
		public MainWindow()
        {
            InitializeComponent();
			GreenBrush.Center = new Point(0.25, 0.25);
			RedBrush.Center = new Point(0.25, 0.25);
			DoubleFormat.NumberDecimalSeparator = ".";
			OPU1_StatusTimer.Interval = TimeSpan.FromMilliseconds(100);
			OPU1_StatusTimer.Tick += OPU1_StatusTimer_Tick;
			OPU_PosTimer.Interval = TimeSpan.FromMilliseconds(50);
			OPU_PosTimer.Tick += OPU_PosTimer_Tick;
		}

		private void OPU_PosTimer_Tick(object? sender, EventArgs e)
		{
			if (Client_OPU1 != null) {
				SendCommand_OPU1("POS?");
			}
		}

		private void OPU1_StatusTimer_Tick(object? sender, EventArgs e)
		{
			if (Client_OPU1 != null)
			{
				switch (OPU1_Status)
				{
					case 0:
						SendCommand_OPU1("EN?");
						OPU1_Status++;
						break;
					case 1:
						SendCommand_OPU1("FH?");
						OPU1_Status++;
						break;
					case 2:
						SendCommand_OPU1("INIT?");
						OPU1_Status++;
						break;
					case 3:
						SendCommand_OPU1("STOP?");
						OPU1_Status++;
						break;
					case 4:
						SendCommand_OPU1("FLT?");
						OPU1_Status = 0;
						break;
				}
			}
		}

		public void SendCommand_OPU1(string Com)
		{
			if (Client_OPU1.Connected)
			{
				WriteByteData = Encoding.UTF8.GetBytes("^" + Com.Replace(',','.') + "~\r\n");
				Stream_OPU1.Write(WriteByteData, 0, WriteByteData.Length);
			}
		}

		private async void ConnectOPU1()
		{
			try
			{
				IPAddress Address = IPAddress.Parse("192.168.0.101");
				int Port = 2000;
				Client_OPU1 = new TcpClient(Address.ToString(), Port);

				if (Client_OPU1.Connected)
				{
					//ConnectStatus.Fill = GreenBrush;
					Stream_OPU1 = Client_OPU1.GetStream();
					OPU1_StatusTimer.Start();
					await StartReadingOPU1DataAsync();
				}
				else
				{
					//ConnectStatus.Fill = RedBrush;
				}
			}
			catch(Exception ex) { 
				MessageBox.Show(ex.Message);
			}
		}

		private async Task StartReadingOPU1DataAsync()
		{
			try
			{
				byte[] Buffer = new byte[1024];
				while (Client_OPU1.Connected)
				{
					try
					{
						int BytesRead = await Stream_OPU1.ReadAsync(Buffer);
						if (BytesRead == 0)
						{
							Client_OPU1.Close();
							break;
						}
						string Data = Encoding.UTF8.GetString(Buffer, 0, BytesRead);

						switch (Data)
						{
							case "^EN?:1~":
								PowerCheck.IsChecked = true;
								break;
							case "^STOP?:1~":
								
								break;
							case "^FLT?:0~":
								InFaultCheck.IsChecked = false;
								break;
							default:
								if (Data.Contains("EN?"))
								{
									PowerCheck.IsChecked = false;
								}
								if (Data.Contains("STOP?"))
								{
									OPU1_Stopped = false;
									MoveStatus.Fill = Brushes.Red;
									
								}
								if (Data.Contains("FLT?"))
								{
									InFaultCheck.IsChecked = true;
								}
								if (Data.Contains("POS?"))
								{
									string[] Positions = Data.Substring(Data.IndexOf(":") + 1).Split(":");
									int Index = 0;
									foreach (string Position in Positions)
									{
										OPU1_Position = double.Parse(Position.Replace('.',',').Replace("~",string.Empty));
										Index++;
									}
								}
								break;
						}
					}
					catch (IOException)
					{
						MessageBox.Show("Connection error");
						//ConnectStatus.Fill = RedBrush;
						OPU1_StatusTimer.Stop();
						break;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Connection Error: {ex.Message}");
				//ConnectStatus.Fill = RedBrush;
				OPU1_StatusTimer.Stop();
			}
		}

		private void OPU1_Connect_Click(object sender, RoutedEventArgs e)
		{
			ConnectOPU1();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (Client_OPU1 != null)
			{
				if (Client_OPU1.Connected)
				{
					SendCommand_OPU1("DIS");
				}
			}
		}

		private void OPU1_Power_Click(object sender, RoutedEventArgs e)
		{
			if (!OPU1_Enabled)
			{
				SendCommand_OPU1("EN");
				SendCommand_OPU1("FH");
			}
			else
			{
				SendCommand_OPU1("DIS");
			}
		}

		private void OPU1_Stop_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU1("STOP");
		}

		private void OPU1_Clear_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU1("CLR");
		}

		private void LeftPanelSize_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
		{
			if (LeftPanel.Width + e.HorizontalChange >= LeftPanel.MinWidth && LeftPanel.Width + e.HorizontalChange <= LeftPanel.MaxWidth)
			{
				LeftPanel.Width += e.HorizontalChange;
			}
			else if (LeftPanel.Width + e.HorizontalChange <= LeftPanel.MinWidth)
			{
				LeftPanel.Width = LeftPanel.MinWidth;
			}
			else
			{
				LeftPanel.Width = LeftPanel.MaxWidth;
			}
		}

		private void Tool_Click(object sender, RoutedEventArgs e)
		{
			Control.SelectedIndex = int.Parse((string)((TreeViewItem)sender).Tag);
		}

		private void Power_Click(object sender, RoutedEventArgs e)
		{
			PowerCheck.IsChecked = !PowerCheck.IsChecked;
		}

		private void Connection_Click(object sender, RoutedEventArgs e)
		{
			ConnectionCheck.IsChecked = !ConnectionCheck.IsChecked;
		}

		private void Fault_Click(object sender, RoutedEventArgs e)
		{
			InFaultCheck.IsChecked = !InFaultCheck.IsChecked;
		}

		private void Abs_StartMove_Click(object sender, RoutedEventArgs e)
		{
			if (Client_OPU1 != null)
			{
				if (PowerCheck.IsChecked == false)
				{
					MessageBox.Show("Нет Питания!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
				else
				{
					if (Abs_TgtPos.Text != string.Empty && Abs_TgtVel.Text != string.Empty && Abs_TgtAcc.Text != string.Empty)
					{
						
					}
				}
			}
			else
			{
				MessageBox.Show("Нет Подключения!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			
        }

		private void Abs_StopMove_Click(object sender, RoutedEventArgs e)
		{

		}

		private void NumberInputTextbox_LostFocus(object sender, RoutedEventArgs e)
		{
			double Num;
			if (!double.TryParse(((TextBox)sender).Text, DoubleFormat, out Num))
			{
				((TextBox)sender).BorderBrush = Brushes.Red;
			}
			else
			{
				((TextBox)sender).BorderBrush = new SolidColorBrush(Color.FromArgb(0xff, 0xAB, 0xAd, 0xB3));
			}
		}
	}
}
