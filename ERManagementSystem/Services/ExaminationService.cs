using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using ERManagementSystem.Models;
using ERManagementSystem.Repositories;
using ERManagementSystem.Helpers;

namespace ERManagementSystem.Services
{
    public class ExaminationService
    {
        private readonly ExaminationRepository _examRepository;
        private readonly ERVisitRepository _erVisitRepository;
        private readonly TriageRepository _triageRepository;
        private readonly MockStaffService _mockStaffService;
        private readonly StateManagementService _stateManagementService;
        private readonly TriageParametersRepository _triageParamsRepo;

        public ExaminationService(
            ExaminationRepository examRepository,
            ERVisitRepository erVisitRepository,
            TriageRepository triageRepository,
            MockStaffService mockStaffService,
            StateManagementService stateManagementService,
            TriageParametersRepository triageParamsRepo)
        {
            _examRepository = examRepository;
            _erVisitRepository = erVisitRepository;
            _triageRepository = triageRepository;
            _mockStaffService = mockStaffService;
            _stateManagementService = stateManagementService;
            _triageParamsRepo = triageParamsRepo;
        }

        /// <summary>
        /// Task 4.5: Doctor Request Workflow
        /// (1) Read triage record for the visit
        /// (2) Send specialization + symptoms to MockStaffService
        /// (3) Receive assigned Doctor_ID
        /// (4) Transition visit from IN_ROOM → WAITING_FOR_DOCTOR
        /// </summary>
        public int RequestDoctor(int visitID)
        {
            var triage = _triageRepository.GetByVisitId(visitID);

            if (triage == null)
            {
                Logger.Warning($"RequestDoctor failed: Triage record missing for Visit {visitID}");
                throw new Exception($"Triage record not found for visit {visitID}");
            }

            var triageParameters = _triageParamsRepo.GetByTriageId(triage.Triage_ID);

            if (triageParameters == null)
            {
                Logger.Warning($"RequestDoctor failed: Triage parameters missing for Triage {triage.Triage_ID}");
                throw new Exception($"Triage parameters not found for triage {triage.Triage_ID}");
            }

            int assignedDoctorId = _mockStaffService.RequestDoctor(
                triage.Specialization,
                triageParameters);

            _stateManagementService.ChangeVisitStatus(visitID, "WAITING_FOR_DOCTOR");
            Logger.Info($"Visit {visitID} transitioned to WAITING_FOR_DOCTOR.");

            return assignedDoctorId;
        }

        /// <summary>
        /// Task 4.4 / 4.8: Persists the examination record and transitions
        /// the visit from WAITING_FOR_DOCTOR → IN_EXAMINATION.
        /// </summary>
        public void SaveExamination(Examination exam)
        {
            _examRepository.Add(exam);
            _stateManagementService.ChangeVisitStatus(exam.Visit_ID, "IN_EXAMINATION");
            Logger.Info($"Visit {exam.Visit_ID} transitioned to IN_EXAMINATION following saved examination.");
        }
    }        
}
