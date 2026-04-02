using ERManagementSystem.Repositories;
using ERManagementSystem.Services;
using ERManagementSystem.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.IO;

namespace ERManagementSystem.Views
{
    public sealed partial class TransferLogView : Page
    {
        public TransferLogViewModel? ViewModel { get; private set; }

        public TransferLogView()
        {
            InitializeComponent();
        }

        public TransferLogView(TransferLogViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ViewModel == null)
            {
                var dbConnection = new DataAccess.DatabaseConnection();
                var sqlHelper = new Helpers.SqlHelper(dbConnection);
                var transferDir = Path.Combine(AppContext.BaseDirectory, "transfers");
                var transferLogRepository = new TransferLogRepository(sqlHelper);
                var erVisitRepo = new ERVisitRepository(sqlHelper);
                var roomRepo = new RoomRepository(sqlHelper);
                // Task 5.13: pass RoomRepository so auto-clean fires on TRANSFERRED/CLOSED
                var stateSvc = new StateManagementService(erVisitRepo, roomRepo);
                var transferService = new TransferService(sqlHelper, transferDir, stateSvc);
                ViewModel = new TransferLogViewModel(transferService, sqlHelper, stateSvc, transferLogRepository);
            }

            this.Loaded += (s, args) =>
            {
                ViewModel.XamlRoot = this.XamlRoot;
            };
            ViewModel.LoadData();
        }
    }
}