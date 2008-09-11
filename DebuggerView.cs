using System;

namespace bugreport
{
    class DebuggerView
    {
        private readonly Boolean interactive;
        private MachineState state;

        public DebuggerView(Boolean interactive)
        {
            this.interactive = interactive;
        }

        public void printInfo(object sender, EmulationEventArgs e)
        {
            String address = getEffectiveAddressFor(e);
            Console.Write(address + ":");
            Console.Write("\t");

            Byte[] code = getCodeFor(e);
            printOpcodeInfo(code);

            Console.WriteLine(state.Registers);
            Console.WriteLine();

            handleInputIfNecessary();
        }

        private void printOpcodeInfo(byte[] code)
        {
            foreach (Byte codeByte in code)
            {
                Console.Write(String.Format("{0:x2}", codeByte) + " ");
            }

            // magic numbers that happen to look good :)
            Int32 numberOfTabs = 3 - code.Length / 3;
            for (Int32 i = 0; i < numberOfTabs; i++)
                Console.Write("\t");

            Console.Write(OpcodeFormatter.GetInstructionName(code));
            Console.Write("\t");

            String operands = OpcodeFormatter.GetOperands(code, state.InstructionPointer);
            Console.Write(operands);
            if (operands.Length < 8)
                Console.Write("\t");

            String encoding = OpcodeFormatter.GetEncoding(code);
            Console.Write("\t");
            Console.WriteLine(encoding);
        }

        Byte[] getCodeFor(EmulationEventArgs e)
        {
            Byte[] code = new Byte[e.Code.Count];
            e.Code.CopyTo(code, 0);
            return code;
        }


        string getEffectiveAddressFor(EmulationEventArgs e)
        {
            state = e.MachineState;
            String address = String.Format("{0:x8}", state.InstructionPointer);
            return address;
        }


        protected void handleInputIfNecessary()
        {
            //TODO: cover this with a system-level test
            Boolean enterPressed = false;
            if (interactive)
            {
                while (!enterPressed)
                {
                    var input = getInput();
                    var command = new DebuggerCommand(input);
                    if (command.IsEnter)
                    {
                        enterPressed = true;
                        continue;
                    }
                    if (command.IsStackPrint)
                    {
                        printStackFor(state);
                        continue;
                    }
                    if (command.IsDisassemble)
                    {
                        var hex = input.Substring("disasm".Length + 1);
                        var code = DumpFileParser.getByteArrayFromHexString(hex);
                        printOpcodeInfo(code);
                        continue;
                    }
                    if (command.IsQuit)
                    {
                        Environment.Exit(0);
                    }

                    Console.WriteLine("invalid command");
                }
            }
        }

        private string getInput()
        {
            Console.Write("0x{0:x8} > ", state.InstructionPointer);
            return Console.ReadLine();
        }

        private void printStackFor(MachineState state)
        {
            AbstractValue esp = state.Registers[RegisterName.ESP];
            
            Console.WriteLine("Stack dump");
            Console.WriteLine("esp-8\t\t esp-4\t\t esp");
            Console.WriteLine("{0}\t\t {1}\t\t {2}", esp.PointsTo[-2], esp.PointsTo[-1], esp.PointsTo[0]);
        }
    }
}
