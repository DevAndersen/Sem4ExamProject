using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseNormalizer.DatabaseHandlers
{
    public class DatabaseHandlerSQL : IDatabaseHandler
    {
        public List<string[]> GetDataFromDatabase(string databaseLocation, string[] columns, string queryConditions)
        {
            SqlConnection connection = new SqlConnection()
            {
                ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"" + databaseLocation + "\";Integrated Security=True;Connection Timeout=600;"
            };

            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            string columnList = "";

            for (int i = 0; i < columns.Length; i++)
            {
                columnList += columns[i] + (i == columns.Length - 1 ? "" : ", ");
            }

            cmd.CommandText = "SELECT " + columnList + " FROM moviedb " + queryConditions;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;

            connection.Open();

            reader = cmd.ExecuteReader();

            List<string[]> resultStrings = new List<string[]>();

            while (reader.Read())
            {
                string[] strings = new string[columns.Length];

                for (int i = 0; i < columns.Length; i++)
                {
                    object data = reader[columns[i]];
                    string dataString = "";

                    if (data.ToString().Contains('|'))
                    {
                        dataString = reader[columns[i]].ToString().Split('|')[0];
                    }
                    else
                    {
                        dataString = reader[columns[i]].ToString();
                    }

                    strings[i] = dataString;
                }
                string[] resultString = new string[columns.Length];
                for (int i = 0; i < strings.Length; i++)
                {
                    resultString[i] = strings[i];
                }
                resultStrings.Add(resultString);
            }
            reader.Close();
            connection.Close();

            return resultStrings;
        }
    }
}