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

namespace TCPDevice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient Client_OPU1;
		private TcpClient Client_OPU2;
		private NetworkStream Stream_OPU1;
		private NetworkStream Stream_OPU2;
		private float[] OPU1_LowerLimits;
		private float[] OPU1_UpperLimits;
		private float[] OPU1_SpeedLimits = { 10.0f, 10.0f, 10.0f, 10.0f };
		private float[] OPU2_LowerLimits;
		private float[] OPU2_UpperLimits;
		private float[] OPU2_SpeedLimits = { 10.0f, 10.0f, 10.0f, 10.0f };
		private DispatcherTimer OPU1_StatusTimer;
		private byte[] WriteByteData;
		public string Axis = "";
        public MainWindow()
        {
            InitializeComponent();
		}

		public void SendCommand(string Com)
		{
			if (Client_OPU1.Connected)
			{
				WriteByteData = Encoding.UTF8.GetBytes("^" + Com + "\r\r");
				Stream_OPU1.Write(WriteByteData, 0, WriteByteData.Length);
			}
		}
		
		

		private void OPU1_Connect_Click(object sender, RoutedEventArgs e)
		{
			//IPAddress Address = IPAddress.Parse("192.168.0.101");
			//int Port = 2000;
			//Client_OPU1 = new TcpClient(Address.ToString(), Port);
			//Stream_OPU1 = Client_OPU1.GetStream();
			if (Client_OPU1.Connected)
			{
				OPU1_ConnectStatus.Fill = Brushes.Green;
			}
			else
			{
				OPU1_ConnectStatus.Fill = Brushes.Red;
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{

		}

		private void OPU1_Power_Click(object sender, RoutedEventArgs e)
		{
			SendCommand("EN");
		}

		private void OPU1_Home_Click(object sender, RoutedEventArgs e)
		{
			SendCommand("FH");
		}

		private void OPU1_AzSetLim_Click(object sender, RoutedEventArgs e)
		{
			Axis = "A";
			Window LimPopup = new PopupLimits();
			LimPopup.Owner = this;
			LimPopup.ShowDialog();
		}
	}
}
