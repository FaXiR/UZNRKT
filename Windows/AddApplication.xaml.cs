using CourseProject.Modules;
using KursProject.Modules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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

namespace UZNRKT.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddApplication.xaml
    /// </summary>
    public partial class AddApplication : Window
    {
        /// <summary>
        /// Упрощенное взаимодействие с БД Access
        /// </summary>
        private UsingAccess UsAc;

        /// <summary>
        /// ID заявки
        /// </summary>
        private string ID = null;

        public AddApplication(UsingAccess UsAc, string IdZayavki)
        {
            InitializeComponent();

            this.UsAc = UsAc;

            LoadAllComboBox();

            if (IdZayavki != null)
            {
                this.ID = IdZayavki;
                SetValue(IdZayavki);
                F_Button_AddEdit.Content = "Сохранить";
                Title = "Редактирования заявки №" + IdZayavki;
            }
            else
            {
                F_DataZayavki.Text = DateTime.Now.ToShortDateString();
                F_Button_AddEdit.Content = "Добавить";
                Title = "Создание заявки";
            }
        }

        private void ButtonClick_add(object sender, RoutedEventArgs e)
        {
            bool AutoOpenInUsAc = UsAc.AutoOpen;

            UsAc.AutoOpen = false;
            UsAc.ConnectOpen();
            
            if (F_Button_AddEdit.Content.ToString() == "Добавить")
            {
                AddNewApplication();
            }
            else
            {
                SaveApplication();
            }

            UsAc.ConnectClose();
            UsAc.AutoOpen = AutoOpenInUsAc;

            this.DialogResult = true;
        }
        private void ButtonClick_cancel(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        /// <summary>
        /// Подружает все значения в поля
        /// </summary>
        private void LoadAllComboBox()
        {
            LoadComboBox("FIO_Master", " Sotrudniki", F_FIO_Master);
            LoadComboBox("FIO_Client", "Clients", F_FIO_Client);
            LoadComboBox("Naimenovanie", "Neispravnosti", F_Type_neispravnosti);
            LoadComboBox("Type_TypeTehniki", "TypeTehniki", F_Type_tehniki);
            LoadComboBox("Nazvanie_Izgotovitel", "Izgotovitel", F_Producer);
            LoadComboBox("Model", "Oborudovanie", F_Model);
            LoadComboBox("Nazvanie_Services", "Services", F_NameUsligu);
            LoadComboBox("Nazvanie_TMC", "TMC", F_DetalName);
            LoadComboBox("Statys_Statys", "Statys", F_Statys);
        }

        /// <summary>
        /// Подругражет значение в ComboBox
        /// </summary>
        /// <param name="select">Колонка</param>
        /// <param name="from">Таблица</param>
        /// <param name="boxName">Имя combobox'а</param>
        private void LoadComboBox(string select, string from, ComboBox boxName)
        {
            ObservableCollection<string> list = new ObservableCollection<string>();
            list.Add("---");

            var tab = UsAc.Execute($@"SELECT {select} FROM {from}");
            for (int i = 0; i < tab.Count; i++)
            {
                list.Add(tab.Table.Rows[i][select].ToString());
            }

            boxName.ItemsSource = list;
            boxName.SelectedIndex = 0;
        }

        /// <summary>
        /// Задает значение для ComboBox
        /// </summary>
        /// <param name="value">Искомое значение</param>
        /// <param name="boxName">ComboBox</param>
        private void SetValueToComboBox(string value, ComboBox boxName)
        {
            //Получаем лист из Combobox
            var ComboboxList = (ObservableCollection<string>)boxName.ItemsSource;

            //Перебираем значение Combobox
            for (int i = 0; i < ComboboxList.Count; i++)
            {
                if (value == ComboboxList[i])
                {
                    boxName.SelectedIndex = i;
                    return;
                }
            }

            boxName.SelectedIndex = 0;
        }

        /// <summary>
        /// Выставление значений Combobox'ам
        /// </summary>
        /// <param name="IDZayavki">ID заявки</param>
        private void SetValue(string IDZayavki)
        {
            var ZayavkiTable = new Tables(UsAc);
            ZayavkiTable.Zayavki.Where = $@"ID_Zayavki = {IDZayavki}";
            ZayavkiTable.Zayavki.UpdateTable();

            var table = ZayavkiTable.Zayavki.DVTable.Table;
            SetValueToComboBox(table.Rows[0]["ФИО мастера"].ToString(), F_FIO_Master);
            SetValueToComboBox(table.Rows[0]["ФИО клиента"].ToString(), F_FIO_Client);
            SetValueToComboBox(table.Rows[0]["Тип неисправности"].ToString(), F_Type_neispravnosti);
            SetValueToComboBox(table.Rows[0]["Тип техники"].ToString(), F_Type_tehniki);
            SetValueToComboBox(table.Rows[0]["Изготовитель"].ToString(), F_Producer);
            SetValueToComboBox(table.Rows[0]["Модель"].ToString(), F_Model);
            SetValueToComboBox(table.Rows[0]["Название услуги"].ToString(), F_NameUsligu);
            SetValueToComboBox(table.Rows[0]["Деталь"].ToString(), F_DetalName);
            SetValueToComboBox(table.Rows[0]["Статус выполнения"].ToString(), F_Statys);

            F_DataZayavki.Text = table.Rows[0]["Дата заявки"].ToString();
            F_DetalCount.Text = table.Rows[0]["Кол-во деталей"].ToString();
            F_DataFinished.Text = table.Rows[0]["Дата окончания"].ToString();
            F_Summ.Text = table.Rows[0]["Сумма"].ToString();
        }

        /// <summary>
        /// Метод добавления новой записи
        /// </summary>
        private void AddNewApplication()
        {
            string value = null;

            string From = null;
            string Values = null;

            value = F_DataZayavki.Text;
            if (value.Length != 0)
            {
                From += "Date_Zayavki";
                Values += $@"""{F_DataZayavki.Text}"""; 
            }

            value = F_FIO_Master.Text;
            if (value != "---")
            {
                From += ",Master";
                Values += "," + GetId("Sotrudniki", "ID_Master", F_FIO_Master.SelectedItem.ToString(), "FIO_Master");
            }

            value = F_FIO_Client.Text;
            if (value != "---")
            {
                From += ",Client";
                Values += "," + GetId("Clients", "ID_Client", F_FIO_Client.SelectedItem.ToString(), "FIO_Client");
            }

            value = F_Type_neispravnosti.Text;
            if (value != "---")
            {
                From += ",Neispravnost_Zayavki";
                Values += "," + GetId("Neispravnosti", "ID_Neispravnosti", F_Type_neispravnosti.SelectedItem.ToString(), "Naimenovanie");
            }

            value = F_Type_tehniki.Text;
            if (value != "---")
            {
                From += ",Type_Tehniki_Zayavki";
                Values += "," + GetId("TypeTehniki", "ID_TypeTehniki", F_Type_tehniki.SelectedItem.ToString(), "Type_TypeTehniki");

            }

            value = F_Producer.Text;
            if (value != "---")
            {
                From += ",Izgotovitel";
                Values += "," + GetId("Izgotovitel", "ID_Izgotovitel", F_Producer.SelectedItem.ToString(), "Nazvanie_Izgotovitel");
                ;
            }

            value = F_Model.Text;
            if (value != "---")
            {
                From += ",ID_Oborudovaniya";
                Values += "," + GetId("Oborudovanie", "ID_Oborudovaniya", F_Model.SelectedItem.ToString(), "Model");

            }

            value = F_NameUsligu.Text;
            if (value != "---")
            {
                From += ",Services";
                Values += "," + GetId("Services", "ID_Services", F_NameUsligu.SelectedItem.ToString(), "Nazvanie_Services");

            }

            value = F_DetalCount.Text;
            if (value.Length != 0)
            {
                From += ",KolvoServ";
                Values += "," + F_DetalCount.Text;
            }

            value = F_DetalName.Text;
            if (value != "---")
            {
                From += ",Materiali";
                Values += "," + GetId("TMC", "ID_TMC", F_DetalName.SelectedItem.ToString(), "Nazvanie_TMC");

            }

            value = F_Statys.Text;
            if (value != "---")
            {
                From += ",Statys";
                Values += "," + GetId("Statys", "ID_Statys", F_Statys.SelectedItem.ToString(), "Statys_Statys");
            }

            value = F_DataFinished.Text;
            if (value.Length != 0)
            {
                From += ",Date_okonchaniya_Zayavki";
                Values += "," + $@"""{F_DataFinished.Text}""";
            }

            value = F_Summ.Text;
            if (value.Length != 0)
            {
                From += ",Summa";
                Values += "," + $@"""{F_Summ.Text}""";
            }

            UsAc.ExecuteNonQuery($@"INSERT INTO Zayavki ({From}) VALUES ({Values})");
        }

        /// <summary>
        /// Метод добавления изменения записи
        /// </summary>
        private void SaveApplication()
        {

        }

        private string GetId(string table, string idInColumn, string foundWord, string FoundInColumn)
        {
            try
            {
                return UsAc.Execute($@"SELECT {idInColumn} FROM {table} WHERE {FoundInColumn} = ""{foundWord}""").Table.Rows[0][idInColumn].ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}

