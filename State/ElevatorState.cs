using System;
using System.Collections.Generic;

namespace ElevatorSystem
{
    // ---------------- STATE PATTERN ----------------
    public abstract class ElevatorState
    {
        public abstract string Name { get; }
        public virtual void Enter(Elevator e) { }
        public virtual void Tick(Elevator e, double dt) { }
        public virtual void Request(Elevator e, int floor) => e.Enqueue(floor);
        public virtual double TimeToClear(Elevator e) => 0;   // time until doors are closed & elevator is free
        public virtual void Resume(Elevator e) { }
    }
}
