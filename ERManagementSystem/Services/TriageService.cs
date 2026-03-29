using ERManagementSystem.Models;
using ERManagementSystem.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERManagementSystem.Services
{
    internal class TriageService
    {
        private readonly TriageRepository _triageRepository;
        private readonly TriageParametersRepository _triageParametersRepository;
        private readonly NurseService _nurseService;
        private readonly StateManagementService _stateService;

        public TriageService(
            TriageRepository triageRepository,
            TriageParametersRepository triageParametersRepository,
            NurseService nurseService,
            StateManagementService stateService)
        {
            _triageRepository = triageRepository;
            _triageParametersRepository = triageParametersRepository;
            _nurseService = nurseService;
            _stateService = stateService;
        }

        /// <summary>
        /// Requests an available nurse from the mock external Staff Management system.
        /// </summary>
        public int? RequestAvailableNurse()
        {
            return _nurseService.RequestAvailableNurse();
        }

        /// <summary>
        /// Creates and persists the triage parameters record.
        /// </summary>
        public void CreateTriageParameters(Triage_Parameters parameters) //de schimbat la diagrama UML nu trebuie transmis si id-ul
        {
            _triageParametersRepository.Add(parameters);
        }

        /// <summary>
        /// Creates the triage record using the triage parameters.
        /// This method:
        /// 1. Requests an available nurse
        /// 2. Calculates triage level
        /// 3. Determines specialization
        /// 4. Persists the triage parameters
        /// 5. Persists the triage record
        /// </summary>
        public Triage CreateTriage(int visitId, Triage_Parameters parameters)
        {
            int? nurseId = RequestAvailableNurse();
            if (nurseId == null)
                throw new InvalidOperationException("No available nurse.");

            int triageLevel = CalculateTriageLevel(parameters);
            string specialization = DetermineSpecialization(parameters);

            Triage triage = new Triage
            {
                Visit_ID = visitId,
                Triage_Level = triageLevel,
                Specialization = specialization,
                Nurse_ID = nurseId.Value,
                Triage_Time = DateTime.Now
            };

            
            int triageId = _triageRepository.AddAndReturnId(triage);

            
            parameters.Triage_ID = triageId;

            
            CreateTriageParameters(parameters);

            triage.Triage_ID = triageId;

            _stateService.ChangeVisitStatus(visitId, "TRIAGED");


            return triage;
        }

        /// <summary>
        /// Calculates the triage level entirely in C#.
        ///
        /// Step 1: Critical condition check
        /// - If consciousness = 3 OR breathing = 3 OR injury_type = 3 OR bleeding = 3
        ///   => triage level = 1 (highest priority)
        ///
        /// Step 2: Weighted severity score
        /// severity_score =
        ///   (consciousness x 3) +
        ///   (breathing x 3) +
        ///   (bleeding x 2) +
        ///   (injury_type x 2) +
        ///   (pain_level x 1)
        ///
        /// Suggested score-to-level mapping:
        /// - 0 to 6   => Level 5
        /// - 7 to 12  => Level 4
        /// - 13 to 18 => Level 3
        /// - 19+      => Level 2
        ///
        /// Adjust these thresholds if your project documentation uses different cutoffs.
        /// </summary>
        public int CalculateTriageLevel(Triage_Parameters parameters)
        {
            if (parameters.Consciousness == 3 ||
                parameters.Breathing == 3 ||
                parameters.Injury_Type == 3 ||
                parameters.Bleeding == 3)
            {
                return 1;
            }

            int severityScore =
                (parameters.Consciousness * 3) +
                (parameters.Breathing * 3) +
                (parameters.Bleeding * 2) +
                (parameters.Injury_Type * 2) +
                (parameters.Pain_Level * 1);

            if (severityScore >= 19)
                return 2;

            if (severityScore >= 13)
                return 3;

            if (severityScore >= 7)
                return 4;

            return 5;
        }

        /// <summary>
        /// Determines the specialization for the triage record.
        ///
        /// Rules:
        /// - injury_type = 2 -> Orthopedics
        /// - breathing = 2 -> Pulmonology
        /// - consciousness = 2 OR 3 -> Neurology
        /// - bleeding = 3 OR injury_type = 3 -> General Surgery
        /// - else -> Emergency Medicine
        ///
        /// Note: This follows the rule order exactly as given.
        /// </summary>
        public string DetermineSpecialization(Triage_Parameters parameters)
        {
            if (parameters.Bleeding == 3 || parameters.Injury_Type == 3)
                return "General Surgery";

            if (parameters.Injury_Type == 2)
                return "Orthopedics";

            if (parameters.Breathing == 2)
                return "Pulmonology";

            if (parameters.Consciousness == 2 || parameters.Consciousness == 3)
                return "Neurology";

            return "Emergency Medicine";
        }

    }
}
