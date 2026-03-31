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

namespace TCPDevice
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private TcpClient Client_OPU1;
		private NetworkStream Stream_OPU1;
		private float[] OPU1_LowerLimits = new float[4];
		private float[] OPU1_UpperLimits = new float[4];
		private float[] OPU1_LowerVelLimits = {3.0f, 3.0f, 3.0f, 25.0f };
		private float[] OPU1_UpperVelLimits = {12.0f, 12.0f, 12.0f, 50.0f };
		private bool OPU1_Enabled = false;
		private double[] OPU1_Position = { 0.0, 0.0, 0.0, 0.0 };
		private bool OPU1_Stopped = false;
		private bool OPU1_InError = false;

		private DispatcherTimer OPU1_StatusTimer;
		private DispatcherTimer OPU2_StatusTimer;
		private int OPU1_Status = 0;
		private int OPU2_Status = 0;
		private byte[] WriteByteData;
		public string Axis_OPU1 = "";
		public string Axis_OPU2 = "";
		public bool AllowNegative = false;

		private int OPU1_IngoreLimit_Counter = 0;
		private int OPU2_IngoreLimit_Counter = 0;

		private RadialGradientBrush GreenBrush = new(Color.FromRgb(255, 255, 255), Color.FromRgb(0, 255, 0));
		private RadialGradientBrush RedBrush = new(Color.FromRgb(255, 255, 255), Color.FromRgb(255, 0, 0));
		private NumberFormatInfo DoubleFormat = new NumberFormatInfo();
		public MainWindow()
        {
            InitializeComponent();
			GreenBrush.Center = new Point(0.25, 0.25);
			RedBrush.Center = new Point(0.25, 0.25);
			DoubleFormat.NumberDecimalSeparator = ".";
			OPU1_StatusTimer = new DispatcherTimer();
			OPU1_StatusTimer.Interval = TimeSpan.FromMilliseconds(100);
			OPU1_StatusTimer.Tick += OPU1_StatusTimer_Tick;
		}

		private void OPU1_StatusTimer_Tick(object? sender, EventArgs e)
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
					SendCommand_OPU1("POS?");
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
					OPU1_ConnectStatus.Fill = GreenBrush;
					Stream_OPU1 = Client_OPU1.GetStream();
					OPU1_StatusTimer.Start();
					await StartReadingOPU1DataAsync();
				}
				else
				{
					OPU1_ConnectStatus.Fill = RedBrush;
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
								OPU1_Enabled = true;
								OPU1_PowerStatus.Fill = GreenBrush;
								break;
							case "^STOP?:1:1:1:1~":
								OPU1_Stopped = true;
								OPU1_StopStatus.Fill = GreenBrush;
								break;
							case "^FLT?:0:0:0:0~":
								OPU1_InError = false;
								OPU1_ClearStatus.Fill = GreenBrush;
								break;
							default:
								if (Data.Contains("EN?"))
								{
									OPU1_Enabled = false;
									OPU1_PowerStatus.Fill = RedBrush;
								}
								if (Data.Contains("STOP?"))
								{
									OPU1_Stopped = false;
									OPU1_StopStatus.Fill = RedBrush;
									
								}
								if (Data.Contains("FLT?"))
								{
									OPU1_InError = true;
									OPU1_ClearStatus.Fill= RedBrush;
								}
								if (Data.Contains("POS?"))
								{
									string[] Positions = Data.Substring(Data.IndexOf(":") + 1).Split(":");
									int Index = 0;
									foreach (string Position in Positions)
									{
										OPU1_Position[Index] = double.Parse(Position.Replace('.',',').Replace("~",string.Empty));
										Index++;
									}
									OPU1_AzCurPos.Content = OPU1_Position[0].ToString("0.000°");

									OPU1_UmCurPos.Content = OPU1_Position[1].ToString("0.000°");


									OPU1_PolCurPos.Content = OPU1_Position[2].ToString("0.000°");

									OPU1_YCurPos.Content = OPU1_Position[3].ToString("0.000 мм");
								}
								if (Data.Contains("LIM?"))
								{
									char Axis = Data[Data.IndexOf(":") - 1];
									string[] Limits = Data.Substring(Data.IndexOf(":") + 1).Split(":");
									OPU1_LowerLimits["AEPY".IndexOf(Axis)] = float.Parse(Limits[0].Replace('.', ',').Replace("~", string.Empty));
									OPU1_UpperLimits["AEPY".IndexOf(Axis)] = float.Parse(Limits[1].Replace('.', ',').Replace("~", string.Empty));
									switch("AEPY".IndexOf(Axis)){
										case 0:
											OPU1_AzLimitsLabel.Content = "[ " + OPU1_LowerLimits["AEPY".IndexOf(Axis)].ToString() + " ; " + OPU1_UpperLimits["AEPY".IndexOf(Axis)].ToString() + " ]";
											break;
										case 1:
											OPU1_UmLimitsLabel.Content = "[ " + OPU1_LowerLimits["AEPY".IndexOf(Axis)].ToString() + " ; " + OPU1_UpperLimits["AEPY".IndexOf(Axis)].ToString() + " ]";
											break;
										case 2:
											OPU1_PolLimitsLabel.Content = "[ " + OPU1_LowerLimits["AEPY".IndexOf(Axis)].ToString() + " ; " + OPU1_UpperLimits["AEPY".IndexOf(Axis)].ToString() + " ]";
											break;
										case 3:
											OPU1_YLimitsLabel.Content = "[ " + OPU1_LowerLimits["AEPY".IndexOf(Axis)].ToString() + " ; " + OPU1_UpperLimits["AEPY".IndexOf(Axis)].ToString() + " ]";
											break;
									}
								}
								break;
						}
					}
					catch (IOException)
					{
						MessageBox.Show("Connection error");
						OPU1_ConnectStatus.Fill = RedBrush;
						OPU1_StatusTimer.Stop();
						break;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Connection Error: {ex.Message}");
				OPU1_ConnectStatus.Fill = RedBrush;
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


		private void OPU1_AzJogLeft_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (double.TryParse(OPU1_AzTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU1("MOVE A " + OPU1_LowerLimits[0].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU1_AzJogLeft_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SendCommand_OPU1("STOP A");
		}

		private void OPU1_AzJogRight_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (double.TryParse(OPU1_AzTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU1("MOVE A " + OPU1_UpperLimits[0].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU1_AzJogRight_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SendCommand_OPU1("STOP A");
		}

		private void OPU1_AzMove_Click(object sender, RoutedEventArgs e)
		{
			if (double.TryParse(OPU1_AzTarPos.Text, DoubleFormat, out double Pos))
			{
				if (double.TryParse(OPU1_AzTarSpd.Text, DoubleFormat, out double Spd))
				{
					SendCommand_OPU1("MOVE A " + Pos.ToString() + " " + Spd.ToString());
				}
			}
		}

		private void OPU1_AZStop_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU1("STOP A");
		}

		private void OPU1_UmJogLeft_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (double.TryParse(OPU1_UmTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU1("MOVE E " + OPU1_LowerLimits[1].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU1_UmJogLeft_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SendCommand_OPU1("STOP E");
		}

		private void OPU1_UmMove_Click(object sender, RoutedEventArgs e)
		{
			if (double.TryParse(OPU1_UmTarPos.Text, DoubleFormat, out double Pos))
			{
				if (double.TryParse(OPU1_UmTarSpd.Text, DoubleFormat, out double Spd))
				{
					SendCommand_OPU1("MOVE E " + Pos.ToString() + " " + Spd.ToString());
				}
			}
		}

		private void OPU1_UmJogRight_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (double.TryParse(OPU1_UmTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU1("MOVE E " + OPU1_UpperLimits[1].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU1_UmJogRight_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SendCommand_OPU1("STOP E");
		}

		private void OPU1_UmStop_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU1("STOP E");
		}

		private void OPU1_PolJogLeft_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (double.TryParse(OPU1_PolTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU1("MOVE P " + OPU1_LowerLimits[2].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU1_PolJogLeft_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SendCommand_OPU1("STOP P");
		}

		private void OPU1_PolMove_Click(object sender, RoutedEventArgs e)
		{
			if (double.TryParse(OPU1_PolTarPos.Text, DoubleFormat, out double Pos))
			{
				if (double.TryParse(OPU1_PolTarSpd.Text, DoubleFormat, out double Spd))
				{
					SendCommand_OPU1("MOVE P " + Pos.ToString() + " " + Spd.ToString());
				}
			}
		}

		private void OPU1_PolJogRight_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (double.TryParse(OPU1_PolTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU1("MOVE P " + OPU1_UpperLimits[2].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU1_PolJogRight_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SendCommand_OPU1("STOP P");
		}

		private void OPU1_PolStop_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU1("STOP P");
		}

		private void OPU1_YJogLeft_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (double.TryParse(OPU1_YTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU1("MOVE Y " + OPU1_LowerLimits[3].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU1_YJogLeft_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SendCommand_OPU1("STOP Y");
		}

		private void OPU1_YMove_Click(object sender, RoutedEventArgs e)
		{
			if (double.TryParse(OPU1_YTarPos.Text, DoubleFormat, out double Pos))
			{
				if (double.TryParse(OPU1_YTarSpd.Text, DoubleFormat, out double Spd))
				{
					SendCommand_OPU1("MOVE Y " + Pos.ToString() + " " + Spd.ToString());
				}
			}
		}

		private void OPU1_YJogRight_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (double.TryParse(OPU1_YTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU1("MOVE Y " + OPU1_UpperLimits[3].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU1_YJogRight_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SendCommand_OPU1("STOP Y");
		}

		private void OPU1_YStop_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU1("STOP Y");
		}

		private void OPU1_Stop_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU1("STOP");
		}

		private void OPU1_Clear_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU1("CLR");
		}

		private void OPU1_AzJogLeft_TouchDown(object sender, TouchEventArgs e)
		{
			if (double.TryParse(OPU1_AzTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU1("MOVE A " + OPU1_LowerLimits[0].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU1_AzJogLeft_TouchLeave(object sender, TouchEventArgs e)
		{
			SendCommand_OPU1("STOP A");
		}

		private void OPU1_AzJogRight_TouchDown(object sender, TouchEventArgs e)
		{
			if (double.TryParse(OPU1_AzTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU1("MOVE A " + OPU1_UpperLimits[0].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU1_AzJogRight_TouchLeave(object sender, TouchEventArgs e)
		{
			SendCommand_OPU1("STOP A");
		}

		private void OPU1_UmJogLeft_TouchDown(object sender, TouchEventArgs e)
		{
			if (double.TryParse(OPU1_UmTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU1("MOVE E " + OPU1_LowerLimits[1].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU1_UmJogRight_TouchDown(object sender, TouchEventArgs e)
		{
			if (double.TryParse(OPU1_UmTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU1("MOVE E " + OPU1_UpperLimits[1].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU1_UmJogLeft_TouchLeave(object sender, TouchEventArgs e)
		{
			SendCommand_OPU1("STOP E");
		}

		private void OPU1_UmJogRight_TouchLeave(object sender, TouchEventArgs e)
		{
			SendCommand_OPU1("STOP E");
		}

		private void OPU1_PolJogLeft_TouchDown(object sender, TouchEventArgs e)
		{
			if (double.TryParse(OPU1_PolTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU1("MOVE P " + OPU1_LowerLimits[2].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU1_PolJogLeft_TouchLeave(object sender, TouchEventArgs e)
		{
			SendCommand_OPU1("STOP P");
		}

		private void OPU1_PolJogRight_TouchDown(object sender, TouchEventArgs e)
		{
			if (double.TryParse(OPU1_PolTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU1("MOVE P " + OPU1_UpperLimits[2].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU1_PolJogRight_TouchLeave(object sender, TouchEventArgs e)
		{
			SendCommand_OPU1("STOP P");
		}

		private void OPU1_YJogLeft_TouchDown(object sender, TouchEventArgs e)
		{
			if (double.TryParse(OPU1_YTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU1("MOVE Y " + OPU1_LowerLimits[3].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU1_YJogLeft_TouchLeave(object sender, TouchEventArgs e)
		{
			SendCommand_OPU1("STOP Y");
		}

		private void OPU1_YJogRight_TouchDown(object sender, TouchEventArgs e)
		{
			if (double.TryParse(OPU1_YTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU1("MOVE Y " + OPU1_UpperLimits[3].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU1_YJogRight_TouchLeave(object sender, TouchEventArgs e)
		{
			SendCommand_OPU1("STOP Y");
		}

		private void OPU1_TextBox_TouchDown(object sender, TouchEventArgs e)
		{
			KeyboardWindow KW = new KeyboardWindow();
			KW.TB = (TextBox)sender;
			KW.Owner = this;
			KW.ShowDialog();
		}

		private void OPU1_Speed_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (((TextBox)sender).Text.Contains("-"))
			{
				((TextBox)sender).Text = ((TextBox)sender).Text.Replace("-", string.Empty);
			}
		}

		private void OPU1_ServiceImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (OPU1_IngoreLimit_Counter != 10)
			{
				OPU1_IngoreLimit_Counter += 1;
			}
			else
			{
				SendCommand_OPU1("INGORELIMIT");
			}
		}
	}
}
