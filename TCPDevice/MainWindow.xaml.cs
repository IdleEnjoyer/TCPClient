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
using System.Timers;
using System.Diagnostics;
using System.Windows.Threading;

#pragma warning disable CS8618
#pragma warning disable CS8602

namespace TCPDevice
{
    public partial class MainWindow : Window
    {
        private TcpClient Client;
        private NetworkStream Stream;
        private byte[] ByteData;
        private Stopwatch TimersElapsed = new Stopwatch();
        private DispatcherTimer DispTimer = new DispatcherTimer();
        private Timer Demo1 = new Timer(65000);
        private bool Demo1Cycle = false;
        private Timer Demo2 = new Timer(5000);
        public MainWindow()
        {
            InitializeComponent();
            Demo1.Elapsed += Demo1_Elapsed;
            Demo2.Elapsed += Demo2_Elapsed;
            DispTimer.Interval = TimeSpan.FromMilliseconds(100);
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
                MessageBox.Show("Не подключено", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task StartReadingDataAsync()
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
                        ConnectionStatus.Content = "Отключен";
                        ConnectionStatus.Foreground = Brushes.Red;
                        MessageBox.Show("Сервер закрыт!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        break;
                    }
                    string Data = Encoding.ASCII.GetString(Buffer, 0, BytesRead);
                    AddLineToTextBox("Сервер " + System.DateTime.Now.ToString() + ": " + Data, ServerData);
                    //ServerData.Text += "Сервер " + System.DateTime.Now.ToString() + ": " + Data + "\n";
                    ServerData.ScrollToEnd();
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Сервер закрыт!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    break;
                }
            }
        }

        //private void StateBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        TextBox? Command = EnabledCmd.SelectedItem as TextBox;
        //        string StateData = Command.Text;
        //        SendData(StateData);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Не подключено!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        //    }
        //}

        private void EnabledCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ComboBox? EnabledCB = PolarisationCmd.Items.GetItemAt(1) as ComboBox;
                TextBox? EnabledTB = EnabledCB.Items.GetItemAt(1) as TextBox;
                ComboBox? DisabledCB = PolarisationCmd.Items.GetItemAt(0) as ComboBox;
                TextBox? DisabledTB = DisabledCB.Items.GetItemAt(1) as TextBox;
                string StateData = "";
                if(EnabledCheckBox.IsChecked == true)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        SendData("RES");
                    }
                }
                StateData = EnabledCheckBox.IsChecked == true ? StateData += EnabledTB.Text : StateData += DisabledTB.Text;
                SendData(StateData);
                if (EnabledCheckBox.IsChecked != true)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        SendData("RES");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не подключено!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void AbortBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox? Command = AbortCmd.SelectedItem as TextBox;
                string StateData = Command.Text;
                SendData(StateData);
                Demo1.Stop();
                Demo1Cycle = false;
                Demo2.Stop();
                TimersElapsed.Reset();
                TimersElapsed.Stop();
                DispTimer.Stop();
                DispTimer.Tick -= DispTimer2_Tick;
                Demo1Btn.IsEnabled = true;
                Demo2Btn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не подключено!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        //private void ClearBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        TextBox? Command = ClearCmd.SelectedItem as TextBox;
        //        string StateData = Command.Text;
        //        SendData(StateData);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Не подключено!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        //    }
        //}

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        TextBox? Command = ResetCmd.SelectedItem as TextBox;
        //        string StateData = Command.Text;
        //        SendData(StateData);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Не подключено!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        //    }
        //}

        //private void PositionBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        TextBox? Command = PositionCmd.SelectedItem as TextBox;
        //        string StateData = Command.Text;
        //        SendData(StateData);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Не подключено!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        //    }
        //}

        //private void ErrorsBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        TextBox? Command = ErrorCmd.SelectedItem as TextBox;
        //        string StateData = Command.Text;
        //        SendData(StateData);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Не подключено!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        //    }
        //}

        //private void HomeBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        TextBox? Command = HomeCmd.SelectedItem as TextBox;
        //        string StateData = Command.Text;
        //        SendData(StateData);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Не подключено!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        //    }
        //}

        //private void StoppedBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        TextBox? Command = StopCmd.SelectedItem as TextBox;
        //        string StateData = Command.Text;
        //        SendData(StateData);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Не подключено!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        //    }
        //}

        //private void SpeedBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        TextBox? Command = SpeedCmd.SelectedItem as TextBox;
        //        string StateData = Command.Text;
        //        double Speed = double.Parse(SpeedInput.Text.Replace(".",","));
        //        StateData += " " + Speed.ToString().Replace(",", ".");
        //        SendData(StateData);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Не подключено!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        //    }
        //}

        //private void MoveBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        TextBox? Command = MoveCmd.SelectedItem as TextBox;
        //        string StateData = Command.Text;
        //        double Movement = double.Parse(MoveInput.Text.Replace(".", ","));
        //        StateData += " " + Movement.ToString().Replace(",", ".");
        //        SendData(StateData);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Не подключено!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        //    }
        //}
        
        void SendData(string Data)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                string EndSymbol = EndInput.Text;
                string StartSymbol = StartInput.Text;
                EndSymbol = Regex.Unescape(EndSymbol);
                string DataString = StartSymbol + Data + EndSymbol;
                ByteData = System.Text.Encoding.ASCII.GetBytes(DataString);
                DataString = DataString.Substring(0, DataString.Length - 2);
                Stream.Write(ByteData, 0, ByteData.Length);
                AddLineToTextBox("Клиент " + System.DateTime.Now.ToString() + ": " + DataString, ServerData);
                //ServerData.Text += "Клиент " + System.DateTime.Now.ToString() + ": " + DataString;
                ServerData.ScrollToEnd();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => SendData(Data));
            }
        }

        //private void SpdMovSend_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        TextBox? Command = SpeedCmd.SelectedItem as TextBox;
        //        string StateData = Command.Text;
        //        double Speed = double.Parse(SpeedInput.Text.Replace(".", ","));
        //        StateData += " " + Speed.ToString().Replace(",", ".");
        //        SendData(StateData);
        //        Command = MoveCmd.SelectedItem as TextBox;
        //        StateData = Command.Text;
        //        double Movement = double.Parse(MoveInput.Text.Replace(".", ","));
        //        StateData += " " + Movement.ToString().Replace(",", ".");
        //        SendData(StateData);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Не подключено!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        //    }
        //}

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

        private void Demo1Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Client.Connected)
                {
                    CurrentDemo.Content = "ВЕРТИКАЛЬНАЯ";
                    Demo2Btn.IsEnabled = false;
                    Demo1Btn.IsEnabled = false;
                    SendData("MOVE 0 50");
                    TimersElapsed.Start();
                    DispTimer.Tick += DispTimer1_Tick;
                    DispTimer.Start();
                    Demo1.AutoReset = true;
                    Demo1.Enabled = true;
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не подключено!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

        }

        private void DispTimer1_Tick(object? sender, EventArgs e)
        {
            int remainingTime = 65000 - (int)TimersElapsed.Elapsed.TotalMilliseconds;
            TimerLabel.Content = "Таймер: " + (remainingTime / 1000.0).ToString("0.000");
        }

        private void Demo1_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (!Demo1Cycle)
            {
                SendData("MOVE 3000 50");
                Demo1Cycle = true;
            }
            else
            {
                Demo1Cycle = false;
                SendData("MOVE 0 50");
            }
            TimersElapsed.Restart();
        }

        private void Demo2_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (!Demo1Cycle)
            {
                SendData("MOVE 90 45");
                Demo1Cycle = true;
            }
            else
            {
                Demo1Cycle = false;
                SendData("MOVE 0 45");
            }
            TimersElapsed.Restart();
        }

        private void Demo2Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Client.Connected)
                {
                    CurrentDemo.Content = "ПОЛЯРИЗАЦИЯ";
                    Demo2Btn.IsEnabled = false;
                    Demo1Btn.IsEnabled = false;
                    SendData("MOVE 0 45");
                    TimersElapsed.Start();
                    DispTimer.Tick += DispTimer2_Tick;
                    DispTimer.Start();
                    Demo2.AutoReset = true;
                    Demo2.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не подключено!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void DispTimer2_Tick(object? sender, EventArgs e)
        {
            int remainingTime = 5000 - (int)TimersElapsed.Elapsed.TotalMilliseconds;
            TimerLabel.Content = "Таймер: " + (remainingTime / 1000.0).ToString("0.000");
        }

        private void AddLineToTextBox(string line, TextBox textBox)
        {
            textBox.AppendText(line + Environment.NewLine);

            // Check if the number of lines exceeds 50
            if (textBox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Length > 50)
            {
                // Remove the text from the beginning up to the end of the 50th line
                int index = textBox.Text.IndexOf(Environment.NewLine);
                textBox.Text = textBox.Text.Substring(index + Environment.NewLine.Length);
            }
        }
    }
}
