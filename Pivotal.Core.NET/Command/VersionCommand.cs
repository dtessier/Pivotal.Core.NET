using System;
using System.Collections.Generic;
using System.Text;

namespace Pivotal.Core.NET.Command {
  public class VersionCommand :  ICommand {
    public String Version { get; set; }

    public VersionCommand(Encoding encoding) {

    }
  }
}
