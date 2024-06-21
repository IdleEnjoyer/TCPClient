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
    /// Логика взаимодействия для AddProjectWindow.xaml
    /// </summary>
    public partial class AddProjectWindow : Window
    {
        int AxisAmount = 1;
        int PropAmount = 1;
        public AddProjectWindow()
        {
            InitializeComponent();
        }

        private void AddAxisBttn_Click(object sender, RoutedEventArgs e)
        {
            AxisAmount++;
            TextBox NewAxis = new TextBox();
            NewAxis.Text = $"ОСЬ {AxisAmount}";
            NewAxis.FontSize = 20;
            NewAxis.HorizontalContentAlignment = HorizontalAlignment.Center;
            NewAxis.VerticalContentAlignment = VerticalAlignment.Center;
            NewAxis.Name = $"AxisC{AxisAmount}";
            ProjectGrid.Width += 100;
            Grid.SetColumn(NewAxis, AxisAmount);
            Grid.SetRow(NewAxis, 0);
            ProjectGrid.ColumnDefinitions.Add(new ColumnDefinition());
            ProjectGrid.Children.Add(NewAxis);
        }

        private void AddPropBttn_Click(object sender, RoutedEventArgs e)
        {
            PropAmount++;
            TextBox NewProp = new TextBox();
            NewProp.Text = $"СВОЙСТВО {PropAmount}";
            NewProp.FontSize = 14;
            NewProp.HorizontalContentAlignment = HorizontalAlignment.Center;
            NewProp.VerticalContentAlignment = VerticalAlignment.Center;
            NewProp.Name = $"PropR{PropAmount}";
            ProjectGrid.Height += 50;
            Grid.SetColumn(NewProp, 0);
            Grid.SetRow(NewProp, PropAmount);
            ProjectGrid.RowDefinitions.Add(new RowDefinition());
            ProjectGrid.Children.Add(NewProp);
        }

        private void RemoveAxisBttn_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < ProjectGrid.Children.Count; i++)
            {
                UIElement Child = ProjectGrid.Children[i];
                if (Child.GetType() == typeof(TextBox))
                {
                    TextBox AxisToRemove = (TextBox)Child;
                    if (AxisToRemove.Name == $"AxisC{AxisAmount}")
                    {
                        AxisAmount--;
                        ProjectGrid.Width -= 100;
                        ProjectGrid.ColumnDefinitions.RemoveAt(ProjectGrid.ColumnDefinitions.Count - 1);
                        ProjectGrid.Children.Remove(Child);
                        return;
                    }
                }
            }
        }

        private void RemovePropBttn_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < ProjectGrid.Children.Count; i++)
            {
                UIElement Child = ProjectGrid.Children[i];
                if (Child.GetType() == typeof(TextBox))
                {
                    TextBox PropToRemove = (TextBox)Child;
                    if (PropToRemove.Name == $"PropR{PropAmount}")
                    {
                        PropAmount--;
                        ProjectGrid.Height -= 50;
                        ProjectGrid.RowDefinitions.RemoveAt(ProjectGrid.RowDefinitions.Count - 1);
                        ProjectGrid.Children.Remove(Child);
                        return;
                    }
                }
            }
        }

    }
}
