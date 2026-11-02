using System;
using System.Collections.Generic;

namespace ElevatorSystem
{
    // ---------------- ELEVATOR (the "context" of the State pattern + the observable subject) ----------------
    public class Elevator
    {
        public const int Floors = 2;
        public const double TravelSeconds = 3.0;   // time to travel one floor
        public const double DoorSeconds = 1.0;     // time to open / close the doors
        public const double DwellSeconds = 3.0;    // doors stay open this long
        public const string FromCar = "Car panel";
        public const string FromHall = "Hall call";

        readonly List<IElevatorObserver> observers = new List<IElevatorObserver>();
        readonly List<int> queue = new List<int>();        // pending stops (FIFO - ongoing trips are never disrupted)
        readonly HashSet<int> carStops = new HashSet<int>(); // stops requested from the car panel (for button lights)

        public Elevator(int id) { Id = id; State = IdleState.Instance; }

        public int Id { get; }
        public string Name => $"Elevator {(char)('A' + Id)}";
        public ElevatorState State { get; private set; }

        public double Position { get; internal set; }   // 0.0 = Floor 0, 1.0 = Floor 1 (continuous while moving)
        public double DoorOpen { get; internal set; }   // 0.0 closed .. 1.0 fully open
        public int LastFloor { get; internal set; }     // last floor reached (what the displays show)
        public int? Target { get; internal set; }
        public int Direction { get; internal set; }     // +1 up, -1 down, 0 none
        public double StateTimer { get; internal set; }

        public bool IsEmergency => State is EmergencyState;
        public bool AtFloor => Math.Abs(Position - Math.Round(Position)) < 1e-9;
        public bool IsPending(int floor) => Target == floor || queue.Contains(floor);
        public bool IsCarLit(int floor) => carStops.Contains(floor) && IsPending(floor);

        public string LocationText
        {
            get
            {
                string place = AtFloor
                    ? $"at Floor {(int)Math.Round(Position)}"
                    : $"between Floor {(int)Math.Floor(Position)} and Floor {(int)Math.Ceiling(Position)} ({Position * 100:0}% of the way up)";
                return $"{place}, doors {DoorOpen * 100:0}% open";
            }
        }

        // ---- observer plumbing ----
        public void Attach(IElevatorObserver o) => observers.Add(o);
        public void Detach(IElevatorObserver o) => observers.Remove(o);
        public void Notify(ElevatorEventType type, string message = "")
        {
            var ev = new ElevatorEvent(type, message);
            foreach (var o in observers.ToArray()) o.OnElevatorEvent(this, ev);
        }

        // ---- public commands (delegated to the current state) ----
        public void Tick(double dt)
        {
            double p = Position, d = DoorOpen;
            State.Tick(this, dt);
            if (p != Position || d != DoorOpen || State is DoorsOpenState) Notify(ElevatorEventType.Animated);
        }

        public void Request(int floor, string source)
        {
            if (floor < 0 || floor >= Floors) throw new ArgumentOutOfRangeException(nameof(floor));
            if (IsEmergency)
            {
                Notify(ElevatorEventType.Info, $"{source} request for Floor {floor} ignored - SOS active");
                return;
            }
            if (source == FromCar) carStops.Add(floor);
            Notify(ElevatorEventType.Request, $"{source} request for Floor {floor}");
            State.Request(this, floor);
            if (!IsPending(floor)) carStops.Remove(floor);   // request was absorbed (e.g. doors re-opened)
        }

        public void PressSos()
        {
            if (IsEmergency) return;
            string where = LocationText;     // capture BEFORE the state changes
            Notify(ElevatorEventType.Sos, $"SOS pressed - lift stopped {where}");
            SetState(EmergencyState.Instance);
        }

        public void ResumeFromSos()
        {
            if (!IsEmergency) return;
            State.Resume(this);
            Notify(ElevatorEventType.Resumed, $"SOS reset - {Name} resumes {LocationText}");
        }

        // ---- helpers used by the states / dispatcher ----
        internal void SetState(ElevatorState next)
        {
            var old = State;
            State = next;
            next.Enter(this);
            Notify(ElevatorEventType.StateChanged, $"{old.Name} -> {next.Name}");
        }
        internal void Enqueue(int floor) { if (!IsPending(floor)) queue.Add(floor); }
        internal int? TakeNext()
        {
            if (queue.Count == 0) return null;
            int f = queue[0]; queue.RemoveAt(0); return f;
        }
        internal void Served(int floor) => carStops.Remove(floor);

        // Used by the dispatcher to hand a hall call to another elevator when this one is stuck.
        public void CancelHallStop(int floor)
        {
            if (carStops.Contains(floor)) return;      // a passenger inside still wants it
            queue.Remove(floor);
            if (Target == floor) Target = null;
        }

        /// <summary>Estimated seconds until this elevator's doors open at 'floor' (queue is never re-ordered).</summary>
        public double EstimateArrival(int floor)
        {
            if (IsEmergency) return double.PositiveInfinity;
            var stops = new List<int>();
            if (Target.HasValue) stops.Add(Target.Value);
            stops.AddRange(queue);
            if (stops.Count == 0 && AtFloor && LastFloor == floor) return 0;

            double t = State.TimeToClear(this), pos = Position;
            foreach (int s in stops)
            {
                t += Math.Abs(pos - s) * TravelSeconds;
                if (s == floor) return t;
                t += 2 * DoorSeconds + DwellSeconds;
                pos = s;
            }
            return t + Math.Abs(pos - floor) * TravelSeconds;
        }
    }
}
