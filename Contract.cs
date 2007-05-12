using System;

namespace bugreport
{
public interface Contract
{
    Boolean IsSatisfiedBy(MachineState state, Byte[] code);
    MachineState Execute(MachineState state);
}
}
