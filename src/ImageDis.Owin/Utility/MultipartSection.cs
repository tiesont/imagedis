// The following code has been borrowed from the https://github.com/ASP-NET-MVC/aspnetwebstack repository.
//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See https://github.com/ASP-NET-MVC/aspnetwebstack/blob/master/License.txt for license information.

using System.Collections.Generic;
using System.IO;

namespace ImageDis.Owin.Utility
{
    public class MultipartSection
    {
        public string ContentType
        {
            get
            {
                string[] values;
                if (Headers.TryGetValue("Content-Type", out values))
                {
                    return string.Join(", ", values);
                }
                return null;
            }
        }

        public string ContentDisposition
        {
            get
            {
                string[] values;
                if (Headers.TryGetValue("Content-Disposition", out values))
                {
                    return string.Join(", ", values);
                }
                return null;
            }
        }

        public IDictionary<string, string[]> Headers { get; set; }

        public Stream Body { get; set; }

        /// <summary>
        /// The position where the body starts in the total multipart body.
        /// This may not be available if the total multipart body is not seekable.
        /// </summary>
        public long? BaseStreamOffset { get; set; }
    }
}