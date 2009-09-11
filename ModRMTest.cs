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
    public class ModRMTest
    {
        private Byte[] code;

        [Test]
        public void EaxEaxNoIndex()
        {
            // mov    eax,DWORD PTR [eax]
            code = new Byte[] {0x8b, 0x00};
            Assert.AreEqual(RegisterName.EAX, ModRM.GetEvFor(code));
            Assert.AreEqual(RegisterName.EAX, ModRM.GetGvFor(code));
            Assert.IsFalse(ModRM.HasIndex(code));
            Assert.IsTrue(ModRM.IsEffectiveAddressDereferenced(code));
        }

        [Test]
        [ExpectedException(typeof (NotImplementedException))]
        public void EaxEbpPlusDword12()
        {
            const int INDEX = 0x0c;
            code = new Byte[] {0x8b, 0x85, INDEX, 0x00, 0x00, 0x00};
            Assert.IsTrue(ModRM.HasIndex(code));
            Assert.AreEqual(INDEX, ModRM.GetIndexFor(code));
        }

        [Test]
        public void EaxEbpPlusSByte12()
        {
            // mov    eax,DWORD PTR [ebp+index]
            const int INDEX = 0x0c;
            code = new Byte[] {0x8b, 0x45, INDEX};
            Assert.AreEqual(RegisterName.EBP, ModRM.GetEvFor(code));
            Assert.AreEqual(RegisterName.EAX, ModRM.GetGvFor(code));
            Assert.AreEqual(INDEX, ModRM.GetIndexFor(code));
        }

        [Test]
        public void EaxEbx4()
        {
            // mov    eax,DWORD PTR [ebx-4]
            code = new Byte[] {0x8b, 0x43, 0xfc};
            Assert.AreEqual(RegisterName.EBX, ModRM.GetEvFor(code));
            Assert.AreEqual(RegisterName.EAX, ModRM.GetGvFor(code));
            Assert.AreEqual(0xfc, ModRM.GetIndexFor(code));
        }

        [Test]
        public void EbxEbp12()
        {
            // mov    ebx,DWORD PTR [ebp-12]
            code = new Byte[] {0x8b, 0x5d, 0xf4};
            Assert.AreEqual(RegisterName.EBP, ModRM.GetEvFor(code));
            Assert.AreEqual(RegisterName.EBX, ModRM.GetGvFor(code));
            Assert.IsFalse(ModRM.HasSIB(code));
            Assert.AreEqual(0xf4, ModRM.GetIndexFor(code));
        }

        [Test]
        public void EbxEsp0()
        {
            // mov    ebx,DWORD PTR [esp]
            code = new Byte[] {0x8b, 0x1c, 0x24};
            //// Assert.AreEqual(RegisterName.ESP, ModRM.GetEvFor(code));
            Assert.IsTrue(ModRM.HasSIB(code));
            Assert.AreEqual(RegisterName.EBX, ModRM.GetGvFor(code));
            Assert.IsFalse(ModRM.HasIndex(code));
        }

        [Test]
        public void EdiEbp4()
        {
            // mov    edi,DWORD PTR [ebp-4]
            code = new Byte[] {0x8b, 0x7d, 0xfc};
            Assert.AreEqual(RegisterName.EBP, ModRM.GetEvFor(code));
            Assert.AreEqual(RegisterName.EDI, ModRM.GetGvFor(code));
            Assert.AreEqual(0xfc, ModRM.GetIndexFor(code));
        }

        [Test]
        public void EsiEbp8()
        {
            // mov    esi,DWORD PTR [ebp-8]
            code = new Byte[] {0x8b, 0x75, 0xf8};
            Assert.AreEqual(RegisterName.EBP, ModRM.GetEvFor(code));
            Assert.AreEqual(RegisterName.ESI, ModRM.GetGvFor(code));
            Assert.AreEqual(0xf8, ModRM.GetIndexFor(code));
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void GetEvWithSIB()
        {
            code = new Byte[] {0xc7, 0x04, 0x24, 0x10, 0x00, 0x00, 0x00};
            Assert.IsTrue(ModRM.HasSIB(code));
            ModRM.GetEvFor(code);
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void GetIndexWithNoIndex()
        {
            code = new Byte[] {0x89, 0xe5};
            Assert.IsFalse(ModRM.HasIndex(code));
            ModRM.GetIndexFor(code);
        }

        [Test]
        public void GvEbOffsetToRegister()
        {
            // movzx  edx,BYTE PTR ds:0x80495e0
            code = new Byte[] {0x0f, 0xb6, 0x15, 0xe0, 0x95, 0x04, 0x08};
            Assert.AreEqual(RegisterName.EDX, ModRM.GetGvFor(code));
            Assert.IsFalse(ModRM.HasIndex(code));
            Assert.IsTrue(ModRM.IsEffectiveAddressDereferenced(code));
            Assert.IsTrue(ModRM.HasOffset(code));
        }

        [Test]
        public void GvMWithSIB()
        {
            code = new Byte[] {0x8d, 0x04, 0x02};
            Assert.IsTrue(ModRM.HasSIB(code));
            Assert.AreEqual(RegisterName.EAX, ModRM.GetGvFor(code));
        }

        [Test]
        public void IsEvDwordFalse()
        {
            code = new Byte[] {0x31, 0x07};
            Assert.IsFalse(ModRM.IsEvDword(code));
        }

        [Test]
        public void IsEvDwordTrue()
        {
            code = new Byte[] {0x31, 0x05};
            Assert.IsTrue(ModRM.IsEvDword(code));
        }

        [Test]
        public void MovEbpEsp()
        {
            // mov    ebp,esp
            code = new Byte[] {0x89, 0xe5};
            Assert.AreEqual(RegisterName.EBP, ModRM.GetEvFor(code));
            Assert.AreEqual(RegisterName.ESP, ModRM.GetGvFor(code));
            Assert.IsFalse(ModRM.HasIndex(code));
            Assert.IsFalse(ModRM.IsEffectiveAddressDereferenced(code));
        }

        [Test]
        public void MovEsp16()
        {
            // mov DWORD PTR [esp],0x10
            code = new Byte[] {0xc7, 0x04, 0x24, 0x10, 0x00, 0x00, 0x00};
            Assert.IsTrue(ModRM.HasSIB(code));
        }

        [Test]
        public void TwoByteWithModRMNoIndex()
        {
            // movzx  ebx,BYTE PTR [eax]
            code = new Byte[] {0x0f, 0xb6, 0x18};
            Assert.AreEqual(RegisterName.EAX, ModRM.GetEvFor(code));
            Assert.AreEqual(RegisterName.EBX, ModRM.GetGvFor(code));
            Assert.IsFalse(ModRM.HasIndex(code));
            Assert.IsTrue(ModRM.IsEffectiveAddressDereferenced(code));
        }

        [Test]
        public void TwoByteWithModRMWithIndex()
        {
            // movzx  eax,BYTE PTR [ebp-5]
            code = new Byte[] {0x0f, 0xb6, 0x45, 0xfb};
            Assert.AreEqual(RegisterName.EBP, ModRM.GetEvFor(code));
            Assert.AreEqual(RegisterName.EAX, ModRM.GetGvFor(code));
            Assert.AreEqual(0xfb, ModRM.GetIndexFor(code));
            Assert.IsTrue(ModRM.IsEffectiveAddressDereferenced(code));
        }

        [Test]
        public void TwoByteWithModRMWithOnlyOffset()
        {
            // movzx  edx,BYTE PTR ds:0x80495e0
            code = new Byte[] {0x0f, 0xb6, 0x15, 0xe0, 0x95, 0x04, 0x08};
            Assert.AreEqual(RegisterName.EDX, ModRM.GetGvFor(code));
            Assert.IsFalse(ModRM.HasIndex(code));
            Assert.IsTrue(ModRM.IsEffectiveAddressDereferenced(code));
            Assert.IsTrue(ModRM.HasOffset(code));
        }
    }
}