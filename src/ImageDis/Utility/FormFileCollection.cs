// The following code has been borrowed from the https://github.com/ASP-NET-MVC/aspnetwebstack repository.
//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See https://github.com/ASP-NET-MVC/aspnetwebstack/blob/master/License.txt for license information.

using System.Collections.Generic;

namespace ImageDis.Utility
{
    public class FormFileCollection : List<FormFile>
    {
        public FormFile this[string name]
        {
            get { return GetFile(name); }
        }

        public FormFile GetFile(string name)
        {
            return Find(file => string.Equals(name, GetName(file.ContentDisposition)));
        }

        public IList<FormFile> GetFiles(string name)
        {
            return FindAll(file => string.Equals(name, GetName(file.ContentDisposition)));
        }

        private static string GetName(string contentDisposition)
        {
            // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
            ContentDispositionHeaderValue cd;
            ContentDispositionHeaderValue.TryParse(contentDisposition, out cd);
            return HeaderUtilities.RemoveQuotes(cd != null ? cd.Name : string.Empty);
        }
    }
}