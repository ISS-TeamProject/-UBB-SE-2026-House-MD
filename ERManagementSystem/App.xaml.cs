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

        // Expose the main window so ViewModels can access XamlRoot for dialogs
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

            // ── Repositories (Miruna's: Patient & Visit) ─────────────────────
            services.AddTransient<PatientRepository>();
            services.AddTransient<ERVisitRepository>();

            // ── Repositories (Triage) ─────────────────────────────────────────
            services.AddTransient<TriageRepository>();
            services.AddTransient<TriageParametersRepository>();

            // ── Services (Miruna's: Registration & State) ────────────────────
            services.AddTransient<RegistrationService>();
            services.AddTransient<StateManagementService>();

            // ── Services (Triage & Queue) ────────────────────────────────────
            services.AddSingleton<NurseService>();
            services.AddTransient<TriageService>();
            services.AddTransient<QueueService>();

            // ── ViewModels ───────────────────────────────────────────────────
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<PatientRegistrationViewModel>();
            services.AddTransient<TriageViewModel>();
            services.AddTransient<QueueViewModel>();

            // ── Views ────────────────────────────────────────────────────────
            services.AddTransient<PatientRegistrationView>();
            services.AddTransient<TriageView>();
            services.AddTransient<QueueView>();

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