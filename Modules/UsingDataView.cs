using System.Data;

namespace UZNRKT.Modules
{
    /// <summary>
    /// Упрощенное взаимодейстеие с таблицой
    /// </summary>
    public class UsingDataView
    {
        /// <summary>
        /// Упрощенное взаимодейстевие с Access
        /// </summary>
        private UsingAccess UsAc;

        /// <summary>
        /// Таблица
        /// </summary>
        public DataView DVTable { get; private set; }

        /// <summary>
        /// Поля для отображения
        /// </summary>
        public string Select { get; set; }

        /// <summary>
        /// Таблица из которой берутся поля
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Условие отбора
        /// </summary>
        public string Where { get; set; }

        /// <summary>
        /// Сортировка по одному полю (Если задается одинаковое поле, то добавляется DESC)
        /// </summary>
        public string OrderBy
        {
            get
            {
                return orderBy;
            }
            set
            {
                if (value == null)
                {
                    orderBy = null;
                }
                else if (orderBy == value)
                {
                    orderBy = value + " DESC";
                }
                else
                {
                    orderBy = value;
                }
            }
        }
        private string orderBy = null;

        /// <summary>
        /// Формируемый запрос для таблицы
        /// </summary>
        public string SQLRequest { get; private set; }

        /// <summary>
        /// Обновление данных в Таблице
        /// </summary>
        public void UpdateTable()
        {
            SQLRequest = null;

            if (Select != null)
            {
                SQLRequest += $"SELECT {Select} ";
            }
            if (From != null)
            {
                SQLRequest += $"FROM {From} ";
            }
            if (Where != null)
            {
                SQLRequest += $"WHERE {Where} ";
            }
            if (orderBy != null)
            {
                SQLRequest += $"ORDER BY {orderBy}";
            }

            DVTable = UsAc.Execute(SQLRequest);
        }

        /// <summary>
        /// Удаление записи в таблице
        /// </summary>
        /// <param name="Where">Условие отбора</param>
        public void DeleteFrom(string Where)
        {
            UsAc.ExecuteNonQuery($"DELETE FROM {From} WHERE {Where}");
        }

        /// <summary>
        /// Добавление записи таблицы
        /// </summary>
        /// <param name="Column">Название поля</param>
        /// <param name="Values">Значение для поля</param>
        public void InsertInto(string Column, string Values)
        {
            UsAc.ExecuteNonQuery($"INSERT INTO {From} ({Column}) VALUES ({Values})");
        }

        /// <summary>
        /// Измение записи
        /// </summary>
        /// <param name="set">Нужное поле и значение</param>
        /// <param name="where">Условие отбора</param>
        public void Update(string set, string where)
        {
            UsAc.ExecuteNonQuery($"UPDATE {From} SET {set} WHERE {where}");
        }
            
        /// <summary>
        /// Упрощенное взаимодейстеие с таблицой
        /// </summary>
        /// <param name="UsAc">Для обновления содержимого таблицы</param>
        /// <param name="Select">Поля</param>
        /// <param name="From">Таблицы</param>
        /// <param name="Where">Условия</param>
        /// <param name="OrderBy">Сортировка</param>
        public UsingDataView(UsingAccess UsAc, string Select, string From, string Where, string OrderBy)
        {
            this.UsAc = UsAc;
            this.Select = Select;
            this.From = From;
            this.Where = Where;
            this.OrderBy = OrderBy;

            UpdateTable();
        }
    }
}
