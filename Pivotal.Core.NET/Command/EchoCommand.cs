using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.Xml.Serialization;

namespace Pivotal.Core.NET.Command {
    public class EchoCommand :  ICommand, IBinarySerializable {
        public Boolean Reply { get; set; }
		public Int16 Sequence { get; set; }
		
        public EchoCommand() {
            Reply = false;
        }
		
		public Int16 BinarySerial() {
			return 1000;
		}
		
		public byte[] BinarySerialize(Encoding encoding) {
			return null;
		}
		
		public ICommand BinaryDeserialize(byte[] buf, Encoding encoding) {
			return this;
		}
    }
}
