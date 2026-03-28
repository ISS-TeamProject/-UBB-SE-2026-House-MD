using ERManagementSystem.DataAccess;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ERManagementSystem.Helpers
{
    public class SqlHelper
    {
        private readonly DatabaseConnection _databaseConnection;

        public SqlHelper(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        public SqlDataReader ExecuteReader(string sql, params SqlParameter[] parameters)
        {
            var connection = _databaseConnection.Open();
            var command = new SqlCommand(sql, connection);

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }
        // Repositories should call this method in a using statement to ensure proper disposal of the connection and reader
        /* IMPORTANT
           using var reader = _sqlHelper.ExecuteReader(query, parameters);
           while (reader.Read())
            {
                ...
            }
        */
        public int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using var connection = _databaseConnection.Open();
            using var command = new SqlCommand(sql, connection);

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return command.ExecuteNonQuery();
        }
    }
}