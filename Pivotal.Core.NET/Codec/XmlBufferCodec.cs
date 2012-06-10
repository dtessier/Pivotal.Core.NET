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
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using Pivotal.Core.NET.Command;

// TODO incomplete and not fully tested, optimize via Streams not byte[].. cleanup code.


namespace Pivotal.Core.NET.Codec {
  public class XmlBufferCodec : AbstractBufferCodec, IBufferCodec {
        
    protected static XmlBufferCodec Instance { get; set; }
		
    // protected Encoding Encoding { get; set; }
    // protected readonly Object SerializerLock = new Object();
    //protected XmlSerializer Serializer = new XmlSerializer();

//        public XmlBufferCodec(Encoding encoding) {
//            this.Encoding = encoding;
//        }
		
		
    public static XmlBufferCodec SingletonInstance() {
      if (Instance == null) {
        Instance = new XmlBufferCodec();
      }
      return Instance;
    }
		
    /// <summary>
    /// The minimum bytes required to determine the type of command
    /// </summary>
    /// <returns>return negative one, if the command type is not determined by a fixed length buffer</returns>
    public short MinLengthRequired() {
      return -1;
    }

    protected override object BuildSerial(Type type) {
      return type.Name;
    }
		
    /// <summary>
    /// Determines the command objects XML serial id for a given buffer array, 
    /// the array may or may be incomplete, but could contain enough bytes to identify 
    /// the command type. e.g. <EchoCommand>..
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
      if ((min == -1) || (ba != null && ba.Length >= min)) {


        // starting point of array search is zero
        int start = 0;
        bool exit = true;

        do {
          // reset exit point
          exit = true;

          // check for opening xml, but make sure its not <?xml...
          start = Array.IndexOf<byte> (ba, (byte)'<', start);

          if (start >= 0 && start + 1 < ba.Length) {
            if (ba [start + 1] == '?') {
              start++;
              exit = false;
            }
          }
        } while (!exit);


        if (start >= 0) {
          int end = Array.IndexOf<byte> (ba, (byte)'>', start);
					
          // has it ended with a > if not check for white space
          bool ended = end > start;
					
          // check for white space instead of >, may contain xmlns namespace 
          // info, which is not necessary when identifying the command.
          if (!ended) { 
            end = Array.IndexOf<byte> (ba, (byte)' ', start);
            ended = end > start;
          }
					
          // if either '>' or white space ' '
          if (ended) {
            // check for 
            // <?xml version="1.0"?>

            int length = end - start - 1;
            byte[] element = new byte[length];
            Buffer.BlockCopy (ba, start + 1, element, 0, length);
						
            //String e = System.Text.Encoding.UTF8.GetString (element);
            //System.Console.Out.WriteLine("Element: " + e);
						
            return encoding.GetString (element);
          }
        }

      }

      return null;
    }

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
      if (stream == null) {
        return default(ICommand);
      }
			
      CommandIdentifier identifier = null;
			
      if (Identities != null && Identities.ContainsKey (serial)) {
        identifier = (CommandIdentifier)Identities [serial];
      }
			
      if (identifier == null) {
        return default(ICommand);
      }
			
      Object command = null;
            
      // create a reader, using the specific encoding of the socket
      using (StreamReader reader  = new StreamReader(stream, encoding)) {
        //XmlDocument xmldoc = new XmlDocument();
        //xmldoc.Load (reader);
	
        // create a serializer for the given object type.
        XmlSerializer serializer = new XmlSerializer(identifier.CommandType);
        //XmlWriterSettings settings = new XmlWriterSettings();	
        //settings.Encoding = encoding;
        //settings.Indent = false;
				
        //XmlReader reader = XmlReader.Create(stream, settings);
        command = serializer.Deserialize (reader);
        //xmldoc.

/*
                using (XmlReader xmlr = XmlReader.Create(reader)) {
                    XmlDocument xml = new XmlDocument();

                }
                */

//                    // serializer lock, making it thread safe.
//                    lock (SerializerLock) {
//                        c = Serializer.Deserialize(reader);
//                       reader.Close();
//                   }
      }

      if (!(command is ICommand)) {
        // TODO throw exception
        return default(ICommand);
      }


      return (ICommand)command;
    }

    public Stream ToStream(ICommand command, Encoding encoding) {

      return default(Stream);
    }

    public byte[] ToByteArray(ICommand command, Encoding encoding) {
      return ToByteArray (command, encoding, true);
    }
		
    public byte[] ToByteArray(ICommand command, Encoding encoding, bool nil) {
      // TODO multiple xml serializers could potentially be harmful to the memory space.
      // should think obout serializer/desrializing in a better way, perhaps keeping 
      // serializers for individual types of classes and thread-safing them, could
      // potentially write a factory-serializer which incompasses the serializer/des...
      Type type = command.GetType ();
      XmlSerializer serializer = new XmlSerializer(type);
			
      MemoryStream memory = new MemoryStream();
      //StreamWriter writer = new StreamWriter(memory, encoding);
			
      // setup the xml writer for the appropriate encoding type.
      XmlWriterSettings settings = new XmlWriterSettings();
			
      settings.Encoding = encoding;
      settings.Indent = false;
      XmlWriter writer = XmlWriter.Create (memory, settings);
      serializer.Serialize (writer, command);
			
      // TODO this should be part of the a setting?
      memory.WriteByte ((byte)'\0');
			
      return memory.ToArray ();
    }
  }
}
