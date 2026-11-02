using System;
using System.Collections.Generic;

namespace ElevatorSystem
{
    public class MovingState : ElevatorState
    {
        public static readonly MovingState Instance = new MovingState();
        public override string Name => "Moving";
        public override void Enter(Elevator e) => e.Direction = Math.Sign(e.Target.Value - e.Position);
        public override void Tick(Elevator e, double dt)
        {
            double step = dt / Elevator.TravelSeconds, goal = e.Target.Value;
            if (Math.Abs(goal - e.Position) <= step)
            {
                e.Position = goal;
                e.LastFloor = e.Target.Value;
                e.Direction = 0;
                e.Target = null;
                e.Notify(ElevatorEventType.FloorReached, $"Arrived at Floor {e.LastFloor}");
                e.SetState(DoorsOpeningState.Instance);
            }
            else e.Position += Math.Sign(goal - e.Position) * step;
        }
    }
}
