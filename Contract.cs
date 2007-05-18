using System;

namespace bugreport
{
public abstract class Contract
{
    protected Opcode opcode = new X86Opcode();

    public abstract Boolean IsSatisfiedBy(MachineState state, Byte[] code);
    public abstract MachineState Execute(MachineState state);
}
}
