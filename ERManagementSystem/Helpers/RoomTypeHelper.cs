using ERManagementSystem.Models;

namespace ERManagementSystem.Helpers
{
    /// <summary>
    /// Task 5.4 — Room type determination algorithm (Feature 6).
    /// Maps triage specialization and symptom parameters to the correct ER_Room type.
    /// All logic is in C#; no SQL involvement.
    /// </summary>
    public static class RoomTypeHelper
    {
        /// <summary>
        /// Determines the required room type for a visit based on its triage specialization
        /// and raw symptom parameters (1–3 scale, where 3 = most critical).
        /// Priority order matches the Feature 6 specification exactly.
        /// </summary>
        /// <param name="specialization">Triage-assigned specialization string (may be null/empty).</param>
        /// <param name="bleeding">Bleeding severity: 1 = none/minor, 2 = moderate, 3 = severe.</param>
        /// <param name="injuryType">Injury type severity: 1 = minor, 2 = moderate, 3 = major trauma.</param>
        /// <param name="consciousness">Consciousness level: 1 = alert, 2 = confused, 3 = unconscious.</param>
        /// <param name="breathing">Breathing status: 1 = normal, 2 = difficulty, 3 = absent/critical.</param>
        /// <returns>One of the six ER_Room.RoomType constants.</returns>
        public static string DetermineRoomType(
            string? specialization,
            int bleeding,
            int injuryType,
            int consciousness,
            int breathing)
        {
            // Rule 1 — Operating Room (highest priority)
            if (specialization == "General Surgery" || bleeding == 3 || injuryType == 3)
                return ER_Room.RoomType.OperatingRoom;

            // Rule 2 — Trauma / Resuscitation Bay
            if (consciousness == 3 || breathing == 3)
                return ER_Room.RoomType.TraumaBay;

            // Rule 3 — Respiratory / Monitored Room
            if (specialization == "Pulmonology" || breathing == 2)
                return ER_Room.RoomType.RespiratoryRoom;

            // Rule 4 — Neurology / Quiet Observation Room
            if (specialization == "Neurology" || consciousness == 2)
                return ER_Room.RoomType.NeurologyRoom;

            // Rule 5 — Orthopedic / Procedure Room
            if (specialization == "Orthopedics" || injuryType == 2)
                return ER_Room.RoomType.OrthopedicRoom;

            // Rule 6 — General Examination Room (default)
            return ER_Room.RoomType.GeneralRoom;
        }
    }
}