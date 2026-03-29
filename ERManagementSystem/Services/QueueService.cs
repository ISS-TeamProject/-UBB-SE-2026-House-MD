using System;
using System.Collections.Generic;
using System.Linq;
using ERManagementSystem.Models;
using ERManagementSystem.Repositories;
using Microsoft.Data.SqlClient;

namespace ERManagementSystem.Services
{
    internal class QueueService
    {
        private readonly ERVisitRepository _visitRepository;
        private readonly StateManagementService _stateService;

        public QueueService(ERVisitRepository visitRepository, StateManagementService stateService)
        {
            _visitRepository = visitRepository;
            _stateService = stateService;
        }

        /// <summary>
        /// Moves all visits with status "TRIAGED" to "WAITING_FOR_ROOM".
        /// </summary>
        public void MoveTriagedToWaitingForRoom(List<(ER_Visit visit, Triage triage)> queue)
        {
            foreach (var (visit, triage) in queue)
            {
                if (visit.Status == "TRIAGED")
                {
                    _stateService.ChangeVisitStatus(visit.Visit_ID, "WAITING_FOR_ROOM");
                    visit.Status = "WAITING_FOR_ROOM";
                }
            }
        }

        /// <summary>
        /// Fetches all active visits with their triage data and orders them.
        /// </summary>
        public List<(ER_Visit visit, Triage triage)> GetOrderedQueue()
        {
           
            var queueWithTriage = _visitRepository.GetActiveVisitsWithTriage();

            MoveTriagedToWaitingForRoom(queueWithTriage);
            
            return OrderByTriageLevelAndArrivalTime(queueWithTriage);
        }

        /// <summary>
        /// Orders the queue: triage level ascending, then arrival time ascending.
        /// </summary>
        private List<(ER_Visit visit, Triage triage)> OrderByTriageLevelAndArrivalTime(
            List<(ER_Visit visit, Triage triage)> queue)
        {
            return queue
                .OrderBy(x => x.triage.Triage_Level) // 1 = highest priority
                .ThenBy(x => x.visit.Arrival_date_time)
                .ToList();
        }

        /// <summary>
        /// Removes a visit from the queue.
        /// </summary>
        public void RemoveFromQueue(int visitId)
        {
            // Depending on your workflow, "removing from queue" may mean:
            // 1) Mark visit as IN_ROOM
            // 2) Or remove from an in-memory queue (if you have one)
            // Here we assume the first approach: update status to IN_ROOM

            var visit = _visitRepository.GetByVisitId(visitId);
            if (visit == null)
                throw new InvalidOperationException($"Visit {visitId} not found.");

            visit.Status = "IN_ROOM"; // or whatever status indicates removal
            _visitRepository.UpdateStatus(visitId, visit.Status);
        }


    }
}
