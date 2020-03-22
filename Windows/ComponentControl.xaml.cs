using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using UZNRKT.Modules;

namespace UZNRKT.Windows
{
    /// <summary>
    /// Логика взаимодействия для ComponentControl.xaml
    /// </summary>
    public partial class ComponentControl : Window
    {
        UsingAccess UsAc;
        string ID_TMC;

        public ComponentControl(UsingAccess UsAc, string ID_TMC)
        {
            InitializeComponent();

            this.UsAc = UsAc;
            this.ID_TMC = ID_TMC;

            LoadComboBox("Nazvanie_Postavchik", "Postavchiki", F_Postav);

            if (ID_TMC == null)
            {

            }
            else
            {
                F_Name.IsEnabled = false;
                F_Postav.IsEnabled = false;
                F_Button_AddEdit.Content = "Изменить";

                SetValue(ID_TMC);
            }
        }

        /// <summary>
        /// Подругражет значение в ComboBox
        /// </summary>
        /// <param name="select">Колонка</param>
        /// <param name="from">Таблица</param>
        /// <param name="boxName">Имя combobox'а</param>
        private void LoadComboBox(string select, string from, ComboBox boxName)
        {
            ObservableCollection<string> list = new ObservableCollection<string>();
            list.Add("---");

            var tab = UsAc.Execute($@"SELECT {select} FROM {from}");
            for (int i = 0; i < tab.Count; i++)
            {
                list.Add(tab.Table.Rows[i][select].ToString());
            }

            boxName.ItemsSource = list;
            boxName.SelectedIndex = 0;
        }

        /// <summary>
        /// Выставление значений Combobox'ам
        /// </summary>
        private void SetValue(string ID_TMC)
        {
            var table = UsAc.Execute($"SELECT Postavchiki.Nazvanie_Postavchik, TMC.Nazvanie_TMC, TMC.Kolichestvo_TMC FROM Postavchiki RIGHT JOIN TMC ON Postavchiki.ID_Postavchik = TMC.Postavchik_TMC WHERE TMC.ID_TMC = {ID_TMC}").Table;
            string value = table.Rows[0]["Nazvanie_Postavchik"].ToString();

            //Получаем лист из Combobox
            var ComboboxList = (ObservableCollection<string>)F_Postav.ItemsSource;

            //Перебираем значение Combobox
            for (int i = 0; i < ComboboxList.Count; i++)
            {
                if (value == ComboboxList[i])
                {
                    F_Postav.SelectedIndex = i;
                    return;
                }
            }
            F_Postav.SelectedIndex = 0;

            F_Name.Text = table.Rows[0]["Nazvanie_TMC"].ToString();
            F_Count.Text = table.Rows[0]["Kolichestvo_TMC"].ToString();
        }

        private void ButtonClick_add(object sender, RoutedEventArgs e)
        {
            bool AutoOpenInUsAc = UsAc.AutoOpen;

            UsAc.AutoOpen = false;
            UsAc.ConnectOpen();

            if (F_Button_AddEdit.Content.ToString() == "Добавить")
            {
                if (!AddNewApplication())
                    return;
            }
            else
            {
                if (!SaveApplication())
                    return;
            }

            UsAc.ConnectClose();
            UsAc.AutoOpen = AutoOpenInUsAc;

            this.DialogResult = true;
        }

        private bool AddNewApplication()
        {
            if (F_Name.Text.Length <= 0)
            {
                MessageBox.Show("Впишите название для компонента");
                return false;
            }
            try
            {
                if (F_Count.Text.Length <= 0)
                {
                    MessageBox.Show("Введите количество компонентов");
                    return false;
                }

                Convert.ToInt32(F_Count.Text);
            }
            catch
            {
                MessageBox.Show("В поле количество присутсвуют лишние символы");
                return false;
            }
            if (F_Postav.SelectedItem.ToString() == "---")
            {
                MessageBox.Show("Поставщик не выбран");
                return false;
            }

            string ID_Postavchik = UsAc.Execute($@"SELECT ID_Postavchik FROM Postavchiki WHERE Nazvanie_Postavchik = ""{F_Postav.SelectedItem.ToString()}""").Table.Rows[0]["ID_Postavchik"].ToString();

            UsAc.ExecuteNonQuery($@"INSERT INTO TMC (Nazvanie_TMC, Kolichestvo_TMC, Postavchik_TMC) VALUES (""{F_Name.Text}"", {F_Count.Text}, {ID_Postavchik})");

            return true;
        }
        private bool SaveApplication()
        {
            try
            {
                if (F_Count.Text.Length <= 0)
                {
                    MessageBox.Show("Введите количество компонентов");
                    return false;
                }

                Convert.ToInt32(F_Count.Text);
            }
            catch
            {
                MessageBox.Show("В поле количество присутсвуют лишние символы");
                return false;
            }

            return false;
        }

        private void ButtonClick_cancel(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
