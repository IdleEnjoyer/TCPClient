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
using System.Threading;
using System.Windows.Media.Animation;
using System.Timers;
using System.Windows.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#pragma warning disable CS8618
#pragma warning disable CS8602

namespace TCPDevice
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Command> Commands = new();
        public TcpClient Client {get; set;}
        private NetworkStream Stream;
        private byte[] ByteData;
        private System.Timers.Timer PauseTime;
        private DispatcherTimer DispTimer = new DispatcherTimer();
        private Stopwatch TimersElapsed = new Stopwatch();
        public MainWindow()
        {
            InitializeComponent();
            DispTimer.Interval = TimeSpan.FromMilliseconds(10);
            DemoCommandList.ItemsSource = Commands;
        }

        public class Command
        {
            public string CMD { get; set; }
            public string TMR { get; set; }
        }
        public async void Connect()
        {
            try
            {
                string ipAddress = ((IPEndPoint)Client.Client.RemoteEndPoint).Address.MapToIPv4().ToString();
                IP.Header = ipAddress;
                Stream = Client.GetStream();

                await StartReadingDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Connect init error");
                return;
            }
        }

        public void ChangeConnection(bool State)
        {
            try
            {
                if (State)
                {
                    ConnectionStatus.Header = "Подключено";
                    ConnectionStatus.Background = Brushes.Green;
                }
                else
                {
                    ConnectionStatus.Header = "Отключено";
                    ConnectionStatus.Background = Brushes.Red;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Status change error");
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
                        ConnectionStatus.Header = "Отключен";
                        ConnectionStatus.Background = Brushes.Red;
                        MessageBox.Show("Сервер закрыт!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        break;
                    }
                    string Data = Encoding.ASCII.GetString(Buffer, 0, BytesRead);
                    ServerData.Text += "Сервер " + System.DateTime.Now.ToString() + ": " + Data + "\n";
                    ServerData.ScrollToEnd();
                }
                catch (IOException)
                {
                    MessageBox.Show("Подключение было прервано!", "Data reading error");
                    break;
                }
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
                MessageBox.Show(ex.Message, "Enabling error");
            }
        }

        //private void AbortBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        this.Owner.Show();
        //        SendData("STOP");
        //        PauseTime.Stop();
        //        PauseTime.Elapsed -= PauseTime1_Elapsed;
        //        PauseTime.Elapsed -= PauseTime2_Elapsed;
        //        DispTimer.Stop();
        //        TimersElapsed.Restart();
        //        TimersElapsed.Stop();
        //        foreach (UIElement item in CmdGrid.Children)
        //        {
        //             item.IsEnabled = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}
        
        void SendData(string Data)
        {
            string EndSymbol = EndSymbolInput.Text;
            string StartSymbol = StartSymbolInput.Text;
            EndSymbol = Regex.Unescape(EndSymbol);
            string DataString = StartSymbol + Data + EndSymbol;
            ByteData = System.Text.Encoding.ASCII.GetBytes(DataString);
            Stream.Write(ByteData, 0, ByteData.Length);
            ServerData.Text += "Клиент " + System.DateTime.Now.ToString() + ": " + DataString;
            ServerData.ScrollToEnd();
        }

        private void SendCmd_Click(object sender, RoutedEventArgs e)
        {
            SendData(CommandInput.Text);
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

        private void AddCommand_Click(object sender, RoutedEventArgs e)
        {
            Command Input = new Command { CMD = DemoCommandInput.Text, TMR = DemoTimerInput.Text };
            Commands.Add(Input);
        }

        private void AddTab_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
