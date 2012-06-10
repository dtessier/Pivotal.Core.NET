using System;

using System.Reflection;
using System.Text.RegularExpressions;

using Pivotal.Core.NET;
using Pivotal.Core.NET.Sockets;

namespace Pivotal.Core.NET
{
	class MainClass
	{
		public static void Main(string[] args) {		
			TestServer server = new TestServer();
			server.Start();
		}
		
		public static void DoClientTest () {
			TestClient client = new TestClient();
			
		}
	}
}
