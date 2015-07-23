﻿using System;
using System.Linq;

namespace ImageDis
{
    public class ImageDisOptions
    {
        public ImageDisOptions(
            IStorageProvider storageProvider,
            IImageTransformProvider imageTransformProvider, 
            string path = null,
            string[] allowedContentTypes = null,
            IKeyGenerator keyGenerator = null)
        {
            if (storageProvider == null)
                throw new ArgumentNullException("storageProvider");

            StorageProvider = storageProvider;

            if (imageTransformProvider == null)
                throw new ArgumentNullException("imageTransformProvider");

            ImageTransformProvider = imageTransformProvider;

            Path = string.IsNullOrWhiteSpace(path) ? "/imagedis" : path;

            AllowedContentTypes = allowedContentTypes == null || !allowedContentTypes.Any() 
                ? new[] { "image/jpeg", "image/png" } 
                : allowedContentTypes;

            KeyGenerator = keyGenerator ?? new RandomKeyGenerator();
        }

        public string Path { get; set; }

        public string[] AllowedContentTypes { get; set; }

        public IKeyGenerator KeyGenerator { get; set; }

        public IImageTransformProvider ImageTransformProvider { get; set; }

        public IStorageProvider StorageProvider { get; set; }
    }
}
