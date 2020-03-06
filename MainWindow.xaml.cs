using System;
using System.Collections.Generic;
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
        }

        /// <summary>
        /// Создание подключения
        /// </summary>
        /// <returns>Успех подключения</returns>
        private bool CreateConnection()
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
            //
            //..
            //
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
    }
}
