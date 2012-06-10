using System;

using System.IO;
using System.Text;

using Pivotal.Core.NET.Command;

namespace Pivotal.Core.NET {
	public class BinarySerializer {
		
		
		public BinarySerializer () {
			
		}

		public void Serializer(ICommand command, Encoding encoding) {
			// TODO should use some sort of buffered stream? buffer to disk, or other based on size?
			Stream stream = new MemoryStream();
		
			
		}
		
		public void Deserialize(byte[] ba, Encoding encoding) {
		
		}
		
	}
}

