using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace ElevatorSystem
{
    /// <summary>Observer: drives the control panel inside one lift (display, floor buttons, SOS, reset, timer).</summary>
    public class CarPanelBinder : IElevatorObserver
    {
        readonly Elevator elevator;
        readonly Label display, state, timer;
        readonly LightButton[] floorButtons;
        readonly Button sos, reset;

        public CarPanelBinder(Elevator elevator, Label title, Label display, Label state, Label timer,
                              LightButton floor0, LightButton floor1, Button sos, Button reset)
        {
            this.elevator = elevator; this.display = display; this.state = state; this.timer = timer;
            this.sos = sos; this.reset = reset;
            floorButtons = new[] { floor0, floor1 };
            title.Text = "Control panel - " + elevator.Name;
            for (int f = 0; f < Elevator.Floors; f++)
            {
                int floor = f;
                floorButtons[f].Click += (s, e) => this.elevator.Request(floor, Elevator.FromCar);
            }
            sos.Click += (s, e) => this.elevator.PressSos();
            reset.Click += (s, e) => this.elevator.ResumeFromSos();
            elevator.Attach(this);
            Sync();
        }

        public void OnElevatorEvent(Elevator e, ElevatorEvent ev) => Sync();

        void Sync()
        {
            var e = elevator;
            string arrow = e.Direction > 0 ? " ▲" : e.Direction < 0 ? " ▼" : "";
            display.Text = "FLOOR " + e.LastFloor + arrow;
            state.Text = e.State.Name;
            state.ForeColor = e.IsEmergency ? Color.Red : Color.Black;
            for (int f = 0; f < Elevator.Floors; f++) floorButtons[f].Lit = e.IsCarLit(f);
            sos.Enabled = !e.IsEmergency;
            reset.Enabled = e.IsEmergency;

            if (e.State is MovingState && e.Target.HasValue)
                timer.Text = $"Trip timer: {Math.Abs(e.Target.Value - e.Position) * Elevator.TravelSeconds:0.0} s left";
            else if (e.State is DoorsOpenState)
                timer.Text = $"Doors close in: {Math.Max(0, Elevator.DwellSeconds - e.StateTimer):0.0} s";
            else if (e.IsEmergency)
                timer.Text = "Timers stopped (SOS)";
            else
                timer.Text = "Timer: waiting";
        }
    }
}
