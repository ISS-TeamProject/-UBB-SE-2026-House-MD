using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;

namespace ERManagementSystem.Repositories
{
    public class ERVisitRepository
    {
        private readonly SqlHelper _sqlHelper;

        public ERVisitRepository(SqlHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

     

        public void Add(ER_Visit visit)
        {
            const string query = @"
                INSERT INTO dbo.ER_Visit
                    (Patient_ID, Arrival_date_time, Chief_Complaint, Status)
                OUTPUT INSERTED.Visit_ID
                VALUES
                    (@Patient_ID, @Arrival_date_time, @Chief_Complaint, @Status)";

            var parameters = new[]
            {
                new SqlParameter("@Patient_ID",        visit.Patient_ID),
                new SqlParameter("@Arrival_date_time", visit.Arrival_date_time),
                new SqlParameter("@Chief_Complaint",   visit.Chief_Complaint),
                new SqlParameter("@Status",            ER_Visit.VisitStatus.REGISTERED)
            };

           
            using var reader = _sqlHelper.ExecuteReader(query, parameters);
            if (reader.Read())
                visit.Visit_ID = Convert.ToInt32(reader["Visit_ID"]);
        }

    

        public List<ER_Visit> GetActiveVisits()
        {
            const string query = @"
                SELECT Visit_ID, Patient_ID, Arrival_date_time, Chief_Complaint, Status
                FROM   dbo.ER_Visit
                WHERE  Status NOT IN ('TRANSFERRED', 'CLOSED')";

            var visits = new List<ER_Visit>();

            using var reader = _sqlHelper.ExecuteReader(query);
            while (reader.Read())
                visits.Add(MapReaderToERVisit(reader));

            return visits;
        }

        public void UpdateStatus(int visitId, string newStatus)
        {
            const string query = @"
                UPDATE dbo.ER_Visit
                SET    Status = @Status
                WHERE  Visit_ID = @Visit_ID";

            var parameters = new[]
            {
                new SqlParameter("@Status",   newStatus),
                new SqlParameter("@Visit_ID", visitId)
            };

            _sqlHelper.ExecuteNonQuery(query, parameters);
        }

        

        public ER_Visit? GetByVisitId(int visitId)
        {
            const string query = @"
                SELECT Visit_ID, Patient_ID, Arrival_date_time, Chief_Complaint, Status
                FROM   dbo.ER_Visit
                WHERE  Visit_ID = @Visit_ID";

            var parameters = new[]
            {
                new SqlParameter("@Visit_ID", visitId)
            };

            using var reader = _sqlHelper.ExecuteReader(query, parameters);
            if (reader.Read())
                return MapReaderToERVisit(reader);

            return null;
        }

        

        public List<ER_Visit> GetByStatus(string status)
        {
            const string query = @"
                SELECT Visit_ID, Patient_ID, Arrival_date_time, Chief_Complaint, Status
                FROM   dbo.ER_Visit
                WHERE  Status = @Status";

            var parameters = new[]
            {
                new SqlParameter("@Status", status)
            };

            var visits = new List<ER_Visit>();

            using var reader = _sqlHelper.ExecuteReader(query, parameters);
            while (reader.Read())
                visits.Add(MapReaderToERVisit(reader));

            return visits;
        }

        

        private static ER_Visit MapReaderToERVisit(SqlDataReader reader)
        {
            return new ER_Visit
            {
                Visit_ID = Convert.ToInt32(reader["Visit_ID"]),
                Patient_ID = reader["Patient_ID"].ToString()!,
                Arrival_date_time = Convert.ToDateTime(reader["Arrival_date_time"]),
                Chief_Complaint = reader["Chief_Complaint"].ToString()!,
                Status = reader["Status"].ToString()!
            };
        }
    }
}