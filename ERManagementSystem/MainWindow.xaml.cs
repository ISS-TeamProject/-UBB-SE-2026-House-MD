using ERManagementSystem.Services;
using ERManagementSystem.ViewModels;
using ERManagementSystem.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ERManagementSystem
{
    
    public sealed partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; }

        public MainWindow()
        {
            /*
            this.InitializeComponent();

            ViewModel = App.Services.GetRequiredService<MainWindowViewModel>();
            this.Content.XamlRoot.Changed += (_, _) => { };

            var navigationService = App.Services.GetRequiredService<NavigationService>();
            navigationService.Initialize(ContentFrame);

            AppNavigationView.SelectedItem = AppNavigationView.MenuItems[0];
            ContentFrame.Navigate(typeof(PatientRegistrationView));*/
        }

        private void AppNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer?.Tag is not string tag)
                return;

            switch (tag)
            {
                case "PatientRegistration":
                    ViewModel.ShowPatientRegistrationCommand.Execute(null);
                    break;
                case "Queue":
                    ViewModel.ShowQueueCommand.Execute(null);
                    break;
                case "Triage":
                    ViewModel.ShowTriageCommand.Execute(null);
                    break;
                case "RoomAssignment":
                    ViewModel.ShowRoomAssignmentCommand.Execute(null);
                    break;
                case "Examination":
                    ViewModel.ShowExaminationCommand.Execute(null);
                    break;
                case "TransferLog":
                    ViewModel.ShowTransferLogCommand.Execute(null);
                    break;
                case "RoomManagement":
                    ViewModel.ShowRoomManagementCommand.Execute(null);
                    break;
            }
        }
    }
}
