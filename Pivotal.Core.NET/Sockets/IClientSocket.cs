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

using Pivotal.Core.NET.Command;
using Pivotal.Core.NET.Codec;

namespace Pivotal.Core.NET.Sockets {

  /// <summary>
  /// Interface defining the client socket layer
  /// </summary>
  public interface IClientSocket {

    /// <summary>
    /// Property identifying whether the command received from the end point should be passed
    /// from the client socket to <see cref="OnReceiveDelegate"/> asynchronously, or synchronously.
    /// 
    /// Dependant on implementation of the this interface.
    /// </summary>
    Boolean OnReceivedInvokeAsynchronously { get; set; }

    /// <summary>
    /// Event notification when connection is completed
    /// </summary>
    event OnConnectDelegate OnConnected;

    /// <summary>
    /// Event notification when a new command is received from remote-end-point
    /// </summary>
    event OnReceiveDelegate OnReceived;

    /// <summary>
    /// Event notification when command has been sent (NOT necessarily received) to remote-end-point
    /// </summary>
    event OnSentDelegate OnSent;

    /// <summary>
    /// Connect to remote end point.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    void Connect(String host, Int32 port);
    
    /// <summary>
    /// Gets or sets the buffer codec.
    /// </summary>
    /// <value>
    /// The buffer codec.
    /// </value>
    IBufferCodec BufferCodec { get; set; }
    
    /// <summary>
    /// Send the specified command.
    /// </summary>
    /// <param name='command'>
    /// Command.
    /// </param>
    void Send(ICommand command);
  }
}