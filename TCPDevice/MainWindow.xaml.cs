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

namespace TCPDevice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient Client;
        private NetworkStream Stream;
        private byte[] ByteData;
        private string DataString;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IPAddress Address = IPAddress.Parse(IPInput.Text);
                int Port = int.Parse(PortInput.Text);

                Client = new TcpClient(Address.ToString(), Port);
                Stream = Client.GetStream();

                ConnectionStatus.Content = "Connected!";
                ConnectionStatus.Foreground = Brushes.Green;

                await StartReadingDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void StopConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Client.Close();
                ConnectionStatus.Content = "Disconnected";
                ConnectionStatus.Foreground = Brushes.Red;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task StartReadingDataAsync()
        {
            byte[] Buffer = new byte[1024];
            while (Client.Connected)
            {
                try
                {
                    int BytesRead = await Stream.ReadAsync(Buffer, 0, Buffer.Length);
                    if (BytesRead == 0)
                    {
                        Client.Close();
                        ConnectionStatus.Content = "Disconnected";
                        ConnectionStatus.Foreground = Brushes.Red;
                        MessageBox.Show("Server closed!", "Attention", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        break;
                    }
                    string Data = Encoding.ASCII.GetString(Buffer, 0, BytesRead);
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message, "huh");
                    break;
                }
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if()
        }

        private void StateBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
