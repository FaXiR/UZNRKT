using System;
using System.Collections.Generic;
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
        UsingAccess UsAc;
        /// <summary>
        /// Логин пользователя
        /// </summary>
        string UserName = null;
        /// <summary>
        /// ID пользователя
        /// </summary>
        string UserID = null;
        /// <summary>
        /// Роль пользователя
        /// </summary>
        string UserRole = null;

        public MainWindow()
        {
            InitializeComponent();

            //TODO: добавить проверку наличия БД
            UsAc = new UsingAccess("db.accdb", null, null, "install");
            UsAc.AutoOpen = true;

            //Авторизация пользователя
            var window = new Windows.LoginPassword(UsAc);
            if (window.ShowDialog() == true)
            {
                //Запись данных о вошедшем пользователе
                UserRole = window.Role;
                UserName = window.Login;
                UserID = window.ID;
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
    }
}
