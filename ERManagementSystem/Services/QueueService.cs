using System;
using System.Collections.Generic;
using System.Linq;
using ERManagementSystem.Models;
using ERManagementSystem.Repositories;
using ERManagementSystem.Helpers;

namespace ERManagementSystem.Services
{
    public class QueueService
    {
        private readonly ERVisitRepository _visitRepository;

        public QueueService(ERVisitRepository visitRepository)
        {
            _visitRepository = visitRepository;
        }

        /// <summary>
        /// Fetches all active visits with their triage data and orders them.
        /// </summary>
        public List<(ER_Visit visit, Triage triage)> GetOrderedQueue()
        {
            Logger.Info("[QueueService] Fetching active queue");

            try
            {
                var queueWithTriage = _visitRepository.GetActiveVisitsWithTriage();

                Logger.Info($"[QueueService] Retrieved {queueWithTriage.Count} active visits");

                return OrderByTriageLevelAndArrivalTime(queueWithTriage);
            }
            catch (Exception ex)
            {
                Logger.Error("[QueueService] Failed to fetch ordered queue", ex);
                throw;
            }
        }

        /// <summary>
        /// Orders the queue: triage level ascending, then arrival time ascending.
        /// </summary>
        private List<(ER_Visit visit, Triage triage)> OrderByTriageLevelAndArrivalTime(
            List<(ER_Visit visit, Triage triage)> queue)
        {
            Logger.Info("[QueueService] Ordering queue by triage level and arrival time");

            var ordered = queue
                .OrderBy(x => x.triage.Triage_Level)
                .ThenBy(x => x.visit.Arrival_date_time)
                .ToList();

            Logger.Info("[QueueService] Queue ordering completed");

            return ordered;
        }

        /// <summary>
        /// Removes a visit from the queue.
        /// </summary>
        public void RemoveFromQueue(int visitId)
        {
            Logger.Info($"[QueueService] Removing visit {visitId} from queue");

            try
            {
                var visit = _visitRepository.GetByVisitId(visitId);

                if (visit == null)
                {
                    Logger.Warning($"[QueueService] Visit {visitId} not found");
                    throw new InvalidOperationException($"Visit {visitId} not found.");
                }

                visit.Status = "IN_ROOM";
                _visitRepository.UpdateStatus(visitId, visit.Status);

                Logger.Info($"[QueueService] Visit {visitId} moved to IN_ROOM");
            }
            catch (Exception ex)
            {
                Logger.Error($"[QueueService] Failed to remove visit {visitId} from queue", ex);
                throw;
            }
        }


    }
}
