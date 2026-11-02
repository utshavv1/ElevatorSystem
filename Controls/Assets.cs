using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace ElevatorSystem
{
    /// <summary>Loads pictures using a path relative to the exe (portable).</summary>
    public static class Assets
    {
        public static readonly Image ElevatorFrame = LoadFrame();
        public static readonly Image Brushed = Load("brushed.jpg");

        static Image Load(string name)
        {
            try
            {
                string path = Path.Combine(AppContext.BaseDirectory, "Images", name);
                if (!File.Exists(path)) return null;
                using (var tmp = Image.FromFile(path)) return new Bitmap(tmp);   // copy so the file is not locked
            }
            catch { return null; }   // controls keep their design colours
        }

        // crop only the door frame out of the elevator picture (without the little call panel on the right)
        static Image LoadFrame()
        {
            var full = Load("elevator.jpg");
            if (full == null) return null;
            var src = new Rectangle((int)(full.Width * 0.144), (int)(full.Height * 0.108), (int)(full.Width * 0.641), (int)(full.Height * 0.758));
            var bmp = new Bitmap(src.Width, src.Height);
            using (var g = Graphics.FromImage(bmp)) g.DrawImage(full, new Rectangle(0, 0, src.Width, src.Height), src, GraphicsUnit.Pixel);
            full.Dispose();
            return bmp;
        }
    }
}
