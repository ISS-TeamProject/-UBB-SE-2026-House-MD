using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using ERManagementSystem.Repositories;
using ERManagementSystem.Services;
using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ERManagementSystem.ViewModels
{
    /// <summary>
    /// Tasks 6.7, 6.9, 6.10, 6.11, 6.13
    ///
    /// Properties and methods match the class diagram exactly:
    ///   SelectedVisit  [ObservableProperty]
    ///   SendPatientData(): void  [RelayCommand]
    ///   LoadLogs(): void  [RelayCommand]
    ///
    /// Task 6.9: validation errors shown via WinUI 3 ContentDialog.
    /// </summary>
    public partial class TransferLogViewModel : BaseViewModel
    {
        public Action? ClearGridSelection { get; set; }
        public Action? RefreshGrid { get; set; }
        //public Action? ClearGridSelection { get; set; }
        private readonly TransferLogRepository _transferLogRepository;
        private readonly StateManagementService _stateManagementService;
        private readonly TransferService _transferService;
        private readonly SqlHelper _sqlHelper;

        // XamlRoot needed to show ContentDialogs — set by the View
        public Microsoft.UI.Xaml.XamlRoot? XamlRoot { get; set; }

        // Observable Properties
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSelectedVisit))]
        private VisitSummary? selectedVisit;

        [ObservableProperty]
        private ObservableCollection<VisitSummary> eligibleVisits = new();

        [ObservableProperty]
        private ObservableCollection<Transfer_Log> transferLogs = new();

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private bool canRetry = false;

        public bool HasSelectedVisit => SelectedVisit != null;

        // Constructor
        public TransferLogViewModel(TransferService transferService, SqlHelper sqlHelper,
    StateManagementService stateManagementService, TransferLogRepository transferLogRepository)
        {
            _transferService = transferService;
            _sqlHelper = sqlHelper;
            _stateManagementService = stateManagementService;
            _transferLogRepository = transferLogRepository;
        }

        // LoadLogs(): void  (Task 6.13)
        [RelayCommand]
        public void LoadLogs()
        {
            TransferLogs.Clear();
            CanRetry = false;

            if (SelectedVisit == null) return;

            var logs = _transferService.GetLogs(SelectedVisit.Visit_ID);
            foreach (var log in logs)
                TransferLogs.Add(log);

            // Task 6.11: enable Retry if most recent attempt was FAILED
            var latest = TransferLogs.FirstOrDefault();
            if (latest != null && latest.Status == "FAILED")
                CanRetry = true;
        }

        partial void OnSelectedVisitChanged(VisitSummary? value)
        {
            LoadLogs();
        }

        // Task 6.7 — Load visits in IN_EXAMINATION status
        [RelayCommand]
        public void LoadData()
        {
            SelectedVisit = null;
            TransferLogs.Clear();
            StatusMessage = string.Empty;
            CanRetry = false;

            var freshList = new ObservableCollection<VisitSummary>();

            const string sql = @"
        SELECT v.Visit_ID, v.Chief_Complaint, v.Status,
               p.First_Name, p.Last_Name, p.Transferred
        FROM dbo.ER_Visit v
        INNER JOIN dbo.Patient p ON p.Patient_ID = v.Patient_ID
        WHERE v.Status = 'IN_EXAMINATION'
        ORDER BY v.Arrival_date_time ASC";

            using var reader = _sqlHelper.ExecuteReader(sql);
            while (reader.Read())
            {
                freshList.Add(new VisitSummary
                {
                    Visit_ID = reader.GetInt32(0),
                    Chief_Complaint = reader.GetString(1),
                    Status = reader.GetString(2),
                    PatientName = reader.GetString(3) + " " + reader.GetString(4),
                    Transferred = reader.GetBoolean(5)
                });
            }

            EligibleVisits = freshList;
        }

        // SendPatientData(): void  (Tasks 6.7, 6.9, 6.10)
        [RelayCommand]
        public async void SendPatientData()
        {
            // Task 6.9: Validation with WinUI 3 ContentDialog 
            if (SelectedVisit == null)
            {
                await ShowDialog("Validation Error", "Please select a visit before sending.");
                return;
            }
            if (SelectedVisit.Status != "IN_EXAMINATION")
            {
                await ShowDialog("Validation Error",
                    "Transfer is only allowed for visits with status IN_EXAMINATION.");
                return;
            }
            if (SelectedVisit.Transferred)
            {
                await ShowDialog("Validation Error",
                    "This patient already has a successful transfer (Transferred = true).");
                return;
            }

            try
            {
                // Task 6.4 & 6.5: send data, save JSON, log attempt
                _transferService.SendPatientData(SelectedVisit.Visit_ID);

                // Task 6.10: transition visit + mark patient as transferred
                _transferService.TransitionVisitToTransferred(SelectedVisit.Visit_ID);
                _transferService.MarkPatientAsTransferred(SelectedVisit.Visit_ID);

                StatusMessage = "SUCCESS";
                CanRetry = false;

                // Task 6.10: show WinUI 3 ContentDialog success message
                await ShowDialog("Transfer Successful",
                    $"Patient data for Visit {SelectedVisit.Visit_ID} has been successfully sent to Patient Management.");
            }
            catch (Exception ex)
            {
                StatusMessage = $"FAILED — {ex.Message}";
                CanRetry = true;
                await ShowDialog("Transfer Failed",
                    $"Transfer failed: {ex.Message}\nYou can retry using the Retry button.");
            }
            finally
            {
                // Task 6.13: always refresh after every attempt
                LoadLogs();
            }
        }

        // Task 6.11 — Retry failed transfer
        [RelayCommand]
        public async void RetryTransfer()
        {
            if (SelectedVisit == null) return;

            try
            {
                var result = _transferService.RetryTransfer(SelectedVisit.Visit_ID);

                if (result.Status == "SUCCESS")
                {
                    _transferService.TransitionVisitToTransferred(SelectedVisit.Visit_ID);
                    _transferService.MarkPatientAsTransferred(SelectedVisit.Visit_ID);
                    StatusMessage = "Retry SUCCESS";
                    CanRetry = false;
                    await ShowDialog("Retry Successful",
                        $"Patient data for Visit {SelectedVisit.Visit_ID} was successfully sent on retry.");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Retry FAILED — {ex.Message}";
                await ShowDialog("Retry Failed", $"Retry failed: {ex.Message}");
            }
            finally
            {
                LoadLogs();
            }
        }

        [RelayCommand]
        public async void CloseVisit()
        {
            if (SelectedVisit == null)
            {
                await ShowDialog("Validation Error", "Please select a visit before closing.");
                return;
            }

            var visitId = SelectedVisit.Visit_ID;
            var patientName = SelectedVisit.PatientName;

            try
            {
                _stateManagementService.CloseVisit(visitId);

                LoadData();

                await ShowDialog("Visit Closed",
                    $"Visit {visitId} for {patientName} has been closed successfully.");
            }
            catch (Exception ex)
            {
                await ShowDialog("Close Failed", $"Could not close visit: {ex.Message}");
            }
        }

        // Task 6.9 helper — WinUI 3 ContentDialog
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
    }

    // VisitSummary — lightweight helper for the eligible visits DataGrid
    public class VisitSummary
    {
        public int Visit_ID { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Chief_Complaint { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool Transferred { get; set; }
    }
}