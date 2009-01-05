// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;

namespace bugreport
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CoverageExcludeAttribute : Attribute {}
}
