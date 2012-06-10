using System;
using System.Text;

using Pivotal.Core.NET.Command;

namespace Pivotal.Core.NET
{
	public interface ICodecSerializable {
		byte[] Serialize(Encoding encoding);
		ICommand Deserialize(byte[] buf, Encoding encoding); 
	}
}

