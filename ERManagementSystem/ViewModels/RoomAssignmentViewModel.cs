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
    /// <summary>
    /// Tasks 5.7 + 5.9 — ViewModel for the Room Assignment view (Feature 6).
    /// Exposes waiting visits and available rooms; supports auto and manual assignment.
    ///
    /// STUB NOTE: WaitingVisits is loaded via ERVisitRepository.GetByStatus("WAITING_FOR_ROOM").
    /// TODO (member 3 - Mihai): replace LoadWaitingVisits() body with QueueService call
    /// once his priority-sorted queue is available — the ViewModel interface stays the same.
    /// </summary>
    public partial class RoomAssignmentViewModel : BaseViewModel
    {
        private readonly RoomAssignmentService _roomAssignmentService;
        private readonly RoomRepository        _roomRepository;
        private readonly ERVisitRepository     _erVisitRepository;

        public RoomAssignmentViewModel(
            RoomAssignmentService roomAssignmentService,
            RoomRepository        roomRepository,
            ERVisitRepository     erVisitRepository)
        {
            _roomAssignmentService = roomAssignmentService;
            _roomRepository        = roomRepository;
            _erVisitRepository     = erVisitRepository;
        }

        // ── Observable state ─────────────────────────────────────────────────

        [ObservableProperty]
        private ObservableCollection<ER_Visit> waitingVisits = new();

        [ObservableProperty]
        private ObservableCollection<ER_Room> availableRooms = new();

        [ObservableProperty]
        private ER_Visit? selectedVisit;

        [ObservableProperty]
        private ER_Room? selectedRoom;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        // Triage parameter inputs for auto-assign room type detection
        [ObservableProperty] private string specialization = string.Empty;
        [ObservableProperty] private int bleeding     = 1;
        [ObservableProperty] private int injuryType   = 1;
        [ObservableProperty] private int consciousness = 1;
        [ObservableProperty] private int breathing    = 1;

        // ── Commands ─────────────────────────────────────────────────────────

        [RelayCommand]
        private void LoadData()
        {
            try
            {
                IsBusy = true;
                StatusMessage = string.Empty;

                // STUB: replace with Mihai's priority-sorted queue when ready
                var waiting = _erVisitRepository.GetByStatus(ER_Visit.VisitStatus.WAITING_FOR_ROOM);
                WaitingVisits  = new ObservableCollection<ER_Visit>(waiting);

                var available = _roomRepository.GetAvailableRooms();
                AvailableRooms = new ObservableCollection<ER_Room>(available);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading data: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Auto-assign: picks the top waiting visit and finds a room based on triage parameters.
        /// Task 5.9 validation: at least one waiting visit must exist.
        /// </summary>
        [RelayCommand]
        private async Task AssignRoom()
        {
            // Task 5.9 — validation: auto-assign requires waiting visits
            if (WaitingVisits.Count == 0)
            {
                await ShowDialog("No Waiting Visits", "There are no visits currently waiting for a room.");
                return;
            }

            try
            {
                IsBusy = true;
                string? spec = string.IsNullOrWhiteSpace(Specialization) ? null : Specialization;

                bool assigned = _roomAssignmentService.AutoAssignRoom(
                    spec, Bleeding, InjuryType, Consciousness, Breathing);

                if (assigned)
                {
                    await ShowDialog("Room Assigned",
                        "The highest-priority visit has been automatically assigned to a matching room.");
                    LoadData();
                }
                else
                {
                    await ShowDialog("No Room Available",
                        "No suitable available room was found for the current visit. " +
                        "The patient remains in WAITING_FOR_ROOM.");
                }
            }
            catch (Exception ex)
            {
                await ShowDialog("Assignment Failed", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Manual assign: user selects a specific visit and a specific room.
        /// Task 5.9 validation: both must be selected, room available, visit waiting.
        /// </summary>
        [RelayCommand]
        private async Task ManualAssignRoom()
        {
            // Task 5.9 — validation for manual assignment
            if (SelectedVisit == null || SelectedRoom == null)
            {
                await ShowDialog("Selection Required",
                    "Please select both a waiting visit and an available room before assigning.");
                return;
            }

            if (SelectedRoom.Availability_Status != ER_Room.RoomStatus.Available)
            {
                await ShowDialog("Room Not Available",
                    $"Room {SelectedRoom.Room_ID} is currently '{SelectedRoom.Availability_Status}'. " +
                    $"Only 'available' rooms can be assigned.");
                return;
            }

            if (SelectedVisit.Status != ER_Visit.VisitStatus.WAITING_FOR_ROOM)
            {
                await ShowDialog("Visit Not Waiting",
                    $"Visit {SelectedVisit.Visit_ID} is in status '{SelectedVisit.Status}'. " +
                    $"Only visits in WAITING_FOR_ROOM can be assigned a room.");
                return;
            }

            try
            {
                IsBusy = true;
                _roomAssignmentService.AssignRoomToVisit(SelectedVisit.Visit_ID, SelectedRoom.Room_ID);

                await ShowDialog("Room Assigned",
                    $"Visit {SelectedVisit.Visit_ID} has been assigned to " +
                    $"Room {SelectedRoom.Room_ID} ({SelectedRoom.Room_Type}).");

                SelectedVisit = null;
                SelectedRoom  = null;
                LoadData();
            }
            catch (Exception ex)
            {
                await ShowDialog("Assignment Failed", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static async Task ShowDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title           = title,
                Content         = message,
                CloseButtonText = "OK",
                XamlRoot        = App.MainAppWindow?.Content?.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
