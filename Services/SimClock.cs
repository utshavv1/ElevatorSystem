using System;
using System.Collections.Generic;
using System.Linq;

namespace ElevatorSystem
{
    /// <summary>One WinForms timer drives every animation through a delegate.</summary>
    public class SimClock
    {
        readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 30 };
        readonly System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        double last;

        public event TickHandler Tick;

        public SimClock()
        {
            timer.Tick += (s, e) =>
            {
                double now = sw.Elapsed.TotalSeconds;
                double dt = Math.Min(now - last, 0.1);   // clamp so a UI stall never teleports the lift
                last = now;
                Tick?.Invoke(dt);
            };
        }
        public void Start() { last = sw.Elapsed.TotalSeconds; timer.Start(); }
        public void Stop() => timer.Stop();
    }
}
