using System;
using System.Text;

namespace bugreport
{
    public class InvalidOpcodeException : Exception
    {
        public InvalidOpcodeException(params Byte[] code)
            : base(FormatOpcodes(code))
        {
        }

        public static String FormatOpcodes(params Byte[] code)
        {
            var message = new StringBuilder("Invalid opcode: ");

            foreach (Byte opcode in code)
            {
                message.Append(String.Format(" 0x{0:x2}", opcode));
            }

            return message.ToString();
        }
    }
}