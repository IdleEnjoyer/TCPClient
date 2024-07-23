using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        int LastPropID = 1;
        int LastPropAmount = 1;
        List<int> Amounts = new List<int>();
        public AddProjectWindow()
        {
            InitializeComponent();

            Button BT = new Button();
            BT.Name = "Button";
            BT.Content = Prop.Text;
            BT.Margin = new Thickness(10, 10, 10, 10);
            Grid.SetRow(BT, 1);
            Grid.SetColumn(BT, 1);
            ProjectGrid.Children.Add(BT);
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
            LastPropID++;

            ProjectGrid.Height += 50;
            ProjectGrid.RowDefinitions.Add(new RowDefinition());
            //<TextBox x:Name="Prop" Text="СВОЙСТВО 1" HorizontalContentAlignment="Center" VerticalContentAlignment="Center" Grid.Column="0" Grid.Row="1" FontSize="14" HorizontalAlignment="Left" Margin="0,20,0,0" Width="95" VerticalAlignment="Bottom" Height="25"/>
            TextBox NewProp = new TextBox();
            NewProp.HorizontalAlignment = HorizontalAlignment.Left;
            NewProp.VerticalAlignment = VerticalAlignment.Bottom;
            NewProp.Width = 95;
            NewProp.Height = 25;
            NewProp.Text = $"СВОЙСТВО {LastPropID}";
            NewProp.FontSize = 14;
            NewProp.HorizontalContentAlignment = HorizontalAlignment.Center;
            NewProp.VerticalContentAlignment = VerticalAlignment.Center;
            NewProp.Name = "Prop";
            Grid.SetColumn(NewProp, 0);
            Grid.SetRow(NewProp, ProjectGrid.RowDefinitions.Count - 1);
            ProjectGrid.Children.Add(NewProp);


            //<TextBox x:Name="Amount" Text="1" Grid.Column="0" Grid.Row="1" FontSize="16" HorizontalAlignment="Right" VerticalContentAlignment="Center" Margin="0,20,0,0" Width="25" HorizontalContentAlignment="Center" PreviewTextInput="Amount_PreviewTextInput" TextChanged="Amount_TextChanged" LostFocus="Amount_LostFocus" VerticalAlignment="Bottom" Height="25" Initialized="Amount_Initialized"/>
            //TextBox NewNum = new TextBox();
            //NewNum.HorizontalAlignment = HorizontalAlignment.Right;
            //NewNum.VerticalAlignment = VerticalAlignment.Bottom;
            //NewNum.Width = 25;
            //NewNum.Height = 25;
            //NewNum.Text = "1";
            //NewNum.FontSize = 16;
            //NewNum.HorizontalContentAlignment = HorizontalAlignment.Center;
            //NewNum.VerticalContentAlignment = VerticalAlignment.Center;
            //NewNum.Name = "Amount";
            //NewNum.PreviewTextInput += Amount_PreviewTextInput;
            //Grid.SetColumn(NewNum, 0);
            //Grid.SetRow(NewNum, LastPropID);
            //ProjectGrid.Children.Add(NewNum);

            //<Button x:Name="RemoveProp" Content="X" Grid.Column="0" Grid.Row="1" VerticalAlignment="Top" HorizontalAlignment="Right" Height="25" Width="25" Click="RemoveProp_Click"/>
            Button Remove = new Button();
            Remove.Height = 25;
            Remove.Width = 25;
            Remove.Name = "RemoveProp";
            Remove.Content = "X";
            Remove.VerticalAlignment = VerticalAlignment.Top;
            Remove.HorizontalAlignment = HorizontalAlignment.Left;
            Remove.Click += RemoveProp_Click;
            Grid.SetColumn(Remove, 0);
            Grid.SetRow(Remove, ProjectGrid.RowDefinitions.Count - 1);
            ProjectGrid.Children.Add(Remove);

            //<Button x:Name="Incr" Content ="+" Grid.Column="0" Grid.Row="1" HorizontalAlignment="Right" VerticalAlignment="Bottom" Width="25" Height="25" Margin="0,0,0,25" FontSize="16" Click="Incr_Click"/>
            Button Increment = new Button();
            Increment.Height = 25;
            Increment.Width = 25;
            Increment.Name = "Incr";
            Increment.Content = "+";
            Increment.VerticalAlignment = VerticalAlignment.Top;
            Increment.HorizontalAlignment = HorizontalAlignment.Right;
            Increment.FontSize = 16;
            Increment.Click += Incr_Click;
            Grid.SetColumn(Increment, 0);
            Grid.SetRow(Increment, ProjectGrid.RowDefinitions.Count - 1);
            ProjectGrid.Children.Add(Increment);

            //<Button x:Name="Decr" Content ="-" Grid.Column="0" Grid.Row="1" HorizontalAlignment="Right" VerticalAlignment="Bottom"  Width="25" Height="25"  FontSize="16" Click="Decr_Click"/>
            Button Decrement = new Button();
            Decrement.Height = 25;
            Decrement.Width = 25;
            Decrement.Name = "Decr";
            Decrement.Content = "-";
            Decrement.VerticalAlignment = VerticalAlignment.Bottom;
            Decrement.HorizontalAlignment = HorizontalAlignment.Right;
            Decrement.FontSize = 16;
            Decrement.Click += Decr_Click;
            Grid.SetColumn(Decrement, 0);
            Grid.SetRow(Decrement, ProjectGrid.RowDefinitions.Count - 1);
            ProjectGrid.Children.Add(Decrement);


            Button BT = new Button();
            BT.Name = "Button";
            BT.Content = Prop.Text;
            BT.Margin = new Thickness(10, 10, 10, 10);
            Grid.SetRow(BT, ProjectGrid.RowDefinitions.Count - 1);
            Grid.SetColumn(BT, 1);
            ProjectGrid.Children.Add(BT);
        }

        //private void RemoveAxisBttn_Click(object sender, RoutedEventArgs e)
        //{
        //    for(int i = 0; i < ProjectGrid.Children.Count; i++)
        //    {
        //        UIElement Child = ProjectGrid.Children[i];
        //        if (Child.GetType() == typeof(TextBox))
        //        {
        //            TextBox AxisToRemove = (TextBox)Child;
        //            if (AxisToRemove.Name == $"AxisC{AxisAmount}")
        //            {
        //                AxisAmount--;
        //                ProjectGrid.Width -= 100;
        //                ProjectGrid.ColumnDefinitions.RemoveAt(ProjectGrid.ColumnDefinitions.Count - 1);
        //                ProjectGrid.Children.Remove(Child);
        //                return;
        //            }
        //        }
        //    }
        //}

        //private void RemovePropBttn_Click(object sender, RoutedEventArgs e)
        //{
        //    List<UIElement> EList = new List<UIElement>();
        //    for (int i = 0; i < ProjectGrid.Children.Count; i++)
        //    {
        //        UIElement Child = ProjectGrid.Children[i];
        //        if (Grid.GetRow(Child) >= LastPropID)
        //        {
        //            EList.Add(Child);
        //        }
        //    }
        //    foreach (UIElement Child in EList)
        //    {
        //        ProjectGrid.Children.Remove(Child);
        //    }
        //    ProjectGrid.RowDefinitions.RemoveRange(LastPropID, LastPropAmount);
        //    ProjectGrid.Height -= 50 * LastPropAmount;
        //    LastPropID = LastPropID - LastPropAmount;
        //}

        private void OnPress(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.OemPlus:
                    {
                        ProjectGrid.RowDefinitions.Add(new RowDefinition());
                        ProjectGrid.Height += 50;
                    }
                    break;
                case Key.OemMinus:
                    {
                        ProjectGrid.RowDefinitions.Remove(ProjectGrid.RowDefinitions.Last());
                        ProjectGrid.Height -= 50;
                    }
                    break;
            }
        }

        private void Amount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsNumber(e.Text))
            {
                e.Handled = true;
            }
        }

        private bool IsNumber(string text)
        {
            Regex NumRegex = new Regex("[^0-9]+");
            return !NumRegex.IsMatch(text);
        }

        private void RemoveProp_Click(object sender, RoutedEventArgs e)
        {
            Button? BT = sender as Button;
            int Index = Grid.GetRow(BT);
            int Amount = 1;
            foreach(UIElement Child in ProjectGrid.Children)
            {
                TextBox? TB = Child as TextBox;
                if (Child.GetType() == typeof(TextBox))
                {
                    if (Grid.GetRow(TB) > Index && TB.Name == "Prop")
                    {
                        Amount = Grid.GetRow(TB) - Index;
                        break;
                    }
                }
            }
            List<UIElement> Chlist = new List<UIElement>();
            for (int i = 0; i < ProjectGrid.Children.Count; i++)
            {
                if (Grid.GetRow(ProjectGrid.Children[i]) >= Index && Grid.GetRow(ProjectGrid.Children[i]) < Index + Amount - 1)
                {
                    Chlist.Add(ProjectGrid.Children[i]);
                }
            }
            foreach (UIElement Child in Chlist)
            {
                ProjectGrid.Children.Remove(Child);
            }
            foreach (UIElement Child in ProjectGrid.Children)
            {
                if (Grid.GetRow(Child) > Index)
                {
                    Grid.SetRow(Child, Grid.GetRow(Child) - Amount);
                }
            }
            ProjectGrid.RowDefinitions.RemoveRange(Index + 1, Amount);
            ProjectGrid.Height -= 50 * Amount;
        }

        private void Incr_Click(object sender, RoutedEventArgs e)
        {
            Button? BT = sender as Button;
            int Index = Grid.GetRow(BT);
            int NextIndex = Index + 1;
            foreach (UIElement Child in ProjectGrid.Children)
            {
                if (Child.GetType() == typeof(TextBox)) {
                    TextBox? TB = Child as TextBox;
                    if(Grid.GetRow(TB) > Index && TB.Name == "Prop")
                    {
                        NextIndex = Grid.GetRow(TB);
                        break;
                    }
                }
            }
            ProjectGrid.RowDefinitions.Add(new RowDefinition());
            ProjectGrid.Height += 50;
            foreach (UIElement Child in ProjectGrid.Children) { 
                if(Grid.GetRow(Child) >= NextIndex)
                {
                    Grid.SetRow(Child, Grid.GetRow(Child) + 1);
                }
            }
        }

        private void Decr_Click(object sender, RoutedEventArgs e)
        {
            Button? BT = sender as Button;
            int Index = Grid.GetRow(BT);
            int NextIndex = Index + 1;
            bool Last = true;
            foreach (UIElement Child in ProjectGrid.Children)
            {
                if (Child.GetType() == typeof(TextBox))
                {
                    TextBox? TB = Child as TextBox;
                    if (Grid.GetRow(TB) > Index && TB.Name == "Prop")
                    {
                        NextIndex = Grid.GetRow(TB);
                        Last = false;
                        break;
                    }
                }
            }
            if((NextIndex != Index + 1 || Last) && Index != ProjectGrid.RowDefinitions.Count - 1)
            {
                ProjectGrid.RowDefinitions.Remove(ProjectGrid.RowDefinitions.Last());
                ProjectGrid.Height -= 50;
                foreach (UIElement Child in ProjectGrid.Children)
                {
                    if (Grid.GetRow(Child) >= NextIndex)
                    {
                        Grid.SetRow(Child, Grid.GetRow(Child) - 1);
                    }
                }
            }
        }
    }
}
