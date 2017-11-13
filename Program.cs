using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace cmdmanager
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var left = LaunchCmd();
            var right = LaunchCmd();
            var bottom = LaunchCmd();

            Form mainForm = new MainForm(left, right, bottom);
            Application.Run(mainForm);
        }

        class MainForm : Form
        {
            internal MainForm(Process left, Process right, Process bottom)
            {
                Width = 1024;
                Height = 768;

                mProcesses.Add(left);
                mProcesses.Add(right);
                mProcesses.Add(bottom);

                Text = "fiera of the command line";

                this.BackColor = Color.LimeGreen;
                this.TransparencyKey = Color.LimeGreen;

                while (left.MainWindowHandle == IntPtr.Zero
                    || right.MainWindowHandle == IntPtr.Zero
                    || bottom.MainWindowHandle == IntPtr.Zero)
                    System.Threading.Thread.Sleep(100);

                /*// Remove WS_POPUP style and add WS_CHILD style
                const UInt32 WS_POPUP = 0x80000000;
                const UInt32 WS_CHILD = 0x40000000;

                uint style = (uint) GetWindowLong(left.MainWindowHandle, GWL_STYLE);
                style = (style & ~(WS_POPUP)) | WS_CHILD;
                SetWindowLong(left.MainWindowHandle, GWL_STYLE, style);*/

                SetParent(left.MainWindowHandle, this.Handle);
                SetParent(right.MainWindowHandle, this.Handle);
                SetParent(bottom.MainWindowHandle, this.Handle);

                System.Drawing.Rectangle r = Bounds;

                AdjustWindows(left.MainWindowHandle, right.MainWindowHandle, bottom.MainWindowHandle, r);

                this.Closing += delegate(object sender, System.ComponentModel.CancelEventArgs e)
                {
                    foreach (Process p in mProcesses)
                    {
                        if (!p.HasExited)
                            p.Kill();
                    }
                };
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);

                IntPtr left = mProcesses.Count >= 1 ? mProcesses[0].MainWindowHandle : IntPtr.Zero;
                IntPtr right = mProcesses.Count >= 2 ? mProcesses[1].MainWindowHandle : IntPtr.Zero;
                IntPtr bottom = mProcesses.Count >= 3 ? mProcesses[2].MainWindowHandle : IntPtr.Zero;

                AdjustWindows(left, right, bottom, Bounds);
            }

            static void AdjustWindows(IntPtr left, IntPtr right, IntPtr bottom, Rectangle r)
            {
                var halfWidth = r.Width / 2;
                var halfHeight = r.Height / 2;

                if (left != IntPtr.Zero)
                    MoveWindow(left, 0, 0, halfWidth, halfHeight, true);
                if (right != IntPtr.Zero)
                    MoveWindow(right, r.Width / 2, 0, halfWidth - 12, halfHeight, true);
                if (bottom != IntPtr.Zero)
                    MoveWindow(bottom, 0, r.Height / 2, r.Width - 12, halfHeight - 38, true);
            }

            List<Process> mProcesses = new List<Process>();

            const int GWL_STYLE = -16;
            const uint WS_VISIBLE = 0x10000000;
            const uint WS_BORDER = 0x00800000;

            [DllImport("user32.dll", SetLastError = true)]
            static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
            [DllImport("user32.dll", SetLastError = true)]
            static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);
            [DllImport("user32.dll")]
            static extern int SetWindowLong(IntPtr hWnd, int nIndex, UInt32 dwNewLong);
            [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
            static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        }

        static Process LaunchCmd()
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe");

            Process p = new Process();
            p.StartInfo = psi;

            p.Start();

            return p;
        }
    }
}

/*                Panel topPanel = new Panel();
                topPanel.BackColor = Color.Red;
                topPanel.Dock = DockStyle.Top;
                Splitter splitter = new Splitter();
                splitter.Dock = DockStyle.Top;

                Panel bottomPanel = new Panel();
                bottomPanel.Dock = DockStyle.Fill;
                bottomPanel.BackColor = Color.Blue;
 
                Controls.Add(topPanel);
                Controls.Add(splitter);
                Controls.Add(bottomPanel);
 
*/