﻿// The following code has been borrowed from the https://github.com/ASP-NET-MVC/aspnetwebstack repository.
//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See https://github.com/ASP-NET-MVC/aspnetwebstack/blob/master/License.txt for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ImageDis.Utility
{
    public class MediaTypeHeaderValue
    {
        private const string CharsetString = "charset";
        private const string BoundaryString = "boundary";

        private static readonly HttpHeaderParser<MediaTypeHeaderValue> SingleValueParser
            = new GenericHeaderParser<MediaTypeHeaderValue>(false, GetMediaTypeLength);
        private static readonly HttpHeaderParser<MediaTypeHeaderValue> MultipleValueParser
            = new GenericHeaderParser<MediaTypeHeaderValue>(true, GetMediaTypeLength);

        // Use a collection instead of a dictionary since we may have multiple parameters with the same name.
        private ObjectCollection<NameValueHeaderValue> _parameters;
        private string _mediaType;
        private bool _isReadOnly;

        private MediaTypeHeaderValue()
        {
            // Used by the parser to create a new instance of this type.
        }

        public MediaTypeHeaderValue(string mediaType)
        {
            CheckMediaTypeFormat(mediaType, "mediaType");
            _mediaType = mediaType;
        }

        public MediaTypeHeaderValue(string mediaType, double quality)
            : this(mediaType)
        {
            Quality = quality;
        }

        public string Charset
        {
            get
            {
                var val = NameValueHeaderValue.Find(_parameters, CharsetString);
                return val != null ? val.Value : null;
            }
            set
            {
                HeaderUtilities.ThrowIfReadOnly(IsReadOnly);
                // We don't prevent a user from setting whitespace-only charsets. Like we can't prevent a user from
                // setting a non-existing charset.
                var charsetParameter = NameValueHeaderValue.Find(_parameters, CharsetString);
                if (string.IsNullOrEmpty(value))
                {
                    // Remove charset parameter
                    if (charsetParameter != null)
                    {
                        _parameters.Remove(charsetParameter);
                    }
                }
                else
                {
                    if (charsetParameter != null)
                    {
                        charsetParameter.Value = value;
                    }
                    else
                    {
                        Parameters.Add(new NameValueHeaderValue(CharsetString, value));
                    }
                }
            }
        }

        public Encoding Encoding
        {
            get
            {
                var charset = Charset;
                if (!string.IsNullOrWhiteSpace(charset))
                {
                    try
                    {
                        return Encoding.GetEncoding(charset);
                    }
                    catch (ArgumentException)
                    {
                        // Invalid or not supported
                    }
                }
                return null;
            }
            set
            {
                HeaderUtilities.ThrowIfReadOnly(IsReadOnly);
                if (value == null)
                {
                    Charset = null;
                }
                else
                {
                    Charset = value.WebName;
                }
            }
        }

        public string Boundary
        {
            get
            {
                var val = NameValueHeaderValue.Find(_parameters, BoundaryString);
                return val != null ? val.Value : null;
            }
            set
            {
                HeaderUtilities.ThrowIfReadOnly(IsReadOnly);
                var boundaryParameter = NameValueHeaderValue.Find(_parameters, BoundaryString);
                if (string.IsNullOrEmpty(value))
                {
                    // Remove charset parameter
                    if (boundaryParameter != null)
                    {
                        _parameters.Remove(boundaryParameter);
                    }
                }
                else
                {
                    if (boundaryParameter != null)
                    {
                        boundaryParameter.Value = value;
                    }
                    else
                    {
                        Parameters.Add(new NameValueHeaderValue(BoundaryString, value));
                    }
                }
            }
        }

        public ICollection<NameValueHeaderValue> Parameters
        {
            get
            {
                if (_parameters == null)
                {
                    if (IsReadOnly)
                    {
                        _parameters = ObjectCollection<NameValueHeaderValue>.EmptyReadOnlyCollection;
                    }
                    else
                    {
                        _parameters = new ObjectCollection<NameValueHeaderValue>();
                    }
                }
                return _parameters;
            }
        }

        public double? Quality
        {
            get { return HeaderUtilities.GetQuality(Parameters); }
            set { HeaderUtilities.SetQuality(Parameters, value); }
        }

        public string MediaType
        {
            get { return _mediaType; }
            set
            {
                HeaderUtilities.ThrowIfReadOnly(IsReadOnly);
                CheckMediaTypeFormat(value, "value");
                _mediaType = value;
            }
        }

        public string Type
        {
            get
            {
                return _mediaType.Substring(0, _mediaType.IndexOf('/'));
            }
        }

        public string SubType
        {
            get
            {
                return _mediaType.Substring(_mediaType.IndexOf('/') + 1);
            }
        }

        /// <summary>
        /// MediaType = "*/*"
        /// </summary>
        public bool MatchesAllTypes
        {
            get
            {
                return MediaType.Equals("*/*", StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// SubType = "*"
        /// </summary>
        public bool MatchesAllSubTypes
        {
            get
            {
                return SubType.Equals("*", StringComparison.Ordinal);
            }
        }

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        public bool IsSubsetOf(MediaTypeHeaderValue otherMediaType)
        {
            if (otherMediaType == null)
            {
                return false;
            }

            if (!Type.Equals(otherMediaType.Type, StringComparison.OrdinalIgnoreCase))
            {
                if (!otherMediaType.MatchesAllTypes)
                {
                    return false;
                }
            }
            else if (!SubType.Equals(otherMediaType.SubType, StringComparison.OrdinalIgnoreCase))
            {
                if (!otherMediaType.MatchesAllSubTypes)
                {
                    return false;
                }
            }

            if (Parameters != null)
            {
                if (Parameters.Count != 0 && (otherMediaType.Parameters == null || otherMediaType.Parameters.Count == 0))
                {
                    return false;
                }

                // Make sure all parameters listed locally are listed in the other one. The other one may have additional parameters.
                foreach (var param in _parameters)
                {
                    var otherParam = NameValueHeaderValue.Find(otherMediaType._parameters, param.Name);
                    if (otherParam == null)
                    {
                        return false;
                    }
                    if (!string.Equals(param.Value, otherParam.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Performs a deep copy of this object and all of it's NameValueHeaderValue sub components,
        /// while avoiding the cost of revalidating the components.
        /// </summary>
        /// <returns>A deep copy.</returns>
        public MediaTypeHeaderValue Copy()
        {
            var other = new MediaTypeHeaderValue();
            other._mediaType = _mediaType;

            if (_parameters != null)
            {
                other._parameters = new ObjectCollection<NameValueHeaderValue>(
                    _parameters.Select(item => item.Copy()));
            }
            return other;
        }

        /// <summary>
        /// Performs a deep copy of this object and all of it's NameValueHeaderValue sub components,
        /// while avoiding the cost of revalidating the components. This copy is read-only.
        /// </summary>
        /// <returns>A deep, read-only, copy.</returns>
        public MediaTypeHeaderValue CopyAsReadOnly()
        {
            if (IsReadOnly)
            {
                return this;
            }

            var other = new MediaTypeHeaderValue();
            other._mediaType = _mediaType;
            if (_parameters != null)
            {
                other._parameters = new ObjectCollection<NameValueHeaderValue>(
                    _parameters.Select(item => item.CopyAsReadOnly()), isReadOnly: true);
            }
            other._isReadOnly = true;
            return other;
        }

        public override string ToString()
        {
            return _mediaType + NameValueHeaderValue.ToString(_parameters, ';', true);
        }

        public override bool Equals(object obj)
        {
            var other = obj as MediaTypeHeaderValue;

            if (other == null)
            {
                return false;
            }

            return (string.Compare(_mediaType, other._mediaType, StringComparison.OrdinalIgnoreCase) == 0) &&
                HeaderUtilities.AreEqualCollections(_parameters, other._parameters);
        }

        public override int GetHashCode()
        {
            // The media-type string is case-insensitive.
            return StringComparer.OrdinalIgnoreCase.GetHashCode(_mediaType) ^ NameValueHeaderValue.GetHashCode(_parameters);
        }

        public static MediaTypeHeaderValue Parse(string input)
        {
            var index = 0;
            return SingleValueParser.ParseValue(input, ref index);
        }

        public static bool TryParse(string input, out MediaTypeHeaderValue parsedValue)
        {
            var index = 0;
            return SingleValueParser.TryParseValue(input, ref index, out parsedValue);
        }

        public static IList<MediaTypeHeaderValue> ParseList(IList<string> inputs)
        {
            return MultipleValueParser.ParseValues(inputs);
        }

        public static bool TryParseList(IList<string> inputs, out IList<MediaTypeHeaderValue> parsedValues)
        {
            return MultipleValueParser.TryParseValues(inputs, out parsedValues);
        }

        private static int GetMediaTypeLength(string input, int startIndex, out MediaTypeHeaderValue parsedValue)
        {
            Contract.Requires(startIndex >= 0);

            parsedValue = null;

            if (string.IsNullOrEmpty(input) || (startIndex >= input.Length))
            {
                return 0;
            }

            // Caller must remove leading whitespaces. If not, we'll return 0.
            string mediaType = null;
            var mediaTypeLength = MediaTypeHeaderValue.GetMediaTypeExpressionLength(input, startIndex, out mediaType);

            if (mediaTypeLength == 0)
            {
                return 0;
            }

            var current = startIndex + mediaTypeLength;
            current = current + HttpRuleParser.GetWhitespaceLength(input, current);
            MediaTypeHeaderValue mediaTypeHeader = null;

            // If we're not done and we have a parameter delimiter, then we have a list of parameters.
            if ((current < input.Length) && (input[current] == ';'))
            {
                mediaTypeHeader = new MediaTypeHeaderValue();
                mediaTypeHeader._mediaType = mediaType;

                current++; // skip delimiter.
                var parameterLength = NameValueHeaderValue.GetNameValueListLength(input, current, ';',
                    mediaTypeHeader.Parameters);

                parsedValue = mediaTypeHeader;
                return current + parameterLength - startIndex;
            }

            // We have a media type without parameters.
            mediaTypeHeader = new MediaTypeHeaderValue();
            mediaTypeHeader._mediaType = mediaType;
            parsedValue = mediaTypeHeader;
            return current - startIndex;
        }

        private static int GetMediaTypeExpressionLength(string input, int startIndex, out string mediaType)
        {
            Contract.Requires((input != null) && (input.Length > 0) && (startIndex < input.Length));

            // This method just parses the "type/subtype" string, it does not parse parameters.
            mediaType = null;

            // Parse the type, i.e. <type> in media type string "<type>/<subtype>; param1=value1; param2=value2"
            var typeLength = HttpRuleParser.GetTokenLength(input, startIndex);

            if (typeLength == 0)
            {
                return 0;
            }

            var current = startIndex + typeLength;
            current = current + HttpRuleParser.GetWhitespaceLength(input, current);

            // Parse the separator between type and subtype
            if ((current >= input.Length) || (input[current] != '/'))
            {
                return 0;
            }
            current++; // skip delimiter.
            current = current + HttpRuleParser.GetWhitespaceLength(input, current);

            // Parse the subtype, i.e. <subtype> in media type string "<type>/<subtype>; param1=value1; param2=value2"
            var subtypeLength = HttpRuleParser.GetTokenLength(input, current);

            if (subtypeLength == 0)
            {
                return 0;
            }

            // If there are no whitespaces between <type> and <subtype> in <type>/<subtype> get the media type using
            // one Substring call. Otherwise get substrings for <type> and <subtype> and combine them.
            var mediatTypeLength = current + subtypeLength - startIndex;
            if (typeLength + subtypeLength + 1 == mediatTypeLength)
            {
                mediaType = input.Substring(startIndex, mediatTypeLength);
            }
            else
            {
                mediaType = input.Substring(startIndex, typeLength) + "/" + input.Substring(current, subtypeLength);
            }

            return mediatTypeLength;
        }

        private static void CheckMediaTypeFormat(string mediaType, string parameterName)
        {
            if (string.IsNullOrEmpty(mediaType))
            {
                throw new ArgumentException("An empty string is not allowed.", parameterName);
            }

            // When adding values using strongly typed objects, no leading/trailing LWS (whitespaces) are allowed.
            // Also no LWS between type and subtype are allowed.
            string tempMediaType;
            var mediaTypeLength = GetMediaTypeExpressionLength(mediaType, 0, out tempMediaType);
            if ((mediaTypeLength == 0) || (tempMediaType.Length != mediaType.Length))
            {
                throw new FormatException(string.Format(CultureInfo.InvariantCulture, "Invalid media type '{0}'.", mediaType));
            }
        }
    }
}