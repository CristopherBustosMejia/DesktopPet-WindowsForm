using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopPet.Utils
{
    public static class ResourcesUtils
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
        public static String GetSprSheetPath(String name) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Sprites", $"{name}.png");
        public static String GetSprSheetPath()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Selecciona el sprite sheet del nuevo pet";
                ofd.Filter = "Archivos de imagen|*.png;*.jpg;*.jpeg;*.bmp|Todos los archivos|*.*";
                ofd.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Sprites");
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string spriteSheetPath = ofd.FileName;
                    if (!File.Exists(spriteSheetPath))
                    {
                        MessageBox.Show("El archivo seleccionado no existe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return ResourcesUtils.GetSprSheetPath("Default");
                    }
                    return spriteSheetPath;
                }
                MessageBox.Show("No se selecciono ningun archivo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return ResourcesUtils.GetSprSheetPath("Default");
            }
        }
    }
}
   
