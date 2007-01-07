// Copyright (c) 2006 Luis Miras, Doug Coker, Todd Nagengast, Anthony Lineberry, Dan Moniz, Bryan Siepert
// Licensed under GPLv3 draft 2
// See LICENSE.txt for details.

using System;

namespace bugreport
{	
	public enum RegisterName : int {EAX, ECX, EDX, EBX, ESP, EBP, ESI, EDI};
	
	public class RegisterCollection
	{
		private AbstractValue[] registers;
		
		public RegisterCollection()
		{
			registers = new AbstractValue[8];
			for (UInt32 i = 0; i < registers.Length; ++i) {
				registers[i] = new AbstractValue();
			}
		}
		
		public RegisterCollection(RegisterCollection _copyMe)
		{
			registers = new AbstractValue[8];
			for (UInt32 i = 0; i < registers.Length; ++i) {
				registers[i] = new AbstractValue(_copyMe.registers[i]);
			}
			
		}
		public AbstractValue this[RegisterName index]
		{
			get { return registers[(Int32)index]; }
			set { registers[(Int32)index] = value; }
		}
		
		public override string ToString()
		{
			String result = String.Empty;
			for(UInt32 i = 0; i < registers.Length; ++i)
			{
				AbstractValue value = registers[i];
				result += RegisterName.GetName(typeof(RegisterName), i) + "=" + value + "\t";
				if (value.ToString().Length < 8)
				{
					result += "\t";
				}

				if ((i + 1) % 4 == 0)
				{
					result += Environment.NewLine;
				}
			}
			return result;
		}
	}
}
