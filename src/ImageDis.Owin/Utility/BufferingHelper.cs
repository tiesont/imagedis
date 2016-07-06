﻿// The following code has been borrowed from the https://github.com/ASP-NET-MVC/aspnetwebstack repository.
//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See https://github.com/ASP-NET-MVC/aspnetwebstack/blob/master/License.txt for license information.

using System;
using System.IO;

namespace ImageDis.Owin.Utility
{
    public static class BufferingHelper
    {
        internal const int DefaultBufferThreshold = 1024 * 30;

        public static string TempDirectory
        {
            get
            {
                // Look for folders in the following order.
                var temp = Environment.GetEnvironmentVariable("ASPNET_TEMP") ??     // ASPNET_TEMP - User set temporary location.
                           Path.GetTempPath();                                      // Fall back.

                if (!Directory.Exists(temp))
                {
                    // TODO: ???
                    throw new DirectoryNotFoundException(temp);
                }

                return temp;
            }
        }

        public static Microsoft.Owin.IOwinRequest EnableRewind(this Microsoft.Owin.IOwinRequest request, int bufferThreshold = DefaultBufferThreshold)
        {
            var body = request.Body;
            if (!body.CanSeek)
            {
                // TODO: Register this buffer for disposal at the end of the request to ensure the temp file is deleted.
                //  Otherwise it won't get deleted until GC closes the stream.
                request.Body = new FileBufferingReadStream(body, bufferThreshold, TempDirectory);
            }
            return request;
        }
    }
}