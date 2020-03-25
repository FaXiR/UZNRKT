using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using UZNRKT.Modules;

namespace UZNRKT.Windows
{
    /// <summary>
    /// Логика взаимодействия для Report.xaml
    /// </summary>
    public partial class Report : System.Windows.Window
    {
        string Application = null;
        UsingAccess UsAc = null;
        DataView table = null;

        public Report(UsingAccess UsAc, DataView table, string Application)
        {
            InitializeComponent();
            this.UsAc = UsAc;

            this.table = table;
            if (table == null)
            {
                F_ApplicationCount.Text = "Число записей: 0";
                F_ButtonApplicationCount.IsEnabled = false;
            }
            else
            {
                F_ApplicationCount.Text = $"Число записей: {table.Count.ToString()}";
            }

            this.Application = Application;
            if (Application == null)
            {
                F_ApplicationID.Text = "Заявка не выбрана";
                F_ButtonApplication.IsEnabled = false;
            }
            else
            {
                F_ApplicationID.Text = $"Заявка №: {Application}";
            }
        }

        private void F_ButtonApplicationCount_Click(object sender, RoutedEventArgs e)
        {
            OutToExcell("Список заявок", table);
            this.DialogResult = true;
        }

        /// <summary>
        /// Вывод таблицы в эксель
        /// </summary>
        private void OutToExcell(string title, DataView table)
        {
            var excelapp = new Microsoft.Office.Interop.Excel.Application();
            var workbook = excelapp.Workbooks.Add();
            Microsoft.Office.Interop.Excel.Worksheet worksheet = workbook.ActiveSheet;

            //Получение названий колонок
            var ColumnName = new List<string>();
            for (int i = 0; i < table.Table.Columns.Count; i++)
            {
                ColumnName.Add(table.Table.Columns[i].ToString());
            }

            //Выводим название колонок
            for (int x = 0; x < ColumnName.Count; x++)
            {
                worksheet.Rows[2].Columns[x + 1] = ColumnName[x];
            }

            //заполням ячейки
            for (int y = 3; y < table.Count + 3; y++)
            {
                for (int x = 0; x < ColumnName.Count; x++)
                {
                    worksheet.Rows[y].Columns[x + 1] = table.Table.Rows[y - 3][ColumnName[x]];
                }
            }

            // (Титульник над содержимым) Выделяем диапазон ячеек от A1 до числа столбцов из DataView       
            Microsoft.Office.Interop.Excel.Range TitleRange = (Microsoft.Office.Interop.Excel.Range)worksheet.get_Range((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, 1], (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, ColumnName.Count]).Cells;

            // Производим объединение
            TitleRange.Merge(Type.Missing);

            //Размер текста
            TitleRange.Cells.Font.Size = 16;

            //Выравнивание по центру
            TitleRange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

            //Задание bold для текста
            TitleRange.Font.Bold = true;

            //Задаем название титульника
            worksheet.Cells[1, 1] = title;

            //Выделение всех ячеек с данными
            Microsoft.Office.Interop.Excel.Range ContentRange = (Microsoft.Office.Interop.Excel.Range)worksheet.get_Range((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, 1], (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[table.Count + 2, ColumnName.Count]).Cells;

            //Выставление линий 
            ContentRange.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            ContentRange.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            ContentRange.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideHorizontal).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            ContentRange.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideVertical).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            ContentRange.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;

            //Выставление автоширины
            ContentRange.EntireColumn.AutoFit();

            //Отображаем Excel
            excelapp.AlertBeforeOverwriting = false;
            excelapp.Visible = true;
        }

        private void F_ButtonApplication_Click(object sender, RoutedEventArgs e)
        {
            OutToWord(Application);
            this.DialogResult = true;
        }

        public void OutToWord(string ID)
        {
            //Получение записи из таблицы
            var table = new UsingDataView(UsAc, "Zayavki.ID_Zayavki, Clients.FIO_Client, Clients.Phone_Client, Neispravnosti.Naimenovanie, TypeTehniki.Type_TypeTehniki, Oborudovanie.Model, Services.Nazvanie_Services, Services.Stoimost_Services, TMC.Nazvanie_TMC, TMC.Kolichestvo_TMC", "TMC RIGHT JOIN (Services RIGHT JOIN (Oborudovanie RIGHT JOIN (TypeTehniki RIGHT JOIN (Neispravnosti RIGHT JOIN (Clients RIGHT JOIN Zayavki ON Clients.ID_Client = Zayavki.Client) ON Neispravnosti.ID_Neispravnosti = Zayavki.Neispravnost_Zayavki) ON TypeTehniki.ID_TypeTehniki = Zayavki.Type_Tehniki_Zayavki) ON Oborudovanie.ID_Oborudovaniya = Zayavki.ID_Oborudovaniya) ON Services.ID_Services = Zayavki.Services) ON TMC.ID_TMC = Zayavki.Materiali", $"Zayavki.ID_Zayavki = {ID}", null).DVTable.Table;

            var wordApp = new Microsoft.Office.Interop.Word.Application();
            wordApp.Visible = false;
            object missing = System.Reflection.Missing.Value;
            var wordDocument = wordApp.Documents.Open(Environment.CurrentDirectory + "/model/actofwork.docx");
            try
            {
                ReplaceWordStub("{Today}", DateTime.Now.ToShortDateString(), wordDocument);
                ReplaceWordStub("{Num}", ID, wordDocument);
                ReplaceWordStub("{Client}", table.Rows[0]["FIO_Client"].ToString(), wordDocument);
                ReplaceWordStub("{Neispravnost}", table.Rows[0]["Naimenovanie"].ToString(), wordDocument);
                ReplaceWordStub("{Model}", table.Rows[0]["Model"].ToString(), wordDocument);
                ReplaceWordStub("{Phone}", table.Rows[0]["Phone_Client"].ToString(), wordDocument);
                ReplaceWordStub("{Tehnika}", table.Rows[0]["Type_TypeTehniki"].ToString(), wordDocument);
                ReplaceWordStub("{Repair}", table.Rows[0]["Nazvanie_Services"].ToString(), wordDocument);

                Microsoft.Office.Interop.Word.Table wordTable = wordDocument.Tables[2];
                for (int x = 2; x < table.Rows.Count + 2; x++)
                {
                    //Добавляем строку таблицы
                    object oMissing = System.Reflection.Missing.Value;
                    wordTable.Rows.Add(ref oMissing);

                    //# п/п
                    wordTable.Cell(x, 1).Range.Text = (x - 1).ToString();
                    wordTable.Cell(x, 1).Range.Bold = 0;
                    wordTable.Cell(x, 1).Range.Cells.SetHeight(1, WdRowHeightRule.wdRowHeightAuto);

                    //Наименование
                    wordTable.Cell(x, 2).Range.Text = table.Rows[x - 2]["Nazvanie_TMC"].ToString() + "";
                    wordTable.Cell(x, 2).Range.Bold = 0;

                    //Цена
                    wordTable.Cell(x, 3).Range.Text = table.Rows[x - 2]["Stoimost_Services"].ToString();
                    wordTable.Cell(x, 3).Range.Bold = 0;

                    if (table.Rows[x - 2]["Nazvanie_TMC"].ToString().Length < 2)
                    {
                        ReplaceWordStub("{DocCount}", "0" + " руб.", wordDocument);
                    }
                    else
                    {
                        ReplaceWordStub("{DocCount}", table.Rows[x - 2]["Stoimost_Services"].ToString() + " руб.", wordDocument);
                    }

                    wordTable.Cell(x, 3).Range.Bold = 0;
                    wordTable.Cell(x, 3).Range.Cells.SetHeight(1, WdRowHeightRule.wdRowHeightAuto);
                }
            }
            catch (Exception ex)
            {
                object doNotSaveChanges = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
                wordDocument.Close(ref doNotSaveChanges, ref missing, ref missing);
                throw ex;
            }

            wordApp.Visible = true;
        }

        private void ReplaceWordStub(string stubToReplace, string text, Microsoft.Office.Interop.Word.Document wordDocument)
        {
            var range = wordDocument.Content;
            range.Find.ClearFormatting();
            range.Find.Execute(FindText: stubToReplace, ReplaceWith: text);
        }
    }
}
