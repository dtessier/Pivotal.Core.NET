using System;
using Pivotal.Core.NET.Command;

namespace Pivotal.Core.NET.TestServer {
	public class WhatTimeIsItCommand : ICommand {
		public String TheTime { get; set; }
			
		public WhatTimeIsItCommand() {

		}
	}
}

