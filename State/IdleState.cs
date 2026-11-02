using System;
using System.Collections.Generic;

namespace ElevatorSystem
{
    public class IdleState : ElevatorState
    {
        public static readonly IdleState Instance = new IdleState();
        public override string Name => "Idle";
        public override void Tick(Elevator e, double dt)
        {
            int? next = e.TakeNext();
            if (next == null) return;
            if (e.AtFloor && next == e.LastFloor) { e.SetState(DoorsOpeningState.Instance); return; }
            e.Target = next;
            e.SetState(MovingState.Instance);
        }
        public override void Request(Elevator e, int floor)
        {
            if (e.AtFloor && floor == e.LastFloor) e.SetState(DoorsOpeningState.Instance);
            else base.Request(e, floor);
        }
    }
}
