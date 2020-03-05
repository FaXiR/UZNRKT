using System.Windows;
using System.Windows.Input;
using KursProject.Modules;

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
            InitializeComponent();
            UsAc = usingAccess;
            F_Login.Focus();
        }
        /// <summary>
        /// Событие нажатия кнопки войти
        /// </summary>
        private void Enter(object sender, RoutedEventArgs e)
        {
            attemptEnter();
        }
        /// <summary>
        /// Проверка полей на пустоту и попытка авторизации
        /// </summary>
        private void attemptEnter()
        {
            if (F_Login.Text == "" && F_Password.Password == "")
            {
                MessageBox.Show("Введите пользователя и пароль");
                return;
            }
            else if (F_Login.Text == "")
            {
                MessageBox.Show("Введите пользователя");
                return;
            }
            else if (F_Password.Password == "")
            {
                MessageBox.Show("Введите пароль");
                return;
            }

            if (checkLogPas())
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
        private bool checkLogPas()
        {
            //Проверка записи в БД
            var FoundRole = UsAc.Execute($@"Select Role, ID_User From Users where Login_User = ""{F_Login.Text}"" and Password_User = ""{F_Password.Password}""");
            if (FoundRole.Count == 0)
            {
                return false;
            }
            else
            {
                Role = FoundRole.Table.Rows[0]["Role"].ToString();
                ID = FoundRole.Table.Rows[0]["ID_User"].ToString();
                return true;
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
                attemptEnter();
            }
        }
    }
}
