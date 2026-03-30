using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using ERManagementSystem.Models;
using ERManagementSystem.Repositories;

namespace ERManagementSystem.Services
{
    public class ExaminationServices
    {
        private readonly ExaminationRepository _examRepository;
        private readonly ERVisitRepository _erVisitRepository;
        private readonly TriageRepository _triageRepository;
        private readonly MockStaffService _mockStaffService;
        private readonly StateManagementService _stateManagementService;

        public ExaminationServices(
            ExaminationRepository examRepository,
            ERVisitRepository erVisitRepository,
            TriageRepository triageRepository,
            MockStaffService mockStaffService,
            StateManagementService stateManagementService)
        {
            _examRepository = examRepository;
            _erVisitRepository = erVisitRepository;
            _triageRepository = triageRepository;
            _mockStaffService = mockStaffService;
            _stateManagementService = stateManagementService;
        }
        
        // Retrieves Doctor_ID from MockStaffService 
        public int RequestDoctor(int visitID)
        {
            var triage = _triageRepository.GetByVisitId(visitID);

            if (triage == null)
                throw new Exception($"Triage record not found for visit {visitID}");
            
            // Get Triage_Parameters using the SQL join
            var triageParameters = _examRepository.GetTriageWithParameters(triage.Triage_ID);

            if (triageParameters == null)
                throw new Exception($"Triage parameters not found for triage {triage.Triage_ID}");
            
            int assignedDoctorId = _mockStaffService.RequestDoctor(
                triage.Specialization, 
                triageParameters);

            _stateManagementService.ChangeVisitStatus(visitID, "WAITING_FOR_DOCTOR");

            return assignedDoctorId;
        }

        public void SaveExamination(Examination exam)
        {
            _examRepository.Add(exam);
            // TODO: SHOW A WINUI 3 CONTENT DIALOG CONFIRMATION, REFRESH THE LIST TO REMOVE THE VISIT (task 4.8)
            _stateManagementService.ChangeVisitStatus(exam.Visit_ID, "IN EXAMINATION"); 
            // EXAMINATION RECORD STORES Exam_Time, Doctor_ID, Room_ID and Notes
            
            
            _examRepository.Delete(exam);
            
            // !! NO Examination Record table in database
                // add a table examination table with Exam_Time, Doctor_ID, Room_ID and Notes and an Exam_Record_ID
                // ExaminationRepository - needs an AddRecord(ExamRecord) function


            // 4.10: Add a section to ExaminationView showing past examinations for the selected visit. 
            // Fetch history via a hand-written SELECT in ExaminationRepository.GetByVisitId(). 
            // Display in a read-only DataGrid or ListBox below the main form.

        }

    }        
}
