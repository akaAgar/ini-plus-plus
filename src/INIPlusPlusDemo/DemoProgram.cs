/*
==========================================================================
This file is part of INIPlusPlus, an advanced cross-platform INI parser
handling abstract sections and inheritance, by @akaAgar
(https://github.com/akaAgar/ini-plus-plus)
INIPlusPlus is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
INIPlusPlus is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with INIPlusPlus. If not, see https://www.gnu.org/licenses/
==========================================================================
*/

using System;

namespace INIPlusPlus.Demo
{
    /// <summary>
    /// Demo program. Loads data from a demo INI file and displays them in the console.
    /// </summary>
    public sealed class DemoProgram
    {
        /// <summary>
        /// Static Main() method.
        /// </summary>
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
