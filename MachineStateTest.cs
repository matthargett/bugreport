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
    public class MachineStateTest
    {
        private readonly AbstractValue one = new AbstractValue(1);
        private readonly AbstractValue two = new AbstractValue(2).AddTaint();
        private MachineState state;

        private AbstractValue eax
        {
            get { return state.Registers[RegisterName.EAX]; }
            set { state.Registers[RegisterName.EAX] = value; }
        }

        private AbstractValue ebx
        {
            get { return state.Registers[RegisterName.EBX]; }
            set { state.Registers[RegisterName.EBX] = value; }
        }

        private AbstractValue esp
        {
            set { state.Registers[RegisterName.ESP] = value; }
        }

        [SetUp]
        public void SetUp()
        {
            state = new MachineState(new RegisterCollection());
        }

        [Test]
        public void Add()
        {
            eax = one;
            ebx = two;
            state = state.DoOperation(RegisterName.EAX, OperatorEffect.Add, RegisterName.EBX);

            Assert.AreEqual(3, eax.Value);
            Assert.IsTrue(eax.IsTainted);
        }

        [Test]
        public void And()
        {
            eax = new AbstractValue(0x350).AddTaint();
            ebx = new AbstractValue(0xff);
            state = state.DoOperation(RegisterName.EAX, OperatorEffect.And, RegisterName.EBX);

            Assert.AreEqual(0x50, eax.Value);
            Assert.IsTrue(eax.IsTainted);
        }

        [Test]
        public void Assignment()
        {
            eax = one;
            ebx = two;
            state = state.DoOperation(RegisterName.EAX, OperatorEffect.Assignment, RegisterName.EBX);

            Assert.AreEqual(eax, ebx);
            Assert.AreNotSame(eax, ebx);
        }

        [Test]
        public void AssignmentRetainsOOB()
        {
            var oob = new AbstractValue(1);
            oob.IsOOB = true;

            state = state.DoOperation(RegisterName.EAX, OperatorEffect.Assignment, oob);
            Assert.IsTrue(eax.IsOOB);
        }

        [Test]
        public void Copy()
        {
            state.Registers[RegisterName.ESP] = new AbstractValue(new AbstractBuffer(AbstractValue.GetNewBuffer(10)));
            var newState = new MachineState(state);
            Assert.AreNotSame(newState, state);
            Assert.AreNotSame(newState.Registers, state.Registers);
            Assert.AreNotSame(newState.DataSegment, state.DataSegment);
            Assert.AreNotSame(newState.ReturnValue, state.ReturnValue);
            Assert.AreSame(newState.TopOfStack, state.TopOfStack);
        }

        [Test]
        public void DoOperationForShl()
        {
            eax = one.AddTaint();
            ebx = new AbstractValue(0x3);
            state = state.DoOperation(RegisterName.EAX, OperatorEffect.Shl, RegisterName.EBX);

            Assert.AreEqual(0x8, eax.Value);
            Assert.IsTrue(eax.IsTainted);
        }

        [Test]
        public void Equality()
        {
            var same = new MachineState(new RegisterCollection());
            same.DataSegment[0] = new AbstractValue(2);
            var same2 = new MachineState(new RegisterCollection());
            same2.DataSegment[0] = new AbstractValue(2);

            var registers = new RegisterCollection();
            registers[RegisterName.EAX] = new AbstractValue(1);
            var differentViaRegisters = new MachineState(registers);
            differentViaRegisters.DataSegment[0] = new AbstractValue(2);

            Assert.IsTrue(same.Equals(same2));
            Assert.IsFalse(differentViaRegisters.Equals(same));

            Assert.IsTrue(same == same2);
            Assert.IsTrue(same != differentViaRegisters);

            Assert.AreEqual(same.GetHashCode(), same2.GetHashCode());
            Assert.AreNotEqual(same.GetHashCode(), differentViaRegisters.GetHashCode());

            registers = new RegisterCollection();
            var differentViaDataSegmentKey = new MachineState(registers);
            differentViaDataSegmentKey.DataSegment[1] = new AbstractValue(2);

            Assert.IsFalse(same.Equals(differentViaDataSegmentKey));

            registers = new RegisterCollection();
            var differentViaDataSegmentValue = new MachineState(registers);
            differentViaDataSegmentValue.DataSegment[0] = new AbstractValue(1);

            Assert.IsFalse(same.Equals(differentViaDataSegmentValue));
        }

        [Test]
        public void Jnz()
        {
            Byte offset = 6;
            eax = two;
            ebx = one;
            state = state.DoOperation(RegisterName.EAX, OperatorEffect.Cmp, RegisterName.EAX);
            state = state.DoOperation(OperatorEffect.Jnz, new AbstractValue(offset));
            Assert.AreEqual(0, state.InstructionPointer);

            state = state.DoOperation(RegisterName.EAX, OperatorEffect.Cmp, RegisterName.EBX);
            state = state.DoOperation(OperatorEffect.Jnz, new AbstractValue(offset));
            Assert.AreEqual(offset, state.InstructionPointer);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void JnzPointerOffset()
        {
            var pointer = new AbstractValue(AbstractValue.GetNewBuffer(1));
            state = state.DoOperation(OperatorEffect.Jnz, pointer);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void NonAssignmentOfPointer()
        {
            eax = one;
            ebx = new AbstractValue(new[] {two});
            state = state.DoOperation(RegisterName.EAX, OperatorEffect.Add, RegisterName.EBX);
        }

        [Test]
        public void OperationResultEquality()
        {
            var same = new OperationResult(new AbstractValue(1), false);
            var same2 = new OperationResult(new AbstractValue(1), false);
            var different = new OperationResult(new AbstractValue(2), true);

            Assert.IsTrue(same.Equals(same2));
            Assert.IsFalse(same.Equals(different));

            Assert.IsTrue(same == same2);
            Assert.IsTrue(same != different);

            Assert.AreEqual(same.GetHashCode(), same2.GetHashCode());
            Assert.AreNotEqual(same.GetHashCode(), different.GetHashCode());
        }

        [Test]
        public void PointerAdd()
        {
            AbstractValue[] buffer = AbstractValue.GetNewBuffer(0x10);
            buffer[4] = one;
            eax = new AbstractValue(buffer);

            state = state.DoOperation(RegisterName.EAX, OperatorEffect.Add, new AbstractValue(0x4));
            Assert.AreEqual(one, eax.PointsTo[0]);
        }

        [Test]
        public void PointerAnd()
        {
            AbstractValue[] buffer = AbstractValue.GetNewBuffer(0x10);
            buffer[4] = one;

            eax = new AbstractValue(buffer);

            state = state.DoOperation(RegisterName.EAX, OperatorEffect.Add, new AbstractValue(0x4));
            Assert.AreEqual(one, eax.PointsTo[0]);

            var andValue = new AbstractValue(0xfffffff0);
            MachineState newState = state.DoOperation(RegisterName.EAX, OperatorEffect.And, andValue);
            Assert.AreNotSame(newState, state);
            Assert.AreNotEqual(newState, state);

            state = newState;
            Assert.AreEqual(one, eax.PointsTo[4]);
        }

        [Test]
        public void PointerSub()
        {
            AbstractValue[] buffer = AbstractValue.GetNewBuffer(0x10);
            buffer[0] = one;
            eax = new AbstractValue(buffer);
            ebx = new AbstractValue(0x4);
            state = state.DoOperation(RegisterName.EAX, OperatorEffect.Add, RegisterName.EBX);
            Assert.IsNotNull(eax.PointsTo);
            state = state.DoOperation(RegisterName.EAX, OperatorEffect.Sub, RegisterName.EBX);
            Assert.AreEqual(one, eax.PointsTo[0]);
        }

        [Test]
        public void PushPopThenAssignToTop()
        {
            AbstractValue[] buffer = AbstractValue.GetNewBuffer(0x20);
            esp = new AbstractValue(buffer);
            state = state.PushOntoStack(one);

            // TODO(matt_hargett): extract into state.PopOffStack()
            state = state.DoOperation(RegisterName.ESP, OperatorEffect.Sub, new AbstractValue(0x4));
            state = state.DoOperation(RegisterName.ESP, 0, OperatorEffect.Assignment, two);
            Assert.AreEqual(two, state.TopOfStack);
        }

        [Test]
        public void PushTwiceThenManuallyAdjustStack()
        {
            AbstractValue[] buffer = AbstractValue.GetNewBuffer(0x20);
            esp = new AbstractValue(buffer);
            state = state.PushOntoStack(one);
            state = state.PushOntoStack(two);

            state = state.DoOperation(RegisterName.ESP, OperatorEffect.Sub, new AbstractValue(0x4));
            Assert.AreEqual(one, state.TopOfStack);
        }

        [Test]
        public void Shr()
        {
            eax = new AbstractValue(0x8).AddTaint();
            ebx = new AbstractValue(0x3);
            state = state.DoOperation(RegisterName.EAX, OperatorEffect.Shr, RegisterName.EBX);

            Assert.AreEqual(0x1, eax.Value);
            Assert.IsTrue(eax.IsTainted);
        }

        [Test]
        public void Sub()
        {
            eax = one;
            ebx = two;
            state = state.DoOperation(RegisterName.EBX, OperatorEffect.Sub, RegisterName.EAX);

            Assert.AreEqual(1, ebx.Value);
            Assert.IsTrue(ebx.IsTainted);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Unknown()
        {
            state.DoOperation(RegisterName.EAX, OperatorEffect.Unknown, RegisterName.EBX);
        }

        [Test]
        public void Xor()
        {
            eax = new AbstractValue(0xff).AddTaint();
            ebx = new AbstractValue(0xff);
            state = state.DoOperation(RegisterName.EAX, OperatorEffect.Xor, RegisterName.EBX);

            Assert.AreEqual(0x0, eax.Value);
            Assert.IsTrue(eax.IsTainted);
        }
    }
}