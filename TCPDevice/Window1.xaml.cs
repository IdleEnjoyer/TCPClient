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
using System.Windows.Shapes;

namespace TCPDevice
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void StartConnection_Click(object sender, RoutedEventArgs e)
        {
            MainWindow Wind = new MainWindow();
            Wind.Owner = this;
            this.Hide();
            Wind.Show();
        }

        private void StopConnection_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
