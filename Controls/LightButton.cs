using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace ElevatorSystem
{
    /// <summary>Button with a built-in light (turns amber when lit, goes back to whatever BackColor you chose in the designer).</summary>
    public class LightButton : Button
    {
        bool lit, internalChange;
        Color normal = Color.Silver;

        public LightButton()
        {
            FlatStyle = FlatStyle.Flat;
            UseVisualStyleBackColor = false;
            BackColor = normal;
            TabStop = false;
        }

        [DefaultValue(false)]
        public bool Round { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Lit
        {
            get { return lit; }
            set
            {
                lit = value;
                internalChange = true;
                BackColor = lit ? Color.Gold : normal;
                internalChange = false;
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            if (!lit && !internalChange) normal = BackColor;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Round)
            {
                var p = new GraphicsPath();
                p.AddEllipse(0, 0, Width, Height);
                Region = new Region(p);
            }
        }
    }
}
