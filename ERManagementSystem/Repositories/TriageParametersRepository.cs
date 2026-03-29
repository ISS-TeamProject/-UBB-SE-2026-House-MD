using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using Microsoft.Data.SqlClient;
using System;

namespace ERManagementSystem.Repositories
{
    public class TriageParametersRepository
    {
        private readonly SqlHelper _sqlHelper;

        public TriageParametersRepository(SqlHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        /// <summary>
        /// Inserts a new Triage_Parameters record into the database.
        /// </summary>
        public void Add(Triage_Parameters parameters)
        {
            string sql = @"INSERT INTO Triage_Parameters (Triage_ID, Consciousness, Breathing, Bleeding, Injury_Type, Pain_Level)
                           VALUES (@Triage_ID, @Consciousness, @Breathing, @Bleeding, @Injury_Type, @Pain_Level)";

            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@Triage_ID", parameters.Triage_ID),
                new SqlParameter("@Consciousness", parameters.Consciousness),
                new SqlParameter("@Breathing", parameters.Breathing),
                new SqlParameter("@Bleeding", parameters.Bleeding),
                new SqlParameter("@Injury_Type", parameters.Injury_Type),
                new SqlParameter("@Pain_Level", parameters.Pain_Level)
            };

            _sqlHelper.ExecuteNonQuery(sql, sqlParams);
        }

        /// <summary>
        /// Retrieves Triage_Parameters by Triage_ID.
        /// Returns null if no record is found.
        /// </summary>
        public Triage_Parameters? GetByTriageId(int triageId)
        {
            string sql = "SELECT Triage_ID, Consciousness, Breathing, Bleeding, Injury_Type, Pain_Level FROM Triage_Parameters WHERE Triage_ID = @Triage_ID";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Triage_ID", triageId)
            };

            using var reader = _sqlHelper.ExecuteReader(sql, parameters);

            if (reader.Read())
            {
                return new Triage_Parameters
                {
                    Triage_ID = reader.GetInt32(reader.GetOrdinal("Triage_ID")),
                    Consciousness = reader.GetInt32(reader.GetOrdinal("Consciousness")),
                    Breathing = reader.GetInt32(reader.GetOrdinal("Breathing")),
                    Bleeding = reader.GetInt32(reader.GetOrdinal("Bleeding")),
                    Injury_Type = reader.GetInt32(reader.GetOrdinal("Injury_Type")),
                    Pain_Level = reader.GetInt32(reader.GetOrdinal("Pain_Level"))
                };
            }

            return null;
        }
    }
}
