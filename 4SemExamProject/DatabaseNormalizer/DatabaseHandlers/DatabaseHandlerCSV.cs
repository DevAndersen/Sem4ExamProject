using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseNormalizer.DatabaseHandlers
{
    /// <summary>
    /// Code based on https://social.msdn.microsoft.com/Forums/sqlserver/en-US/7122b09f-8a16-4dab-a636-2f321afcc3d0/excel-csv-handling-in-c?forum=csharpgeneral
    /// </summary>
    public class DatabaseHandlerCSV : IDatabaseHandler
    {
        public List<string[]> GetDataFromDatabase(string databaseLocation, string[] columns, string queryConditions)
        {
            string columnList = "";

            for (int i = 0; i < columns.Length; i++)
            {
                columnList += columns[i] + (i == columns.Length - 1 ? "" : ", ");
            }
            
            var connString = string.Format(
                @"Provider=Microsoft.Jet.OleDb.4.0; Data Source={0};Extended Properties=""Text;HDR=YES;FMT=Delimited""",
                Path.GetDirectoryName(databaseLocation)
            );

            DataSet ds = new DataSet();
            using (var conn = new OleDbConnection(connString))
            {
                conn.Open();
                string query = "SELECT " + columnList + " FROM [" + Path.GetFileName(databaseLocation) + "] " + queryConditions;
                using (var adapter = new OleDbDataAdapter(query, conn))
                {
                    adapter.Fill(ds);
                }
            }

            List<String[]> data = new List<string[]>();

            DataTable collection = ds.Tables[0];
            for (int i = 0; i < collection.Rows.Count; i++)
            {
                string[] dataRow = new string[collection.Rows[i].ItemArray.Length];
                for (int j = 0; j < collection.Rows[i].ItemArray.Length; j++)
                {
                    dataRow[j] = collection.Rows[i].ItemArray[j].ToString();
                }
                data.Add(dataRow);
            }

            return data;
        }
    }
}