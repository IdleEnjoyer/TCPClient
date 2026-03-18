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
						string Com = "SETLIM " + MW.Axis_OPU1 + MW.Axis_OPU2 + " " + Down + " " + Up;
						if (MW.Axis_OPU2 == String.Empty)
						{
							MW.SendCommand_OPU1(Com);
						}
						if (MW.Axis_OPU1 == String.Empty)
						{
							MW.SendCommand_OPU2(Com);
						}
						this.Close();
					}
				}
			}
        }

		private void OPU1_TextBox_TouchDown(object sender, TouchEventArgs e)
		{
			KeyboardWindow KW = new KeyboardWindow();
			KW.TB = (TextBox)sender;
			KW.Owner = this;
			KW.ShowDialog();
		}
	}
}
