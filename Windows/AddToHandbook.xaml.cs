using CourseProject.Modules;
using KursProject.Modules;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace UZNRKT.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddToHandbook.xaml
    /// </summary>
    public partial class AddToHandbook : Window
    {
        private UsingAccess UsAc;
        private string from;
        private string TableName;
        DataView table;

        List<string> TypeOfColumnt = new List<string>();
        List<string> NameOfColumn = new List<string>();
        List<TextBox> Field = new List<TextBox>();


        public AddToHandbook(string TitleForWindow, UsingAccess UsAc, string from)
        {
            InitializeComponent();
            this.Title = TitleForWindow;
            this.from = from;
            this.UsAc = UsAc;
            this.table = UsAc.Execute("SELECT * FROM " + from);

            CreateFields();
        }

        private void CreateFields()
        {
            //Получение названий колонок
            var ColumnName = new List<string>();
            for (int i = 0; i < table.Table.Columns.Count; i++)
            {
                ColumnName.Add(table.Table.Columns[i].ToString());
            }
            NameOfColumn = ColumnName;

            for (int x = 0; x < ColumnName.Count; x++)
            {
                TypeOfColumnt.Add(table.Table.Rows[0][ColumnName[x]].GetType().ToString());
            }

            //Получение максимального ID
            string MAX = (Convert.ToInt32(UsAc.Execute($@"SELECT MAX({ColumnName[0]}) as {ColumnName[0]} from {from}").Table.Rows[0][ColumnName[0]].ToString()) + 1).ToString();

            //Перебор названия колонок и создание соотв. textBlock' ов
            foreach (string name in ColumnName)
            {
                var TB = new TextBlock
                {
                    Text = name + ":",
                    Height = 24,
                    Margin = new Thickness(2),
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                F_StacPanelTextBlock.Children.Add(TB);
            }

            int j = 0;

            //Перебор типов данных и создание соотв полей
            foreach (string type in TypeOfColumnt)
            {
                TextBox textbox = new TextBox
                {
                    Height = 24,
                    Width = 136,
                    Margin = new Thickness(2),
                    HorizontalAlignment = HorizontalAlignment.Left,
                };

                if (j == 0)
                {
                    textbox.Text = MAX;
                    textbox.IsEnabled = false;
                }

                Field.Add(textbox);
                F_StacPanelFields.Children.Add(textbox);

                j++;
            }

            //Выставление высоты
            this.Height = (TypeOfColumnt.Count + 3) * 28;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CreateRecord();
            this.DialogResult = true;
        }

        private void CreateRecord()
        {
            string column = null;
            string value = null;

            for (int i = 0; i < TypeOfColumnt.Count; i++)
            {
                //Проверка на наличие текста в TextBox
                if (Field[i].Text != "")
                {
                    column += $@"{NameOfColumn[i]},";

                    if (TypeOfColumnt[i] == "System.Int32")
                    {
                        value += $@"{Field[i].Text},";
                    }
                    else
                    {
                        value += $@"""{Field[i].Text}"",";
                    }
                }
            }

            column = column.Substring(0, column.Length - 1);
            value = value.Substring(0, value.Length - 1);
            UsAc.Execute($@"INSERT INTO {from} ({column}) VALUES ({value})");
        }
    }
}
