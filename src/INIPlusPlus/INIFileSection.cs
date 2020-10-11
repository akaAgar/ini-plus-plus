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
using System.Linq;

namespace INIPlusPlus
{
    /// <summary>
    /// A section in the .INI file
    /// </summary>
    internal sealed class INIFileSection : IDisposable
    {
        /// <summary>
        /// Is the section abstract?
        /// </summary>
        internal bool Abstract { get; }

        /// <summary>
        /// What "parent" section does this section inherits (null if none)?
        /// </summary>
        internal string ParentSection { get; }

        /// <summary>
        /// A dictionary of all values in this section.
        /// </summary>
        private readonly Dictionary<string, string> Values;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="isAbstract">Is the section abstract?</param>
        /// <param name="parentSection">What "parent" section does this section inherits (null if none)?</param>
        internal INIFileSection(bool isAbstract = false, string parentSection = null)
        {
            Abstract = isAbstract;
            ParentSection = parentSection?.ToLowerInvariant();

            Values = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Gets/sets a value in the values dictionary.
        /// </summary>
        /// <param name="key">Key to the value</param>
        /// <returns>Value, or null if it doesn't exist</returns>
        internal string this[string key]
        {
            get
            {
                return Values.ContainsKey(key) ? Values[key] : null;
            }
            set
            {
                key = key?.ToLowerInvariant();

                if (Values.ContainsKey(key))
                    Values[key] = value;
                else
                    Values.Add(key, value);
            }
        }

        /// <summary>
        /// All value keys in this section.
        /// </summary>
        internal string[] Keys
        {
            get
            {
                return Values.Keys.ToArray();
            }
        }

        /// <summary>
        /// Clears the section of all values.
        /// </summary>
        internal void Clear()
        {
            Values.Clear();
        }

        /// <summary>
        /// Does the section contains a given key?
        /// </summary>
        /// <param name="key">A key</param>
        /// <returns>True if the key exists, false if it doesn't</returns>
        internal bool ContainsKey(string key)
        {
            return Values.ContainsKey(key);
        }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose()
        {
            Clear();
        }
    }
}
