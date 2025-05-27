using DesktopPet.Utils;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace DesktopPet.Forms
{
    public partial class PetForm : Form
    {
        private bool isClickThroughEnabled = true;
        private const int HOTKEY_ID = 9000;
        AnimationBox pet;

        public PetForm()
        {
            InitializeComponent();
            InitializeForm();
            InitializePet();
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
            this.ClientSize = new System.Drawing.Size(800, 450);
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
                Location = new Point(100, 100)
            };
            this.Controls.Add(pet);
        }

        private void PetForm_Load(object sender, EventArgs e)
        {
            pet.StartAnimation(SpriteUtils.Animations.Idle,100);
            DisableClickThrough();
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
