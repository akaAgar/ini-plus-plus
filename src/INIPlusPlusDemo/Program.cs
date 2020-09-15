using System;

namespace INIPlusPlus.Demo
{
    public sealed class DemoProgram
    {
        private static void Main()
        {
#if DEBUG
            INIFile ini = new INIFile(@"..\Release\demo.ini");
#else
            INIFile ini = new INIFile("demo.ini");
#endif

            Console.WriteLine("FILE MAP");
            Console.WriteLine("========");
            Console.WriteLine("- INI file sections: " + string.Join(",", ini.GetSections()));
            Console.WriteLine("- INI file sections (including abstract sections): " + string.Join(",", ini.GetSections(true)));
            Console.WriteLine();
            foreach (string s in ini.GetSections())
            {
                Console.WriteLine($"[{s.ToUpperInvariant()}]");
                foreach (string k in ini.GetKeysInSection(s)) Console.WriteLine("- " + k);
                Console.WriteLine();
            }

            Console.WriteLine("VALUES READING TEST");
            Console.WriteLine("===================");
            Console.WriteLine("Value exists (cat->legs): " + ini.ValueExists("cat", "legs"));
            Console.WriteLine("Value exists (cat->tentacles): " + ini.ValueExists("cat", "tentacles"));
            Console.WriteLine();
            Console.WriteLine("Value (cat->legs): " + ini.GetValue<int>("cat", "legs"));
            Console.WriteLine("Value (cat->weight_in_kilograms): " + ini.GetValue<float>("cat", "weight_in_kilograms"));
            Console.WriteLine("Value (kitten->legs): " + ini.GetValue<int>("kitten", "legs"));
            Console.WriteLine("Value (kitten->weight_in_kilograms): " + ini.GetValue<float>("kitten", "weight_in_kilograms"));
            Console.WriteLine();

            Console.WriteLine("VALUES WRITING TEST");
            Console.WriteLine("===================");
            Console.WriteLine("Set (cat->weight_in_kilograms) to 8.4");
            ini.SetValue("cat", "weight_in_kilograms", 8.4f);
            Console.WriteLine("Value (cat->weight_in_kilograms): " + ini.GetValue<float>("cat", "weight_in_kilograms"));

            Console.WriteLine();
            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }
    }
}
