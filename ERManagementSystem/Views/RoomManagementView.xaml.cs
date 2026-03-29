using ERManagementSystem.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ERManagementSystem.Views
{
    public sealed partial class RoomManagementView : Page
    {
        // Resolved BEFORE InitializeComponent so x:Bind never sees null
        public RoomManagementViewModel ViewModel { get; private set; }

        public RoomManagementView()
        {
            ViewModel = App.Services.GetRequiredService<RoomManagementViewModel>();
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // If a specific VM was passed (e.g. from MainWindowViewModel), use it
            if (e.Parameter is RoomManagementViewModel vm)
                ViewModel = vm;

            // Load data after navigation is complete — errors are caught inside the command
            ViewModel.LoadRoomsCommand.Execute(null);
        }
    }
}
