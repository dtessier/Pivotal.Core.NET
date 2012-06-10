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
using System.Threading;

using System.Diagnostics;

using System.Text;

using System.Collections;
using System.Collections.Generic;

using Pivotal.Core.NET.Command;
using Pivotal.Core.NET.Sockets;
using Pivotal.Core.NET.Utilities;
using Pivotal.Core.NET.Codec;

namespace Pivotal.Core.NET.Sockets {

  /// <summary>
  /// Abstract server class used as a base class for all server types.
  /// </summary>
  public abstract class AbstractServer : AbstractNetworking, IServer {
    
    /// <summary>
    /// Default thread loop, every 10 seconds. (use new to override)
    /// </summary>
    protected Int32 SERVER_LOOP_INTERVAL = 10000;

    /// <summary>
    /// Default dead socket cleanup, every minute. (use new to override)
    /// </summary>
    protected Int32 DEAD_SOCKET_CLEANUP_INTERVAL = 10000;

    /// <summary>
    /// Server ID
    /// </summary>
    public Int32 Id { get; set; }
  
    /// <summary>
    /// Server Name
    /// </summary>
    public String ServerName { get; set; }

    /// <summary>
    /// Internal socket listener
    /// </summary>
    protected Socket _listener;

    /// <summary>
    /// Interal server loop
    /// </summary>
    protected Thread _runner;
    
    /// <summary>
    /// IP version 4 Bind Address
    /// </summary>
    protected IPAddress _IPv4;

    /// <summary>
    /// IP version 6 Bind Address
    /// </summary>
    protected IPAddress _IPv6;

    /// <summary>
    /// IP version 4 - Binding Port
    /// </summary>
    protected Int32 _IPv4_PORT_NO;

    /// <summary>
    /// IP version 6 - Binding Port
    /// </summary>
    protected Int64 _IPv6_PORT_NO;

    /// <summary>
    /// Maximum Receive Size of the Socket
    /// </summary>
    public Int32 MaxReceiveSize { get; set; }
    
    /// <summary>
    /// Maximum Listener Queue Length
    /// </summary>
    public Int32 MaxQueueLength { get; set; }

    /// <summary>
    /// Default encoding type used for processing text content on a socket stream. (read and write)
    /// </summary>
    public Encoding StreamEncoding { get; set; }
    
    /// <summary>
    /// Gets or sets the default client buffer codec.
    /// </summary>
    /// <value>
    /// The default client buffer codec.
    /// </value>
    public IBufferCodec DefaultBufferCodec { get; set; }

    /// <summary>
    /// List of sockets connected to this server instance
    /// </summary>
    protected List<IClientSocket> Clients = new List<IClientSocket>();

    /// <summary>
    /// Default constructor, defines default values
    /// </summary>
    public AbstractServer() {
      // do nothing.
      MaxReceiveSize = 0;
      MaxQueueLength = 150;

      // create a random server  name
      ServerName = StringUtils.CreateRandomString (
        "SVR-",
        String.Empty,
        10,
        true,
        true
      )
        .ToUpper ();
    }

    /// <summary>
    /// Check listener active status
    /// </summary>
    public Boolean IsActive {
      get {
        // TODO needs better checking
        return _listener != null;
      }
    }
    
    public Int32 IPv4PortNo { 
      get { return _IPv4_PORT_NO; } 
      set {
        if (!IsActive) {
          _IPv4_PORT_NO = value;
        } else {
          throw new InvalidOperationException("Cannot change ports while socket is listening.");
        }
      }
    }
    
    public String IPv4 { 
      set {
        if (!IPAddress.TryParse (value, out _IPv4)) {
          throw new FormatException(String.Format (
            "IPv4 {0} is not a valid, must be in the format a valid ip address. Example 10.10.10.100",
            IPv4PortNo
          ));
        }
      }
      get {
        return _IPv4.ToString ();
      }
    }

    /// <summary>
    /// Synchronized (thread-safe) way of adding a new socket to the client list.
    /// </summary>
    /// <param name="socket"></param>
    /// <returns></returns>
    protected virtual bool AddClient(IClientSocket socket) {
      lock (Clients) {
        Clients.Add (socket);
      }
      return false;
    }

    /// <summary>
    /// Start the Listener(s).
    /// </summary>
    public virtual void Start() {

      if (_IPv4 != null) {
        EndPoint endpoint = new IPEndPoint(_IPv4, IPv4PortNo);
        
        _listener = new Socket(
          AddressFamily.InterNetwork,
          SocketType.Stream,
          ProtocolType.Tcp
        );
        _listener.Bind (endpoint);
        _listener.Listen (MaxQueueLength);
        _listener.BeginAccept (
          MaxReceiveSize,
          new AsyncCallback (AcceptCallback),
          _listener
        );

        #if DEBUG

        // TODO logger
        String output = String.Format (
          "| {0} started on IP version 4 on {1}:{2} |",
          ServerName,
          IPv4,
          IPv4PortNo
        );
        String pad = "-".PadRight (output.Length, '-');
        Console.Out.WriteLine (pad);
        Console.Out.WriteLine (output);
        Console.Out.WriteLine (pad);
        #endif
        
        OnStarted ();
      }
      
      // TODO IPv6

      if (IsActive) {
        ThreadStart start = new ThreadStart(Runner);
        Thread runner = new Thread(start);
        _runner = runner;
        _runner.Start ();
      }
    }
    
    /// <summary>
    /// Raises the started event.
    /// </summary>
    protected abstract void OnStarted();
    
    /// <summary>
    /// Loop, keeps the process alive
    /// </summary>
    protected void Runner() {
      for (;;) {
        // TODO logger
        Debug.WriteLine (String.Format (
          "Server Loop {0} ms",
          SERVER_LOOP_INTERVAL
        ));
     
        Thread.Sleep (SERVER_LOOP_INTERVAL);
      }
    }
    
    public virtual void Stop() {
      _runner.Abort ();
      // TODO probably need to kill the async accept callback wait.
    }

    /// <summary>
    /// Abstract OnConnect, must be implemented.
    /// </summary>
    /// <param name="client"></param>
    public virtual void OnConnect(IClientSocket client) {
      // do nothing
      Debug.WriteLine (String.Format ("Client connected to server via ", client));
    }

    /// <summary>
    /// Abstract OnReceived, must be implemented.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="command"></param>
    public virtual void OnReceived(IClientSocket client, ICommand command) {

      // IMPORTANT: the asynchronous handling of this event should already be 
      // handled at the socket level, and as such, should not be double handled 
      // at the server layer.
      if (!AttemptReceiveEventHandler (client, command, false)) {
        // should be override...
      }
    }

    /// <summary>
    /// Abstract OnSent, must be implemented
    /// </summary>
    /// <param name="client"></param>
    /// <param name="command"></param>
    public virtual void OnSent(IClientSocket client, ICommand command) {
      // do nothing, yet.
    }

    /// <summary>
    /// Accept Callback, ends and registers the events to the local send; receive; connect
    /// </summary>
    /// <param name="result"></param>
    protected virtual void AcceptCallback(IAsyncResult result) {
      // probably needs a locking mechanism?
      Socket listener = (Socket)result.AsyncState;
      if (result.IsCompleted) {
        Socket socket = listener.EndAccept (result);
        
        // TODO the implementation must be a factory or virtual?
        IClientSocket client = new ClientSocket(
          socket,
          StreamEncoding,
          DefaultBufferCodec
        );

        client.OnConnected += new Sockets.OnConnectDelegate(OnConnect);
        client.OnReceived += new Sockets.OnReceiveDelegate(OnReceived);
        client.OnSent += new Sockets.OnSentDelegate(OnSent);
        ((ClientSocket)client).InvokeConnected ();

        // Create a new socket wrappper and add it to the client list.
        AddClient (client);
        
        // begin accept again
        listener.BeginAccept (
          MaxReceiveSize,
          new AsyncCallback(AcceptCallback),
          listener
        );
      }
    }
  }
}