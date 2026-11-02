using System;
using System.Collections.Generic;

namespace ElevatorSystem
{
    public class DoorsOpenState : ElevatorState
    {
        public static readonly DoorsOpenState Instance = new DoorsOpenState();
        public override string Name => "Doors Open";
        public override void Enter(Elevator e) => e.StateTimer = 0;
        public override void Tick(Elevator e, double dt)
        {
            e.StateTimer += dt;
            if (e.StateTimer >= Elevator.DwellSeconds) e.SetState(DoorsClosingState.Instance);
        }
        public override void Request(Elevator e, int floor)
        {
            if (floor == e.LastFloor) e.StateTimer = 0;   // keep doors open a bit longer
            else e.Enqueue(floor);
        }
        public override double TimeToClear(Elevator e) => Math.Max(0, Elevator.DwellSeconds - e.StateTimer) + Elevator.DoorSeconds;
    }
}
