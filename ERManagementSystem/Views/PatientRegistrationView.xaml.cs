using ERManagementSystem.ViewModels;
using Microsoft.UI.Xaml.Controls;
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
            }
        }

        // SelectedItem on a ComboBox with ComboBoxItem children doesn't bind cleanly
        // to a string property in WinUI 3 — we push the value manually here instead.
        private void GenderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null)
                return;

            if (sender is ComboBox box && box.SelectedItem is ComboBoxItem item)
                ViewModel.Gender = item.Tag?.ToString() ?? string.Empty;
        }
    }
}