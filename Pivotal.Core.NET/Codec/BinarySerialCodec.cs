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
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using Pivotal.Core.NET.Command;

namespace Pivotal.Core.NET.Codec {
   
// TODO incomplete.
  
  /// <summary>
  /// Binary serial codec.
  /// </summary>
  public class BinarySerialCodec : AbstractBufferCodec, IBufferCodec {
    
    public BinarySerialCodec() {
         
    }
    
    public override void AddIdentity(CommandIdentifier identity) {
      
      // if the command is not assignable-from
      if (!(typeof(IBinarySerializable).IsAssignableFrom (identity.CommandType))) {
        throw new InvalidCastException(
          String.Format (
          "Cannot register command {0} to type {1}, make sure your command implements {2}", 
          identity.CommandType,
          typeof(IBinarySerializable),
          identity.CommandType
        )
        );
              
        
      }
      
      base.AddIdentity (identity);
    }
    
    /// <summary>
    /// The minimum bytes required to determine the type of command
    /// </summary>
    /// <returns>return negative one, if the command type is not determined by a fixed length buffer</returns>
    public short MinLengthRequired() {
      return 2; // 2 byte integer, thats 16384 combinations for command types.
    }
    
    protected override Object BuildSerial(Type type) {
      throw new NotImplementedException("Not yet implemented, should extract it from IBinarySerializable types");
    }

    /// <summary>
    /// Determines the command objects binary serial id for a given buffer array, 
    /// the array may or may be incomplete, and could contain enough bytes to identify 
    /// the command type. Ideally, in a binary situtation, you would want fixed-length
    /// command identifiers, however, a null \0 termination or a pipe could also define
    /// the identifier.
    /// 
    /// </summary>
    /// <returns>
    /// The serial id if determined, otherwise null.
    /// </returns>
    /// <param name='ba'>
    /// Byte array from the socket receive handler.
    /// </param>
    /// <param name='encoding'>
    /// Encoding type, ideally UTF-8, but could be anything defined by the application, e.g. ISO-8859-1
    /// </param>
    public Object DetermineSerial(byte[] ba, Encoding encoding) {
      int min = MinLengthRequired ();
      if (min <= 0) {
        return null;
      } else if (ba != null && ba.Length >= min) {
        byte[] serialbuf = new byte[min];
        Buffer.BlockCopy (ba, 0, serialbuf, 0, min);
        
        // TODO should be adjustable? based e.g. sub-class
        
        if (min == 2) {
          // TODO consider little-endian ? the data should always be in the same format, even if its cross platform.
          return BitConverter.ToInt16 (serialbuf, 0);
        }
      }

      return null;
    }

    // TO BE COMPLETED PROPERLY.

    public ICommand ToCommand(Object serial, byte[] ba, Encoding encoding) {

      if (ba != null && ba.Length > 0) {
        using (MemoryStream ms = new MemoryStream(ba)) {
          ms.Seek (0, SeekOrigin.Begin);
          return ToCommand (serial, ms, encoding);
        }
      }

      return default(ICommand);
    }

    public ICommand ToCommand(Object serial, Stream stream, Encoding encoding) {
      if (stream != null) {
        
        
        Object c = null;
                
        if (serial != null) {
          
        }
        
        // USE a stream reader?
        //DetermineSerial(
        
        if (!(c is ICommand)) {
          // TODO throw exception
          return default(ICommand);
        }


        return (ICommand)c;
      }
            
      return default(ICommand);
    }

    public Stream ToStream(ICommand command, Encoding encoding) {

      return default(Stream);
    }

    public byte[] ToByteArray(ICommand command, Encoding encoding) {
            
      return default(byte[]);
    }
    
    public byte[] ToByteArray(ICommand command, Encoding encoding, bool nil) {
    
      return default(byte[]);
    }
  }
}
