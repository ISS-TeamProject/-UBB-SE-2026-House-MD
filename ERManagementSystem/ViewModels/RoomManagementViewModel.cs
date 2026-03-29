using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERManagementSystem.Models;
using ERManagementSystem.Services;
using Microsoft.UI.Xaml.Controls;

namespace ERManagementSystem.ViewModels
{
    /// <summary>
    /// Task 5.11 — ViewModel for the Room Management view (Feature 7).
    /// Exposes three observable room lists and commands to load/clean rooms.
    /// </summary>
    public partial class RoomManagementViewModel : BaseViewModel
    {
        private readonly RoomManagementService _roomManagementService;

        public RoomManagementViewModel(RoomManagementService roomManagementService)
        {
            _roomManagementService = roomManagementService;
        }

        // ── Observable collections (task 5.11) ───────────────────────────────

        [ObservableProperty]
        private ObservableCollection<ER_Room> availableRooms = new();

        [ObservableProperty]
        private ObservableCollection<ER_Room> occupiedRooms = new();

        [ObservableProperty]
        private ObservableCollection<ER_Room> cleaningRooms = new();

        // ── Dashboard stats (task 5.14) ───────────────────────────────────────

        [ObservableProperty]
        private int totalRooms;

        [ObservableProperty]
        private int availableCount;

        [ObservableProperty]
        private int occupiedCount;

        [ObservableProperty]
        private int cleaningCount;

        [ObservableProperty]
        private ER_Room? selectedCleaningRoom;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        // ── Commands ─────────────────────────────────────────────────────────

        [RelayCommand]
        private void LoadRooms()
        {
            try
            {
                IsBusy = true;
                StatusMessage = string.Empty;

                AvailableRooms = new ObservableCollection<ER_Room>(_roomManagementService.GetAvailableRooms());
                OccupiedRooms  = new ObservableCollection<ER_Room>(_roomManagementService.GetOccupiedRooms());
                CleaningRooms  = new ObservableCollection<ER_Room>(_roomManagementService.GetCleaningRooms());

                AvailableCount = AvailableRooms.Count;
                OccupiedCount  = OccupiedRooms.Count;
                CleaningCount  = CleaningRooms.Count;
                TotalRooms     = AvailableCount + OccupiedCount + CleaningCount;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading rooms: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task MarkRoomAsCleaned()
        {
            if (SelectedCleaningRoom == null)
            {
                await ShowDialog("No Room Selected", "Please select a room in the Cleaning section first.");
                return;
            }

            try
            {
                IsBusy = true;
                _roomManagementService.MarkRoomAsCleaned(SelectedCleaningRoom.Room_ID);

                await ShowDialog("Room Ready",
                    $"Room {SelectedCleaningRoom.Room_ID} ({SelectedCleaningRoom.Room_Type}) " +
                    $"has been marked as available.");

                SelectedCleaningRoom = null;
                LoadRooms();
            }
            catch (Exception ex)
            {
                await ShowDialog("Error", ex.Message);
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
