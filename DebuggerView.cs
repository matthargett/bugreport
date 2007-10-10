using System;

namespace bugreport
{

    class DebuggerView
    {
        private readonly Boolean interactive;
        private MachineState currentState;

        public DebuggerView(Boolean interactive)
        {
            this.interactive = interactive;    
        }
        public void printInfo(object sender, EmulationEventArgs e)
        {
            Byte[] code = new Byte[e.Code.Count];
            e.Code.CopyTo(code, 0);
            currentState = e.MachineState;
            String address = String.Format("{0:x8}", currentState.InstructionPointer);
            Console.Write(address + ":");
            Console.Write("\t");

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

            String operands = OpcodeFormatter.GetOperands(code, currentState.InstructionPointer);
            Console.Write(operands);
            if (operands.Length < 8)
                Console.Write("\t");

            String encoding = OpcodeFormatter.GetEncoding(code);
            Console.Write("\t");
            Console.WriteLine(encoding);

            Console.WriteLine(currentState.Registers);
            Console.WriteLine();

            promptIfNecessary();
        }

        protected void promptIfNecessary()
        {
            Boolean enterPressed = false;
            if (interactive)
            {
                while (!enterPressed)
                {
                    string input = getInput();
                    if (isEnter(input))
                    {
                        enterPressed = true;
                        continue;
                    }
                    else if (isStackPrint(input))
                    {
                        printStack();
                        continue;
                    }
                    else if (isQuit(input))
                    {
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("invalid command");
                    }
                }
            }
        }

        private string getInput()
        {
            Console.Write("0x{0:x8} > ", currentState.InstructionPointer);
            return Console.ReadLine();
        }

        private static Boolean isQuit(string input)
        {
            if (input == "q")
            {
                return true;
            }

            return false;
        }

        private void printStack()
        {
            AbstractValue esp = currentState.Registers[RegisterName.ESP];
            
            // print esp-2, esp-1, esp

            Console.WriteLine("Stack dump");
            Console.WriteLine("esp-2\t\t esp-1\t\t esp");
            Console.WriteLine("{0}\t\t {1}\t\t {2}", esp.PointsTo[-2], esp.PointsTo[-1], esp.PointsTo[0]);

        }

        private static Boolean isStackPrint(string input)
        {
            if (input == "p")
            {
                return true;
            }
            return false;
        }

        private static Boolean isEnter(string input)
        {
            if (input == "")
            {
                return true;
            }
            return false;
        }
    }
}
