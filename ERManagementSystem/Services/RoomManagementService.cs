using System;
using System.Collections.Generic;
using ERManagementSystem.Models;
using ERManagementSystem.Repositories;

namespace ERManagementSystem.Services
{
    /// <summary>
    /// Task 5.10 — Room Management Service (Feature 7).
    /// Manages room availability lifecycle: available → occupied → cleaning → available.
    /// </summary>
    public class RoomManagementService
    {
        private readonly RoomRepository _roomRepository;

        public RoomManagementService(RoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public List<ER_Room> GetAvailableRooms()
            => _roomRepository.GetAvailableRooms();

        public List<ER_Room> GetOccupiedRooms()
            => _roomRepository.GetOccupiedRooms();

        public List<ER_Room> GetCleaningRooms()
            => _roomRepository.GetCleaningRooms();

        /// <summary>
        /// Marks a room as available after cleaning is complete (cleaning → available).
        /// Validates the transition via the ER_Room model before persisting.
        /// </summary>
        public void MarkRoomAsCleaned(int roomId)
        {
            ER_Room room = _roomRepository.GetById(roomId)
                ?? throw new InvalidOperationException($"Room {roomId} was not found.");

            if (room.Availability_Status != ER_Room.RoomStatus.Cleaning)
                throw new InvalidOperationException(
                    $"Room {roomId} cannot be marked as cleaned " +
                    $"because its current status is '{room.Availability_Status}', not 'cleaning'.");

            // Validate via model state machine (cleaning → available)
            room.UpdateAvailabilityStatus(ER_Room.RoomStatus.Available);

            // Persist
            _roomRepository.UpdateAvailabilityStatus(roomId, ER_Room.RoomStatus.Available);
        }
    }
}
