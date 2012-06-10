using System;
using System.Collections.Generic;
using System.Text;

namespace Pivotal.Core.NET.Command {
  public class SwitchEncodingCommand :  ICommand {
    public String Encoding { get; set; }

    public SwitchEncodingCommand(Encoding encoding) {
      Encoding = encoding.ToString ();
    }
  }
}
