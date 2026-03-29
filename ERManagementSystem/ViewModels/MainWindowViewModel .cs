using CommunityToolkit.Mvvm.Input;
using ERManagementSystem.Services;
using ERManagementSystem.ViewModels;
using ERManagementSystem.Views;
using Microsoft.Extensions.DependencyInjection;

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
            var vm = App.Services.GetRequiredService<PatientRegistrationViewModel>();
            _navigationService.Navigate(typeof(PatientRegistrationView), vm);
        }

        [RelayCommand]
        private void ShowQueue()
        {
            _navigationService.Navigate(typeof(QueueView));
        }

        [RelayCommand]
        private void ShowTriage()
        {
            _navigationService.Navigate(typeof(TriageView));
        }

        [RelayCommand]
        private void ShowRoomAssignment()
        {
            _navigationService.Navigate(typeof(RoomAssignmentView));
        }

        [RelayCommand]
        private void ShowExamination()
        {
            _navigationService.Navigate(typeof(ExaminationView));
        }

        [RelayCommand]
        private void ShowTransferLog()
        {
            _navigationService.Navigate(typeof(TransferLogView));
        }

        [RelayCommand]
        private void ShowRoomManagement()
        {
            _navigationService.Navigate(typeof(RoomManagementView));
        }
    }
}