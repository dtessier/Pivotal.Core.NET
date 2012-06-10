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
using System.IO;
using System.Collections;

using System.Diagnostics;

using System.Reflection;
using System.Text.RegularExpressions;

using Pivotal.Core.NET.Command;

namespace Pivotal.Core.NET.Codec {
  /// <summary>
  /// Abstract buffer codec, base class when building custom handlers.
  /// </summary>
  public abstract class AbstractBufferCodec {
	
    protected Hashtable Identities { get; set; }
		
    /// <summary>
    /// Get the identifier for a specified type of command, this is determined based 
    /// on the codec type used. For example: if XmlSerializer is used, then a command 
    /// identifier may be EchoCommand, where Binary Codec may return value 1000.
    /// 
    /// The Codec implementation MUST implement this properly if ScanIdentities is used.
    /// 
    /// </summary>
    /// <param name='type'>
    /// Type.
    /// </param>
    protected abstract object BuildSerial(Type type);
		
    /// <summary>
    /// Scans the assembly for a given search pattern to identify types of commands.
    /// </summary>
    /// <param name='assemblyName'>
    /// Assembly name.
    /// </param>
    /// <param name='searchPattern'>
    /// Search pattern.
    /// </param>
    public virtual void ScanForIdentity(String assemblyName, String searchPattern) {
      Regex regex = new Regex(searchPattern);
			            
      Assembly assembly = null;
      try {
        assembly = Assembly.Load (assemblyName);
      } catch (FileNotFoundException) {
        Debug.WriteLine (String.Format (
			"Warning: assembly by the name of {0} when scanning " +
        	"for ICommand types was not found", assemblyName)
        );
        return;
      }
			
      Type[] types = assembly.GetTypes ();
      foreach (Type type in types) {
				
        // TODO should check for ICommand interface...
				
        String name = type.FullName;
				
        if (regex.IsMatch (name)) {
          Console.Out.WriteLine (string.Format ("Matched Command Type {0}", type)); 
          Object serial = BuildSerial (type);
          CommandIdentifier identifier = new CommandIdentifier(serial, type);
          AddIdentity (identifier);
        }		
      }
			
      return;
    }
		
    /// <summary>
    /// Adds the command identifier, used when serializing or deserializing
    /// commands, this is necessary to determine the type of command to 
    /// deserialize to. e.g. EchoCommand in XML codecs will be defined by
    /// "EchoCommand", versus binary Codec may define it as serial 1000
    /// 
    /// Each codec type is resonsible for building its own serial command types.
    /// 
    /// </summary>
    /// <param name='identity'>
    /// Identity.
    /// </param>
    public virtual void AddIdentity(CommandIdentifier identity) {
      if (this.Identities == null) {
        this.Identities = new Hashtable();
      }
			
      // if the serial already exists, check to see if its a different command type?
      if (Identities.ContainsKey (identity.Serial)) {
        CommandIdentifier ci = Identities [identity.Serial] as CommandIdentifier;
        if (!ci.CommandType.Equals (identity.CommandType)) {
          throw new InvalidDataException(String.Format (
            "Command identity already exists for command serial {0} on {1} != serial {2} on {3}", 
            ci.Serial,
            ci.CommandType.ToString(),
            identity.Serial,
            identity.CommandType)
          );
        }
      }
			
      Identities [identity.Serial] = identity;
    }
  }
}

