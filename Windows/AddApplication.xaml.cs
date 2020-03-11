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
                F_DataZayavki.Text = DateTime.Now.ToString();
                F_Button_AddEdit.Content = "Добавить";
                Title = "Создание заявки";
            }
        }

        private void ButtonClick_add(object sender, RoutedEventArgs e)
        {
            if (F_Button_AddEdit.Content.ToString() == "Добавить")
            {
                AddNewApplication();
            }
            else
            {
                SaveApplication();
            }

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

            //Date_Zayavki, 
            //Master,
            //Client, 
            //Neispravnost_Zayavki, 
            //Type_Tehniki_Zayavki,
            //Izgotovitel,
            //ID_Oborudovaniya,
            //Services,
            //KolvoServ,
            //Materiali,
            //Statys, 
            //Date_okonchaniya_Zayavki,

            string From = null;
            string Values = null;

            if (($@"""{F_DataZayavki.Text}"",").Replace(",","").Length == 0)
            {
                return;
            }

            if ((GetId("Sotrudniki", "ID_Master", F_FIO_Master.SelectedItem.ToString(), "FIO_Master") + ",").Replace(",", "").Length == 0)
            {
                return;
            }

            return;

            //request += 
            //request += GetId("Sotrudniki", "ID_Master", F_FIO_Master.SelectedItem.ToString(), "FIO_Master") + ",";
            //request += GetId("Clients", "ID_Client", F_FIO_Client.SelectedItem.ToString(), "FIO_Client") + ",";
            //request += GetId("Neispravnosti", "ID_Neispravnosti", F_Type_neispravnosti.SelectedItem.ToString(), "Naimenovanie") + ",";
            //request += GetId("TypeTehniki", "ID_TypeTehniki", F_Type_tehniki.SelectedItem.ToString(), "Type_TypeTehniki") + ",";
            //request += GetId("Izgotovitel", "ID_Izgotovitel", F_Producer.SelectedItem.ToString(), "Nazvanie_Izgotovitel") + ",";
            //request += GetId("Oborudovanie", "ID_Oborudovaniya", F_Model.SelectedItem.ToString(), "Model") + ",";
            //request += GetId("Services", "ID_Services", F_NameUsligu.SelectedItem.ToString(), "Nazvanie_Services") + ",";
            //request += Convert.ToInt32(("0" + F_DetalCount.Text)).ToString() + ",";
            //request += GetId("TMC", "ID_TMC", F_DetalName.SelectedItem.ToString(), "Nazvanie_TMC") + ",";
            //request += GetId("Statys", "ID_Statys", F_Statys.SelectedItem.ToString(), "Statys_Statys") + ",";
            //request += $@"""{F_DataFinished.Text}"",";
            //request += $@"""{F_Summ.Text}"")";

       //     UsAc.ExecuteNonQuery($@"INSERT INTO Zayavki ({From}) VALUES ({Values})";
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

