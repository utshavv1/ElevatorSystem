using System;
using System.Collections.Generic;

namespace ElevatorSystem
{
    public class DoorsOpeningState : ElevatorState
    {
        public static readonly DoorsOpeningState Instance = new DoorsOpeningState();
        public override string Name => "Doors Opening";
        public override void Enter(Elevator e) => e.Served(e.LastFloor);
        public override void Tick(Elevator e, double dt)
        {
            e.DoorOpen += dt / Elevator.DoorSeconds;
            if (e.DoorOpen >= 1) { e.DoorOpen = 1; e.SetState(DoorsOpenState.Instance); }
        }
        public override void Request(Elevator e, int floor) { if (floor != e.LastFloor) e.Enqueue(floor); }
        public override double TimeToClear(Elevator e) => (1 - e.DoorOpen) * Elevator.DoorSeconds + Elevator.DwellSeconds + Elevator.DoorSeconds;
    }
}
