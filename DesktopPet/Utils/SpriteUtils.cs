using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopPet.Utils
{
    public static class SpriteUtils
    {
        public static String spriteSheetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sprites","Dog.png");
        public const int numAnimations = 9;
        public enum Animations
        {
            Idle = 0,
            Sit = 1,
            Lay = 2,
            Run = 3,
            Walk = 4,
            Barking_Run = 5,
            Barking_Walk = 6,
            Trick = 7,
            Sleep = 8,
        }
    }
}
   
