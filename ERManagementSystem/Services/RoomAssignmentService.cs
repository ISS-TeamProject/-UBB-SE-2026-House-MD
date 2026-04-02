using System;
using System.Collections.Generic;
using System.Linq;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using ERManagementSystem.Repositories;

namespace ERManagementSystem.Services
{
    public class RoomAssignmentService
    {
        private readonly RoomRepository               _roomRepository;
        private readonly ERVisitRepository            _erVisitRepository;
        private readonly StateManagementService       _stateManagementService;
        private readonly TriageParametersRepository   _triageParamsRepository;

        public RoomAssignmentService(
            RoomRepository               roomRepository,
            ERVisitRepository            erVisitRepository,
            StateManagementService       stateManagementService,
            TriageParametersRepository   triageParamsRepository)
        {
            _roomRepository         = roomRepository;
            _erVisitRepository      = erVisitRepository;
            _stateManagementService = stateManagementService;
            _triageParamsRepository = triageParamsRepository;
        }

        public ER_Room? FindAvailableRoom(string requiredRoomType)
        {
            return _roomRepository.GetAvailableRooms()
                                  .FirstOrDefault(r => r.Room_Type == requiredRoomType);
        }

        public void AssignRoomToVisit(int visitId, int roomId)
        {
            ER_Room room = _roomRepository.GetById(roomId)
                ?? throw new InvalidOperationException($"Room {roomId} was not found.");

            if (room.Availability_Status != ER_Room.RoomStatus.Available)
                throw new InvalidOperationException(
                    $"Room {roomId} is not available (current: '{room.Availability_Status}').");

            ER_Visit visit = _erVisitRepository.GetByVisitId(visitId)
                ?? throw new InvalidOperationException($"Visit {visitId} was not found.");

            if (visit.Status != ER_Visit.VisitStatus.WAITING_FOR_ROOM)
                throw new InvalidOperationException(
                    $"Visit {visitId} is not in WAITING_FOR_ROOM (current: '{visit.Status}').");

            UpdateRoomAvailability(roomId, ER_Room.RoomStatus.Occupied);
            _roomRepository.SetCurrentVisit(roomId, visitId);
            _stateManagementService.ChangeVisitStatus(visitId, ER_Visit.VisitStatus.IN_ROOM);
            Logger.Info($"Visit {visitId} assigned to Room {roomId}.");
        }

        public void UpdateRoomAvailability(int roomId, string newStatus)
        {
            ER_Room room = _roomRepository.GetById(roomId)
                ?? throw new InvalidOperationException($"Room {roomId} was not found.");

            room.UpdateAvailabilityStatus(newStatus);
            _roomRepository.UpdateAvailabilityStatus(roomId, newStatus);
        }

        /// <summary>
        /// Auto-assign: picks the highest-priority WAITING_FOR_ROOM visit using
        /// QueueService ordering (triage level asc, arrival asc), determines room type
        /// from triage params, finds a matching available room, and assigns it.
        /// Uses ERVisitRepository.GetActiveVisitsWithTriage() — same data QueueService uses.
        /// </summary>
        public bool AutoAssignRoom()
        {
            // Get waiting visits with triage, ordered by priority (same as QueueService)
            var waitingWithTriage = _erVisitRepository.GetActiveVisitsWithTriage()
                .Where(x => x.visit.Status == ER_Visit.VisitStatus.WAITING_FOR_ROOM)
                .OrderBy(x => x.triage.Triage_Level)
                .ThenBy(x => x.visit.Arrival_date_time)
                .ToList();

            if (waitingWithTriage.Count == 0)
                return false;

            var (topVisit, topTriage) = waitingWithTriage.First();

            var parameters = _triageParamsRepository.GetByTriageId(topTriage.Triage_ID);
            
            // Defaulting parameters to 1 if missing for safety
            int bleeding = parameters?.Bleeding ?? 1;
            int injuryType = parameters?.Injury_Type ?? 1;
            int consciousness = parameters?.Consciousness ?? 1;
            int breathing = parameters?.Breathing ?? 1;

            string requiredType = RoomTypeHelper.DetermineRoomType(
                topTriage.Specialization, bleeding, injuryType, consciousness, breathing);

            ER_Room? room = FindAvailableRoom(requiredType);
            if (room == null)
            {
                Logger.Warning($"AutoAssignRoom: No '{requiredType}' room available for Visit {topVisit.Visit_ID}.");
                return false;
            }

            AssignRoomToVisit(topVisit.Visit_ID, room.Room_ID);
            return true;
        }
    }
}
