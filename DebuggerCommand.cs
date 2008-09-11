namespace bugreport
{
    public class DebuggerCommand
    {
        private readonly string input;

        public DebuggerCommand(string input)
        {
            this.input = input;
        }

        public bool IsDisassemble
        {
            get { return input.StartsWith("disasm"); }
        }

        public bool IsQuit
        {
            get { return input == "q"; }
        }

        public bool IsStackPrint
        {
            get { return input == "p"; }
        }

        public bool IsEnter
        {
            get { return input == ""; }
        }
    }
}