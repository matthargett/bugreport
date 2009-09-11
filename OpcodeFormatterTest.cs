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
    public class OpcodeFormatterTest
    {
        private Byte[] code;

        [Test]
        public void JzImmediate()
        {
            code = new Byte[] {0xe8, 0xb9, 0xff, 0xff, 0xff};
            Assert.AreEqual("call", OpcodeFormatter.GetInstructionName(code));
            Assert.AreEqual("0x0804837c", OpcodeFormatter.GetOperands(code, 0x080483be));
            Assert.AreEqual("(Jz)", OpcodeFormatter.GetEncodingFor(code));
        }

        [Test]
        public void LeaEdxEaxPlus16()
        {
            code = new Byte[] {0x8d, 0x50, 0x10};
            Assert.AreEqual("lea", OpcodeFormatter.GetInstructionName(code));
            Assert.AreEqual("edx, [eax+16]", OpcodeFormatter.GetOperands(code, 0));
        }

        [Test]
        public void MovEaxZero()
        {
            code = new Byte[] {0xb8, 0x00, 0x00, 0x00, 0x00};
            Assert.AreEqual("mov", OpcodeFormatter.GetInstructionName(code));
            Assert.AreEqual("eax, 0x0", OpcodeFormatter.GetOperands(code, 0));
            Assert.AreEqual("(rAxIv)", OpcodeFormatter.GetEncodingFor(code));
        }

        [Test]
        public void MovEbpEsp()
        {
            code = new Byte[] {0x89, 0xe5};
            Assert.AreEqual("mov", OpcodeFormatter.GetInstructionName(code));
            Assert.AreEqual("ebp, esp", OpcodeFormatter.GetOperands(code, 0));
            Assert.AreEqual("(EvGv)", OpcodeFormatter.GetEncodingFor(code));
        }

        [Test]
        public void MovPtrEaxPlusSixteenZero()
        {
            code = new Byte[] {0xc6, 0x40, 0x10, 0x00};
            Assert.AreEqual("mov", OpcodeFormatter.GetInstructionName(code));
            Assert.AreEqual("[eax+16], 0x0", OpcodeFormatter.GetOperands(code, 0));
            Assert.AreEqual("(EbIb)", OpcodeFormatter.GetEncodingFor(code));
        }

        [Test]
        public void MovPtrEsp10()
        {
            code = new Byte[] {0xc7, 0x04, 0x24, 0x10, 0x00, 0x00, 0x00};
            Assert.AreEqual("mov", OpcodeFormatter.GetInstructionName(code));
            Assert.AreEqual("[esp], 0x10", OpcodeFormatter.GetOperands(code, 0));
            Assert.AreEqual("(EvIz)", OpcodeFormatter.GetEncodingFor(code));
        }

        [Test]
        public void MovZxEaxBytePtr()
        {
            code = new Byte[] {0x0f, 0xb6, 0x05, 0x00, 0x96, 0x04, 0x08};

            // TODO: this should actually be movzx
            Assert.AreEqual("mov", OpcodeFormatter.GetInstructionName(code));
            Assert.AreEqual("eax, [0x8049600]", OpcodeFormatter.GetOperands(code, 0));
        }

        [Test]
        public void PopEbp()
        {
            code = new Byte[] {0x5d};
            Assert.AreEqual("pop", OpcodeFormatter.GetInstructionName(code));
            Assert.AreEqual("ebp", OpcodeFormatter.GetOperands(code, 0));
            Assert.AreEqual("(rBP)", OpcodeFormatter.GetEncodingFor(code));
        }

        [Test]
        public void PushEbp()
        {
            code = new Byte[] {0x55};
            Assert.AreEqual("push", OpcodeFormatter.GetInstructionName(code));
            Assert.AreEqual("ebp", OpcodeFormatter.GetOperands(code, 0));
            Assert.AreEqual("(rBP)", OpcodeFormatter.GetEncodingFor(code));
        }
    }
}