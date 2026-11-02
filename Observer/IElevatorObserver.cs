using System;
using System.Collections.Generic;

namespace ElevatorSystem
{
    public interface IElevatorObserver
    {
        void OnElevatorEvent(Elevator elevator, ElevatorEvent ev);
    }
}
