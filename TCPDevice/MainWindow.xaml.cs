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
		private float[] OPU1_LowerLimits = { -130.0f, 130.0f };
		private float[] OPU1_UpperLimits = { -105.0f, 105.0f };
		private bool OPU1_Enabled = false;
		private double[] OPU1_Position = { 0.0, 0.0 };
		private double[] OPU1_TargetPos = { 0.0, 0.0 };
		private List<List<Point>> Trajectories = new List<List<Point>>( new List<Point>[] {
			new List<Point>(new Point[] { new Point(100.0, 100.0), new Point(-100.0, 100.0), new Point(-100.0, -100.0), new Point(100.0, -100.0) }),
			new List<Point>(new Point[] { new Point(100.0, 100.0), new Point(-100.0, 100.0), new Point(100.0, -100.0), new Point(-100.0, -100.0) }),
			new List<Point>(new Point[] { new Point(0.0, 100.0), new Point(-100.0, -100.0), new Point(100.0, -100.0) }),
			new List<Point>(new Point[] { new Point(0.0, 100.0), new Point(66.66, -100.0), new Point(-100.0, 66.66), new Point(100.0, 66.66), new Point(-66.66, -100.0) }),
			new List<Point>(new Point[] { new Point(-100.0, 0.0), new Point(-66.66, 50.0), new Point(-33.33, 100.0), new Point(33.33, 100.0), new Point(66.66, 50.0), new Point(100.0, 0.0), new Point(66.66, -50.0), new Point(33.33, -100.0), new Point(-33.33, -100.0), new Point(-66.66, -50.0) })
		});
		enum Trajectory
		{
			Square,
			Hourglass,
			Triangle,
			Star,
			Circle
		}
		
		private bool OPU1_Stopped = false;
		private bool OPU1_InError = false;
		private DispatcherTimer Trajectory_Timer = new DispatcherTimer();
		private int Current_Point;
		private Trajectory Current_Trajectory;
		private DispatcherTimer Move_Timer = new DispatcherTimer();
		private DispatcherTimer Status_Timer = new DispatcherTimer();
		private int Status = 0;

		private byte[] WriteByteData;
		public string Axis_OPU1 = "";
		private NumberFormatInfo DoubleFormat = new NumberFormatInfo();

		private bool isDragging = false;
		public MainWindow()
        {
            InitializeComponent();
			DoubleFormat.NumberDecimalSeparator = ".";
			Status_Timer.Interval = TimeSpan.FromMilliseconds(50);
			Status_Timer.Tick += Status_Timer_Tick;
			Move_Timer.Interval = TimeSpan.FromMilliseconds(100);
			Move_Timer.Tick += Move_Timer_Tick;
			Trajectory_Timer.Interval = TimeSpan.FromMilliseconds(50);
			Trajectory_Timer.Tick += Trajectory_Timer_Tick;
		}

		private void Trajectory_Timer_Tick(object? sender, EventArgs e)
		{
			if (Math.Abs(OPU1_Position[0] - Trajectories[(int)Current_Trajectory][Current_Point].X) <= 0.1 & Math.Abs(OPU1_Position[1] - Trajectories[(int)Current_Trajectory][Current_Point].Y) <= 0.1)
			{
				Vector V = new Vector();
				if (Current_Point == Trajectories[(int)Current_Trajectory].Count - 1)
				{
					V.X = Trajectories[(int)Current_Trajectory][0].X - Trajectories[(int)Current_Trajectory][Current_Point].X;
					V.Y = Trajectories[(int)Current_Trajectory][0].Y - Trajectories[(int)Current_Trajectory][Current_Point].Y;
					Current_Point = 0;
				}
				else
				{
					V.X = Trajectories[(int)Current_Trajectory][Current_Point + 1].X - Trajectories[(int)Current_Trajectory][Current_Point].X;
					V.Y = Trajectories[(int)Current_Trajectory][Current_Point + 1].Y - Trajectories[(int)Current_Trajectory][Current_Point].Y;
					Current_Point++;
				}
				V.Normalize();
				if (V.X != 0)
				{
					SendCommand($"MOVE#1 {Trajectories[(int)Current_Trajectory][Current_Point].X.ToString().Replace(',', '.')} {(40.0 * Math.Abs(V.X)).ToString().Replace(',', '.')}");
				}
				if (V.Y != 0)
				{
					SendCommand($"MOVE#2 {Trajectories[(int)Current_Trajectory][Current_Point].Y.ToString().Replace(',', '.')} {(40.0 * Math.Abs(V.Y)).ToString().Replace(',', '.')}");
				}
			}
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

		private void Move_Timer_Tick(object? sender, EventArgs e)
		{
			if (Client != null)
			{
				if (OPU1_Enabled)
				{
					SendCommand($"MOVE#1 {OPU1_TargetPos[0]} 80");
					SendCommand($"MOVE#2 {OPU1_TargetPos[1]} 80");
				}
			}
		}

		public void SendCommand(string Com)
		{
			if (Client.Connected)
			{
				WriteByteData = Encoding.UTF8.GetBytes("^" + Com + "~");
				Stream.Write(WriteByteData, 0, WriteByteData.Length);
			}
		}



		private async void ConnectOPU1()
		{
			try
			{
				IPAddress Address = IPAddress.Parse("192.168.1.88");
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
							case "^EN?:1:1~":
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
									double.TryParse(Data.Split(':')[1], DoubleFormat, out OPU1_Position[0]);
									double.TryParse(Data.Split(':')[2].Replace('~', ' '), DoubleFormat, out OPU1_Position[1]);
									//OPU1_Position[0] = double.Parse(Data.Split(':')[1].Replace('.',','));
									//OPU1_Position[1] = double.Parse(Data.Split(':')[2].Replace('~', ' ').Replace('.', ','));
									XPos_Label.Content = "Позиция Х: " + OPU1_Position[0].ToString("0.00 мм");
									YPos_Label.Content = "Позиция Y: " + OPU1_Position[1].ToString("0.00 мм");
									Canvas.SetTop(Slider_Y_Rect, 10 + OPU1_Position[1] + 105.0);
									Canvas.SetLeft(Slider_X_Rect, 10 + OPU1_Position[0] + 130.0);
									Canvas.SetLeft(Slider_Y_Rect, 10 + OPU1_Position[0] + 130.0);
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

		private void Vertical_Pos_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			//Canvas.SetTop(Target_Pos_Point, 95 + Vertical_Pos_Slider.Value / 10.0);
			//Canvas.SetTop(Slider_Y_Rect, 10 + Vertical_Pos_Slider.Value + 105.0);
			Trajectory_Timer.Stop();
			if (Client != null)
			{
				if (OPU1_Enabled)
				{
					SendCommand($"MOVE#2 {e.NewValue} 80");
				}
			}
		}

		private void Horizontal_Pos_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			//Canvas.SetLeft(Target_Pos_Point, 120 + Horizontal_Pos_Slider.Value / 10.0 );
			//Canvas.SetLeft(Slider_X_Rect, 10 + Horizontal_Pos_Slider.Value + 130.0);
			//Canvas.SetLeft(Slider_Y_Rect, 10 + Horizontal_Pos_Slider.Value + 130.0);
			Trajectory_Timer.Stop();
			if (Client != null)
			{
				if (OPU1_Enabled)
				{
					SendCommand($"MOVE#1 {e.NewValue} 80");
				}
			}
		}

		private void Target_Pos_Point_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Trajectory_Timer.Stop();
			isDragging = true;
			Move_Timer.Start();
			Target_Pos_Popup.IsOpen = true;
		}

		private void Window_MouseMove(object sender, MouseEventArgs e)
		{
			if (isDragging)//120 95 380 315
			{
				if (Mouse.GetPosition(relativeTo: Demo_Canvas).Y - 20.0 >= 95.0 && Mouse.GetPosition(relativeTo: Demo_Canvas).Y - 20.0 <= 305.0)
				{
					Canvas.SetTop(Target_Pos_Point, Mouse.GetPosition(relativeTo: Demo_Canvas).Y - 20);
				}
				else if (Mouse.GetPosition(relativeTo: Demo_Canvas).Y - 20.0 <= 95.0)
				{
					Canvas.SetTop(Target_Pos_Point, 95.0);
				}
				else
				{
					Canvas.SetTop(Target_Pos_Point, 305.0);
				}
				if (Mouse.GetPosition(relativeTo: Demo_Canvas).X - 20.0 >= 120.0 && Mouse.GetPosition(relativeTo: Demo_Canvas).X - 20.0 <= 380.0)
				{
					Canvas.SetLeft(Target_Pos_Point, Mouse.GetPosition(relativeTo: Demo_Canvas).X - 20);
				}
				else if (Mouse.GetPosition(relativeTo: Demo_Canvas).X - 20.0 <= 120.0)
				{
					Canvas.SetLeft(Target_Pos_Point, 120.0);
				}
				else
				{
					Canvas.SetLeft(Target_Pos_Point, 380.0);
				}

				OPU1_TargetPos[0] = Canvas.GetLeft(Target_Pos_Point) - 250.0;
				OPU1_TargetPos[1] = Canvas.GetTop(Target_Pos_Point) - 200.0;
				Target_Pos_Popup.HorizontalOffset = Target_Pos_Popup.HorizontalOffset += 0.01;
				Target_Pos_Popup.HorizontalOffset = 0;
				Target_Pos_Popup.VerticalOffset = -40;
				PopupText.Text = OPU1_TargetPos[0].ToString("0.00") + " ; " + OPU1_TargetPos[1].ToString("0.00");

				
			}
		}

		private void Window_MouseUp(object sender, MouseButtonEventArgs e)
		{
			isDragging = false;
			Move_Timer.Stop();
			Target_Pos_Popup.IsOpen = false;
		}

		private void Target_Pos_Point_PreviewTouchDown(object sender, TouchEventArgs e)
		{
			Trajectory_Timer.Stop();
			isDragging = true;
			Move_Timer.Start();
			Target_Pos_Popup.IsOpen = true;
		}

		private void Window_TouchMove(object sender, TouchEventArgs e)
		{
			if (isDragging)//120 95 380 315
			{
				if (e.GetTouchPoint(relativeTo: Demo_Canvas).Position.Y - 20.0 >= 95.0 && e.GetTouchPoint(relativeTo: Demo_Canvas).Position.Y - 20.0 <= 315.0)
				{
					Canvas.SetTop(Target_Pos_Point, e.GetTouchPoint(relativeTo: Demo_Canvas).Position.Y - 20);
				}
				else if (e.GetTouchPoint(relativeTo: Demo_Canvas).Position.Y - 20.0 <= 95.0)
				{
					Canvas.SetTop(Target_Pos_Point, 95.0);
				}
				else
				{
					Canvas.SetTop(Target_Pos_Point, 315.0);
				}
				if (e.GetTouchPoint(relativeTo: Demo_Canvas).Position.X - 20.0 >= 120.0 && e.GetTouchPoint(relativeTo: Demo_Canvas).Position.X - 20.0 <= 380.0)
				{
					Canvas.SetLeft(Target_Pos_Point, e.GetTouchPoint(relativeTo: Demo_Canvas).Position.X - 20);
				}
				else if (e.GetTouchPoint(relativeTo: Demo_Canvas).Position.X - 20.0 <= 120.0)
				{
					Canvas.SetLeft(Target_Pos_Point, 120);
				}
				else
				{
					Canvas.SetLeft(Target_Pos_Point, 380);
				}

				OPU1_TargetPos[0] = Canvas.GetLeft(Target_Pos_Point) - 250.0;
				OPU1_TargetPos[1] = Canvas.GetTop(Target_Pos_Point) - 200.0;
				Target_Pos_Popup.HorizontalOffset = Target_Pos_Popup.HorizontalOffset += 0.01;
				Target_Pos_Popup.HorizontalOffset = 0;
				Target_Pos_Popup.VerticalOffset = -40;
				PopupText.Text = OPU1_TargetPos[0].ToString("0.00") + " ; " + OPU1_TargetPos[1].ToString("0.00");
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
			Trajectory_Timer.Stop();
			if (Client != null)
			{
				if (!OPU1_Enabled)
				{
					SendCommand("CLR");
					SendCommand("EN");
					SendCommand("MH");
				}
				else
				{
					SendCommand("DIS");
				}
				
			}
		}

		private void Stop_Btn_Click(object sender, RoutedEventArgs e)
		{
			Trajectory_Timer.Stop();
			if (Client != null)
			{
				SendCommand("STOP");
			}
		}

		private void Window_TouchUp(object sender, TouchEventArgs e)
		{
			isDragging = false;
			Move_Timer.Stop();
			Target_Pos_Popup.IsOpen = false;
		}

		private void Random_Btn_Click(object sender, RoutedEventArgs e)
		{
			if (Client != null)
			{
				if (OPU1_Enabled)
				{
					SendCommand("DEMO");
				}
			}
		}

		private void Square_Btn_Click(object sender, RoutedEventArgs e)
		{
			Trajectory_Timer.Stop();
			if (Client != null)
			{
				if (OPU1_Enabled)
				{
					Current_Trajectory = Trajectory.Square;
					Current_Point = 0;
					SendCommand($"MOVE#1 {Trajectories[(int)Current_Trajectory][Current_Point].X} 40");
					SendCommand($"MOVE#2 {Trajectories[(int)Current_Trajectory][Current_Point].Y} 40");
					Trajectory_Timer.Start();
				}
			}
		}

		private void Circle_Btn_Click(object sender, RoutedEventArgs e)
		{
			Trajectory_Timer.Stop();
			if (Client != null)
			{
				if (OPU1_Enabled)
				{
					Current_Trajectory = Trajectory.Circle;
					Current_Point = 0;
					SendCommand($"MOVE#1 {Trajectories[(int)Current_Trajectory][Current_Point].X} 20");
					SendCommand($"MOVE#2 {Trajectories[(int)Current_Trajectory][Current_Point].Y} 20");
					Trajectory_Timer.Start();
				}
			}
		}

		private void Triangle_Btn_Click(object sender, RoutedEventArgs e)
		{
			Trajectory_Timer.Stop();
			if (Client != null)
			{
				if (OPU1_Enabled)
				{
					Current_Trajectory = Trajectory.Triangle;
					Current_Point = 0;
					SendCommand($"MOVE#1 {Trajectories[(int)Current_Trajectory][Current_Point].X} 20");
					SendCommand($"MOVE#2 {Trajectories[(int)Current_Trajectory][Current_Point].Y} 20");
					Trajectory_Timer.Start();
				}
			}
		}

		private void Star_Btn_Click(object sender, RoutedEventArgs e)
		{
			Trajectory_Timer.Stop();
			if (Client != null)
			{
				if (OPU1_Enabled)
				{
					Current_Trajectory = Trajectory.Star;
					Current_Point = 0;
					SendCommand($"MOVE#1 {Trajectories[(int)Current_Trajectory][Current_Point].X} 20");
					SendCommand($"MOVE#2 {Trajectories[(int)Current_Trajectory][Current_Point].Y} 20");
					Trajectory_Timer.Start();
				}
			}
		}

		private void Hourglass_Btn_Click(object sender, RoutedEventArgs e)
		{
			Trajectory_Timer.Stop();
			if (Client != null)
			{
				if (OPU1_Enabled)
				{
					Current_Trajectory = Trajectory.Hourglass;
					Current_Point = 0;
					SendCommand($"MOVE#1 {Trajectories[(int)Current_Trajectory][Current_Point].X} 20");
					SendCommand($"MOVE#2 {Trajectories[(int)Current_Trajectory][Current_Point].Y} 20");
					Trajectory_Timer.Start();
				}
			}
		}
	}
}
