using System;

namespace bugreport
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class CoverageExcludeAttribute : System.Attribute {}
}
