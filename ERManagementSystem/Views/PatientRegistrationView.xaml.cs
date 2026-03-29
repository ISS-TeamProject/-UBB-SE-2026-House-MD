using ERManagementSystem.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;

namespace ERManagementSystem.Views
{
    public sealed partial class PatientRegistrationView : Page
    {
        public PatientRegistrationViewModel? ViewModel { get; private set; }

        public PatientRegistrationView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is PatientRegistrationViewModel vm)
            {
                ViewModel = vm;
                Bindings.Update();
            }
        }

        // DatePicker.Date is non-nullable DateTimeOffset in WinUI 3 —
        // cannot bind directly to DateTimeOffset?. Push the value manually
        // and set HasDateOfBirth = true so the ViewModel knows a date was picked.
        private void DateOfBirthPicker_DateChanged(
            CalendarDatePicker sender,
            CalendarDatePickerDateChangedEventArgs args)
        {
            if (ViewModel == null) return;

            if (args.NewDate.HasValue)
            {
                ViewModel.DateOfBirth = args.NewDate.Value;
                ViewModel.HasDateOfBirth = true;
            }
            else
            {
                ViewModel.HasDateOfBirth = false;
            }
        }

        // Gender — same pattern as before
        private void GenderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;

            if (sender is ComboBox box && box.SelectedItem is ComboBoxItem item)
                ViewModel.Gender = item.Tag?.ToString() ?? string.Empty;
        }
    }
}