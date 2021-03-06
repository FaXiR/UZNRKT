﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private string BDWay = Environment.CurrentDirectory + "\\db.mdb";
        /// <summary>
        /// Время авторизации пользователя
        /// </summary>
        private string TimeIn = null;

        public MainWindow()
        {
            InitializeComponent();

            //Окно загрузки (Т.к. БД может долго подключатся и приложение висит, то это окно будет сигнализировать о запуске приложения)
            var loadingStatus = new Windows.LoadingApp();
            loadingStatus.Show();

            if (CreateConnection() == true)
            {
                //Открытие соединения
                UsAc.ConnectOpen();

                //Объявление таблиц
                Table = new Tables(UsAc);

                //Закрытия окна загрузки
                loadingStatus.Close();

                //Авторизация
                AutorizationUser();

                //Запуск таймера, для отображения текущнго времени
                var timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.IsEnabled = true;
                timer.Tick += (o, e) => { DisplayingTime.Text = DateTime.Now.ToString(); };
                timer.Start();

            }
            else
            {
                //Закрытия окна загрузки
                loadingStatus.Close();
            }
        }

        /// <summary>
        /// Задает выбранный номер строки в справочнике
        /// </summary>
        private string DataGrid_Handbooks_SelectItem
        {
            get
            {
                return _dataGrid_Handbooks_SelectItem;
            }
            set
            {
                if (value == null)
                {
                    _dataGrid_Handbooks_SelectItem = null;
                    F_Grid_Handbooks_SelectHandbooksIndex.Text = null;
                    F_Grid_Handbooks_SelectHandbooksStackPanel.Visibility = Visibility.Hidden;
                }
                else
                {
                    _dataGrid_Handbooks_SelectItem = value;
                    F_Grid_Handbooks_SelectHandbooksIndex.Text = value;
                    F_Grid_Handbooks_SelectHandbooksStackPanel.Visibility = Visibility.Visible;
                }
            }
        }
        private string _dataGrid_Handbooks_SelectItem = null;


        /// <summary>
        /// Задает выбранный номер строки в списке пользователей
        /// </summary>
        private string DataGrid_Users_SelectItem
        {
            get
            {
                return _dataGrid_Users_SelectItem;
            }
            set
            {
                if (value == null)
                {
                    _dataGrid_Users_SelectItem = null;
                    F_Grid_User_SelectedUser.Text = null;
                    F_Grid_User_SelectedUser_StackPanel.Visibility = Visibility.Hidden;
                }
                else
                {
                    _dataGrid_Users_SelectItem = value;
                    F_Grid_User_SelectedUser.Text = value;
                    F_Grid_User_SelectedUser_StackPanel.Visibility = Visibility.Visible;
                }
            }
        }
        private string _dataGrid_Users_SelectItem = null;

        /// <summary>
        /// Создание подключения
        /// </summary>
        /// <returns>Успех подключения</returns>
        private bool CreateConnection()
        {
            //Определение пути до БД
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

            //Подключение к БД
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
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось подключится к базе данных, пожалуйста, обратитесь к администратору \n\n" + ex.ToString(), "Ошибка подклчения к БД");
                    this.Close();
                    return false;
                }
            }
        }

        /// <summary>
        /// Событие при закрытии приложения
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Если подключения к БД нет или пользователь не авторизован - закрыть приложение без раздумий
            if (UsAc == null || UserID == null || Table == null)
            {
                e.Cancel = false;
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
        private bool AutorizationUser()
        {
            //Авторизация пользователя
            var window = new Windows.LoginPassword(UsAc);
            if (window.ShowDialog() == true)
            {
                //Запись данных о вошедшем пользователе
                UserRole = window.Role;
                UserName = window.Login;
                UserID = window.ID;

                //Определение доступных пунктов
                LoadMenu();
                //Запись времени авторизации
                AddLogToAutorization(1);

                //Обновляет таблицу заявок
                FoundApplicationInList(null, null, null, null);

                //Обновляет таблицу склада
                FoundStorageInList(null, null);

                //Определяет доступных клиентов, статусы и поставщиков
                LoadClientStatusPostavshik();

                //Сброс выбранной справки
                F_DataGrid_Handbook.ItemsSource = null;
                F_Grid_Handbooks_SelectHandbooks.Text = null;
                DataGrid_Handbooks_SelectItem = null;

                //Выставление главной страницы
                F_Grid_Applications.Visibility = Visibility.Visible;
                F_Grid_Storage.Visibility = Visibility.Hidden;
                F_Grid_User.Visibility = Visibility.Hidden;
                F_Grid_Handbooks.Visibility = Visibility.Hidden;
                this.Show();
            }
            else
            {
                //Вход был отменен
                this.Close();
                return false;
            }

            return true;
        }

        /// <summary>
        /// События нажатия элемента в Menu
        /// </summary>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            switch (menuItem.Header.ToString())
            {
                case "Заявки":
                    //Обновление списка юзеров, мастеров и поставщиков
                    LoadClientStatusPostavshik();

                    F_Grid_Applications.Visibility = Visibility.Visible;
                    F_Grid_Storage.Visibility = Visibility.Hidden;
                    F_Grid_User.Visibility = Visibility.Hidden;
                    F_Grid_Handbooks.Visibility = Visibility.Hidden;
                    break;
                case "Склад":
                    F_Grid_Applications.Visibility = Visibility.Hidden;
                    F_Grid_Storage.Visibility = Visibility.Visible;
                    F_Grid_User.Visibility = Visibility.Hidden;
                    F_Grid_Handbooks.Visibility = Visibility.Hidden;
                    break;
                case "Справочники":
                    F_Grid_Applications.Visibility = Visibility.Hidden;
                    F_Grid_Storage.Visibility = Visibility.Hidden;
                    F_Grid_User.Visibility = Visibility.Hidden;
                    F_Grid_Handbooks.Visibility = Visibility.Visible;
                    break;
                case "Печать":
                    var window = new Windows.Report(UsAc, (DataView)F_ListOfApplications.ItemsSource, Title_SelectApplication);
                    window.ShowDialog();
                    break;
                case "О программе":
                    {
                        var InfoWindow = new Windows.AppInfo();
                        InfoWindow.ShowDialog();
                    }
                    break;
                case "Выход":
                    {
                        AddLogToAutorization(0);
                        UserName = UserID = UserRole = null;
                        this.Hide();
                        AutorizationUser();
                    }
                    break;
                case "Пользователи":
                    //Обновление данных в таблицах
                    Table.AutorizationTime.UpdateTable();

                    Table.Users.Where = null;
                    Table.Users.UpdateTable();

                    F_TimeAutorization.ItemsSource = Table.AutorizationTime.DVTable;
                    F_Grid_User_Users.ItemsSource = Table.Users.DVTable;

                    F_Grid_Applications.Visibility = Visibility.Hidden;
                    F_Grid_Storage.Visibility = Visibility.Hidden;
                    F_Grid_User.Visibility = Visibility.Visible;
                    F_Grid_Handbooks.Visibility = Visibility.Hidden;
                    break;
                case "Помощь":
                    //Проверка наличия файла помощи
                    if (File.Exists(Environment.CurrentDirectory + "\\Help.chm"))
                    {
                        Process.Start(Environment.CurrentDirectory + "\\Help.chm");
                    }
                    else
                    {
                        MessageBox.Show("Файл помощи не найден!");
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
        /// Событие клика по записи в справочниках. Задает index выбранной записи
        /// </summary>
        private void DataGrid_SelectedCellsChangedInHandbooks(object sender, SelectedCellsChangedEventArgs e)
        {
            DataGrid DG = (DataGrid)sender;
            int index = DG.SelectedIndex;

            if (index == -1)
                DataGrid_Handbooks_SelectItem = null;
            else
                DataGrid_Handbooks_SelectItem = index.ToString();
            return;
        }

        private void DataGrid_SelectedCellsChangedInTMC(object sender, SelectedCellsChangedEventArgs e)
        {
            DataGrid DG = (DataGrid)sender;
            int index = DG.SelectedIndex;

            if (index == -1)
                Title_SelectTMC = null;
            else
                Title_SelectTMC = ((DataView)DG.ItemsSource).Table.Rows[index]["ID"].ToString();
            return;
        }

        /// <summary>
        /// Событие клика по записи в справочниках. Задает index выбранной записи
        /// </summary>
        private void DataGrid_SelectedCellsChangedInUserList(object sender, SelectedCellsChangedEventArgs e)
        {
            DataGrid DG = (DataGrid)sender;
            int index = DG.SelectedIndex;

            if (index == -1)
                DataGrid_Users_SelectItem = null;
            else
                DataGrid_Users_SelectItem = ((DataView)DG.ItemsSource).Table.Rows[index]["# ID"].ToString();
            return;
        }

        /// <summary>
        /// Событие клика по листу в справочниках. Выставляет таблицу в зависимости от выбора
        /// </summary>
        private void ListBox_SelectedItemChanged(object sender, SelectionChangedEventArgs e)
        {
            string Handbooks = F_ListBox_Handbooks.SelectedItem.ToString();
            F_Grid_Handbooks_SelectHandbooks.Text = Handbooks;
            DataGrid_Handbooks_SelectItem = null;

            F_Grid_Handbooks_AddButton.IsEnabled = true;
            F_Grid_Handbooks_DeleteButton.IsEnabled = true;

            switch (Handbooks)
            {
                case "Сотрудники/Мастера":
                    Table.Sotrudniki.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.Sotrudniki.DVTable;

                    if (UserRole == "1")
                    {
                        F_Grid_Handbooks_AddButton.IsEnabled = true;
                        F_Grid_Handbooks_DeleteButton.IsEnabled = true;
                    }
                    else if (UserRole == "2")
                    {
                        F_Grid_Handbooks_AddButton.IsEnabled = false;
                        F_Grid_Handbooks_DeleteButton.IsEnabled = false;
                    }
                    break;
                case "Клиенты":
                    Table.Clients.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.Clients.DVTable;
                    break;
                case "Тип неисправности":
                    Table.Neispravnosti.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.Neispravnosti.DVTable;
                    break;
                case "Изготовители":
                    Table.Izgotovitel.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.Izgotovitel.DVTable;
                    break;
                case "Услуги":
                    Table.Services.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.Services.DVTable;
                    break;
                case "Стутс заявки":
                    Table.Statys.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.Statys.DVTable;
                    break;
                case "Тип техники":
                    Table.TypeTehniki.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.TypeTehniki.DVTable;
                    break;
                case "Оборудование":
                    Table.Oborudovanie.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.Oborudovanie.DVTable;
                    break;
                case "Договора о поставке":
                    Table.DogovorOPostavke.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.DogovorOPostavke.DVTable;
                    break;
                case "Должности":
                    Table.Doljnosti.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.Doljnosti.DVTable;

                    if (UserRole == "1")
                    {
                        F_Grid_Handbooks_AddButton.IsEnabled = true;
                        F_Grid_Handbooks_DeleteButton.IsEnabled = true;
                    }
                    else if (UserRole == "2")
                    {
                        F_Grid_Handbooks_AddButton.IsEnabled = false;
                        F_Grid_Handbooks_DeleteButton.IsEnabled = false;
                    }

                    break;
            }
        }

        /// <summary>
        /// Подргужает известных клиентов и статусы в F_Grid_Applications_Client и F_Grid_Applications_Status
        /// </summary>
        private void LoadClientStatusPostavshik()
        {
            ObservableCollection<string> ClientList = new ObservableCollection<string>();
            ClientList.Add("---");

            var tab = UsAc.Execute("SELECT FIO_Client FROM Clients");
            for (int i = 0; i < tab.Count; i++)
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


            ObservableCollection<string> PostavshikList = new ObservableCollection<string>();
            PostavshikList.Add("---");

            tab = UsAc.Execute("SELECT Nazvanie_Postavchik FROM Postavchiki");
            for (int i = 0; i < tab.Count; i++)
            {
                PostavshikList.Add(tab.Table.Rows[i]["Nazvanie_Postavchik"].ToString());
            }
            F_Grid_Storage_Postavshik.ItemsSource = PostavshikList;

            F_Grid_Applications_Client.SelectedIndex = 0;
            F_Grid_Applications_Status.SelectedIndex = 0;
            F_Grid_Storage_Postavshik.SelectedIndex = 0;
        }

        /// <summary>
        /// Скрывает или делает доступным палень "Пользователи" в зависимости от роли.
        /// </summary>
        private void LoadMenu()
        {
            if (UserRole == "1")
            {
                F_Menu_UserItem.Visibility = Visibility.Visible;
                F_Menu_UserItem.IsEnabled = true;
                F_TimeAutorization.ItemsSource = Table.AutorizationTime.DVTable;
            }
            else if (UserRole == "2")
            {
                F_Menu_UserItem.Visibility = Visibility.Hidden;
                F_Menu_UserItem.IsEnabled = false;
            }
        }

        /// <summary>
        /// Создает запись о том, что пользователь вошел в то или иное время
        /// </summary>
        /// <param name="InOut">1 - вход. 0 - выход</param>
        private void AddLogToAutorization(int InOut)
        {
            if (UserID == null)
            {
                return;
            }

            try
            {
                if (InOut == 1)
                {
                    var time = DateTime.Now.ToString();
                    UsAc.ExecuteNonQuery($@"INSERT INTO Log_avtorizatcii (ID_User, Time_in) VALUES ({UserID}, ""{time}"")");
                    TimeIn = time;
                }
                else if (InOut == 0)
                {
                    UsAc.ExecuteNonQuery($@"UPDATE Log_avtorizatcii SET Time_out = ""{DateTime.Now.ToString()}"" WHERE Time_in = ""{TimeIn}""");
                    TimeIn = null;
                }
            }
            catch { }
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


        private string Title_SelectTMC
        {
            get
            {
                return _selectTMC;
            }
            set
            {
                if (value == null)
                {
                    F_Grid_Storage_TitleSelectTMC.Text = value;
                    F_Grid_Storage_TitleSelect.Visibility = Visibility.Hidden;
                }
                else
                {
                    F_Grid_Storage_TitleSelectTMC.Text = value;
                    F_Grid_Storage_TitleSelect.Visibility = Visibility.Visible;
                }
                _selectTMC = value;
            }
        }
        string _selectTMC = null;


        /// <summary>
        /// Событие нажатия кнопки добавления записи
        /// </summary>
        private void F_Grid_Applications_AddClick(object sender, RoutedEventArgs e)
        {
            Windows.AddApplication AddApp = new Windows.AddApplication(UsAc, null);

            //Получение результата
            if (AddApp.ShowDialog() == true)
            {
                //Обновление справочников
                F_Grid_Handbooks_AddButton.IsEnabled = true;
                F_Grid_Handbooks_DeleteButton.IsEnabled = true;

                if (F_ListBox_Handbooks.SelectedItem != null)
                {
                    switch (F_ListBox_Handbooks.SelectedItem.ToString())
                    {
                        case "Сотрудники/Мастера":
                            Table.Sotrudniki.UpdateTable();
                            F_DataGrid_Handbook.ItemsSource = Table.Sotrudniki.DVTable;
                            break;
                        case "Клиенты":
                            Table.Clients.UpdateTable();
                            F_DataGrid_Handbook.ItemsSource = Table.Clients.DVTable;
                            break;
                        case "Тип неисправности":
                            Table.Neispravnosti.UpdateTable();
                            F_DataGrid_Handbook.ItemsSource = Table.Neispravnosti.DVTable;
                            break;
                        case "Изготовители":
                            Table.Izgotovitel.UpdateTable();
                            F_DataGrid_Handbook.ItemsSource = Table.Izgotovitel.DVTable;
                            break;
                        case "Услуги":
                            Table.Services.UpdateTable();
                            F_DataGrid_Handbook.ItemsSource = Table.Services.DVTable;
                            break;
                        case "Стутс заявки":
                            Table.Statys.UpdateTable();
                            F_DataGrid_Handbook.ItemsSource = Table.Statys.DVTable;
                            break;
                        case "Тип техники":
                            Table.TypeTehniki.UpdateTable();
                            F_DataGrid_Handbook.ItemsSource = Table.TypeTehniki.DVTable;
                            break;
                        case "Оборудование":
                            Table.Oborudovanie.UpdateTable();
                            F_DataGrid_Handbook.ItemsSource = Table.Oborudovanie.DVTable;
                            break;
                        case "Договора о поставке":
                            Table.DogovorOPostavke.UpdateTable();
                            F_DataGrid_Handbook.ItemsSource = Table.DogovorOPostavke.DVTable;
                            break;
                        case "Должности":
                            Table.Doljnosti.UpdateTable();
                            F_DataGrid_Handbook.ItemsSource = Table.Doljnosti.DVTable;

                            if (UserRole == "1")
                            {
                                F_Grid_Handbooks_AddButton.IsEnabled = true;
                                F_Grid_Handbooks_DeleteButton.IsEnabled = true;
                            }
                            else if (UserRole == "2")
                            {
                                F_Grid_Handbooks_AddButton.IsEnabled = false;
                                F_Grid_Handbooks_DeleteButton.IsEnabled = false;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Запись была отменена");
                return;
            }
        }

        /// <summary>
        /// Событие нажатия кнопки редактирования записи
        /// </summary>
        private void F_Grid_Applications_EditClick(object sender, RoutedEventArgs e)
        {
            if (Title_SelectApplication == null)
            {
                return;
            }

            Windows.AddApplication AddApp = new Windows.AddApplication(UsAc, Title_SelectApplication);

            //Получение результата
            if (AddApp.ShowDialog() == true)
            {

            }
            else
            {
                MessageBox.Show("Редактирование было отменено");
                return;
            }
        }

        /// <summary>
        /// Событие нажатия кнопки сброса списка заявок
        /// </summary>
        private void F_Grid_Applications_ResetClick(object sender, RoutedEventArgs e)
        {
            FoundApplicationInList(null, null, null, null);
        }

        /// <summary>
        /// Событие нажатия кнопки сброса списка компонентов
        /// </summary>
        private void F_Grid_Storage_ResetClick(object sender, RoutedEventArgs e)
        {
            FoundStorageInList(null, null);
        }

        /// <summary>
        /// Событие нажатия кнопки сброса времени авторизаций
        /// </summary>
        private void F_Grid_Users_ResetClick(object sender, RoutedEventArgs e)
        {
            Table.AutorizationTime.Where = null;
            Table.AutorizationTime.UpdateTable();
            F_TimeAutorization.ItemsSource = Table.AutorizationTime.DVTable;

            DataGrid_Users_SelectItem = null;
        }

        private void F_Grid_Users_SelectUserAutorization(object sender, RoutedEventArgs e)
        {
            if (DataGrid_Users_SelectItem == null)
            {
                Table.AutorizationTime.Where = null;
            }
            else
            {
                Table.AutorizationTime.Where = $@"Users.ID_User = {DataGrid_Users_SelectItem}";
            }

            Table.AutorizationTime.UpdateTable();
            F_TimeAutorization.ItemsSource = Table.AutorizationTime.DVTable;
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
                UsAc.ExecuteNonQuery($@"DELETE FROM Zayavki Where ID_Zayavki = {Title_SelectApplication}");
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
                    if (int.TryParse(ID_Application, out int num))
                    {
                        request += $@" and Zayavki.ID_Zayavki = {ID_Application}";
                    }
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

        /// <summary>
        /// События завершения работы приложения
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            AddLogToAutorization(0);
        }

        /// <summary>
        /// Событие нажатия кнопки добавления записи в справочниках
        /// </summary>
        private void F_Grid_Handbooks_Add(object sender, RoutedEventArgs e)
        {
            string from = null;

            switch (F_ListBox_Handbooks.SelectedItem.ToString())
            {
                case "Сотрудники/Мастера":
                    from = "Sotrudniki";
                    break;
                case "Клиенты":
                    from = "Clients";
                    break;
                case "Тип неисправности":
                    from = "Neispravnosti";
                    break;
                case "Изготовители":
                    from = "Izgotovitel";
                    break;
                case "Услуги":
                    from = "Services";
                    break;
                case "Стутс заявки":
                    from = "Statys";
                    break;
                case "Тип техники":
                    from = "TypeTehniki";
                    break;
                case "Оборудование":
                    from = "Oborudovanie";
                    break;
                case "Договора о поставке":
                    from = "DogovorOPostavke";
                    break;
                case "Должности":
                    from = "Doljnosti";
                    break;
                default:
                    return;
            }

            var wind = new Windows.AddToHandbook("Добавление записи", UsAc, from);
            if (wind.ShowDialog() == true)
            {
                MessageBox.Show("Запись была создана");
            }
            else
            {
                MessageBox.Show("Запись отменена");
            }
        }

        /// <summary>
        /// Событие нажатия кнопки редактирования записи в справочниках
        /// </summary>
        private void F_Grid_Handbooks_Edit(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Событие нажатия кнопки удаления записи в справочниках
        /// </summary>
        private void F_Grid_Handbooks_Delete(object sender, RoutedEventArgs e)
        {
            if (DataGrid_Handbooks_SelectItem == null)
            {
                return;
            }

            string selectTable = null;
            //Получение ID
            switch (F_ListBox_Handbooks.SelectedItem.ToString())
            {
                case "Сотрудники/Мастера":
                    selectTable = "Sotrudniki";
                    break;
                case "Клиенты":
                    selectTable = "Clients";
                    break;
                case "Тип неисправности":
                    selectTable = "Neispravnosti";
                    break;
                case "Изготовители":
                    selectTable = "Izgotovitel";
                    break;
                case "Услуги":
                    selectTable = "Services";
                    break;
                case "Стутс заявки":
                    selectTable = "Statys";
                    break;
                case "Тип техники":
                    selectTable = "TypeTehniki";
                    break;
                case "Оборудование":
                    selectTable = "Oborudovanie";
                    break;
                case "Договора о поставке":
                    selectTable = "DogovorOPostavke";
                    break;
                case "Должности":
                    selectTable = "Doljnosti";
                    break;
                default:
                    return;
            }

            //Получение имени первого столбца
            var table = UsAc.Execute($@"SELECT * FROM {selectTable}");
            string IDColumnName = table.Table.Columns[0].ToString();
            string selectID = table.Table.Rows[Convert.ToInt32(DataGrid_Handbooks_SelectItem)][IDColumnName].ToString();


            UsAc.ExecuteNonQuery($@"DELETE FROM {selectTable} WHERE {IDColumnName} = {selectID}");

            MessageBox.Show("Запись была удалена");

            //Обновление справочников
            F_Grid_Handbooks_AddButton.IsEnabled = true;
            F_Grid_Handbooks_DeleteButton.IsEnabled = true;

            switch (selectTable)
            {
                case "Сотрудники/Мастера":
                    Table.Sotrudniki.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.Sotrudniki.DVTable;
                    break;
                case "Клиенты":
                    Table.Clients.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.Clients.DVTable;
                    break;
                case "Тип неисправности":
                    Table.Neispravnosti.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.Neispravnosti.DVTable;
                    break;
                case "Изготовители":
                    Table.Izgotovitel.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.Izgotovitel.DVTable;
                    break;
                case "Услуги":
                    Table.Services.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.Services.DVTable;
                    break;
                case "Стутс заявки":
                    Table.Statys.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.Statys.DVTable;
                    break;
                case "Тип техники":
                    Table.TypeTehniki.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.TypeTehniki.DVTable;
                    break;
                case "Оборудование":
                    Table.Oborudovanie.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.Oborudovanie.DVTable;
                    break;
                case "Договора о поставке":
                    Table.DogovorOPostavke.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.DogovorOPostavke.DVTable;
                    break;
                case "Должности":
                    Table.Doljnosti.UpdateTable();
                    F_DataGrid_Handbook.ItemsSource = Table.Doljnosti.DVTable;

                    if (UserRole == "1")
                    {
                        F_Grid_Handbooks_AddButton.IsEnabled = true;
                        F_Grid_Handbooks_DeleteButton.IsEnabled = true;
                    }
                    else if (UserRole == "2")
                    {
                        F_Grid_Handbooks_AddButton.IsEnabled = false;
                        F_Grid_Handbooks_DeleteButton.IsEnabled = false;
                    }
                    break;
            }
        }

        private void F_Grid_Users_AddClick(object sender, RoutedEventArgs e)
        {
            var AddUser = new Windows.UserControl(UsAc, null);
            if (AddUser.ShowDialog() == true)
            {
                Table.Users.Where = null;
                Table.Users.UpdateTable();

                F_Grid_User_Users.ItemsSource = Table.Users.DVTable;
            }
        }

        private void F_Grid_Users_PasswordResetClick(object sender, RoutedEventArgs e)
        {
            if (DataGrid_Users_SelectItem == null)
            {
                return;
            }

            UsAc.ExecuteNonQuery($@"UPDATE Users SET Password_User = null WHERE ID_User = {DataGrid_Users_SelectItem}");

            MessageBox.Show("Пароль был сброшен");
        }

        private void F_Grid_Users_DeleteClick(object sender, RoutedEventArgs e)
        {
            if (DataGrid_Users_SelectItem == null)
            {
                return;
            }

            UsAc.ExecuteNonQuery($@"DELETE FROM Users Where ID_User = {DataGrid_Users_SelectItem}");

            Table.Users.Where = null;
            Table.Users.UpdateTable();

            F_Grid_User_Users.ItemsSource = Table.Users.DVTable;
        }

        private void F_Grid_Storage_AddClick(object sender, RoutedEventArgs e)
        {
            var window = new Windows.ComponentControl(UsAc, null);
            if (window.ShowDialog() == true)
            {
                MessageBox.Show("Запись добавлена");
            }
            else
            {
                MessageBox.Show("Запись была отменена");
            }
        }

        private void F_Grid_Storage_EditClick(object sender, RoutedEventArgs e)
        {
            if (Title_SelectTMC == null)
            {
                return;
            }

            var window = new Windows.ComponentControl(UsAc, Title_SelectTMC);
            if (window.ShowDialog() == true)
            {
                MessageBox.Show("Запись обновлена");
            }
            else
            {
                MessageBox.Show("Запись была отменена");
            }
        }

        private void F_Grid_Storage_DeleteClick(object sender, RoutedEventArgs e)
        {
            if (Title_SelectTMC == null)
            {
                return;
            }

            try
            {
                UsAc.ExecuteNonQuery($@"DELETE FROM TMC Where ID_TMC = {Title_SelectTMC}");
            }
            finally
            {
                MessageBox.Show("Запись удалена, обновите таблицу");
                Title_SelectApplication = null;
            }


        }
        private void F_Grid_Storage_FoundClick(object sender, RoutedEventArgs e)
        {
            FoundStorageInList(F_Grid_Storage_Component.Text, F_Grid_Storage_Postavshik.SelectedItem.ToString());
        }

        /// <summary>
        /// Поиск записей в таблице Postavchiki.
        /// </summary>
        private void FoundStorageInList(string Component, string Postavshik)
        {
            string request = null;

            if (!(Postavshik == null && Postavshik == "" || Component == null && Component != ""))
            {
                request += "1=1";

                if (Component != null && Component != "")
                {
                    request += $@" and TMC.Nazvanie_TMC Like ""%{Component}%""";
                }

                if (Postavshik != null && Postavshik != "" && Postavshik != "---")
                {
                    request += $@" and Postavchiki.Nazvanie_Postavchik = ""{Postavshik}""";
                }
            }

            Table.TMC.Where = request;
            Table.TMC.UpdateTable();

            F_ListOfStorage.ItemsSource = Table.TMC.DVTable;
        }

        private void TextBoxOnlyNum_KeyPress(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(Char.IsDigit(e.Text, 0));
        }
    }
}