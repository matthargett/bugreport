// Copyright (c) 2006-2008 Luis Miras, Doug Coker, Todd Nagengast,
// Anthony Lineberry, Dan Moniz, Bryan Siepert, Mike Seery, Cullen Bryan,
// Matt Hargett, Huy 
// Licensed under GPLv3
// See LICENSE.txt for details.

using NUnit.Framework;

namespace bugreport
{
    [TestFixture]
    public class DebuggerCommandTest
    {
        [Test]
        public void IsDisassemble()
        {
            var command = new DebuggerCommand("disasm");
            Assert.IsTrue(command.IsDisassemble);

            command = new DebuggerCommand("NOTdisasm");
            Assert.IsFalse(command.IsDisassemble);
        }

        [Test]
        public void IsQuit()
        {
            var command = new DebuggerCommand("q");
            Assert.IsTrue(command.IsQuit);

            command = new DebuggerCommand("NOTq");
            Assert.IsFalse(command.IsQuit);
        } 
        
        [Test]
        public void IsStackPrint()
        {
            var command = new DebuggerCommand("p");
            Assert.IsTrue(command.IsStackPrint);

            command = new DebuggerCommand("NOTp");
            Assert.IsFalse(command.IsStackPrint);
        }     
        
        [Test]
        public void IsEnter()
        {
            var command = new DebuggerCommand("");
            Assert.IsTrue(command.IsEnter);

            command = new DebuggerCommand("NOT");
            Assert.IsFalse(command.IsEnter);
        }
    }
}