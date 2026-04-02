using System;
using System.Collections.Generic;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using ERManagementSystem.Repositories;

namespace ERManagementSystem.Services
{
    public class RoomManagementService
    {
        private readonly RoomRepository _roomRepository;

        public RoomManagementService(RoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public List<ER_Room> GetAvailableRooms() => _roomRepository.GetAvailableRooms();
        public List<ER_Room> GetOccupiedRooms()  => _roomRepository.GetOccupiedRooms();
        public List<ER_Room> GetCleaningRooms()  => _roomRepository.GetCleaningRooms();

        public void MarkRoomAsCleaning(int roomId)
        {
            ER_Room room = _roomRepository.GetById(roomId)
                ?? throw new InvalidOperationException($"Room {roomId} was not found.");

            if (room.Availability_Status != ER_Room.RoomStatus.Occupied)
                throw new InvalidOperationException(
                    $"Room {roomId} cannot be set to cleaning from '{room.Availability_Status}'. Must be 'occupied'.");

            room.UpdateAvailabilityStatus(ER_Room.RoomStatus.Cleaning);
            _roomRepository.UpdateAvailabilityStatus(roomId, ER_Room.RoomStatus.Cleaning);
            _roomRepository.ClearCurrentVisit(roomId);   // clear visit link so panel doesn't show stale data
            Logger.Info($"Room {roomId} set to cleaning.");
        }

        public void MarkRoomAsCleaned(int roomId)
        {
            ER_Room room = _roomRepository.GetById(roomId)
                ?? throw new InvalidOperationException($"Room {roomId} was not found.");

            if (room.Availability_Status != ER_Room.RoomStatus.Cleaning)
                throw new InvalidOperationException(
                    $"Room {roomId} cannot be marked as cleaned — current status is '{room.Availability_Status}', not 'cleaning'.");

            room.UpdateAvailabilityStatus(ER_Room.RoomStatus.Available);
            _roomRepository.UpdateAvailabilityStatus(roomId, ER_Room.RoomStatus.Available);
            Logger.Info($"Room {roomId} is now available.");
        }
    }
}
