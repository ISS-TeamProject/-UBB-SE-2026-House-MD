using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERManagementSystem.Services;

namespace ERManagementSystem.ViewModels
{
    public partial class MainWindowViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public MainWindowViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        [RelayCommand]
        private void ShowPatientRegistration()
        {
            //_navigationService.Navigate(typeof(PatientRegistrationView));
        }

        [RelayCommand]
        private void ShowQueue()
        {
            //_navigationService.Navigate(typeof(QueueView));
        }

        [RelayCommand]
        private void ShowTriage()
        {
            //_navigationService.Navigate(typeof(TriageView));
        }

        [RelayCommand]
        private void ShowRoomAssignment()
        {
            //_navigationService.Navigate(typeof(RoomAssignmentView));
        }

        [RelayCommand]
        private void ShowExamination()
        {
            //_navigationService.Navigate(typeof(ExaminationView));
        }

        [RelayCommand]
        private void ShowTransferLog()
        {
            // _navigationService.Navigate(typeof(TransferLogView));
        }

        [RelayCommand]
        private void ShowRoomManagement()
        {
            //_navigationService.Navigate(typeof(RoomManagementView));
        }
    }
}
