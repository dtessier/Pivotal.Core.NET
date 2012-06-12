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
using System.Collections;

using System.Diagnostics;

using Pivotal.Core.NET.Command;
using Pivotal.Core.NET.Sockets;

namespace Pivotal.Core.NET.Sockets {
  
  /// <summary>
  /// Handler command event class, used to delegate specific types of commands to a given delegate method.
  /// </summary>
  public class HandlerCommandEvent {
    public Type Type { get; set; }

    public event OnReceiveDelegate ReceiveEvent;
  }
  
  /// <summary>
  /// Abstract Networking abstraction layer, handles common tasks between sockets and server processes. 
  /// 
  /// For example: this class is implemented by both the AbstractServer and ClientSocket classes, which will allow
  /// the program to add handler delegate methods for specific types of commands, e.g. EchoCommand.
  ///
  /// </summary>
  /// 
  /// <
  public abstract class AbstractNetworking {

    /// <summary>
    /// The handler event commands.
    /// </summary>
    protected Hashtable EventHandlerCommands = new Hashtable();

    /// <summary>
    /// Determines whether this instance is receive event handler type the specified type.
    /// </summary>
    /// <returns>
    /// <c>true</c> if this instance is receive event handler type the specified type; otherwise, <c>false</c>.
    /// </returns>
    /// <param name='type'>
    /// Type.
    /// </param>
    public virtual HandlerCommandEvent IsReceiveEventHandlerType(Type type) {
      if (EventHandlerCommands.ContainsKey (type)) {
        return (HandlerCommandEvent)EventHandlerCommands [type];
      }
      return default(HandlerCommandEvent);
    }

    /// <summary>
    /// Adds a delegate method for a given Command type, e.g. EchoCommand -> OnEcho(..).
    /// </summary>
    /// <param name='type'>
    /// Type of the class, should implement ICommand, but not yet strict.
    /// </param>
    /// <param name='e'>
    /// E. is the OnReceiveDelegate method pointer
    /// </param>
    public virtual void AddReceiveEventHandler(Type type, OnReceiveDelegate e) {
      if (!typeof(ICommand).IsAssignableFrom (type)) {
        throw new ArgumentException(String.Format (
          "Illegal command type specified {0}, must be of type {1}",
          type,
          typeof(ICommand))
        );
      }

      HandlerCommandEvent co = null;
      if (EventHandlerCommands.ContainsKey (type)) {
        co = (HandlerCommandEvent)EventHandlerCommands [type];
      } else {
        co = new HandlerCommandEvent();
        EventHandlerCommands.Add (type, e);
      }

      co.ReceiveEvent += e;
      co.Type = type;
    }
    
    /// <summary>
    /// Attempts to execute the delegate method, either asynchronously, or synchronous.
    /// 
    /// Will pass the command, and client socket instances to the delegate method.
    /// 
    /// </summary>
    /// <returns>
    /// The receive event handler.
    /// </returns>
    /// <param name='client'>
    /// An instnace of the IClientSocket interface.
    /// </param>
    /// <param name='command'>
    /// The command object, should correspond to the type of delegate method handled, e.g. EchoCommand.
    /// </param>
    /// <param name='async'>
    /// If set to <c>true</c> then execute asynchronously.
    /// </param>
    protected virtual bool AttemptReceiveEventHandler(IClientSocket client, ICommand command, bool async) {
      if (command == null) {
        Debug.WriteLine (String.Format (
          "Invalid command was received, null pointer for client {0}",
          client
        )
        );
        // TODO error on socket??
        return false;
      }
      
      Type type = command.GetType ();
      //  Object serial = client.BufferCodec.BuildSerial (type);
      
      if (!EventHandlerCommands.ContainsKey (type)) {
        Debug.WriteLine (String.Format (
          "Command {0} with serial {1} received for Socket {2} on " +
          "Server type {3} but no OnReceiveDelegate event handler found for command type",
          command,
          type,
          client,
          this)
        );
        
        return false;
      }
      
      OnReceiveDelegate deleg = (OnReceiveDelegate)EventHandlerCommands [type];
      if (async) {
        deleg.BeginInvoke (
          client,
          command,
          BeginInvokeReceiveEventHandlerCallback,
          deleg
        );
      } else {
        deleg.Invoke (client, command);
      }
      
      return true;
    }
    
    /// <summary>
    /// Begins the invoke receive event handler callback, ending the invocation.
    /// </summary>
    /// <param name='result'>
    /// Result.
    /// </param>
    protected virtual void BeginInvokeReceiveEventHandlerCallback(IAsyncResult result) {
      OnReceiveDelegate deleg = (OnReceiveDelegate)result.AsyncState;
      deleg.EndInvoke (result);
    }
    
  }
}

