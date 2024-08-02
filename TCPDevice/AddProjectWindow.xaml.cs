using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

#pragma warning disable CS8602

namespace TCPDevice
{
    /// <summary>
    /// Логика взаимодействия для AddProjectWindow.xaml
    /// </summary>
    public partial class AddProjectWindow : Window
    {
        int AxisAmount = 1;
        int PropAmount = 1;
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
            Choice.Name = $"Choice{Column}{Row}";
            Choice.VerticalAlignment = VerticalAlignment.Top;
            Choice.SelectionChanged += Choice_SelectionChanged;
            Grid.SetColumn(Choice, Column);
            Grid.SetRow(Choice, Row);
            ProjectGrid.Children.Add(Choice);

            ComboBoxItem None = new ComboBoxItem();
            None.Content = "Ничего";
            None.Name = $"None{Column}{Row}";
            Choice.Items.Add(None);

            ComboBoxItem LabelItem = new ComboBoxItem();
            LabelItem.Content = "Надпись";
            LabelItem.Name = $"Label{Column}{Row}";
            Choice.Items.Add(LabelItem);

            ComboBoxItem Button = new ComboBoxItem();
            Button.Content = "Кнопка";
            Button.Name = $"Button{Column}{Row}";
            Choice.Items.Add(Button);

            ComboBoxItem Input = new ComboBoxItem();
            Input.Content = "Поле ввода";
            Input.Name = $"Input{Column}{Row}";
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
            Remove.Name = $"RemoveAxis{AxisAmount}";
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
            PropAmount++;

            ProjectGrid.Height += 50;
            ProjectGrid.RowDefinitions.Add(new RowDefinition());

            //<TextBox x:Name="Prop" Text="СВОЙСТВО 1" HorizontalContentAlignment="Center" VerticalContentAlignment="Center" Grid.Column="0" Grid.Row="1" FontSize="14" HorizontalAlignment="Left" Margin="0,20,0,0" Width="95" VerticalAlignment="Bottom" Height="25"/>
            TextBox NewProp = new TextBox();
            NewProp.HorizontalAlignment = HorizontalAlignment.Left;
            NewProp.VerticalAlignment = VerticalAlignment.Bottom;
            NewProp.Width = 95;
            NewProp.Height = 25;
            NewProp.Text = $"СВОЙСТВО {PropAmount}";
            NewProp.FontSize = 14;
            NewProp.HorizontalContentAlignment = HorizontalAlignment.Center;
            NewProp.VerticalContentAlignment = VerticalAlignment.Center;
            NewProp.Name = $"Prop{PropAmount}";
            Grid.SetColumn(NewProp, 0);
            Grid.SetRow(NewProp, ProjectGrid.RowDefinitions.Count - 1);
            ProjectGrid.Children.Add(NewProp);

            //<Button x:Name="RemoveProp" Content="X" Grid.Column="0" Grid.Row="1" VerticalAlignment="Top" HorizontalAlignment="Right" Height="25" Width="25" Click="RemoveProp_Click"/>
            Button Remove = new Button();
            Remove.Height = 25;
            Remove.Width = 25;
            Remove.Name = $"RemoveProp{PropAmount}";
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
            Increment.Name = $"Incr{PropAmount}";
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
            Decrement.Name = $"Decr{PropAmount}";
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
            if (Item.Name.Contains("Button") || Item.Name.Contains("Label"))
            {
                if (e.RemovedItems.Count != 0)
                {
                    ComboBoxItem? Prev = e.RemovedItems[0] as ComboBoxItem;
                    if (!Prev.Name.Contains("Button") && !Prev.Name.Contains("Label"))
                    {
                        TextBox TB = new TextBox();
                        TB.Name = $"Command{AxisAmount}{PropAmount}";
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
                    TB.Name = $"Command{AxisAmount}{PropAmount}";
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
                UIElement? Delete = null;
                foreach (UIElement Child in ProjectGrid.Children)
                {
                    if (Child.GetType() == typeof(TextBox))
                    {
                        TextBox TB = (TextBox)Child;
                        if (TB.Name.Contains("Command") && Grid.GetRow(TB) == Grid.GetRow(Choice) && Grid.GetColumn(TB) == Grid.GetColumn(Choice))
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

        private void RemoveProp_Click(object sender, RoutedEventArgs e)
        {
            Button? BT = sender as Button;
            int Index = Grid.GetRow(BT);
            int Amount = 1;
            bool Last = true;
            foreach (UIElement Child in ProjectGrid.Children)
            {
                TextBox? TB = Child as TextBox;
                if (Child.GetType() == typeof(TextBox))
                {
                    if (Grid.GetRow(TB) > Index && TB.Name.Contains("Prop"))
                    {
                        Amount = Grid.GetRow(TB) - Index;
                        Last = false;
                        break;
                    }
                }
            }
            if (Last)
            {
                Amount = ProjectGrid.RowDefinitions.Count - Index;
            }
            List<UIElement> Chlist = new List<UIElement>();
            for (int i = 0; i < ProjectGrid.Children.Count; i++)
            {
                if (Grid.GetRow(ProjectGrid.Children[i]) >= Index && Grid.GetRow(ProjectGrid.Children[i]) < Index + Amount)
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

            ProjectGrid.RowDefinitions.RemoveRange(Index, Amount);
            ProjectGrid.Height -= 50 * Amount;
            PropAmount--;
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
                    if (Grid.GetRow(TB) > Index && TB.Name.Contains("Prop"))
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
            for (int i = 1; i < ProjectGrid.ColumnDefinitions.Count; i++)
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
                    if (Grid.GetRow(TB) > Index && TB.Name.Contains("Prop"))
                    {
                        NextIndex = Grid.GetRow(TB);
                        Last = false;
                        break;
                    }
                }
            }
            if ((NextIndex != Index + 1 || Last) && Index != ProjectGrid.RowDefinitions.Count - 1)
            {
                List<UIElement> L = new List<UIElement>();
                foreach (UIElement Child in ProjectGrid.Children)
                {
                    if (Grid.GetRow(Child) == NextIndex && NextIndex != Index)
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
            foreach (UIElement Child in ProjectGrid.Children)
            {
                if (Index == Grid.GetColumn(Child))
                {
                    Elems.Add(Child);
                }
            }
            foreach (UIElement Child in Elems)
            {
                ProjectGrid.Children.Remove(Child);
            }
            foreach (UIElement Child in ProjectGrid.Children)
            {
                if (Grid.GetColumn(Child) > Index)
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
                    //else
                    //{
                    //    int Index = Grid.GetRow(CB);
                    //    int LastTBIndex = 1;
                    //    int NextTBIndex = 1;
                    //    foreach(UIElement TEMP in ProjectGrid.Children)
                    //    {
                    //        if(TEMP.GetType() == typeof(TextBox) && Grid.GetColumn(TEMP) == 0)
                    //        {
                    //            LastTBIndex = Grid.GetRow(TEMP);
                    //        }
                    //        if(TEMP.GetType() == typeof(TextBox) && Grid.GetRow(TEMP) > Index)
                    //        {
                    //            NextTBIndex = Grid.GetRow(TEMP);
                    //            break;
                    //        }
                    //    }
                    //    List<UIElement> ComboBoxes = new List<UIElement>();
                    //    foreach(UIElement TEMP in ProjectGrid.Children)
                    //    {
                    //        if(Grid.GetRow(TEMP) >= LastTBIndex && Grid.GetRow(TEMP) < NextTBIndex && TEMP.GetType() == typeof(ComboBox) && (ComboBox)TEMP != CB && Grid.GetColumn(TEMP) == Grid.GetColumn(CB))
                    //        {
                    //            ComboBox? aCB = TEMP as ComboBox;
                    //            ComboBoxes.Add(aCB);
                    //        }
                    //    }
                    //    int buttonCount = 0;
                    //    int inputCount = 0;
                    //    foreach(UIElement Elem in ComboBoxes)
                    //    {
                    //        ComboBox? comboBox = Elem as ComboBox;
                    //        ComboBoxItem? CBI = comboBox.SelectedItem as ComboBoxItem;
                    //        if (CBI.Name.Contains("Button"))
                    //        {
                    //            buttonCount++;
                    //        }
                    //        if (CBI.Name.Contains("Input"))
                    //        {
                    //            inputCount++;
                    //        }
                    //    }
                    //    if (buttonCount > 2)
                    //    {
                    //        MessageBox.Show("Нельзя создать больше 2х кнопок в свойсте!");
                    //        FilledOut = false;
                    //    }
                    //    if (inputCount > 1 && buttonCount == 2)
                    //    {
                    //        MessageBox.Show("Нельзя создать больше 1го поля ввода, когда есть 2 кнопки!");
                    //        FilledOut = false;
                    //    }
                    //}
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
            if (!FillCheck())
            {
                MessageBox.Show("Не все поля заполнены!");
            }
            else
            {
                List<UIElement> Deletion = new List<UIElement>();
                List<UIElement> Addition = new List<UIElement>();
                for (int i = 0; i < ProjectGrid.Children.Count; i++)
                {
                    UIElement Child = ProjectGrid.Children[i];
                    if (Child.GetType() == typeof(Button))
                    {
                        Deletion.Add(Child);
                    }
                    if (Child.GetType() == typeof(TextBox) && (Grid.GetRow(Child) == 0 || Grid.GetColumn(Child) == 0))
                    {
                        Deletion.Add(Child);
                        TextBox? TB = Child as TextBox;
                        Label Replace = new Label();
                        Replace.Name = TB.Name;
                        Replace.Content = TB.Text;
                        Replace.FontSize = 16;
                        Replace.HorizontalAlignment = HorizontalAlignment.Center;
                        Replace.VerticalAlignment = VerticalAlignment.Center;
                        Grid.SetColumn(Replace, Grid.GetColumn(TB));
                        Grid.SetRow(Replace, Grid.GetRow(TB));
                        Addition.Add(Replace);
                    }
                    if (Child.GetType() == typeof(ComboBox))
                    {
                        ComboBox? CB = Child as ComboBox;
                        ComboBoxItem? CBI = CB.SelectedItem as ComboBoxItem;
                        if (CBI.Name.Contains("None"))
                        {
                            Deletion.Add(Child);
                        }
                        if (CBI.Name.Contains("Input"))
                        {
                            Deletion.Add(Child);
                            TextBox Input = new TextBox();
                            Input.Name = CBI.Name;
                            Input.VerticalAlignment = VerticalAlignment.Center;
                            Input.FontSize = 16;
                            Grid.SetColumn(Input, Grid.GetColumn(CB));
                            Grid.SetRow(Input, Grid.GetRow(CB));
                            Addition.Add(Input);
                        }
                        if (CBI.Name.Contains("Button"))
                        {
                            Deletion.Add(CB);
                            Button Command = new Button();
                            Command.Name = CBI.Name;
                            Command.Content = "Отправить";
                            Command.VerticalAlignment = VerticalAlignment.Center;
                            Command.HorizontalAlignment = HorizontalAlignment.Center;
                            Command.FontSize = 16;
                            foreach (UIElement TEMP in ProjectGrid.Children)
                            {
                                if (TEMP.GetType() == typeof(TextBox) && Grid.GetRow(TEMP) == Grid.GetRow(CB) && Grid.GetColumn(TEMP) == Grid.GetColumn(CB))
                                {
                                    TextBox? TB = TEMP as TextBox;
                                    Command.Resources.Add("Command", TB.Text);
                                    Deletion.Add(TEMP);
                                    break;
                                }
                            }
                            Grid.SetColumn(Command, Grid.GetColumn(CB));
                            Grid.SetRow(Command, Grid.GetRow(CB));
                            Addition.Add(Command);
                        }
                        if (CBI.Name.Contains("Label"))
                        {

                            Deletion.Add(CB);
                            Label L = new Label();
                            L.Name = CBI.Name;
                            foreach (UIElement TEMP in ProjectGrid.Children)
                            {
                                if (TEMP.GetType() == typeof(TextBox) && Grid.GetRow(TEMP) == Grid.GetRow(CB) && Grid.GetColumn(TEMP) == Grid.GetColumn(CB))
                                {
                                    TextBox? TB = TEMP as TextBox;
                                    L.Content = TB.Text;
                                    Deletion.Add(TEMP);
                                    break;
                                }
                            }
                            L.HorizontalAlignment = HorizontalAlignment.Center;
                            L.VerticalAlignment = VerticalAlignment.Center;
                            L.FontSize = 16;
                            Grid.SetColumn(L, Grid.GetColumn(CB));
                            Grid.SetRow(L, Grid.GetRow(CB));
                            Addition.Add(L);
                        }
                    }
                }
                foreach (UIElement Elem in Deletion)
                {
                    ProjectGrid.Children.Remove(Elem);
                }
                foreach (UIElement Elem in Addition)
                {
                    ProjectGrid.Children.Add(Elem);
                }
                ((MainWindow)Owner).CreateDevice(XamlWriter.Save(ProjectGrid));
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            if (FillCheck())
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
            if (OFD.FileName != null)
            {
                string XAMLImport;
                FileStream FS = File.OpenRead(OFD.FileName);
                StreamReader SR = new StreamReader(FS);
                XAMLImport = SR.ReadToEnd();
                Grid? Import = XamlReader.Parse(XAMLImport) as Grid;

                ProjectGrid.Children.RemoveRange(0, ProjectGrid.Children.Count);

                ProjectGrid.Height = Import.Height;
                ProjectGrid.Width = Import.Width;

                ProjectGrid.ColumnDefinitions.RemoveRange(0, ProjectGrid.ColumnDefinitions.Count);
                foreach (ColumnDefinition CD in Import.ColumnDefinitions)
                {
                    ProjectGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }
                ProjectGrid.RowDefinitions.RemoveRange(0, ProjectGrid.RowDefinitions.Count);
                foreach (RowDefinition RD in Import.RowDefinitions)
                {
                    ProjectGrid.RowDefinitions.Add(new RowDefinition());
                }

                PropAmount = 0;
                for (int i = Import.Children.Count - 1; i >= 0; i--)
                {
                    UIElement Child = Import.Children[i];
                    Import.Children.Remove(Child);
                    ProjectGrid.Children.Add(Child);
                    if (Child.GetType() == typeof(TextBox))
                    {
                        TextBox? TB = Child as TextBox;
                        if (TB.Name.Contains("Prop"))
                        {
                            PropAmount++;
                        }
                    }
                    if (Child.GetType() == typeof(Button))
                    {
                        Button? BT = Child as Button;
                        if (BT.Name.Contains("Incr"))
                        {
                            BT.Click += Incr_Click;
                        }
                        if (BT.Name.Contains("Decr"))
                        {
                            BT.Click += Decr_Click;
                        }
                        if (BT.Name.Contains("RemoveProp"))
                        {
                            BT.Click += RemoveProp_Click;
                        }
                        if (BT.Name.Contains("RemoveAxis"))
                        {
                            BT.Click += RemoveAxis_Click;
                        }
                    }
                    if (Child.GetType() == typeof(ComboBox))
                    {
                        ComboBox? CB = Child as ComboBox;
                        CB.SelectionChanged += Choice_SelectionChanged;
                    }
                }

                AxisAmount = Import.ColumnDefinitions.Count - 1;
            }
        }
    }
}
