using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

#pragma warning disable CS8618
#pragma warning disable CS8602
#pragma warning disable CS8604

namespace TCPDevice
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Command> Commands = new();
        private ObservableCollection<Command> CommandsSerial = new();
        private List<Button> Buttons = new();
        private List<Button> ButtonsSerial = new();
        public TcpClient Client { get; set; }
        private NetworkStream Stream;
        private byte[] ByteData;
        private System.Timers.Timer PauseTime;
        private DispatcherTimer DispTimer = new();
        private DispatcherTimer PortCheckTimer = new();
        private Stopwatch TimersElapsed = new();
        private int CurrentTimerInterval = 0;
        private List<int> TimerIntervals = new();
        private string CurrentDevice = "";
        private string CurrentSerial = "";
        private bool Saved = true;
        public SerialPortTracker PortTracker;
        [GeneratedRegex("[+-]?(\\d*\\.\\d+|\\d+\\.\\d*|\\d+)")]
        private static partial Regex MyRegex();

        public MainWindow()
        {
            InitializeComponent();
            PortTracker = new SerialPortTracker(this);
            DataContext = PortTracker;
            DispTimer.Interval = TimeSpan.FromMilliseconds(100);
            PortCheckTimer.Interval = TimeSpan.FromMilliseconds(100);
            PortCheckTimer.Tick += PortCheckTimer_Tick;
            PortCheckTimer.Start();
            DemoCommandList.ItemsSource = Commands;
            DemoCommandListCom.ItemsSource = CommandsSerial;
            if (!App.Current.Properties["LastOpenedProject"].ToString().Contains("NULL"))
            {
                //MessageBox.Show(App.Current.Properties["LastOpenedProject"].ToString());
                CreateDevice(App.Current.Properties["LastOpenedProject"].ToString(), 1);
            }
            if (!App.Current.Properties["LastOpenedSerial"].ToString().Contains("NULL"))
            {
                CreateDevice(App.Current.Properties["LastOpenedSerial"].ToString(), 2);
            }
        }
        private void PortCheckTimer_Tick(object? sender, EventArgs e)
        {
            PortTracker.CheckSerialPorts();
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
            catch (Exception ex)
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
                        ServerData.Text += "Сервер " + System.DateTime.Now.ToLongTimeString() + ": " + Data;
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
                ChangeConnection(false);
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
            catch (Exception)
            {
                MessageBox.Show("Не удалось отправить команду!\nПроверьте подключение!");
            }
        }
        public void SendDataCom(string Data, SerialPort Port)
        {
            try
            {
                Port.Write(Data+"\r\n");
                ComData.Text += $"Клиент {System.DateTime.Now.ToLongTimeString()}: " + Data + "\n";
                ComData.ScrollToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void SendCmd_Click(object sender, RoutedEventArgs e)
        {
            Button? BTN = sender as Button;
            Grid? GRD = BTN.Parent as Grid;
            foreach (UIElement Child in GRD.Children)
            {
                if (Grid.GetColumn(Child) == 0 && Grid.GetRow(Child) == Grid.GetRow(BTN))
                {
                    TextBox? TB = Child as TextBox;
                    SendData(TB.Text);
                }
            }
        }
        private void OnPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox? TB = sender as TextBox;
                SendData(TB.Text);
            }
        }
        private void AddCommand_Click(object sender, RoutedEventArgs e)
        {
            Command Input = new() { CMD = DemoCommandInput.Text, TMR = DemoTimerInput.Text };
            Commands.Add(Input);
        }
        private void AddCommandCom_Click(object sender, RoutedEventArgs e)
        {
            Command Input = new() { CMD = DemoCommandInputCom.Text, TMR = DemoTimerInputCom.Text };
            CommandsSerial.Add(Input);
        }
        private void AddTab_Click(object sender, RoutedEventArgs e)
        {
            Saved = false;
            AddProjectWindow NewTab = new()
            {
                Owner = this
            };
            NewTab.Show();
        }
        private void TimerStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TimerIntervals.Clear();
                Button? BT = sender as Button;
                if (!BT.Name.Contains("Com")){
                    foreach (Command Item in DemoCommandList.Items)
                    {
                        Commands[DemoCommandList.Items.IndexOf(Item)].CMD = Item.CMD;
                        Commands[DemoCommandList.Items.IndexOf(Item)].TMR = Item.TMR;
                    }
                    for (int ItemId = 0; ItemId < Commands.Count; ItemId++)
                    {
                        TimerIntervals.Add(int.Parse(Commands[ItemId].TMR));
                    }
                    PauseTime = new System.Timers.Timer();
                    PauseTime.Elapsed += PauseTime_Elapsed;
                    SendData(Commands[CurrentTimerInterval].CMD);
                    PauseTime.Interval = TimerIntervals[0];
                    DemoCommandList.SelectedItem = DemoCommandList.Items[0];
                    PauseTime.Start();
                    TimersElapsed.Start();
                    DispTimer.Tick += DispTimer_Tick;
                    DispTimer.Start();
                }
                else
                {
                    foreach (Command Item in DemoCommandListCom.Items)
                    {
                        CommandsSerial[DemoCommandListCom.Items.IndexOf(Item)].CMD = Item.CMD;
                        CommandsSerial[DemoCommandListCom.Items.IndexOf(Item)].TMR = Item.TMR;
                    }
                    for (int ItemId = 0; ItemId < CommandsSerial.Count; ItemId++)
                    {
                        TimerIntervals.Add(int.Parse(CommandsSerial[ItemId].TMR));
                    }
                    PauseTime = new System.Timers.Timer();
                    PauseTime.Elapsed += PauseTimeCom_Elapsed;
                    SendDataCom(CommandsSerial[CurrentTimerInterval].CMD, PortTracker.GetPort(PortNumber.SelectedItem.ToString()));
                    PauseTime.Interval = TimerIntervals[0];
                    DemoCommandListCom.SelectedItem = DemoCommandListCom.Items[0];
                    PauseTime.Start();
                    TimersElapsed.Start();
                    DispTimer.Tick += DispTimerCom_Tick;
                    DispTimer.Start();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Очередь таймера пуста");
            }
        }
        private void DispTimer_Tick(object? sender, EventArgs e)
        {
            int TimeElapsed = TimerIntervals[CurrentTimerInterval] - (int)TimersElapsed.Elapsed.TotalMilliseconds;
            TimerLabel.Content = $"{TimeSpan.FromMilliseconds(TimeElapsed).TotalSeconds:F3}";
        }
        private void DispTimerCom_Tick(object? sender, EventArgs e)
        {
            int TimeElapsed = TimerIntervals[CurrentTimerInterval] - (int)TimersElapsed.Elapsed.TotalMilliseconds;
            TimerLabelCom.Content = $"{TimeSpan.FromMilliseconds(TimeElapsed).TotalSeconds:F3}";
        }
        private void PauseTime_Elapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if(CurrentTimerInterval >= TimerIntervals.Count - 1)
                    {
                        CurrentTimerInterval = 0;
                    }
                    else
                    {
                        CurrentTimerInterval++;
                    }
                    SendData(Commands[CurrentTimerInterval].CMD);
                    PauseTime.Interval = TimerIntervals[CurrentTimerInterval];
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

        private void PauseTimeCom_Elapsed(object? sender, ElapsedEventArgs e)
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
                    SendDataCom(CommandsSerial[CurrentTimerInterval].CMD, PortTracker.GetPort(PortNumber.SelectedItem.ToString()));
                    PauseTime.Interval = TimerIntervals[CurrentTimerInterval];
                    DemoCommandListCom.SelectedItem = DemoCommandListCom.Items[CurrentTimerInterval];
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
        private void DeleteItemCom_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button? Self = sender as Button;
                CommandsSerial.RemoveAt(ButtonsSerial.IndexOf(Self));
                ButtonsSerial.RemoveAt(ButtonsSerial.IndexOf(Self));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + CommandsSerial.Count.ToString() + "\n" + ButtonsSerial.Count.ToString());
            }
        }
        private void DeleteItem_Initialized(object sender, EventArgs e)
        {
            Button? button = sender as Button;
            Buttons.Add(button);
        }
        private void DeleteItemCom_Initialized(object sender, EventArgs e)
        {
            Button? button = sender as Button;
            ButtonsSerial.Add(button);
        }
        private void TimerStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button? BT = sender as Button;
                PauseTime.Stop();
                if (!BT.Name.Contains("Com"))
                {
                    PauseTime.Elapsed -= PauseTime_Elapsed;
                    CommandGrid.IsEnabled = true;
                    CommandGridCom.IsEnabled = true;
                }
                else
                {
                    PauseTime.Elapsed -= PauseTimeCom_Elapsed;
                    CommandGridCom.IsEnabled = true;
                    CommandGrid.IsEnabled = true;
                }
                CurrentTimerInterval = 0;
                TimersElapsed.Reset();
                DispTimer.Stop();
                DispTimer.Tick -= DispTimer_Tick;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void TMR_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsNumber(e.Text))
            {
                e.Handled = true;
            }
        }
        static private bool IsNumber(string text)
        {
            Regex NumRegex = MyRegex();
            return NumRegex.IsMatch(text);
        }
        private void Import_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog Dial = new();
                Dial.ShowDialog(this);
                List<string[]> Words = new();
                StreamReader ImportFileStream = new(Dial.FileName);
                int i = 0;
                Button? BT = sender as Button;
                if (!BT.Name.Contains("Com"))
                {
                    Commands.Clear();
                }
                else
                {
                    CommandsSerial.Clear();
                }
                while (!ImportFileStream.EndOfStream)
                {
                    Words.Add(ImportFileStream.ReadLine().Split('\t'));
                    Command C = new()
                    {
                        CMD = $"{Words[i][0]}",
                        TMR = $"{Words[i][1]}"
                    };
                    if (!BT.Name.Contains("Com"))
                    {
                        Commands.Add(C);
                    }
                    else
                    {
                        CommandsSerial.Add(C);
                    }
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
                SaveFileDialog SFD = new()
                {
                    InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
                };
                SFD.ShowDialog();
                FileStream FS = File.Create(SFD.FileName);
                StreamWriter SW = new(FS);

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
        public void CreateDevice(string XamlString, int Type)
        {
            string[] Lines = new string[0];
            if (Type == 1)
            {
                CurrentDevice = XamlString;
                Lines = CurrentDevice.Split('\t');
                DeviceTab.Content = null;
            }
            if (Type == 2)
            {
                CurrentSerial = XamlString;
                Lines = CurrentSerial.Split("\t");
                DeviceTabCom.Content = null;
            }
            ScrollViewer SV = new()
            {
                Name = "Viewer",
                HorizontalScrollBarVisibility = ScrollBarVisibility.Visible
            };
            Grid? GR = XamlReader.Parse(Lines[1]) as Grid;
            foreach (UIElement Child in GR.Children)
            {
                if (Child.GetType() == typeof(Button))
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
                        List<TextBox> Inputs = new();
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
                                Command += TB.Text.Replace(",", ".") + " ";
                            }
                            if (Type == 1)
                            {
                                SendData(BT.Resources["Command"].ToString() + " " + Command);
                            }
                            if (Type == 2)
                            {
                                SendDataCom(BT.Resources["Command"].ToString() + " " + Command, PortTracker.FixedPorts[Grid.GetColumn(BT) - 1]);
                            }
                        }
                        if (Inputs.Count == 1)
                        {
                            if (Inputs[0].Resources["First"].ToString() == " ")
                            {
                                Inputs[0].Resources["First"] = BT.Name;
                            }
                            if (Inputs[0].Resources["First"].ToString() == BT.Name)
                            {
                                string Command = BT.Resources["Command"].ToString() + " " + Inputs[0].Text.Replace(",", ".");
                                if (Type == 1)
                                {
                                    SendData(Command);
                                }
                                if (Type == 2)
                                {
                                    SendDataCom(Command, PortTracker.FixedPorts[Grid.GetColumn(BT) - 1]);
                                }
                            }
                            else
                            {
                                if (Inputs[0].Resources["Second"].ToString() == " ")
                                {
                                    Inputs[0].Resources["Second"] = BT.Name;
                                }
                                if (Inputs[0].Resources["Second"].ToString() == BT.Name)
                                {
                                    string Command = BT.Resources["Command"].ToString() + " -" + Inputs[0].Text.Replace(",", ".");
                                    if (Type == 1)
                                    {
                                        SendData(Command);
                                    }
                                    if (Type == 2)
                                    {
                                        SendDataCom(Command, PortTracker.FixedPorts[Grid.GetColumn(BT) - 1]);
                                    }
                                }
                            }
                        }
                        if (Inputs.Count == 0)
                        {
                            if (Type == 1)
                            {
                                SendData(BT.Resources["Command"].ToString());
                            }
                            if (Type == 2)
                            {
                                SendDataCom(BT.Resources["Command"].ToString(), PortTracker.FixedPorts[Grid.GetColumn(BT) - 1]);
                            }
                        }
                    };
                }
                if (Child.GetType() == typeof(TextBox))
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
            if (Type == 1)
            {
                SV.Background = new SolidColorBrush(Color.FromRgb(0xC9, 0xC9, 0xC9));
                DeviceTab.Content = SV;
                DeviceTab.Header = "Устройство";
            }
            else
            {
                SV.Background = new SolidColorBrush(Color.FromRgb(0xB9, 0xB9, 0xB9));
                DeviceTabCom.Content = SV;
                DeviceTabCom.Header = "Устройство";
                string[] LoadedPorts = Lines[2].Split("/");
                foreach (string Item in LoadedPorts)
                {
                    SerialPort SP = new(Item, 115200);
                    try
                    {
                        PortTracker.FixedPorts.Add(SP);
                        PortTracker.FixedPorts.Last().Open();
                    }
                    catch
                    {

                    }
                }
                foreach (SerialPort Item in PortTracker.FixedPorts)
                {
                    Item.DataReceived += (sender, e) =>
                    {
                        try
                        {
                            SerialPort? SP = sender as SerialPort;
                            ComDataRecieve(SP, SP.ReadTo("-->"));
                        }
                        catch (Exception ex)
                        {
                            ComData.Text += "Сервер" + System.DateTime.Now.ToLongTimeString() + $": {ex.Message}\n";
                        }
                    };
                }
            }
        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog OFD = new();
                MenuItem? MI = sender as MenuItem;
                if (!MI.Name.Contains("Com"))
                {
                    OFD.Filter = "Устройство (*.tesart)|*.tesart";
                    OFD.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
                    OFD.ShowDialog();
                    if (OFD.FileName != string.Empty)
                    {
                        StreamReader SR = new(OFD.FileName);
                        string XamlString = SR.ReadLine() + '\t' + SR.ReadLine();
                        App.Current.Properties["LastOpenedProject"] = XamlString;
                        CreateDevice(XamlString, 1);
                        SR.Dispose();
                    }
                }
                else
                {
                    OFD.Filter = "Устройство (*.comtesart)|*.comtesart";
                    OFD.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
                    OFD.ShowDialog();
                    if (OFD.FileName != string.Empty)
                    {
                        StreamReader SR = new(OFD.FileName);
                        string XamlString = SR.ReadLine() + '\t' + SR.ReadLine() + "\t" + SR.ReadLine();
                        App.Current.Properties["LastOpenedSerial"] = XamlString;
                        CreateDevice(XamlString, 2);
                        SR.Dispose();
                    }
                }
                OFD.Reset();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Redact_Click(object sender, RoutedEventArgs e)
        {
            Saved = false;
            MenuItem? MI = sender as MenuItem;
            if (!MI.Name.Contains("Com"))
            {
                AddProjectWindow APW = new()
                {
                    Owner = this
                };
                APW.Show();
                APW.ImportProject(App.Current.Properties["LastOpenedProject"].ToString());
            }
            else
            {
                AddSerial AS = new()
                {
                    Owner = this
                };
                AS.Show();
                AS.ImportProject(App.Current.Properties["LastOpenedSerial"].ToString());
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MenuItem? MI = sender as MenuItem;
            if (!MI.Name.Contains("Com"))
            {
                SaveProject(CurrentDevice, 1);
            }
            else
            {
                SaveProject(CurrentSerial, 2);
            }
            Saved = true;
        }
        private void SaveProject(string XamlString, int Type)
        {
            try
            {
                SaveFileDialog SFD = new();
                if (Type == 1)
                {
                    SFD.Filter = "Устройство (*.tesart)|*.tesart";
                    SFD.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
                    SFD.ShowDialog();
                    if (SFD.FileName != null)
                    {
                        //string XAMLString = XamlWriter.Save(ProjectGrid);
                        //string FileName = $"Устройство_{DateTime.Today.Day}_{DateTime.Today.Month}_{DateTime.Today.Year}_{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}.tesart";
                        FileStream FS = File.Create(SFD.FileName);
                        StreamWriter SW = new(FS);
                        SW.Write(XamlString, 0, XamlString.Length);
                        App.Current.Properties["LastOpenedProject"] = XamlString;
                        SW.Close();
                        FS.Close();
                    }
                }
                if (Type == 2)
                {
                    SFD.Filter = "Устройство (*.comtesart)|*.comtesart";
                    SFD.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
                    SFD.ShowDialog();
                    if (SFD.FileName != null)
                    {
                        //string XAMLString = XamlWriter.Save(ProjectGrid);
                        //string FileName = $"Устройство_{DateTime.Today.Day}_{DateTime.Today.Month}_{DateTime.Today.Year}_{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}.tesart";
                        FileStream FS = File.Create(SFD.FileName);
                        StreamWriter SW = new(FS);
                        XamlString += $"\t{string.Join("/", PortTracker.FixedPorts.Select(x => x.PortName).ToArray())}";
                        SW.Write(XamlString, 0, XamlString.Length);
                        App.Current.Properties["LastOpenedSerial"] = XamlString;
                        SW.Close();
                        FS.Close();
                    }
                }
                SFD.Reset();
            }
            catch (System.Exception)
            {
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!Saved)
            {
                if (MessageBox.Show("Последний проект не был сохранён, всё равно выйти?", "Выход", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
        private void PortNumber_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.RemovedItems.Count > 0)
                {
                    PortTracker.GetPort(e.RemovedItems[0].ToString()).Close();
                }
                if (!PortTracker.GetPort(PortNumber.SelectedItem.ToString()).IsOpen)
                {
                    PortTracker.GetPort(PortNumber.SelectedItem.ToString()).Open();
                }
                PortOpened.Header = $"Opened: {PortTracker.GetPort(PortNumber.SelectedItem.ToString()).IsOpen}";
            }
            catch (Exception ex)
            {
                PortTracker.GetPort(PortNumber.SelectedItem.ToString()).Close();
                MessageBox.Show(ex.Message);
                PortOpened.Header = $"Opened: False";
            }
        }
        private void ComInput_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (PortNumber.SelectedIndex != -1)
                    {
                        TextBox? TB = sender as TextBox;
                        SendDataCom(TB.Text, PortTracker.GetPort(PortNumber.SelectedItem.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ComSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PortNumber.SelectedIndex != -1)
                {
                    Button? BT = sender as Button;
                    foreach (UIElement Child in ChatGridCom.Children)
                    {
                        if (Child.GetType() == typeof(TextBox) && ((TextBox)Child).Name.Last() == BT.Name.Last())
                        {
                            SendDataCom(((TextBox)Child).Text, PortTracker.GetPort(PortNumber.SelectedItem.ToString()));
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void ComDataRecieve(string Data)
        {
            this.Dispatcher.Invoke(() =>
            {
                Data = Data.Replace("\r", " ").Replace("\n", "");
                ComData.Text += $"{PortNumber.SelectedItem} " + System.DateTime.Now.ToLongTimeString() + ": " + Data + "\n";
                ComData.ScrollToEnd();
            });
        }
        public void ComDataRecieve(SerialPort SP, string Data)
        {
            this.Dispatcher.Invoke(() =>
            {
                Data = Data.Replace("\r", " ").Replace("\n", "");
                ComData.Text += $"{SP.PortName} " + System.DateTime.Now.ToLongTimeString() + ": " + Data + "\n";
                ComData.ScrollToEnd();
            });
        }
        private void AddCom_Click(object sender, RoutedEventArgs e)
        {
            Saved = false;
            AddSerial NewTab = new()
            {
                Owner = this
            };
            NewTab.Show();
        }

        private void Reconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PortTracker.GetPort(PortNumber.SelectedItem.ToString()).Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
