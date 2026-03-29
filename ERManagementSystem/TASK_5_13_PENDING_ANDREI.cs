// ============================================================
// TASK 5.13 — Auto-set room to cleaning on visit end
// ============================================================
// This file shows the ONLY change needed in StateManagementService.cs
// (Miruna's file). Add the marked section below to ChangeVisitStatus().
//
// BLOCKED ON: Andrei (member 4) — needs ExaminationRepository.GetRoomIdByVisitId()
// When Andrei is done: uncomment the TODO block below and add the
// ExaminationRepository and RoomRepository constructor parameters.
// ============================================================

/*
CHANGES TO: ERManagementSystem/Services/StateManagementService.cs

1. Add constructor parameters:
    private readonly RoomRepository _roomRepository;
    private readonly ERManagementSystem.Repositories.ExaminationRepository _examinationRepository; // Andrei's

    public StateManagementService(
        ERVisitRepository erVisitRepository,
        RoomRepository roomRepository,                                    // ADD
        ExaminationRepository examinationRepository)                      // ADD (Andrei)
    {
        _erVisitRepository      = erVisitRepository;
        _roomRepository         = roomRepository;                         // ADD
        _examinationRepository  = examinationRepository;                  // ADD
    }

2. Inside ChangeVisitStatus(), after _erVisitRepository.UpdateStatus():

    // ── Task 5.13: auto-set room to cleaning when visit ends ──────────
    // TODO (unblock when Andrei's ExaminationRepository is ready)
    if (newStatus == ER_Visit.VisitStatus.TRANSFERRED ||
        newStatus == ER_Visit.VisitStatus.CLOSED)
    {
        int? roomId = _examinationRepository.GetRoomIdByVisitId(visitId);
        if (roomId.HasValue)
        {
            try
            {
                ER_Room room = _roomRepository.GetById(roomId.Value)!;
                if (room.Availability_Status == ER_Room.RoomStatus.Occupied)
                {
                    room.UpdateAvailabilityStatus(ER_Room.RoomStatus.Cleaning);
                    _roomRepository.UpdateAvailabilityStatus(roomId.Value, ER_Room.RoomStatus.Cleaning);
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail the visit status change
                // LoggingUtility.Log(LogLevel.WARNING, $"Room auto-clean failed for visit {visitId}: {ex.Message}");
            }
        }
    }
    // ── End task 5.13 ────────────────────────────────────────────────
*/
