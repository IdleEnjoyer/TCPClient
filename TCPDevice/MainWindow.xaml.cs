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
		public MainWindow()
        {
            InitializeComponent();
			GreenBrush.Center = new Point(0.25, 0.25);
			RedBrush.Center = new Point(0.25, 0.25);
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
					OPU1_Status++;
					break;
				case 5:
					SendCommand_OPU1("LIM? A");
					OPU1_Status++;
					break;
				case 6:
					SendCommand_OPU1("LIM? E");
					OPU1_Status++;
					break;
				case 7:
					SendCommand_OPU1("LIM? P");
					OPU1_Status++;
					break;
				case 8:
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
					SendCommand_OPU2("POS?");
					OPU2_Status++;
					break;
				case 3:
					SendCommand_OPU2("STOP?");
					OPU2_Status++;
					break;
				case 4:
					SendCommand_OPU2("FLT?");
					OPU2_Status++;
					break;
				case 5:
					SendCommand_OPU2("LIM? A");
					OPU2_Status++;
					break;
				case 6:
					SendCommand_OPU2("LIM? E");
					OPU2_Status++;
					break;
				case 7:
					SendCommand_OPU2("LIM? P");
					OPU2_Status++;
					break;
				case 8:
					SendCommand_OPU2("LIM? X");
					OPU2_Status++;
					break;
				case 9:
					SendCommand_OPU2("LIM? Y");
					OPU2_Status = 0;
					break;
			}
		}

		public void SendCommand_OPU1(string Com)
		{
			if (Client_OPU1.Connected)
			{
				WriteByteData = Encoding.UTF8.GetBytes("^" + Com + "~\r\n");
				Stream_OPU1.Write(WriteByteData, 0, WriteByteData.Length);
			}
		}

		public void SendCommand_OPU2(string Com)
		{
			if (Client_OPU2.Connected)
			{
				WriteByteData = Encoding.UTF8.GetBytes("^" + Com + "~\r\n");
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
					Stream_OPU1 = Client_OPU1.GetStream();
					OPU1_StatusTimer.Start();
					await StartReadingOPU1DataAsync();
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

		private async void ConnectOPU2()
		{
			try
			{
				IPAddress Address = IPAddress.Parse("192.168.0.102");
				int Port = 2000;
				Client_OPU2 = new TcpClient(Address.ToString(), Port);

				if (Client_OPU2.Connected)
				{
					Stream_OPU2 = Client_OPU2.GetStream();
					OPU2_StatusTimer.Start();
					//await StartReadingOPU2DataAsync();
				}
				else
				{
					throw new Exception("No connection");
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
							
						}
					}
					catch (IOException)
					{
						MessageBox.Show("Connection error");
						OPU1_StatusTimer.Stop();
						break;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Connection Error: {ex.Message}");
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

			if (Client_OPU2 != null)
			{
				if (Client_OPU2.Connected)
				{
					SendCommand_OPU2("DIS");
				}
			}
		}

		private void Vertical_Pos_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			Canvas.SetTop(Target_Pos_Point, 2 * Vertical_Pos_Slider.Value / 10.0);
			Canvas.SetTop(Slider_Y_Rect, 10 + Vertical_Pos_Slider.Value / 10.0);
		}

		private void Horizontal_Pos_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			Canvas.SetLeft(Target_Pos_Point, 2 * Horizontal_Pos_Slider.Value / 10.0 );
			Canvas.SetLeft(Slider_X_Rect, 10 + Horizontal_Pos_Slider.Value / 10.0 );
			Canvas.SetLeft(Slider_Y_Rect, 10 + Horizontal_Pos_Slider.Value / 10.0 );
		}
	}
}
