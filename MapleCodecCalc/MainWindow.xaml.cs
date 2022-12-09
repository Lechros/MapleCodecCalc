using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MapleCodecCalc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Length = 0;
            result = new char[10, 10]; // [i, digit]
            entries = new ObservableCollection<Entry>();
            Data.ItemsSource = entries;
            SetResults();
        }

        public int Length;
        private char[,] result;
        private ObservableCollection<Entry> entries { get; set; }

        public void SaveData()
        {
            string data = string.Empty;
            foreach(var entry in entries)
            {
                data += $"{entry.Number} {entry.String} {entry.Remarks}\n";
            }
            File.WriteAllText("data.tmp", data);
        }

        public void LoadData()
        {
            string data;
            try
            {
                data = File.ReadAllText("data.tmp");
            }
            catch
            {
                MessageBox.Show("No data.");
                return;
            }
            foreach(var line in data.Split("\n"))
            {
                string[] parts = line.Split(' ', 3);
                Entry entry = new Entry();
                try
                {
                    entry.Number = Convert.ToInt32(parts[0]);
                    entry.String = parts[1];
                    if(parts[2] != null)
                        entry.Remarks = parts[2];
                    if(!AddEntry(entry))
                    {
                        MessageBox.Show("Failed loading data!");
                    }
                }
                catch
                {
                    MessageBox.Show("Failed loading data!");
                }
            }
        }

        public void SetResults()
        {
            resultLabel.Text = GetResultText(-1);
            result0.Text = GetResultText(0);
            result1.Text = GetResultText(1);
            result2.Text = GetResultText(2);
            result3.Text = GetResultText(3);
            result4.Text = GetResultText(4);
            result5.Text = GetResultText(5);
            result6.Text = GetResultText(6);
            result7.Text = GetResultText(7);
            result8.Text = GetResultText(8);
            result9.Text = GetResultText(9);
        }

        private void SetLength(int length)
        {
            if(length > 10)
            {
                MessageBox.Show("Length must be <= 10");
            }
            else if(length > 0)
            {
                Length = length;
                LengthText.IsEnabled = false;
                SetLengthButton.IsEnabled = false;
                SetResults();
            }
        }

        private string GetResultText(int digit)
        {
            string text = String.Empty;
            if(digit < 0)
            {
                text += "* ";
                for(int i = 0; i < Length; i++)
                {
                    text += (Length - i - 1) + " ";
                }
            }
            else
            {
                text += digit + " ";
                for(int i = 0; i < Length; i++)
                {
                    char letter = result[i, digit];
                    if('A' <= letter && letter <= 'Z' || 'a' <= letter && letter <= 'z') { }
                    else
                    {
                        letter = '_';
                    }
                    text += letter + " ";
                }
            }
            return text;
        }

        private bool AddEntry(Entry entry)
        {
            if(Length == 0)
            {
                SetLength(entry.String.Length);
            }
            if(!entry.IsValid(Length)) return false;
            entries.Add(entry);
            Evaluate();
            SetResults();
            SaveData();
            return true;
        }

        public bool Evaluate()
        {
            for(int i = 0; i < Length; i++)
            {
                for(int digit = 0; digit < 10; digit++)
                {
                    result[i, digit] = '\0';
                }
            }
            foreach(var entry in entries)
            {
                string d = entry.Number.ToString($"D{Length}");
                string e = entry.String;
                for(int i = 0; i < Length; i++)
                {
                    int digit = d[i] - '0';
                    if(result[i, digit] == '\0')
                    {
                        result[i, digit] = e[i];
                    }
                    else if(result[i, digit] != e[i])
                    {
                        MessageBox.Show("Some data doesn't match.");
                        return false;
                    }
                }
            }
            return true;
        }

        private void SetLengthButton_Click(object sender, RoutedEventArgs e)
        {
            SetLength(Convert.ToInt32(LengthText.Text));
        }

        private void AddDataButton_Click(object sender, RoutedEventArgs e)
        {
            var entry = new Entry();
            try
            {
                entry.Number = Convert.ToInt32(NumberText.Text);
                entry.String = StringText.Text.Trim();
                entry.Remarks = RemarkText.Text.Trim();
                if(!AddEntry(entry))
                {
                    MessageBox.Show("Tried to add invalid data!");
                }
            }
            catch
            {
                MessageBox.Show("Tried to add invalid data!");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(Data.SelectedIndex >= 0)
            {
                entries.RemoveAt(Data.SelectedIndex);
                Evaluate();
                SetResults();
                SaveData();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void MapNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if(MapNumber.Text.Trim().Length > Length) throw new Exception();
                string value = Convert.ToInt32(MapNumber.Text.Trim()).ToString($"D{Length}");
                string mapped = string.Empty;
                for(int i = 0; i < Length; i++)
                {
                    int digit = value[i] - '0';
                    if(result[i, digit] == '\0')
                    {
                        mapped += '*';
                    }
                    else
                    {
                        mapped += result[i, digit];
                    }
                }
                Mapped.Text = mapped;
            }
            catch
            {
                Mapped.Text = "-ERROR-";
            }
        }

        private void MapString_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if(MapString.Text.Trim().Length != Length) throw new Exception();
                string str = MapString.Text.Trim();
                string mapped = string.Empty;
                for(int i = 0; i < Length; i++)
                {
                    for(int d = 0; d < 10; d++)
                    {
                        if(result[i, d] == str[i])
                        {
                            mapped += d;
                            goto NextLoop;
                        }
                    }
                    // no match
                    mapped += '*';
                NextLoop:
                    continue;
                }
                Mapped_2.Text = mapped;
            }
            catch
            {
                Mapped_2.Text = "-ERROR-";
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string text = string.Empty;
            for(int i = 0; i < Length; i++)
            {
                for(int d = 0; d < 10; d++)
                {
                    char letter = result[i, d];
                    text += letter != '\0' ? letter : '*';
                }
                text += '\n';
            }
            Clipboard.SetText(text);
        }
    }

    public class Entry
    {
        public int Number { get; set; }
        public string String { get; set; } = "";
        public string Remarks { get; set; } = "";

        public bool IsValid(int length)
        {
            if(Number <= 0) return false;
            if(string.IsNullOrWhiteSpace(String)) return false;
            if(Number.ToString().Length > length || String.Length != length) return false;
            return true;
        }
    }
}
