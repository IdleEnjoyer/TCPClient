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

                ConnectionStatus.Content = "Подключено!";
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
                ConnectionStatus.Content = "Отключен";
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
                        ConnectionStatus.Content = "Отключен";
                        ConnectionStatus.Foreground = Brushes.Red;
                        MessageBox.Show("Сервер закрыт!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        break;
                    }
                    string Data = Encoding.ASCII.GetString(Buffer, 0, BytesRead);
                    ServerData.Text += "Сервер " + System.DateTime.Now.ToString() + ": " + Data + "\n";
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message);
                    break;
                }
            }
        }

        private void StateBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string StateData = "EN0?\n\r";
                ByteData = System.Text.Encoding.ASCII.GetBytes(StateData);
                Stream.Write(ByteData, 0, ByteData.Length);
                ServerData.Text += "Клиент " + System.DateTime.Now.ToString() + ": " + StateData.Substring(0, StateData.Length - 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void EnabledCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                string StateData = "";
                StateData = EnabledCheckBox.IsChecked == true ? StateData += "EN0 1\n\r" : StateData += "EN0 0\n\r";
                ByteData = System.Text.Encoding.ASCII.GetBytes(StateData);
                Stream.Write(ByteData, 0, ByteData.Length);
                ServerData.Text += "Клиент " + System.DateTime.Now.ToString() + ": " + StateData.Substring(0, StateData.Length - 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AbortBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string StateData = "ABORT0\n\r";
                ByteData = System.Text.Encoding.ASCII.GetBytes(StateData);
                Stream.Write(ByteData, 0, ByteData.Length);
                ServerData.Text += "Клиент " + System.DateTime.Now.ToString() + ": " + StateData.Substring(0, StateData.Length - 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string StateData = "CLR0\n\r";
                ByteData = System.Text.Encoding.ASCII.GetBytes(StateData);
                Stream.Write(ByteData, 0, ByteData.Length);
                ServerData.Text += "Клиент " + System.DateTime.Now.ToString() + ": " + StateData.Substring(0, StateData.Length - 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string StateData = "MH0\n\r";
                ByteData = System.Text.Encoding.ASCII.GetBytes(StateData);
                Stream.Write(ByteData, 0, ByteData.Length);
                ServerData.Text += "Клиент " + System.DateTime.Now.ToString() + ": " + StateData.Substring(0, StateData.Length - 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PositionBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string StateData = "POS0?\n\r";
                ByteData = System.Text.Encoding.ASCII.GetBytes(StateData);
                Stream.Write(ByteData, 0, ByteData.Length);
                ServerData.Text += "Клиент " + System.DateTime.Now.ToString() + ": " + StateData.Substring(0, StateData.Length - 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ErrorsBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string StateData = "ERR0?\n\r";
                ByteData = System.Text.Encoding.ASCII.GetBytes(StateData);
                Stream.Write(ByteData, 0, ByteData.Length);
                ServerData.Text += "Клиент " + System.DateTime.Now.ToString() + ": " + StateData.Substring(0, StateData.Length - 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void HomeBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string StateData = "MH0?\n\r";
                ByteData = System.Text.Encoding.ASCII.GetBytes(StateData);
                Stream.Write(ByteData, 0, ByteData.Length);
                ServerData.Text += "Клиент " + System.DateTime.Now.ToString() + ": " + StateData.Substring(0, StateData.Length - 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StoppedBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string StateData = "STOP0?\n\r";
                ByteData = System.Text.Encoding.ASCII.GetBytes(StateData);
                Stream.Write(ByteData, 0, ByteData.Length);
                ServerData.Text += "Клиент " + System.DateTime.Now.ToString() + ": " + StateData.Substring(0, StateData.Length - 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SpeedBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string StateData = "SPD0";
                double Speed = double.Parse(SpeedInput.Text.Replace(".",","));
                StateData += " " + Speed.ToString().Replace(",", ".") + "\n\r";
                ByteData = System.Text.Encoding.ASCII.GetBytes(StateData);
                Stream.Write(ByteData, 0, ByteData.Length);
                ServerData.Text += "Клиент " + System.DateTime.Now.ToString() + ": " + StateData.Substring(0, StateData.Length - 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MoveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string StateData = "MOVE0";
                double Movement = double.Parse(MoveInput.Text.Replace(".", ","));
                StateData += " " + Movement.ToString().Replace(",", ".") + "\n\r";
                ByteData = System.Text.Encoding.ASCII.GetBytes(StateData);
                Stream.Write(ByteData, 0, ByteData.Length);
                ServerData.Text += "Клиент " + System.DateTime.Now.ToString() + ": " + StateData.Substring(0, StateData.Length - 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
