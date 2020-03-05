using System.Data;
using System.Data.OleDb;

namespace KursProject.Modules
{
    /// <summary>
    /// Упрощенное взаимодействие с БД Access
    /// </summary>
    public class UsingAccess
    {
        #region простой участок кода
        /// <summary>
        /// Провайдер доступа. Например: Microsoft.Jet.OLEDB.4.0 / Microsoft.ACE.OLEDB.12.0
        /// </summary>
        public string Provider { get; set; } = null;

        /// <summary>
        /// Путь к базе данных
        /// </summary>
        public string DataSource { get; set; } = null;

        /// <summary>
        /// Пароль для базы данных
        /// </summary>
        public string Password { get; set; } = null;

        /// <summary>
        /// Логин от базы данных
        /// </summary>
        public string Login { get; set; } = null;

        /// <summary>
        /// Смешивает Provider, DataSource и Password если в них есть данные
        /// </summary>
        public string ConnectString
        {
            get
            {
                string result = null;
                if (this.Provider != null)
                {
                    result += $"Provider={this.Provider};";
                }
                if (this.DataSource != null)
                {
                    result += $"Data Source={this.DataSource};";
                }
                if (this.Login != null)
                {
                    result += $"User ID={this.Login};";
                }
                if (this.Password != null)
                {
                    result += $"Jet OLEDB:Database Password={this.Password};";
                }

                return result;
            }
        }

        /// <summary>
        /// Автоматическое подключение/отключение для SQL запросов
        /// </summary>
        public bool AutoOpen = false;

        /// <summary>
        /// Ссылка на экземпляр класса OleDbConnection для соединения с БД
        /// </summary>
        private OleDbConnection myConnection;

        /// <summary>
        /// Открывает соединение с БД
        /// </summary>
        public void ConnectOpen()
        {
            myConnection.Open();
        }

        /// <summary>
        /// Закрывает соединение с БД
        /// </summary>
        public void ConnectClose()
        {
            myConnection.Close();
        }

        /// <summary>
        /// Проверяет БД путем открытия и закрытия БД (Если ошибка, то выдает исключение)
        /// </summary>
        public void ConnectChech()
        {
            myConnection.Open();
            myConnection.Close();
        }
        #endregion

        /// <summary>
        /// Упрощенное взаимодействие с БД Access. Для ненужных полей используйте null
        /// </summary>
        /// <param name="DataSource">Путь к БД</param>
        /// <param name="Provider">Провайдер доступа. Например: Microsoft.Jet.OLEDB.4.0 / Microsoft.ACE.OLEDB.12.0</param>
        /// <param name="Login">Логин от БД</param>
        /// <param name="Password">Пароль от БД</param>
        public UsingAccess(string DataSource, string Provider, string Login, string Password)
        {
            this.DataSource = DataSource;
            this.Password = Password;
            this.Login = Login;
            if (Provider == null)
            {
                try
                {
                    this.Provider = "Microsoft.Jet.OLEDB.4.0";
                    myConnection = new OleDbConnection(this.ConnectString);
                    ConnectChech();
                }
                catch (System.Data.OleDb.OleDbException)
                {
                    this.Provider = "Microsoft.ACE.OLEDB.12.0";
                    myConnection = new OleDbConnection(this.ConnectString);
                    ConnectChech();
                }
            }
            else
            {
                this.Provider = Provider;
                myConnection = new OleDbConnection(this.ConnectString);
                ConnectChech();
            }            
        }

        /// <summary>
        /// Выполняет SQL запрос с возвратом данных 
        /// </summary>
        /// <param name="SQLRequest">SQL запрос</param>
        /// <returns>Таблицу (DataView)</returns>
        public DataView Execute(string SQLRequest)
        {
            if (AutoOpen)
                ConnectOpen();

            OleDbCommand command = new OleDbCommand
            {
                Connection = myConnection,
                CommandText = SQLRequest
            };

            OleDbDataAdapter da = new OleDbDataAdapter(command);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (AutoOpen)
                ConnectClose();

            return dt.DefaultView;
        }

        /// <summary>
        /// Выполняет SQL запрос без возврата данных
        /// </summary>
        /// <param name="SQLRequest">SQL запрос</param>
        public void ExecuteNonQuery(string SQLRequest)
        {
            OleDbCommand command = new OleDbCommand(SQLRequest, myConnection);

            if (AutoOpen)
                ConnectOpen();

            command.ExecuteNonQuery();

            if (AutoOpen)
                ConnectClose();
        }
    }
}
