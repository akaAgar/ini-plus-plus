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
using System.Globalization;

namespace INIPlusPlus
{
    internal static class ConversionTools
    {
        internal static string ValToString(bool value)
        {
            return value.ToString(NumberFormatInfo.InvariantInfo);
        }

        internal static string ValToString(int value, string stringFormat = null)
        {
            if (string.IsNullOrEmpty(stringFormat))
                return value.ToString(NumberFormatInfo.InvariantInfo);
            else
                return value.ToString(stringFormat, NumberFormatInfo.InvariantInfo);
        }

        internal static string ValToString(float value, string stringFormat = null)
        {
            if (string.IsNullOrEmpty(stringFormat))
                return value.ToString(NumberFormatInfo.InvariantInfo);
            else
                return value.ToString(stringFormat, NumberFormatInfo.InvariantInfo);
        }

        internal static string ValToString(double value, string stringFormat = null)
        {
            if (string.IsNullOrEmpty(stringFormat))
                return value.ToString(NumberFormatInfo.InvariantInfo);
            else
                return value.ToString(stringFormat, NumberFormatInfo.InvariantInfo);
        }

        internal static double StringToDouble(string stringValue, double defaultValue = 0.0)
        {
            try { return Convert.ToDouble(stringValue.Trim(), NumberFormatInfo.InvariantInfo); }
            catch (Exception) { return defaultValue; }
        }

        internal static bool StringToBool(string stringValue, bool defaultValue = false)
        {
            try { return Convert.ToBoolean(stringValue, NumberFormatInfo.InvariantInfo); }
            catch (Exception) { return defaultValue; }
        }

        internal static float StringToFloat(string stringValue, float defaultValue = 0.0f)
        {
            try { return Convert.ToSingle(stringValue.Trim(), NumberFormatInfo.InvariantInfo); }
            catch (Exception) { return defaultValue; }
        }

        internal static int StringToInt(string stringValue, int defaultValue = 0)
        {
            try { return Convert.ToInt32(stringValue.Trim(), NumberFormatInfo.InvariantInfo); }
            catch (Exception) { return defaultValue; }
        }

    }
}
