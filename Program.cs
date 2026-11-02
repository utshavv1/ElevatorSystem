using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ElevatorSystem
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
                MessageBox.Show("Unexpected error: " + e.Exception.Message, "Elevator System", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Application.Run(new MainForm());
        }
    }
}
