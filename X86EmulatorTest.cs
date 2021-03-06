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
    public class X86EmulatorTest
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            state = new MachineState(new RegisterCollection());
            reportItems = new ReportCollection();
            var buffer = new AbstractBuffer(AbstractValue.GetNewBuffer(0x200));
            var pointer = new AbstractValue(buffer);
            state.Registers[RegisterName.ESP] = pointer;
            state = state.DoOperation(RegisterName.EBP, OperatorEffect.Assignment, RegisterName.ESP);
        }

        #endregion

        private readonly AbstractValue one = new AbstractValue(1);
        private MachineState state;
        private Byte[] code;
        private ReportCollection reportItems;

        private void PrintStackAndBlock()
        {
            Console.WriteLine(
                "EIP: {5:x8}\tESP-8: {0}\tESP-4: {1}\tESP: {2}\tESP+4: {3}\tESP+8: {4}\t",
                state.Registers[RegisterName.ESP].PointsTo[-8],
                state.Registers[RegisterName.ESP].PointsTo[-4],
                state.Registers[RegisterName.ESP].PointsTo[0],
                state.Registers[RegisterName.ESP].PointsTo[4],
                state.Registers[RegisterName.ESP].PointsTo[8],
                state.InstructionPointer);
            Console.WriteLine(
                "EIP: {5:x8}\tEBP-8: {0}\tEBP-4: {1}\tEBP: {2}\tEBP+4: {3}\tEBP+8: {4}\t",
                state.Registers[RegisterName.EBP].PointsTo[-8],
                state.Registers[RegisterName.EBP].PointsTo[-4],
                state.Registers[RegisterName.EBP].PointsTo[0],
                state.Registers[RegisterName.EBP].PointsTo[4],
                state.Registers[RegisterName.EBP].PointsTo[8],
                state.InstructionPointer);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddImmediateNonPointerDeref()
        {
            // add    [eax], 0x00
            code = new Byte[] {0x83, 0x00, 0x00};
            state.Registers[RegisterName.EAX] = one;
            Assert.IsFalse(one.IsPointer);
            state = X86Emulator.Run(reportItems, state, code);
        }

        [Test]
        public void AddImmediateToEAX()
        {
            // add    eax,0xf
            const byte IMMEDIATE = 0x0f;
            code = new Byte[] {0x83, 0xc0, IMMEDIATE};
            const uint VALUE = 1;
            state.Registers[RegisterName.EAX] = new AbstractValue(VALUE);
            state = X86Emulator.Run(reportItems, state, code);
            Assert.AreEqual(VALUE + IMMEDIATE, state.Registers[RegisterName.EAX].Value);
        }

        [Test]
        public void AddOneToEAX()
        {
            code = new Byte[] {0x05, 0x01, 0x00, 0x00, 0x00};
            state.Registers[RegisterName.EAX] = new AbstractValue(1);
            state = X86Emulator.Run(reportItems, state, code);
            Assert.AreEqual(code.Length, state.InstructionPointer);
            Assert.AreEqual(0x2, state.Registers[RegisterName.EAX].Value);
        }

        [Test]
        [Ignore("this is the essense of a bug in one of the system tests")]
        public void BigComplicatedMuthafuqqa()
        {
            const byte VALUE = 0xcd;
            state = state.DoOperation(RegisterName.ESP, OperatorEffect.Add, new AbstractValue(0x40));
            state.InstructionPointer = 0x804839b;
            state = X86Emulator.Run(reportItems, state, new byte[] {0x55});
            state = X86Emulator.Run(reportItems, state, new byte[] {0x89, 0xe5});
            PrintStackAndBlock();
            state = X86Emulator.Run(reportItems, state, new byte[] {0x83, 0xec, 0x08});
            state = X86Emulator.Run(reportItems, state, new byte[] {0x83, 0xe4, 0xf0});
            state = X86Emulator.Run(reportItems, state, new byte[] {0xb8, 0x00, 0x00, 0x00, 0x00});
            state = X86Emulator.Run(reportItems, state, new byte[] {0x83, 0xc0, 0x0f});
            state = X86Emulator.Run(reportItems, state, new byte[] {0x83, 0xc0, 0x0f});
            state = X86Emulator.Run(reportItems, state, new byte[] {0xc1, 0xe8, 0x04});
            state = X86Emulator.Run(reportItems, state, new byte[] {0xc1, 0xe0, 0x04});
            state = X86Emulator.Run(reportItems, state, new byte[] {0x29, 0xc4});
            state = X86Emulator.Run(reportItems, state, new byte[] {0xc7, 0x04, 0x24, VALUE, 0, 0, 0});
            Assert.AreEqual(VALUE, state.TopOfStack.Value);
            state = X86Emulator.Run(reportItems, state, new byte[] {0xe8, 0x10, 0, 0, 0});
            PrintStackAndBlock();
            state = X86Emulator.Run(reportItems, state, new byte[] {0x55});
            PrintStackAndBlock();
            state = X86Emulator.Run(reportItems, state, new byte[] {0x89, 0xe5});
            PrintStackAndBlock();
            state = X86Emulator.Run(reportItems, state, new byte[] {0x83, 0xec, 0x18});
            PrintStackAndBlock();
            state = X86Emulator.Run(reportItems, state, new byte[] {0x8b, 0x45, 0x08});
            PrintStackAndBlock();
            Assert.AreEqual(VALUE, state.Registers[RegisterName.EAX].Value);
        }

        [Test]
        public void CmpEbpPlusEight0()
        {
            // cmp    DWORD PTR [ebp+8],0x0
            code = new Byte[] {0x83, 0x7d, 0x08, 0x0};
            var buffer = AbstractValue.GetNewBuffer(16);

            var pointer = new AbstractValue(buffer);
            state.Registers[RegisterName.EBP] = pointer;
            state.Registers[RegisterName.EBP].PointsTo[8] = new AbstractValue(1);

            state = X86Emulator.Run(reportItems, state, code);
            Assert.AreEqual(0x4, state.InstructionPointer);
            Assert.IsFalse(state.ZeroFlag);

            state.Registers[RegisterName.EBP].PointsTo[8] = new AbstractValue(0);
            state = X86Emulator.Run(reportItems, state, code);
            Assert.AreEqual(0x8, state.InstructionPointer);
            Assert.IsTrue(state.ZeroFlag);
        }

        [Test]
        public void CmpKnownEax0()
        {
            // cmp eax, 0
            code = new Byte[] {0x83, 0xf8, 0x0};
            state.Registers[RegisterName.EAX] = new AbstractValue(1);
            state = X86Emulator.Run(reportItems, state, code);
            Assert.AreEqual(0x3, state.InstructionPointer);
            Assert.IsFalse(state.ZeroFlag);

            state.Registers[RegisterName.EAX] = new AbstractValue(0);
            state = X86Emulator.Run(reportItems, state, code);
            Assert.AreEqual(0x6, state.InstructionPointer);
            Assert.IsTrue(state.ZeroFlag);
        }

        [Test]
        [Ignore("In progress --Luis")]
        public void CmpUnknownEax0()
        {
            // cmp eax, 0
            ////MachineState[] possibleStates;
            ////
            ////code = new Byte[] {0x83, 0xf8, 0x0};
            ////state.Registers[RegisterName.EAX] = new AbstractValue();
            ////possibleStates = X86Emulator.Run(reportItems, state, code);
            ////Assert.AreEqual(2, possibleStates.Length);
        }

        [Test]
        public void DefaultValues()
        {
            Assert.AreEqual(0x0, state.InstructionPointer);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DereferenceRegisterWithNonPointer()
        {
            // mov    eax,DWORD PTR [eax]
            code = new Byte[] {0x8b, 0x00};
            state.Registers[RegisterName.EAX] = one;
            Assert.IsFalse(one.IsPointer);
            state = X86Emulator.Run(reportItems, state, code);
        }

        [Test]
        public void DoublePushEAXThenPopEAX()
        {
            var pushCode = new Byte[] {0x50};
            var popCode = new Byte[] {0x58};

            var two = new AbstractValue(2);
            state.Registers[RegisterName.EAX] = one;
            state = X86Emulator.Run(reportItems, state, pushCode);
            state.Registers[RegisterName.EAX] = two;
            state = X86Emulator.Run(reportItems, state, pushCode);
            state.Registers[RegisterName.EAX] = null;
            state = X86Emulator.Run(reportItems, state, popCode);
            Assert.AreEqual(two, state.Registers[RegisterName.EAX]);
            state = X86Emulator.Run(reportItems, state, popCode);
            Assert.AreEqual(one, state.Registers[RegisterName.EAX]);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyCodeArray()
        {
            code = new Byte[] {};
            state = X86Emulator.Run(reportItems, state, code);
        }

        [Test]
        public void GetNewBufferReturnsUnallocatedValues()
        {
            var buffer = AbstractValue.GetNewBuffer(16);
            for (var i = 0; i < 16; i++)
            {
                Assert.IsFalse(buffer[i].IsInitialized);
            }
        }

        [Test]
        public void InitialRegisters()
        {
            var registers = new RegisterCollection();
            registers[RegisterName.EAX] = one;
            state = new MachineState(registers);
            var newState = X86Emulator.Run(reportItems, state, new byte[] {0x90});
            Assert.AreEqual(one, newState.Registers[RegisterName.EAX]);
            Assert.AreNotSame(state, newState);
            Assert.AreNotEqual(state, newState);
        }

        [Test]
        [ExpectedException(typeof(InvalidOpcodeException))]
        public void InvalidOpcode()
        {
            // int3 -- not really invalid, but we probably won't see it in any program we care about
            code = new Byte[] {0xcc};
            state = X86Emulator.Run(reportItems, state, code);
        }

        [Test]
        public void JnzPlus6()
        {
            // cmp eax, 0
            var cmpCode = new Byte[] {0x83, 0xf8, 0x0};
            state.Registers[RegisterName.EAX] = new AbstractValue(0);
            state = X86Emulator.Run(reportItems, state, cmpCode);

            const byte OFFSET = 0x06;
            var jnzCode = new Byte[] {0x75, OFFSET};
            var oldEIP = state.InstructionPointer;
            state = X86Emulator.Run(reportItems, state, jnzCode);

            Assert.AreEqual(jnzCode.Length, state.InstructionPointer - oldEIP);

            state.Registers[RegisterName.EAX] = new AbstractValue(1);
            state = X86Emulator.Run(reportItems, state, cmpCode);

            oldEIP = state.InstructionPointer;
            state = X86Emulator.Run(reportItems, state, jnzCode);
            Assert.AreEqual(jnzCode.Length + OFFSET, state.InstructionPointer - oldEIP);
        }

        [Test]
        public void LeaEaxFromEdxPlusEax()
        {
            // lea    eax,[edx+eax]
            code = new Byte[] {0x8d, 0x04, 0x02};

            var zero = new AbstractValue(0);
            var two = new AbstractValue(2);
            var values = new[] {zero, two};
            var buffer = new AbstractBuffer(values);
            state.Registers[RegisterName.EDX] = new AbstractValue(buffer);
            state.Registers[RegisterName.EAX] = new AbstractValue(1);

            state = X86Emulator.Run(reportItems, state, code);
            var eax = state.Registers[RegisterName.EAX];
            Assert.AreEqual(two.Value, eax.PointsTo[0].Value);
        }

        [Test]
        public void LeaEdxFromEaxPlus16()
        {
            // lea    edx,[eax+index]
            const byte INDEX = 0x1;
            code = new Byte[] {0x8d, 0x50, INDEX};

            var zero = new AbstractValue(0);
            var values = new[] {zero, one};
            var buffer = new AbstractBuffer(values);
            state.Registers[RegisterName.EAX] = new AbstractValue(buffer);

            state = X86Emulator.Run(reportItems, state, code);
            var edx = state.Registers[RegisterName.EDX];
            Assert.IsNotNull(edx);
            Assert.AreEqual(one, edx.PointsTo[0]);
        }

        [Test]
        public void MallocCall()
        {
            const uint INITIAL_INSTRUCTION_POINTER = 0x804838f;

            // TODO: need to reconcile with esp/ebp handling
            state = state.PushOntoStack(new AbstractValue(16));
            state.InstructionPointer = INITIAL_INSTRUCTION_POINTER;
            code = new Byte[] {0xe8, 0x14, 0xff, 0xff, 0xff};
            state = X86Emulator.Run(reportItems, state, code);
            Assert.AreEqual(INITIAL_INSTRUCTION_POINTER + code.Length, state.InstructionPointer);
            Assert.AreEqual(16, state.ReturnValue.PointsTo.Length);
        }

        [Test]
        public void MovBytePtrEaxInBounds()
        {
            // mov    BYTE PTR [eax], value
            const byte VALUE = 0x01;
            var buffer = AbstractValue.GetNewBuffer(16);
            var pointer = new AbstractValue(buffer);
            state.ReturnValue = pointer;
            code = new Byte[] {0xc6, 0x00, VALUE};
            state = X86Emulator.Run(reportItems, state, code);
            Assert.AreEqual(VALUE, state.ReturnValue.PointsTo[0].Value);
        }

        [Test]
        public void MovBytePtrEaxPlus16FromBl()
        {
            // mov    BYTE PTR [eax+16],bl
            const byte OFFSET = 0x10;
            state.Registers[RegisterName.EBX] = new AbstractValue(0x1);
            var buffer = AbstractValue.GetNewBuffer((uint)OFFSET + 1);
            var pointer = new AbstractValue(buffer);
            state.ReturnValue = pointer;

            code = new Byte[] {0x88, 0x58, OFFSET};
            state = X86Emulator.Run(reportItems, state, code);

            Assert.AreEqual(0x1, state.ReturnValue.PointsTo[OFFSET].Value);
        }

        [Test]
        public void MovDwordPtrEaxPlus16FromEbx()
        {
            // mov    BYTE PTR [eax+16],bl
            const byte OFFSET = 0x10;
            state.Registers[RegisterName.EBX] = new AbstractValue(0x1);
            var buffer = AbstractValue.GetNewBuffer((uint)OFFSET + 1);
            var pointer = new AbstractValue(buffer);
            state.Registers[RegisterName.EAX] = pointer;

            code = new Byte[] {0x89, 0x58, OFFSET};
            state = X86Emulator.Run(reportItems, state, code);

            Assert.AreEqual(0x1, state.Registers[RegisterName.EAX].PointsTo[OFFSET].Value);
        }

        [Test]
        public void MovEax0()
        {
            code = new Byte[] {0xb8, 0x00, 0x00, 0x00, 0x00};
            state = X86Emulator.Run(reportItems, state, code);
            Assert.AreEqual(code.Length, state.InstructionPointer);
            Assert.AreEqual(0x0, state.Registers[RegisterName.EAX].Value);
        }

        [Test]
        public void MovEaxDword()
        {
            code = new Byte[] {0xb8, 0x37, 0x13, 0x03, 0x00};
            state = X86Emulator.Run(reportItems, state, code);
            Assert.AreEqual(code.Length, state.InstructionPointer);
            Assert.AreEqual(0x00031337, state.ReturnValue.Value);
        }

        [Test]
        public void MovEaxEaxFourByteValue()
        {
            // mov    eax,DWORD PTR [eax]
            const uint VALUE = 0x31337;
            var buffer = new[] {new AbstractValue(VALUE)};
            var pointer = new AbstractValue(buffer);
            state.Registers[RegisterName.EAX] = pointer;

            code = new Byte[] {0x8b, 0x00};
            state = X86Emulator.Run(reportItems, state, code);

            Assert.AreEqual(0x2, state.InstructionPointer);
            Assert.AreEqual(VALUE, state.Registers[RegisterName.EAX].Value);
        }

        [Test]
        public void MovEaxEaxPointerPointerValue()
        {
            // mov    eax,DWORD PTR [eax]
            const uint VALUE = 0x31337;
            var argv = new AbstractValue(VALUE);
            var argvBuffer = new[] {argv};
            var argvPointer = new AbstractValue(argvBuffer);
            state.Registers[RegisterName.EAX] = argvPointer;

            code = new Byte[] {0x8b, 0x00};
            state = X86Emulator.Run(reportItems, state, code);

            Assert.AreEqual(0x2, state.InstructionPointer);
            Assert.AreEqual(VALUE, state.Registers[RegisterName.EAX].Value);
        }

        [Test]
        public void MovEaxEbpPlusTwelve()
        {
            var buffer = AbstractValue.GetNewBuffer(16);
            buffer[12] = new AbstractValue(1);

            var pointer = new AbstractValue(buffer);
            state.Registers[RegisterName.EBP] = pointer;

            code = new Byte[] {0x8b, 0x45, 0xc};
            state = X86Emulator.Run(reportItems, state, code);

            Assert.AreEqual(0x3, state.InstructionPointer);
            Assert.AreEqual(1, state.ReturnValue.Value);
        }

        [Test]
        public void MovEbpEsp()
        {
            // mov    ebp,esp
            code = new Byte[] {0x89, 0xe5};
            var value = new AbstractValue(0x31337);
            state = state.PushOntoStack(value);
            state = X86Emulator.Run(reportItems, state, code);

            Assert.AreEqual(0x2, state.InstructionPointer);
            Assert.AreEqual(value, state.Registers[RegisterName.ESP].PointsTo[0]);
            Assert.AreEqual(value, state.Registers[RegisterName.EBP].PointsTo[0]);
        }

        [Test]
        public void MovEbpMinus8()
        {
            // mov    DWORD PTR [ebp-8],0xf
            const byte FIFTEEN = 0x0f;
            var value = AbstractValue.GetNewBuffer(0x100);
            state.Registers[RegisterName.EBP] = new AbstractValue(value);
            code = new Byte[] {0xc7, 0x45, 0xf8, FIFTEEN, 0x00, 0x00, 0x00};
            state = X86Emulator.Run(reportItems, state, code);

            Assert.AreEqual(FIFTEEN, state.Registers[RegisterName.EBP].PointsTo[0xf8].Value);
        }

        [Test]
        public void MovEspDword()
        {
            code = new Byte[] {0xc7, 0x04, 0x24, 0x37, 0x13, 0x03, 0x00};
            state = X86Emulator.Run(reportItems, state, code);
            Assert.AreEqual(0x7, state.InstructionPointer);
            Assert.AreEqual(0x00031337, state.TopOfStack.Value);
        }

        [Test]
        public void MovFromGlobalIntoEax()
        {
            code = new Byte[] {0xa1, 0xe4, 0x84, 0x04, 0x08};
            var value = new AbstractValue(31337);
            state.DataSegment[0x080484e4] = value;
            state = X86Emulator.Run(reportItems, state, code);

            Assert.AreEqual(value, state.Registers[RegisterName.EAX]);
        }

        [Test]
        public void MovIntoAssignedEaxInBounds()
        {
            const Byte VALUE = 0x01;
            const byte INDEX = 0xf;
            var buffer = AbstractValue.GetNewBuffer(16);
            var pointer = new AbstractValue(buffer);
            state.ReturnValue = pointer;
            code = new Byte[] {0xc6, 0x40, INDEX, VALUE};
            state = X86Emulator.Run(reportItems, state, code);
            Assert.AreEqual(VALUE, state.ReturnValue.PointsTo[INDEX].Value);
        }

        [Test]
        public void MovIntoAssignedEaxOutOfBounds()
        {
            const byte VALUE = 0x01;
            const byte INDEX = 0x10;
            var buffer = AbstractValue.GetNewBuffer(INDEX);
            var pointer = new AbstractValue(buffer);
            state.ReturnValue = pointer;
            var nopCode = new Byte[] {0x90};
            code = new Byte[] {0xc6, 0x40, INDEX, VALUE};
            state = X86Emulator.Run(reportItems, state, nopCode);
            state = X86Emulator.Run(reportItems, state, code);
            Assert.AreEqual(1, reportItems.Count);
            Assert.AreEqual(nopCode.Length, reportItems[0].InstructionPointer);
            Assert.IsFalse(reportItems[0].IsTainted);
        }

        [Test]
        public void MovIntoEdxFromOffset()
        {
            // movzx  edx,BYTE PTR ds:0x80495e0
            const uint VALUE = 1;
            var abstractValue = new AbstractValue(VALUE);
            state.DataSegment[0x80495e0] = abstractValue;
            code = new Byte[] {0x0f, 0xb6, 0x15, 0xe0, 0x95, 0x04, 0x08};
            state = X86Emulator.Run(reportItems, state, code);
            Assert.AreEqual(VALUE, state.Registers[RegisterName.EDX].Value);
        }

        [Test]
        public void MovIntoOffsetFromAl()
        {
            // mov    ds:0x80495e0,al
            const uint VALUE = 1;
            const uint OFFSET = 0x80495e0;
            code = new Byte[] {0xa2, 0xe0, 0x95, 0x04, 0x08};
            state.Registers[RegisterName.EAX] = new AbstractValue(VALUE);
            state = X86Emulator.Run(reportItems, state, code);
            Assert.AreEqual(VALUE, state.DataSegment[OFFSET].Value);
        }

        [Test]
        public void MovPtrEax16()
        {
            // mov    DWORD PTR [eax],0x10
            var value = AbstractValue.GetNewBuffer(1);
            state.Registers[RegisterName.EAX] = new AbstractValue(value);
            code = new Byte[] {0xc7, 0x00, 0x10, 0x00, 0x00, 0x00};
            state = X86Emulator.Run(reportItems, state, code);

            var sixteen = state.Registers[RegisterName.EAX].PointsTo[0];
            Assert.AreEqual(0x10, sixteen.Value);
        }

        [Test]
        public void MovPtrEsp16()
        {
            const byte SIXTEEN = 0x10;
            code = new Byte[] {0xc7, 0x04, 0x24, SIXTEEN, 0x00, 0x00, 0x00};
            var newState = X86Emulator.Run(reportItems, state, code);
            Assert.AreNotSame(newState, state);
            Assert.AreNotEqual(newState, state);
            Assert.AreEqual(0x7, newState.InstructionPointer);
            Assert.AreEqual(SIXTEEN, newState.TopOfStack.Value);
        }

        [Test]
        public void MovPtrEspEax()
        {
            // mov [esp]. eax
            code = new Byte[] {0x89, 0x04, 0x24};
            state.Registers[RegisterName.EAX] = new AbstractValue(0x10);
            var values = new[] {new AbstractValue(1)};
            var buffer = new AbstractBuffer(values);
            state.Registers[RegisterName.ESP] = new AbstractValue(buffer);

            state = X86Emulator.Run(reportItems, state, code);

            var espBuffer = state.Registers[RegisterName.ESP].PointsTo;
            Assert.AreEqual(0x10, espBuffer[0].Value);
        }

        [Test]
        public void NonMallocCall()
        {
            const uint INITIAL_INSTRUCTION_POINTER = 0x80483b8;
            state.InstructionPointer = INITIAL_INSTRUCTION_POINTER;
            code = new Byte[] {0xe8, 0xbf, 0xff, 0xff, 0xff};
            state = X86Emulator.Run(reportItems, state, code);

            Assert.AreEqual(0x804837c, state.InstructionPointer);
            Assert.AreEqual(INITIAL_INSTRUCTION_POINTER + (UInt32)code.Length, state.TopOfStack.Value);
        }

        [Test]
        public void PushEAXThenPopEAX()
        {
            var pushCode = new Byte[] {0x50};
            var popCode = new Byte[] {0x58};

            state.Registers[RegisterName.EAX] = one;
            state = X86Emulator.Run(reportItems, state, pushCode);
            Assert.AreEqual(0x1, state.InstructionPointer);
            state.Registers[RegisterName.EAX] = null;
            state = X86Emulator.Run(reportItems, state, popCode);
            Assert.AreEqual(0x2, state.InstructionPointer);
            Assert.AreEqual(one, state.Registers[RegisterName.EAX]);
        }

        [Test]
        public void PushESIThenPopESI()
        {
            var pushCode = new Byte[] {0x56};
            var popCode = new Byte[] {0x5e};

            state.Registers[RegisterName.ESI] = one;
            state = X86Emulator.Run(reportItems, state, pushCode);
            Assert.AreEqual(0x1, state.InstructionPointer);
            state.Registers[RegisterName.ESI] = null;
            state = X86Emulator.Run(reportItems, state, popCode);
            Assert.AreEqual(0x2, state.InstructionPointer);
            Assert.AreEqual(one, state.Registers[RegisterName.ESI]);
        }

        [Test]
        public void PushESPThenPopESP()
        {
            var pushCode = new Byte[] {0x54};
            var popCode = new Byte[] {0x5c};
            state = X86Emulator.Run(reportItems, state, pushCode);

            Assert.AreEqual(0x1, state.InstructionPointer);
            state = X86Emulator.Run(reportItems, state, popCode);
            Assert.AreEqual(0x2, state.InstructionPointer);
            Assert.AreEqual(state.Registers[RegisterName.ESP], state.Registers[RegisterName.ESP]);
        }

        [Test]
        public void PushEbpThenPopEbp()
        {
            var pushCode = new Byte[] {0x55};
            var popCode = new Byte[] {0x5d};

            state.Registers[RegisterName.EBP] = one;
            state = X86Emulator.Run(reportItems, state, pushCode);
            Assert.AreEqual(0x1, state.InstructionPointer);
            state.Registers[RegisterName.EBP] = null;
            state = X86Emulator.Run(reportItems, state, popCode);
            Assert.AreEqual(0x2, state.InstructionPointer);
            Assert.AreEqual(one, state.Registers[RegisterName.EBP]);
        }

        [Test]
        public void PushEbx()
        {
            code = new Byte[] {0x53};
            state = X86Emulator.Run(reportItems, state, code);

            Assert.AreEqual(0x1, state.InstructionPointer);
            Assert.AreEqual(state.TopOfStack, state.Registers[RegisterName.EBX]);
        }

        [Test]
        public void PushEbxThenPopEbx()
        {
            var pushCode = new Byte[] {0x53};
            var popCode = new Byte[] {0x5b};

            state.Registers[RegisterName.EBX] = one;
            state = X86Emulator.Run(reportItems, state, pushCode);
            Assert.AreEqual(0x1, state.InstructionPointer);
            state.Registers[RegisterName.EBX] = null;
            state = X86Emulator.Run(reportItems, state, popCode);
            Assert.AreEqual(0x2, state.InstructionPointer);
            Assert.AreEqual(one, state.Registers[RegisterName.EBX]);
        }

        [Test]
        public void PushEcxThenPopEcx()
        {
            var pushCode = new Byte[] {0x51};
            var popCode = new Byte[] {0x59};

            state.Registers[RegisterName.ECX] = one;
            state = X86Emulator.Run(reportItems, state, pushCode);
            Assert.AreEqual(0x1, state.InstructionPointer);
            state.Registers[RegisterName.ECX] = null;
            state = X86Emulator.Run(reportItems, state, popCode);
            Assert.AreEqual(0x2, state.InstructionPointer);
            Assert.AreEqual(one, state.Registers[RegisterName.ECX]);
        }

        [Test]
        public void PushEdxThenPopEdx()
        {
            var pushCode = new Byte[] {0x52};
            var popCode = new Byte[] {0x5a};

            state.Registers[RegisterName.EDX] = one;
            state = X86Emulator.Run(reportItems, state, pushCode);
            Assert.AreEqual(0x1, state.InstructionPointer);
            state.Registers[RegisterName.EDX] = null;
            state = X86Emulator.Run(reportItems, state, popCode);
            Assert.AreEqual(0x2, state.InstructionPointer);
            Assert.AreEqual(one, state.Registers[RegisterName.EDX]);
        }

        [Test]
        public void PushIzThenPopEbp()
        {
            var pushCode = new Byte[] {0x68, 0x10, 0x84, 0x04, 0x08};
            var popCode = new Byte[] {0x5d};

            state = X86Emulator.Run(reportItems, state, pushCode);
            Assert.AreEqual(0x5, state.InstructionPointer);
            state.Registers[RegisterName.EBP] = null;
            state = X86Emulator.Run(reportItems, state, popCode);
            Assert.AreEqual(0x6, state.InstructionPointer);
            Assert.AreEqual(0x08048410, state.Registers[RegisterName.EBP].Value);
        }

        [Test]
        public void ReturnValueIsAliasedToEax()
        {
            var value = new AbstractValue(0x31337);
            state.ReturnValue = value;
            Assert.AreEqual(value, state.Registers[RegisterName.EAX]);
        }

        [Test]
        public void Sub()
        {
            code = new Byte[] {0x83, 0xec, 0x08};
            state.Registers[RegisterName.ESP] = new AbstractValue(0x0a);
            state = X86Emulator.Run(reportItems, state, code);
            Assert.AreEqual(0x3, state.InstructionPointer);
            Assert.AreEqual(0x02, state.Registers[RegisterName.ESP].Value);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SubNonPointerDeref()
        {
            // sub eax, [eax]
            code = new Byte[] {0x29, 0x00};
            state.Registers[RegisterName.EAX] = one;
            Assert.IsFalse(one.IsPointer);
            state = X86Emulator.Run(reportItems, state, code);
        }
    }
}