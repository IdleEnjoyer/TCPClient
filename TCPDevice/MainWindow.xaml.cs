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
using System.Windows.Markup;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Cryptography;

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
        string CurrentDevice = "";
        bool Saved = false;

        public MainWindow()
        {
            InitializeComponent();
            DispTimer.Interval = TimeSpan.FromMilliseconds(10);
            DemoCommandList.ItemsSource = Commands;
            if (App.Current.Properties["LastOpenedProject"].ToString() != "None")
            {
                OpenProject(App.Current.Properties["LastOpenedProject"].ToString());
                Saved = true;
            }
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
                    ConnectionStatus.Content = "Подключено";
                    ConnectionStatus.Foreground = Brushes.Green;
                }
                else
                {
                    ConnectionStatus.Content = "Отключено";
                    ConnectionStatus.Foreground = Brushes.Red;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Status change error");
            }
        }

        private async Task StartReadingDataAsync()
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        
        public void SendData(string Data)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
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
            catch(Exception ex)
            {
                MessageBox.Show("Не удалось отправить команду!\nПроверьте подключение!");
            }
        }

        private void SendCmd_Click(object sender, RoutedEventArgs e)
        {
            Button? BTN = sender as Button;
            Grid? GRD = BTN.Parent as Grid;
            foreach(UIElement Child in GRD.Children)
            {
                if(Grid.GetColumn(Child) == 0 && Grid.GetRow(Child) == Grid.GetRow(BTN))
                {
                    TextBox? TB = Child as TextBox;
                    SendData(TB.Text);
                }
            }
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
                TextBox? TB = sender as TextBox;
                SendData(TB.Text);
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
            }
            catch (Exception )
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
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
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
                MessageBox.Show(ex.Message + "\n" + Commands.Count.ToString() + "\n" + Buttons.Count.ToString());
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
            Regex NumRegex = new Regex("[+-]?(\\d*\\.\\d+|\\d+\\.\\d*|\\d+)");
            return NumRegex.IsMatch(text);
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
                    C.CMD = $"{Words[i][0]}";
                    C.TMR = $"{Words[i][1]}";
                    Commands.Add(C);
                    i++;
                }
                MessageBox.Show("Импорт выполнен успешно");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Что-то пошло не так: " + ex.Message);
            }
        }

        private void StopConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Client.Close();
                ChangeConnection(Client.Connected);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Connection stop error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IPAddress Address = IPAddress.Parse(IPInput.Text);
                int Port = int.Parse(PortInput.Text);
                

                Client = new TcpClient(Address.ToString(), Port);

                Connect();
                ChangeConnection(Client.Connected);
            }
            catch (Exception ex)
            {
                ChangeConnection(false);
                MessageBox.Show(ex.Message, "Connection start error", MessageBoxButton.OK, MessageBoxImage.Question);
                return;
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog SFD = new SaveFileDialog();
                SFD.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                SFD.ShowDialog();
                FileStream FS = File.Create(SFD.FileName);
                StreamWriter SW = new StreamWriter(FS);

                foreach (Command Com in Commands)
                {
                    if (Com == Commands.Last())
                    {
                        SW.Write(Com.CMD + "\t" + Com.TMR);
                    }
                    else
                    {
                        SW.WriteLine(Com.CMD + "\t" + Com.TMR);
                    }
                    
                }
                SW.Close();
                FS.Close();
                SFD.Reset();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void CreateDevice(string XamlString)
        {
            CurrentDevice = XamlString;
            string[] Lines = CurrentDevice.Split('\n');
            DeviceTab.Content = null;
            ScrollViewer SV = new ScrollViewer();
            //<ScrollViewer x:Name="Viewer" Grid.Row="1" Grid.ColumnSpan="4" HorizontalScrollBarVisibility="Visible">
            SV.Name = "Viewer";
            SV.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            SV.Background = new SolidColorBrush(Color.FromRgb(0xC9,0xC9,0xC9));
            Grid? GR = XamlReader.Parse(Lines[1]) as Grid;
            foreach(UIElement Child in GR.Children)
            {
                if(Child.GetType() == typeof(Button))
                {
                    Button? BT = Child as Button;
                    BT.Click += (sender, e) =>
                    {
                        int Index = Grid.GetRow(BT);
                        int LastTBIndex = 1;
                        int NextTBIndex = 1;
                        bool Last = true;
                        foreach (UIElement TEMP in GR.Children)
                        {
                            if (TEMP.GetType() == typeof(Label) && Grid.GetColumn(TEMP) == 0 && Grid.GetRow(TEMP) <= Index)
                            {
                                LastTBIndex = Grid.GetRow(TEMP);
                            }
                            if (TEMP.GetType() == typeof(Label) && Grid.GetColumn(TEMP) == 0 && Grid.GetRow(TEMP) > Index)
                            {
                                NextTBIndex = Grid.GetRow(TEMP);
                                Last = false;
                                break;
                            }
                        }
                        if (Last)
                        {
                            NextTBIndex = GR.RowDefinitions.Count;
                        }
                        List<TextBox> Inputs = new List<TextBox>();
                        foreach (UIElement TEMP in GR.Children)
                        {
                            if (Grid.GetRow(TEMP) >= LastTBIndex && Grid.GetRow(TEMP) < NextTBIndex && TEMP.GetType() == typeof(TextBox) && Grid.GetColumn(TEMP) == Grid.GetColumn(BT))
                            {
                                TextBox? aTB = TEMP as TextBox;
                                Inputs.Add(aTB);
                            }
                        }
                        if (Inputs.Count > 1)
                        {
                            string Command = "";
                            foreach (TextBox TB in Inputs)
                            {
                                Command += TB.Text + " ";
                            }
                            SendData(BT.Resources["Command"].ToString() + " " + Command);
                        }
                        if (Inputs.Count == 1)
                        {
                            if (Inputs[0].Resources["First"].ToString() == " ")
                            {
                                Inputs[0].Resources["First"] = BT.Name;
                            }
                            if (Inputs[0].Resources["First"].ToString() == BT.Name)
                            {
                                string Command = BT.Resources["Command"].ToString() + " " + Inputs[0].Text;
                                SendData(Command);
                            }
                            else
                            {
                                if (Inputs[0].Resources["Second"].ToString() == " ")
                                {
                                    Inputs[0].Resources["Second"] = BT.Name;
                                }
                                if (Inputs[0].Resources["Second"].ToString() == BT.Name)
                                {
                                    string Command = BT.Resources["Command"].ToString() + " -" + Inputs[0].Text;
                                    SendData(Command);
                                }
                            }
                        }
                        if (Inputs.Count == 0)
                        {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          
                            SendData(BT.Resources["Command"].ToString());
                        }
                    };
                }
                if(Child.GetType() == typeof(TextBox))
                {
                    TextBox? TB = Child as TextBox;
                    TB.LostFocus += (sender, e) =>
                    {
                        if (!IsNumber(TB.Text))
                        {
                            TB.Text = "1.0";
                        }
                    };
                }
            }
            SV.Content = GR;
            DeviceTab.Content = SV;
            DeviceTab.Header = "Устройство";
        }


        private void Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog OFD = new OpenFileDialog();
                OFD.Filter = "Устройство (*.tesart)|*.tesart";
                OFD.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
                OFD.ShowDialog();
                if(OFD.FileName != string.Empty)
                {
                    StreamReader SR = new StreamReader(OFD.FileName);
                    string XamlString = SR.ReadLine() + '\n' + SR.ReadLine();
                    App.Current.Properties["LastOpenedProject"] = OFD.FileName;
                    CreateDevice(XamlString);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void OpenProject(string FilePath)
        {
            if(File.Exists(FilePath))
            {
                StreamReader SR = new StreamReader(FilePath);
                string XamlString = SR.ReadLine() + '\n' + SR.ReadLine();
                CreateDevice(XamlString);
            }
        }

        private void Redact_Click(object sender, RoutedEventArgs e)
        {
            Saved = false;
            AddProjectWindow APW = new AddProjectWindow();
            APW.Owner = this;
            APW.Show();
            APW.ImportProject(App.Current.Properties["LastOpenedProject"].ToString());
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveProject(CurrentDevice);
            Saved = true;
        }

        void SaveProject(string XamlString)
        {
            try
            {
                //string XAMLString = XamlWriter.Save(ProjectGrid);
                string FileName = $"Устройство_{DateTime.Today.Day}_{DateTime.Today.Month}_{DateTime.Today.Year}_{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}.tesart";
                FileStream FS = File.Create(FileName);
                StreamWriter SW = new StreamWriter(FS);
                SW.Write(XamlString, 0, XamlString.Length);
                App.Current.Properties["LastOpenedProject"] = FileName;
                SW.Close();
                FS.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!Saved)
            {
                if(MessageBox.Show("Последний проект не был сохранён, всё равно выйти?", "Выход" , MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
