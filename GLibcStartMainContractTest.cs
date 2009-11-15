// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using NUnit.Framework;

namespace bugreport
{
    [TestFixture]
    public class GLibcStartMainContractTest
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            var registers = new RegisterCollection();
            registers[RegisterName.ESP] = new AbstractValue(AbstractValue.GetNewBuffer(1));
            state = new MachineState(registers);
            contract = new GLibcStartMainContract();
        }

        #endregion

        private GLibcStartMainContract contract;
        private MachineState state;

        [Test]
        public void Execute()
        {
            var address = new AbstractValue(0xdeadbabe);
            state = state.PushOntoStack(address);
            state = contract.Execute(state);
            Assert.AreEqual(address.Value, state.InstructionPointer);
        }

        [Test]
        public void IsSatisified()
        {
            var code = new Byte[] {0xe8, 0xb7, 0xff, 0xff, 0xff};
            state = new MachineState {InstructionPointer = 0x80482fc};
            Assert.IsTrue(contract.IsSatisfiedBy(state, code));

            state.InstructionPointer = 0xdeadbeef;
            Assert.IsFalse(contract.IsSatisfiedBy(state, code));
        }
    }
}