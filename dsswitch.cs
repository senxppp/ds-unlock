using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DSSwitch
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ToggleForm());
        }
    }

    internal sealed class ToggleForm : Form
    {
        private const string ExePath = @"D:\ds-unlock\Cyberpunk2077.exe";
        private const string ProcName = "Cyberpunk2077";
        private readonly Timer _poll = new Timer();
        private bool _on;

        public ToggleForm()
        {
            Text = "DS \u5f00\u5173";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(280, 180);
            BackColor = Color.FromArgb(250, 250, 252);
            DoubleBuffered = true;
            Cursor = Cursors.Hand;

            _poll.Interval = 700;
            _poll.Tick += delegate { Sync(); };
            _poll.Start();

            MouseClick += delegate(object s, MouseEventArgs e) { if (HitSwitch(e.Location)) Toggle(); };
            Sync();
        }

        private static Rectangle SwitchRect
        {
            get { return new Rectangle(95, 52, 90, 40); }
        }

        private static bool HitSwitch(Point p)
        {
            return p.X >= 65 && p.X <= 215 && p.Y >= 30 && p.Y <= 115;
        }

        private static bool IsBaitRunning()
        {
            foreach (Process p in Process.GetProcessesByName(ProcName))
            {
                try
                {
                    if (string.Equals(p.MainModule.FileName, ExePath, StringComparison.OrdinalIgnoreCase)) return true;
                }
                catch { }
            }
            return false;
        }

        private void Sync()
        {
            bool r = IsBaitRunning();
            _on = r;
            Text = "DS \u5f00\u5173 - " + (_on ? "\u5df2\u5f00\u542f" : "\u5df2\u5173\u95ed");
            Invalidate();
        }

        private void Toggle()
        {
            if (_on)
            {
                foreach (Process p in Process.GetProcessesByName(ProcName))
                {
                    try
                    {
                        if (string.Equals(p.MainModule.FileName, ExePath, StringComparison.OrdinalIgnoreCase)) p.Kill();
                    }
                    catch { }
                }
            }
            else
            {
                try { Process.Start(ExePath); }
                catch (Exception ex) { MessageBox.Show("Failed: " + ex.Message); }
            }
            Sync();
        }

        private static GraphicsPath RoundRect(Rectangle r, int radius)
        {
            GraphicsPath gp = new GraphicsPath();
            int d = radius * 2;
            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            using (Font f = new Font("Microsoft YaHei UI", 12f, FontStyle.Bold))
            {
                g.DrawString("DS \u6a21\u5f0f", f, Brushes.DimGray, 24, 14);
            }

            Rectangle r = SwitchRect;
            Color track = _on ? Color.FromArgb(255, 105, 180) : Color.FromArgb(205, 205, 212);
            using (GraphicsPath gp = RoundRect(r, 20))
            using (SolidBrush b = new SolidBrush(track))
            {
                g.FillPath(b, gp);
            }

            int kx = _on ? r.Right - 38 : r.X + 2;
            using (SolidBrush kb = new SolidBrush(Color.White))
            {
                g.FillEllipse(kb, kx, r.Y + 2, 36, 36);
            }

            using (Font f2 = new Font("Microsoft YaHei UI", 9f))
            {
                string state = _on
                    ? "\u5df2\u5f00\u542f \u00b7 \u8bf7\u542f\u52a8\u6e38\u620f"
                    : "\u5df2\u5173\u95ed \u00b7 \u70b9\u51fb\u5f00\u542f DS \u6a21\u5f0f";
                g.DrawString(state, f2, Brushes.Gray, 24, 108);
                g.DrawString("\u70b9\u51fb\u5f00\u5173\u5207\u6362 \u00b7 \u5173\u95ed\u7a97\u53e3 = \u9000\u51fa\uff08\u72b6\u6001\u4fdd\u6301\uff09", f2, Brushes.LightGray, 24, 134);
            }
        }
    }
}
