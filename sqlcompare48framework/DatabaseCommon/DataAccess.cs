using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using static DatabaseCommon.DataObject;

namespace DatabaseCommon
{
    public static class DataAccess
    {
        public static DataTable GetDataTable(
            string connectionString,
            string sql
        )
        {
            var returnDataset = new DataSet();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    var dataAdapter = new SqlDataAdapter(command);
                    dataAdapter.Fill(returnDataset);
                }
            }
            return returnDataset.Tables[0];
        }

        public static string GetConnectionString(
            string serverName,
            string databaseName
        )
        {
            return GetConnectionString(serverName, databaseName, string.Empty, string.Empty);
        }

        public static string GetConnectionString(
            string serverName,
            string databaseName,
            string password,
            string login
       )
        {
            string connectionString = "server=" + serverName + ";database=" + databaseName + ";";
            // If a password is provided, add login and password to connection string
            // Otherwise, use Windows authentication
            if (password.Length == 0)
            {
                connectionString += "integrated security=sspi;";
            }
            else
            {
                connectionString += "User ID=" + login + ";password=" + password + ";";
            }
            return connectionString;
        }

        public static DataSet FillDataSet(
            string storedProcName,
            List<StoredProcParameterValue> parameterList,
            string connectionString
        )
        {
            DataSet returnDataset = new DataSet();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(storedProcName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    SqlDataAdapter dataAdapter = new SqlDataAdapter(SetStoredProcParameters(command, parameterList));
                    dataAdapter.Fill(returnDataset);
                }
            }
            return returnDataset;
        }

        private static SqlCommand SetStoredProcParameters(
            SqlCommand command,
            List<StoredProcParameterValue> parameterList
        )
        {
            foreach (var item in parameterList)
            {
                command.Parameters.Add(item.ParameterName, item.ParameterDataType).Value = item.ParameterValue;
            }
            return command;
        }
    }
}
