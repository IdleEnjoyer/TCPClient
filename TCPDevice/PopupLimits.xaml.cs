using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Логика взаимодействия для PopupLimits.xaml
    /// </summary>
    public partial class PopupLimits : Window
    {
        public PopupLimits()
        {
            InitializeComponent();
        }

		private void Confirm_Click(object sender, RoutedEventArgs e)
		{
			float Up = 0.0f;
			float Down = 0.0f;
			if (float.TryParse(UpperLim.Text, CultureInfo.InvariantCulture, out Up))
			{
				if (float.TryParse(LowerLim.Text, CultureInfo.InvariantCulture, out Down))
				{
					if (Down < Up)
					{
						MainWindow MW = this.Owner as MainWindow;
						string Com = "SETLIM " + MW.Axis + " " + Down + " " + Up;
						MW.SendCommand(Com);
						//MessageBox.Show(Com);
						this.Close();
					}
				}
			}
        }
    }
}
