// The following code has been borrowed from the https://github.com/ASP-NET-MVC/aspnetwebstack repository.
//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See https://github.com/ASP-NET-MVC/aspnetwebstack/blob/master/License.txt for license information.

using System.Collections.Generic;

namespace ImageDis.Utility
{
    /// <summary>
    /// Contains the parsed form values.
    /// </summary>
    public class FormCollection : ReadableStringCollection
    {
        public FormCollection(IDictionary<string, string[]> store)
            : this(store, new FormFileCollection())
        {
        }

        public FormCollection(IDictionary<string, string[]> store, FormFileCollection files)
            : base(store)
        {
            Files = files;
        }

        public FormFileCollection Files { get; private set; }
    }
}