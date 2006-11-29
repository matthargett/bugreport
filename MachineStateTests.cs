
using NUnit.Framework;
using System;

namespace bugreport
{
	[TestFixture]
	public class MachineStateTests
	{
		[Ignore("TODO")]
		[Test]
		public void Equal()
		{
			MachineState a = new MachineState(new RegisterCollection());
			MachineState b = new MachineState(new RegisterCollection());
			Assert.AreEqual(a, b);
		}
	
		[Test]
		public void NotEqual()
		{

		}

	
	}
}
