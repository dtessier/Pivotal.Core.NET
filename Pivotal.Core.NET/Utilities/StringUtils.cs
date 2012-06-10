/* Pivotal 5 Solutions Inc. - Core .NET library, communication layer.
 * 
 * Copyright (C) 2012  KASRA RASAEE
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>. 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Pivotal.Core.NET.Utilities {
    public class StringUtils {
    

        public static char GetRandomAlpha(bool anyCase) {
            //Random rand = new Random();
            double phi = 1.61803399;
            Random rand = new Random((int)((((DateTime.Now.Millisecond + 1) / (DateTime.Now.Second + 1) * (DateTime.Now.Hour + 1)) * (DateTime.Now.Millisecond + 1)) / phi));

            int c = 65;
            int ulcase = rand.Next(1, 2000000);
            // TODO this could be an issue - sleep for no more than 500 ms
            // TODO need a much better way to do this, this is stupidity :-)
            System.Threading.Thread.Sleep(rand.Next(100, 500));
            if (ulcase <= 1000000) {
                c = rand.Next(65, 90);
            } else {
                c = rand.Next(97, 122);
            }

            return (char)c;
        }

        /// <summary>
        /// Create a random string, restricted by length, alpha numeric, numeric, or both.
        /// </summary>
        /// <param name="prefix">String to prefix the hash with</param>
        /// <param name="suffix">String to suffix the hash with</param>
        /// <param name="length">Length of generated string</param>
        /// <param name="allowNumeric">Use numeric values? 0 to 9 per character</param>
        /// <param name="allowAlpha">Use alphanumeric values?</param>
        /// <returns>The generated string prefixed and suffixed as per paramaters</returns>
        public static String CreateRandomString(String prefix, String suffix, Int32 length, Boolean allowNumeric, Boolean allowAlpha) {
            StringBuilder sb = new StringBuilder();
            if (length <= 0) {
                return null;
            }

            if (!Comparison.IsEmptyOrNull(prefix)) {
                sb.Append(prefix);
            }

            double phi = 1.61803399;
            Random rand = new Random((int)((((DateTime.Now.Millisecond+1) / (DateTime.Now.Second+1) * (DateTime.Now.Hour+1)) * (DateTime.Now.Millisecond+1)) / phi));
            for (int i = 0; i < length; i++) {
                char c = 'A';
                if (allowAlpha && allowNumeric) {
                    int naCase = rand.Next(1, 20000);
                    if (naCase <= 10000) {
                        c = GetRandomAlpha(true);
                    } else {
                        c = (char)rand.Next(48, 57);
                    }
                } else if (allowAlpha) {
                    c = GetRandomAlpha(true);
                } else if (allowNumeric) {
                    c = (char)rand.Next(48, 57);
                }
                sb.Append(c);
            }

            if (!Comparison.IsEmptyOrNull(suffix)) {
                sb.Append(suffix);
            }
            return sb.ToString();
        }
    }
}
