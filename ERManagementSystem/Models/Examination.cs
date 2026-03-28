using System;

namespace ERManagementSystem.Models
{
    public class Examination
    {
        public int Exam_ID { get; set; }
        public int Visit_ID { get; set; }
        public int Doctor_ID { get; set; }
        public DateTime Exam_Time { get; set; }
        public int Room_ID {get; set;}
        public string Notes {get; set;} = string.Empty;

        public Examination()
        {
            Exam_Time = DateTime.Now;
        }

        public Examination(int examID, int visitId, int doctorID, DateTime examTime, int roomID, string notes)
        {
            Exam_ID = examID;
            Visit_ID = visitId;
            Doctor_ID = doctorID;
            Exam_Time = examTime;
            Room_ID = roomID;
            Notes = notes;
        }
        //Examination records are already in schema.sql
    
        //TODO: add validations
    }
}