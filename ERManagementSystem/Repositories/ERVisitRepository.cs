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

            try
            {
                using var reader = _sqlHelper.ExecuteReader(query, parameters);
                if (reader.Read())
                {
                    visit.Visit_ID = Convert.ToInt32(reader["Visit_ID"]);
                    Logger.Info($"ER Visit created with ID {visit.Visit_ID} for Patient {visit.Patient_ID}.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"DB error in ERVisitRepository.Add for Patient {visit.Patient_ID}.", ex);
                throw;
            }
        }

        public List<ER_Visit> GetActiveVisits()
        {
            const string query = @"
                SELECT Visit_ID, Patient_ID, Arrival_date_time, Chief_Complaint, Status
                FROM   dbo.ER_Visit
                WHERE  Status NOT IN ('TRANSFERRED', 'CLOSED')";

            var visits = new List<ER_Visit>();

            try
            {
                using var reader = _sqlHelper.ExecuteReader(query);
                while (reader.Read())
                    visits.Add(MapReaderToERVisit(reader));

                Logger.Info($"GetActiveVisits returned {visits.Count} visit(s).");
            }
            catch (Exception ex)
            {
                Logger.Error("DB error in ERVisitRepository.GetActiveVisits.", ex);
                throw;
            }

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

            try
            {
                _sqlHelper.ExecuteNonQuery(query, parameters);
                Logger.Info($"Visit {visitId} status updated to '{newStatus}' in DB.");
            }
            catch (Exception ex)
            {
                Logger.Error($"DB error in ERVisitRepository.UpdateStatus for Visit {visitId}.", ex);
                throw;
            }
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

            try
            {
                using var reader = _sqlHelper.ExecuteReader(query, parameters);
                if (reader.Read())
                    return MapReaderToERVisit(reader);

                Logger.Warning($"GetByVisitId: Visit {visitId} not found.");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error($"DB error in ERVisitRepository.GetByVisitId for Visit {visitId}.", ex);
                throw;
            }
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

            try
            {
                using var reader = _sqlHelper.ExecuteReader(query, parameters);
                while (reader.Read())
                    visits.Add(MapReaderToERVisit(reader));

                Logger.Info($"GetByStatus('{status}') returned {visits.Count} visit(s).");
            }
            catch (Exception ex)
            {
                Logger.Error($"DB error in ERVisitRepository.GetByStatus('{status}').", ex);
                throw;
            }

            return visits;
        }

        public List<(ER_Visit visit, Triage triage)> GetActiveVisitsWithTriage()
        {
            string sql = @"
                SELECT v.Visit_ID, v.Patient_ID, v.Arrival_date_time, v.Chief_Complaint, v.Status,
                       t.Triage_ID, t.Visit_ID AS Triage_Visit_ID, t.Triage_Level, t.Specialization, t.Nurse_ID, t.Triage_Time
                FROM ER_Visit v
                INNER JOIN Triage t ON v.Visit_ID = t.Visit_ID
                WHERE v.Status IN ('WAITING_FOR_ROOM')";

            var list = new List<(ER_Visit, Triage)>();

            try
            {
                using var reader = _sqlHelper.ExecuteReader(sql);
                while (reader.Read())
                {
                    var visit = new ER_Visit
                    {
                        Visit_ID = reader.GetInt32(reader.GetOrdinal("Visit_ID")),
                        Patient_ID = reader.GetString(reader.GetOrdinal("Patient_ID")),
                        Arrival_date_time = reader.GetDateTime(reader.GetOrdinal("Arrival_date_time")),
                        Chief_Complaint = reader.GetString(reader.GetOrdinal("Chief_Complaint")),
                        Status = reader.GetString(reader.GetOrdinal("Status"))
                    };

                    var triage = new Triage
                    {
                        Triage_ID = reader.GetInt32(reader.GetOrdinal("Triage_ID")),
                        Visit_ID = reader.GetInt32(reader.GetOrdinal("Triage_Visit_ID")),
                        Triage_Level = reader.GetInt32(reader.GetOrdinal("Triage_Level")),
                        Specialization = reader.GetString(reader.GetOrdinal("Specialization")),
                        Nurse_ID = reader.GetInt32(reader.GetOrdinal("Nurse_ID")),
                        Triage_Time = reader.GetDateTime(reader.GetOrdinal("Triage_Time"))
                    };

                    list.Add((visit, triage));
                }

                Logger.Info($"GetActiveVisitsWithTriage returned {list.Count} visit(s).");
            }
            catch (Exception ex)
            {
                Logger.Error("DB error in ERVisitRepository.GetActiveVisitsWithTriage.", ex);
                throw;
            }

            return list;
        }

        public VisitDetailsData? GetVisitDetails(int visitId)
        {
            const string query = @"
                SELECT
                    p.Patient_ID, p.First_Name, p.Last_Name, p.Date_of_Birth,
                    p.Gender, p.Phone, p.Emergency_Contact,
                    v.Visit_ID, v.Arrival_date_time, v.Chief_Complaint, v.Status,
                    t.Triage_Level, t.Specialization, t.Nurse_ID, t.Triage_Time
                FROM       dbo.ER_Visit  v
                INNER JOIN dbo.Patient   p ON v.Patient_ID = p.Patient_ID
                LEFT  JOIN dbo.Triage    t ON v.Visit_ID   = t.Visit_ID
                WHERE v.Visit_ID = @Visit_ID";

            var parameters = new[] { new SqlParameter("@Visit_ID", visitId) };

            try
            {
                using var reader = _sqlHelper.ExecuteReader(query, parameters);
                if (!reader.Read())
                {
                    Logger.Warning($"GetVisitDetails: No data found for Visit {visitId}.");
                    return null;
                }

                var data = new VisitDetailsData
                {
                    Patient_ID = reader["Patient_ID"].ToString()!,
                    First_Name = reader["First_Name"].ToString()!,
                    Last_Name = reader["Last_Name"].ToString()!,
                    Date_of_Birth = Convert.ToDateTime(reader["Date_of_Birth"]),
                    Gender = reader["Gender"].ToString()!,
                    Phone = reader["Phone"].ToString()!,
                    Emergency_Contact = reader["Emergency_Contact"].ToString()!,
                    Visit_ID = Convert.ToInt32(reader["Visit_ID"]),
                    Arrival_date_time = Convert.ToDateTime(reader["Arrival_date_time"]),
                    Chief_Complaint = reader["Chief_Complaint"].ToString()!,
                    Status = reader["Status"].ToString()!,
                    Triage_Level = reader["Triage_Level"] == DBNull.Value ? null : Convert.ToInt32(reader["Triage_Level"]),
                    Specialization = reader["Specialization"] == DBNull.Value ? null : reader["Specialization"].ToString(),
                    Nurse_ID = reader["Nurse_ID"] == DBNull.Value ? null : Convert.ToInt32(reader["Nurse_ID"]),
                    Triage_Time = reader["Triage_Time"] == DBNull.Value ? null : Convert.ToDateTime(reader["Triage_Time"])
                };

                Logger.Info($"GetVisitDetails loaded Visit {visitId} for Patient {data.Patient_ID}.");
                return data;
            }
            catch (Exception ex)
            {
                Logger.Error($"DB error in ERVisitRepository.GetVisitDetails for Visit {visitId}.", ex);
                throw;
            }
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