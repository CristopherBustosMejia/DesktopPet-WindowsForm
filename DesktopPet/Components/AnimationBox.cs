using DesktopPet.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopPet.Utils
{
    public class AnimationBox : PictureBox
    {
        protected List<List<Image>> animations = new List<List<Image>>();
        private int frameIndex = 0;
        private Enum currentState = SpriteUtils.Animations.Idle;
        private Timer animationTimer = new Timer();
        private Bitmap spriteSheet;
        public AnimationBox()
        {
            if (!File.Exists(SpriteUtils.spriteSheetPath))
                throw new FileNotFoundException("No se encontró el sprite sheet.");

            spriteSheet = new Bitmap(SpriteUtils.spriteSheetPath);
            LoadAnimations();
        }

        protected void LoadAnimations()
        {
            for(int i = 0; i < SpriteUtils.numAnimations; i++)
            {
                animations.Add(SliceSpriteSheet(i));
            }
        }
        public void StartAnimation(SpriteUtils.Animations state, int interval = 100)
        {
            currentState = state;
            frameIndex = 0;
            animationTimer.Interval = interval;
            animationTimer.Tick += (s, e) =>
            {
                var frames = this.getAnimation(state);
                this.Image = frames[frameIndex];
                frameIndex = (frameIndex + 1) % frames.Count;
            };
            animationTimer.Start();
        }

        protected List<Image> SliceSpriteSheet(int animationRow)
        {
            List<Image> frames = new List<Image>();
            int numSprites = animationRow == 8 ? 4 : 8;
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
        public List<Image> getAnimation(SpriteUtils.Animations animationNumber)
        {
            return animations[(int)animationNumber];
        }
    }
}
