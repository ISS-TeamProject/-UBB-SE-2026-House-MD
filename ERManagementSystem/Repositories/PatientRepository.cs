using System;
using System.Data;
using Microsoft.Data.SqlClient;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;

namespace ERManagementSystem.Repositories
{
    
    public class PatientRepository
    {
        private readonly SqlHelper _sqlHelper;

        public PatientRepository(SqlHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

       
        public void Add(Patient patient)
        {
            const string query = @"
                INSERT INTO dbo.Patient
                    (Patient_ID, First_Name, Last_Name, Date_of_Birth,
                     Gender, Phone, Emergency_Contact, Transferred)
                VALUES
                    (@Patient_ID, @First_Name, @Last_Name, @Date_of_Birth,
                     @Gender, @Phone, @Emergency_Contact, @Transferred)";

            var parameters = new[]
            {
                new SqlParameter("@Patient_ID",        patient.Patient_ID),
                new SqlParameter("@First_Name",        patient.First_Name),
                new SqlParameter("@Last_Name",         patient.Last_Name),
                new SqlParameter("@Date_of_Birth",     patient.Date_of_Birth),
                new SqlParameter("@Gender",            patient.Gender),
                new SqlParameter("@Phone",             patient.Phone),
                new SqlParameter("@Emergency_Contact", patient.Emergency_Contact),
                new SqlParameter("@Transferred",       patient.Transferred)
            };

            _sqlHelper.ExecuteNonQuery(query, parameters);
        }

       
        public Patient GetById(string id)
        {
            const string query = @"
                SELECT Patient_ID, First_Name, Last_Name, Date_of_Birth,
                       Gender, Phone, Emergency_Contact, Transferred
                FROM   dbo.Patient
                WHERE  Patient_ID = @Patient_ID";

            var parameters = new[]
            {
                new SqlParameter("@Patient_ID", id)
            };

            Patient? patient = null;

            using var reader = _sqlHelper.ExecuteReader(query, parameters);
            if (reader.Read())
            {
                patient = MapReaderToPatient(reader);
            }

            return patient;
        }

        
        public void Update(Patient patient)
        {
            const string query = @"
                UPDATE dbo.Patient
                SET    First_Name        = @First_Name,
                       Last_Name         = @Last_Name,
                       Date_of_Birth     = @Date_of_Birth,
                       Gender            = @Gender,
                       Phone             = @Phone,
                       Emergency_Contact = @Emergency_Contact,
                       Transferred       = @Transferred
                WHERE  Patient_ID = @Patient_ID";

            var parameters = new[]
            {
                new SqlParameter("@Patient_ID",        patient.Patient_ID),
                new SqlParameter("@First_Name",        patient.First_Name),
                new SqlParameter("@Last_Name",         patient.Last_Name),
                new SqlParameter("@Date_of_Birth",     patient.Date_of_Birth),
                new SqlParameter("@Gender",            patient.Gender),
                new SqlParameter("@Phone",             patient.Phone),
                new SqlParameter("@Emergency_Contact", patient.Emergency_Contact),
                new SqlParameter("@Transferred",       patient.Transferred)
            };

            _sqlHelper.ExecuteNonQuery(query, parameters);
        }

       
        public void Delete(Patient patient)
        {
            const string query = @"
                DELETE FROM dbo.Patient
                WHERE Patient_ID = @Patient_ID";

            var parameters = new[]
            {
                new SqlParameter("@Patient_ID", patient.Patient_ID)
            };

            _sqlHelper.ExecuteNonQuery(query, parameters);
        }

        
        private Patient MapReaderToPatient(SqlDataReader reader)
        {
            return new Patient
            {
                Patient_ID = reader["Patient_ID"].ToString(),
                First_Name = reader["First_Name"].ToString(),
                Last_Name = reader["Last_Name"].ToString(),
                Date_of_Birth = Convert.ToDateTime(reader["Date_of_Birth"]),
                Gender = reader["Gender"].ToString(),
                Phone = reader["Phone"].ToString(),
                Emergency_Contact = reader["Emergency_Contact"].ToString(),
                Transferred = Convert.ToBoolean(reader["Transferred"])
            };
        }
    }
}