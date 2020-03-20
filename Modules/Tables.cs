namespace UZNRKT.Modules
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

            Neispravnosti = new UsingDataView(UsAc, "Neispravnosti.Naimenovanie AS Неисправность", "Neispravnosti", null, null);
            Izgotovitel = new UsingDataView(UsAc, "Izgotovitel.Nazvanie_Izgotovitel AS Название", "Izgotovitel", null, null);
            Services = new UsingDataView(UsAc, "Services.Nazvanie_Services AS Название, Services.Stoimost_Services AS Стоимость", "Services", null, null);
            Statys = new UsingDataView(UsAc, "Statys.Statys_Statys AS Статус", "Statys", null, null);
            TypeTehniki = new UsingDataView(UsAc, " TypeTehniki.Type_TypeTehniki AS Тип", "TypeTehniki", null, null);
            Oborudovanie = new UsingDataView(UsAc, " Oborudovanie.Model AS Модель, Oborudovanie.SerNomer AS[Серийный номер], Oborudovanie.Komplektaciya AS Комплектация, Oborudovanie.Primechaniya AS Примечания", "Oborudovanie", null, null);
            Doljnosti = new UsingDataView(UsAc, " Doljnosti.Nazvanie_Doljnost AS Должность", "Doljnosti", null, null);



            Zayavki = new UsingDataView(
                UsAc
                , "Zayavki.ID_Zayavki AS [ID заявки], Zayavki.Date_Zayavki AS [Дата заявки], Sotrudniki.FIO_Master AS [ФИО мастера], Clients.FIO_Client AS [ФИО клиента], Neispravnosti.Naimenovanie AS [Тип неисправности], TypeTehniki.Type_TypeTehniki AS [Тип техники], Izgotovitel.Nazvanie_Izgotovitel AS Изготовитель, Oborudovanie.Model AS Модель, Services.Nazvanie_Services AS [Название услуги], Zayavki.KolvoServ AS [Кол-во деталей], TMC.Nazvanie_TMC AS Деталь, Statys.Statys_Statys AS [Статус выполнения], Zayavki.Date_okonchaniya_Zayavki AS [Дата окончания], Zayavki.Summa AS Сумма"
                , "TypeTehniki RIGHT JOIN (TMC RIGHT JOIN (Statys RIGHT JOIN (Services RIGHT JOIN (Oborudovanie RIGHT JOIN (Neispravnosti RIGHT JOIN (Sotrudniki RIGHT JOIN (Izgotovitel RIGHT JOIN (Clients RIGHT JOIN Zayavki ON Clients.ID_Client = Zayavki.Client) ON Izgotovitel.ID_Izgotovitel = Zayavki.Izgotovitel) ON Sotrudniki.ID_Master = Zayavki.Master) ON Neispravnosti.ID_Neispravnosti = Zayavki.Neispravnost_Zayavki) ON Oborudovanie.ID_Oborudovaniya = Zayavki.ID_Oborudovaniya) ON Services.ID_Services = Zayavki.Services) ON Statys.ID_Statys = Zayavki.Statys) ON TMC.ID_TMC = Zayavki.Materiali) ON TypeTehniki.ID_TypeTehniki = Zayavki.Type_Tehniki_Zayavki"
                , null
                , null);

            AutorizationTime = new UsingDataView(
                UsAc
                , "Users.Login_User AS Пользователь, Log_avtorizatcii.Time_in AS [Время входа], Log_avtorizatcii.Time_out AS [Время выхода]"
                , "Users Users RIGHT JOIN Log_avtorizatcii ON Users.ID_User = Log_avtorizatcii.ID_User"
                , null
                , "Log_avtorizatcii.Time_in DESC");

            Sotrudniki = new UsingDataView(
                UsAc
                , "Sotrudniki.FIO_Master AS [ФИО Мастера], Doljnosti.Nazvanie_Doljnost AS Должность, Sotrudniki.DateOfBirth_Master AS [Дата рождения], Sotrudniki.Phone_Master AS Телефон, Sotrudniki.Adress_Master AS Адрес, Sotrudniki.Email_Master AS Emali"
                , "Doljnosti RIGHT JOIN Sotrudniki ON Doljnosti.ID_Doljnost = Sotrudniki.Doljnost_Master"
                , null
                , null);

            DogovorOPostavke = new UsingDataView(
                UsAc
                , "Postavchiki.Nazvanie_Postavchik AS Поставщик, DogovorOPostavke.DateZakaza AS [Дата заказа], DogovorOPostavke.NazvanieMateriala AS Материал, DogovorOPostavke.CenaZaEd AS [Цена заказа], DogovorOPostavke.Kolichestvo AS Количество, DogovorOPostavke.DatePostavki AS [Дата поставки], DogovorOPostavke.SummaZakaza AS [Сумма заказа]"
                , "Postavchiki RIGHT JOIN DogovorOPostavke ON Postavchiki.ID_Postavchik = DogovorOPostavke.Postavchik"
                , null
                , null);

            Users = new UsingDataView(
                UsAc
                , "Users.ID_User AS [# ID], Users.Login_User AS Логин, Sotrudniki.FIO_Master AS ФИО, Doljnosti.Nazvanie_Doljnost AS Должность, Roles.Role_Role AS Роль"
                , "(Roles RIGHT JOIN Users ON Roles.ID_Role = Users.Role) LEFT JOIN (Doljnosti RIGHT JOIN Sotrudniki ON Doljnosti.ID_Doljnost = Sotrudniki.Doljnost_Master) ON Users.ID_User = Sotrudniki.IDUsera"
                , null
                , null);
        }

        public UsingDataView Zayavki;
        public UsingDataView AutorizationTime;
        public UsingDataView Sotrudniki;
        public UsingDataView Neispravnosti;
        public UsingDataView Izgotovitel;
        public UsingDataView Services;
        public UsingDataView Statys;
        public UsingDataView TypeTehniki;
        public UsingDataView Oborudovanie;
        public UsingDataView Doljnosti;
        public UsingDataView DogovorOPostavke;
        public UsingDataView Users;
    }
}
