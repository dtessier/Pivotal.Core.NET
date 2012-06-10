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

namespace Pivotal.Core.NET.Codec {
  
  /// <summary>
  /// Command identifier to define a given serial id to a given command type.
  /// 
  /// For example, in the binary codec scenario, the buffer may be constructed as such.
  /// 
  /// 1000:0:976 which will entail, 
  ///  - command id 1000 = EchoCommand
  ///  - reply: false
  ///  - sequence: 976
  /// 
  /// in XML, it may look like 
  /// <code>
  ///   <EchoCommand>
  ///     <Reply>false</Reply>
  ///     <Sequence>987</Sequence>
  ///   </EchoCommand>
  /// </code>
  /// </summary>
  public class CommandIdentifier {
    public Object Serial { get; set; }

    public Type CommandType { get; set; }
    
    public CommandIdentifier(Object serial, Type commandType) {
      this.Serial = serial;
      this.CommandType = commandType;
    }
  }
}

