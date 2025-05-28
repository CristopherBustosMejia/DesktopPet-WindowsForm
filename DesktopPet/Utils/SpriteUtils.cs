using System;
using System.IO;

namespace DesktopPet.Utils
{
    public static class SpriteUtils
    {
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
        public enum Intervals
        {
            Idle = 150,
            Sit = 150,
            Lay = 150,
            Run = 100,
            Walk = 100,
            Barking_Run = 100,
            Barking_Walk = 100,
            Trick = 100,
            Sleep = 200
        }
        public const int numAnimations = 9;
        public static String GetSprSheetPath(String name) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sprites", $"{name}.png");
    }
}
   
