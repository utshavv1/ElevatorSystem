using System;
using System.Collections.Generic;

namespace ElevatorSystem
{
    public class ElevatorEvent
    {
        public ElevatorEventType Type { get; }
        public string Message { get; }
        public ElevatorEvent(ElevatorEventType type, string message = "") { Type = type; Message = message; }
        // "Animated" fires every 30 ms for redraws only - it is never written to the database.
        public bool ShouldBeLogged => Type != ElevatorEventType.Animated;
    }
}
