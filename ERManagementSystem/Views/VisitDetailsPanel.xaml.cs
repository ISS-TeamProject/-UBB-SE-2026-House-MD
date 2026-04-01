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
        // ─────────────────────────────────────────────

        public void LoadVisit(VisitDetailsData? data)
        {
            if (data == null)
            {
                ClearPanel();
                return;
            }

            // Patient
            FullNameText.Text = data.FullName;
            PatientIdText.Text = data.Patient_ID;
            DateOfBirthText.Text = data.Date_of_Birth.ToString("dd/MM/yyyy");
            GenderText.Text = data.Gender;
            PhoneText.Text = data.Phone;
            EmergencyContactText.Text = data.Emergency_Contact;

            // Visit
            VisitIdText.Text = data.Visit_ID.ToString();
            ArrivalText.Text = data.Arrival_date_time.ToString("dd/MM/yyyy HH:mm");
            ChiefComplaintText.Text = data.Chief_Complaint;

            // Triage — show card only if triage exists
            if (data.HasTriage)
            {
                TriageCard.Visibility = Visibility.Visible;
                TriageLevelText.Text = data.TriageLevelDisplay;
                SpecializationText.Text = data.SpecializationDisplay;
                NurseIdText.Text = data.Nurse_ID?.ToString() ?? "—";
                TriageTimeText.Text = data.Triage_Time?.ToString("dd/MM/yyyy HH:mm") ?? "—";
            }
            else
            {
                TriageCard.Visibility = Visibility.Collapsed;
            }

            // Timeline
            TimelineItems.ItemsSource = BuildTimeline(data.Status);
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
