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
    public class X86OpcodeTest
    {
        private readonly Opcode opcode = new X86Opcode();
        private Byte[] code;
        private OpcodeEncoding encoding;

        [Test]
        public void AnotherGvM()
        {
            code = new Byte[] {0x8d, 0x04, 0x02};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            Assert.IsTrue(opcode.HasModRM(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.GvM, encoding);

            OperatorEffect operatorEffect = opcode.GetOperatorEffect(code);
            Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);
            Assert.AreEqual(RegisterName.EAX, opcode.GetDestinationRegister(code));
        }

        [Test]
        public void EbGb()
        {
            // mov    BYTE PTR [eax+16],bl
            code = new Byte[] {0x88, 0x58, 0x10};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.EbGb, encoding);

            OperatorEffect operatorEffect = opcode.GetOperatorEffect(code);
            Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);
            Assert.AreEqual(RegisterName.EBX, opcode.GetSourceRegister(code));
        }

        [Test]
        public void EbIb()
        {
            Byte immediate = 0;
            code = new Byte[] {0xc6, 0x40, 0x10, immediate};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.EbIb, encoding);
            Assert.AreEqual(immediate, opcode.GetImmediate(code));
        }

        [Test]
        public void EvGv()
        {
            code = new Byte[] {0x89, 0x00};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.EvGv, encoding);
        }

        [Test]
        public void EvGvSub()
        {
            code = new Byte[] {0x29, 0xc4};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.EvGv, encoding);

            OperatorEffect operatorEffect = opcode.GetOperatorEffect(code);
            Assert.AreEqual(OperatorEffect.Sub, operatorEffect);
        }

        [Test]
        public void EvIbAdd()
        {
            // 83 c0 0f                add    eax,0xf
            code = new Byte[] {0x83, 0xc0, 0x0f};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.EvIb, encoding);

            OperatorEffect operatorEffect = opcode.GetOperatorEffect(code);
            Assert.AreEqual(OperatorEffect.Add, operatorEffect);
        }

        [Test]
        public void EvIbAnd()
        {
            code = new Byte[] {0x83, 0xe4, 0xf0};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.EvIb, encoding);

            OperatorEffect operatorEffect = opcode.GetOperatorEffect(code);
            Assert.AreEqual(OperatorEffect.And, operatorEffect);
        }

        [Test]
        public void EvIbCmp()
        {
            code = new Byte[] {0x83, 0x7d, 0x08, 0x01};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.EvIb, encoding);

            OperatorEffect operatorEffect = opcode.GetOperatorEffect(code);
            Assert.AreEqual(OperatorEffect.Cmp, operatorEffect);
        }

        [Test]
        public void EvIbShl()
        {
            code = new Byte[] {0xc1, 0xe0, 0x04};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.EvIb, encoding);

            OperatorEffect operatorEffect = opcode.GetOperatorEffect(code);
            Assert.AreEqual(OperatorEffect.Shl, operatorEffect);
        }

        [Test]
        public void EvIbShr()
        {
            code = new Byte[] {0xc1, 0xe8, 0x04};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.EvIb, encoding);

            OperatorEffect operatorEffect = opcode.GetOperatorEffect(code);
            Assert.AreEqual(OperatorEffect.Shr, operatorEffect);
        }

        [Test]
        public void EvIbSub()
        {
            code = new Byte[] {0x83, 0xec, 0x08};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.EvIb, encoding);

            OperatorEffect operatorEffect = opcode.GetOperatorEffect(code);
            Assert.AreEqual(OperatorEffect.Sub, operatorEffect);
        }

        [Test]
        public void EvIz()
        {
            code = new Byte[] {0xc7, 0x04, 0x24, 0x10, 0x00, 0x00, 0x00};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.EvIz, encoding);
            Assert.IsTrue(opcode.HasDestinationRegister(code));
            Assert.AreEqual(RegisterName.ESP, opcode.GetDestinationRegister(code));
            Assert.AreEqual(0x10, opcode.GetImmediate(code));
        }

        [Test]
        public void GvEbOffsetToRegister()
        {
            // movzx  edx,BYTE PTR ds:0x80495e0
            code = new Byte[] {0x0f, 0xb6, 0x15, 0xe0, 0x95, 0x04, 0x08};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.GvEb, encoding);

            OperatorEffect operatorEffect = opcode.GetOperatorEffect(code);
            Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);
        }

        [Test]
        public void GvEbRegisterToRegister()
        {
            // movzx  ebx,BYTE PTR [eax]
            code = new Byte[] {0x0f, 0xb6, 0x18};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.GvEb, encoding);

            OperatorEffect operatorEffect = opcode.GetOperatorEffect(code);
            Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);
            Assert.AreEqual(RegisterName.EAX, opcode.GetSourceRegister(code));
        }

        [Test]
        public void GvEv()
        {
            // mov eax, [eax]
            code = new Byte[] {0x8b, 0x00};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.GvEv, encoding);
            Assert.AreEqual(RegisterName.EAX, opcode.GetSourceRegister(code));
            Assert.AreEqual(RegisterName.EAX, opcode.GetDestinationRegister(code));
        }

        [Test]
        public void GvEvDword()
        {
            // mov eax, [0x11223344]
            code = new Byte[] {0x8b, 0x05, 0x44, 0x33, 0x22, 0x11};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.GvEv, encoding);
            Assert.AreEqual(RegisterName.None, opcode.GetSourceRegister(code));
            Assert.AreEqual(RegisterName.EAX, opcode.GetDestinationRegister(code));
        }

        [Test]
        public void GvEvSIB()
        {
            // mov eax, [base_register+scale_register*scaler
            code = new Byte[] {0x8b, 0x04, 0x24};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.GvEv, encoding);
            Assert.AreEqual(RegisterName.None, opcode.GetSourceRegister(code));
            Assert.AreEqual(RegisterName.EAX, opcode.GetDestinationRegister(code));
        }

        [Test]
        public void GvM()
        {
            // lea edx, [eax+16]
            code = new Byte[] {0x8d, 0x50, 0x10};
            Assert.IsTrue(opcode.HasModRM(code));
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.GvM, encoding);
            Assert.AreEqual(RegisterName.EAX, opcode.GetSourceRegister(code));

            OperatorEffect operatorEffect = opcode.GetOperatorEffect(code);
            Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);
        }

        [Test]
        public void Halt()
        {
            code = new Byte[] {0xf4};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.None, encoding);
            Assert.AreEqual(StackEffect.None, opcode.GetStackEffect(code));
            Assert.IsTrue(opcode.TerminatesFunction(code));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidGetImmediate()
        {
            code = new Byte[] {0x90};
            Assert.IsFalse(opcode.HasImmediate(code));
            opcode.GetImmediate(code);
        }

        [Test]
        public void Jb()
        {
            code = new Byte[] {0x75, 0x06};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.Jb, encoding);

            OperatorEffect operatorEffect = opcode.GetOperatorEffect(code);
            Assert.AreEqual(OperatorEffect.Jnz, operatorEffect);
        }

        [Test]
        public void Jz()
        {
            code = new Byte[] {0xe8, 0x14, 0xff, 0xff, 0xff};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.Jz, encoding);

            Assert.AreEqual(OperatorEffect.Call, opcode.GetOperatorEffect(code));
            Assert.IsTrue(opcode.HasImmediate(code));
            Assert.AreEqual(0xffffff14, opcode.GetImmediate(code));
        }

        [Test]
        public void LeaveReturn()
        {
            code = new Byte[] {0xc9};
            Assert.AreEqual(OperatorEffect.Leave, opcode.GetOperatorEffect(code));
            Assert.IsFalse(opcode.TerminatesFunction(code));

            code = new Byte[] {0xc3};
            Assert.AreEqual(OperatorEffect.Return, opcode.GetOperatorEffect(code));
            Assert.IsTrue(opcode.TerminatesFunction(code));
        }

        [Test]
        public void None()
        {
            code = new Byte[] {0x90};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.None, encoding);
            Assert.AreEqual(StackEffect.None, opcode.GetStackEffect(code));
        }

        [Test]
        public void ObAL()
        {
            // mov    ds:0x80495e0,al
            code = new Byte[] {0xa2, 0xe0, 0x95, 0x04, 0x08};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.ObAL, encoding);

            OperatorEffect operatorEffect = opcode.GetOperatorEffect(code);
            Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);
        }

        [Test]
        public void PoprAX()
        {
            code = new Byte[] {0x58};
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.rAX, encoding);
            Assert.AreEqual(StackEffect.Pop, opcode.GetStackEffect(code));
            Assert.IsFalse(opcode.HasSourceRegister(code));
            Assert.IsTrue(opcode.HasDestinationRegister(code));
            Assert.AreEqual(RegisterName.EAX, opcode.GetDestinationRegister(code));
        }

        [Test]
        public void PoprBP()
        {
            code = new Byte[] {0x5d};
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.rBP, encoding);
            Assert.AreEqual(StackEffect.Pop, opcode.GetStackEffect(code));
            Assert.AreEqual(RegisterName.None, opcode.GetSourceRegister(code));
            Assert.AreEqual(RegisterName.EBP, opcode.GetDestinationRegister(code));
        }

        [Test]
        public void PoprBX()
        {
            code = new Byte[] {0x5b};
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.rBX, encoding);
            Assert.AreEqual(StackEffect.Pop, opcode.GetStackEffect(code));
            Assert.AreEqual(RegisterName.EBX, opcode.GetDestinationRegister(code));
            Assert.AreEqual(RegisterName.None, opcode.GetSourceRegister(code));
        }

        [Test]
        public void PoprCX()
        {
            code = new Byte[] {0x59};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.rCX, encoding);
            Assert.AreEqual(StackEffect.Pop, opcode.GetStackEffect(code));
            Assert.IsFalse(opcode.HasSourceRegister(code));
            Assert.IsTrue(opcode.HasDestinationRegister(code));
            Assert.AreEqual(RegisterName.ECX, opcode.GetDestinationRegister(code));
        }

        [Test]
        public void PoprDX()
        {
            code = new Byte[] {0x5a};
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.rDX, encoding);
            Assert.AreEqual(StackEffect.Pop, opcode.GetStackEffect(code));
            Assert.IsFalse(opcode.HasSourceRegister(code));
            Assert.IsTrue(opcode.HasDestinationRegister(code));
            Assert.AreEqual(RegisterName.EDX, opcode.GetDestinationRegister(code));
        }

        [Test]
        public void PoprSI()
        {
            code = new Byte[] {0x5e};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.rSI, encoding);
            Assert.AreEqual(StackEffect.Pop, opcode.GetStackEffect(code));
            Assert.IsFalse(opcode.HasSourceRegister(code));
            Assert.IsTrue(opcode.HasDestinationRegister(code));
            Assert.AreEqual(RegisterName.ESI, opcode.GetDestinationRegister(code));
        }

        [Test]
        public void PoprSP()
        {
            code = new Byte[] {0x5c};
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.rSP, encoding);
            Assert.AreEqual(StackEffect.Pop, opcode.GetStackEffect(code));
            Assert.IsFalse(opcode.HasSourceRegister(code));
            Assert.IsTrue(opcode.HasDestinationRegister(code));
            Assert.AreEqual(RegisterName.None, opcode.GetSourceRegister(code));
            Assert.AreEqual(RegisterName.ESP, opcode.GetDestinationRegister(code));
        }

        [Test]
        public void PushIz()
        {
            code = new Byte[] {0x68, 0x10, 0x84, 0x04, 0x08};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.Iz, encoding);
            Assert.AreEqual(StackEffect.Push, opcode.GetStackEffect(code));
            Assert.IsFalse(opcode.HasSourceRegister(code));
            Assert.IsFalse(opcode.HasDestinationRegister(code));
            Assert.IsTrue(opcode.HasImmediate(code));
            Assert.AreEqual(0x08048410, opcode.GetImmediate(code));
        }

        [Test]
        public void PushrAX()
        {
            code = new Byte[] {0x50};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.rAX, encoding);
            Assert.AreEqual(StackEffect.Push, opcode.GetStackEffect(code));
            Assert.IsTrue(opcode.HasSourceRegister(code));
            Assert.IsFalse(opcode.HasDestinationRegister(code));
            Assert.AreEqual(RegisterName.EAX, opcode.GetSourceRegister(code));
        }

        [Test]
        public void PushrBP()
        {
            code = new Byte[] {0x55};
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.rBP, encoding);
            Assert.AreEqual(StackEffect.Push, opcode.GetStackEffect(code));
            Assert.AreEqual(RegisterName.None, opcode.GetDestinationRegister(code));
            Assert.AreEqual(RegisterName.EBP, opcode.GetSourceRegister(code));
        }

        [Test]
        public void PushrBX()
        {
            code = new Byte[] {0x53};
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.rBX, encoding);
            Assert.AreEqual(StackEffect.Push, opcode.GetStackEffect(code));
            Assert.AreEqual(RegisterName.None, opcode.GetDestinationRegister(code));
            Assert.AreEqual(RegisterName.EBX, opcode.GetSourceRegister(code));
        }

        [Test]
        public void PushrCX()
        {
            code = new Byte[] {0x51};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.rCX, encoding);
            Assert.AreEqual(StackEffect.Push, opcode.GetStackEffect(code));
            Assert.IsTrue(opcode.HasSourceRegister(code));
            Assert.IsFalse(opcode.HasDestinationRegister(code));
            Assert.AreEqual(RegisterName.ECX, opcode.GetSourceRegister(code));
        }

        [Test]
        public void PushrDX()
        {
            code = new Byte[] {0x52};
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.rDX, encoding);
            Assert.AreEqual(StackEffect.Push, opcode.GetStackEffect(code));
            Assert.IsTrue(opcode.HasSourceRegister(code));
            Assert.IsFalse(opcode.HasDestinationRegister(code));
            Assert.AreEqual(RegisterName.EDX, opcode.GetSourceRegister(code));
        }

        [Test]
        public void PushrSI()
        {
            code = new Byte[] {0x56};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.rSI, encoding);
            Assert.AreEqual(StackEffect.Push, opcode.GetStackEffect(code));
            Assert.IsTrue(opcode.HasSourceRegister(code));
            Assert.IsFalse(opcode.HasDestinationRegister(code));
            Assert.AreEqual(RegisterName.ESI, opcode.GetSourceRegister(code));
        }

        [Test]
        public void PushrSP()
        {
            code = new Byte[] {0x54};
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.rSP, encoding);
            Assert.AreEqual(StackEffect.Push, opcode.GetStackEffect(code));
            Assert.IsTrue(opcode.HasSourceRegister(code));
            Assert.IsFalse(opcode.HasDestinationRegister(code));
            Assert.AreEqual(RegisterName.ESP, opcode.GetSourceRegister(code));
            Assert.AreEqual(RegisterName.None, opcode.GetDestinationRegister(code));
        }

        [Test]
        public void RAXIv()
        {
            code = new Byte[] {0xb8, 0x00, 0x00, 0x00, 0x00};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.rAxIv, encoding);
            Assert.AreEqual(RegisterName.EAX, opcode.GetDestinationRegister(code));
        }

        [Test]
        public void rAxIz()
        {
            code = new Byte[] {0x05, 0x04, 0x01, 0x00, 0x00};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.rAxIz, encoding);

            OperatorEffect operatorEffect = opcode.GetOperatorEffect(code);
            Assert.AreEqual(OperatorEffect.Add, operatorEffect);
        }

        [Test]
        public void rAxOv()
        {
            code = new Byte[] {0xa1, 0xe4, 0x84, 0x04, 0x08};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.rAxOv, encoding);

            OperatorEffect operatorEffect = opcode.GetOperatorEffect(code);
            Assert.AreEqual(OperatorEffect.Assignment, operatorEffect);
        }

        [Test]
        [ExpectedException(typeof(InvalidOpcodeException))]
        public void UnknownOpcode()
        {
            code = new Byte[] {0xf0, 0x00};
            opcode.GetEncoding(code);
        }

        [Test]
        public void XorEvGv()
        {
            // xor EBP,EBP
            code = new Byte[] {0x31, 0xed};
            Assert.AreEqual(code.Length, opcode.GetInstructionLength(code));
            encoding = opcode.GetEncoding(code);
            Assert.AreEqual(OpcodeEncoding.EvGv, encoding);
            Assert.AreEqual(OperatorEffect.Xor, opcode.GetOperatorEffect(code));
            Assert.AreEqual(RegisterName.EBP, opcode.GetDestinationRegister(code));
            Assert.AreEqual(RegisterName.EBP, opcode.GetSourceRegister(code));
        }
    }
}