using System;
using System.Collections.Generic;
using System.Linq;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using ERManagementSystem.Repositories;
using ERManagementSystem.Services;

namespace ERManagementSystem.Services
{
    /// <summary>
    /// Tasks 5.5 + 5.6 — Room Assignment Service (Feature 6).
    /// Handles automatic and manual room assignment.
    /// Uses ERVisitRepository.GetByStatus("WAITING_FOR_ROOM") as a stub for the
    /// priority-sorted queue — replace with Mihai's (member 3) queue service when ready.
    /// </summary>
    public class RoomAssignmentService
    {
        private readonly RoomRepository           _roomRepository;
        private readonly ERVisitRepository        _erVisitRepository;
        private readonly StateManagementService   _stateManagementService;

        public RoomAssignmentService(
            RoomRepository         roomRepository,
            ERVisitRepository      erVisitRepository,
            StateManagementService stateManagementService)
        {
            _roomRepository         = roomRepository;
            _erVisitRepository      = erVisitRepository;
            _stateManagementService = stateManagementService;
        }

        // ── Task 5.5 ─────────────────────────────────────────────────────────

        /// <summary>
        /// Finds the first available room matching the required room type.
        /// Returns null if no suitable room exists.
        /// </summary>
        public ER_Room? FindAvailableRoom(string requiredRoomType)
        {
            List<ER_Room> availableRooms = _roomRepository.GetAvailableRooms();
            return availableRooms.FirstOrDefault(r => r.Room_Type == requiredRoomType);
        }

        /// <summary>
        /// Assigns a specific room to a specific visit:
        /// marks the room as occupied and transitions the visit to IN_ROOM.
        /// </summary>
        public void AssignRoomToVisit(int visitId, int roomId)
        {
            ER_Room? room = _roomRepository.GetById(roomId)
                ?? throw new InvalidOperationException($"Room {roomId} was not found.");

            if (room.Availability_Status != ER_Room.RoomStatus.Available)
                throw new InvalidOperationException(
                    $"Room {roomId} is not available (current status: '{room.Availability_Status}').");

            ER_Visit? visit = _erVisitRepository.GetByVisitId(visitId)
                ?? throw new InvalidOperationException($"Visit {visitId} was not found.");

            if (visit.Status != ER_Visit.VisitStatus.WAITING_FOR_ROOM)
                throw new InvalidOperationException(
                    $"Visit {visitId} is not in WAITING_FOR_ROOM status (current: '{visit.Status}').");

            // Mark room occupied first, then transition visit status
            UpdateRoomAvailability(roomId, ER_Room.RoomStatus.Occupied);
            _stateManagementService.ChangeVisitStatus(visitId, ER_Visit.VisitStatus.IN_ROOM);
        }

        /// <summary>
        /// Updates the availability status of a room in both the model and the database.
        /// Enforces the ER_Room state-machine rules (available→occupied→cleaning→available).
        /// </summary>
        public void UpdateRoomAvailability(int roomId, string newStatus)
        {
            ER_Room room = _roomRepository.GetById(roomId)
                ?? throw new InvalidOperationException($"Room {roomId} was not found.");

            // Validate the transition through the model's state-machine
            room.UpdateAvailabilityStatus(newStatus);

            // Persist
            _roomRepository.UpdateAvailabilityStatus(roomId, newStatus);
        }

        // ── Task 5.6 ─────────────────────────────────────────────────────────

        /// <summary>
        /// Full automatic assignment flow (Feature 6):
        /// 1. Select the highest-priority waiting visit.
        /// 2. Determine the required room type (using triage parameters if available).
        /// 3. Find a matching available room.
        /// 4. Assign it. If no room matches, the visit stays in WAITING_FOR_ROOM.
        /// Returns true if a room was successfully assigned, false otherwise.
        /// </summary>
        /// <param name="specialization">Triage specialization for the visit (from Triage table).</param>
        /// <param name="bleeding">Bleeding severity 1–3.</param>
        /// <param name="injuryType">Injury type severity 1–3.</param>
        /// <param name="consciousness">Consciousness level 1–3.</param>
        /// <param name="breathing">Breathing status 1–3.</param>
        public bool AutoAssignRoom(
            string? specialization = null,
            int bleeding    = 1,
            int injuryType  = 1,
            int consciousness = 1,
            int breathing   = 1)
        {
            // Step 1 — Get highest-priority waiting visit
            // STUB: sorted by Triage_Level desc, Arrival_date_time asc via ERVisitRepository.
            // TODO (member 3 - Mihai): replace with QueueService.GetHighestPriorityVisit() when ready.
            List<ER_Visit> waitingVisits = _erVisitRepository.GetByStatus(ER_Visit.VisitStatus.WAITING_FOR_ROOM);
            if (waitingVisits.Count == 0)
                return false;

            // Without Mihai's triage-level sort, fall back to earliest arrival
            ER_Visit topVisit = waitingVisits.OrderBy(v => v.Arrival_date_time).First();

            // Step 2 — Determine required room type
            string requiredType = RoomTypeHelper.DetermineRoomType(
                specialization, bleeding, injuryType, consciousness, breathing);

            // Step 3 — Find a matching available room
            ER_Room? room = FindAvailableRoom(requiredType);
            if (room == null)
                return false; // Patient stays in WAITING_FOR_ROOM

            // Step 4 — Assign
            AssignRoomToVisit(topVisit.Visit_ID, room.Room_ID);
            return true;
        }
    }
}
