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

namespace TCPDevice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient Client;
        private byte[] Data;
        private string DataString;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IPAddress Address = IPAddress.Parse(IPInput.Text);
                int Port = int.Parse(PortInput.Text);
                
                Client = new TcpClient(Address.ToString(), Port);

                ConnectionStatus.Content = "Connected!";
                ConnectionStatus.Foreground = Brushes.Green;
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
                ConnectionStatus.Content = "Disconnected...";
                ConnectionStatus.Foreground = Brushes.Red;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!DataField.Text.Contains(EndSymbol.Text))
                {
                    DataString += DataField.Text;
                    MessageBox.Show("Text added");
                }
                else
                {
                    DataString += DataField.Text.Split(EndSymbol.Text)[0];
                    Data = System.Text.Encoding.ASCII.GetBytes(DataString);
                    NetworkStream Stream = Client.GetStream();
                    Stream.Write(Data, 0, Data.Length);
                    DataString = string.Empty;
                    MessageBox.Show("Data sent");
                }
                DataField.Text = string.Empty;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowData_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(DataString, "Current data");
        }
    }
}
