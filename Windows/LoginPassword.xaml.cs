using System.Windows;
using System.Windows.Input;
using UZNRKT.Modules;

namespace UZNRKT.Windows
{
    /// <summary>
    /// Логика взаимодействия для LoginPassword.xaml
    /// </summary>
    public partial class LoginPassword : Window
    {
        #region переменные
        public string Login
        {
            get { return F_Login.Text.ToString(); }
            set { F_Login.Text = value; }
        }

        public string Password
        {
            get { return F_Password.Password.ToString(); }
            set { F_Password.Password = value; }
        }

        public string Role = null;

        public string ID = null;

        private UsingAccess UsAc;
        #endregion

        /// <summary>
        /// Окно авторизации
        /// </summary>
        /// <param name="usingAccess">Соеденение с Access для проверки Логина/Пароля</param>
        public LoginPassword(UsingAccess usingAccess)
        {
            System.Console.WriteLine("Запуск окна авторизации");
            InitializeComponent();
            UsAc = usingAccess;
            F_Login.Focus();
        }
        /// <summary>
        /// Событие нажатия кнопки войти
        /// </summary>
        private void Enter(object sender, RoutedEventArgs e)
        {
            AttemptEnter();
        }
        /// <summary>
        /// Проверка полей на пустоту и попытка авторизации
        /// </summary>
        private void AttemptEnter()
        {
            if (F_Login.Text.Length > 4)
            {
                CheckNewUser(F_Login.Text);
            }
            

            if (F_Login.Text.Length < 4 && F_Password.Password.Length < 4)
            {
                MessageBox.Show("Введите пользователя и пароль");
                return;
            }
            else if (F_Login.Text.Length < 4)
            {
                MessageBox.Show("Введите пользователя");
                return;
            }
            else if (F_Password.Password.Length < 4)
            {
                MessageBox.Show("Введите пароль");
                return;
            }

            if (CheckLogPas())
            {
                this.DialogResult = true;
            }
            else
            {
                F_Password.Clear();
                F_Login.Focus();
                F_Login.SelectAll();
            }
        }
        /// <summary>
        /// Поиск логина/пароля в БД
        /// </summary>
        /// <returns>Наличие записи</returns>
        private bool CheckLogPas()
        {
            //Проверка записи в БД
            var FoundRole = UsAc.Execute($@"Select Role, ID_User From Users where Login_User = ""{F_Login.Text}"" and Password_User = ""{F_Password.Password}""");
            if (FoundRole.Count == 0)
            {
                return false;
            }
            else
            {
                System.Console.WriteLine("Запись найдена");
                Role = FoundRole.Table.Rows[0]["Role"].ToString();
                ID = FoundRole.Table.Rows[0]["ID_User"].ToString();
                return true;
            }
        }

        private bool CheckNewUser(string User)
        {
            //Проверка записи в БД
            var FoundUser = UsAc.Execute($@"Select Password_User From Users where Login_User = ""{F_Login.Text}""");

            //Определение числа записей
            if (FoundUser.Table.Rows.Count == 0)
            {
                return false;
            }
            else
            {
                if (FoundUser.Table.Rows[0]["Password_User"].ToString() == "")
                {
                    //Окно создания пароля
                    new Windows.CreatePassword(User, UsAc).ShowDialog();

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Событие нажатии кнопки в TextBox Логина
        /// </summary>
        private void KeyUp_Login(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                F_Password.Focus();
                F_Password.SelectAll();
            }
        }
        /// <summary>
        /// Событие нажатии кнопки в PasswordBox пароля
        /// </summary>
        private void KeyUp_Password(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AttemptEnter();
            }
        }
    }
}
