using PriceProcessor;
using Excel = Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace PriceProcessor
{
    public partial class Form1 : Form
    {
        private int indexRowDataGrid2 = 0;  //  переменная счётчик строк в dgv2

        private string connectionString;

        public void openConnection()
        {
            try
            {
                StreamReader sr = new StreamReader(@"connectionString.txt");
                connectionString =  sr.ReadLine();
            }
            catch
            {
                connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=Excel;Integrated Security=True";
            }
        }

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            openConnection();
            ExchangeRates er = new ExchangeRates();
            rates.Text = (double.Parse(er.convert()) / 10000).ToString();
            DataBaseToDataGrid();
            DataBasePriceUSD();
        }

        /// <summary>
        /// корректировка столбцов с бел рублями по актуальному курсу
        /// </summary>
        public void DataBasePriceUSD()
        {
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                string str = dataGridView1.Rows[i].Cells[1].Value.ToString();
                for (int j = 0; j < str.Length; j++)
                {
                    if (!Regex.IsMatch(str[j].ToString(), @"[0-9]") && str[j] != ',')
                    {
                        str = str.Remove(j, 1);
                        j--;
                    }
                }
                dataGridView1.Rows[i].Cells[2].Value = Math.Round(Convert.ToDouble(double.Parse(str) * double.Parse(rates.Text)), 2).ToString() + " BYR";
            }
        }

        /// <summary>
        /// ввод данных в бд
        /// </summary>
        public void DataGridToDataBase()
        {
            try
            {
                SqlConnection con = new SqlConnection(connectionString);
                con.Open();
                for (int i = 0; i < dataGridView2.RowCount; i++)
                {
                    using (SqlCommand com = new SqlCommand("INSERT INTO OutputPrice(name, priceUSD, priceBYR) VALUES(@name, @priceUSD, @priceBYR)", con))
                    {
                        com.Parameters.AddWithValue("@name", dataGridView2.Rows[i].Cells[0].Value);
                        com.Parameters.AddWithValue("@priceUSD", dataGridView2.Rows[i].Cells[1].Value);
                        com.Parameters.AddWithValue("@priceBYR", dataGridView2.Rows[i].Cells[2].Value);
                        com.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void substringSearch()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (SqlCommand com = new SqlCommand("SELECT * FROM Price WHERE name LIKE '%" + textBox1.Text + "%' ESCAPE '|'", con))
                    {
                        SqlDataReader reader = com.ExecuteReader();
                        dataGridView1.Rows.Clear();
                        int index = 0;
                        while (reader.Read())
                        {
                            dataGridView1.Rows.Add();
                            dataGridView1.Rows[index].Cells[0].Value = reader.GetString(0);
                            dataGridView1.Rows[index].Cells[1].Value = reader.GetString(1);
                            dataGridView1.Rows[index].Cells[2].Value = reader.GetString(2);
                            index++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }        

        /// <summary>
        /// вывод таблицы из бд
        /// </summary>
        public void DataBaseToDataGrid()
        {
            try
            {
                dataGridView1.ColumnCount = 3;
                dataGridView1.Columns[0].Width = 475;
                dataGridView1.Columns[1].Width = 60;
                dataGridView1.Columns[2].Width = 80;
                dataGridView1.Columns[0].HeaderText = "Название товара";
                dataGridView1.Columns[1].HeaderText = "Цена USD";
                dataGridView1.Columns[2].HeaderText = "Цена BYR";
                dataGridView2.ColumnCount = 3;
                dataGridView2.Columns[0].Width = 475;
                dataGridView2.Columns[1].Width = 79;
                dataGridView2.Columns[2].Width = 79;
                dataGridView2.Columns[0].HeaderText = "Название товара";
                dataGridView2.Columns[1].HeaderText = "Цена USD";
                dataGridView2.Columns[2].HeaderText = "Цена BYR";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (SqlCommand com = new SqlCommand("SELECT * FROM Price", con))
                    {
                        using (SqlDataReader reader = com.ExecuteReader())
                        {
                            int index = 0;
                            while (reader.Read())
                            {
                                dataGridView1.Rows.Add();
                                dataGridView1.Rows[index].Cells[0].Value = reader.GetString(0);
                                dataGridView1.Rows[index].Cells[1].Value = reader.GetString(1);
                                dataGridView1.Rows[index].Cells[2].Value = reader.GetString(2);
                                index++;
                            }
                            com.Dispose();
                            reader.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public string moneyParse(string StrInput)
        {
            try
            {
                string StrOutput = "";
                for (int i = 0; i < StrInput.Length; i++)
                {
                    if (StrInput[i] != ' ')
                        StrOutput += StrInput[i];
                    else break;
                }
                return StrOutput;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return "Ошибка!";
            }
        }

        /// <summary>
        /// добавление строки в другую таблицу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                dataGridView2.Rows.Add();
                for (int i = 0; i < 3; i++)
                    dataGridView2.Rows[indexRowDataGrid2].Cells[i].Value = dataGridView1.Rows[e.RowIndex].Cells[i].Value;
                indexRowDataGrid2++;
                usd.Text = (double.Parse(usd.Text) + double.Parse(moneyParse(dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[1].Value.ToString()))).ToString();
                byr.Text = (double.Parse(byr.Text) + double.Parse(moneyParse(dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[2].Value.ToString()))).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// удаление строки из другой таблицы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView2_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                usd.Text = (double.Parse(usd.Text) - double.Parse(moneyParse(dataGridView2.Rows[e.RowIndex].Cells[1].Value.ToString()))).ToString();
                byr.Text = (double.Parse(byr.Text) - double.Parse(moneyParse(dataGridView2.Rows[e.RowIndex].Cells[2].Value.ToString()))).ToString();
                dataGridView2.Rows.RemoveAt(e.RowIndex);
                indexRowDataGrid2--;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// перезапускаем ексель запрос на вывод таблицы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void перезагрузитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataBaseToDataGrid();
        }

        /// <summary>
        /// очищаем вторую таблицу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void очиститьТаблицуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView2.Rows.Clear();
            usd.Text = "0";
            byr.Text = "0";
            indexRowDataGrid2 = 0;
        }

        private void выйтиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// поиск по таблице
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            substringSearch();  // sql запрос на поиск подстроки
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                substringSearch();  // sql запрос на поиск подстроки
        }

        private void печататьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveExcel();
        }

        public void SaveExcel()
        {
            try
            {
                Excel.Application app = new Excel.Application();
                app.Visible = false;
                Excel.Workbook wb = app.Workbooks.Add(1);
                Excel.Worksheet ws = (Excel.Worksheet)wb.Worksheets[1];
                ws.Name = "Exported from gridview";
                ws.Rows.HorizontalAlignment = HorizontalAlignment.Center;
                for (int i = 1; i < dataGridView2.Columns.Count + 1; i++)
                {
                    ws.Cells[1, i] = dataGridView2.Columns[i - 1].HeaderText;
                }
                for (int i = 0; i < dataGridView2.Rows.Count - 1; i++)
                {
                    for (int j = 0; j < dataGridView2.Columns.Count; j++)
                    {
                        ws.Cells[i + 2, j + 1] = dataGridView2.Rows[i].Cells[j].Value.ToString();
                    }
                }
                ws.Cells[dataGridView2.Rows.Count + 2, 0 + 1] = "Итог:";
                ws.Cells[dataGridView2.Rows.Count + 2, 1 + 1] = Math.Round(double.Parse(usd.Text), 2).ToString() + " USD";
                ws.Cells[dataGridView2.Rows.Count + 2, 2 + 1] = Math.Round(double.Parse(byr.Text), 2).ToString() + " BYR";
                ws.Cells.EntireColumn.AutoFit();
                wb.SaveAs("D:\\Отчёт.xlsx", Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                app.Quit();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
