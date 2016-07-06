// The following code has been borrowed from the https://github.com/ASP-NET-MVC/aspnetwebstack repository.
//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See https://github.com/ASP-NET-MVC/aspnetwebstack/blob/master/License.txt for license information.

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageDis.Owin.Utility
{
    public static class StreamHelperExtensions
    {
        public static async Task DrainAsync(this Stream stream, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[1024];
            cancellationToken.ThrowIfCancellationRequested();
            while (await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken) > 0)
            {
                // Not all streams support cancellation directly.
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}