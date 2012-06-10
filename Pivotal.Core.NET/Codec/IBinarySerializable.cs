
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
 */using System;
using System.Text;

using Pivotal.Core.NET.Command;

namespace Pivotal.Core.NET {
  /// <summary>
  /// Should be defined on Command types that will need to be handled by the BinarySerialCodec
  /// </summary>
  public interface IBinarySerializable {    
    byte[] BinarySerialize(Encoding encoding);

    ICommand BinaryDeserialize(byte[] buf, Encoding encoding); 
  }
}

