using PriceProcessor;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceProcessor
{
    class SQLOpen
    {
        public void deleteOutputPrice(string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand com = new SqlCommand("DELETE FROM OutputPrice", con))
                {
                    com.ExecuteNonQuery();
                }
            }
        }

        public void AddDB(string nameFile, string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                ExcelFile ex = new ExcelFile();
                string[,] list = ex.OpenExcel(nameFile);
                con.Open();
                for (int i = 0; i < ex.GetArraySizeRows; i++)
                {
                    using (SqlCommand com = new SqlCommand("INSERT INTO Price(name, priceUSD, priceBYR) VALUES(@name, @priceUSD, @priceBYR)", con))
                    {
                        com.Parameters.AddWithValue("@name", list[1, i]);
                        com.Parameters.AddWithValue("@priceUSD", list[2, i]);
                        com.Parameters.AddWithValue("@priceBYR", list[3, i]);
                        com.ExecuteNonQuery();
                        com.Dispose();
                    }
                }
            }
        }
    }
}
