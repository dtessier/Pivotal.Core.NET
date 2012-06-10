using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Pivotal.Core.NET.Utilities {

    /// <summary>
    /// Utility for comparing various strings, numbers, lists, so forth.
    /// </summary>
    public static class Comparison {

		public static bool IsEmpty(byte[] buffer) {
			return buffer == null || buffer.Length == 0;
		}
		
        public static bool IsEqualCaseInsensitive(String s1, String s2) {
            return String.Compare(s1, s2, true) == 0;
        }

        public static bool TrimIsEmptyOrNull(String value) {
            if (value != null) {
                value = value.Trim();
            }

            return IsEmptyOrNull(value);
        }

        public static bool IsEmptyOrNull<T>(List<T> list) {
            return !IsNotEmptyOrNull(list);
        }

        public static bool IsNotEmptyOrNull<T>(List<T> list) {
            return (list != null && list.Count > 0);
        }

        public static bool IsNull(Object value) {
            return value == null;
        }

        public static bool IsEmptyOrNull(String value) {
            return value == null || "".Equals(value);
        }

        public static bool IsLessThanZero(Int16 value) {
            return value < 0;
        }

        public static bool IsLessThanZero(Int32 value) {
            return value < 0;
        }

        public static bool IsZero(Int64 value) {
            return value == 0;
        }

        public static bool IsZero(Int32 value) {
            return value == 0;
        }

        public static bool IsZero(Int16 value) {
            return value == 0;
        }

        public static String ReturnIfNull(String value, String returnValue) {
            if (IsEmptyOrNull(value)) {
                return returnValue;
            }
            return "";
        }
    }
}
