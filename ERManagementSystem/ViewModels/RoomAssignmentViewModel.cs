using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using ERManagementSystem.Repositories;
using ERManagementSystem.Services;
using Microsoft.UI.Xaml.Controls;

namespace ERManagementSystem.ViewModels
{
    public partial class RoomAssignmentViewModel : BaseViewModel
    {
        private readonly RoomAssignmentService _roomAssignmentService;
        private readonly RoomRepository        _roomRepository;
        private readonly ERVisitRepository     _erVisitRepository;
        private readonly PatientRepository     _patientRepository;
        private readonly TriageRepository      _triageRepository;

        public Microsoft.UI.Xaml.XamlRoot? XamlRoot { get; set; }

        public RoomAssignmentViewModel(
            RoomAssignmentService roomAssignmentService,
            RoomRepository        roomRepository,
            ERVisitRepository     erVisitRepository,
            PatientRepository     patientRepository,
            TriageRepository      triageRepository)
        {
            _roomAssignmentService = roomAssignmentService;
            _roomRepository        = roomRepository;
            _erVisitRepository     = erVisitRepository;
            _patientRepository     = patientRepository;
            _triageRepository      = triageRepository;
        }

        [ObservableProperty] private ObservableCollection<ER_Visit> waitingVisits  = new();
        [ObservableProperty] private ObservableCollection<ER_Room>  availableRooms = new();
        [ObservableProperty] private ER_Visit? selectedVisit;
        [ObservableProperty] private ER_Room?  selectedRoom;
        [ObservableProperty] private Patient? selectedPatient;
        [ObservableProperty] private Triage? selectedTriage;
        [ObservableProperty] private string statusMessage = string.Empty;

        partial void OnSelectedVisitChanged(ER_Visit? value)
        {
            if (value == null)
            {
                SelectedPatient = null;
                SelectedTriage = null;
                return;
            }

            try
            {
                SelectedPatient = _patientRepository.GetById(value.Patient_ID);
                SelectedTriage = _triageRepository.GetByVisitId(value.Visit_ID);
            }
            catch
            {
                SelectedPatient = null;
                SelectedTriage = null;
            }
        }

        [RelayCommand]
        public void LoadData()
        {
            try
            {
                IsBusy = true;
                StatusMessage = string.Empty;

                // Uses GetActiveVisitsWithTriage for priority-ordered waiting visits
                var waitingWithTriage = _erVisitRepository.GetActiveVisitsWithTriage();
                WaitingVisits = new ObservableCollection<ER_Visit>();
                foreach (var (visit, _) in waitingWithTriage)
                    WaitingVisits.Add(visit);

                AvailableRooms = new ObservableCollection<ER_Room>(_roomRepository.GetAvailableRooms());
            }
            catch (Exception ex)
            {
                Logger.Error("RoomAssignmentViewModel.LoadData failed.", ex);
                StatusMessage = $"Error loading data: {ex.Message}";
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task AssignRoom()
        {
            if (WaitingVisits.Count == 0)
            {
                await ShowDialog("No Waiting Visits", "There are no visits currently waiting for a room.");
                return;
            }
            try
            {
                IsBusy = true;
                bool assigned = _roomAssignmentService.AutoAssignRoom();
                if (assigned)
                {
                    await ShowDialog("Room Assigned", "The highest-priority visit has been automatically assigned to a matching room.");
                    LoadData();
                }
                else
                {
                    await ShowDialog("No Suitable Room", "No proper room matching this patient's requirements is currently available.\n\nPlease either wait for the required room to open up or manually assign them to an available room.");
                }
            }
            catch (Exception ex) { await ShowDialog("Assignment Failed", ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task ManualAssignRoom()
        {
            if (SelectedVisit == null || SelectedRoom == null)
            {
                await ShowDialog("Selection Required", "Please select both a waiting visit and an available room.");
                return;
            }
            if (SelectedRoom.Availability_Status != ER_Room.RoomStatus.Available)
            {
                await ShowDialog("Room Not Available", $"Room {SelectedRoom.Room_ID} is '{SelectedRoom.Availability_Status}'. Only available rooms can be assigned.");
                return;
            }
            if (SelectedVisit.Status != ER_Visit.VisitStatus.WAITING_FOR_ROOM)
            {
                await ShowDialog("Visit Not Waiting", $"Visit {SelectedVisit.Visit_ID} is in '{SelectedVisit.Status}'. Only WAITING_FOR_ROOM visits can be assigned.");
                return;
            }
            try
            {
                IsBusy = true;
                _roomAssignmentService.AssignRoomToVisit(SelectedVisit.Visit_ID, SelectedRoom.Room_ID);
                await ShowDialog("Room Assigned", $"Visit {SelectedVisit.Visit_ID} → Room {SelectedRoom.Room_ID} ({SelectedRoom.Room_Type}).");
                SelectedVisit = null;
                SelectedRoom  = null;
                LoadData();
            }
            catch (Exception ex) { await ShowDialog("Assignment Failed", ex.Message); }
            finally { IsBusy = false; }
        }

        private async Task ShowDialog(string title, string message)
        {
            if (XamlRoot == null) return;
            var dialog = new ContentDialog
            {
                Title = title, Content = message, CloseButtonText = "OK", XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
