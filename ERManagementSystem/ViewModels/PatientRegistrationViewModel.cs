using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERManagementSystem.Models;
using ERManagementSystem.Services;
using Microsoft.UI.Xaml.Controls;

namespace ERManagementSystem.ViewModels
{
    public partial class PatientRegistrationViewModel : BaseViewModel
    {
        private readonly RegistrationService _registrationService;

        public PatientRegistrationViewModel(RegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

      

        [ObservableProperty]
        private string patientId = string.Empty;

        [ObservableProperty]
        private string firstName = string.Empty;

        [ObservableProperty]
        private string lastName = string.Empty;

        [ObservableProperty]
        private DateTimeOffset dateOfBirth = DateTimeOffset.Now.AddYears(-30);

        [ObservableProperty]
        private string gender = string.Empty;

        [ObservableProperty]
        private string phone = string.Empty;

        [ObservableProperty]
        private string emergencyContact = string.Empty;

        [ObservableProperty]
        private string chiefComplaint = string.Empty;

        

        [ObservableProperty]
        private string patientIdError = string.Empty;

        [ObservableProperty]
        private string firstNameError = string.Empty;

        [ObservableProperty]
        private string lastNameError = string.Empty;

        [ObservableProperty]
        private string dateOfBirthError = string.Empty;

        [ObservableProperty]
        private string phoneError = string.Empty;

        [ObservableProperty]
        private string chiefComplaintError = string.Empty;

      
        [ObservableProperty]
        private bool isFormValid = false;

       
        partial void OnPatientIdChanged(string value) => ValidateAll();
        partial void OnFirstNameChanged(string value) => ValidateAll();
        partial void OnLastNameChanged(string value) => ValidateAll();
        partial void OnDateOfBirthChanged(DateTimeOffset value) => ValidateAll();
        partial void OnPhoneChanged(string value) => ValidateAll();
        partial void OnChiefComplaintChanged(string value) => ValidateAll();

     
        private void ValidateAll()
        {
            bool valid = true;

            if (string.IsNullOrWhiteSpace(PatientId))
            { PatientIdError = "Patient ID (CNP) is required."; valid = false; }
            else
                PatientIdError = string.Empty;

            if (string.IsNullOrWhiteSpace(FirstName))
            { FirstNameError = "First name is required."; valid = false; }
            else
                FirstNameError = string.Empty;

            if (string.IsNullOrWhiteSpace(LastName))
            { LastNameError = "Last name is required."; valid = false; }
            else
                LastNameError = string.Empty;

            if (DateOfBirth >= DateTimeOffset.Now)
            { DateOfBirthError = "Date of birth must be in the past."; valid = false; }
            else
                DateOfBirthError = string.Empty;

            if (string.IsNullOrWhiteSpace(Phone))
            { PhoneError = "Phone number is required."; valid = false; }
            else if (!Regex.IsMatch(Phone, @"^07\d{8}$"))
            { PhoneError = "Phone must be in format 07XXXXXXXX."; valid = false; }
            else
                PhoneError = string.Empty;

            if (string.IsNullOrWhiteSpace(ChiefComplaint))
            { ChiefComplaintError = "Chief complaint is required."; valid = false; }
            else
                ChiefComplaintError = string.Empty;

            IsFormValid = valid;
           
            RegisterPatientAndVisitCommand.NotifyCanExecuteChanged();
        }

        

        private Microsoft.UI.Xaml.XamlRoot? GetXamlRoot()
            => App.MainAppWindow?.Content?.XamlRoot;

      
        [RelayCommand(CanExecute = nameof(IsFormValid))]
        private async Task RegisterPatientAndVisit()
        {
            try
            {
                var patient = new Patient
                {
                    Patient_ID = PatientId,
                    First_Name = FirstName,
                    Last_Name = LastName,
                    Date_of_Birth = DateOfBirth.DateTime,
                    Gender = Gender,
                    Phone = Phone,
                    Emergency_Contact = EmergencyContact
                };

                ER_Visit visit = _registrationService.RegisterPatientAndVisit(patient, ChiefComplaint);

                var dialog = new ContentDialog
                {
                    Title = "Registration Successful",
                    Content = $"Patient ID: {patient.Patient_ID}\n" +
                              $"Visit ID:   {visit.Visit_ID}\n" +
                              $"Status:     {visit.Status}",
                    CloseButtonText = "OK",
                    XamlRoot = GetXamlRoot()
                };

                await dialog.ShowAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Registration Failed",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = GetXamlRoot()
                };

                await dialog.ShowAsync();
            }
        }

        [RelayCommand]
        private void ClearForm()
        {
            PatientId = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            DateOfBirth = DateTimeOffset.Now.AddYears(-30);
            Gender = string.Empty;
            Phone = string.Empty;
            EmergencyContact = string.Empty;
            ChiefComplaint = string.Empty;

            PatientIdError = string.Empty;
            FirstNameError = string.Empty;
            LastNameError = string.Empty;
            DateOfBirthError = string.Empty;
            PhoneError = string.Empty;
            ChiefComplaintError = string.Empty;

            IsFormValid = false;
            RegisterPatientAndVisitCommand.NotifyCanExecuteChanged();
        }
    }
}