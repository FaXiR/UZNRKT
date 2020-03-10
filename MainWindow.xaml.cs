﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CourseProject.Modules;
using KursProject.Modules;
using UZNRKT.Modules;

namespace UZNRKT
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Упрощенное взаимодействие с БД
        /// </summary>
        private UsingAccess UsAc;
        /// <summary>
        /// Упрощенное взаимодействие с таблицами
        /// </summary>
        private Tables Table;
        /// <summary>
        /// Логин пользователя
        /// </summary>
        private string UserName = null;
        /// <summary>
        /// ID пользователя
        /// </summary>
        private string UserID = null;
        /// <summary>
        /// Роль пользователя
        /// </summary>
        private string UserRole = null;
        /// <summary>
        /// Путь до БД
        /// </summary>
        private string BDWay = Environment.CurrentDirectory + "\\db.accdb";

        public MainWindow()
        {
            InitializeComponent();

            AutorizationUser();

            FoundApplicationInList(null, null, null, null);

            LoadClientAndStatus();
        }

        /// <summary>
        /// Создание подключения
        /// </summary>
        /// <returns>Успех подключения</returns>
        private bool CreateConnection()

        {
            try
            {
                UsAc = new UsingAccess(BDWay, null, null, null);
                return true;
            }
            catch
            {
                try
                {
                    UsAc = new UsingAccess(BDWay, null, null, "install");
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// Событие при закрытии приложения
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Console.WriteLine("Событие закрытия окна");

            //Если подключения к БД нет или пользователь не авторизован - закрыть приложение без раздумий
            if (UsAc == null || UserID == null)
            {
                return;
            }

            //Опрос пользователя
            if (MessageBox.Show("Выйти из программы?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
            {
                try
                {
                    UsAc.ConnectClose();
                }
                finally
                {
                    e.Cancel = true;
                }
            }
        }
        /// <summary>
        /// Подключение к БД и авторизация пользователя
        /// </summary>
        private void AutorizationUser()
        {
            Console.WriteLine("Подключение к БД");
            //Подключение к БД
            try
            {
                //Чтение пути до БД из файла
                string way = File.ReadAllLines("db.txt", Encoding.GetEncoding(1251))[0];
                if (way != "")
                {
                    BDWay = way;
                }
            }
            catch { }
            Console.WriteLine("Путь к БД: " + BDWay);
            Console.WriteLine("Подключение к Access");
            if (!CreateConnection())
            {
                //Если подключиться не удалось
                MessageBox.Show("Не удалось подключится к базе данных, пожалуйста, обратитесь к администратору");
                this.Close();
                return;
            }
            UsAc.AutoOpen = true;
            Console.WriteLine("Подключено");
            Console.WriteLine("Авторизация");
            //Авторизация пользователя
            var window = new Windows.LoginPassword(UsAc);
            if (window.ShowDialog() == true)
            {
                //Запись данных о вошедшем пользователе
                UserRole = window.Role;
                UserName = window.Login;
                UserID = window.ID;

                Console.WriteLine(UserRole);
                Console.WriteLine(UserName);
                Console.WriteLine(UserID);
                this.Show();
            }
            else
            {
                //Вход был отменен
                this.Close();
                return;
            }

            //Объявление таблиц
            Table = new Tables(UsAc);
        }

        /// <summary>
        /// События нажатия элемента в Menu
        /// </summary>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            switch (menuItem.Header.ToString())
            {
                case "О программе":
                    {
                        var InfoWindow = new Windows.AppInfo();
                        Console.WriteLine("Открытие окна о программе");
                        InfoWindow.ShowDialog();
                    }
                    break;
                case "Выход":
                    {
                        UserName = UserID = UserRole = null;
                        this.Hide();
                        Console.WriteLine("Выход из учетки");
                        AutorizationUser();
                    }
                    break;

                default:
                    MessageBox.Show(menuItem.Header.ToString());
                    break;
            }

        }





        /// <summary>
        /// Событие автогенерации колонок. Отлавливает и корректирует поля Даты.
        /// </summary>
        private void DataGrid_OnAutoGenerating(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy";
        }

        /// <summary>
        /// Событие клика по записи. Задает index выбранной записи
        /// </summary>
        private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            DataGrid DG = (DataGrid)sender;

            //Получение имени
            string name = DG.Name;
            //Получение номера записи
            int index = DG.SelectedIndex;

            if (index == -1)
            {
                Title_SelectApplication = null;
            }
            else
            {
                Title_SelectApplication = ((DataView)DG.ItemsSource).Table.Rows[index]["ID заявки"].ToString();
            }
        }

        /// <summary>
        /// Подргужает известных клиентов и статусы в F_Grid_Applications_Client и F_Grid_Applications_Status
        /// </summary>
        private void LoadClientAndStatus()
        {
            ObservableCollection<string> ClientList = new ObservableCollection<string>();
            ClientList.Add("---");

            var tab = UsAc.Execute("SELECT FIO_Client FROM Clients");
            for (int i = 0; i< tab.Count; i++)
            {
                ClientList.Add(tab.Table.Rows[i]["FIO_Client"].ToString());
            }
            F_Grid_Applications_Client.ItemsSource = ClientList;


            ObservableCollection<string> StatusList = new ObservableCollection<string>();
            StatusList.Add("---");

            tab = UsAc.Execute("SELECT Statys_Statys FROM Statys");
            for (int i = 0; i < tab.Count; i++)
            {
                StatusList.Add(tab.Table.Rows[i]["Statys_Statys"].ToString());
            }
            F_Grid_Applications_Status.ItemsSource = StatusList;

            F_Grid_Applications_Client.SelectedIndex = 0;
            F_Grid_Applications_Status.SelectedIndex = 0;
        }


        /// <summary>
        /// Задает число найденных заявок
        /// </summary>
        private string Title_ApplicationsListCount
        {
            set
            {
                if (value == null)
                {
                    F_Grid_Applications_TitleCountApplication.Text = null;
                }
                else
                {
                    F_Grid_Applications_TitleCountApplication.Text = "найдено " + value;
                }
            }
        }

        /// <summary>
        /// Возвращает или задает index выбранной заявки (Индекс = номер заявки)
        /// </summary>
        private string Title_SelectApplication
        {
            get
            {
                return _selectApplicationIndex;
            }
            set
            {
                if (value == null)
                {
                    F_Grid_Applications_TitleSelectApplication.Text = value;
                    F_Grid_Applications_TitleSelect.Visibility = Visibility.Hidden;
                }
                else
                {
                    F_Grid_Applications_TitleSelectApplication.Text = value;
                    F_Grid_Applications_TitleSelect.Visibility = Visibility.Visible;
                }

                _selectApplicationIndex = value;
            }
        }
        private string _selectApplicationIndex = null;


        /// <summary>
        /// Событие нажатия кнопки добавления записи
        /// </summary>
        private void F_Grid_Applications_AddClick(object sender, RoutedEventArgs e)
        {
            Windows.AddApplication AddApp = new Windows.AddApplication(UsAc);

            string DateApplication;
            string Client;
            string Type;
            string Producer;

            //Получение результата
            if (AddApp.ShowDialog() == true)
            {
                DateApplication = AddApp.DateApplication;
                Client = AddApp.Client;
                Type = AddApp.Type;
                Producer = AddApp.Producer;
            }
            else
            {
                MessageBox.Show("Запись была отменена");
                return;
            }

            string request = $@"INSERT INTO Zayavki (Date_Zayavki, Client, Type_Tehniki_Zayavki, Izgotovitel) VALUES (""{DateApplication}"", {Client}, {Type}, {Producer})";

            UsAc.ExecuteNonQuery(request);
        }

        /// <summary>
        /// Событие нажатия кнопки сброса списка заявок
        /// </summary>
        private void F_Grid_Applications_ResetClick(object sender, RoutedEventArgs e)
        {
            FoundApplicationInList(null, null, null, null);
        }

        /// <summary>
        /// Событие нажатия кнопки поиска заявок
        /// </summary>
        private void F_Grid_Applications_FoundClick(object sender, RoutedEventArgs e)
        {
            FoundApplicationInList(F_Grid_Applications_ID_Application.Text, F_Grid_Applications_DateApplication.Text, F_Grid_Applications_Client.Text, F_Grid_Applications_Status.Text);
        }

        /// <summary>
        /// Событие нажатия кнопки удаления записи в списке заявок
        /// </summary>
        private void F_Grid_Applications_DeleteClick(object sender, RoutedEventArgs e)
        {
            if (Title_SelectApplication == null)
            {
                return;
            }

            try
            {
                Table.Zayavki.DeleteFrom($@"ID_Zayavki = ""{ Title_SelectApplication}""");
            }
            finally
            {
                MessageBox.Show("Запись удалена, обновите таблицу");
                Title_SelectApplication = null;
            }
        }

        /// <summary>
        /// Поиск записей в таблице Zayavki.
        /// </summary>
        private void FoundApplicationInList(string ID_Application, string Date_Application, string FIO_Client, string Statys_Statys)
        {
            string request = null;

            if (!(ID_Application == null && ID_Application != "" || Date_Application == null && Date_Application != "" || FIO_Client == null && FIO_Client != "" || Statys_Statys == null && Statys_Statys != ""))
            {
                request += "1=1";

                if (ID_Application != null && ID_Application != "")
                {
                    request += $@" and Zayavki.ID_Zayavki = {ID_Application}";
                }

                if (Date_Application != null && Date_Application != "")
                {
                    string FoundDate = Date_Application.Substring(3, 2) + "/" + Date_Application.Substring(0, 2) + "/" + Date_Application.Substring(6, 4);
                    request += $@" and Zayavki.Date_Zayavki = #{FoundDate}#";
                }

                if (FIO_Client != null && FIO_Client != "" && FIO_Client != "---")
                {
                    request += $@" and Clients.FIO_Client = ""{FIO_Client}""";
                }

                if (Statys_Statys != null && Statys_Statys != "" && Statys_Statys != "---")
                {
                    request += $@" and Statys.Statys_Statys = ""{Statys_Statys}""";
                }
            }

            Table.Zayavki.Where = request;
            Table.Zayavki.UpdateTable();

            F_ListOfApplications.ItemsSource = Table.Zayavki.DVTable;
            Title_ApplicationsListCount = Table.Zayavki.DVTable.Count.ToString();
        }
    }
}
