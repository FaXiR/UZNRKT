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
    /// Логика взаимодействия для UserControl.xaml
    /// </summary>
    public partial class UserControl : Window
    {
        UsingAccess UsAc;
        string ID_User = null;

        public UserControl(UsingAccess UsAc, string ID_User)
        {
            InitializeComponent();

            this.UsAc = UsAc;
            LoadComboBox();

            if (ID_User != null)
            {
                //SetValue(ID_User);
                F_Button_AddEdit.Content = "Сохранить";
                Title = "Редактирование пользователя с # ID " + ID_User;
            }
            else
            {

                F_Button_AddEdit.Content = "Добавить";
                Title = "Добавление пользователя";
            }
        }

        /// <summary>
        /// Подружает все значения в поля
        /// </summary>
        private void LoadComboBox()
        {
            ObservableCollection<string> list = new ObservableCollection<string>();
            list.Add("---");

            var tab = UsAc.Execute($@"SELECT Nazvanie_Doljnost FROM Doljnosti");
            for (int i = 0; i < tab.Count; i++)
            {
                list.Add(tab.Table.Rows[i]["Nazvanie_Doljnost"].ToString());
            }

            F_Doljnost.ItemsSource = list;
            F_Doljnost.SelectedIndex = 0;
        }

        private string GetId(string table, string idInColumn, string foundWord, string FoundInColumn)
        {
            try
            {
                return UsAc.Execute($@"SELECT {idInColumn} FROM {table} WHERE {FoundInColumn} = ""{foundWord}""").Table.Rows[0][idInColumn].ToString();
            }
            catch
            {
                return null;
            }
        }

        private void ButtonClick_add(object sender, RoutedEventArgs e)
        {
            bool AutoOpenInUsAc = UsAc.AutoOpen;

            UsAc.AutoOpen = false;
            UsAc.ConnectOpen();

            bool SuccessAdd = false;

            if (F_Button_AddEdit.Content.ToString() == "Добавить")
            {
                SuccessAdd = AddNewUser();
            }
            else
            {
                //SuccessAdd = SaveUser();
            }

            UsAc.ConnectClose();
            UsAc.AutoOpen = AutoOpenInUsAc;

            if (SuccessAdd)
                this.DialogResult = true;
        }
        private void ButtonClick_cancel(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private bool AddNewUser()
        {
            //Проверка длины логина
            if (F_Login.Text.Length < 4)
            {
                MessageBox.Show("Длина логина не может быть короче 4 символов");
                return false;
            }

            //Добавление как пользователя
            UsAc.ExecuteNonQuery($@"INSERT INTO Users (Login_User, Password_User, Role) VALUES (""{F_Login.Text}"","""", 2)");

            //Получение ID пользователя
            string ID = null;
            var table = UsAc.Execute("SELECT Max(Users.ID_User) AS [ID] FROM Users");
            ID = table.Table.Rows[0]["ID"].ToString();

            //Добавление как сотрдуника
            string value = null;

            string From = null;
            string Values = null;

            value = F_FIO.Text;
            if (value.Length != 0)
            {
                From += "FIO_Master";
                Values += $@"""{F_FIO.Text}""";
            }

            value = F_Doljnost.Text;
            if (value != "---")
            {
                From += ",Doljnost_Master";
                Values += "," + GetId("Doljnosti", "ID_Doljnost", F_Doljnost.SelectedItem.ToString(), "Nazvanie_Doljnost");
            }

            value = F_Birthday.Text;
            if (value.Length != 0)
            {
                From += ",DateOfBirth_Master";
                Values += $@",""{F_Birthday.Text}""";
            }

            value = F_Phone.Text;
            if (value.Length != 0)
            {
                From += ",Phone_Master";
                Values += $@",""{F_Phone.Text}""";
            }

            value = F_Address.Text;
            if (value.Length != 0)
            {
                From += ",Adress_Master";
                Values += $@",""{F_Address.Text}""";
            }

            value = F_Email.Text;
            if (value.Length != 0)
            {
                From += ",Email_Master";
                Values += $@",""{F_Email.Text}""";
            }

            From += ",IDUsera";
            Values += $@",{ID}";

            UsAc.ExecuteNonQuery($@"INSERT INTO Sotrudniki ({From}) VALUES ({Values})");

            return true;
        }
    }
}
