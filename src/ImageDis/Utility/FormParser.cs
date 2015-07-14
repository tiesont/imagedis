﻿// The following code has been borrowed from the https://github.com/ASP-NET-MVC/aspnetwebstack repository.
//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See https://github.com/ASP-NET-MVC/aspnetwebstack/blob/master/License.txt for license information.

using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageDis.Utility
{
    public class FormParser
    {
        private readonly IOwinRequest _request;

        public FormParser(IOwinRequest request)
        {
            _request = request;
        }

        private MediaTypeHeaderValue ContentType
        {
            get
            {
                MediaTypeHeaderValue mt;
                MediaTypeHeaderValue.TryParse(_request.ContentType, out mt);
                return mt;
            }
        }

        public bool HasFormContentType
        {
            get
            {
                // Set directly
                if (Form != null)
                {
                    return true;
                }

                var contentType = ContentType;
                return HasApplicationFormContentType(contentType) || HasMultipartFormContentType(contentType);
            }
        }

        public FormCollection Form { get; set; }

        public FormCollection ReadForm()
        {
            if (Form != null)
            {
                return Form;
            }

            if (!HasFormContentType)
            {
                throw new InvalidOperationException("Incorrect Content-Type: " + _request.ContentType);
            }

            // TODO: How do we prevent thread exhaustion?
            return ReadFormAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        public async Task<FormCollection> ReadFormAsync(CancellationToken cancellationToken)
        {
            if (Form != null)
            {
                return Form;
            }

            if (!HasFormContentType)
            {
                throw new InvalidOperationException("Incorrect Content-Type: " + _request.ContentType);
            }

            cancellationToken.ThrowIfCancellationRequested();

            _request.EnableRewind();

            IDictionary<string, string[]> formFields = null;
            var files = new FormFileCollection();

            // Some of these code paths use StreamReader which does not support cancellation tokens.
            //using (cancellationToken.Register(_request.HttpContext.Abort))
            //{
                var contentType = ContentType;
                // Check the content-type
                if (HasApplicationFormContentType(contentType))
                {
                    var encoding = FilterEncoding(contentType.Encoding);
                    formFields = await FormReader.ReadFormAsync(_request.Body, encoding, cancellationToken);
                }
                else if (HasMultipartFormContentType(contentType))
                {
                    var formAccumulator = new KeyValueAccumulator<string, string>(StringComparer.OrdinalIgnoreCase);

                    var boundary = GetBoundary(contentType);
                    var multipartReader = new MultipartReader(boundary, _request.Body);
                    var section = await multipartReader.ReadNextSectionAsync(cancellationToken);
                    while (section != null)
                    {
                        var headers = new HeaderDictionary(section.Headers);
                        ContentDispositionHeaderValue contentDisposition;
                        ContentDispositionHeaderValue.TryParse(headers.Get(HeaderNames.ContentDisposition), out contentDisposition);
                        if (HasFileContentDisposition(contentDisposition))
                        {
                            // Find the end
                            await section.Body.DrainAsync(cancellationToken);

                            var file = new FormFile(_request.Body, section.BaseStreamOffset.Value, section.Body.Length)
                            {
                                Headers = headers,
                            };
                            files.Add(file);
                        }
                        else if (HasFormDataContentDisposition(contentDisposition))
                        {
                            // Content-Disposition: form-data; name="key"
                            //
                            // value

                            var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                            MediaTypeHeaderValue mediaType;
                            MediaTypeHeaderValue.TryParse(headers.Get(HeaderNames.ContentType), out mediaType);
                            var encoding = FilterEncoding(mediaType != null ? mediaType.Encoding : null);
                            using (var reader = new StreamReader(section.Body, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
                            {
                                var value = await reader.ReadToEndAsync();
                                formAccumulator.Append(key, value);
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false, "Unrecognized content-disposition for this section: " + headers.Get(HeaderNames.ContentDisposition));
                        }

                        section = await multipartReader.ReadNextSectionAsync(cancellationToken);
                    }

                    formFields = formAccumulator.GetResults();
                }
            //}

            Form = new FormCollection(formFields, files);
            return Form;
        }

        private Encoding FilterEncoding(Encoding encoding)
        {
            // UTF-7 is insecure and should not be honored. UTF-8 will succeed for most cases.
            if (encoding == null || Encoding.UTF7.Equals(encoding))
            {
                return Encoding.UTF8;
            }
            return encoding;
        }

        private bool HasApplicationFormContentType(MediaTypeHeaderValue contentType)
        {
            // Content-Type: application/x-www-form-urlencoded; charset=utf-8
            return contentType != null && contentType.MediaType.Equals("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase);
        }

        private bool HasMultipartFormContentType(MediaTypeHeaderValue contentType)
        {
            // Content-Type: multipart/form-data; boundary=----WebKitFormBoundarymx2fSWqWSd0OxQqq
            return contentType != null && contentType.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase);
        }

        private bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="key";
            return contentDisposition != null && contentDisposition.DispositionType.Equals("form-data")
                && string.IsNullOrEmpty(contentDisposition.FileName) && string.IsNullOrEmpty(contentDisposition.FileNameStar);
        }

        private bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
            return contentDisposition != null && contentDisposition.DispositionType.Equals("form-data")
                && (!string.IsNullOrEmpty(contentDisposition.FileName) || !string.IsNullOrEmpty(contentDisposition.FileNameStar));
        }

        // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
        // TODO: Limit the length of boundary we accept. The spec says ~70 chars.
        private static string GetBoundary(MediaTypeHeaderValue contentType)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary);
            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidOperationException("Missing content-type boundary.");
            }
            return boundary;
        }
    }
}