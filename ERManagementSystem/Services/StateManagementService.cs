using System;
using System.Collections.Generic;
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
                throw new InvalidOperationException(
                    $"Invalid transition: cannot move ER Visit {visit.Visit_ID} " +
                    $"from '{visit.Status}' to '{newStatus}'. " +
                    $"Allowed next states: [{string.Join(", ", ER_Visit.ValidTransitions[visit.Status])}].");

            visit.Status = newStatus;
        }

        
        public bool ValidateTransition(string currentStatus, string newStatus)
            => CanTransitionTo(currentStatus, newStatus);

       
        public void ChangeVisitStatus(int visitId, string newStatus)
        {
            
            ER_Visit visit = _erVisitRepository.GetByVisitId(visitId);

            if (visit == null)
                throw new InvalidOperationException(
                    $"ER Visit with ID {visitId} was not found.");

            ChangeStatus(visit, newStatus);

            
            _erVisitRepository.UpdateStatus(visitId, newStatus);
        }
    }
}