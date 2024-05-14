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
        private bool LoopCycle = false;
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
                    if (CurrentPos < 0)
                    {
                        CurrentPos = double.Parse(Data);
                    }
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

        //private async Task ServerResponseAsync()
        //{
        //    byte[] Buffer = new byte[1024];
        //    System.Timers.Timer Timeout = new System.Timers.Timer(5000);
        //    Timeout.Elapsed += Timeout_Elapsed;
        //    Timeout.Start();
        //    while (Client.Connected && Waiting)
        //    {
        //        try
        //        {
        //            Task<int> bytesReadTask = Stream.ReadAsync(Buffer).AsTask();
        //            Task completedTask = await Task.WhenAny(bytesReadTask, Task.Delay(TimeSpan.FromSeconds(5)));
        //            if(completedTask == bytesReadTask)
        //            {
        //                int bytesRead = await bytesReadTask;
        //                MessageBox.Show(bytesRead.ToString());
        //            }
        //            else
        //            {
        //                break;
        //            }
        //            //if (BytesRead == 0)
        //            //{
        //            //    Client.Close();
        //            //    ConnectionStatus.Content = "Отключен";
        //            //    ConnectionStatus.Foreground = Brushes.Red;
        //            //    MessageBox.Show("Сервер закрыт!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        //            //    break;
        //            //}
        //            //string Data = Encoding.ASCII.GetString(Buffer, 0, BytesRead);
        //            //if (CurrentPos < 0)
        //            //{
        //            //    CurrentPos = double.Parse(Data);
        //            //}
        //            //ServerData.Text += "Сервер " + System.DateTime.Now.ToString() + ": " + Data + "\n";
        //            //ServerData.ScrollToEnd();
        //            return;
        //        }
        //        catch (IOException ex)
        //        {
        //            MessageBox.Show(ex.Message);
        //            break;
        //        }
        //    }
        //    Timeout.Stop();
        //    Timeout.Elapsed -= Timeout_Elapsed;
        //    ServerData.Text += "Истекло время ожидания сервера\n";
        //    return;
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
                double Step = double.Parse(MoveInput1.Text.Replace(".", ","));
                string StateData = "MOVEA1 " + Pos + " " + Step + " " + Speed;
                SendData(StateData);
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
                double Step = double.Parse(MoveInput2.Text.Replace(".", ","));
                string StateData = "MOVEA2 " + Pos + " " + Step + " " + Speed;
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

        private void LeftStep1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentPos1 - double.Parse(MoveInput1.Text.Replace(".", ",")) < 0.0)
                {
                    CurrentPos1 = CurrentPos1 + double.Parse(MoveInput1.Text.Replace(".", ","));
                    double Speed = double.Parse(SpeedInput1.Text.Replace(".", ","));
                    SendData("MOVEA1 " + CurrentPos1.ToString() + " " + Speed);
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
                if(CurrentPos1 + double.Parse(MoveInput1.Text.Replace(".", ",")) < 100.0) {
                    CurrentPos1 = CurrentPos1 + double.Parse(MoveInput1.Text.Replace(".", ","));
                    double Speed = double.Parse(SpeedInput1.Text.Replace(".", ","));
                    SendData("MOVEA1 " + CurrentPos1.ToString() + " " + Speed);
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
                if (CurrentPos2 - double.Parse(MoveInput2.Text.Replace(".", ",")) < 0.0)
                {
                    CurrentPos2 = CurrentPos2 + double.Parse(MoveInput2.Text.Replace(".", ","));
                    double Speed = double.Parse(SpeedInput2.Text.Replace(".", ","));
                    SendData("MOVEA2 " + CurrentPos2.ToString() + " " + Speed);
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
                if (CurrentPos2 + double.Parse(MoveInput2.Text.Replace(".", ",")) < 100.0)
                {
                    CurrentPos2 = CurrentPos2 + double.Parse(MoveInput2.Text.Replace(".", ","));
                    double Speed = double.Parse(SpeedInput2.Text.Replace(".", ","));
                    SendData("MOVEA2 " + CurrentPos2.ToString() + " " + Speed);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
