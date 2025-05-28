using DesktopPet.Utils;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopPet.Forms
{
    public partial class PetForm : Form
    {
        private bool isClickThroughEnabled = true;
        private bool followActivateWindow = true;
        private const int HOTKEY_ID = 9000;
        private IntPtr lastWindow = IntPtr.Zero;
        private CancellationTokenSource moveCancellation;
        private Point origin;
        private Point targetPosition;
        private System.Windows.Forms.Timer behaviorTimer = new System.Windows.Forms.Timer();
        private AnimationBox pet;
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem toggleWindowFocusItem;
        private ToolStripMenuItem exitItem;

        public PetForm()
        {
            InitializeComponent();
            InitializeForm();
            InitializePet();
            InitializeContextMenu();
        }
        private void InitializeForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.KeyPreview = true;
        }
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "PetForm";
            this.Text = "DesktopPet";
            this.Load += new System.EventHandler(this.PetForm_Load);
            this.ResumeLayout(false);
        }
        private void InitializePet()
        {
            pet = new AnimationBox()
            {
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.AutoSize,
                Location = new Point(0, 0)
            };
            this.Controls.Add(pet);
        }
        private void InitializeContextMenu()
        {
            contextMenu = new ContextMenuStrip();
            toggleWindowFocusItem = new ToolStripMenuItem("Follow focused window", null, ToggleWindowFocusItem);
            exitItem = new ToolStripMenuItem("Salir", null, (s,e) => this.Close());

            contextMenu.Items.AddRange(new ToolStripItem[]
            {
                toggleWindowFocusItem,
                new ToolStripSeparator(),
                exitItem
            });
            this.ContextMenuStrip = contextMenu;
        }
        private void PetForm_Load(object sender, EventArgs e)
        {
            pet.StartAnimation(SpriteUtils.Animations.Idle,150);

            this.Size = pet.Image != null ? pet.Image.Size : new Size(64, 48);

            EnableClickThrough();
            this.Size = pet.Size;
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            origin = new Point(workingArea.Width - pet.Width, workingArea.Height - pet.Height);
            this.Location = origin;
            behaviorTimer.Interval = 1000;
            behaviorTimer.Tick += BehaviorTick;
            behaviorTimer.Start();
            bool success = WinAPI.RegisterHotKey(this.Handle, HOTKEY_ID, WinAPI.MOD_CTRL, (int)Keys.F12);

            if (success)
                Debug.WriteLine("Hotkey registered successfully.");
            else
                Debug.WriteLine("Failed to register hotkey.");
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            WinAPI.UnregisterHotKey(this.Handle, HOTKEY_ID);
            base.OnFormClosing(e);
        }
        private async void BehaviorTick(object sender, EventArgs e)
        {
            Debug.WriteLine("Behavior tick triggered.");
            if (!followActivateWindow) return;
            moveCancellation?.Cancel();
            moveCancellation = new CancellationTokenSource();

            IntPtr hWnd = WinAPI.GetForegroundWindow();
            if(hWnd == IntPtr.Zero || hWnd == this.Handle ) return;
            
            WinAPI.RECT rect;

            if(lastWindow == hWnd && IsInCorner(targetPosition))
            {
                Debug.WriteLine("Already in corner, skipping move.");
                pet.StartAnimation(SpriteUtils.Animations.Trick, (int)SpriteUtils.Intervals.Trick);
                return;
            }

            if (WinAPI.GetWindowRect(hWnd, out rect))
            {
                Rectangle winRect = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
                Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
                int height = workingArea.Bottom - this.Height;
                int petX = this.Location.X;
                int distToLeft = Math.Abs(petX - winRect.Left);
                int distToRight = Math.Abs(petX - winRect.Right);
                int target = (distToLeft < distToRight) ? winRect.Left : winRect.Right - this.Width;
                target = Math.Max(workingArea.Left, Math.Min(target, workingArea.Right - this.Width));
                targetPosition = new Point(target, height);
                try
                {
                    Debug.WriteLine("Starting to run");
                    lastWindow = hWnd;
                    await RunTo(targetPosition, moveCancellation.Token);
                }
                catch (OperationCanceledException ocException)
                {
                    Debug.WriteLine("Move cancelled token: " + ocException.CancellationToken);
                }
            }
        }
        private async void ToggleWindowFocusItem(object sender, EventArgs e)
        {
            followActivateWindow = !followActivateWindow;
            toggleWindowFocusItem.Checked = followActivateWindow;
            Debug.WriteLine("Follow active window: " + followActivateWindow);
            if (followActivateWindow)
                behaviorTimer.Start();
            else
            {
                behaviorTimer.Stop();
                await RunTo(origin, CancellationToken.None);
                pet.StartAnimation(SpriteUtils.Animations.Sit, (int)SpriteUtils.Intervals.Sit);
            }
                
        }
        public void EnableClickThrough()
        {
            int wl = WinAPI.GetWindowLong(this.Handle, WinAPI.GWL_EXSTYLE);
            wl |= WinAPI.WS_EX_LAYERED | WinAPI.WS_EX_TRANSPARENT;
            WinAPI.SetWindowLong(this.Handle, WinAPI.GWL_EXSTYLE, wl);
            isClickThroughEnabled = true;
            Debug.WriteLine("Click-through enabled: " + isClickThroughEnabled);
        }
        public void DisableClickThrough()
        {
            int wl = WinAPI.GetWindowLong(this.Handle, WinAPI.GWL_EXSTYLE);
            wl &= ~WinAPI.WS_EX_TRANSPARENT;
            wl |= WinAPI.WS_EX_LAYERED;
            WinAPI.SetWindowLong(this.Handle, WinAPI.GWL_EXSTYLE, wl);
            WinAPI.SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0, WinAPI.SWP_NOMOVE | WinAPI.SWP_NOSIZE | WinAPI.SWP_NOZORDER | WinAPI.SWP_FRAMECHANGED);
            this.Invalidate();
            isClickThroughEnabled = false;
            Debug.WriteLine("Click-through disabled: " + isClickThroughEnabled);
        }
        private async Task RunTo(Point target, CancellationToken token)
        {
            pet.StartAnimation(SpriteUtils.Animations.Run, (int)SpriteUtils.Intervals.Run);
            float speed = 2f;
            while (Math.Abs(this.Location.X - target.X) > speed)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                float direction = this.Location.X < target.X ? 1 * speed : -1 * speed;
                pet.FlipX = direction > 0;
                this.Location = new Point((int)(this.Location.X + direction), this.Location.Y);
                await Task.Delay(1);
            } 
            this.Location = target;
        }
        private bool IsInCorner(Point target)
        {
            return this.Location.X == target.X;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WinAPI.WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                if(isClickThroughEnabled)
                    DisableClickThrough();
                else
                    EnableClickThrough();
                int wl = WinAPI.GetWindowLong(this.Handle, WinAPI.GWL_EXSTYLE);
                Debug.WriteLine("Window styles: 0x" + wl.ToString("X"));
            }

            if (m.Msg == WinAPI.WM_NCHITTEST && !isClickThroughEnabled)
            {
                m.Result = (IntPtr)WinAPI.HTCLIENT;
                return;
            }
            base.WndProc(ref m);
        }
    }
}
