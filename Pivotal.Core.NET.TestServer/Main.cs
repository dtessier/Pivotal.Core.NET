using System;

namespace Pivotal.Core.NET.TestServer {
	class MainClass {
		public static void Main(string[] args) {
			TestServer server = new TestServer();
			server.Start ();
		}
	}
}
