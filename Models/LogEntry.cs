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
    public class LogEntry
    {
        public DateTime Time { get; set; }
        public string ElevatorName { get; set; }
        public string EventType { get; set; }
        public string StateName { get; set; }
        public int FloorNo { get; set; }
        public int PositionPct { get; set; }
        public string Description { get; set; }
    }
}
