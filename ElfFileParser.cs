// This file is part of bugreport.
// Copyright (c) 2006-2009 The bugreport Developers.
// See AUTHORS.txt for details.
// Licensed under the GNU General Public License, Version 3 (GPLv3).
// See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace bugreport
{
    public sealed class ElfFileParser : IParsable, IDisposable
    {
        private readonly Stream stream;
        private readonly Byte[] textData;

        public ElfFileParser(Stream stream)
        {
            this.stream = stream;
            textData = new Byte[stream.Length];
        }

        public UInt32 BaseAddress
        {
            get { return 0x080482e0; }
        }

        public UInt32 EntryPointAddress
        {
            get { return 0x080482e0; }
        }

        public ReadOnlyCollection<ReportItem> ExpectedReportItems
        {
            get { return new ReadOnlyCollection<ReportItem>(new List<ReportItem>()); }
        }

        public Byte[] GetBytes()
        {
            stream.Seek(0x2e0, SeekOrigin.Begin);
            stream.Read(textData, 0, (Int32)(stream.Length - stream.Position));
            return textData;
        }

        public void Dispose()
        {
            if (null != stream)
            {
                stream.Dispose();
            }
        }
    }
}