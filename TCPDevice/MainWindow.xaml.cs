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
using System.Linq;
using Microsoft.Win32;

#pragma warning disable CS8618
#pragma warning disable CS8602

namespace TCPDevice
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Command> Commands = new();
        private List<Button> Buttons = new List<Button>();
        public TcpClient Client {get; set;}
        private NetworkStream Stream;
        private byte[] ByteData;
        private System.Timers.Timer PauseTime;
        private DispatcherTimer DispTimer = new DispatcherTimer();
        private Stopwatch TimersElapsed = new Stopwatch();
        private int CurrentTimerInterval = 0;
        private List<int> TimerIntervals = new List<int>();

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
                string ipAddress = (Client.Client.RemoteEndPoint as IPEndPoint).Address.MapToIPv4().ToString();
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
        
        void SendData(string Data)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string EndSymbol = EndSymbolInput.Text;
                string StartSymbol = StartSymbolInput.Text;
                EndSymbol = Regex.Unescape(EndSymbol);
                string DataString = StartSymbol + Data + EndSymbol;
                ByteData = System.Text.Encoding.ASCII.GetBytes(DataString);
                Stream.Write(ByteData, 0, ByteData.Length);
                ServerData.Text += "Клиент " + System.DateTime.Now.ToString() + ": " + DataString;
                ServerData.ScrollToEnd();
            });
        }

        private void SendCmd_Click(object sender, RoutedEventArgs e)
        {
            SendData(CommandInput.Text);
        }

        private void SendCmd1_Click(object sender, RoutedEventArgs e)
        {
            SendData(CommandInput1.Text);
        }

        private void SendCmd2_Click(object sender, RoutedEventArgs e)
        {
            SendData(CommandInput2.Text);
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
            AddProjectWindow NewTab = new AddProjectWindow();
            NewTab.Owner = this;
            NewTab.Show();
        }

        private void TimerStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TimerIntervals.Clear();
                for(int ItemId = 0;  ItemId < Commands.Count; ItemId++)
                {
                    TimerIntervals.Add(int.Parse(Commands[ItemId].TMR));
                }
                PauseTime = new System.Timers.Timer();
                PauseTime.Elapsed += PauseTime_Elapsed;
                SendData(Commands[CurrentTimerInterval].CMD);
                PauseTime.Interval = TimerIntervals[0]*1000;
                DemoCommandList.SelectedItem = DemoCommandList.Items[0];
                PauseTime.Start();
                TimersElapsed.Start();
                DispTimer.Tick += DispTimer_Tick;
                DispTimer.Start();
                CommandGrid.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Очередь таймера пуста");
            }
        }

        private void DispTimer_Tick(object? sender, EventArgs e)
        {
            int TimeElapsed = TimerIntervals[CurrentTimerInterval] * 1000 - (int)TimersElapsed.Elapsed.TotalMilliseconds;
            TimerLabel.Content = $"{TimeSpan.FromMilliseconds(TimeElapsed).TotalSeconds:F3}";
        }

        private void PauseTime_Elapsed(object? sender, ElapsedEventArgs e)
        {
            //SendData("Check" + TimerIntervals[CurrentTimerInterval]);
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    
                    if (CurrentTimerInterval >= TimerIntervals.Count - 1)
                    {
                        CurrentTimerInterval = 0;
                    }
                    else
                    {
                        CurrentTimerInterval++;
                    }
                    SendData(Commands[CurrentTimerInterval].CMD);
                    PauseTime.Interval = TimerIntervals[CurrentTimerInterval] * 1000;
                    DemoCommandList.SelectedItem = DemoCommandList.Items[CurrentTimerInterval];
                    TimersElapsed.Restart();
                });
            }
            catch (Exception ex)
            {
                PauseTime.Stop();
                MessageBox.Show(ex.Message);
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button? Self = sender as Button;
                Commands.RemoveAt(Buttons.IndexOf(Self));
                Buttons.RemoveAt(Buttons.IndexOf(Self));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DeleteItem_Initialized(object sender, EventArgs e)
        {
            Button? button = sender as Button;
            Buttons.Add(button);
        } 

        private void TimerStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PauseTime.Stop();
                PauseTime.Elapsed -= PauseTime_Elapsed;
                CurrentTimerInterval = 0;
                TimersElapsed.Reset();
                DispTimer.Stop();
                DispTimer.Tick -= DispTimer_Tick;
                CommandGrid.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TMR_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if(!IsNumber(e.Text))
            {
                e.Handled = true;
            }
        }

        private bool IsNumber(string text)
        {
            Regex NumRegex = new Regex("[^0-9]+");
            return !NumRegex.IsMatch(text);
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog Dial = new OpenFileDialog();
                Dial.ShowDialog(this);
                List<string[]> Words = new List<string[]>();
                StreamReader ImportFileStream = new StreamReader(Dial.FileName);
                int i = 0;
                Commands.Clear();
                while(!ImportFileStream.EndOfStream)
                {
                    Words.Add(ImportFileStream.ReadLine().Split('\t'));
                    Command C = new Command();
                    C.CMD = $"{Words[i][0]} {Words[i][1]}";
                    C.TMR = $"{Words[i][2]}";
                    Commands.Add(C);
                    i++;
                }
                MessageBox.Show("Импорт выполнен успешно");
            }
            catch
            {
                MessageBox.Show("Что-то пошло не так");
            }
        }
    }
}
