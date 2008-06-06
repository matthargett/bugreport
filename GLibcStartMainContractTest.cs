using NUnit.Framework;
using System;

namespace bugreport
{
[TestFixture]
public class GLibcStartMainContractTest
{
    GLibcStartMainContract contract;
    MachineState state;
    
    [SetUp]
    public void SetUp()
    {
        RegisterCollection registers = new RegisterCollection();
        registers[RegisterName.ESP] = new AbstractValue(AbstractValue.GetNewBuffer(1));
        state = new MachineState(registers);
        contract = new GLibcStartMainContract();
    }

    [Test]
    public void IsSatisified()
    {
        Byte[] code = new Byte[] {0xe8, 0xb7, 0xff, 0xff, 0xff};        
        state = new MachineState();
        state.InstructionPointer = 0x80482fc;                    
        Assert.IsTrue(contract.IsSatisfiedBy(state, code));
        
        state.InstructionPointer = 0xdeadbeef;
        Assert.IsFalse(contract.IsSatisfiedBy(state, code));
    }
    
    [Test]
    public void Execute()
    {
        AbstractValue address = new AbstractValue(0xdeadbabe);
        state = state.PushOntoStack(address);
        state = contract.Execute(state);        
        Assert.AreEqual(address.Value, state.InstructionPointer);
    }
}
}
