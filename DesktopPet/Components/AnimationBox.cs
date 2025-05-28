using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace DesktopPet.Utils
{
    public class AnimationBox : PictureBox
    {
        public bool FlipX { get; set; } = false;
        protected List<List<Image>> animations = new List<List<Image>>();
        private int frameIndex = 0;
        private ResourcesUtils.Animations currentState = ResourcesUtils.Animations.Idle;
        private readonly Timer animationTimer = new Timer();
        private readonly Bitmap spriteSheet;
        public AnimationBox()
        {
            if (!File.Exists(ResourcesUtils.GetSprSheetPath("Dog")))
                throw new FileNotFoundException("No se encontró el sprite sheet.");

            spriteSheet = new Bitmap(ResourcesUtils.GetSprSheetPath("Dog"));
            LoadAnimations();
            animationTimer.Tick += AnimationTick;
        }

        protected void LoadAnimations()
        {
            for(int i = 0; i < ResourcesUtils.numAnimations; i++)
            {
                animations.Add(SliceSpriteSheet(i));
            }
        }
        public void StartAnimation(ResourcesUtils.Animations state, int interval)
        {
            animationTimer.Stop();
            currentState = state;
            frameIndex = 0;
            animationTimer.Interval = interval;
            animationTimer.Start();
        }
        private void AnimationTick(object sender, EventArgs e)
        {
            List<Image> frames = this.getAnimation(currentState);
            Image frame = frames[frameIndex];
            if (FlipX)
            {
                frame = (Image)frame.Clone();
                frame.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
            this.Image = frame;
            frameIndex ++;
            frameIndex = frameIndex >= frames.Count ? 0 : frameIndex; 
        }

        protected List<Image> SliceSpriteSheet(int animationRow)
        {
            List<Image> frames = new List<Image>();
            Dictionary<int, int> frameCounts = new Dictionary<int, int>
            {
                { 0, 6 }, { 1, 6 }, { 2, 6 }, { 3, 8 },
                { 4, 8 }, { 5, 8 }, { 6, 8 }, { 7, 8 }, { 8, 4 }
            };
            if (!frameCounts.TryGetValue(animationRow, out int numSprites))
                throw new ArgumentOutOfRangeException(nameof(animationRow), "Número de animación no válido.");

            if (spriteSheet.Width < (numSprites * 64) || spriteSheet.Height < ((animationRow + 1) * 48))
                throw new Exception("El sprite sheet es más pequeño de lo necesario para esta animación.");

            for (int i = 0; i < numSprites; i++)
            {
                Rectangle frameRect = new Rectangle(64 * i, 48 * animationRow, 64, 48);
                Bitmap frame = spriteSheet.Clone(frameRect, spriteSheet.PixelFormat);
                frames.Add(frame);
            }
            return frames;
        }
        public List<Image> getAnimation(ResourcesUtils.Animations animationNumber)
        {
            return animations[(int)animationNumber];
        }
    }
}
