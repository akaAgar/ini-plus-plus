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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace INIPlusPlus
{
    public sealed class INIFile : IDisposable
    {
        private readonly Dictionary<string, INIFileSection> Sections = new Dictionary<string, INIFileSection>(StringComparer.InvariantCultureIgnoreCase);

        public INIFile()
        {
            Clear();
        }

        public INIFile(string filePath)
        {
            Clear();

            if (!File.Exists(filePath)) return;
            string iniString = File.ReadAllText(filePath, Encoding.UTF8);
            ParseINIString(iniString);
        }

        public string GetSectionParentSection(string section)
        {
            if (!Sections.ContainsKey(section)) return "";

            return Sections[section].ParentSection ?? "";
        }

        public static INIFile CreateFromRawINIString(string iniString)
        {
            INIFile ini = new INIFile();
            ini.Clear();
            ini.ParseINIString(iniString);
            return ini;
        }

        public void Dispose()
        {
            Clear();
        }

        public void Clear()
        {
            foreach (string s in Sections.Keys)
                Sections[s].Clear();

            Sections.Clear();
        }

        public bool SaveToFile(string filePath)
        {
            try
            {
                string fileContent = "";

                foreach (string s in Sections.Keys)
                {
                    if (string.IsNullOrEmpty(s)) continue;
                    fileContent += $"[{s}]\r\n";

                    foreach (string v in Sections[s].Keys)
                    {
                        if (string.IsNullOrEmpty(v)) continue;
                        if (string.IsNullOrEmpty(Sections[s][v])) continue;

                        fileContent += $"{v}={Sections[s][v]}\r\n";
                    }

                    fileContent += "\r\n";
                }

                File.WriteAllText(filePath, fileContent, Encoding.UTF8);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsSectionAbstract(string section)
        {
            return Sections.ContainsKey(section) && Sections[section].Abstract;
        }

        public string[] GetSections()
        {
            return Sections.Keys.OrderBy(x => x).ToArray();
        }

        public T GetValue<T>(string section, string key, T defaultValue = default)
        {
            if (!ValueExists(section, key))
            {
                if ((defaultValue == null) && (typeof(T) == typeof(string))) return (T)Convert.ChangeType("", typeof(T));
                return defaultValue;
            }

            object val = ReadValue(section, key) ?? "";

            try
            {
                if (typeof(T) == typeof(string)) val = (string)val;
                else if (typeof(T) == typeof(bool)) val = ConversionTools.StringToBool((string)val);
                else if (typeof(T) == typeof(int)) val = ConversionTools.StringToInt((string)val);
                else if (typeof(T) == typeof(float)) val = ConversionTools.StringToFloat((string)val);
                else if (typeof(T) == typeof(double)) val = ConversionTools.StringToDouble((string)val);
                else if (typeof(T).IsEnum) val = Enum.Parse(typeof(T), (string)val, true);

                return (T)Convert.ChangeType(val, typeof(T));
            }
            catch (Exception)
            {
                return default;
            }
        }

        public T[] GetValueArray<T>(string section, string key, char separator = ',')
        {
            if (string.IsNullOrEmpty(GetValue<string>(section, key))) return new T[0];

            object val = ReadValue(section, key) ?? "";

            try
            {
                if (typeof(T) == typeof(string)) val = val.ToString().Split(separator);
                else if (typeof(T) == typeof(bool)) val = ConvertArray<bool>(val.ToString().Split(separator));
                else if (typeof(T) == typeof(int)) val = ConvertArray<int>(val.ToString().Split(separator));
                else if (typeof(T) == typeof(float)) val = ConvertArray<float>(val.ToString().Split(separator));
                else if (typeof(T) == typeof(double)) val = ConvertArray<double>(val.ToString().Split(separator));
                else if (typeof(T).IsEnum) val = ConvertArray<T>(val.ToString().Split(separator));

                return (T[])Convert.ChangeType(val, typeof(T[]));
            }
            catch (Exception)
            {
                return default;
            }
        }

        public void SetValue<T>(string section, string key, T val)
        {
            object oVal = val;

            if (typeof(T) == typeof(string)) WriteValue(section, key, (string)oVal);
            else if (typeof(T) == typeof(bool)) WriteValue(section, key, ConversionTools.ValToString((bool)oVal));
            else if (typeof(T) == typeof(int)) WriteValue(section, key, ConversionTools.ValToString((int)oVal));
            else if (typeof(T) == typeof(float)) WriteValue(section, key, ConversionTools.ValToString((float)oVal));
            else if (typeof(T) == typeof(double)) WriteValue(section, key, ConversionTools.ValToString((double)oVal));
            else WriteValue(section, key, val.ToString());
        }

        public void SetValueArray<T>(string section, string key, T[] val, char separator = ',')
        {
            val = val ?? new T[0];
            object oVal = val;

            if (typeof(T) == typeof(string))
                WriteValue(section, key, string.Join(separator.ToString(), (string[])oVal));
            else
                WriteValue(section, key, val.ToString());
        }

        public string[] GetKeysInSection(string section)
        {
            if (!Sections.ContainsKey(section)) return new string[0];

            List<string> keys = new List<string>();
            keys.AddRange(Sections[section].Keys);

            List<string> checkedSections = new List<string>();

            while (
                !string.IsNullOrEmpty(Sections[section].ParentSection) &&
                !checkedSections.Contains(section.ToLowerInvariant()) &&
                Sections.ContainsKey(Sections[section].ParentSection))
            {
                checkedSections.Add(section.ToLowerInvariant()); // To avoid circular inheritances
                section = Sections[section].ParentSection;
                keys.AddRange(Sections[section].Keys);
            }

            return keys.Distinct().OrderBy(x => x).ToArray();
        }

        //public string[] GetTopLevelKeysInSection(string section)
        //{
        //    List<string> keys = new List<string>(GetKeysInSection(section));

        //    for (int i = 0; i < keys.Count; i++)
        //        if (keys[i].Contains("."))
        //            keys[i] = keys[i].Substring(0, keys[i].IndexOf("."));

        //    return keys.Distinct().ToArray();
        //}

        public bool ValueExists(string section, string key)
        {
            return ReadValue(section, key) != null;
        }

        private string ReadValue(string section, string key)
        {
            if (string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key)) return null;

            List<string> checkedSections = new List<string>();

            while (Sections.ContainsKey(section))
            {
                if (Sections[section].ContainsKey(key))
                    return Sections[section][key];

                if (string.IsNullOrEmpty(Sections[section].ParentSection))
                    return null;

                checkedSections.Add(section.ToLowerInvariant()); // To avoid circular inheritances
                if (checkedSections.Contains(Sections[section].ParentSection.ToLowerInvariant()))
                    return null;

                section = Sections[section].ParentSection;
            }

            return null;
        }

        private bool WriteValue(string section, string key, string value)
        {
            section = (section ?? "").ToLowerInvariant().Trim();
            key = (key ?? "").ToLowerInvariant().Trim();
            value = value ?? "";
            if (string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key)) return false;

            if (!Sections.ContainsKey(section))
                Sections.Add(section, new INIFileSection());
            
            Sections[section][key] = value;
            
            return true;
        }

        private void ParseINIString(string iniString)
        {
            string[] lines = (iniString + "\n").Replace("\r\n", "\n").Split('\n');

            Clear();

            string section = null;

            foreach (string li in lines)
            {
                string l = li.Trim(' ', '\t'); // Trim line
                if (l.StartsWith(";")) continue; // Line is a comment

                if (l.StartsWith("[")) // found a new section
                {
                    // try to get section name, make sure it's valid
                    section = l.Trim('[', ']', ' ', '\t', ':').ToLowerInvariant();
                    string parentSection = null;

                    if (section.Contains(':')) // Sections inherits another section, name declared in the [SECTION:PARENT_SECTION] format
                    {
                        try
                        {
                            string sectionWithParent = section;
                            section = sectionWithParent.Split(':')[0].Trim();
                            parentSection = sectionWithParent.Split(':')[1].Trim();
                        }
                        catch (Exception)
                        {
                            section = l.Trim('[', ']', ' ', '\t', ':').ToLowerInvariant();
                            parentSection = null;
                        }
                    }

                    bool abstractSection = section.StartsWith("_");

                    if (string.IsNullOrEmpty(section)) { section = null; continue; }

                    if (!Sections.ContainsKey(section))
                        Sections.Add(section, new INIFileSection(abstractSection, parentSection));

                    continue;
                }

                if (l.Contains('=')) // The line contains an "equals" sign, it means we found a value
                {
                    if (section == null) continue; // we're not in a section, ignore

                    string[] v = l.Split(new char[] { '=' }, 2); // Split the line at the first "equal" sign: key = value
                    if (v.Length < 2) continue;

                    string key = v[0].Trim().ToLowerInvariant();
                    string value = v[1].Trim().Trim('\"');
                    WriteValue(section, key, value);
                }
            }
        }

        private T[] ConvertArray<T>(string[] sourceArray)
        {
            try
            {
                T[] arr = new T[sourceArray.Length];

                for (int i = 0; i < sourceArray.Length; i++)
                {
                    object o = default(T);

                    if (typeof(T) == typeof(bool)) o = ConversionTools.StringToBool(sourceArray[i]);
                    else if (typeof(T) == typeof(int)) o = ConversionTools.StringToInt(sourceArray[i]);
                    else if (typeof(T) == typeof(double)) o = ConversionTools.StringToDouble(sourceArray[i]);
                    else if (typeof(T) == typeof(float)) o = ConversionTools.StringToFloat(sourceArray[i]);
                    else if (typeof(T).IsEnum) o = Enum.Parse(typeof(T), sourceArray[i].ToString(), true);

                    arr[i] = (T)Convert.ChangeType(o, typeof(T));
                }

                return arr;
            }
            catch (Exception)
            {
                return new T[0];
            }
        }
    }
}
