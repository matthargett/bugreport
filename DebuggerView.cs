// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;

namespace bugreport
{
    internal sealed class DebuggerView
    {
        private readonly Boolean interactive;
        private MachineState state;

        public DebuggerView(Boolean interactive)
        {
            this.interactive = interactive;
        }

        public void PrintInfo(object sender, EmulationEventArgs emulationEvent)
        {
            var address = GetEffectiveAddressFor(emulationEvent);
            Console.Write(address + ":");
            Console.Write("\t");

            var code = GetCodeFor(emulationEvent);
            PrintOpcodeInfoFor(code);

            Console.WriteLine(state.Registers);
            Console.WriteLine();

            HandleInputIfNecessary();
        }

        private void HandleInputIfNecessary()
        {
            if (!interactive) return;

            // TODO: cover this with a system-level test
            var enterPressed = false;

            while (!enterPressed)
            {
                var input = GetInput();
                var command = new DebuggerCommand(input);
                if (command.IsEnter)
                {
                    enterPressed = true;
                    continue;
                }

                if (command.IsStackPrint)
                {
                    PrintStackFor(state);
                    continue;
                }

                if (command.IsDisassemble)
                {
                    var hex = input.Substring("disasm".Length + 1);
                    var code = DumpFileParser.GetByteArrayFor(hex);
                    PrintOpcodeInfoFor(code);
                    continue;
                }

                if (command.IsQuit)
                {
                    Environment.Exit(0);
                }

                Console.WriteLine("invalid command");
            }
        }

        private void PrintOpcodeInfoFor(byte[] code)
        {
            foreach (var codeByte in code)
            {
                Console.Write(String.Format("{0:x2}", codeByte) + " ");
            }

            // magic numbers that happen to look good :)
            var numberOfTabs = 3 - (code.Length / 3);
            for (var i = 0; i < numberOfTabs; i++)
            {
                Console.Write("\t");
            }

            Console.Write(OpcodeFormatter.GetInstructionName(code));
            Console.Write("\t");

            var operands = OpcodeFormatter.GetOperands(code, state.InstructionPointer);
            Console.Write(operands);
            if (operands.Length < 8)
            {
                Console.Write("\t");
            }

            var encoding = OpcodeFormatter.GetEncodingFor(code);
            Console.Write("\t");
            Console.WriteLine(encoding);
        }

        private static Byte[] GetCodeFor(EmulationEventArgs e)
        {
            var code = new Byte[e.Code.Count];
            e.Code.CopyTo(code, 0);
            return code;
        }

        private string GetEffectiveAddressFor(EmulationEventArgs e)
        {
            state = e.MachineState;
            var address = String.Format("{0:x8}", state.InstructionPointer);
            return address;
        }

        private string GetInput()
        {
            Console.Write("0x{0:x8} > ", state.InstructionPointer);
            return Console.ReadLine();
        }

        private static void PrintStackFor(MachineState state)
        {
            var esp = state.Registers[RegisterName.ESP];

            Console.WriteLine("Stack dump");
            Console.WriteLine("esp-8\t\t esp-4\t\t esp");
            Console.WriteLine("{0}\t\t {1}\t\t {2}", esp.PointsTo[-2], esp.PointsTo[-1], esp.PointsTo[0]);
        }
    }
}