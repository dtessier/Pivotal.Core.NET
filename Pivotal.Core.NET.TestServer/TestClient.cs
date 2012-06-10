using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using Pivotal.Core.NET.Codec;
using Pivotal.Core.NET.Sockets;
using Pivotal.Core.NET.Command;

namespace Pivotal.Core.NET.TestServer {
  public class TestClient {
    protected ClientSocket client;
    protected static volatile Int16 sequencer = 0;
		
    public TestClient() {
			
    }
		
    public void Go() {
      client = new ClientSocket();
      client.OnConnected += new Sockets.OnConnectDelegate(OnConnected);
      client.OnReceived += new Sockets.OnReceiveDelegate(OnReceived);
			
      client.AddReceiveEventHandler (typeof(EchoCommand), OnEcho);
      client.AddReceiveEventHandler (typeof(WhatTimeIsItCommand), OnWhatTimeIsIt);
			
      // use the xml buffer codec for this client endpoint.
      client.BufferCodec = new XmlBufferCodec();
			
      // NOTE that this will happen several times, since the Server and Client testing
      // is done in the same main running process, as such, multiple scanning will
      // happen, in a real-life program, this will not happen, since each process is
      // self contained, and should theoritically handle its own SET.
      client.BufferCodec.ScanForIdentity (
        "Pivotal.Core.NET",
        "^Pivotal.Core.NET.*Command$"
      );
      client.BufferCodec.ScanForIdentity (
        "Pivotal.Core.NET.TestServer",
        "^Pivotal.Core.NET.*Command$"
      );
			
      client.Connect ("127.0.0.1", 8090);
    }
		
    public void OnEcho(IClientSocket socket, ICommand command) {
      EchoCommand reply = (EchoCommand)command;
      Console.Out.WriteLine ("< Client received echo-reply on seq: " + reply.Sequence);
			
      WhatTimeIsItCommand wtit = new WhatTimeIsItCommand();
      client.Send (wtit);
    }

    protected void OnWhatTimeIsIt(IClientSocket client, ICommand command) {
      WhatTimeIsItCommand wtit = (WhatTimeIsItCommand)command;
      Console.Out.WriteLine (String.Format (
        "< Server replied back with time is {0} for client {1}",
        wtit.TheTime,
        client
      ));
    }
		
    protected void OnConnected(IClientSocket client) {
      // set it to asynchronous mode
      client.OnReceivedInvokeAsynchronously = true;
			
      System.Diagnostics.Debug.WriteLine (String.Format (
        "Test Client Connected {0}",
        client
      ));
      EchoCommand echo = new EchoCommand();
      echo.Reply = false;
      echo.Sequence = ++sequencer;
      client.Send (echo);
		
    }

    public void OnReceived(IClientSocket client, ICommand command) {
      Console.WriteLine ("Received command which means no method handler was found: " + command.ToString ());
    }
  }
}

