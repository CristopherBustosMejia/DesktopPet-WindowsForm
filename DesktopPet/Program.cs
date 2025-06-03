using System;
using System.Windows.Forms;
using DesktopPet.Forms;
using System.Collections.Generic;

namespace DesktopPet
{
    internal static class Program
    {
        static List<PetForm> pets = new List<PetForm>();
        static bool hotKeyRegistered = false;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AddPet();
            Application.Run();
        }
        public static void AddPet()
        {
            PetForm pet = new PetForm();
            pets.Add(pet);
            pet.FormClosed += (s, e) => pets.Remove(pet);
            pet.Show();
        }
        public static void AddPet(String path)
        {
            PetForm pet = new PetForm(path);
            pets.Add(pet);
            pet.FormClosed += (s, e) => pets.Remove(pet);
            pet.Show();
        }
        public static List<PetForm> GetPets() => pets;
        public static bool IsHotKeyRegistered() => hotKeyRegistered;
        public static void RegisterHotKey()
        {
            if (!hotKeyRegistered)
            {
                hotKeyRegistered = true;
            }
        }
    }
}
