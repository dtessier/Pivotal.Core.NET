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
using System.Net;
using System.Net.Sockets;
using Pivotal.Core.NET.Command;
using Pivotal.Core.NET.Sockets;

namespace Pivotal.Core.NET.Sockets {
	
	/// <summary>
	/// IServer interface
	/// </summary>
	public interface IServer {
		/// <summary>
		/// Gets or sets the servers identifier, should be unique across a domain of servers.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		Int32 Id { get; set; }

		/// <summary>
		/// Start this instance.
		/// </summary>
		void Start();
		
		/// <summary>
		/// Stop this instance.
		/// </summary>
		void Stop();
		
		
		/// <summary>
		/// Raised when a client socket connects to the server, connect event.
		/// </summary>
		/// <param name='client'>
		/// Client.
		/// </param>
        void OnConnect(IClientSocket client);
		
		/// <summary>
		/// Raised when the server receives a known command type, BUT has not 
		/// been explicitly defined as part of the HandlerEventCommands.
		/// </summary>
		/// <param name='client'>
		/// Client.
		/// </param>
		/// <param name='command'>
		/// Command.
		/// </param>
        void OnReceived(IClientSocket client, ICommand command);
		
		/// <summary>
		/// Raised when the server finishes sending a command on the specified socket.
		/// </summary>
		/// <param name='client'>
		/// Client.
		/// </param>
		/// <param name='command'>
		/// Command.
		/// </param>
        void OnSent(IClientSocket client, ICommand command);
	}
}

