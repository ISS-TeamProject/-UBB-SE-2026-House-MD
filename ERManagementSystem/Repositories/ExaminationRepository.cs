using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace ERManagementSystem.Repositories
{
    public class ExaminationRepository
    {
        private readonly SqlHelper _sqlHelper;

        public ExaminationRepository(SqlHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        // Inserts a new Examination record into the database.
        public void Add(Examination exam)
        {
            string sql = @"INSERT INTO Examination (Visit_ID, Doctor_ID, Exam_Time, Room_ID, Notes)
                           VALUES (@Visit_ID, @Doctor_ID, @Exam_Time, @Room_ID, @Notes)";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Visit_ID", exam.Visit_ID),
                new SqlParameter("@Doctor_ID", exam.Doctor_ID),
                new SqlParameter("@Exam_time", exam.Exam_Time),
                new SqlParameter("@Room_ID", exam.Room_ID),
                new SqlParameter("@Notes", exam.Notes)
            };

            try
            {
                _sqlHelper.ExecuteNonQuery(sql, parameters);
                Logger.Info($"Successfully added new examination record for Visit {exam.Visit_ID}.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Database operation failed in ExaminationRepository.Add for Visit {exam.Visit_ID}", ex);
                throw;
            }
        }

        // Task 4.10: Retrieves a list of Examination records by Visit_ID (history).
        public List<Examination> GetByVisitId(int visitId)
        {       
            
            var history = new List<Examination>();
            string sql = "SELECT Exam_ID, Visit_ID, Doctor_ID, Exam_Time, Room_ID, Notes FROM Examination WHERE Visit_ID = @Visit_ID ORDER BY Exam_Time DESC";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Visit_ID", visitId)
            };

            using var reader = _sqlHelper.ExecuteReader(sql, parameters);

            while (reader.Read())
            {
                history.Add(new Examination
                {
                    Exam_ID = reader.GetInt32(reader.GetOrdinal("Exam_ID")),
                    Visit_ID = reader.GetInt32(reader.GetOrdinal("Visit_ID")),
                    Doctor_ID = reader.GetInt32(reader.GetOrdinal("Doctor_ID")),
                    Exam_Time = reader.GetDateTime(reader.GetOrdinal("Exam_Time")),
                    Room_ID = reader.GetInt32(reader.GetOrdinal("Room_ID")),
                    Notes = reader.GetString(reader.GetOrdinal("Notes"))
                });
            }

            return history;
        }

        // Retrieves a list of Examination records by Patient_ID (full history).
        public List<Examination> GetByPatientId(string patientId)
        {
            var history = new List<Examination>();
            string sql = @"
                SELECT e.Exam_ID, e.Visit_ID, e.Doctor_ID, e.Exam_Time, e.Room_ID, e.Notes 
                FROM Examination e 
                JOIN ER_Visit v ON e.Visit_ID = v.Visit_ID 
                WHERE v.Patient_ID = @Patient_ID 
                ORDER BY e.Exam_Time DESC";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Patient_ID", patientId)
            };

            using var reader = _sqlHelper.ExecuteReader(sql, parameters);

            while (reader.Read())
            {
                history.Add(new Examination
                {
                    Exam_ID = reader.GetInt32(reader.GetOrdinal("Exam_ID")),
                    Visit_ID = reader.GetInt32(reader.GetOrdinal("Visit_ID")),
                    Doctor_ID = reader.GetInt32(reader.GetOrdinal("Doctor_ID")),
                    Exam_Time = reader.GetDateTime(reader.GetOrdinal("Exam_Time")),
                    Room_ID = reader.GetInt32(reader.GetOrdinal("Room_ID")),
                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? string.Empty : reader.GetString(reader.GetOrdinal("Notes"))
                });
            }

            return history;
        }

        // Task 4.13: Auto-save notes feature
        public void UpdateNotes(int examId, string notes)
        {
            string sql = "UPDATE Examination SET Notes = @Notes WHERE Exam_ID = @Exam_ID";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Exam_ID", examId),
                new SqlParameter("@Notes", notes)
            };

            try
            {
                _sqlHelper.ExecuteNonQuery(sql, parameters);
                Logger.Info($"Successfully auto-saved notes for examination {examId}.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Database operation failed in ExaminationRepository.UpdateNotes for Exam {examId}", ex);
                throw;
            }
        }

        public void Delete(Examination exam)
        {
            const string query = @"
                DELETE FROM dbo.Examination
                WHERE Exam_ID = @Exam_ID";

            var parameters = new[]
            {
                new SqlParameter("@Exam_ID", exam.Exam_ID)
            };

            _sqlHelper.ExecuteNonQuery(query, parameters);
        }

        public Triage_Parameters GetTriageWithParameters(int triageID)
        {
            string query = @"
                SELECT tp.*
                FROM Triage t
                JOIN Triage_Parameters tp ON t.Triage_ID = tp.Triage_ID
                WHERE t.Triage_ID = @TriageID";

            var parameters = new[] { new SqlParameter("@TriageID", triageID) };
            
            using var reader = _sqlHelper.ExecuteReader(query, parameters);
            
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

        // Task 4.12: Retrieves aggregated summary details via JOINs.
        public ExaminationSummaryDTO? GetExaminationSummary(int examId)
        {
            string sql = @"
                SELECT 
                    p.First_Name, p.Last_Name,
                    v.Arrival_date_time, v.Chief_Complaint,
                    t.Triage_Level, t.Specialization,
                    tp.Consciousness, tp.Breathing, tp.Bleeding, tp.Injury_Type, tp.Pain_Level,
                    e.Doctor_ID, e.Exam_Time, e.Notes
                FROM Examination e
                JOIN ER_Visit v ON e.Visit_ID = v.Visit_ID
                JOIN Patient p ON v.Patient_ID = p.Patient_ID
                JOIN Triage t ON v.Visit_ID = t.Visit_ID
                JOIN Triage_Parameters tp ON t.Triage_ID = tp.Triage_ID
                WHERE e.Exam_ID = @ExamId";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@ExamId", examId)
            };

            try
            {
                using var reader = _sqlHelper.ExecuteReader(sql, parameters);
                if (reader.Read())
                {
                    return new ExaminationSummaryDTO
                    {
                        FirstName = reader.GetString(0),
                        LastName = reader.GetString(1),
                        ArrivalDateTime = reader.GetDateTime(2),
                        ChiefComplaint = reader.GetString(3),
                        TriageLevel = reader.GetInt32(4),
                        Specialization = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                        Consciousness = reader.GetInt32(6),
                        Breathing = reader.GetInt32(7),
                        Bleeding = reader.GetInt32(8),
                        InjuryType = reader.GetInt32(9),
                        PainLevel = reader.GetInt32(10),
                        DoctorId = reader.GetInt32(11),
                        ExamTime = reader.GetDateTime(12),
                        Notes = reader.GetString(13)
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Database query failed in ExaminationRepository.GetExaminationSummary for Exam {examId}", ex);
                throw;
            }

            return null;
        }

        // Temporary method to fetch a valid Room_ID until Room Assignment feature is completed
        public int GetFirstRoomId()
        {
            string query = "SELECT TOP 1 Room_ID FROM dbo.ER_Room";
            using var reader = _sqlHelper.ExecuteReader(query);
            if (reader.Read())
            {
                return reader.GetInt32(0);
            }
            return 1; 
        }
    }
}
