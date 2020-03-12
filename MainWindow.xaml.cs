using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
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
using Microsoft.Win32;
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

            CreateConnection();

            //Закрытия окна загрузки
            loadingStatus.Close();

            if (AutorizationUser())
            {
                //Объявление таблиц
                Table = new Tables(UsAc);

                //Если пользователь был подключен
                FoundApplicationInList(null, null, null, null);
                LoadClientAndStatus();
                LoadMenu();

                //Выставление главной страницы
                F_Grid_Applications.Visibility = Visibility.Visible;
                F_Grid_Storage.Visibility = Visibility.Hidden;
                F_Grid_User.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Создание подключения
        /// </summary>
        /// <returns>Успех подключения</returns>
        private void CreateConnection()
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
                UsAc = new UsingAccess(BDWay, null, null, null)
                {
                    AutoOpen = true
                };
            }
            catch
            {
                try
                {
                    UsAc = new UsingAccess(BDWay, null, null, "install")
                    {
                        AutoOpen = true
                    };
                }
                catch
                {
                    MessageBox.Show("Не удалось подключится к базе данных, пожалуйста, обратитесь к администратору");
                    this.Close();
                    return;
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

                AddLogToAutorization(1);
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
                    F_Grid_Applications.Visibility = Visibility.Visible;
                    F_Grid_Storage.Visibility = Visibility.Hidden;
                    F_Grid_User.Visibility = Visibility.Hidden;
                    break;
                case "Склад":
                    F_Grid_Applications.Visibility = Visibility.Hidden;
                    F_Grid_Storage.Visibility = Visibility.Visible;
                    F_Grid_User.Visibility = Visibility.Hidden;
                    break;
                case "Пользователи":
                    //Обновление данных в таблице
                    Table.AutorizationTime.UpdateTable();
                    F_TimeAutorization.ItemsSource = Table.AutorizationTime.DVTable;

                    F_Grid_Applications.Visibility = Visibility.Hidden;
                    F_Grid_Storage.Visibility = Visibility.Hidden;
                    F_Grid_User.Visibility = Visibility.Visible;
                    break;
                case "Печать":
                    Table.Zayavki.UpdateTable();
                    OutToExcell("Список заявок", Table.Zayavki.DVTable);
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

            F_Grid_Applications_Client.SelectedIndex = 0;
            F_Grid_Applications_Status.SelectedIndex = 0;
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


        /// <summary>
        /// Событие нажатия кнопки добавления записи
        /// </summary>
        private void F_Grid_Applications_AddClick(object sender, RoutedEventArgs e)
        {
            Windows.AddApplication AddApp = new Windows.AddApplication(UsAc, null);

            //Получение результата
            if (AddApp.ShowDialog() == true)
            {

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
                Table.Zayavki.DeleteFrom($@"ID_Zayavki = {Title_SelectApplication}");
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
        /// Вывод таблицы в эксель
        /// </summary>
        private void OutToExcell(string title, DataView table)
        {
            var excelapp = new Microsoft.Office.Interop.Excel.Application();
            var workbook = excelapp.Workbooks.Add();
            Microsoft.Office.Interop.Excel.Worksheet worksheet = workbook.ActiveSheet;

            //Получение названий колонок
            var ColumnName = new List<string>();
            for (int i = 0; i < table.Table.Columns.Count; i++)
            {
                ColumnName.Add(table.Table.Columns[i].ToString());
            }

            //Выводим название колонок
            for (int x = 0; x < ColumnName.Count; x++)
            {
                worksheet.Rows[2].Columns[x + 1] = ColumnName[x];
            }

            //заполням ячейки
            for (int y = 3; y < table.Count + 3; y++)
            {
                for (int x = 0; x < ColumnName.Count; x++)
                {
                    worksheet.Rows[y].Columns[x + 1] = table.Table.Rows[y - 3][ColumnName[x]];
                }
            }

            // (Титульник над содержимым) Выделяем диапазон ячеек от A1 до числа столбцов из DataView       
            Microsoft.Office.Interop.Excel.Range TitleRange = (Microsoft.Office.Interop.Excel.Range)worksheet.get_Range((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, 1], (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, ColumnName.Count]).Cells;

            // Производим объединение
            TitleRange.Merge(Type.Missing);
            
            //Размер текста
            TitleRange.Cells.Font.Size = 16;
            
            //Выравнивание по центру
            TitleRange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            
            //Задание bold для текста
            TitleRange.Font.Bold = true;

            //Задаем название титульника
            worksheet.Cells[1, 1] = title;

            //Выделение всех ячеек с данными
            Microsoft.Office.Interop.Excel.Range ContentRange = (Microsoft.Office.Interop.Excel.Range)worksheet.get_Range((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, 1], (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[table.Count + 2, ColumnName.Count]).Cells;

            //Выставление линий 
            ContentRange.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            ContentRange.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            ContentRange.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideHorizontal).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            ContentRange.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideVertical).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            ContentRange.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;

            //Выставление автоширины
            ContentRange.EntireColumn.AutoFit();

            //Отображаем Excel
            excelapp.AlertBeforeOverwriting = false;
            excelapp.Visible = true;
        }
    }
}
