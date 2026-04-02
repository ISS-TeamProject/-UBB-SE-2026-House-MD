using System;
using System.Collections.Generic;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using ERManagementSystem.Repositories;

namespace ERManagementSystem.Services
{
    public class StateManagementService
    {
        private readonly ERVisitRepository _erVisitRepository;

        public StateManagementService(ERVisitRepository erVisitRepository)
        {
            _erVisitRepository = erVisitRepository;
        }

        public bool CanTransitionTo(string currentStatus, string newStatus)
        {
            if (!ER_Visit.ValidTransitions.ContainsKey(currentStatus))
                return false;

            return ER_Visit.ValidTransitions[currentStatus].Contains(newStatus);
        }

        public void ChangeStatus(ER_Visit visit, string newStatus)
        {
            if (!CanTransitionTo(visit.Status, newStatus))
            {
                Logger.Warning($"Invalid transition rejected: Visit {visit.Visit_ID} " +
                               $"from '{visit.Status}' to '{newStatus}'. " +
                               $"Allowed: [{string.Join(", ", ER_Visit.ValidTransitions[visit.Status])}].");

                throw new InvalidOperationException(
                    $"Invalid transition: cannot move ER Visit {visit.Visit_ID} " +
                    $"from '{visit.Status}' to '{newStatus}'. " +
                    $"Allowed next states: [{string.Join(", ", ER_Visit.ValidTransitions[visit.Status])}].");
            }

            visit.Status = newStatus;
        }

        public bool ValidateTransition(string currentStatus, string newStatus)
            => CanTransitionTo(currentStatus, newStatus);

        public void ChangeVisitStatus(int visitId, string newStatus)
        {
            ER_Visit? visit = _erVisitRepository.GetByVisitId(visitId);

            if (visit == null)
            {
                Logger.Warning($"ChangeVisitStatus failed: Visit {visitId} not found.");
                throw new InvalidOperationException(
                    $"ER Visit with ID {visitId} was not found.");
            }

            string oldStatus = visit.Status;

            try
            {
                ChangeStatus(visit, newStatus);
                _erVisitRepository.UpdateStatus(visitId, newStatus);
                Logger.Info($"Visit {visitId} status changed: '{oldStatus}' → '{newStatus}'.");
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error($"Status change failed for Visit {visitId}.", ex);
                throw;
            }
        }

        

        private static readonly string[] AllowedClosingStates = new[]
        {
            ER_Visit.VisitStatus.IN_EXAMINATION,
            ER_Visit.VisitStatus.TRIAGED
        };

        public bool CanClose(ER_Visit visit)
        {
            return Array.Exists(AllowedClosingStates, s => s == visit.Status);
        }

        public void CloseVisit(int visitId)
        {
            ER_Visit? visit = _erVisitRepository.GetByVisitId(visitId);

            if (visit == null)
            {
                Logger.Warning($"CloseVisit failed: Visit {visitId} not found.");
                throw new InvalidOperationException(
                    $"ER Visit with ID {visitId} was not found.");
            }

            if (!CanClose(visit))
            {
                Logger.Warning($"CloseVisit rejected: Visit {visitId} is in '{visit.Status}'. " +
                               $"Allowed closing states: {string.Join(", ", AllowedClosingStates)}.");

                throw new InvalidOperationException(
                    $"Visit {visitId} cannot be closed from status '{visit.Status}'. " +
                    $"Allowed states: {string.Join(", ", AllowedClosingStates)}.");
            }

            ChangeVisitStatus(visitId, ER_Visit.VisitStatus.CLOSED);
            Logger.Info($"Visit {visitId} successfully closed.");
        }

        public List<ER_Visit> GetByStatus(string status)
        {
            return _erVisitRepository.GetByStatus(status);
        }
    }
}