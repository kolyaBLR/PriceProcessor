using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace PriceProcessor
{
    class ExcelFile
    {
        private int _GetArraySizeColumn = 0;
        public int GetArraySizeColumn { get { return _GetArraySizeColumn; } }

        private int _GetArraySizeRows = 0;
        public int GetArraySizeRows { get { return _GetArraySizeRows; } }

        private Excel.Workbook ObjWorkBook = null;

        public string[,] OpenExcel(string nameFile)
        {
            try
            {
                Excel.Application ObjWorkExcel = new Excel.Application(); //открыть эксель
                ObjWorkBook = ObjWorkExcel.Workbooks.Open(nameFile, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing); //открыть файл
                Excel.Worksheet ObjWorkSheet = (Excel.Worksheet)ObjWorkBook.Sheets[1]; //получить 1 лист
                var lastCell = ObjWorkSheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell);//1 ячейку
                _GetArraySizeColumn = lastCell.Column;
                _GetArraySizeRows = lastCell.Row;
                string[,] list = new string[lastCell.Column, lastCell.Row]; // массив значений с листа равен по размеру листу
                for (int i = 1; i < lastCell.Column; i++) //по всем колонкам
                    for (int j = 0; j < lastCell.Row; j++) // по всем строкам
                        list[i - 1, j] = ObjWorkSheet.Cells[j + 1, i + 1].Text.ToString();//считываем текст в строку
                ObjWorkBook.Close(false, Type.Missing, Type.Missing);
                ObjWorkExcel.Quit(); // выйти из экселя             
                return list;
            }
            catch
            {
                Marshal.ReleaseComObject(ObjWorkBook);  // завершение процессов эксель
                GC.Collect(); // убрать за собой
                return null;
            }
            finally
            {
                Marshal.ReleaseComObject(ObjWorkBook);  // завершение процессов эксель
                GC.Collect(); // убрать за собой
            }
        }
    }
}
