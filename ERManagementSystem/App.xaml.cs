using ERManagementSystem.DataAccess;
using ERManagementSystem.Helpers;
using ERManagementSystem.Repositories;
using ERManagementSystem.Services;
using ERManagementSystem.ViewModels;
using ERManagementSystem.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

namespace ERManagementSystem
{
    public partial class App : Application
    {
        public static ServiceProvider Services { get; private set; } = null!;
        public static Window? MainAppWindow { get; private set; }

        public App()
        {
            this.InitializeComponent();
            ConfigureServices();
            ConfigureGlobalExceptionHandling(); // 1.11 - Global exception handling - for unexpected issues that escape everything else
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // ── Core infrastructure ──────────────────────────────────────────
            services.AddSingleton<DatabaseConnection>();
            services.AddSingleton<SqlHelper>();

            // ── Navigation ───────────────────────────────────────────────────
            services.AddSingleton<NavigationService>();
            services.AddSingleton<INavigationService>(sp =>
                sp.GetRequiredService<NavigationService>());

            // ── Repositories ─────────────────────────────────────────────────
            services.AddTransient<PatientRepository>();
            services.AddTransient<ERVisitRepository>();
            services.AddTransient<TriageRepository>();
            services.AddTransient<TriageParametersRepository>();
            services.AddTransient<ExaminationRepository>();
            services.AddTransient<TransferLogRepository>();
            services.AddTransient<RoomRepository>();              // Alex

            // ── Services ─────────────────────────────────────────────────────
            services.AddTransient<RegistrationService>();
            // Task 5.13: use factory so StateManagementService always gets RoomRepository
            // enabling auto-set room to cleaning when visit is TRANSFERRED or CLOSED.
            services.AddTransient<StateManagementService>(sp =>
                new StateManagementService(
                    sp.GetRequiredService<ERVisitRepository>(),
                    sp.GetRequiredService<RoomRepository>()));
            services.AddSingleton<NurseService>();
            services.AddTransient<TriageService>();
            services.AddTransient<QueueService>();
            services.AddTransient<RoomAssignmentService>();       // Alex
            services.AddTransient<RoomManagementService>();       // Alex

            // ── Services (Triage & Queue) ────────────────────────────────────
            services.AddSingleton<NurseService>();
            services.AddTransient<TriageService>();
            services.AddTransient<QueueService>();

            // ── ViewModels ───────────────────────────────────────────────────
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<PatientRegistrationViewModel>();
            services.AddTransient<TriageViewModel>();
            services.AddTransient<QueueViewModel>();
            services.AddTransient<RoomAssignmentViewModel>();     // Alex
            services.AddTransient<RoomManagementViewModel>();     // Alex

            // ── Views ────────────────────────────────────────────────────────
            services.AddTransient<PatientRegistrationView>();
            services.AddTransient<TriageView>();
            services.AddTransient<QueueView>();
            services.AddTransient<RoomAssignmentView>();          // Alex
            services.AddTransient<RoomManagementView>();          // Alex

            Services = services.BuildServiceProvider();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainAppWindow = new MainWindow();
            MainAppWindow.Activate();
        }

        private void ConfigureGlobalExceptionHandling()
        {
            this.UnhandledException += App_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Logger.Error("Unhandled UI exception.", e.Exception);
            e.Handled = true;

            await ErrorDialogHelper.ShowErrorAsync(
                "Unexpected Error",
                "Something went wrong. The error was logged."
            );
        }

        private void CurrentDomain_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Logger.Error("Unhandled non-UI exception.", ex);
            }
            else
            {
                Logger.Error("Unhandled non-UI exception with unknown exception object.");
            }
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
        {
            Logger.Error("Unobserved task exception.", e.Exception);
            e.SetObserved();
        }
    }
}
