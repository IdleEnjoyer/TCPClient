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
using Microsoft.Win32;
using WindowsAPICodePack.Dialogs;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace TCPDevice
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		Properties.Settings AppSettings = new Properties.Settings();

		private const int ABS_VALID  = 0b_000000000111;
		private const int INC_VALID  = 0b_000000111000;
		private const int STEP_VALID = 0b_001111000000;
		private const int VEL_VALID  = 0b_110000000000;

		private TcpClient Client_OPU1;
		private NetworkStream Stream_OPU1;
		private double OPU1_Position;
		private double OPU1_Velocity;

		private DispatcherTimer OPU1_StatusTimer = new DispatcherTimer();
		private DispatcherTimer OPU_PosTimer = new DispatcherTimer();
		private DispatcherTimer StepTimer = new DispatcherTimer();
		private int OPU1_Status = 0;
		private byte[] WriteByteData;
		private int ValidationFlags = 0b_111111111111;
		private bool IsStepping = false;

		private RadialGradientBrush GreenBrush = new(Color.FromRgb(255, 255, 255), Color.FromRgb(0, 255, 0));
		private RadialGradientBrush RedBrush = new(Color.FromRgb(255, 255, 255), Color.FromRgb(255, 0, 0));
		private NumberFormatInfo DoubleFormat = new NumberFormatInfo();

		private Logger DataLog = new Logger();
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

			StepTimer.Tick += StepTimer_Tick;
		}

		private void StepTimer_Tick(object? sender, EventArgs e)
		{
			if (Client_OPU1 != null)
			{
				SendCommand($"MOVER {Step_TgtStep} {Step_TgtVel} {Step_TgtAcc}");
				StepTimer.Stop();
			}
		}

		private void OPU_PosTimer_Tick(object? sender, EventArgs e)
		{
			if (Client_OPU1 != null) {
				SendCommand("POS?");
			}
		}

		private void OPU1_StatusTimer_Tick(object? sender, EventArgs e)
		{
			if (Client_OPU1 != null)
			{
				switch (OPU1_Status)
				{
					case 0:
						SendCommand("EN?");
						OPU1_Status++;
						break;
					case 1:
						SendCommand("FH?");
						OPU1_Status++;
						break;
					case 2:
						SendCommand("INIT?");
						OPU1_Status++;
						break;
					case 3:
						SendCommand("STOP?");
						OPU1_Status++;
						break;
					case 4:
						SendCommand("FLT?");
						OPU1_Status = 0;
						break;
				}
			}
		}

		public void SendCommand(string Com)
		{
			if (Client_OPU1.Connected)
			{
				WriteByteData = Encoding.UTF8.GetBytes("^" + Com.Replace(',','.') + "~\r\n");
				Stream_OPU1.Write(WriteByteData, 0, WriteByteData.Length);
				DataLog.WriteLog(Logger.LogType.SEND_LOG, "^" + Com.Replace(',', '.') + "~\r\n");
			}
		}

		private async void ConnectOPU()
		{
			try
			{
				IPAddress Address = IPAddress.Parse("192.168.0.101");
				int Port = 2000;
				Client_OPU1 = new TcpClient(Address.ToString(), Port);
				DataLog.WriteLog(Logger.LogType.INFO_LOG, "Подключение");

				if (Client_OPU1.Connected)
				{
					//ConnectStatus.Fill = GreenBrush;
					Stream_OPU1 = Client_OPU1.GetStream();
					ConnectionCheck.IsChecked = true;
					OPU_PosTimer.Start();
					OPU1_StatusTimer.Start();
					DataLog.WriteLog(Logger.LogType.INFO_LOG, "Подключено!");
					await StartReadingDataAsync();
				}
				else
				{
					//ConnectStatus.Fill = RedBrush;
					ConnectionCheck.IsChecked = false;
					DataLog.WriteLog(Logger.LogType.ERR_LOG, "Не удалось подключиться");
				}
			}
			catch(Exception ex) { 
				MessageBox.Show(ex.Message);
				DataLog.WriteLog(Logger.LogType.ERR_LOG, "Не удалось подключиться");
				ConnectionCheck.IsChecked = false;
			}
		}

		private async Task StartReadingDataAsync()
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
						DataLog.WriteLog(Logger.LogType.GET_LOG, Data);
						switch (Data)
						{
							case "^EN?:1~":
								PowerCheck.IsChecked = true;
								break;
							case "^STOP?:1~":
								MovingCheck.IsChecked = true;
								MoveStatus.Fill = Brushes.Green;
								if (IsStepping && StepTimer.IsEnabled)
								{
									StepTimer.Stop();
								}
								break;
							case "^FLT?:0~":
								InFaultCheck.IsChecked = false;
								break;
							case "^INIT?:1~":
								InitCheck.IsChecked = true;
								break;
							default:
								if (Data.Contains("EN?"))
								{
									PowerCheck.IsChecked = false;
									IsStepping = false;
								}
								if (Data.Contains("STOP?"))
								{
									MovingCheck.IsChecked = false;
									MoveStatus.Fill = Brushes.Red;
									if (IsStepping)
									{
										StepTimer.Start();
									}
								}
								if (Data.Contains("FLT?"))
								{
									InFaultCheck.IsChecked = true;
									IsStepping = false;
								}
								if (Data.Contains("INIT?"))
								{
									InitCheck.IsChecked = false;
									IsStepping = false;
								}
								if (Data.Contains("POS?"))
								{
									string Position = Data.Substring(Data.IndexOf(":") + 1).Replace("~", string.Empty);
									OPU1_Position = double.Parse(Position, DoubleFormat);
									Info_CurPos.Text = OPU1_Position.ToString();
								}
								if (Data.Contains("VEL?"))
								{
									string Velocity = Data.Substring(Data.IndexOf(":") + 1).Replace("~", string.Empty);
									OPU1_Velocity = double.Parse(Velocity, DoubleFormat);
									Info_CurVel.Text = OPU1_Velocity.ToString();
								}
								break;
						}
					}
					catch (IOException)
					{
						MessageBox.Show("Connection error");
						DataLog.WriteLog(Logger.LogType.ERR_LOG, "Ошибка подключения при чтении");
						//ConnectStatus.Fill = RedBrush;
						ConnectionCheck.IsChecked = false;
						OPU_PosTimer.Stop();
						OPU1_StatusTimer.Stop();
						break;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Connection Error: {ex.Message}");
				DataLog.WriteLog(Logger.LogType.ERR_LOG, "Ошибка подключения при чтении");
				//ConnectStatus.Fill = RedBrush;
				ConnectionCheck.IsChecked = false;
				OPU_PosTimer.Stop();
				OPU1_StatusTimer.Stop();
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (Client_OPU1 != null)
			{
				if (Client_OPU1.Connected)
				{
					SendCommand("DIS");
				}
			}
			DataLog.Dispose();
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
			if (Client_OPU1 != null)
			{
				IsStepping = false;
				if (PowerCheck.IsChecked == false)
				{
					if (MessageBox.Show("ВНИМАНИЕ!\n\nПосле подачи питания начнётся плавное движение в позицию нуля!\n\nПродолжить?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
					{
						SendCommand("EN");
					}
				}
				else
				{
					SendCommand("DIS");
				}
			}
			
			
		}

		private void Connection_Click(object sender, RoutedEventArgs e)
		{
			if (Client_OPU1 == null)
			{
				ConnectOPU();
			}
			else
			{
				IsStepping = false;
				Client_OPU1.Close();
				DataLog.WriteLog(Logger.LogType.INFO_LOG, "Отключение пользователем от сервера");
			}
		}

		private void Fault_Click(object sender, RoutedEventArgs e)
		{
			if (Client_OPU1 != null)
			{
				SendCommand("CLR");
			}
		}

		private void Abs_StartMove_Click(object sender, RoutedEventArgs e)
		{
			if (Client_OPU1 != null)
			{
				IsStepping = false;
				if (PowerCheck.IsChecked == true)
				{
					SendCommand($"MOVEA {Abs_TgtPos} {Abs_TgtVel} {Abs_TgtAcc}");
				}
				else
				{
					MessageBox.Show("Нет Питания!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
        }

		private void Inc_StartMove_Click(object sender, RoutedEventArgs e)
		{
			if (Client_OPU1 != null)
			{
				IsStepping = false;
				if (PowerCheck.IsChecked == true)
				{
					SendCommand($"MOVER {Inc_TgtPos} {Inc_TgtVel} {Inc_TgtAcc}");
				}
				else
				{
					MessageBox.Show("Нет Питания!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
		}

		private void Step_StartMove_Click(object sender, RoutedEventArgs e)
		{
			if (Client_OPU1 == null)
			{
				if (PowerCheck.IsChecked == true)
				{
					StepTimer.Interval = TimeSpan.FromMilliseconds(double.Parse(Step_TgtPause.Text) * 1000.0);
					SendCommand($"MOVER {Step_TgtStep} {Step_TgtVel} {Step_TgtAcc}");
					IsStepping = true;
				}
				else
				{
					MessageBox.Show("Нет Питания!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
			
		}

		private void Vel_StartMove_Click(object sender, RoutedEventArgs e)
		{
			if (Client_OPU1 != null)
			{
				IsStepping = false;
				if (PowerCheck.IsChecked == true)
				{
					SendCommand($"MOVEV {Vel_TgtVel} {Vel_TgtAcc}");
				}
				else
				{
					MessageBox.Show("Нет Питания!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
		}

		private void StopMove_Click(object sender, RoutedEventArgs e)
		{
			if (Client_OPU1 != null)
			{
				SendCommand("STOP");
				IsStepping = false;
			}
		}

		private void NumberInputTextbox_LostFocus(object sender, RoutedEventArgs e)
		{
			double Num;
			if (!double.TryParse(((TextBox)sender).Text, DoubleFormat, out Num))
			{
				((TextBox)sender).BorderBrush = Brushes.Red;
				ValidationFlags |= int.Parse((string)(((TextBox)sender).Tag));
			}
			else
			{
				((TextBox)sender).BorderBrush = new SolidColorBrush(Color.FromArgb(0xff, 0xAB, 0xAd, 0xB3));
				ValidationFlags &= (ABS_VALID | INC_VALID | STEP_VALID | VEL_VALID) - int.Parse((string)((TextBox)sender).Tag);
			}
            if ((ValidationFlags & ABS_VALID) != 0)
            {
				Abs_StartMove.IsEnabled = false;
            }
			if ((ValidationFlags & INC_VALID) != 0)
			{
				Inc_StartMove.IsEnabled = false;
			}
			if ((ValidationFlags & STEP_VALID) != 0)
			{
				Step_StartMove.IsEnabled = false;
			}
			if ((ValidationFlags & VEL_VALID) != 0)
			{
				Vel_StartMove.IsEnabled = false;
			}
		}

		private void TreeViewItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
		{
			e.Handled = true;
		}

		private void Init_Click(object sender, RoutedEventArgs e)
		{
			if (Client_OPU1 != null)
			{
				if (MessageBox.Show("При инииализации будет совершено быстрое резкое движение!\n\nПродолжить?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
				{
					SendCommand("INIT");
				}
			}
		}

		private void Attest_Variant_DigMeasSystErr_Start_Click(object sender, RoutedEventArgs e)
		{

		}

		private void MenuLogging_Click(object sender, RoutedEventArgs e)
		{
			
		}

		private void LoggingFolderChange_Click(object sender, RoutedEventArgs e)
		{
			CommonOpenFileDialog COFD = new CommonOpenFileDialog();
			COFD.Multiselect = false;
			COFD.IsFolderPicker = true;
			if (COFD.ShowDialog() == CommonFileDialogResult.Ok)
			{
				AppSettings.LogPath = COFD.FileName;
				AppSettings.Save();
				DataLog.WriteLog(Logger.LogType.INFO_LOG, $"Изменение папки хранения логов: {COFD.FileName}");
				DataLog.Dispose();

				DataLog = new Logger(AppSettings.LogPath);
			}
			
		}
	}
}
//TODO:
//EN - предупреждение о движении в ноль
//INIT - предупредение о резком старте движения
//SH - предупреждение о снятии и подаче питания на позиционер
