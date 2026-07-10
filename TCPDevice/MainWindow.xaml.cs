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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TCPDevice
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	/// 

	public class AnimationChange : INotifyPropertyChanged
	{
		private bool _OPU_Enabled { get; set; } = false;
		private bool _OPU_InError { get; set; } = false;
		private bool _OPU_Connected { get; set; } = false;

		public bool OPU_Enabled
		{
			get => _OPU_Enabled;
			set
			{
				if (_OPU_Enabled != value)
				{
					_OPU_Enabled = value;
					OnPropertyChanged();
				}
			}
		}

		public bool OPU_InError
		{
			get => _OPU_InError;
			set
			{
				if (_OPU_InError != value)
				{
					_OPU_InError = value;
					OnPropertyChanged();
				}
			}
		}

		public bool OPU_Connected
		{
			get => _OPU_Connected;
			set
			{
				if (_OPU_Connected != value)
				{
					_OPU_Connected = value;
					OnPropertyChanged();
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
	public partial class MainWindow : Window
	{
		public TcpClient Client_OPU1;
		private NetworkStream Stream_OPU1;
		private float[] OPU1_LowerLimits = new float[2];
		private float[] OPU1_UpperLimits = new float[2];
		public bool OPU1_Enabled { get; set; } = false;
		private double[] OPU1_Position = { 0.0, 0.0 };
		public bool OPU1_Stopped = false;
		public bool OPU1_InError { get; set; } = false;
		private bool OPU1_Zeroing = false;

		private TcpClient Client_OPU2;
		private NetworkStream Stream_OPU2;
		private float[] OPU2_LowerLimits = new float[5];
		private float[] OPU2_UpperLimits = new float[5];
		public bool OPU2_Enabled { get; set; } = false;
		private double[] OPU2_Position = { 0.0, 0.0, 0.0, 0.0, 0.0 };
		public bool OPU2_Stopped = false;
		public bool OPU2_InError { get; set; } = false;
		private bool OPU2_Zeroing = false;

		private DispatcherTimer OPU1_PosTimer;
		private DispatcherTimer OPU2_PosTimer;

		private DispatcherTimer OPU1_DemoTimer_P;
		private DispatcherTimer OPU1_DemoTimer_Y;

		private DispatcherTimer OPU2_DemoTimer_A;
		private DispatcherTimer OPU2_DemoTimer_E;
		private DispatcherTimer OPU2_DemoTimer_P;
		private DispatcherTimer OPU2_DemoTimer_X;
		private DispatcherTimer OPU2_DemoTimer_Y;

		private bool[] OPU1_DemoState = { false, false };
		private bool[] OPU2_DemoState = { false, false, false, false, false };

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

		public AnimationChange AC1 = new AnimationChange();
		public AnimationChange AC2 = new AnimationChange();

		private NumberFormatInfo DoubleFormat = new NumberFormatInfo();
		public MainWindow()
        {
            InitializeComponent();

			OPU1_Power.DataContext = AC1;
			OPU1_Connect.DataContext = AC1;
			OPU1_Clear.DataContext = AC1;
			OPU2_Power.DataContext = AC2;
			OPU2_Connect.DataContext = AC2;
			OPU2_Clear.DataContext = AC2;

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
			OPU1_StatusTimer.Start();

			OPU2_StatusTimer = new DispatcherTimer();
			OPU2_StatusTimer.Interval = TimeSpan.FromMilliseconds(100);
			OPU2_StatusTimer.Tick += OPU2_StatusTimer_Tick;
			OPU2_StatusTimer.Start();

			OPU1_DemoTimer_P = new DispatcherTimer();
			OPU1_DemoTimer_P.Interval = TimeSpan.FromSeconds(70);
			OPU1_DemoTimer_P.Tick += OPU1_DemoTimer_P_Tick;

			OPU1_DemoTimer_Y = new DispatcherTimer();
			OPU1_DemoTimer_Y.Interval = TimeSpan.FromSeconds(70);
			OPU1_DemoTimer_Y.Tick += OPU1_DemoTimer_Y_Tick;

			OPU2_DemoTimer_A = new DispatcherTimer();
			OPU2_DemoTimer_A.Interval = TimeSpan.FromSeconds(14);
			OPU2_DemoTimer_A.Tick += OPU2_DemoTimer_A_Tick;

			OPU2_DemoTimer_E = new DispatcherTimer();
			OPU2_DemoTimer_E.Interval = TimeSpan.FromSeconds(14);
			OPU2_DemoTimer_E.Tick += OPU2_DemoTimer_E_Tick;

			OPU2_DemoTimer_P = new DispatcherTimer();
			OPU2_DemoTimer_P.Interval = TimeSpan.FromSeconds(70);
			OPU2_DemoTimer_P.Tick += OPU2_DemoTimer_P_Tick;

			OPU2_DemoTimer_X = new DispatcherTimer();
			OPU2_DemoTimer_X.Interval = TimeSpan.FromSeconds(30);
			OPU2_DemoTimer_X.Tick += OPU2_DemoTimer_X_Tick;

			OPU2_DemoTimer_Y = new DispatcherTimer();
			OPU2_DemoTimer_Y.Interval = TimeSpan.FromSeconds(70);
			OPU2_DemoTimer_Y.Tick += OPU2_DemoTimer_Y_Tick;
		}
		private void OPU2_DemoTimer_A_Tick(object? sender, EventArgs e)
		{
			if (Client_OPU2 != null)
			{
				if (OPU2_DemoState[0])
				{
					SendCommand_OPU2("MOVE A -5 1");
					OPU2_DemoState[0] = false;
				}
				else
				{
					SendCommand_OPU2("MOVE A 5 1");
					OPU2_DemoState[0] = true;
				}
			}
		}
		private void OPU2_DemoTimer_E_Tick(object? sender, EventArgs e)
		{
			if (Client_OPU2 != null)
			{
				if (OPU2_DemoState[1])
				{
					SendCommand_OPU2("MOVE E -10 2");
					OPU2_DemoState[1] = false;
				}
				else
				{
					SendCommand_OPU2("MOVE E 10 2");
					OPU2_DemoState[1] = true;
				}
			}
		}
		private void OPU2_DemoTimer_P_Tick(object? sender, EventArgs e)
		{
			if (Client_OPU2 != null)
			{
				if (OPU2_DemoState[2])
				{
					SendCommand_OPU2("MOVE P -180 6");
					OPU2_DemoState[2] = false;
				}
				else
				{
					SendCommand_OPU2("MOVE P 180 6");
					OPU2_DemoState[2] = true;
				}
			}
		}
		private void OPU2_DemoTimer_X_Tick(object? sender, EventArgs e)
		{
			if (Client_OPU2 != null)
			{
				if (OPU2_DemoState[3])
				{
					SendCommand_OPU2("MOVE X 550 25");
					OPU2_DemoState[3] = false;
				}
				else
				{
					SendCommand_OPU2("MOVE X 50 25");
					OPU2_DemoState[3] = true;
				}
			}
		}
		private void OPU2_DemoTimer_Y_Tick(object? sender, EventArgs e)
		{
			if (Client_OPU2 != null)
			{
				if (OPU2_DemoState[4])
				{
					SendCommand_OPU2("MOVE Y 2700 25");
					OPU2_DemoState[4] = false;
				}
				else
				{
					SendCommand_OPU2("MOVE Y 1200 25");
					OPU2_DemoState[4] = true;
				}
			}
		}

		private void OPU1_DemoTimer_Y_Tick(object? sender, EventArgs e)
		{
			if (Client_OPU1 != null)
			{
				if (OPU1_DemoState[0])
				{
					SendCommand_OPU1("MOVE P -180 6");
					OPU1_DemoState[0] = false;
				}
				else
				{
					SendCommand_OPU1("MOVE P 180 6");
					OPU1_DemoState[0] = true;
				}
			}
		}

		private void OPU1_DemoTimer_P_Tick(object? sender, EventArgs e)
		{
			if (Client_OPU1 != null)
			{
				if (OPU1_DemoState[1])
				{
					SendCommand_OPU1("MOVE Y 2500 16.66");
					OPU1_DemoState[1] = false;
				}
				else
				{
					SendCommand_OPU1("MOVE Y 1500 16.66");
					OPU1_DemoState[1] = true;
				}
			}
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
			if (Client_OPU1 != null)
			{
				if (Client_OPU1.Connected)
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
							SendCommand_OPU1("LIM? P");
							OPU1_Status++;
							break;
						case 5:
							SendCommand_OPU1("LIM? Y");
							OPU1_Status = 0;
							break;
					}
				}
				else
				{
					AC1.OPU_Connected = false;
				}
			}
		}

		private void OPU2_StatusTimer_Tick(object? sender, EventArgs e)
		{
			if (Client_OPU2 != null)
			{
				if (Client_OPU2.Connected)
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
				else
				{
					AC2.OPU_Connected = false;
				}
			}
		}

		public void SendCommand_OPU1(string Com)
		{
			if (Client_OPU1 != null)
			{
				if (Client_OPU1.Connected)
				{
					WriteByteData = Encoding.UTF8.GetBytes("^" + Com.Replace(',', '.') + "~\r\n");
					Stream_OPU1.Write(WriteByteData, 0, WriteByteData.Length);
				}
			}
		}

		public void SendCommand_OPU2(string Com)
		{
			if (Client_OPU2 != null)
			{
				if (Client_OPU2.Connected)
				{
					WriteByteData = Encoding.UTF8.GetBytes("^" + Com.Replace(',', '.') + "~\r\n");
					Stream_OPU2.Write(WriteByteData, 0, WriteByteData.Length);
				}
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
					AC1.OPU_Connected = true;
					//OPU1_ConnectStatus.Fill = GreenBrush;
					Stream_OPU1 = Client_OPU1.GetStream();
					OPU1_PosTimer.Start();
					await StartReadingOPU1DataAsync();
				}
				else
				{
					//OPU1_ConnectStatus.Fill = RedBrush;
				}
			}
			catch(Exception ex) {
				AC1.OPU_Connected = true;
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
					AC2.OPU_Connected = true;
					Stream_OPU2 = Client_OPU2.GetStream();
					OPU2_PosTimer.Start();
					await StartReadingOPU2DataAsync();
				}
				else
				{
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
								AC1.OPU_Enabled = OPU1_Enabled;
								//OPU1_PowerStatus.Fill = GreenBrush;
								break;
							case "^STOP?:1:1~":
								OPU1_Stopped = true;
								if (OPU1_Zeroing)
								{
									SendCommand_OPU1("MOVE Y 1500 50");
									OPU1_Zeroing = false;
								}
								break;
							case "^FLT?:0:0~":
								AC1.OPU_InError = false;
								//OPU1_ClearStatus.Fill = GreenBrush;
								break;
							default:
								if (Data.Contains("EN?"))
								{
									OPU1_Enabled = false;
									AC1.OPU_Enabled = OPU1_Enabled;
									//OPU1_PowerStatus.Fill = RedBrush;
								}
								if (Data.Contains("STOP?"))
								{
									OPU1_Stopped = false;
									//OPU1_StopStatus.Fill = RedBrush;
									
								}
								if (Data.Contains("FLT?"))
								{
									AC1.OPU_InError = true;
									//OPU1_ClearStatus.Fill= RedBrush;
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

									OPU1_PolCurPos.Content = OPU1_Position[0].ToString("0.00°");

									OPU1_YCurPos.Content = OPU1_Position[1].ToString("0.00 мм");
								}
								if (Data.Contains("LIM?"))
								{
									char Axis = Data[Data.IndexOf(":") - 1];
									string[] Limits = Data.Substring(Data.IndexOf(":") + 1).Split(":");
									OPU1_LowerLimits["PY".IndexOf(Axis)] = float.Parse(Limits[0].Replace('.', ',').Replace("~", string.Empty));
									OPU1_UpperLimits["PY".IndexOf(Axis)] = float.Parse(Limits[1].Replace('.', ',').Replace("~", string.Empty));
									switch("PY".IndexOf(Axis)){
										case 0:
											OPU1_PolLimitsLabel.Content = "[ " + OPU1_LowerLimits["PY".IndexOf(Axis)].ToString() + " ; " + OPU1_UpperLimits["PY".IndexOf(Axis)].ToString() + " ]";
											break;
										case 1:
											OPU1_YLimitsLabel.Content = "[ " + OPU1_LowerLimits["PY".IndexOf(Axis)].ToString() + " ; " + OPU1_UpperLimits["PY".IndexOf(Axis)].ToString() + " ]";
											break;
									}
								}
								break;
						}
					}
					catch (IOException)
					{
						MessageBox.Show("Connection error");
						//OPU1_ConnectStatus.Fill = RedBrush;
						OPU1_StatusTimer.Stop();
						OPU1_PosTimer.Stop();
						break;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Connection Error: {ex.Message} {ex.StackTrace}");
				//OPU1_ConnectStatus.Fill = RedBrush;
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
								AC2.OPU_Enabled = OPU2_Enabled;
								break;
							case "^STOP?:1:1:1:1:1~":
								OPU2_Stopped = true;
								if (OPU2_Zeroing)
								{
									SendCommand_OPU2("MOVE Y 1500 25");
									SendCommand_OPU2("MOVE X 0 25");
									OPU2_Zeroing = false;
								}
								break;
							case "^FLT?:0:0:0:0:0~":
								AC2.OPU_InError = false;
								break;
							default:
								if (Data.Contains("EN?"))
								{
									OPU2_Enabled = false;
									AC2.OPU_Enabled = OPU2_Enabled;
								}
								if (Data.Contains("STOP?"))
								{
									OPU2_Stopped = false;

								}
								if (Data.Contains("FLT?"))
								{
									AC2.OPU_InError = true;
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
									OPU2_AzCurPos.Content = OPU2_Position[0].ToString("0.00°");

									OPU2_UmCurPos.Content = OPU2_Position[1].ToString("0.00°");

									OPU2_PolCurPos.Content = OPU2_Position[2].ToString("0.00°");

									OPU2_XCurPos.Content = OPU2_Position[3].ToString("0.00 мм");

									OPU2_YCurPos.Content = OPU2_Position[4].ToString("0.00 мм");
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
						OPU2_StatusTimer.Stop();
						OPU2_PosTimer.Stop();
						break;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Connection Error: {ex.Message}");
				OPU2_StatusTimer.Stop();
				OPU2_PosTimer.Stop();
			}
		}

		private void OPU1_Connect_Click(object sender, RoutedEventArgs e)
		{
			if (Client_OPU1 != null)
			{
				Client_OPU1.Close();
			}
			else
			{
				ConnectOPU1();
			}
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

				OPU1_DemoTimer_P.Stop();
				OPU1_DemoTimer_Y.Stop();
			}
			else
			{
				SendCommand_OPU1("DIS");
				OPU1_DemoTimer_P.Stop();
				OPU1_DemoTimer_Y.Stop();
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

			OPU1_DemoTimer_P.Stop();
			OPU1_DemoTimer_Y.Stop();
		}

		private void OPU1_Clear_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU1("CLR");
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
			if (Client_OPU2 != null)
			{
				Client_OPU2.Close();
			}
			else
			{
				ConnectOPU2();
			}
		}

		private void OPU2_Power_Click(object sender, RoutedEventArgs e)
		{
			if (!OPU2_Enabled)
			{
				SendCommand_OPU2("EN");
				SendCommand_OPU2("FH");

				OPU2_DemoTimer_A.Stop();
				OPU2_DemoTimer_E.Stop();
				OPU2_DemoTimer_P.Stop();
				OPU2_DemoTimer_X.Stop();
				OPU2_DemoTimer_Y.Stop();
			}
			else
			{
				SendCommand_OPU2("DIS");

				OPU2_DemoTimer_A.Stop();
				OPU2_DemoTimer_E.Stop();
				OPU2_DemoTimer_P.Stop();
				OPU2_DemoTimer_X.Stop();
				OPU2_DemoTimer_Y.Stop();
			}
		}

		private void OPU2_Stop_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU2("STOP");

			OPU2_DemoTimer_A.Stop();
			OPU2_DemoTimer_E.Stop();
			OPU2_DemoTimer_P.Stop();
			OPU2_DemoTimer_X.Stop();
			OPU2_DemoTimer_Y.Stop();
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
			if (double.TryParse(OPU2_UmTarSpd.Text, DoubleFormat, out double Spd))
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

		private void OPU1_Home_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU1("MOVE P 0 6");
			AC1.OPU_Enabled = true;
			OPU1_Zeroing = true;
		}

		private void OPU2_Home_Click(object sender, RoutedEventArgs e)
		{
			SendCommand_OPU2("MOVE A 0 3");
			SendCommand_OPU2("MOVE E 0 3");
			SendCommand_OPU2("MOVE P 0 3");
			OPU2_Zeroing = true;
		}

		private void OPU1_DemoStart_Click(object sender, RoutedEventArgs e)
		{
			if (Client_OPU1 != null)
			{
				if (OPU1_Enabled)
				{
					SendCommand_OPU1("MOVE P -180 6");
					SendCommand_OPU1("MOVE Y 2500 16.66");

					OPU1_DemoTimer_P.Start();
					OPU1_DemoTimer_Y.Start();
				}
			}
		}

		private void OPU2_DemoStart_Click(object sender, RoutedEventArgs e)
		{
			if (Client_OPU2 != null)
			{
				if (OPU2_Enabled)
				{
					OPU2_DemoTimer_A.Start();
					OPU2_DemoTimer_E.Start();
					OPU2_DemoTimer_P.Start();
					OPU2_DemoTimer_X.Start();
					OPU2_DemoTimer_Y.Start();

				}
			}
		}
	}
}
