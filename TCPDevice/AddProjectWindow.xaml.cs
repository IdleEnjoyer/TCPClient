using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
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

            AddChoice(1, ProjectGrid.RowDefinitions.Count - 1);
        }

        private void AddChoice(int Column, int Row)
        {
            ComboBox Choice = new ComboBox();
            Choice.Height = 25;
            Choice.Width = 100;
            Choice.Name = "Choice";
            Choice.VerticalAlignment = VerticalAlignment.Top;
            Choice.SelectionChanged += Choice_SelectionChanged;
            Grid.SetColumn(Choice, Column);
            Grid.SetRow(Choice, Row);
            ProjectGrid.Children.Add(Choice);

            ComboBoxItem None = new ComboBoxItem();
            None.Content = "Ничего";
            None.Name = "None";
            Choice.Items.Add(None);

            ComboBoxItem LabelItem = new ComboBoxItem();
            LabelItem.Content = "Надпись";
            LabelItem.Name = "Label";
            Choice.Items.Add(LabelItem);

            ComboBoxItem Button = new ComboBoxItem();
            Button.Content = "Кнопка";
            Button.Name = "Button";
            Choice.Items.Add(Button);

            ComboBoxItem Input = new ComboBoxItem();
            Input.Content = "Поле ввода";
            Input.Name = "Input";
            Choice.Items.Add(Input);
        }

        private void AddAxisBttn_Click(object sender, RoutedEventArgs e)
        {
            AxisAmount++;
            ProjectGrid.Width += 120;
            ProjectGrid.ColumnDefinitions.Add(new ColumnDefinition());

            //<TextBox x:Name="Axis" Text="ОСЬ 1" HorizontalContentAlignment="Center" VerticalContentAlignment="Center" Grid.Column="1" Grid.Row="0" FontSize="16" Margin="0,20,0,0" VerticalAlignment="Bottom" Height="25"/>
            TextBox NewAxis = new TextBox();
            NewAxis.Text = $"ОСЬ {AxisAmount}";
            NewAxis.Height = 25;
            NewAxis.VerticalAlignment = VerticalAlignment.Bottom;
            NewAxis.FontSize = 16;
            NewAxis.HorizontalContentAlignment = HorizontalAlignment.Center;
            NewAxis.VerticalContentAlignment = VerticalAlignment.Center;
            NewAxis.Name = $"AxisC{AxisAmount}";
            Grid.SetColumn(NewAxis, AxisAmount);
            Grid.SetRow(NewAxis, 0);
            ProjectGrid.Children.Add(NewAxis);

            //<Button x:Name="RemoveAxis" Content="X" Grid.Column="1" Grid.Row="0" VerticalAlignment="Top" HorizontalAlignment="Right" Height="25" Width="25" Click="RemoveAxis_Click"/>
            Button Remove = new Button();
            Remove.Name = "RemoveAxis";
            Remove.Content = "X";
            Remove.VerticalAlignment = VerticalAlignment.Top;
            Remove.HorizontalAlignment = HorizontalAlignment.Right;
            Remove.Height = 25;
            Remove.Width = 25;
            Remove.Click += RemoveAxis_Click;
            Grid.SetColumn(Remove, AxisAmount);
            Grid.SetRow(Remove, 0);
            ProjectGrid.Children.Add(Remove);

            for (int i = 1; i < ProjectGrid.RowDefinitions.Count; i++)
            {
                AddChoice(AxisAmount, i);
            }
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

            for (int i = 1; i < ProjectGrid.ColumnDefinitions.Count; i++)
            {
                AddChoice(i, ProjectGrid.RowDefinitions.Count - 1);
            }
        }

        private void Choice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox? Choice = sender as ComboBox;
            ComboBoxItem? Item = Choice.SelectedItem as ComboBoxItem;
            if (Item.Name == "Button" || Item.Name == "Label")
            {
                if(e.RemovedItems.Count != 0)
                {
                    ComboBoxItem Prev = e.RemovedItems[0] as ComboBoxItem;
                    if (Prev.Name != "Button" && Prev.Name != "Label")
                    {
                        TextBox TB = new TextBox();
                        TB.Name = "Command";
                        TB.Width = 100;
                        TB.Height = 25;
                        TB.VerticalAlignment = VerticalAlignment.Bottom;
                        Grid.SetColumn(TB, Grid.GetColumn(Choice));
                        Grid.SetRow(TB, Grid.GetRow(Choice));
                        ProjectGrid.Children.Add(TB);
                    }
                }
                else
                {
                    TextBox TB = new TextBox();
                    TB.Name = "Command";
                    TB.Width = 100;
                    TB.Height = 25;
                    TB.VerticalAlignment = VerticalAlignment.Bottom;
                    Grid.SetColumn(TB, Grid.GetColumn(Choice));
                    Grid.SetRow(TB, Grid.GetRow(Choice));
                    ProjectGrid.Children.Add(TB);
                }
                
            }
            else
            {
                UIElement Delete = null;
                foreach (UIElement Child in ProjectGrid.Children)
                {
                    if (Child.GetType() == typeof(TextBox))
                    {
                        TextBox TB = (TextBox)Child;
                        if (TB.Name == "Command" && Grid.GetRow(TB) == Grid.GetRow(Choice) && Grid.GetColumn(TB) == Grid.GetColumn(Choice))
                        {
                            Delete = TB;
                            break;
                        }
                    }
                }
                if (Delete != null)
                {
                    ProjectGrid.Children.Remove(Delete);
                }
            }
        }

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
                if (Child.GetType() == typeof(TextBox)) 
                {
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
            foreach (UIElement Child in ProjectGrid.Children)
            {
                if (Grid.GetRow(Child) >= NextIndex)
                {
                    Grid.SetRow(Child, Grid.GetRow(Child) + 1);
                }
            }
            for(int i = 1; i < ProjectGrid.ColumnDefinitions.Count; i++)
            {
                AddChoice(i, NextIndex);
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
                List<UIElement> L = new List<UIElement>();
                foreach (UIElement Child in ProjectGrid.Children)
                {
                    if (Grid.GetRow(Child) == NextIndex - 1 && NextIndex - 1 != Index)
                    {
                        L.Add(Child);
                    }
                    if (Grid.GetRow(Child) >= NextIndex)
                    {
                        Grid.SetRow(Child, Grid.GetRow(Child) - 1);
                    }

                }
                ProjectGrid.RowDefinitions.Remove(ProjectGrid.RowDefinitions.Last());
                ProjectGrid.Height -= 50;
                for (int i = 0; i < L.Count; i++)
                {
                    ProjectGrid.Children.Remove(L[i]);
                }
                
            }
        }

        private void RemoveAxis_Click(object sender, RoutedEventArgs e)
        {
            List<UIElement> Elems = new List<UIElement>();
            Button? BT = sender as Button;
            int Index = Grid.GetColumn(BT);
            foreach(UIElement Child in ProjectGrid.Children)
            {
                if(Index == Grid.GetColumn(Child))
                {
                    Elems.Add(Child);
                }
            }
            foreach (UIElement Child in Elems)
            {
                ProjectGrid.Children.Remove(Child);
            }
            foreach(UIElement Child in ProjectGrid.Children)
            {
                if(Grid.GetColumn(Child) > Index)
                {
                    Grid.SetColumn(Child, Grid.GetColumn(Child) - 1);
                }
            }
            AxisAmount--;
            ProjectGrid.Width -= 120;
            ProjectGrid.ColumnDefinitions.Remove(ProjectGrid.ColumnDefinitions.Last());
        }

        bool FillCheck()
        {
            bool FilledOut = true;
            List<UIElement> Check = new List<UIElement>();
            List<UIElement> Delete = new List<UIElement>();
            foreach (UIElement Child in ProjectGrid.Children)
            {
                if (Child.GetType() == typeof(TextBox))
                {
                    TextBox? TB = Child as TextBox;
                    if (TB.Text == string.Empty)
                    {
                        TB.BorderBrush = Brushes.Red;
                        FilledOut = false;
                    }
                    else
                    {
                        TB.BorderBrush = Brushes.Green;
                    }
                }
                if (Child.GetType() == typeof(ComboBox))
                {
                    ComboBox? CB = Child as ComboBox;
                    CB.OverridesDefaultStyle = true;

                    if (CB.SelectedIndex == -1)
                    {
                        Border BD = new Border();
                        BD.BorderBrush = Brushes.Red;
                        BD.BorderThickness = new Thickness(2);
                        Grid.SetColumn(BD, Grid.GetColumn(CB));
                        Grid.SetRow(BD, Grid.GetRow(CB));
                        Check.Add(BD);
                        FilledOut = false;
                    }
                }
                if (Child.GetType() == typeof(Border))
                {
                    Border? BD = Child as Border;
                    Delete.Add(BD);
                }
            }
            foreach (UIElement Elem in Check)
            {
                ProjectGrid.Children.Add(Elem);
            }
            foreach (UIElement Elem in Delete)
            {
                ProjectGrid.Children.Remove(Elem);
            }
            return FilledOut;
        }

        private void Completion_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(FillCheck().ToString());
            if (!FillCheck())
            {
                MessageBox.Show("Не все поля заполнены!");
            }
            else
            {

            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            if(FillCheck())
            {
                string XAMLString = XamlWriter.Save(ProjectGrid);
                SaveFileDialog SFD = new SaveFileDialog();
                SFD.ShowDialog(this);
                if (SFD.FileName != null)
                {
                    FileStream FS = File.Create(SFD.FileName);
                    StreamWriter SW = new StreamWriter(FS);
                    SW.Write(XAMLString, 0, XAMLString.Length);
                    SW.Close();
                    FS.Close();
                }
                SFD.Reset();
            }
            else
            {
                MessageBox.Show("Не все поля заполнены!");
            }
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.ShowDialog();
            if(OFD.FileName != null)
            {
                string XAMLImport;
                FileStream FS = File.OpenRead(OFD.FileName);
                StreamReader SR = new StreamReader(FS);
                XAMLImport = SR.ReadToEnd();
                Grid? Import = XamlReader.Parse(XAMLImport) as Grid;
                ProjectGrid = Import;
            }
        }
    }
}
