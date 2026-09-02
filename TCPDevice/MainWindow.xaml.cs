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
using System.Windows.Media.Animation;
using System.Reflection.Metadata;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Timers;


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

		private double MoveTask_CurPos;
		private double MoveTask_EndPos;
		private double MoveTask_Step;
		private EventHandler MoveTask_Handler; 

		private bool _IsAnimating_TotalProgress = false;
		private bool _IsAnimating_StepProgress = false;
		private double _APFC_CanvasScale = 0.2;
		private const double _APFC_CanvasZoomSpeed = 0.1;
		private const double _APFC_CanvasMaxScale = 2.0;
		private const double _APFC_CanvasMinScale = 0.2;
		private double _APFC_HorScrollPos = 0.0;
		private double _APFC_VerScrollPos = 0.0;
		private bool _AttestIsRunning = false;
		private bool _APFC_Running = false;

		private bool _Demo_Running = false;
		private int DemoType = -1;
		private object PreviousDemo;

		private List<double> APFC_VelList = new List<double>();//new List<double>()
		private double APFC_TimeMeas = 4.0;
		private List<double> APFC_XAxis = new List<double>();

		private DispatcherTimer OPU1_StatusTimer = new DispatcherTimer();
		private DispatcherTimer OPU_PosTimer = new DispatcherTimer();
		private DispatcherTimer OPU_VelTimer = new DispatcherTimer();
		private DispatcherTimer StepTimer = new DispatcherTimer();
		private DispatcherTimer MoveTaskTimer = new DispatcherTimer();
		private int OPU1_Status = 0;
		private byte[] WriteByteData;
		private int ValidationFlags = 0b_111111111111;
		private bool IsStepping = false;

		private NumberFormatInfo DoubleFormat = new NumberFormatInfo();

		private Logger DataLog;
		public MainWindow()
        {
            InitializeComponent();
			DoubleFormat.NumberDecimalSeparator = ".";

			if (AppSettings.LogPath != null)
			{
				DataLog = new Logger(AppSettings.LogPath);
			}
			else
			{
				DataLog = new Logger();
			}

				OPU1_StatusTimer.Interval = TimeSpan.FromMilliseconds(100);
			OPU1_StatusTimer.Tick += OPU1_StatusTimer_Tick;

			OPU_PosTimer.Interval = TimeSpan.FromMilliseconds(75);
			OPU_PosTimer.Tick += OPU_PosTimer_Tick;

			OPU_VelTimer.Interval = TimeSpan.FromMilliseconds(150);
			OPU_VelTimer.Tick += OPU_VelTimer_Tick;

			StepTimer.Tick += StepTimer_Tick;
		}

		private void OPU_VelTimer_Tick(object? sender, EventArgs e)
		{
			if (Client_OPU1 != null)
			{
				SendCommand("VEL?");
			}
		}

		private void StepTimer_Tick(object? sender, EventArgs e)
		{
			if (Client_OPU1 != null)
			{
				SendCommand($"MOVER {Step_TgtStep.Text.Replace(',', '.')} {Step_TgtVel.Text.Replace(',', '.')} {Step_TgtAcc.Text.Replace(',', '.')}");
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
						OPU1_Status++;
						break;
					default:
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
				IPAddress Address = IPAddress.Parse("192.168.0.100");
				int Port = 2000;
				Client_OPU1 = new TcpClient(Address.ToString(), Port);
				DataLog.WriteLog(Logger.LogType.INFO_LOG, "Подключение");

				if (Client_OPU1.Connected)
				{
					//ConnectStatus.Fill = GreenBrush;
					Stream_OPU1 = Client_OPU1.GetStream();
					ConnectionCheck.IsChecked = true;
					OPU_PosTimer.Start();
					OPU_VelTimer.Start();
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
						foreach (string SubData in Data.Split('^'))
						{
							switch (SubData)
							{
								case "EN?:1~":
									PowerCheck.IsChecked = true;
									break;
								case "STOP?:1~":
									MovingCheck.IsChecked = true;
									MoveStatus.Fill = Brushes.Green;
									if (IsStepping)
									{
										StepTimer.Start();
									}
									if (_AttestIsRunning)
									{
										MoveTaskTimer.Start();
									}
									break;
								case "FLT?:0~":
									InFaultCheck.IsChecked = false;
									break;
								case "INIT?:1~":
									InitCheck.IsChecked = true;
									break;
								default:
									if (SubData.Contains("EN?"))
									{
										PowerCheck.IsChecked = false;
										//IsStepping = false;
										//_AttestIsRunning = false;
										StepTimer.Stop();
										MoveTaskTimer.Stop();
									}
									if (SubData.Contains("STOP?"))
									{
										MovingCheck.IsChecked = false;
										MoveStatus.Fill = Brushes.Red;
									}
									if (SubData.Contains("FLT?"))
									{
										InFaultCheck.IsChecked = true;
										//IsStepping = false;
										//_AttestIsRunning = false;
										StepTimer.Stop();
										MoveTaskTimer.Stop();
									}
									if (SubData.Contains("INIT?"))
									{
										InitCheck.IsChecked = false;
										//IsStepping = false;
										//_AttestIsRunning = false;
										StepTimer.Stop();
										MoveTaskTimer.Stop();
									}
									if (SubData.Contains("POS?"))
									{
										string Position = SubData.Substring(SubData.IndexOf(":") + 1, SubData.IndexOf('~') - SubData.IndexOf(':') - 1);
										OPU1_Position = double.Parse(Position, DoubleFormat);
										Info_CurPos.Text = OPU1_Position.ToString();
									}
									if (SubData.Contains("VEL?"))
									{
										string Velocity = SubData.Substring(SubData.IndexOf(":") + 1, SubData.IndexOf('~') - SubData.IndexOf(':') - 1);
										OPU1_Velocity = double.Parse(Velocity, DoubleFormat);
										Info_CurVel.Text = OPU1_Velocity.ToString();
									}
									if (SubData.Contains("APFCD?"))
									{
										string Velocity = SubData.Substring(SubData.IndexOf(":") + 1, SubData.IndexOf('~') - SubData.IndexOf(':') - 1);
										APFC_VelList.Add(double.Parse(Velocity, DoubleFormat));
									}
									if (SubData.Contains("APFCS?"))
									{
										string[] Replies = SubData.Split(':');
										if (Replies[1] == "0")
										{
											_APFC_Running = false;
										}
										if (int.Parse(Replies[2]) != APFC_VelList.Count)
										{
											DataLog.WriteLog(Logger.LogType.INFO_LOG, $"Количество принятых ответов не совпало с отправленными: отправлено {int.Parse(Replies[2])}, принято {APFC_VelList.Count}.");
										}
										APFC_TimeMeas = 0.004; /*double.Parse(Replies[3].Replace("~",string.Empty), DoubleFormat);*/
										APFC_DrawCanvas(APFC_VelList);
										OPU_PosTimer.Start();
										OPU_VelTimer.Start();
									}
									break;
							}
						}
					}
					catch (IOException)
					{
						MessageBox.Show("Connection error");
						DataLog.WriteLog(Logger.LogType.ERR_LOG, "Ошибка подключения при чтении");
						//ConnectStatus.Fill = RedBrush;
						ConnectionCheck.IsChecked = false;
						OPU_PosTimer.Stop();
						OPU_VelTimer.Stop();
						OPU1_StatusTimer.Stop();
						break;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Connection Error: {ex.Message}\n{ex.StackTrace}");
				DataLog.WriteLog(Logger.LogType.ERR_LOG, "Ошибка подключения при чтении");
				//ConnectStatus.Fill = RedBrush;
				ConnectionCheck.IsChecked = false;
				OPU_PosTimer.Stop();
				OPU_VelTimer.Stop();
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
					Client_OPU1.Close();
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
				_APFC_Running = false;
				_AttestIsRunning = false;
				if (PowerCheck.IsChecked == true)
				{
					SendCommand($"MOVEA {Abs_TgtPos.Text.Replace(',','.')} {Abs_TgtVel.Text.Replace(',', '.')} {Abs_TgtAcc.Text.Replace(',', '.')}");
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
				_APFC_Running = false;
				_AttestIsRunning = false;
				if (PowerCheck.IsChecked == true)
				{
					SendCommand($"MOVER {Inc_TgtPos.Text.Replace(',', '.')} {Inc_TgtVel.Text.Replace(',', '.')} {Inc_TgtAcc.Text.Replace(',', '.')}");
				}
				else
				{
					MessageBox.Show("Нет Питания!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
		}

		private void Step_StartMove_Click(object sender, RoutedEventArgs e)
		{
			if (Client_OPU1 != null)
			{
				IsStepping = false;
				_APFC_Running = false;
				_AttestIsRunning = false;
				if (PowerCheck.IsChecked == true)
				{
					StepTimer.Interval = TimeSpan.FromMilliseconds(double.Parse(Step_TgtPause.Text, DoubleFormat) * 1000.0);
					SendCommand($"MOVER {Step_TgtStep.Text.Replace(',', '.')} {Step_TgtVel.Text.Replace(',', '.')} {Step_TgtAcc.Text.Replace(',', '.')}");
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
				_APFC_Running = false;
				_AttestIsRunning = false;
				if (PowerCheck.IsChecked == true)
				{
					SendCommand($"MOVEV {Vel_TgtVel.Text.Replace(',', '.')} {Vel_TgtAcc.Text.Replace(',', '.')}");
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
				_APFC_Running = false;
				_AttestIsRunning = false;
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
			Abs_StartMove.IsEnabled = (ValidationFlags & ABS_VALID) != 0 ? false : true;
			Inc_StartMove.IsEnabled = (ValidationFlags & INC_VALID) != 0 ? false : true;
			Step_StartMove.IsEnabled = (ValidationFlags & STEP_VALID) != 0 ? false : true;
			Vel_StartMove.IsEnabled = (ValidationFlags & VEL_VALID) != 0 ? false : true;
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
			if (Client_OPU1 != null)
			{
				if (Client_OPU1.Connected)
				{
					if (!_AttestIsRunning)
					{
						SendCommand("DIS");
						_AttestIsRunning = true;
						_APFC_Running = false;
						IsStepping = false;
						MessageBox.Show("Запущен режим аттестации!\nПодайте питание и совершите инициализацию для продолжения!","Внимание!",MessageBoxButton.OK, MessageBoxImage.Exclamation);
					}
				}
			}
		}
		private void Attest_Variant_DigMeasSystErr_Next_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (Client_OPU1 != null)
				{
					if (Client_OPU1.Connected)
					{
						if (_AttestIsRunning)
						{
							if (PowerCheck.IsChecked != true)
							{
								throw new Exception("Не подано питание при аттестации!");
							}
							if (InitCheck.IsChecked != true)
							{
								throw new Exception("Не проведена инициализация при аттестации!");
							}
							if (MoveTask_Handler != null)
							{
								MoveTaskTimer.Tick -= MoveTask_Handler;
							}
							MoveTask_Handler = (sender, e) =>
							{
								if (MoveTask_CurPos + MoveTask_Step <= MoveTask_EndPos)
								{
									MoveTask_CurPos += MoveTask_Step;
									SendCommand($"MOVER {MoveTask_Step} 6 6");
									Attest_Variant_DigMeasSystErr_StepProgress.Value++;
									MoveTaskTimer.Stop();
								}
								else
								{
									Attest_Variant_DigMeasSystErr_TotalProgress.Value++;
									MoveTaskTimer.Stop();
									MessageBox.Show("Этап закончен, наимте \"Далее\" для продолжения");
									if (Attest_Variant_DigMeasSystErr_TotalProgress.Value == 4)
									{
										_AttestIsRunning = false;
									}
								}
							};
							
							switch (Attest_Variant_DigMeasSystErr_TotalProgress.Value){
								case 0:
								{
									MoveTask_CurPos = 0;
									MoveTask_EndPos = 360;
									MoveTask_Step = 5;
									MoveTaskTimer.Start();
								} break;
								case 1:
								{
									MoveTask_CurPos = 360;
									MoveTask_EndPos = 0;
									MoveTask_Step = -5;
									MoveTaskTimer.Start();
								}
								break;
								case 2:
								{
									MoveTask_CurPos = 0;
									MoveTask_EndPos = -360;
									MoveTask_Step = -5;
									MoveTaskTimer.Start();
								}
								break;
								case 3:
								{
									
									MoveTask_CurPos = -360;
									MoveTask_EndPos = 0;
									MoveTask_Step = 5;
									MoveTaskTimer.Start();
								}
								break;
							}
						}
						else
						{
							throw new Exception("Не запущена аттестация!");
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message,"Внимание",MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Attest_Variant_DigMeasSystErr_Stop_Click(object sender, RoutedEventArgs e)
		{
			_AttestIsRunning = false;
			MoveTaskTimer.Stop();
			MoveTaskTimer.Tick -= MoveTask_Handler;
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

		private void EmStop_Click(object sender, RoutedEventArgs e)
		{

		}

		private void TotalProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_IsAnimating_TotalProgress)
			{
				return;
			}
			_IsAnimating_TotalProgress = true;
			DoubleAnimation DAT = new DoubleAnimation(e.OldValue, e.NewValue, new Duration(TimeSpan.FromMilliseconds(700)), FillBehavior.Stop);
			ExponentialEase EF = new ExponentialEase();
			EF.Exponent = 2;
			EF.EasingMode = EasingMode.EaseOut;
			DAT.EasingFunction = EF;
			DAT.Completed += DAT_Completed;
			((ProgressBar)sender).BeginAnimation(ProgressBar.ValueProperty, DAT);
			

			e.Handled = true;
		}

		private void StepProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_IsAnimating_StepProgress)
			{
				return;
			}
			_IsAnimating_StepProgress = true;
			DoubleAnimation DAS = new DoubleAnimation(e.OldValue, e.NewValue, new Duration(TimeSpan.FromMilliseconds(700)),FillBehavior.Stop);
			DAS.Completed += DAS_Completed;
			ExponentialEase EF = new ExponentialEase();
			EF.Exponent = 2;
			EF.EasingMode = EasingMode.EaseOut;
			DAS.EasingFunction = EF;
			((ProgressBar)sender).BeginAnimation(ProgressBar.ValueProperty, DAS);
			

			e.Handled = true;
		}

		private void DAS_Completed(object? sender, EventArgs e)
		{
			_IsAnimating_StepProgress = false;
		}

		private void DAT_Completed(object? sender, EventArgs e)
		{
			_IsAnimating_TotalProgress = false;
		}

		private void window_KeyUp(object sender, KeyEventArgs e)
		{
			
			switch (e.Key)
			{
				case Key.OemPlus:
					Attest_Variant_DigMeasSystErr_TotalProgress.Value += 1;
					break;
				case Key.OemMinus:
					Attest_Variant_DigMeasSystErr_TotalProgress.Value -= 1;
					break;
				case Key.Return:
					APFC_DrawCanvas(APFC_XAxis);
					break;
			}
		}

		public static double Map(double value, double inputMin, double inputMax, double outputMin, double outputMax)
		{
			return ((value - inputMin) / (inputMax - inputMin)) * (outputMax - outputMin) + outputMin;
		}

		private void APFC_DrawCanvas(List<double> Points)
		{
			try
			{
				APFC_Canvas.Children.Clear();
				Line YAxis = new Line();
				YAxis.X1 = 50;
				YAxis.Y1 = 50;
				YAxis.X2 = 50;
				YAxis.Y2 = 1450;

				Line XAxis = new Line();
				XAxis.X1 = 50;
				XAxis.Y1 = 1450;
				XAxis.X2 = 3450;
				XAxis.Y2 = 1450;

				Line Zero = new Line();
				Zero.X1 = 50;
				Zero.Y1 = 750;
				Zero.X2 = 3450;
				Zero.Y2 = 750;
				Zero.Opacity = 0.2;
				Zero.StrokeDashArray = new DoubleCollection { 6, 12 };

				APFC_Canvas.Children.Add(YAxis);
				APFC_Canvas.Children.Add(XAxis);
				APFC_Canvas.Children.Add(Zero);
				if (APFC_VelList.Count > 0)
				{
					APFC_XAxis.Clear();
					double Frequency = double.Parse(APFC_TgtFreq.Text, DoubleFormat);
					double Amplitude = double.Parse(APFC_TgtAmp.Text, DoubleFormat);
					int numPeriods = int.Parse(APFC_Periods.Text);
					double Period = 1.0 / Frequency;
					//Measured graph
					for (int Index = 0; /*Convert.ToDouble(Index) * APFC_TimeMeas <= Period &&*/ Index < APFC_VelList.Count; Index++)
					{
						double X = Map(Index * APFC_TimeMeas, 0, Period * numPeriods, 100, 3400);
						double Y = Map(APFC_VelList[Index], 0, Amplitude * 3, 1400, 100);
						Point P = new Point(X, Y);
						Ellipse E = new Ellipse();
						ToolTip TT = new ToolTip();
						TT.Content = (Index * APFC_TimeMeas).ToString() + " ; " + (APFC_VelList[Index]).ToString();
						E.ToolTip = TT;
						E.Width = 20;
						E.Height = 20;
						E.Fill = Brushes.Black;
						Canvas.SetLeft(E, P.X - 10);
						Canvas.SetTop(E, P.Y - 10);

						APFC_Canvas.Children.Add(E);
					}
					//Perfect graph
					for (int Index = 0; Convert.ToDouble(Index) * APFC_TimeMeas <= Period * numPeriods; Index++)
					{
						double X = Map(Index * APFC_TimeMeas, 0, Period * numPeriods, 100, 3400);
						double Y = Map(Amplitude - Amplitude * Math.Cos(2 * Math.PI * Frequency * Index * APFC_TimeMeas), 0, Amplitude * 3, 1400, 100);
						Point P = new Point(X, Y);
						Ellipse E = new Ellipse();
						ToolTip TT = new ToolTip();
						TT.Content = (Index * APFC_TimeMeas).ToString() + " ; " + (Amplitude - Amplitude * Math.Cos(2 * Math.PI * Frequency * Index * APFC_TimeMeas)).ToString();
						E.ToolTip = TT;
						E.Width = 20;
						E.Height = 20;
						E.Fill = Brushes.Blue;
						Canvas.SetLeft(E, P.X - 10);
						Canvas.SetTop(E, P.Y - 10);

						APFC_Canvas.Children.Add(E);
					}

					//Mark Peaks
				}

				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void APFC_Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			_APFC_CanvasScale += e.Delta > 0 ? _APFC_CanvasZoomSpeed : -_APFC_CanvasZoomSpeed;
			
			_APFC_CanvasScale = Math.Min(_APFC_CanvasMaxScale, _APFC_CanvasScale);
			_APFC_CanvasScale = Math.Max(_APFC_CanvasMinScale, _APFC_CanvasScale);
			CanvasSize.Content = "%" + Math.Round(_APFC_CanvasScale * 500).ToString();
			Point MousePoint = e.GetPosition(APFC_Canvas);
			APFC_Canvas.RenderTransformOrigin = new Point(MousePoint.X / APFC_Canvas.ActualWidth, MousePoint.Y / APFC_Canvas.ActualHeight);
			ScaleTransform ST = new ScaleTransform(_APFC_CanvasScale, _APFC_CanvasScale);
			APFC_Canvas.LayoutTransform = ST;

			APFC_ScrollView.ScrollToHorizontalOffset(_APFC_HorScrollPos);
			APFC_ScrollView.ScrollToVerticalOffset(_APFC_VerScrollPos);
		}

		private void APFC_Canvas_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			_APFC_HorScrollPos = APFC_ScrollView.HorizontalOffset;
			_APFC_VerScrollPos = APFC_ScrollView.VerticalOffset;
		}

		private void APFC_Start_Click(object sender, RoutedEventArgs e)
		{
			if (APFC_TgtAmp.Text != string.Empty && APFC_TgtFreq.Text != string.Empty && APFC_Periods.Text != string.Empty)
			{
				double D = double.Parse(APFC_TgtAmp.Text, DoubleFormat);
				double F = double.Parse(APFC_TgtFreq.Text, DoubleFormat);
				double V = D * 2 * Math.PI * F;
				double A = 2 * Math.PI * F * V;
				int K = int.Parse(APFC_Periods.Text);

				if (V < 0.1 || V > 7200.0 || A < 0.1 || A> 3000.0)
				{
					MessageBox.Show($"Введённые значения не удовлетворяют условиям:\nV = 2 * π * F * D = {V} [0.1; 7200.0]\nA = 2 * π * F * V = {A} [0.1; 3000.0]");
					return;
				}
				else
				{
					if (!_APFC_Running)
					{
						OPU_PosTimer.Stop();
						OPU_VelTimer.Stop();
						SendCommand("DIS");
						_APFC_Running = true;
						MessageBox.Show("Запущен режим снятия АФЧХ!\n1. Установите полезную нагрузку\n2. Подайте питание\n3. Проведите инициализацию\n4. Нажмите на кнопку \"Старт\"");
					}
					else
					{
						try
						{
							if (PowerCheck.IsChecked == false)
							{
								throw new Exception("Не подано питание при снятии АФЧХ!");
							}
							if (InitCheck.IsChecked == false)
							{
								throw new Exception("Не проведена инициализация при снятии АФЧХ!");
							}
							SendCommand($"APFC {V} {F} {K}");
						}
						catch (Exception ex)
						{
							MessageBox.Show(ex.Message);
						}
					}
				}
			}
		}

		private void APFC_Stop_Click(object sender, RoutedEventArgs e)
		{
			SendCommand("STOP");
			_APFC_Running = false;
		}

		private void APFC_LostFocus(object sender, RoutedEventArgs e)
		{
			if (((TextBox)sender).Text != string.Empty)
			{
				if (double.TryParse(((TextBox)sender).Text, DoubleFormat, out double D))
				{
					((TextBox)sender).BorderBrush = new SolidColorBrush(Color.FromArgb(0xff, 0xAB, 0xAd, 0xB3));
				}
				else
				{
					((TextBox)sender).BorderBrush = Brushes.Red;
				}
			}
			else
			{
				((TextBox)sender).BorderBrush = Brushes.Red;
			}
		}

		private void Demo_Choice(object sender, RoutedEventArgs e)
		{
			if (PreviousDemo != null)
			{
				DemoType = int.Parse((string)((Button)sender).Tag);
				((Button)PreviousDemo).Background = new SolidColorBrush(Color.FromRgb(0x02, 0x0c, 0x0b));
				((Button)sender).Background = Brushes.LightBlue;
				PreviousDemo = sender;
			}
			else
			{
				DemoType = int.Parse((string)((Button)sender).Tag);
				((Button)sender).Background = Brushes.LightBlue;
				PreviousDemo = sender;
			}
		}
	}
}
//TODO:
//EN - предупреждение о движении в ноль
//INIT - предупредение о резком старте движения
//SH - предупреждение о снятии и подаче питания на позиционер
