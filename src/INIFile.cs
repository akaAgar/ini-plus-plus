﻿/*
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
using System.Globalization;
using System.Linq;
using System.Text;

namespace INIPlusPlus
{
    /// <summary>
    /// An .INI file
    /// </summary>
    public class INIFile : IDisposable
    {
        /// <summary>
        /// Dictionary of all sections in this INI file.
        /// </summary>
        private readonly Dictionary<string, INIFileSection> Sections = new Dictionary<string, INIFileSection>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Constructor. Creates an empty file with no sections or values.
        /// </summary>
        public INIFile() { Clear(); }

        /// <summary>
        /// Constructor. Load data from an INI file.
        /// </summary>
        /// <param name="filePath">Path to the INI file</param>
        /// <param name="encoding">Text encoding to use. Default is UTF-8</param>
        public INIFile(string filePath, Encoding encoding = null)
        {
            Clear();

            if (!File.Exists(filePath)) return;
            string iniString = File.ReadAllText(filePath, encoding ?? Encoding.UTF8);
            ParseINIString(iniString);
        }

        //public string GetSectionParentSection(string section)
        //{
        //    if (!Sections.ContainsKey(section)) return "";

        //    return Sections[section].ParentSection ?? "";
        //}

        /// <summary>
        /// Creates an instance of INIFile from a raw INI string
        /// </summary>
        /// <param name="iniString">String containing INI data</param>
        /// <returns>An INIFile</returns>
        public static INIFile CreateFromRawINIString(string iniString)
        {
            INIFile ini = new INIFile();
            ini.Clear();
            ini.ParseINIString(iniString);
            return ini;
        }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose()
        {
            Clear();
        }

        /// <summary>
        /// Clears all sections and values.
        /// </summary>
        public void Clear()
        {
            foreach (string s in Sections.Keys)
                Sections[s].Clear();

            Sections.Clear();
        }

        /// <summary>
        /// Saves the current state of the INIFile to a file.
        /// </summary>
        /// <param name="filePath">Path to the file to write</param>
        /// <param name="encoding">Text encoding to use. Default is UTF-8</param>
        /// <returns>True if everything went right, false if an error happened</returns>
        public bool SaveToFile(string filePath, Encoding encoding = null)
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

                File.WriteAllText(filePath, fileContent, encoding ?? Encoding.UTF8);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// Returns the names of all non-abstract sections.
        /// </summary>
        /// <returns>A string array</returns>
        public string[] GetSections()
        {
            return (from string s in Sections.Keys where !Sections[s].Abstract select s).OrderBy(x => x).ToArray();
        }

        /// <summary>
        /// Reads and returns a value from the ini file.
        /// </summary>
        /// <typeparam name="T">Type of the value to read</typeparam>
        /// <param name="section">Section in which to read the value</param>
        /// <param name="key">Key of the value</param>
        /// <param name="defaultValue">Default value to return if the values doesn't exist or is invalid</param>
        /// <returns>A value</returns>
        public T GetValue<T>(string section, string key, T defaultValue = default)
        {
            if (!ValueExists(section, key))
            {
                if ((defaultValue == null) && (typeof(T) == typeof(string))) return (T)Convert.ChangeType("", typeof(T));
                return defaultValue;
            }

            string val = ReadValue(section, key) ?? "";

            try
            {
                if (CanConvertStringTo<T>())
                    return ConvertStringTo(val, defaultValue);
                else
                    return default;
            }
            catch (Exception)
            {
                return default;
            }
        }

        /// <summary>
        /// Reads and returns an array of value from the ini file.
        /// </summary>
        /// <typeparam name="T">Type of the value to read</typeparam>
        /// <param name="section">Section in which to read the value</param>
        /// <param name="key">Key of the value</param>
        /// <param name="separator">Separator character between values in the array</param>
        /// <returns>An array of values</returns>
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

        /// <summary>
        /// Sets a value in the INI file
        /// </summary>
        /// <typeparam name="T">Type of the value to set</typeparam>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue<T>(string section, string key, T value)
        {
            if (CanConvertStringFrom<T>())
                WriteValue(section, key, ConvertStringFrom(value));
            else
                WriteValue(section, key, "");
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

        /// <summary>
        /// Does the value exist?
        /// </summary>
        /// <param name="section">Section in which the value is stored</param>
        /// <param name="key">Value key</param>
        /// <returns>True if the value exists, false if it doesn't</returns>
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

        /// <summary>
        /// Write a value to the INI file
        /// </summary>
        /// <param name="section">Section to write to</param>
        /// <param name="key">Key to write</param>
        /// <param name="value">Value</param>
        /// <returns>True if written successfully, false otherwise</returns>
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

        /// <summary>
        /// Parses a INI string to populate sections, keys and values.
        /// </summary>
        /// <param name="iniString">A string containing INI data</param>
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

        protected virtual bool CanConvertStringTo<T>()
        {
            Type type = typeof(T);

            if (
                (type == typeof(bool)) ||
                (type == typeof(double)) ||
                (type == typeof(float)) ||
                (type == typeof(int)) ||
                (type == typeof(string)) ||
                type.IsEnum)
                return true;

            return false;
        }

        protected virtual bool CanConvertStringFrom<T>()
        {
            Type type = typeof(T);

            if (
                (type == typeof(bool)) ||
                (type == typeof(double)) ||
                (type == typeof(float)) ||
                (type == typeof(int)) ||
                (type == typeof(string)) ||
                type.IsEnum)
                return true;

            return false;
        }

        protected virtual T ConvertStringTo<T>(string value, T defaultValue = default)
        {
            Type type = typeof(T);

            object outObject = defaultValue;

            if (type == typeof(bool))
                try { outObject = Convert.ToBoolean(value, NumberFormatInfo.InvariantInfo); } catch (Exception) { }
            else if (type == typeof(double))
                try { outObject = Convert.ToDouble(value, NumberFormatInfo.InvariantInfo); } catch (Exception) { }
            else if (type == typeof(float))
                try { outObject = Convert.ToSingle(value, NumberFormatInfo.InvariantInfo); } catch (Exception) { }
            else if (type == typeof(int))
                try { outObject = Convert.ToInt32(value, NumberFormatInfo.InvariantInfo); } catch (Exception) { }
            else if (type == typeof(string))
                outObject = value;
            else if (type.IsEnum)
                try { outObject = Enum.Parse(typeof(T), value, true); } catch (Exception) { }

            return (T)outObject;
        }

        protected virtual string ConvertStringFrom<T>(T value)
        {
            Type type = typeof(T);

            object inObject = value;
            string outString = value.ToString();

            if (type == typeof(bool))
                outString = ((bool)inObject).ToString(NumberFormatInfo.InvariantInfo);
            else if (type == typeof(double))
                outString = ((double)inObject).ToString(NumberFormatInfo.InvariantInfo);
            else if (type == typeof(float))
                outString = ((float)inObject).ToString(NumberFormatInfo.InvariantInfo);
            else if (type == typeof(int))
                outString = ((int)inObject).ToString(NumberFormatInfo.InvariantInfo);

            return outString;
        }

        private T[] ConvertArray<T>(string[] sourceArray)
        {
            try
            {
                T[] arr = new T[sourceArray.Length];

                for (int i = 0; i < sourceArray.Length; i++)
                {
                    if (CanConvertStringTo<T>())
                        arr[i] = ConvertStringTo<T>(sourceArray[i]);
                    else
                        arr[i] = default;
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
