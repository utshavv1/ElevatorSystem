using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ElevatorSystem
{
    // Layout/controls: MainForm.Designer.cs (open MainForm.cs [Design] to move/resize/restyle everything). Behaviour: this file.
    public partial class MainForm : Form
    {
        readonly List<Elevator> elevators = new List<Elevator>();
        readonly Dispatcher dispatcher;
        readonly SimClock clock = new SimClock();
        readonly LogRepository repo;
        readonly LogService logService;
        readonly ToolTip tip = new ToolTip();
        readonly System.Windows.Forms.Timer blinkTimer = new System.Windows.Forms.Timer { Interval = 500 };
        readonly LightButton[] hallButtons;
        readonly ElevatorBinder[] scenes;
        bool blink;

        public MainForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            ApplyArt();
            hallButtons = new[] { btnHall0, btnHall1 };

            repo = new LogRepository();                       // SqlConnection string: LogRepository.DefaultConnectionString
            logService = new LogService(repo);
            logService.StatusChanged += SetDbStatus;

            elevators.Add(new Elevator(0));
            elevators.Add(new Elevator(1));
            dispatcher = new Dispatcher(elevators);
            dispatcher.HallLightsChanged += RefreshHallLights;

            scenes = new[]
            {
                new ElevatorBinder(elevators[0], carA, leafLA, leafRA, lblCarHdrA, lblSosA, dispA0, dispA1),
                new ElevatorBinder(elevators[1], carB, leafLB, leafRB, lblCarHdrB, lblSosB, dispB0, dispB1)
            };
            new CarPanelBinder(elevators[0], lblPTitleA, lblPDisplayA, lblPStateA, lblPTimerA, btnPF0A, btnPF1A, btnPSosA, btnPResetA);
            new CarPanelBinder(elevators[1], lblPTitleB, lblPDisplayB, lblPStateB, lblPTimerB, btnPF0B, btnPF1B, btnPSosB, btnPResetB);

            for (int f = 0; f < Elevator.Floors; f++)
            {
                int floor = f;
                hallButtons[f].Click += (s, e) => dispatcher.RequestHall(floor);
            }

            foreach (var e in elevators)
            {
                clock.Tick += e.Tick;                       // delegate-driven animation
                e.Attach(new LogObserver(logService));      // observer -> database
            }

            blinkTimer.Tick += (s, e) =>
            {
                blink = !blink;
                lblClock.Text = DateTime.Now.ToString("yyyy-MM-dd  HH:mm:ss");
                foreach (var sc in scenes) sc.SetBlink(blink);
            };

            Shown += (s, e) =>
            {
                logService.Start();
                foreach (var el in elevators) el.Notify(ElevatorEventType.Info, "Elevator ready at Floor 0, doors closed");
                clock.Start();
                blinkTimer.Start();
                CheckDatabase();
            };
            FormClosing += (s, e) =>
            {
                clock.Stop();
                blinkTimer.Stop();
                logService.Dispose();
            };
        }

        // Put the pictures (loaded from the Images folder next to the exe) on the designer controls.
        void ApplyArt()
        {
            foreach (Control c in new Control[] { carA, fdoorA0, fdoorA1, carB, fdoorB0, fdoorB1 }) Skin(c, Assets.ElevatorFrame);
            foreach (Control c in new Control[] { leafLA, leafRA, fareaA0, fareaA1, leafLB, leafRB, fareaB0, fareaB1 }) Skin(c, Assets.Brushed);
        }

        static void Skin(Control c, Image img)
        {
            if (img == null) return;
            c.BackgroundImage = img;
            c.BackgroundImageLayout = ImageLayout.Stretch;
        }

        void btnLog_Click(object sender, EventArgs e)
        {
            using (var f = new LogForm(repo)) f.ShowDialog(this);
        }

        void RefreshHallLights()
        {
            for (int f = 0; f < Elevator.Floors; f++) hallButtons[f].Lit = dispatcher.IsHallLit(f);
        }

        void CheckDatabase()
        {
            var w = new BackgroundWorker();
            w.DoWork += (s, e) => e.Result = repo.TestConnection();
            w.RunWorkerCompleted += (s, e) =>
            {
                string err = e.Error != null ? e.Error.Message : (string)e.Result;
                SetDbStatus(err == null, err == null ? "Database: connected (ElevatorDB)" : "Database unavailable - logging to Logs/fallback_log.txt (" + err + ")");
                w.Dispose();
            };
            w.RunWorkerAsync();
        }

        void SetDbStatus(bool ok, string message)
        {
            lblDb.Text = message;
            tip.SetToolTip(lblDb, message);
            lblDb.ForeColor = ok ? Color.DarkGreen : Color.Firebrick;
        }
    }
}
