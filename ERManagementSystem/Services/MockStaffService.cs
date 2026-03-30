using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ERManagementSystem.Models;
using ERManagementSystem.Repositories;

namespace ERManagementSystem.Services
{
    using System;
    public class MockStaffService
    {
        // Simulates requesting a doctor from Staff Management system
        public int RequestDoctor(string specialization, Triage_Parameters symptoms)
        {
            // Hardcoded doctor assignment based on specialization
            return specialization switch
            {
                "Cardiology" => 101,
                "Orthopedics" => 102,
                "Neurology" => 103,
                "General" => 104,
                _ => 104 // Default to General practitioner
            };
        }


        // need doctor class so we can return Doctor Instance 
        public Doctor GetDoctorByID(int doctorID)
        {
            return doctorID switch
            {
                101 => new Doctor { DoctorID = 101, Name = "Dr. Smith", Specialty = "Cardiology" },
                102 => new Doctor { DoctorID = 102, Name = "Dr. Johnson", Specialty = "Orthopedics" },
                103 => new Doctor { DoctorID = 103, Name = "Dr. Williams", Specialty = "Neurology" },
                104 => new Doctor { DoctorID = 104, Name = "Dr. Brown", Specialty = "General Medicine" },
                _ => new Doctor { DoctorID = 0, Name = "Unknown", Specialty = "Unknown" }
            };
        }
    }
}