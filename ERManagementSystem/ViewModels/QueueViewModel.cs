using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERManagementSystem.Models;
using ERManagementSystem.Services;

namespace ERManagementSystem.ViewModels
{
    public partial class QueueViewModel : BaseViewModel
    {
        private readonly QueueService _queueService;

        public QueueViewModel(QueueService queueService)
        {
            _queueService = queueService;
        }

        // ── Observable collection for DataGrid ──────────────────────────────
        public ObservableCollection<QueueItemDisplay> ActiveVisits { get; } = new();

        [RelayCommand]
        private void LoadQueue()
        {
            ActiveVisits.Clear();
            var queue = _queueService.GetOrderedQueue();
            foreach (var (visit, triage) in queue)
            {
                ActiveVisits.Add(new QueueItemDisplay(visit, triage));
            }
        }

        [RelayCommand]
        private void RefreshQueue()
        {
            LoadQueue();
        }
    }
}
