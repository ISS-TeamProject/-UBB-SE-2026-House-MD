using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using ERManagementSystem.Services;

namespace ERManagementSystem.ViewModels
{
    /// <summary>
    /// Tasks 4.6, 4.9, 4.11: ExaminationViewModel
    ///
    /// Properties (all [ObservableProperty]):
    ///   SelectedVisit, DoctorId, Notes, DoctorName, DoctorSpecialty, RequestSpecialization
    ///
    /// Commands (all [RelayCommand] with CanExecute):
    ///   RequestDoctor(), SaveExamination(), LoadData()
    ///
    /// Task 4.9: Buttons disabled until conditions are met via CanExecute.
    /// Task 4.11: Shows doctor name, specialty, and triage specialization used in request.
    /// </summary>
    public partial class ExaminationViewModel : BaseViewModel
    {
        private readonly ExaminationService _examinationService;
        private readonly MockStaffService _mockStaffService;
        private readonly SqlHelper _sqlHelper;

        // XamlRoot needed to show ContentDialogs — set by the View
        public Microsoft.UI.Xaml.XamlRoot? XamlRoot { get; set; }


        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RequestDoctorCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveExaminationCommand))]
        private ER_Visit? selectedVisit;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveExaminationCommand))]
        private int doctorId;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveExaminationCommand))]
        private string notes = string.Empty;

        [ObservableProperty]
        private string doctorName = string.Empty;

        [ObservableProperty]
        private string doctorSpecialty = string.Empty;

        // Task 4.11: Show the triage specialization used in the doctor request
        [ObservableProperty]
        private string requestSpecialization = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ER_Visit> eligibleVisits = new();

        [ObservableProperty]
        private string statusMessage = string.Empty;

        // Constructor 

        public ExaminationViewModel(
            ExaminationService examinationService,
            MockStaffService mockStaffService,
            SqlHelper sqlHelper)
        {
            _examinationService = examinationService;
            _mockStaffService = mockStaffService;
            _sqlHelper = sqlHelper;
        }

        // Task 4.9: CanExecute methods 
        // Buttons are disabled until conditions are met.

        private bool CanRequestDoctor()
            => SelectedVisit != null && SelectedVisit.Status == "IN_ROOM";

        private bool CanSaveExamination()
            => SelectedVisit != null
               && SelectedVisit.Status == "WAITING_FOR_DOCTOR"
               && DoctorId > 0
               && !string.IsNullOrWhiteSpace(Notes);

        // LoadData [RelayCommand]
        // Task 4.6: Load visits in IN_ROOM and WAITING_FOR_DOCTOR status
        // via hand-written SELECT through SqlHelper.

        [RelayCommand]
        public void LoadData()
        {
            EligibleVisits.Clear();
            SelectedVisit = null;
            DoctorId = 0;
            DoctorName = string.Empty;
            DoctorSpecialty = string.Empty;
            RequestSpecialization = string.Empty;
            Notes = string.Empty;
            StatusMessage = string.Empty;

            const string sql = @"
                SELECT Visit_ID, Patient_ID, Arrival_date_time, Chief_Complaint, Status
                FROM dbo.ER_Visit
                WHERE Status IN ('IN_ROOM', 'WAITING_FOR_DOCTOR')
                ORDER BY Arrival_date_time ASC";

            using var reader = _sqlHelper.ExecuteReader(sql);
            while (reader.Read())
            {
                EligibleVisits.Add(new ER_Visit
                {
                    Visit_ID = reader.GetInt32(reader.GetOrdinal("Visit_ID")),
                    Patient_ID = reader.GetString(reader.GetOrdinal("Patient_ID")),
                    Arrival_date_time = reader.GetDateTime(reader.GetOrdinal("Arrival_date_time")),
                    Chief_Complaint = reader.GetString(reader.GetOrdinal("Chief_Complaint")),
                    Status = reader.GetString(reader.GetOrdinal("Status"))
                });
            }
        }

        // When SelectedVisit changes, show assigned doctor if applicable

        partial void OnSelectedVisitChanged(ER_Visit? value)
        {
            if (value == null)
            {
                DoctorId = 0;
                DoctorName = string.Empty;
                DoctorSpecialty = string.Empty;
                RequestSpecialization = string.Empty;
                Notes = string.Empty;
                return;
            }

            // Task 4.11: Always fetch the triage specialization for the selected visit
            RequestSpecialization = FetchTriageSpecialization(value.Visit_ID);

            // If visit already has a doctor assigned (WAITING_FOR_DOCTOR),
            // look up doctor details from the examination record
            if (value.Status == "WAITING_FOR_DOCTOR")
            {
                var existingExamSql = @"
                    SELECT Doctor_ID FROM dbo.Examination
                    WHERE Visit_ID = @Visit_ID";
                var param = new Microsoft.Data.SqlClient.SqlParameter("@Visit_ID", value.Visit_ID);
                using var reader = _sqlHelper.ExecuteReader(existingExamSql, param);
                if (reader.Read())
                {
                    DoctorId = reader.GetInt32(0);
                    var doctor = _mockStaffService.GetDoctorByID(DoctorId);
                    DoctorName = doctor.Name;
                    DoctorSpecialty = doctor.Specialty;
                }
            }
            else
            {
                DoctorId = 0;
                DoctorName = string.Empty;
                DoctorSpecialty = string.Empty;
            }

            Notes = string.Empty;
        }

        /// <summary>
        /// Task 4.11: Fetch the triage specialization for a visit via hand-written SELECT.
        /// </summary>
        private string FetchTriageSpecialization(int visitId)
        {
            const string sql = @"
                SELECT t.Specialization
                FROM dbo.Triage t
                WHERE t.Visit_ID = @Visit_ID";
            var param = new Microsoft.Data.SqlClient.SqlParameter("@Visit_ID", visitId);
            using var reader = _sqlHelper.ExecuteReader(sql, param);
            if (reader.Read())
            {
                return reader.GetString(0);
            }
            return string.Empty;
        }

        // ── RequestDoctor [RelayCommand] ─────────────────────────────────
        // Task 4.5 / 4.6: Request a doctor for the selected visit.
        // Task 4.9: CanExecute disables button unless visit is IN_ROOM.

        [RelayCommand(CanExecute = nameof(CanRequestDoctor))]
        public async void RequestDoctor()
        {
            if (SelectedVisit == null) return;

            try
            {
                int visitId = SelectedVisit.Visit_ID;
                int assignedDoctorId = _examinationService.RequestDoctor(visitId);

                var doctor = _mockStaffService.GetDoctorByID(assignedDoctorId);
                string specialization = RequestSpecialization;

                StatusMessage = $"Doctor {doctor.Name} ({doctor.Specialty}) assigned.";

                await ShowDialog("Doctor Assigned",
                    $"Doctor {doctor.Name} (ID: {assignedDoctorId}, Specialty: {doctor.Specialty})\n" +
                    $"Requested Specialization: {specialization}\n" +
                    $"Assigned to Visit {visitId}.");

                // Reload the list so the visit now shows WAITING_FOR_DOCTOR status.
                // LoadData() clears all state including DoctorId, so we must
                // re-select the visit and re-apply the doctor info afterwards.
                LoadData();

                // Re-select the same visit (now with WAITING_FOR_DOCTOR status)
                var reloadedVisit = EligibleVisits.FirstOrDefault(v => v.Visit_ID == visitId);
                if (reloadedVisit != null)
                {
                    SelectedVisit = reloadedVisit;
                }

                // Re-apply doctor info (no Examination record exists yet,
                // so OnSelectedVisitChanged couldn't find it from DB)
                DoctorId = assignedDoctorId;
                DoctorName = doctor.Name;
                DoctorSpecialty = doctor.Specialty;
                RequestSpecialization = specialization;
            }
            catch (Exception ex)
            {
                await ShowDialog("Error", $"Failed to request doctor: {ex.Message}");
            }
        }

        // SaveExamination [RelayCommand]
        // Task 4.4 / 4.8: Save the examination record and transition
        // WAITING_FOR_DOCTOR → IN_EXAMINATION.
        // Task 4.9: CanExecute disables button unless visit is WAITING_FOR_DOCTOR,
        //           doctor is assigned, and Notes are not empty.

        [RelayCommand(CanExecute = nameof(CanSaveExamination))]
        public async void SaveExamination()
        {
            if (SelectedVisit == null) return;

            try
            {
                var examination = new Examination
                {
                    Visit_ID = SelectedVisit.Visit_ID,
                    Doctor_ID = DoctorId,
                    Exam_Time = DateTime.Now,
                    // TODO: find a way to get the room id
                    Room_ID = SelectedVisit.Visit_ID, // don't know in which room patient is palced
                    Notes = Notes
                };

                _examinationService.SaveExamination(examination);

                await ShowDialog("Examination Saved",
                    $"Examination for Visit {SelectedVisit.Visit_ID} has been saved.\n" +
                    $"Doctor: {DoctorName} ({DoctorSpecialty})\n" +
                    $"Status transitioned to IN_EXAMINATION.");

                LoadData();
            }
            catch (Exception ex)
            {
                await ShowDialog("Error", $"Failed to save examination: {ex.Message}");
            }
        }

        // Helper: WinUI 3 ContentDialog 

        private async System.Threading.Tasks.Task ShowDialog(string title, string message)
        {
            if (XamlRoot == null) return;

            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }

        ///// HARD TODO's:
        // TODO: 4.10
            // Examination History per visit selected
        // TODO: 4.12 
            // Examination WPF Window that combines:
                //  patient info, triage results and Examination results.
                //  ?? should this be a separate window that with IN-EXAMINATION patients with the option 
                // to transfer or close the visit ?? 

        // Simple TODO's:
        // 4.13: auto save notes every 60 seconds if any changes.
        // 4.14: when doble clicking on a patient in the main list display a read-only 
            // visit info window that shows triage details, level, specialization, and nurse id  .
    }
}
