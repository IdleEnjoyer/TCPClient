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

#pragma warning disable CS8618
#pragma warning disable CS8602

namespace TCPDevice
{
    public partial class MainWindow : Window
    {
        private TcpClient Client;
        private NetworkStream Stream;
        private byte[] ByteData;
        private double CurrentPos1 = 0;
        private double CurrentPos2 = 0;
        private int LoopCycle1 = 1;
        private int LoopCycle2 = 1;
        private System.Timers.Timer PauseTime;
        private DispatcherTimer DispTimer = new DispatcherTimer();
        private Stopwatch TimersElapsed = new Stopwatch();
        public MainWindow()
        {
            InitializeComponent();
            DispTimer.Interval = TimeSpan.FromMilliseconds(10);
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
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message);
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
                MessageBox.Show(ex.Message);
            }
        }

        private void AbortBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SendData("STOP");
                PauseTime.Stop();
                PauseTime.Elapsed -= PauseTime1_Elapsed;
                PauseTime.Elapsed -= PauseTime2_Elapsed;
                DispTimer.Stop();
                TimersElapsed.Restart();
                TimersElapsed.Stop();
                foreach (UIElement item in CmdGrid.Children)
                {
                     item.IsEnabled = true;
                }
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

        private void TarPosSend1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double Pos = double.Parse(TarPosInput1.Text.Replace(".", ","));
                double Speed = double.Parse(SpeedInput1.Text.Replace(".", ","));
                string StateData = "MOVEA1 " + Pos.ToString().Replace(",",".") + " " + Speed.ToString().Replace(",", ".");
                CurrentPos1 = Pos;
                SendData(StateData);
                CurPosLabel1.Content = CurrentPos1.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TarPosSend2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double Pos = double.Parse(TarPosInput2.Text.Replace(".", ","));
                double Speed = double.Parse(SpeedInput2.Text.Replace(".", ","));
                string StateData = "MOVEA2 " + Pos.ToString().Replace(",", ".") + " " + Speed.ToString().Replace(",", ".");
                CurrentPos2 = Pos;
                SendData(StateData);
                CurPosLabel2.Content = CurrentPos2.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SendCmd_Click(object sender, RoutedEventArgs e)
        {
            TextBox TB = (TextBox)sender;
            if (TB.Name.Contains("TarPosInput1"))
            {
                double Pos = double.Parse(TarPosInput1.Text.Replace(".", ","));
                double Speed = double.Parse(SpeedInput1.Text.Replace(".", ","));
                string StateData = "MOVEA1 " + Pos.ToString().Replace(",", ".") + " " + Speed.ToString().Replace(",", ".");
                CurrentPos1 = Pos;
                SendData(StateData);
                CurPosLabel1.Content = CurrentPos1.ToString();
            }
            else if (TB.Name.Contains("TarPosInput2"))
            {
                double Pos = double.Parse(TarPosInput2.Text.Replace(".", ","));
                double Speed = double.Parse(SpeedInput2.Text.Replace(".", ","));
                string StateData = "MOVEA2 " + Pos.ToString().Replace(",", ".") + " " + Speed.ToString().Replace(",", ".");
                CurrentPos2 = Pos;
                SendData(StateData);
                CurPosLabel2.Content = CurrentPos2.ToString();
            }
            else
            {
                SendData(TB.Text);
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
                SendCmd_Click((TextBox)sender, e);
            }
        }

        private void LeftStep1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentPos1 - double.Parse(MoveInput1.Text.Replace(".", ",")) >= -100.0)
                {
                    CurrentPos1 = CurrentPos1 - double.Parse(MoveInput1.Text.Replace(".", ","));
                    double Speed = double.Parse(SpeedInput1.Text.Replace(".", ","));
                    SendData("MOVEA1 " + CurrentPos1.ToString().Replace(",", ".") + " " + Speed.ToString().Replace(",", "."));
                    CurPosLabel1.Content = CurrentPos1.ToString();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RightStep1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(CurrentPos1 + double.Parse(MoveInput1.Text.Replace(".", ",")) <= 100.0) {
                    CurrentPos1 = CurrentPos1 + double.Parse(MoveInput1.Text.Replace(".", ","));
                    double Speed = double.Parse(SpeedInput1.Text.Replace(".", ","));
                    SendData("MOVEA1 " + CurrentPos1.ToString().Replace(",", ".") + " " + Speed.ToString().Replace(",", "."));
                    CurPosLabel1.Content = CurrentPos1.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LeftStep2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentPos2 - double.Parse(MoveInput2.Text.Replace(".", ",")) >= -100.0)
                {
                    CurrentPos2 = CurrentPos2 - double.Parse(MoveInput2.Text.Replace(".", ","));
                    double Speed = double.Parse(SpeedInput2.Text.Replace(".", ","));
                    SendData("MOVEA2 " + CurrentPos2.ToString().Replace(",", ".") + " " + Speed.ToString().Replace(",", "."));
                    CurPosLabel2.Content = CurrentPos2.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RightStep2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentPos2 + double.Parse(MoveInput2.Text.Replace(".", ",")) <= 100.0)
                {
                    CurrentPos2 = CurrentPos2 + double.Parse(MoveInput2.Text.Replace(".", ","));
                    double Speed = double.Parse(SpeedInput2.Text.Replace(".", ","));
                    SendData("MOVEA2 " + CurrentPos2.ToString().Replace(",", ".") + " " + Speed.ToString().Replace(",", "."));
                    CurPosLabel2.Content = CurrentPos2.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void WalkingStart1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int Time = (int)(double.Parse(PauInput1.Text) * 1000);
                PauseTime = new System.Timers.Timer(Time);
                PauseTime.AutoReset = true;
                PauseTime.Elapsed += PauseTime1_Elapsed;
                PauseTime.Start();
                DispTimer.Tick += DispTimer1_Tick;
                DispTimer.Start();
                TimersElapsed.Start();
                foreach (UIElement item in CmdGrid.Children)
                {
                    if (item != AbortBtn && item.GetType() != typeof(Label))
                    {
                        item.IsEnabled = false;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DispTimer1_Tick(object? sender, EventArgs e)
        {
            int remainingTime = (int)(double.Parse(PauInput1.Text) * 1000) - (int)TimersElapsed.Elapsed.TotalMilliseconds;
            TimerLabel.Content = (remainingTime / 1000.0).ToString("0.000");
        }

        private void PauseTime1_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {

            this.Dispatcher.Invoke(() =>
            {
                if (CurrentPos1 + double.Parse(MoveInput1.Text.Replace(".", ",")) > 100.0 || (CurrentPos1 - double.Parse(MoveInput1.Text.Replace(".", ",")) < -100.0))
                {
                    LoopCycle1 *= -1;
                }
                CurrentPos1 = CurrentPos1 + LoopCycle1 * double.Parse(MoveInput1.Text.Replace(".", ","));
                double Speed = double.Parse(SpeedInput1.Text.Replace(".", ","));
                SendData("MOVEA1 " + CurrentPos1.ToString().Replace(",", ".") + " " + Speed.ToString().Replace(",", "."));
                CurPosLabel1.Content = CurrentPos1.ToString();
                TimersElapsed.Restart();
            });
        }

        private void WalkingStart2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int Time = (int)(double.Parse(PauInput2.Text) * 1000);
                PauseTime = new System.Timers.Timer(Time);
                PauseTime.AutoReset = true;
                PauseTime.Elapsed += PauseTime2_Elapsed;
                PauseTime.Start();
                DispTimer.Tick += DispTimer2_Tick;
                DispTimer.Start();
                TimersElapsed.Start();
                foreach (UIElement item in CmdGrid.Children)
                {
                    if(item != AbortBtn && item.GetType() != typeof(Label))
                    {
                        item.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DispTimer2_Tick(object? sender, EventArgs e)
        {
            int remainingTime = (int)(double.Parse(PauInput2.Text) * 1000) - (int)TimersElapsed.Elapsed.TotalMilliseconds;
            TimerLabel.Content = (remainingTime / 1000.0).ToString("0.000");
        }

        private void PauseTime2_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {

            this.Dispatcher.Invoke(() =>
            {
                if (CurrentPos2 + double.Parse(MoveInput2.Text.Replace(".", ",")) > 100.0 || (CurrentPos2 - double.Parse(MoveInput2.Text.Replace(".", ",")) < -100.0))
                {
                    LoopCycle2 *= -1;
                }
                CurrentPos2 = CurrentPos2 + LoopCycle2 * double.Parse(MoveInput2.Text.Replace(".", ","));
                double Speed = double.Parse(SpeedInput2.Text.Replace(".", ","));
                SendData("MOVEA2 " + CurrentPos2.ToString().Replace(",", ".") + " " + Speed.ToString().Replace(",", "."));
                CurPosLabel2.Content = CurrentPos2.ToString();
                TimersElapsed.Restart();
            });
        }
    }
}
