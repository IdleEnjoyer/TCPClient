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
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Net;

namespace TCPDevice
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public TcpClient Client { get; set; }

        private MainWindow DemoWindow;
        public Window1()
        {
            InitializeComponent();
        }

        private void StartConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IPAddress Address = IPAddress.Parse(IPInput.Text);
                int Port = int.Parse(PortInput.Text);

                if(DemoWindow == null)
                {
                    DemoWindow = new MainWindow();
                    DemoWindow.Owner = this;
                }
                
                
                DemoWindow.Client = new TcpClient(Address.ToString(), Port);

                if(DemoWindow.Client.Connected)
                {
                    DemoWindow.Show();
                    DemoWindow.Connect();
                    DemoWindow.ChangeConnection(DemoWindow.Client.Connected);
                }

                //this.Owner.ConnectionStatus.Content = "Подключено!";
                //this.Owner.ConnectionStatus.Foreground = Brushes.Green;
            }
            catch (Exception ex)
            {
                DemoWindow.Show();
                DemoWindow.ChangeConnection(DemoWindow.Client.Connected);
                MessageBox.Show(ex.Message, "Connection start error", MessageBoxButton.OK, MessageBoxImage.Question);
                return;
            }
        }

        private void StopConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DemoWindow.Client.Close();
                DemoWindow.ChangeConnection(DemoWindow.Client.Connected);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Connection stop error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
