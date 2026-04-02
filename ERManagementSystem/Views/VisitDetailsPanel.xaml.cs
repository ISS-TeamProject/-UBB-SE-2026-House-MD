using System.Collections.Generic;
using ERManagementSystem.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace ERManagementSystem.Views
{
    public sealed partial class VisitDetailsPanel : UserControl
    {
        public VisitDetailsPanel()
        {
            this.InitializeComponent();
        }

        // ─────────────────────────────────────────────
        // Public method — call this to load visit data
        // Pass triage as null if not yet triaged
        // ─────────────────────────────────────────────

        public void LoadVisit(Patient? patient, ER_Visit? visit, Triage? triage = null)
        {
            if (patient == null || visit == null)
            {
                ClearPanel();
                return;
            }

            // Patient
            FullNameText.Text = $"{patient.First_Name} {patient.Last_Name}";
            PatientIdText.Text = patient.Patient_ID;
            DateOfBirthText.Text = patient.Date_of_Birth.ToString("dd/MM/yyyy");
            GenderText.Text = patient.Gender;
            PhoneText.Text = patient.Phone;
            EmergencyContactText.Text = patient.Emergency_Contact;

            // Visit
            VisitIdText.Text = visit.Visit_ID.ToString();
            ArrivalText.Text = visit.Arrival_date_time.ToString("dd/MM/yyyy HH:mm");
            ChiefComplaintText.Text = visit.Chief_Complaint;

            // Triage — show card only if triage exists
            if (triage != null)
            {
                TriageCard.Visibility = Visibility.Visible;
                TriageLevelText.Text = $"Level {triage.Triage_Level}";
                SpecializationText.Text = triage.Specialization;
                NurseIdText.Text = triage.Nurse_ID.ToString();
                TriageTimeText.Text = triage.Triage_Time.ToString("dd/MM/yyyy HH:mm");
            }
            else
            {
                TriageCard.Visibility = Visibility.Collapsed;
            }

            // Timeline
            TimelineItems.ItemsSource = BuildTimeline(visit.Status);
        }

        // ─────────────────────────────────────────────
        // Timeline builder
        // ─────────────────────────────────────────────

        private static List<TimelineItem> BuildTimeline(string currentStatus)
        {
            var states = new[]
            {
                ER_Visit.VisitStatus.REGISTERED,
                ER_Visit.VisitStatus.TRIAGED,
                ER_Visit.VisitStatus.WAITING_FOR_ROOM,
                ER_Visit.VisitStatus.IN_ROOM,
                ER_Visit.VisitStatus.WAITING_FOR_DOCTOR,
                ER_Visit.VisitStatus.IN_EXAMINATION,
                ER_Visit.VisitStatus.TRANSFERRED,
                ER_Visit.VisitStatus.CLOSED
            };

            bool passedCurrent = false;
            var items = new List<TimelineItem>();

            foreach (var state in states)
            {
                bool isCurrent = state == currentStatus;
                bool isPast = !passedCurrent && !isCurrent;

                items.Add(new TimelineItem
                {
                    Label = state.Replace("_", " "),
                    DotColor = isCurrent ? new SolidColorBrush(Colors.DodgerBlue)
                                    : isPast ? new SolidColorBrush(Colors.Gray)
                                                : new SolidColorBrush(Colors.LightGray),
                    TextColor = isCurrent ? new SolidColorBrush(Colors.DodgerBlue)
                                    : isPast ? new SolidColorBrush(Colors.Gray)
                                                : new SolidColorBrush(Colors.LightGray),
                    Weight = isCurrent ? "Bold" : "Normal",
                    BadgeVisibility = isCurrent ? Visibility.Visible : Visibility.Collapsed
                });

                if (isCurrent) passedCurrent = true;
            }

            return items;
        }

        // ─────────────────────────────────────────────
        // Clear
        // ─────────────────────────────────────────────

        private void ClearPanel()
        {
            FullNameText.Text = string.Empty;
            PatientIdText.Text = string.Empty;
            DateOfBirthText.Text = string.Empty;
            GenderText.Text = string.Empty;
            PhoneText.Text = string.Empty;
            EmergencyContactText.Text = string.Empty;
            VisitIdText.Text = string.Empty;
            ArrivalText.Text = string.Empty;
            ChiefComplaintText.Text = string.Empty;
            TriageCard.Visibility = Visibility.Collapsed;
            TimelineItems.ItemsSource = null;
        }
    }

    // ─────────────────────────────────────────────
    // Timeline item helper class
    // ─────────────────────────────────────────────

    public class TimelineItem
    {
        public string Label { get; set; } = string.Empty;
        public SolidColorBrush DotColor { get; set; } = new(Colors.Gray);
        public SolidColorBrush TextColor { get; set; } = new(Colors.Gray);
        public string Weight { get; set; } = "Normal";
        public Visibility BadgeVisibility { get; set; } = Visibility.Collapsed;
    }
}