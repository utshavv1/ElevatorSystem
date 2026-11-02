using System;
using System.Collections.Generic;
using System.Linq;

namespace ElevatorSystem
{
    /// <summary>Observes all elevators and assigns each outside request to the elevator with the shortest estimated response time.</summary>
    public class Dispatcher : IElevatorObserver
    {
        readonly Elevator[] assigned = new Elevator[Elevator.Floors];
        readonly bool[] hallLit = new bool[Elevator.Floors];

        public IReadOnlyList<Elevator> Elevators { get; }
        public event Action HallLightsChanged;

        public Dispatcher(IEnumerable<Elevator> elevators)
        {
            Elevators = elevators.ToList();
            foreach (var e in Elevators) e.Attach(this);
        }

        public bool IsHallLit(int floor) => hallLit[floor];

        public void RequestHall(int floor)
        {
            if (hallLit[floor]) return;            // already being served
            hallLit[floor] = true;
            HallLightsChanged?.Invoke();
            Assign(floor);
            if (assigned[floor] != null) ClearServed(assigned[floor]);
        }

        void Assign(int floor)
        {
            var best = Elevators.Where(e => !e.IsEmergency)
                                .OrderBy(e => e.EstimateArrival(floor)).ThenBy(e => e.Id)
                                .FirstOrDefault();
            assigned[floor] = best;
            best?.Request(floor, Elevator.FromHall);
        }

        // Hall light goes off once the assigned elevator has its doors opening/open at that floor.
        void ClearServed(Elevator e)
        {
            bool changed = false;
            for (int f = 0; f < Elevator.Floors; f++)
            {
                if (hallLit[f] && assigned[f] == e && e.AtFloor && e.LastFloor == f &&
                    (e.State is DoorsOpeningState || e.State is DoorsOpenState))
                { hallLit[f] = false; assigned[f] = null; changed = true; }
            }
            if (changed) HallLightsChanged?.Invoke();
        }

        public void OnElevatorEvent(Elevator e, ElevatorEvent ev)
        {
            if (ev.Type == ElevatorEventType.StateChanged)
            {
                if (e.IsEmergency)   // stuck elevator: give its hall calls to someone else
                    for (int f = 0; f < Elevator.Floors; f++)
                        if (hallLit[f] && assigned[f] == e) { e.CancelHallStop(f); Assign(f); }
                ClearServed(e);
            }
            else if (ev.Type == ElevatorEventType.Resumed)
            {
                for (int f = 0; f < Elevator.Floors; f++)
                    if (hallLit[f] && assigned[f] == null) Assign(f);   // calls nobody could take
            }
        }
    }
}
