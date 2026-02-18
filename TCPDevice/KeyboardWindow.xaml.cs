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
	/// Логика взаимодействия для KeyboardWindow.xaml
	/// </summary>
	public partial class KeyboardWindow : Window
	{
		public TextBox TB;
		bool Negative;
		public KeyboardWindow()
		{
			InitializeComponent();
		}

		private void Digit_Btn_Click(object sender, RoutedEventArgs e)
		{
			string? Digit = ((Button)sender).Content as string;
			TB.Text += Digit;
		}

		private void Digit_Del_Btn_Click(object sender, RoutedEventArgs e)
		{
			if (TB.Text.Length > 0)
			{
				TB.Text = TB.Text.Substring(0, TB.Text.Length - 1);
			}
		}

		private void Digit_Minus_Btn_Click(object sender, RoutedEventArgs e)
		{
			Negative = TB.Text.Contains('-');
			if (!Negative)
			{
				TB.Text = TB.Text.Insert(0, "-");
			}
			else
			{
				TB.Text = TB.Text.Replace("-",string.Empty);
			}
		}

		private void Return_Btn_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
