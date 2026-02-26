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
		private double[] OPU1_Position = { 0.0, 0.0};
		private double[] OPU1_TargetPos = { 0.0, 0.0 };
		private bool OPU1_Stopped = false;
		private bool OPU1_InError = false;

		private byte[] WriteByteData;
		public string Axis_OPU1 = "";
		public string Axis_OPU2 = "";
		public bool AllowNegative = false;

		private bool isDragging = false;
		public MainWindow()
        {
            InitializeComponent();
		}


		public void SendCommand(string Com)
		{
			if (Client.Connected)
			{
				WriteByteData = Encoding.UTF8.GetBytes("^" + Com + "~\r\n");
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

		private void OPU1_Connect_Click(object sender, RoutedEventArgs e)
		{
			ConnectOPU1();
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
			Canvas.SetTop(Slider_Y_Rect, 10 + Vertical_Pos_Slider.Value + 105.0);
		}

		private void Horizontal_Pos_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			//Canvas.SetLeft(Target_Pos_Point, 120 + Horizontal_Pos_Slider.Value / 10.0 );
			Canvas.SetLeft(Slider_X_Rect, 10 + Horizontal_Pos_Slider.Value + 130.0);
			Canvas.SetLeft(Slider_Y_Rect, 10 + Horizontal_Pos_Slider.Value + 130.0);
		}

		private void Target_Pos_Point_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			isDragging = true;
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

				XPos_Label.Content = "Позиция Х: " + OPU1_TargetPos[0].ToString("0.00 мм");
				YPos_Label.Content = "Позиция Y: " + OPU1_TargetPos[1].ToString("0.00 мм");
			}
		}

		private void Window_MouseUp(object sender, MouseButtonEventArgs e)
		{
			isDragging = false;
			Target_Pos_Popup.IsOpen = false;
		}

		private void Target_Pos_Point_PreviewTouchDown(object sender, TouchEventArgs e)
		{
			isDragging = true;
			
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

				XPos_Label.Content = "Позиция Х: " + OPU1_TargetPos[0].ToString("0.00 мм");
				YPos_Label.Content = "Позиция Y: " + OPU1_TargetPos[1].ToString("0.00 мм");
			}
		}
	}
}
