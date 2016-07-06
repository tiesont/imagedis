// The following code has been borrowed from the https://github.com/ASP-NET-MVC/aspnetwebstack repository.
//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See https://github.com/ASP-NET-MVC/aspnetwebstack/blob/master/License.txt for license information.

using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDis.Owin.Utility
{
    public class FormFile
    {
        private Stream _baseStream;
        private long _baseStreamOffset;
        private long _length;

        public FormFile(Stream baseStream, long baseStreamOffset, long length)
        {
            _baseStream = baseStream;
            _baseStreamOffset = baseStreamOffset;
            _length = length;
        }

        public string ContentDisposition
        {
            get { return Headers["Content-Disposition"]; }
            set { Headers["Content-Disposition"] = value; }
        }

        public string ContentType
        {
            get { return Headers["Content-Type"]; }
            set { Headers["Content-Type"] = value; }
        }

        public HeaderDictionary Headers { get; set; }

        public long Length
        {
            get { return _length; }
        }

        public Stream OpenReadStream()
        {
            return new ReferenceReadStream(_baseStream, _baseStreamOffset, _length);
        }
    }
}
