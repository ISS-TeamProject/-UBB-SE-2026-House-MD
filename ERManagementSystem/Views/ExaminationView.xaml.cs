using ERManagementSystem.Repositories;
using ERManagementSystem.Services;
using ERManagementSystem.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ERManagementSystem.Views
{
    public sealed partial class ExaminationView : Page
    {
        public ExaminationViewModel? ViewModel { get; private set; }

        public ExaminationView()
        {
            this.InitializeComponent();
            this.Loaded += ExaminationView_Loaded;
        }

        /// <summary>
        /// XamlRoot is only available after the page is in the visual tree.
        /// Setting it here guarantees ContentDialogs can be shown.
        /// </summary>
        private void ExaminationView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.XamlRoot = this.XamlRoot;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ViewModel == null)
            {
                // Create dependencies manually (same pattern as TransferLogView)
                var dbConnection = new DataAccess.DatabaseConnection();
                var sqlHelper = new Helpers.SqlHelper(dbConnection);

                // Repositories
                var examRepository = new ExaminationRepository(sqlHelper);
                var erVisitRepository = new ERVisitRepository(sqlHelper);
                var triageRepository = new TriageRepository(sqlHelper);

                // Services
                var stateManagementService = new StateManagementService(erVisitRepository);
                var mockStaffService = new MockStaffService();
                var examinationService = new ExaminationService(
                    examRepository,
                    erVisitRepository,
                    triageRepository,
                    mockStaffService,
                    stateManagementService);

                ViewModel = new ExaminationViewModel(examinationService, mockStaffService, sqlHelper);
            }

            // Re-evaluate all x:Bind bindings now that ViewModel is set.
            // x:Bind defaults to OneTime mode and evaluates during InitializeComponent()
            // when ViewModel is still null — this call fixes the command bindings.
            Bindings.Update();

            ViewModel.LoadData();
        }
    }
}