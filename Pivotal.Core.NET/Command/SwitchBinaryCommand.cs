using System;
using System.Collections.Generic;
using System.Text;

namespace Pivotal.Core.NET.Command {
  public class SwitchBinaryCommand :  ICommand {
    public Boolean BinaryMode { get; set; }

    public SwitchBinaryCommand(Boolean binaryMode) {
      BinaryMode = binaryMode;
    }
  }
}
