using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ElevatorSystem
{
    /// <summary>Shows the stored history. All DB work runs on a BackgroundWorker.</summary>
    public class LogForm : Form
    {
        readonly LogRepository repo;
        readonly DataGridView grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White };
        readonly Button btnRefresh = new Button { Text = "Refresh", Location = new Point(8, 8), Size = new Size(90, 28) };
        readonly Button btnClear = new Button { Text = "Clear log", Location = new Point(104, 8), Size = new Size(90, 28) };
        readonly Label lblStatus = new Label { Location = new Point(210, 14), AutoSize = true };

        public LogForm(LogRepository repo)
        {
            this.repo = repo;
            Text = "Elevator operation log (latest 500 records)";
            Size = new Size(1000, 480);
            StartPosition = FormStartPosition.CenterParent;
            var bar = new Panel { Dock = DockStyle.Top, Height = 44 };
            bar.Controls.AddRange(new Control[] { btnRefresh, btnClear, lblStatus });
            Controls.Add(grid);
            Controls.Add(bar);

            btnRefresh.Click += (s, e) => LoadLog();
            btnClear.Click += (s, e) =>
            {
                if (MessageBox.Show("Delete all log records?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                RunAsync<object>(() => { this.repo.Clear(); return null; }, _ => LoadLog());
            };
            Shown += (s, e) => LoadLog();
        }

        void LoadLog() => RunAsync(() => repo.GetLatest(500), table =>
        {
            grid.DataSource = table;
            grid.Columns["LogTime"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss.fff";
            grid.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            lblStatus.Text = table.Rows.Count + " record(s) loaded at " + DateTime.Now.ToString("HH:mm:ss");
        });

        // one helper for every database call made by this window (no duplicated worker code)
        void RunAsync<T>(Func<T> work, Action<T> done)
        {
            btnRefresh.Enabled = btnClear.Enabled = false;
            lblStatus.Text = "Working...";
            var w = new BackgroundWorker();
            w.DoWork += (s, e) => e.Result = work();
            w.RunWorkerCompleted += (s, e) =>
            {
                btnRefresh.Enabled = btnClear.Enabled = true;
                if (e.Error != null) lblStatus.Text = "Database error: " + e.Error.Message;
                else done((T)e.Result);
                w.Dispose();
            };
            w.RunWorkerAsync();
        }
    }
}
