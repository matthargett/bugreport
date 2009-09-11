// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

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
            get { return input.Length == 0; }
        }
    }
}