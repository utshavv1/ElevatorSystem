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
    /// <summary>All SQL Server access lives here (one place for connection handling = no duplicated DB code).</summary>
    public class LogRepository
    {
        // Your SSMS / LocalDB connection string. It has no "Initial Catalog", so every query below names the
        // database explicitly (ElevatorDB.dbo.ElevatorLog) - run Database.sql once to create it.
        public const string DefaultConnectionString =
            "Data Source=(localdb)\\MSSQLLocalDB;Integrated Security=True;Persist Security Info=False;Pooling=False;" +
            "MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=True;" +
            "Application Name=\"SQL Server Management Studio\";Command Timeout=0";

        readonly string connectionString;
        public LogRepository(string connectionString = DefaultConnectionString) { this.connectionString = connectionString; }

        T Run<T>(Func<SqlConnection, T> work)
        {
            using (var c = new SqlConnection(connectionString))
            {
                c.Open();
                return work(c);
            }
        }

        public string TestConnection()
        {
            try
            {
                Run(c =>
                {
                    using (var cmd = new SqlCommand("SELECT TOP 0 Id FROM ElevatorDB.dbo.ElevatorLog", c)) return cmd.ExecuteScalar();
                });
                return null;
            }
            catch (Exception ex) { return ex.Message; }
        }

        // Disconnected model: rows are added to an in-memory DataTable and sent with SqlDataAdapter.Update().
        public void Insert(IEnumerable<LogEntry> entries) => Run(c =>
        {
            using (var adapter = new SqlDataAdapter(
                "SELECT LogTime, ElevatorName, EventType, StateName, FloorNo, PositionPct, Description FROM ElevatorDB.dbo.ElevatorLog WHERE 1 = 0", c))
            {
                var insert = new SqlCommand(
                    "INSERT INTO ElevatorDB.dbo.ElevatorLog (LogTime, ElevatorName, EventType, StateName, FloorNo, PositionPct, Description) " +
                    "VALUES (@t, @n, @e, @s, @f, @p, @d)", c);
                insert.Parameters.Add("@t", SqlDbType.DateTime2, 0, "LogTime");
                insert.Parameters.Add("@n", SqlDbType.NVarChar, 20, "ElevatorName");
                insert.Parameters.Add("@e", SqlDbType.NVarChar, 30, "EventType");
                insert.Parameters.Add("@s", SqlDbType.NVarChar, 30, "StateName");
                insert.Parameters.Add("@f", SqlDbType.Int, 0, "FloorNo");
                insert.Parameters.Add("@p", SqlDbType.Int, 0, "PositionPct");
                insert.Parameters.Add("@d", SqlDbType.NVarChar, 250, "Description");
                insert.UpdatedRowSource = UpdateRowSource.None;
                adapter.InsertCommand = insert;

                var table = new DataTable();
                adapter.Fill(table);                       // empty table with the right columns
                foreach (var l in entries)
                    table.Rows.Add(l.Time, l.ElevatorName, l.EventType, l.StateName, l.FloorNo, l.PositionPct,
                                   l.Description.Length > 250 ? l.Description.Substring(0, 250) : l.Description);
                return adapter.Update(table);              // one round trip, all new rows
            }
        });

        public DataTable GetLatest(int top = 500) => Run(c =>
        {
            using (var cmd = new SqlCommand(
                "SELECT TOP (@n) Id, LogTime, ElevatorName, EventType, StateName, FloorNo, PositionPct, Description " +
                "FROM ElevatorDB.dbo.ElevatorLog ORDER BY Id DESC", c))
            {
                cmd.Parameters.AddWithValue("@n", top);
                var table = new DataTable();
                using (var adapter = new SqlDataAdapter(cmd)) adapter.Fill(table);
                return table;
            }
        });

        public void Clear() => Run(c =>
        {
            using (var adapter = new SqlDataAdapter("SELECT Id FROM ElevatorDB.dbo.ElevatorLog", c))
            {
                var delete = new SqlCommand("DELETE FROM ElevatorDB.dbo.ElevatorLog WHERE Id = @id", c);
                delete.Parameters.Add("@id", SqlDbType.Int, 0, "Id");
                adapter.DeleteCommand = delete;
                var table = new DataTable();
                adapter.Fill(table);
                foreach (DataRow row in table.Rows) row.Delete();
                return adapter.Update(table);
            }
        });
    }
}
