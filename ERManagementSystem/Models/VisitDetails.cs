using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace ERManagementSystem.Models
{
   
    public class VisitDetailsData
    {
   
        public string Patient_ID { get; set; } = string.Empty;
        public string First_Name { get; set; } = string.Empty;
        public string Last_Name { get; set; } = string.Empty;
        public DateTime Date_of_Birth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Emergency_Contact { get; set; } = string.Empty;

       
        public int Visit_ID { get; set; }
        public DateTime Arrival_date_time { get; set; }
        public string Chief_Complaint { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

       
        public int? Triage_Level { get; set; }
        public string? Specialization { get; set; }
        public int? Nurse_ID { get; set; }
        public DateTime? Triage_Time { get; set; }

      
        public string FullName => $"{First_Name} {Last_Name}";
        public bool HasTriage => Triage_Level.HasValue;
        public string TriageLevelDisplay => Triage_Level.HasValue ? $"Level {Triage_Level}" : "Not yet triaged";
        public string SpecializationDisplay => string.IsNullOrEmpty(Specialization) ? "—" : Specialization;
    }
}