using System;
using System.Collections.Generic;

namespace ElevatorSystem
{
    /// <summary>SOS: everything freezes exactly where it is (position, doors, target) until Reset.</summary>
    public class EmergencyState : ElevatorState
    {
        public static readonly EmergencyState Instance = new EmergencyState();
        public override string Name => "SOS Emergency Stop";
        public override void Request(Elevator e, int floor) { }   // ignored (Elevator.Request already logs it)
        public override void Resume(Elevator e)
        {
            if (e.AtFloor)
                e.SetState(e.DoorOpen > 0 ? (ElevatorState)DoorsClosingState.Instance : IdleState.Instance);
            else
            {
                if (e.Target == null) e.Target = e.TakeNext() ?? (int)Math.Round(e.Position);
                e.SetState(MovingState.Instance);   // continue from the exact stopped position
            }
        }
    }
}
