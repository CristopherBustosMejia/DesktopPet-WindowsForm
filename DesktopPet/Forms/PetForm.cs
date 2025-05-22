using DesktopPet.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DesktopPet.Utils;

namespace DesktopPet.Forms
{
    public partial class PetForm : Form
    {
        private System.ComponentModel.IContainer components = null;
        AnimationBox pet;

        public PetForm()
        {
            InitializeComponent();
            //this.FormBorderStyle = FormBorderStyle.None;
            //this.ShowInTaskbar = false;
            this.TopMost = true;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            int wl = WinAPI.GetWindowLong(this.Handle,WinAPI.GWL_EXSTYLE);
            WinAPI.SetWindowLong(this.Handle, WinAPI.GWL_EXSTYLE, wl | WinAPI.WS_EX_TOOLWINDOW);
            pet = new AnimationBox();
            pet.BackColor = Color.Transparent;
            pet.SizeMode = PictureBoxSizeMode.AutoSize;
            pet.Location = new Point(100, 100);
            this.Controls.Add(pet);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
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

        private void PetForm_Load(object sender, EventArgs e)
        {
            pet.StartAnimation(SpriteUtils.Animations.Sleep);
        }
    }

    public class WinAPI
    {
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TOOLWINDOW = 0x00000000;
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int index, int newStyle);
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int index);
    }
}
