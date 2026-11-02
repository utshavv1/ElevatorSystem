using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace ElevatorSystem
{
    /// <summary>Observer: moves the car, slides the door leaves and updates the floor displays of one shaft.
    /// The controls themselves are placed in the designer.</summary>
    public class ElevatorBinder : IElevatorObserver
    {
        const int Top = 40, FloorGap = 215;   // same numbers as the designer layout
        readonly Elevator elevator;
        readonly Control car, leafL, leafR, sosBadge;
        readonly Label carHeader, floorDisp0, floorDisp1;
        readonly int half;
        bool blink;

        public ElevatorBinder(Elevator elevator, Control car, Control leafL, Control leafR, Label carHeader, Control sosBadge, Label floorDisp0, Label floorDisp1)
        {
            this.elevator = elevator; this.car = car; this.leafL = leafL; this.leafR = leafR;
            this.carHeader = carHeader; this.sosBadge = sosBadge; this.floorDisp0 = floorDisp0; this.floorDisp1 = floorDisp1;
            half = leafL.Width;
            elevator.Attach(this);
            Sync();
        }

        public void SetBlink(bool value) { blink = value; Sync(); }
        public void OnElevatorEvent(Elevator e, ElevatorEvent ev) => Sync();

        void Sync()
        {
            var e = elevator;
            car.Top = (int)Math.Round(Top + (1 - e.Position) * FloorGap);
            int slide = (int)(half * e.DoorOpen);
            leafL.Left = -slide;
            leafR.Left = half + slide;
            string arrow = e.Direction > 0 ? "▲" : e.Direction < 0 ? "▼" : "";
            carHeader.Text = "F" + e.LastFloor;
            floorDisp0.Text = floorDisp1.Text = e.LastFloor + " " + arrow;
            sosBadge.Visible = e.IsEmergency && blink;
        }
    }
}
