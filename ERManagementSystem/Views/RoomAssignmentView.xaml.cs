using ERManagementSystem.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ERManagementSystem.Views
{
    public sealed partial class RoomAssignmentView : Page
    {
        // Resolved BEFORE InitializeComponent so x:Bind never sees null
        public RoomAssignmentViewModel ViewModel { get; private set; }

        public RoomAssignmentView()
        {
            ViewModel = App.Services.GetRequiredService<RoomAssignmentViewModel>();
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // If a specific VM was passed (e.g. from MainWindowViewModel), use it
            if (e.Parameter is RoomAssignmentViewModel vm)
                ViewModel = vm;

            // Load data after navigation is complete — errors are caught inside the command
            ViewModel.LoadDataCommand.Execute(null);
        }
    }
}
