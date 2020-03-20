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
using System.Windows.Shapes;
using UZNRKT.Modules;

namespace UZNRKT.Windows
{
    /// <summary>
    /// Логика взаимодействия для CreatePassword.xaml
    /// </summary>
    public partial class CreatePassword : Window
    {
        UsingAccess UsAc;
        string login;

        public CreatePassword(string User, UsingAccess UsAc)
        {
            this.UsAc = UsAc;
            this.login = User;

            InitializeComponent();
        }

        private void Enter(object sender, RoutedEventArgs e)
        {
            //Проверка длины 
            if (F_Pas1.Password.Length < 4)
            {
                MessageBox.Show("Пароль должен содежать не менее 4 символов");
                return;
            }

            if (F_Pas1.Password != F_Pas2.Password)
            {
                MessageBox.Show("Пароли не совпадают");
                return;
            }

            UsAc.ExecuteNonQuery($@"UPDATE Users SET Password_User = ""{F_Pas1.Password}"" WHERE Login_User = ""{login}""");

            MessageBox.Show("Пароль был создан");
            this.DialogResult = true;
        }
    }
}
