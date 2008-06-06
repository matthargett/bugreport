// Copyright (c) 2006-2008 Luis Miras, Doug Coker, Todd Nagengast,
// Anthony Lineberry, Dan Moniz, Bryan Siepert, Mike Seery, Cullen Bryan
// Licensed under GPLv3 draft 3
// See LICENSE.txt for details.

using System;

namespace bugreport
{
[AttributeUsage(AttributeTargets.Method)]
public sealed class CoverageExcludeAttribute : Attribute {}
}
