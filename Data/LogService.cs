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
    /// <summary>Writes log entries on a BackgroundWorker so the GUI/animation never waits for the database.</summary>
    public class LogService : IDisposable
    {
        readonly LogRepository repo;
        readonly BlockingCollection<LogEntry> queue = new BlockingCollection<LogEntry>();
        readonly BackgroundWorker worker = new BackgroundWorker { WorkerReportsProgress = true };
        readonly ManualResetEventSlim finished = new ManualResetEventSlim(false);
        DateTime retryAfter = DateTime.MinValue;
        bool wasDown;

        /// <summary>(isOk, message) - raised on the UI thread.</summary>
        public event Action<bool, string> StatusChanged;

        public LogService(LogRepository repo)
        {
            this.repo = repo;
            worker.DoWork += (s, e) =>
            {
                try
                {
                    foreach (var first in queue.GetConsumingEnumerable())
                    {
                        var batch = new List<LogEntry> { first };
                        LogEntry more;
                        while (batch.Count < 50 && queue.TryTake(out more)) batch.Add(more);
                        Write(batch);
                    }
                }
                finally { finished.Set(); }
            };
            worker.ProgressChanged += (s, e) => StatusChanged?.Invoke(e.ProgressPercentage == 1, (string)e.UserState);
        }

        public void Start() => worker.RunWorkerAsync();
        public void Enqueue(LogEntry entry) { if (!queue.IsAddingCompleted) queue.Add(entry); }

        void Write(List<LogEntry> batch)
        {
            if (DateTime.UtcNow < retryAfter) { batch.ForEach(WriteFallback); return; }   // DB known to be down - don't stall
            try
            {
                repo.Insert(batch);
                if (wasDown) { wasDown = false; worker.ReportProgress(1, "Database reconnected"); }
            }
            catch (Exception ex)
            {
                retryAfter = DateTime.UtcNow.AddSeconds(10);
                batch.ForEach(WriteFallback);
                if (!wasDown) { wasDown = true; worker.ReportProgress(0, "Database unavailable - logging to Logs/fallback_log.txt (" + ex.Message + ")"); }
            }
        }

        static void WriteFallback(LogEntry l)
        {
            try
            {
                string dir = Path.Combine(AppContext.BaseDirectory, "Logs");   // relative to the exe, never absolute
                Directory.CreateDirectory(dir);
                File.AppendAllText(Path.Combine(dir, "fallback_log.txt"),
                    $"{l.Time:yyyy-MM-dd HH:mm:ss.fff} | {l.ElevatorName} | {l.EventType} | {l.StateName} | Floor {l.FloorNo} | {l.PositionPct}% | {l.Description}{Environment.NewLine}");
            }
            catch { /* last resort: never crash the elevator because of logging */ }
        }

        public void Dispose()
        {
            queue.CompleteAdding();
            finished.Wait(3000);   // flush what is left (max 3 s)
        }
    }
}
