using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace ElevatorSystem
{
    /// <summary>Observer that turns elevator events into log entries.</summary>
    public class LogObserver : IElevatorObserver
    {
        readonly LogService service;
        public LogObserver(LogService service) { this.service = service; }

        public void OnElevatorEvent(Elevator e, ElevatorEvent ev)
        {
            if (!ev.ShouldBeLogged) return;
            service.Enqueue(new LogEntry
            {
                Time = DateTime.Now,
                ElevatorName = e.Name,
                EventType = ev.Type.ToString(),
                StateName = e.State.Name,
                FloorNo = e.LastFloor,
                PositionPct = (int)Math.Round(e.Position * 100),
                Description = ev.Message
            });
        }
    }
}
