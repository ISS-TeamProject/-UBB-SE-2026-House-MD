using System;
using System.Collections.Generic;

namespace ERManagementSystem.Models
{
    /// <summary>
    /// Represents an ER visit in the ER Management System.
    /// Maps directly to the dbo.ER_Visit table in the database.
    /// </summary>
    public class ER_Visit
    {
        // ?????????????????????????????????????????????
        // Properties  (names match schema columns exactly)
        // ?????????????????????????????????????????????

        /// <summary>Auto-incremented primary key.</summary>
        public int Visit_ID { get; set; }

        /// <summary>Foreign key referencing Patient.Patient_ID (CNP).</summary>
        public string Patient_ID { get; set; }

        /// <summary>Defaults to current date/time when the visit is created.</summary>
        public DateTime Arrival_date_time { get; set; } = DateTime.Now;

        public string Chief_Complaint { get; set; }

        /// <summary>
        /// Current workflow state of the visit.
        /// Must be one of the 8 values defined in <see cref="AllowedStatuses"/>.
        /// </summary>
        public string Status { get; set; } = VisitStatus.REGISTERED;

        // ?????????????????????????????????????????????
        // Allowed status values (mirrors DB CHECK constraint)
        // ?????????????????????????????????????????????

        /// <summary>
        /// Static class holding the 8 valid status constants,
        /// so you never have to hardcode strings anywhere else.
        /// </summary>
        public static class VisitStatus
        {
            public const string REGISTERED = "REGISTERED";
            public const string TRIAGED = "TRIAGED";
            public const string WAITING_FOR_ROOM = "WAITING_FOR_ROOM";
            public const string IN_ROOM = "IN_ROOM";
            public const string WAITING_FOR_DOCTOR = "WAITING_FOR_DOCTOR";
            public const string IN_EXAMINATION = "IN_EXAMINATION";
            public const string TRANSFERRED = "TRANSFERRED";
            public const string CLOSED = "CLOSED";
        }

        public static readonly IReadOnlyList<string> AllowedStatuses = new[]
        {
            VisitStatus.REGISTERED,
            VisitStatus.TRIAGED,
            VisitStatus.WAITING_FOR_ROOM,
            VisitStatus.IN_ROOM,
            VisitStatus.WAITING_FOR_DOCTOR,
            VisitStatus.IN_EXAMINATION,
            VisitStatus.TRANSFERRED,
            VisitStatus.CLOSED
        };

        // ?????????????????????????????????????????????
        // Valid transitions (enforced by StateManagementService - task 2.12)
        // ?????????????????????????????????????????????

        /// <summary>
        /// Defines which transitions are allowed from each state.
        /// Used by StateManagementService to validate before any status update.
        /// </summary>
        public static readonly Dictionary<string, List<string>> ValidTransitions =
            new Dictionary<string, List<string>>
            {
                { VisitStatus.REGISTERED,         new List<string> { VisitStatus.TRIAGED } },
                { VisitStatus.TRIAGED,            new List<string> { VisitStatus.WAITING_FOR_ROOM, VisitStatus.CLOSED } },
                { VisitStatus.WAITING_FOR_ROOM,   new List<string> { VisitStatus.IN_ROOM } },
                { VisitStatus.IN_ROOM,            new List<string> { VisitStatus.WAITING_FOR_DOCTOR } },
                { VisitStatus.WAITING_FOR_DOCTOR, new List<string> { VisitStatus.IN_EXAMINATION } },
                { VisitStatus.IN_EXAMINATION,     new List<string> { VisitStatus.TRANSFERRED, VisitStatus.CLOSED } },
                { VisitStatus.TRANSFERRED,        new List<string>() },
                { VisitStatus.CLOSED,             new List<string>() }
            };

        // ?????????????????????????????????????????????
        // Validation
        // ?????????????????????????????????????????????

        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Patient_ID))
                errors.Add("Patient ID is required.");

            if (Arrival_date_time == default)
                errors.Add("Arrival date and time is required.");

            if (string.IsNullOrWhiteSpace(Chief_Complaint))
                errors.Add("Chief complaint is required.");
            else if (Chief_Complaint.Length > 255)
                errors.Add("Chief complaint must not exceed 255 characters.");

            if (string.IsNullOrWhiteSpace(Status))
                errors.Add("Status is required.");
            else if (!AllowedStatuses.Contains(Status))
                errors.Add($"Invalid status '{Status}'. Must be one of: {string.Join(", ", AllowedStatuses)}.");

            return errors.Count == 0;
        }

        /// <summary>
        /// Checks whether transitioning from the current Status to
        /// <paramref name="newStatus"/> is a valid workflow move.
        /// </summary>
        public bool CanTransitionTo(string newStatus)
        {
            if (!ValidTransitions.ContainsKey(Status)) return false;
            return ValidTransitions[Status].Contains(newStatus);
        }

        /// <summary>
        /// Attempts to transition the visit to <paramref name="newStatus"/>.
        /// Calls <see cref="CanTransitionTo"/> first — throws an
        /// <see cref="InvalidOperationException"/> with a descriptive message
        /// if the transition is not allowed.
        /// </summary>
        public void ChangeStatus(string newStatus)
        {
            if (!CanTransitionTo(newStatus))
                throw new InvalidOperationException(
                    $"Invalid transition: cannot move from '{Status}' to '{newStatus}'.");

            Status = newStatus;
        }

        // ?????????????????????????????????????????????
        // Helpers
        // ?????????????????????????????????????????????

        public override string ToString() =>
            $"[Visit {Visit_ID}] Patient: {Patient_ID} | Status: {Status} | Arrived: {Arrival_date_time:yyyy-MM-dd HH:mm}";
    }
}
