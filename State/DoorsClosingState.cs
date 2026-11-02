using System;
using System.Collections.Generic;

namespace ElevatorSystem
{
    public class DoorsClosingState : ElevatorState
    {
        public static readonly DoorsClosingState Instance = new DoorsClosingState();
        public override string Name => "Doors Closing";
        public override void Tick(Elevator e, double dt)
        {
            e.DoorOpen -= dt / Elevator.DoorSeconds;
            if (e.DoorOpen <= 0) { e.DoorOpen = 0; e.SetState(IdleState.Instance); }
        }
        public override void Request(Elevator e, int floor)
        {
            if (floor == e.LastFloor) e.SetState(DoorsOpeningState.Instance);   // re-open
            else e.Enqueue(floor);
        }
        public override double TimeToClear(Elevator e) => e.DoorOpen * Elevator.DoorSeconds;
    }
}
