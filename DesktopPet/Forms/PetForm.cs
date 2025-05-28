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
        private const int HOTKEY_ID = 9000;
        private bool isClickThroughEnabled = true;
        private bool followActivateWindow = true;
        private bool freeMovement = false;
        private bool isBusy = false;
        private IntPtr lastWindow = IntPtr.Zero;
        private CancellationTokenSource moveCancellation;
        private Point origin;
        private Point targetPosition;
        private System.Windows.Forms.Timer behaviorTimer = new System.Windows.Forms.Timer();
        private AnimationBox pet;
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem toggleWindowFocusItem;
        private ToolStripMenuItem exitItem;
        private ToolStripMenuItem toggleFreeMovementItem;

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
            toggleFreeMovementItem = new ToolStripMenuItem("Free Movement", null, ToggleFreeMovementItem);
            exitItem = new ToolStripMenuItem("Exit", null, (s, e) => Environment.Exit(0));

            contextMenu.Items.AddRange(new ToolStripItem[]
            {
                toggleWindowFocusItem,
                toggleFreeMovementItem,
                new ToolStripSeparator(),
                exitItem
            });
            this.ContextMenuStrip = contextMenu;
            toggleWindowFocusItem.Checked = followActivateWindow;
            toggleFreeMovementItem.Checked = freeMovement;
        }
        private void PetForm_Load(object sender, EventArgs e)
        {
            pet.StartAnimation(ResourcesUtils.Animations.Idle, 150);

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
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            WinAPI.UnregisterHotKey(this.Handle, HOTKEY_ID);
            base.OnFormClosing(e);
        }
        private async void BehaviorTick(object sender, EventArgs e)
        {
            if (isBusy) return;
            isBusy = true;
            try
            {
                if (!verifyTime())
                {
                    if (IsInTarget(origin))
                        pet.StartAnimation(ResourcesUtils.Animations.Sleep, (int)ResourcesUtils.Intervals.Sleep);
                    else
                        await WalkTo(origin, CancellationToken.None);
                    return;
                }
                if (followActivateWindow)
                {
                    IntPtr hWnd = WinAPI.GetForegroundWindow();
                    if (hWnd == IntPtr.Zero || hWnd == this.Handle) return;

                    moveCancellation?.Cancel();
                    moveCancellation = new CancellationTokenSource();

                    if (lastWindow == hWnd && IsInTarget(targetPosition))
                    {
                        pet.StartAnimation(ResourcesUtils.Animations.Trick, (int)ResourcesUtils.Intervals.Trick);
                        return;
                    }

                    WinAPI.RECT rect;

                    if (WinAPI.GetWindowRect(hWnd, out rect))
                    {
                        setTarget(rect);
                        if (!followActivateWindow) return;
                        try
                        {
                            lastWindow = hWnd;
                            await RunTo(targetPosition, moveCancellation.Token);
                        }
                        catch (OperationCanceledException ocException)
                        {
                            MessageBox.Show("Movimiento cancelado: " + ocException.Message, "Movimiento Cancelado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Environment.Exit(0);
                        }
                    }
                }
                else if (freeMovement)
                {
                    await PerformRandomBehavior();
                }
                else
                {
                    if (!IsInTarget(origin))
                        await WalkTo(origin, CancellationToken.None);
                    else
                        pet.StartAnimation(ResourcesUtils.Animations.Sit, (int)ResourcesUtils.Intervals.Sit);
                }
            }
            finally
            {
                isBusy = false;
            }
        }
        private async void ToggleWindowFocusItem(object sender, EventArgs e)
        {
            freeMovement = false;
            followActivateWindow = !followActivateWindow;
            toggleWindowFocusItem.Checked = followActivateWindow;
            toggleFreeMovementItem.Checked = freeMovement;
        }
        private async void ToggleFreeMovementItem(object sender, EventArgs e)
        {
            followActivateWindow = false;
            toggleWindowFocusItem.Checked = followActivateWindow;
            toggleFreeMovementItem.Checked = freeMovement;
        }
        public void EnableClickThrough()
        {
            int wl = WinAPI.GetWindowLong(this.Handle, WinAPI.GWL_EXSTYLE);
            wl |= WinAPI.WS_EX_LAYERED | WinAPI.WS_EX_TRANSPARENT;
            WinAPI.SetWindowLong(this.Handle, WinAPI.GWL_EXSTYLE, wl);
            isClickThroughEnabled = true;
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
        }
        private async Task RunTo(Point target, CancellationToken token)
        {
            pet.StartAnimation(ResourcesUtils.Animations.Run, (int)ResourcesUtils.Intervals.Run);
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
        private bool IsInTarget(Point target)
        {
            return this.Location.X == target.X;
        }
        private async Task WalkTo(Point target, CancellationToken token)
        {
            pet.StartAnimation(ResourcesUtils.Animations.Walk, (int)ResourcesUtils.Intervals.Walk);
            float speed = 1f;
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
        private void setTarget(WinAPI.RECT rect)
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
        }
        private bool verifyTime()
        {
            DateTime now = DateTime.Now;
            TimeSpan currentTime = now.TimeOfDay;
            TimeSpan startTime = new TimeSpan(6, 0, 0);
            TimeSpan endTime = new TimeSpan(24, 0, 0);
            return currentTime >= startTime && currentTime <= endTime;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WinAPI.WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                if (isClickThroughEnabled)
                    DisableClickThrough();
                else
                    EnableClickThrough();
                int wl = WinAPI.GetWindowLong(this.Handle, WinAPI.GWL_EXSTYLE);
            }

            if (m.Msg == WinAPI.WM_NCHITTEST && !isClickThroughEnabled)
            {
                m.Result = (IntPtr)WinAPI.HTCLIENT;
                return;
            }
            base.WndProc(ref m);
        }
        private async Task PerformRandomBehavior()
        {
            Debug.WriteLine("Performing Random Behavior");
            Random rand = new Random();
            int behavior = rand.Next(0, 5);
            switch (behavior)
            {
                case 0:
                    Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
                    int randomX = rand.Next(workingArea.Left, workingArea.Right - this.Width);
                    Point randomTarget = new Point(randomX, workingArea.Bottom - this.Height);
                    await WalkTo(randomTarget, CancellationToken.None);
                    Debug.WriteLine("Finish Walk");
                    break;
                case 1:
                    pet.StartAnimation(ResourcesUtils.Animations.Sit, (int)ResourcesUtils.Intervals.Sit);
                    Debug.WriteLine("Finish Sit");
                    break;
                case 2:
                    pet.StartAnimation(ResourcesUtils.Animations.Lay, (int)ResourcesUtils.Intervals.Lay);
                    Debug.WriteLine("Finish Lay");
                    break;
                case 3:
                    pet.StartAnimation(ResourcesUtils.Animations.Idle, (int)ResourcesUtils.Intervals.Idle);
                    Debug.WriteLine("Finish Idle");
                    break;
                case 4:
                    pet.StartAnimation(ResourcesUtils.Animations.Trick, (int)ResourcesUtils.Intervals.Trick);
                    Debug.WriteLine("Finish Trick");
                    break;
            }
            
        }
    }
}
