using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;

namespace ERManagementSystem.Repositories
{
    public class RoomRepository
    {
        private readonly SqlHelper _sqlHelper;

        public RoomRepository(SqlHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        // ── Read ─────────────────────────────────────────────────────────────

        public List<ER_Room> GetAllRooms()
        {
            const string query = @"
                SELECT Room_ID, Room_Type, Availability_Status
                FROM   dbo.ER_Room";

            var rooms = new List<ER_Room>();
            using var reader = _sqlHelper.ExecuteReader(query);
            while (reader.Read())
                rooms.Add(MapReaderToRoom(reader));

            return rooms;
        }

        public ER_Room? GetById(int roomId)
        {
            const string query = @"
                SELECT Room_ID, Room_Type, Availability_Status
                FROM   dbo.ER_Room
                WHERE  Room_ID = @Room_ID";

            var parameters = new[] { new SqlParameter("@Room_ID", roomId) };

            using var reader = _sqlHelper.ExecuteReader(query, parameters);
            if (reader.Read())
                return MapReaderToRoom(reader);

            return null;
        }

        public List<ER_Room> GetAvailableRooms()
            => GetRoomsByStatus(ER_Room.RoomStatus.Available);

        public List<ER_Room> GetOccupiedRooms()
            => GetRoomsByStatus(ER_Room.RoomStatus.Occupied);

        public List<ER_Room> GetCleaningRooms()
            => GetRoomsByStatus(ER_Room.RoomStatus.Cleaning);

        public List<ER_Room> GetRoomsByStatus(string status)
        {
            const string query = @"
                SELECT Room_ID, Room_Type, Availability_Status
                FROM   dbo.ER_Room
                WHERE  Availability_Status = @Status";

            var parameters = new[] { new SqlParameter("@Status", status) };

            var rooms = new List<ER_Room>();
            using var reader = _sqlHelper.ExecuteReader(query, parameters);
            while (reader.Read())
                rooms.Add(MapReaderToRoom(reader));

            return rooms;
        }

        // ── Write ─────────────────────────────────────────────────────────────

        public void UpdateAvailabilityStatus(int roomId, string newStatus)
        {
            const string query = @"
                UPDATE dbo.ER_Room
                SET    Availability_Status = @Status
                WHERE  Room_ID = @Room_ID";

            var parameters = new[]
            {
                new SqlParameter("@Status",  newStatus),
                new SqlParameter("@Room_ID", roomId)
            };

            _sqlHelper.ExecuteNonQuery(query, parameters);
        }

        // ── Mapping ──────────────────────────────────────────────────────────

        private static ER_Room MapReaderToRoom(SqlDataReader reader) => new ER_Room
        {
            Room_ID             = Convert.ToInt32(reader["Room_ID"]),
            Room_Type           = reader["Room_Type"].ToString()!,
            Availability_Status = reader["Availability_Status"].ToString()!
        };
    }
}
