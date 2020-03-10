using KursProject.Modules;

namespace CourseProject.Modules
{
    /// <summary>
    /// Хранит все таблицы для проекта UZNRKT
    /// </summary>
    class Tables
    {
        /// <summary>
        /// Упрощенное взаимодействие с Access
        /// </summary>
        private UsingAccess UsAc;

        /// <summary>
        /// Хранит все таблицы для проекта UZNRKT
        /// </summary>
        /// <param name="UsAc">Для обновления содержимого таблиц</param>
        public Tables(UsingAccess UsAc)
        {
            this.UsAc = UsAc;
            Zayavki = new UsingDataView(UsAc, "Zayavki.ID_Zayavki AS [ID заявки], Zayavki.Date_Zayavki AS [Дата заявки], Sotrudniki.FIO_Master AS [ФИО мастера], Clients.FIO_Client AS [ФИО клиента], Neispravnosti.Naimenovanie AS [Тип неисправности], TypeTehniki.Type_TypeTehniki AS [Тип техники], Izgotovitel.Nazvanie_Izgotovitel AS Изготовитель, Oborudovanie.Model AS Модель, Services.Nazvanie_Services AS [Название услуги], Zayavki.KolvoServ AS [Кол-во деталей], TMC.Nazvanie_TMC AS Деталь, Statys.Statys_Statys AS [Статус выполнения], Zayavki.Date_okonchaniya_Zayavki AS [Дата окончания], Zayavki.Summa AS Сумма", "Neispravnosti INNER JOIN (TypeTehniki INNER JOIN (TMC INNER JOIN (Statys INNER JOIN (Services INNER JOIN (Oborudovanie INNER JOIN (Sotrudniki INNER JOIN (Izgotovitel INNER JOIN (Clients INNER JOIN Zayavki ON Clients.ID_Client = Zayavki.Client) ON Izgotovitel.ID_Izgotovitel = Zayavki.Izgotovitel) ON Sotrudniki.ID_Master = Zayavki.Master) ON Oborudovanie.ID_Oborudovaniya = Zayavki.ID_Oborudovaniya) ON Services.ID_Services = Zayavki.Services) ON Statys.ID_Statys = Zayavki.Statys) ON TMC.ID_TMC = Zayavki.Materiali) ON TypeTehniki.ID_TypeTehniki = Zayavki.Type_Tehniki_Zayavki) ON Neispravnosti.ID_Neispravnosti = Zayavki.Neispravnost_Zayavki", null, null);
        }

        /// <summary>
        /// Заявки
        /// </summary>
        public UsingDataView Zayavki;
    }
}
