using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace TCPDevice
{
    internal class SerialPortTracker
    {
        private ObservableCollection<string> ComPortsNames;
        private List<SerialPort> SerialPorts;
        private string Data;
        private MainWindow owner;
        public SerialPortTracker(MainWindow MW)
        {
            ComPortsNames = new ObservableCollection<string>();
            SerialPorts = new List<SerialPort>();
            owner = MW;
        }

        public SerialPortTracker()
        {
            ComPortsNames = new ObservableCollection<string>();
            SerialPorts = new List<SerialPort>();
        }

        public void CheckSerialPorts()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string[] portNames = SerialPort.GetPortNames();
                if (!ComPortsNames.ToArray().SequenceEqual(portNames))
                {
                    ComPortsNames.Clear();
                    foreach (string portName in portNames)
                    {
                        ComPortsNames.Add(portName);
                    }
                }
            });
            List<SerialPort> Deletion = new List<SerialPort>();
            foreach(SerialPort Port in SerialPorts)
            {
                if (!ComPortsNames.Contains(Port.PortName))
                {
                    Port.Close();
                    Deletion.Add(Port);
                }
            }
            List<SerialPort> Addition = new List<SerialPort>();
            
            foreach (string Name in ComPortsNames)
            {
                
                if (!SerialPorts.Exists(x => x.PortName == Name))
                {
                    
                    SerialPort Port = new SerialPort(Name, 115200);
                    Port.DataReceived += Port_DataReceived;
                    Addition.Add(Port);
                }
            }
            foreach (SerialPort Port in Deletion)
            {
                SerialPorts.Remove(Port);
            }
            foreach(SerialPort Port in Addition)
            {
                SerialPorts.Add(Port);
            }
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort? SP = sender as SerialPort;
            owner.ComDataRecieve(SP.ReadExisting());
        }

        public ObservableCollection<string> AvailablePorts
        {
            get { return ComPortsNames; }
        }

        public SerialPort GetPort(string name)
        {
            if(SerialPorts.Exists(x => x.PortName == name))
            {
                return SerialPorts.Find(x => x.PortName == name);
            }
            else
            {
                return null;
            }
        }
    }
}
