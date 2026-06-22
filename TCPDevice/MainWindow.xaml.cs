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
		private bool OPU1_Enabled = false;
		private double[] OPU1_Position = { 0.0, 0.0, 0.0, 0.0 };
		private bool OPU1_Stopped = false;
		private bool OPU1_InError = false;

		private TcpClient Client_OPU2;
		private NetworkStream Stream_OPU2;
		private float[] OPU2_LowerLimits = new float[5];
		private float[] OPU2_UpperLimits = new float[5];
		private bool OPU2_Enabled = false;
		private double[] OPU2_Position = { 0.0, 0.0, 0.0, 0.0, 0.0 };
		private bool OPU2_Stopped = false;
		private bool OPU2_InError = false;

		private DispatcherTimer OPU1_PosTimer;
		private DispatcherTimer OPU2_PosTimer;

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

			OPU1_PosTimer = new DispatcherTimer();
			OPU1_PosTimer.Interval = TimeSpan.FromMilliseconds(100);
			OPU1_PosTimer.Tick += OPU1_PosTimer_Tick;

			OPU2_PosTimer = new DispatcherTimer();
			OPU2_PosTimer.Interval = TimeSpan.FromMilliseconds(100);
			OPU2_PosTimer.Tick += OPU2_PosTimer_Tick;

			OPU1_StatusTimer = new DispatcherTimer();
			OPU1_StatusTimer.Interval = TimeSpan.FromMilliseconds(100);
			OPU1_StatusTimer.Tick += OPU1_StatusTimer_Tick;

			OPU2_StatusTimer = new DispatcherTimer();
			OPU2_StatusTimer.Interval = TimeSpan.FromMilliseconds(100);
			OPU2_StatusTimer.Tick += OPU2_StatusTimer_Tick;
		}

		private void OPU1_PosTimer_Tick(object? sender, EventArgs e)
		{
			SendCommand_OPU1("POS?");
		}

		private void OPU2_PosTimer_Tick(object? sender, EventArgs e)
		{
			SendCommand_OPU2("POS?");
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
					SendCommand_OPU1("STOP?");
					OPU1_Status++;
					break;
				case 3:
					SendCommand_OPU1("FLT?");
					OPU1_Status++;
					break;
				case 4:
					SendCommand_OPU1("LIM? A");
					OPU1_Status++;
					break;
				case 5:
					SendCommand_OPU1("LIM? E");
					OPU1_Status++;
					break;
				case 6:
					SendCommand_OPU1("LIM? P");
					OPU1_Status++;
					break;
				case 7:
					SendCommand_OPU1("LIM? Y");
					OPU1_Status = 0;
					break;
			}
			
		}

		private void OPU2_StatusTimer_Tick(object? sender, EventArgs e)
		{
			
			switch (OPU2_Status)
			{
				case 0:
					SendCommand_OPU2("EN?");
					OPU2_Status++;
					break;
				case 1:
					SendCommand_OPU2("FH?");
					OPU2_Status++;
					break;
				case 2:
					SendCommand_OPU2("STOP?");
					OPU2_Status++;
					break;
				case 3:
					SendCommand_OPU2("FLT?");
					OPU2_Status++;
					break;
				case 4:
					SendCommand_OPU2("LIM? A");
					OPU2_Status++;
					break;
				case 5:
					SendCommand_OPU2("LIM? E");
					OPU2_Status++;
					break;
				case 6:
					SendCommand_OPU2("LIM? P");
					OPU2_Status++;
					break;
				case 7:
					SendCommand_OPU2("LIM? X");
					OPU2_Status++;
					break;
				case 8:
					SendCommand_OPU2("LIM? Y");
					OPU2_Status = 0;
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

		public void SendCommand_OPU2(string Com)
		{
			if (Client_OPU2.Connected)
			{
				WriteByteData = Encoding.UTF8.GetBytes("^" + Com.Replace(',', '.') + "~\r\n");
				Stream_OPU2.Write(WriteByteData, 0, WriteByteData.Length);
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
					OPU1_PosTimer.Start();
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

		private async void ConnectOPU2()
		{
			try
			{
				IPAddress Address = IPAddress.Parse("192.168.0.102");
				int Port = 2000;
				Client_OPU2 = new TcpClient(Address.ToString(), Port);

				if (Client_OPU2.Connected)
				{
					OPU2_ConnectStatus.Fill = GreenBrush;
					Stream_OPU2 = Client_OPU2.GetStream();
					OPU2_StatusTimer.Start();
					OPU2_PosTimer.Start();
					await StartReadingOPU2DataAsync();
				}
				else
				{
					OPU2_ConnectStatus.Fill = RedBrush;
				}
			}
			catch (Exception ex)
			{
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
						OPU1_PosTimer.Stop();
						break;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Connection Error: {ex.Message}");
				OPU1_ConnectStatus.Fill = RedBrush;
				OPU1_StatusTimer.Stop();
				OPU1_PosTimer.Stop();
			}
		}

		private async Task StartReadingOPU2DataAsync()
		{
			try
			{
				byte[] Buffer = new byte[1024];
				while (Client_OPU2.Connected)
				{
					try
					{
						int BytesRead = await Stream_OPU2.ReadAsync(Buffer);
						if (BytesRead == 0)
						{
							Client_OPU2.Close();
							break;
						}
						string Data = Encoding.UTF8.GetString(Buffer, 0, BytesRead);

						switch (Data)
						{
							case "^EN?:1~":
								OPU2_Enabled = true;
								OPU2_PowerStatus.Fill = GreenBrush;
								break;
							case "^STOP?:1:1:1:1:1~":
								OPU2_Stopped = true;
								OPU2_StopStatus.Fill = GreenBrush;
								break;
							case "^FLT?:0:0:0:0:0~":
								OPU2_InError = false;
								OPU2_ClearStatus.Fill = GreenBrush;
								break;
							default:
								if (Data.Contains("EN?"))
								{
									OPU2_Enabled = false;
									OPU2_PowerStatus.Fill = RedBrush;
								}
								if (Data.Contains("STOP?"))
								{
									OPU2_Stopped = false;
									OPU2_StopStatus.Fill = RedBrush;

								}
								if (Data.Contains("FLT?"))
								{
									OPU2_InError = true;
									OPU2_ClearStatus.Fill = RedBrush;
								}
								if (Data.Contains("POS?"))
								{
									string[] Positions = Data.Substring(Data.IndexOf(":") + 1).Split(":");
									int Index = 0;
									foreach (string Position in Positions)
									{
										OPU2_Position[Index] = double.Parse(Position.Replace('.', ',').Replace("~", string.Empty));
										Index++;
									}
									OPU2_AzCurPos.Content = OPU2_Position[0].ToString("0.000°");

									OPU2_UmCurPos.Content = OPU2_Position[1].ToString("0.000°");

									OPU2_PolCurPos.Content = OPU2_Position[2].ToString("0.000°");

									OPU2_XCurPos.Content = OPU2_Position[3].ToString("0.000 мм");

									OPU2_YCurPos.Content = OPU2_Position[4].ToString("0.000 мм");
								}
								if (Data.Contains("LIM?"))
								{
									char Axis = Data[Data.IndexOf(":") - 1];
									string[] Limits = Data.Substring(Data.IndexOf(":") + 1).Split(":");
									OPU2_LowerLimits["AEPXY".IndexOf(Axis)] = float.Parse(Limits[0].Replace('.', ',').Replace("~", string.Empty));
									OPU2_UpperLimits["AEPXY".IndexOf(Axis)] = float.Parse(Limits[1].Replace('.', ',').Replace("~", string.Empty));
									switch ("AEPXY".IndexOf(Axis))
									{
										case 0:
											OPU2_AzLimitsLabel.Content = "[ " + OPU2_LowerLimits["AEPXY".IndexOf(Axis)].ToString() + " ; " + OPU2_UpperLimits["AEPXY".IndexOf(Axis)].ToString() + " ]";
											break;
										case 1:
											OPU2_UmLimitsLabel.Content = "[ " + OPU2_LowerLimits["AEPXY".IndexOf(Axis)].ToString() + " ; " + OPU2_UpperLimits["AEPXY".IndexOf(Axis)].ToString() + " ]";
											break;
										case 2:
											OPU2_PolLimitsLabel.Content = "[ " + OPU2_LowerLimits["AEPXY".IndexOf(Axis)].ToString() + " ; " + OPU2_UpperLimits["AEPXY".IndexOf(Axis)].ToString() + " ]";
											break;
										case 3:
											OPU2_XLimitsLabel.Content = "[ " + OPU2_LowerLimits["AEPXY".IndexOf(Axis)].ToString() + " ; " + OPU2_UpperLimits["AEPXY".IndexOf(Axis)].ToString() + " ]";
											break;
										case 4:
											OPU2_YLimitsLabel.Content = "[ " + OPU2_LowerLimits["AEPXY".IndexOf(Axis)].ToString() + " ; " + OPU2_UpperLimits["AEPXY".IndexOf(Axis)].ToString() + " ]";
											break;
									}
								}
								break;
						}
					}
					catch (IOException)
					{
						MessageBox.Show("Connection error");
						OPU2_ConnectStatus.Fill = RedBrush;
						OPU2_StatusTimer.Stop();
						OPU2_PosTimer.Stop();
						break;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Connection Error: {ex.Message}");
				OPU2_ConnectStatus.Fill = RedBrush;
				OPU2_StatusTimer.Stop();
				OPU2_PosTimer.Stop();
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

			if (Client_OPU2 != null)
			{
				if (Client_OPU2.Connected)
				{
					SendCommand_OPU2("DIS");
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

		private void OPU1_AzSetLim_Click(object sender, RoutedEventArgs e)
		{
			Axis_OPU1 = "A";
			Axis_OPU2 = string.Empty;
			Window LimPopup = new PopupLimits();
			LimPopup.Owner = this;
			LimPopup.ShowDialog();
		}

		private void OPU1_UmSetLim_Click(object sender, RoutedEventArgs e)
		{
			Axis_OPU1 = "E";
			Axis_OPU2 = string.Empty;
			Window LimPopup = new PopupLimits();
			LimPopup.Owner = this;
			LimPopup.ShowDialog();
		}

		private void OPU1_PolSetLim_Click(object sender, RoutedEventArgs e)
		{
			Axis_OPU1 = "P";
			Axis_OPU2 = string.Empty;
			Window LimPopup = new PopupLimits();
			LimPopup.Owner = this;
			LimPopup.ShowDialog();
		}

		private void OPU1_YSetLim_Click(object sender, RoutedEventArgs e)
		{
			Axis_OPU1 = "Y";
			Axis_OPU2 = string.Empty;
			Window LimPopup = new PopupLimits();
			LimPopup.Owner = this;
			LimPopup.ShowDialog();
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

		private void OPU2_Connect_Click(object sender, RoutedEventArgs e)
		{
			ConnectOPU2();
		}

		private void OPU2_Power_Click(object sender, RoutedEventArgs e)
		{
			if (!OPU2_Enabled)
			{
				SendCommand_OPU2("EN");
				SendCommand_OPU2("FH");
			}
			else
			{
				SendCommand_OPU2("DIS");
			}
		}

		private void OPU2_Stop_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU2("STOP");
		}

		private void OPU2_Clear_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU2("CLR");
		}

		private void OPU2_AzJogLeft_TouchDown(object sender, TouchEventArgs e)
		{
			if (double.TryParse(OPU2_AzTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE A " + OPU2_LowerLimits[0].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_AzJogLeft_TouchLeave(object sender, TouchEventArgs e)
		{
			SendCommand_OPU2("STOP A");
		}

		private void OPU2_AzMove_Click(object sender, RoutedEventArgs e)
		{
			if (double.TryParse(OPU2_AzTarPos.Text, DoubleFormat, out double Pos))
			{
				if (double.TryParse(OPU2_AzTarSpd.Text, DoubleFormat, out double Spd))
				{
					SendCommand_OPU2("MOVE A " + Pos.ToString() + " " + Spd.ToString());
				}
			}
		}

		private void OPU2_AzJogRight_TouchDown(object sender, TouchEventArgs e)
		{
			if (double.TryParse(OPU2_AzTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE A " + OPU2_UpperLimits[0].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_AzJogRight_TouchLeave(object sender, TouchEventArgs e)
		{
			SendCommand_OPU2("STOP A");
		}

		private void OPU2_AZStop_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU2("STOP A");
		}

		private void OPU2_AzSetLim_Click(object sender, RoutedEventArgs e)
		{
			Axis_OPU1 = string.Empty;
			Axis_OPU2 = "A";
			Window LimPopup = new PopupLimits();
			LimPopup.Owner = this;
			LimPopup.ShowDialog();
		}

		private void OPU2_UmJogLeft_TouchDown(object sender, TouchEventArgs e)
		{
			if (double.TryParse(OPU1_UmTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE E " + OPU2_LowerLimits[1].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_UmJogLeft_TouchLeave(object sender, TouchEventArgs e)
		{
			SendCommand_OPU2("STOP E");
		}

		private void OPU2_UmMove_Click(object sender, RoutedEventArgs e)
		{
			if (double.TryParse(OPU2_UmTarPos.Text, DoubleFormat, out double Pos))
			{
				if (double.TryParse(OPU2_UmTarSpd.Text, DoubleFormat, out double Spd))
				{
					SendCommand_OPU2("MOVE E " + Pos.ToString() + " " + Spd.ToString());
				}
			}
		}

		private void OPU2_UmJogRight_TouchDown(object sender, TouchEventArgs e)
		{
			if (double.TryParse(OPU2_UmTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE E " + OPU2_UpperLimits[1].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_UmJogRight_TouchLeave(object sender, TouchEventArgs e)
		{
			SendCommand_OPU2("STOP E");
		}

		private void OPU2_UmStop_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU2("STOP E");
		}

		private void OPU2_UmSetLim_Click(object sender, RoutedEventArgs e)
		{
			Axis_OPU1 = string.Empty;
			Axis_OPU2 = "E";
			Window LimPopup = new PopupLimits();
			LimPopup.Owner = this;
			LimPopup.ShowDialog();
		}
		
		private void OPU2_PolJogLeft_TouchDown(object sender, TouchEventArgs e)
		{
			if (double.TryParse(OPU2_PolTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE P " + OPU2_LowerLimits[2].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_PolJogLeft_TouchLeave(object sender, TouchEventArgs e)
		{
			SendCommand_OPU2("STOP P");
		}

		private void OPU2_PolMove_Click(object sender, RoutedEventArgs e)
		{
			if (double.TryParse(OPU2_PolTarPos.Text, DoubleFormat, out double Pos))
			{
				if (double.TryParse(OPU2_PolTarSpd.Text, DoubleFormat, out double Spd))
				{
					SendCommand_OPU2("MOVE P " + Pos.ToString() + " " + Spd.ToString());
				}
			}
		}

		private void OPU2_PolJogRight_TouchLeave(object sender, TouchEventArgs e)
		{
			SendCommand_OPU2("STOP P");
		}

		private void OPU2_PolJogRight_TouchDown(object sender, TouchEventArgs e)
		{
			if (double.TryParse(OPU2_PolTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE P " + OPU2_UpperLimits[2].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_PolStop_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU2("STOP P");
		}

		private void OPU2_PolSetLim_Click(object sender, RoutedEventArgs e)
		{
			Axis_OPU1 = string.Empty;
			Axis_OPU2 = "P";
			Window LimPopup = new PopupLimits();
			LimPopup.Owner = this;
			LimPopup.ShowDialog();
		}

		private void OPU2_XJogLeft_TouchDown(object sender, TouchEventArgs e)
		{
			if (double.TryParse(OPU2_XTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE X " + OPU2_LowerLimits[3].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_XJogLeft_TouchLeave(object sender, TouchEventArgs e)
		{
			SendCommand_OPU2("STOP X");
		}

		private void OPU2_XMove_Click(object sender, RoutedEventArgs e)
		{
			if (double.TryParse(OPU2_XTarPos.Text, DoubleFormat, out double Pos))
			{
				if (double.TryParse(OPU2_XTarSpd.Text, DoubleFormat, out double Spd))
				{
					SendCommand_OPU2("MOVE X " + Pos.ToString() + " " + Spd.ToString());
				}
			}
		}

		private void OPU2_XJogRight_TouchDown(object sender, TouchEventArgs e)
		{
			if (double.TryParse(OPU2_XTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE X " + OPU2_UpperLimits[3].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_XJogRight_TouchLeave(object sender, TouchEventArgs e)
		{
			SendCommand_OPU2("STOP X");
		}

		private void OPU2_XStop_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU2("STOP X");
		}

		private void OPU2_XSetLim_Click(object sender, RoutedEventArgs e)
		{
			Axis_OPU1 = string.Empty;
			Axis_OPU2 = "X";
			Window LimPopup = new PopupLimits();
			LimPopup.Owner = this;
			LimPopup.ShowDialog();
		}

		private void OPU2_YJogLeft_TouchDown(object sender, TouchEventArgs e)
		{
			if (double.TryParse(OPU2_YTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE Y " + OPU2_LowerLimits[4].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_YJogLeft_TouchLeave(object sender, TouchEventArgs e)
		{
			SendCommand_OPU2("STOP Y");
		}

		private void OPU2_YMove_Click(object sender, RoutedEventArgs e)
		{
			if (double.TryParse(OPU2_YTarPos.Text, DoubleFormat, out double Pos))
			{
				if (double.TryParse(OPU2_YTarSpd.Text, DoubleFormat, out double Spd))
				{
					SendCommand_OPU2("MOVE Y " + Pos.ToString() + " " + Spd.ToString());
				}
			}
		}

		private void OPU2_YJogRight_TouchLeave(object sender, TouchEventArgs e)
		{
			SendCommand_OPU2("STOP Y");
		}

		private void OPU2_YJogRight_TouchDown(object sender, TouchEventArgs e)
		{
			if (double.TryParse(OPU2_YTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE Y " + OPU2_UpperLimits[4].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_YStop_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU2("STOP Y");
		}

		private void OPU2_YSetLim_Click(object sender, RoutedEventArgs e)
		{
			Axis_OPU1 = string.Empty;
			Axis_OPU2 = "Y";
			Window LimPopup = new PopupLimits();
			LimPopup.Owner = this;
			LimPopup.ShowDialog();
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

		private void OPU2_AzJogLeft_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SendCommand_OPU2("STOP A");
		}

		private void OPU2_AzJogLeft_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (double.TryParse(OPU2_AzTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE A " + OPU2_LowerLimits[0].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_AzJogRight_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (double.TryParse(OPU2_AzTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE A " + OPU2_UpperLimits[0].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_AzJogRight_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SendCommand_OPU2("STOP A");
		}

		private void OPU2_UmJogLeft_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (double.TryParse(OPU2_UmTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE E " + OPU2_LowerLimits[1].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_UmJogLeft_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SendCommand_OPU2("STOP E");
		}

		private void OPU2_UmJogRight_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (double.TryParse(OPU2_UmTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE E " + OPU2_UpperLimits[1].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_UmJogRight_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SendCommand_OPU2("STOP E");
		}

		private void OPU2_PolJogLeft_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (double.TryParse(OPU2_PolTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE P " + OPU2_LowerLimits[2].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_PolJogLeft_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SendCommand_OPU2("STOP P");
		}

		private void OPU2_PolJogRight_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (double.TryParse(OPU2_PolTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE P " + OPU2_UpperLimits[2].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_PolJogRight_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SendCommand_OPU2("STOP P");
		}

		private void OPU2_XJogLeft_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (double.TryParse(OPU2_XTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE X " + OPU2_LowerLimits[3].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_XJogLeft_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SendCommand_OPU2("STOP X");
		}

		private void OPU2_XJogRight_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (double.TryParse(OPU2_XTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE X " + OPU2_UpperLimits[3].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_XJogRight_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SendCommand_OPU2("STOP X");
		}

		private void OPU2_YJogLeft_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (double.TryParse(OPU2_YTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE Y " + OPU2_LowerLimits[4].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_YJogLeft_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SendCommand_OPU2("STOP Y");
		}

		private void OPU2_YJogRight_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (double.TryParse(OPU2_YTarSpd.Text, DoubleFormat, out double Spd))
			{
				SendCommand_OPU2("MOVE Y " + OPU2_UpperLimits[4].ToString() + " " + Spd.ToString());
			}
		}

		private void OPU2_YJogRight_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SendCommand_OPU2("STOP Y");
		}

		private void OPU1_ServiceImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (OPU1_IngoreLimit_Counter != 10)
			{
				OPU1_IngoreLimit_Counter += 1;
			}
			else
			{
				SendCommand_OPU1("IGNORELIMIT");
				OPU1_IngoreLimit_Counter = 0;
			}
		}

		private void OPU2_ServiceImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (OPU2_IngoreLimit_Counter != 10)
			{
				OPU2_IngoreLimit_Counter += 1;
			}
			else
			{
				SendCommand_OPU2("IGNORELIMIT");
				OPU1_IngoreLimit_Counter = 0;
			}
		}
	}
}
