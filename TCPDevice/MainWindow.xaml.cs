using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

#pragma warning disable CS8618
#pragma warning disable CS8602

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
                    ServerData.ScrollToEnd();
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
                TextBox? Command = EnabledCmd.SelectedItem as TextBox;
                string StateData = Command.Text;
                SendData(StateData);
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
                ComboBox? EnabledCB = PolarisationCmd.Items.GetItemAt(1) as ComboBox;
                TextBox? EnabledTB = EnabledCB.Items.GetItemAt(1) as TextBox;
                ComboBox? DisabledCB = PolarisationCmd.Items.GetItemAt(0) as ComboBox;
                TextBox? DisabledTB = DisabledCB.Items.GetItemAt(1) as TextBox;
                string StateData = "";
                StateData = EnabledCheckBox.IsChecked == true ? StateData += EnabledTB.Text : StateData += DisabledTB.Text;
                SendData(StateData);
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
                TextBox? Command = AbortCmd.SelectedItem as TextBox;
                string StateData = Command.Text;
                SendData(StateData);
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
                TextBox? Command = ClearCmd.SelectedItem as TextBox;
                string StateData = Command.Text;
                SendData(StateData);
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
                TextBox? Command = ResetCmd.SelectedItem as TextBox;
                string StateData = Command.Text;
                SendData(StateData);
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
                TextBox? Command = PositionCmd.SelectedItem as TextBox;
                string StateData = Command.Text;
                SendData(StateData);
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
                TextBox? Command = ErrorCmd.SelectedItem as TextBox;
                string StateData = Command.Text;
                SendData(StateData);
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
                TextBox? Command = HomeCmd.SelectedItem as TextBox;
                string StateData = Command.Text;
                SendData(StateData);
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
                TextBox? Command = StopCmd.SelectedItem as TextBox;
                string StateData = Command.Text;
                SendData(StateData);
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
                TextBox? Command = SpeedCmd.SelectedItem as TextBox;
                string StateData = Command.Text;
                double Speed = double.Parse(SpeedInput.Text.Replace(".",","));
                StateData += " " + Speed.ToString().Replace(",", ".");
                SendData(StateData);
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
                TextBox? Command = MoveCmd.SelectedItem as TextBox;
                string StateData = Command.Text;
                double Movement = double.Parse(MoveInput.Text.Replace(".", ","));
                StateData += " " + Movement.ToString().Replace(",", ".");
                SendData(StateData);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        void SendData(string Data)
        {
            string EndSymbol = EndInput.Text;
            string StartSymbol = StartInput.Text;
            EndSymbol = Regex.Unescape(EndSymbol);
            string DataString = StartSymbol + Data + EndSymbol;
            ByteData = System.Text.Encoding.ASCII.GetBytes(DataString);
            Stream.Write(ByteData, 0, ByteData.Length);
            ServerData.Text += "Клиент " + System.DateTime.Now.ToString() + ": " + DataString;
            ServerData.ScrollToEnd();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox? Command = SpeedCmd.SelectedItem as TextBox;
                string StateData = Command.Text;
                double Speed = double.Parse(SpeedInput.Text.Replace(".", ","));
                StateData += " " + Speed.ToString().Replace(",", ".");
                SendData(StateData);
                Command = MoveCmd.SelectedItem as TextBox;
                StateData = Command.Text;
                double Movement = double.Parse(MoveInput.Text.Replace(".", ","));
                StateData += " " + Movement.ToString().Replace(",", ".");
                SendData(StateData);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SendCmd_Click(object sender, RoutedEventArgs e)
        {
            SendData(CommandInput.Text);
            CommandInput.Text = string.Empty;
        }

        private void OnSelect(object sender, SelectionChangedEventArgs e)
        {
            ComboBox? Element = sender as ComboBox;
            Element.SelectedItem = 0;
        }

        private void OnPress(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                SendCmd_Click((TextBox)sender, e);
            }
        }
    }
}
