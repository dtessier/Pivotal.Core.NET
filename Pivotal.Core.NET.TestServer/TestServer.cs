using System;
using System.Threading;

using System.Collections;
using System.Collections.Generic;

using Pivotal.Core.NET.Codec;
using Pivotal.Core.NET.Sockets;
using Pivotal.Core.NET.Command;

namespace Pivotal.Core.NET.TestServer {
	
  public class TestServer : AbstractServer {

		
    private List<TestClient> testclients = new List<TestClient>();
			
    public TestServer() {	
      IPv4 = "127.0.0.1";
      IPv4PortNo = 8090;
			
      // the default codec for this server is xml, each client that connects 
      // will be given the Xml Codec, which can be changed on a one-by-one basis.
      DefaultBufferCodec = new XmlBufferCodec();
			
      // event handler for all echo commands received from clients.
      AddReceiveEventHandler (typeof(EchoCommand), OnEcho);
      AddReceiveEventHandler (typeof(WhatTimeIsItCommand), OnWhatTimeIsIt);
    }

    protected void OnWhatTimeIsIt(IClientSocket client, ICommand command) {
				
      Console.Out.WriteLine (String.Format (
        "> Server received what time it is request from {0}",
        client
      ));
			
      String theTime = DateTime.Now.TimeOfDay.ToString ();
      WhatTimeIsItCommand wtit = (WhatTimeIsItCommand)command;
      wtit.TheTime = theTime;
      client.Send (wtit);
    }
	
    /// <summary>
    /// Raised when the echo command is received by a client
    /// </summary>
    /// <param name='client'>
    /// Client.
    /// </param>
    /// <param name='echo'>
    /// Echo.
    /// </param>
    protected void OnEcho(IClientSocket client, ICommand echo) {
       
      EchoCommand request = (EchoCommand)echo;
			
      Console.Out.WriteLine ("> Server received echo request, seq: " + request.Sequence);
			
      EchoCommand reply = new EchoCommand();
      reply.Reply = true;
      reply.Sequence = request.Sequence;
      client.Send (reply);
			
    }

    /// <summary>
    /// Raised when a client connects successfully 
    /// </summary>
    /// <param name='client'>
    /// 
    /// </param>
    public override void OnConnect(IClientSocket client) {
      base.OnConnect (client);
			
      // set it to asynchronous mode
      client.OnReceivedInvokeAsynchronously = true;
			
    }
		
    /// <summary>
    ///  Raised when the server started successfully. 
    /// </summary>
    protected override void OnStarted() {
			
      // Scan: for command types such that we can serialize and deserialize them properly
      // Note: you can also add command identities manually to the Codec.
      DefaultBufferCodec.ScanForIdentity (
        "Pivotal.Core.NET",
        "^Pivotal.Core.NET.*Command$"
      );
      DefaultBufferCodec.ScanForIdentity (
        "Pivotal.Core.NET.TestServer",
        "^Pivotal.Core.NET.*Command$"
      );
			
      for (int i = 0; i < 100; i++) {
        // server started.
        TestClient client = new TestClient();
        testclients.Add (client);
        client.Go ();
      }
			
				
    }

    public override void OnReceived(IClientSocket client, ICommand command) {
      // this must get called if you wish to handle specific command-type event handlers,
      // via the AddReceventEventHandler method call, otherwise, each command will arrive
      // at this point and would need to be delegated manually.
      base.OnReceived (client, command); 
    }
        
    public override void OnSent(IClientSocket client, ICommand command) {
      base.OnSent (client, command);
    }

  }
	
}

