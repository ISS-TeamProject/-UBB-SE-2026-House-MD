using ERManagementSystem.DataAccess;
using ERManagementSystem.Helpers;
using ERManagementSystem.Repositories;
using ERManagementSystem.Services;
using ERManagementSystem.ViewModels;
using ERManagementSystem.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

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

            // ── Repositories (Miruna: Patient & Visit) ───────────────────────
            services.AddTransient<PatientRepository>();
            services.AddTransient<ERVisitRepository>();

            // ── Repositories (Alex: Room) ─────────────────────────────────── ← NEW
            services.AddTransient<RoomRepository>();

            // ── Services (Miruna: Registration & State) ──────────────────────
            services.AddTransient<RegistrationService>();
            services.AddTransient<StateManagementService>();

            // ── Services (Alex: Room Assignment & Management) ─────────────── ← NEW
            services.AddTransient<RoomAssignmentService>();
            services.AddTransient<RoomManagementService>();

            // ── ViewModels ───────────────────────────────────────────────────
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<PatientRegistrationViewModel>();

            // ── ViewModels (Alex) ─────────────────────────────────────────── ← NEW
            services.AddTransient<RoomAssignmentViewModel>();
            services.AddTransient<RoomManagementViewModel>();

            // ── Views ────────────────────────────────────────────────────────
            services.AddTransient<PatientRegistrationView>();

            // ── Views (Alex) ──────────────────────────────────────────────── ← NEW
            services.AddTransient<RoomAssignmentView>();
            services.AddTransient<RoomManagementView>();

            Services = services.BuildServiceProvider();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainAppWindow = new MainWindow();
            MainAppWindow.Activate();
        }
    }
}
